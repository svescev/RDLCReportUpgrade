using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Globalization;

namespace $safeprojectname$
{
    class ReportUpgradeFunctions
    {
        public void RefreshDataset(Part report, ref int NewID, ref List<SectionControl> sectionControls, ref List<Section> sectionsList, ref List<DataItem> dataItemList)
        {
            Part datasetPart = report.FindPartByName("DATAITEMS");
            datasetPart.ChangeValuesInPart("DATAITEMS", "DATASET");
            report.RefreshTransformedValues();
            string textConstantsToAdd = "";
            sectionControls = new List<SectionControl>();
            sectionsList = new List<Section>();
            int newSectionId = 0; 
            DataItemHierarhy dataHierar = new DataItemHierarhy(); //hierarcy
            DataItemHierarhy startHierar = dataHierar;
            dataItemList = new List<DataItem>();
            foreach (Part dataitem in datasetPart.children)
            {
                DataItem dataItemC = new DataItem(dataitem);
                dataItemC.dataItemId = NewID;
                dataItemC.indexOfDataitem = datasetPart.children.IndexOf(dataitem);
                dataItemC.ParseDataItemProperties();
                NewID++;
                dataItemList.Add(dataItemC);

                FindSectionAndSectionControls(dataItemC, sectionControls,sectionsList, ref newSectionId);

                //add dataitem hierarchy
                dataHierar = dataHierar.AddDataItem(dataItemC.indexOfDataitem, Convert.ToInt32(dataItemC.dataItemIndent));
            }

            // order sections by processing order
            int sectionOrderId = 1; //starting order no.
            startHierar.TraverseHierarchyAndOrderSections(ref sectionsList, ref sectionOrderId);
            sectionsList = sectionsList.OrderBy(x => x.processingOrder).ToList();

            GenerateNewTextConstants(sectionControls, ref NewID, ref textConstantsToAdd);
            HandleLabels(dataItemList, sectionControls);
            RemoveDuplicateControls(sectionControls);
            HandleSameControlNames(dataItemList, sectionControls);
            AddNewDataToDataset(datasetPart, sectionControls, dataItemList);
            AddTextConstantsToGlobals(report, textConstantsToAdd);
        }

        /// <summary>
        /// Find all sections for a DataItem
        /// </summary>
        /// <param name="dataitem"></param>
        /// <param name="sectionControls"></param>
        /// <param name="indexOfDataitem"></param>
        /// <param name="indentforstruct"></param>
        public void FindSectionControls(Part dataitem, List<SectionControl> sectionControls, int indexOfDataitem, string indentforstruct)
        {
            Part sections = dataitem.FindChildByName("SECTIONS");
            foreach (Part section in sections.children)
            {
                Part controls = section.FindChildByName("CONTROLS");
                foreach (Part control in controls.children)
                {
                    SectionControl secCtrl = new SectionControl(control);
                    if (indentforstruct.Length > 0)
                        secCtrl.indentOfControl = Convert.ToInt32(indentforstruct) + 1;
                    secCtrl.ParseSectionControlProperties();
                    secCtrl.dataitemIndex = indexOfDataitem;
                    sectionControls.Add(secCtrl);
                }
            }
        }

        /// <summary>
        /// Find all sections for a DataItem
        /// added logic for finding section & section properties
        /// </summary>
        /// <param name="dataitem"></param>
        /// <param name="sectionControls"></param>
        /// <param name="sectionsList"></param>
        /// <param name="indexOfDataitem"></param>
        /// <param name="indentforstruct"></param>
        /// <param name="newSectionId"></param>
        public void FindSectionAndSectionControls(DataItem dataitem, List<SectionControl> sectionControls, List<Section> sectionsList, ref int newSectionId)
        {
            Part sections = dataitem.FindChildByName("SECTIONS");
            foreach (Part section in sections.children)
            {
                //create new section
                Part partSecProp = section.FindChildByName("PROPERTIES");
                Section sec = new Section(partSecProp);
                sec.ParseSectionProperties();
                sec.dataitemIndex = dataitem.indexOfDataitem;
                newSectionId ++;
                sec.sectionId = newSectionId;
                sectionsList.Add(sec);
                
                //find section controls
                Part controls = section.FindChildByName("CONTROLS");
                foreach (Part control in controls.children)
                {
                    SectionControl secCtrl = new SectionControl(control);
                    if (!(dataitem.dataItemIndent == 0))
                        secCtrl.indentOfControl = dataitem.dataItemIndent + 1;
                    secCtrl.ParseSectionControlProperties();
                    secCtrl.dataitemIndex = dataitem.indexOfDataitem;
                    secCtrl.parentSectionId = sec.sectionId;  //link to section
                    sectionControls.Add(secCtrl);
                }
            }
        }

        /// <summary>
        /// generate new text constants
        /// </summary>
        /// <param name="sectionControls">list of section controls to check</param>
        /// <param name="NewID">global ID for new fields in dataset and new text constants</param>
        /// <param name="textConstantsToAdd">textConstants to add</param>
        public void GenerateNewTextConstants(List<SectionControl> sectionControls, ref int NewID, ref string textConstantsToAdd)
        {
            for (int i = 0; i < sectionControls.Count; i++)
            {
                SectionControl secCtrl = sectionControls[i];
                if (!(secCtrl.captionML == ""))
                {
                    //get ENU caption
                    string textCaption = secCtrl.GetCaption("ENU");
                    textCaption = secCtrl.RemoveSpecChars(textCaption);
                    textCaption = textCaption.Substring(0, textCaption.Length > 20 ? 20 : textCaption.Length);

                    //only add new text constant if the same text constant doesn't already exists
                    secCtrl.newTextConstant = "Text" + textCaption;
                    SectionControl duplicateTextConstantSecCtrl = sectionControls.Find(x => (x.newTextConstant == secCtrl.newTextConstant) && !(x == secCtrl) && (x.captionML == secCtrl.captionML));
                    if (duplicateTextConstantSecCtrl == null)
                    {
                        NewID++;
                        textConstantsToAdd += "      Text" + textCaption + "@" + NewID.ToString() + " : TextConst '" + secCtrl.captionML + "';" + Environment.NewLine;
                        secCtrl.newTextConstant = "Text" + textCaption;
                    }
                }
            }
        }

        /// <summary>
        /// Labels must be linked to their parents. Labels can be fieldcaption and must be tranformed accordingly
        /// </summary>
        /// <param name="datasetPart">dataset of the report (Part). Dataitem names are searched there</param>
        /// <param name="sectionControls">list of section controls to check</param>
        public void HandleLabels(List <DataItem> dataitemList, List<SectionControl> sectionControls)
        {
            foreach (SectionControl secCtrl in sectionControls)
            {

                //link labels to new text constants, parent controls
                if (secCtrl.controlType == "Label")
                {
                    if (secCtrl.parentControl == "")  //if no parent then a new text constant was generated
                        secCtrl.sourceExpr = secCtrl.newTextConstant;
                    else
                    {
                        //search for parent in all report sections - including those of other dataitems
                        SectionControl parent = sectionControls.Find(x => x.controlId == secCtrl.parentControl);

                        if (parent == null)
                            secCtrl.sourceExpr = secCtrl.parentControl + ":control missing";
                        else //parent control was found
                        {
                            if (!(parent.newTextConstant == null))  //if parent has generated a new text constant
                                secCtrl.sourceExpr = parent.newTextConstant;
                            else  //if parent was a field in the table -> child will be field label (fieldcaption)
                            {
                                string parentDataitemName = dataitemList.Find(x => x.indexOfDataitem == parent.dataitemIndex).dataItemVarName;
                                if (parentDataitemName == null)
                                    secCtrl.sourceExpr = "FIELDCAPTION(" + parent.sourceExpr + ")";
                                else
                                {
                                    if (parent.sourceExpr.Contains(parentDataitemName + "."))  //dataitem name is in parent sourceExpr
                                        secCtrl.sourceExpr = parent.sourceExpr.Replace(parentDataitemName + ".", parentDataitemName + ".FIELDCAPTION(") + ")";
                                    else
                                    {  //no dataitem was there - and it is not needed
                                        //fix3 - dataitem names for parents in other dataitems - even if no dataitemname can be found
                                        bool literal = false;
                                        int dotpos = 0;
                                        for (int i = 0; i < parent.sourceExpr.Length; i++)
                                        {
                                            char test1 = parent.sourceExpr[i];
                                            if (parent.sourceExpr[i] == '"')
                                            {
                                                literal = !literal;
                                            }
                                            if (!literal && parent.sourceExpr[i] == '.')
                                            {
                                                dotpos = i;
                                            }
                                        }

                                        if (!(dotpos == 0))
                                            secCtrl.sourceExpr = parent.sourceExpr.Substring(0,dotpos + 1) +  "FIELDCAPTION(" + parent.sourceExpr.Substring(dotpos + 1) + ")"; 
                                        else
                                            secCtrl.sourceExpr = "FIELDCAPTION(" + parent.sourceExpr + ")"; //original line -  no fix
                                    }
                                }
                                if (!(dataitemList.Find(x => x.indexOfDataitem == parent.dataitemIndex) == dataitemList.Find(x => x.indexOfDataitem == secCtrl.dataitemIndex))) // move child to parent dataset if parent is in different one
                                {
                                    secCtrl.dataitemIndex = parent.dataitemIndex;
                                    secCtrl.indentOfControl = parent.indentOfControl;
                                }
                            }
                        }
                    }
                }
                secCtrl.nameOfPart = System.Text.RegularExpressions.Regex.Replace(secCtrl.sourceExpr, "[^0-9a-zA-Z]+", "_");
                secCtrl.nameOfPart = secCtrl.nameOfPart.TrimStart('_');
            }
        }

        /// <summary>
        /// remove duplicate controls from list of controls (mark them as duplicates
        /// </summary>
        /// <param name="sectionControls">list of section controls to search</param>
        public void RemoveDuplicateControls(List<SectionControl> sectionControls)
        {
            foreach (SectionControl secCtrl in sectionControls)
            {
                if (secCtrl.controlType == "Shape") //ignore shapes, all shapes are marked as duplicates
                    secCtrl.duplicate = true;

                if (!(secCtrl.duplicate))
                {
                    List<SectionControl> duplicates = sectionControls.FindAll(x => (x.sourceExpr == secCtrl.sourceExpr) && (x.dataitemIndex == secCtrl.dataitemIndex));
                    foreach (SectionControl duplicate in duplicates)
                    {
                        if (!(secCtrl == duplicate))
                        {
                            duplicate.duplicate = true;
                            duplicate.originalSectionControl = secCtrl;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// handle multiple controls with same name
        /// </summary>
        /// <param name="datasetPart">dataset of the report (Part). If DataItem has a name, than that becomes the new variable name</param>
        /// <param name="sectionControls">list of section controls to search</param>
        public void HandleSameControlNames(List<DataItem> dataitemList, List<SectionControl> sectionControls)
        {
            foreach (SectionControl secCtrl in sectionControls)
            {
                if (!(secCtrl.duplicate))
                {
                    List<SectionControl> controlSameName = sectionControls.FindAll(x => (x.nameOfPart == secCtrl.nameOfPart) && (x.duplicate == false) && !(x == secCtrl));
                    if (controlSameName.Count > 0)
                    {
                        string parentDataitemName = dataitemList.Find(x => x.indexOfDataitem == secCtrl.dataitemIndex).dataItemVarName;
                        if (parentDataitemName == null)
                        {
                            int i = 2;
                            while (ControlNameUsed(sectionControls, secCtrl.nameOfPart + i))
                            {
                                i++;
                            }
                            secCtrl.nameOfPart = secCtrl.nameOfPart + i;
                        }
                        else
                        {
                            if (!ControlNameUsed(sectionControls, secCtrl.nameOfPart + "_" + parentDataitemName))
                                secCtrl.nameOfPart = secCtrl.nameOfPart + "_" + parentDataitemName;
                            else
                            {
                                int i = 2;
                                while (ControlNameUsed(sectionControls, secCtrl.nameOfPart + "_" + parentDataitemName + i))
                                {
                                    i++;
                                }
                                secCtrl.nameOfPart = secCtrl.nameOfPart + "_" + parentDataitemName + i;
                            }
                        }           
                    }
                }
            }
        }

        /// <summary>
        /// add section control data to dataset
        /// </summary>
        /// <param name="datasetPart">dataset of the report (Part) on which to add controls</param>
        /// <param name="sectionControls">list of section controls to add</param>
        public void AddNewDataToDataset(Part datasetPart, List<SectionControl> sectionControls, List<DataItem> dataItemList)
        {
            foreach (Part dataitem in datasetPart.children)
            {
                int indexOfDataitem = datasetPart.children.IndexOf(dataitem);

                //transformed dataItem
                DataItem dataItemC = dataItemList.Find(x => x.indexOfDataitem == indexOfDataitem);
                dataitem.children = new List<Part>();
                dataitem.transformedValue = dataItemC.transformedValue;
                dataitem.FindNextChildPositions(0, ref dataitem.startPos, ref dataitem.endPos);
                dataitem.startPos = dataitem.startPos + 1;
                dataitem.endPos = dataitem.endPos - 1;
                
                //transformed sections
                foreach (SectionControl secCtrl in sectionControls)
                {
                    if ((secCtrl.dataitemIndex == indexOfDataitem) && !(secCtrl.duplicate))
                    {
                        secCtrl.ComposeTransformedValueInDataitems();
                        dataitem.transformedValue += secCtrl.transformedValue + Environment.NewLine;
                    }
                }
                dataitem.FindNextChildPositions(0, ref dataitem.startPos, ref dataitem.endPos);
                dataitem.startPos = dataitem.startPos + 1;
                dataitem.endPos = dataitem.endPos - 1;
            }
        }


        /// <summary>
        /// Add new text constants to globals
        /// </summary>
        /// <param name="report">report (Part) on which to add new globals</param>
        /// <param name="textConstantsToAdd">new text constants</param>
        public void AddTextConstantsToGlobals(Part report, string textConstantsToAdd)
        {
            Part globalCode = report.FindChildByName("CODE");
            using (StringReader reader = new StringReader(globalCode.transformedValue))
            {
                string line;
                bool beginVars = false;
                bool endVars = false;
                string newTransformedValue = "";

                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Contains("VAR"))
                    {
                        beginVars = true;
                    }
                    if ((!endVars) && (beginVars) && (line.Trim() == ""))
                    {
                        newTransformedValue += textConstantsToAdd;
                        endVars = true;
                    }

                    newTransformedValue += line + Environment.NewLine;
                }
                globalCode.transformedValue = newTransformedValue;
                globalCode.FindChildren();
            }
        }

        public bool ControlNameUsed(List<SectionControl> secCtrl, string partNameToCheck)
        {
            List<SectionControl> controlSameName = secCtrl.FindAll(x => (x.nameOfPart == partNameToCheck) && (x.duplicate == false));
            return (controlSameName.Count > 0);
        }

        public void RefreshRequestForm(Part report, ref int NewID)
        {
            Part requestForm = report.FindChildByName("REQUESTFORM");
            Part prop = requestForm.FindChildByName("PROPERTIES");
            Part controls = requestForm.FindChildByName("CONTROLS");

            Part requestPage = report.FindChildByName("REQUESTPAGE");
            bool newReqPage = false;
            if (requestPage == null)
            {
                newReqPage = true;
                requestPage = requestForm;
                requestPage.transformedValue =
@"  REQUESTPAGE
  {
    PROPERTIES
    {
    }
    CONTROLS
    {
    }
  }";
                requestPage.children = new List<Part>();
                requestPage.FindNextChildPositions(0, ref requestPage.startPos, ref requestPage.endPos);
                requestPage.startPos = requestPage.startPos + 1;
                requestPage.endPos = requestPage.endPos - 1;
                int oldoffset = requestPage.parentChildOffset;
                requestPage.parentChildOffset = 0;
                requestPage.nameOfPart = requestPage.RemoveBlanksAndNewLines(requestPage.transformedValue.Substring(0, requestPage.startPos - 1));
                requestPage.FindChildren();
                requestPage.parentChildOffset = oldoffset; 
            }

            Part prop2 = requestPage.FindChildByName("PROPERTIES");
            Part controls2 = requestPage.FindChildByName("CONTROLS");

            // edit properties part
            string newContectforProp = "";
            bool skipLine;
            String indentforstruct = "1";
            //varnameforstruct = "";
            NewID++;
            using (StringReader reader = new StringReader(prop.transformedValue))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    skipLine = false;

                    if (line.Contains("Width=") | line.Contains("Height="))
                        skipLine = true;
                    if (line.Contains("OnOpenForm="))
                        line = line.Replace("OnOpenForm=", "OnOpenPage=");
                    if (!skipLine)
                        newContectforProp += line + Environment.NewLine;
                }
            }


            prop2.children = new List<Part>();
            prop2.transformedValue = newContectforProp;
            prop2.FindNextChildPositions(0, ref prop2.startPos, ref prop2.endPos);
            prop2.startPos = prop2.startPos + 1;
            prop2.endPos = prop2.endPos - 1;

            //edit controls
            List<SectionControl> transSections = new List<SectionControl>();

            foreach (Part control in controls.children)
            {
                SectionControl secCtrl = new SectionControl(control);
                if (indentforstruct.Length > 0)
                    secCtrl.indentOfControl = Convert.ToInt32(indentforstruct) + 1;
                secCtrl.ParseSectionControlProperties();
                transSections.Add(secCtrl);
            }

            // add new sections to dataset
            controls2.children = new List<Part>();
            controls2.transformedValue = "";
            foreach (SectionControl sec in transSections)
            {
                sec.ComposeTransformedValueInRequestPage();
                if (!(sec.controlType == "Label"))
                    controls2.transformedValue += Environment.NewLine + sec.transformedValue;
            }

            if (transSections.Count > 0)
            {
                controls2.transformedValue = "\n    CONTROLS\r\n" +
                                             "    {\r\n" +
                                             "      { " + (NewID + 1) + "   ;0   ;Container ;\r\n" +
                                             "                  ContainerType=ContentArea }\r\n" +
                                             "      { " + (NewID + 2) + "   ;1   ;Group     ;\r\n" +
                                             "                  CaptionML=ENU=Options }\r\n" +
                                             controls2.transformedValue + "\r\n" +
                                             "    }";
                controls2.FindNextChildPositions(0, ref controls2.startPos, ref controls2.endPos);
                controls2.startPos = controls2.startPos + 1;
                controls2.endPos = controls2.endPos - 1;
            }
            // remove requestform
            if (!newReqPage)
            {
                requestForm.children = new List<Part>();
                requestForm.transformedValue = "";
            }
        }

        public void CreateRDLCPart(Part report,List<SectionControl> sectionControls, ref List<Section> sectionsList)
        {
            string dataSets = GenerateRDLCDatasetsPart(sectionControls);
            string reportSections = GenerateRDLCReportSectionPart(sectionControls,ref sectionsList);
            string code = GenerateRDLCCodePart();


            string reportString = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Report xmlns:rd=""http://schemas.microsoft.com/SQLServer/reporting/reportdesigner"" xmlns:cl=""http://schemas.microsoft.com/sqlserver/reporting/2010/01/componentdefinition"" xmlns=""http://schemas.microsoft.com/sqlserver/reporting/2010/01/reportdefinition"">
  <AutoRefresh>0</AutoRefresh>
  <DataSources>
    <DataSource Name=""DataSource"">
      <ConnectionProperties>
        <DataProvider>SQL</DataProvider>
        <ConnectString />
      </ConnectionProperties>
      <rd:DataSourceID>570de600-864e-4169-b449-a359cc0dcb35</rd:DataSourceID>
    </DataSource>
  </DataSources>
  " + dataSets +
  "\n  " + reportSections +
  "\n  " + code + @"
  <Language>=User!Language</Language>
  <ConsumeContainerWhitespace>true</ConsumeContainerWhitespace>
  <rd:ReportUnitType>Cm</rd:ReportUnitType>
  <rd:ReportID>0eeb6585-38ae-40f1-885b-8d50088d51b4</rd:ReportID>
</Report>
    END_OF_RDLDATA";

            Part rdlData = report.FindChildByName("RDLDATA");
            if (rdlData == null)
            {
                Part codePart = report.FindChildByName("CODE");
                rdlData = new Part();
                rdlData.transformedValue += @"
  RDLDATA
  {
  }";
                rdlData.FindNextChildPositions(0, ref rdlData.startPos, ref rdlData.endPos);
                rdlData.startPos = rdlData.startPos + 1;
                rdlData.endPos = rdlData.endPos - 1;
                rdlData.parentChildOffset = codePart.parentChildOffset + codePart.partLength;
                 //= 0;
                rdlData.nameOfPart = rdlData.RemoveBlanksAndNewLines(rdlData.transformedValue.Substring(0, rdlData.startPos - 1));//"RDLDATA";
                rdlData.parentCount = codePart.parentCount;
                //requestPage.endPos = requestPage.transformedValue.Length;
                //requestPage.partLength = requestPage.transformedValue.Length;
                //.FindChildren();
                //requestPage.parentChildOffset = oldoffset; 
                
                
                report.children.Add(rdlData);
                report.RefreshTransformedValues();
            }
            
            //add string

            // reset rdlc part - so that it can be generated more than once, if user so chooses
            rdlData.transformedValue = @"
  RDLDATA
  {
  }";

            using (StringReader reader = new StringReader(rdlData.transformedValue))
            {
                string line;
                bool beginWrite = false;
                string newTransformedValue = "";

                while ((line = reader.ReadLine()) != null)
                {
                    beginWrite = false;
                    if (line.Contains("{"))
                    {
                        beginWrite = true;

                    }

                    newTransformedValue += line + Environment.NewLine;
                    if (beginWrite)
                    {
                        newTransformedValue += "\r\n" + reportString + "\r\n";
                    }

                }
                rdlData.transformedValue = newTransformedValue;
                //rdlData.FindChildren();
            }
        }

        private string GenerateDatasetFields(List<SectionControl> sectionControls)
        {
            string controls = "";
            foreach (SectionControl sec in sectionControls)
            {
                if (!sec.duplicate)
                    controls += @"
        <Field Name=""" + sec.nameOfPart + @""">
          <DataField>" + sec.nameOfPart + @"</DataField>
        </Field>";
            }
            return controls;
        }

        public string GenerateRDLCDatasetsPart(List<SectionControl> sectionControls)
        {
                        return( @"<DataSets>
    <DataSet Name=""DataSet_Result"">
      <Query>
        <DataSourceName>DataSource</DataSourceName>
        <CommandText />
      </Query>
      <Fields>
        "
                + GenerateDatasetFields(sectionControls) + 
      @"
      </Fields>
      <rd:DataSetInfo>
        <rd:DataSetName>DataSet</rd:DataSetName>
        <rd:SchemaPath>Report.xsd</rd:SchemaPath>
        <rd:TableName>Result</rd:TableName>
      </rd:DataSetInfo>
    </DataSet>
  </DataSets>");
        }

        public string GenerateRDLCReportSectionPart(List<SectionControl> sectionControls, ref List<Section> sectionsList)
        {
            decimal pageWidth = 0; // total width of the body, header segment
            decimal headerHeight = 0;
            decimal bodyHeight = 0;
            decimal footerHeight = 0;
            getRDLCWidthAndHeights(ref sectionsList, ref pageWidth, ref headerHeight, ref bodyHeight, ref footerHeight);

            int uniqueIndex = 0;
            string setData = generateSetData(ref sectionControls, sectionsList);
            string headerVizualization = generateHeaderVizualization(sectionControls, sectionsList, ref uniqueIndex);
            string bodyVizualization = generateBodyVizualization(sectionControls, sectionsList, ref uniqueIndex);
            string footerVizualization = generateFooterVizualization(sectionControls, sectionsList, ref uniqueIndex);

            //<Value EvaluationMode=""Auto"">=Code.SetData(ReportItems!CustAddr.Value,1)</Value>
            return (@"<ReportSections>
    <ReportSection>
      <Body>
        <ReportItems>
          <Tablix Name=""Body_Tablix"">
            <TablixBody>
              <TablixColumns>
                <TablixColumn>
                  <Width>" + Invariant(pageWidth) + @"cm</Width>
                </TablixColumn>
              </TablixColumns>
              <TablixRows>
                <TablixRow>
                  <Height>" + Invariant(bodyHeight) + @"cm</Height>
                  <TablixCells>
                    <TablixCell>
                      <CellContents>
                        <Rectangle Name=""Body_Tablix_Contents"">
                          <ReportItems>
                            <Tablix Name=""TableSalesInvLine"">
                              <TablixBody>
                                <TablixColumns>
                                  <TablixColumn>
                                    <Width>0.56459cm</Width>
                                  </TablixColumn>
                                </TablixColumns>
                                <TablixRows>
                                  <TablixRow>
                                    <Height>0.42301cm</Height>
                                    <TablixCells>
                                      <TablixCell>
                                        <CellContents>
                                          <Textbox Name=""CustAddr"">
                                            <KeepTogether>true</KeepTogether>
                                            <Paragraphs>
                                              <Paragraph>
                                                <TextRuns>
                                                  <TextRun>
                                                    <Value EvaluationMode=""Auto"">" + setData + @"</Value>
                                                    <Style>
                                                      <FontFamily>Segoe UI</FontFamily>
                                                      <FontSize>8pt</FontSize>
                                                      <Color>Red</Color>
                                                    </Style>
                                                  </TextRun>
                                                </TextRuns>
                                                <Style />
                                              </Paragraph>
                                            </Paragraphs>
                                            <ZIndex>3</ZIndex>
                                            <Visibility>
                                              <Hidden>true</Hidden>
                                            </Visibility>
                                            <Style>
                                              <Border />
                                              <PaddingLeft>2pt</PaddingLeft>
                                              <PaddingRight>2pt</PaddingRight>
                                              <PaddingTop>2pt</PaddingTop>
                                              <PaddingBottom>2pt</PaddingBottom>
                                            </Style>
                                          </Textbox>
                                        </CellContents>
                                      </TablixCell>
                                    </TablixCells>
                                  </TablixRow>
                                </TablixRows>
                              </TablixBody>
                              <TablixColumnHierarchy>
                                <TablixMembers>
                                  <TablixMember />
                                </TablixMembers>
                              </TablixColumnHierarchy>
                              <TablixRowHierarchy>
                                <TablixMembers>
                                  <TablixMember>
                                    <KeepTogether>true</KeepTogether>
                                  </TablixMember>
                                </TablixMembers>
                              </TablixRowHierarchy>
                              <Height>0.42301cm</Height>
                              <Width>0.56459cm</Width>
                              <Style />
                            </Tablix>" + bodyVizualization + @"
                          </ReportItems>
                          <KeepTogether>true</KeepTogether>
                          <Style />
                        </Rectangle>
                      </CellContents>
                    </TablixCell>
                  </TablixCells>
                </TablixRow>
              </TablixRows>
            </TablixBody>
            <TablixColumnHierarchy>
              <TablixMembers>
                <TablixMember />
              </TablixMembers>
            </TablixColumnHierarchy>
            <TablixRowHierarchy>
              <TablixMembers>
                <TablixMember>
                  <Group Name=""Body_Group"">
                    <PageBreak>
                      <BreakLocation>Between</BreakLocation>
                      <ResetPageNumber>true</ResetPageNumber>
                    </PageBreak>
                  </Group>
                  <DataElementOutput>Output</DataElementOutput>
                  <KeepTogether>true</KeepTogether>
                </TablixMember>
              </TablixMembers>
            </TablixRowHierarchy>
            <DataSetName>DataSet_Result</DataSetName>
            <Height>" + Invariant(bodyHeight) + @"cm</Height>
            <Width>" + Invariant(pageWidth) + @"cm</Width>
            <Style>
              <FontFamily>Segoe UI</FontFamily>
              <FontSize>8pt</FontSize>
            </Style>
          </Tablix>
        </ReportItems>
        <Height>" + Invariant(bodyHeight) + @"cm</Height>
        <Style />
      </Body>
      <Width>" + Invariant(pageWidth) + @"cm</Width>
      <Page>
        <PageHeader>
          <Height>" + Invariant(headerHeight) + @"cm</Height>
          <PrintOnFirstPage>true</PrintOnFirstPage>
          <PrintOnLastPage>true</PrintOnLastPage>
          <ReportItems>
            <Textbox Name=""SetDataTextbox"">
              <KeepTogether>true</KeepTogether>
              <Paragraphs>
                <Paragraph>
                  <TextRuns>
                    <TextRun>
                      <Value />
                      <Style>
                        <FontFamily>Segoe UI</FontFamily>
                        <FontSize>8pt</FontSize>
                      </Style>
                    </TextRun>
                  </TextRuns>
                  <Style />
                </Paragraph>
              </Paragraphs>
              <Height>11pt</Height>
              <Width>3.12746cm</Width>
              <ZIndex>1</ZIndex>
              <Visibility>
                <Hidden>=Code.SetData(ReportItems!CustAddr.Value, 1)</Hidden>
              </Visibility>
              <Style>
                <Border>
                  <Style>None</Style>
                </Border>
              </Style>
            </Textbox>" + headerVizualization + @"
          </ReportItems>
          <Style />
        </PageHeader>
        <PageFooter>
          <Height>" + Invariant(footerHeight) + @"cm</Height>
          <PrintOnFirstPage>true</PrintOnFirstPage>
          <PrintOnLastPage>true</PrintOnLastPage>" + footerVizualization + @"
          <Style>
            <Border>
              <Style>None</Style>
            </Border>
          </Style>
        </PageFooter>
        <PageHeight>29.7cm</PageHeight>
        <PageWidth>21cm</PageWidth>
        <InteractiveHeight>11in</InteractiveHeight>
        <InteractiveWidth>8.5in</InteractiveWidth>
        <LeftMargin>1.71388cm</LeftMargin>
        <RightMargin>1.05834cm</RightMargin>
        <TopMargin>1.05834cm</TopMargin>
        <BottomMargin>1.48166cm</BottomMargin>
        <ColumnSpacing>1.27cm</ColumnSpacing>
        <Style />
      </Page>
    </ReportSection>
  </ReportSections>");
        }

        public string GenerateRDLCCodePart()
        {
            return (@"<Code>Public Function BlankZero(ByVal Value As Decimal)
    if Value = 0 then
        Return """"
    end if
    Return Value
End Function

Public Function BlankPos(ByVal Value As Decimal)
    if Value &gt; 0 then
        Return """"
    end if
    Return Value
End Function

Public Function BlankZeroAndPos(ByVal Value As Decimal)
    if Value &gt;= 0 then
        Return """"
    end if
    Return Value
End Function

Public Function BlankNeg(ByVal Value As Decimal)
    if Value &lt; 0 then
        Return """"
    end if
    Return Value
End Function

Public Function BlankNegAndZero(ByVal Value As Decimal)
    if Value &lt;= 0 then
        Return """"
    end if
    Return Value
End Function

Shared Data1 as Object
Shared Data2 as Object
Shared Data3 as Object
Shared Data4 as Object

Public Function GetData(Num as Integer, Group as integer) as Object
if Group = 1 then
   Return Cstr(Choose(Num, Split(Cstr(Data1),Chr(177))))
End If

if Group = 2 then
   Return Cstr(Choose(Num, Split(Cstr(Data2),Chr(177))))
End If

if Group = 3 then
   Return Cstr(Choose(Num, Split(Cstr(Data3),Chr(177))))
End If

if Group = 4 then
   Return Cstr(Choose(Num, Split(Cstr(Data4),Chr(177))))
End If
End Function

Public Function SetData(NewData as Object,Group as integer)
  If Group = 1 and NewData &gt; """" Then
      Data1 = NewData
  End If

  If Group = 2 and NewData &gt; """" Then
      Data2 = NewData
  End If

  If Group = 3 and NewData &gt; """" Then
      Data3 = NewData
  End If

  If Group = 4 and NewData &gt; """" Then
      Data4 = NewData
  End If

  Return True
End Function

</Code>");
        }

        /// <summary>
        /// Returns number in correct (international) format
        /// </summary>
        /// <param name="input">input number, various formats</param>
        /// <returns>input number, internation format</returns>
        public string Invariant(decimal input)
        {
            return (input.ToString(CultureInfo.InvariantCulture.NumberFormat));
        }

        /// <summary>
        /// Returns width and heights for the report, also markes Sections on header, footer
        /// </summary>
        /// <param name="sectionsList">Section list, ordered by processing order, ascending</param>
        /// <param name="pageWidth">width of report, return parameter</param>
        /// <param name="headerHeight">height of header, return parameter</param>
        /// <param name="bodyHeight">height of body, return parameter</param>
        /// <param name="footerHeight">height of footer, return parameter</param>
        public void getRDLCWidthAndHeights(ref List<Section> sectionsList,ref decimal pageWidth,ref decimal headerHeight, ref decimal bodyHeight, ref decimal footerHeight){

            pageWidth = 18150; // total width of the body, header segment

            if (sectionsList.Count > 0)
                pageWidth = sectionsList.First().SectionWidth;
            pageWidth = pageWidth / 1000;

            headerHeight = 0;
            bodyHeight = 0;
            footerHeight = 0;

            markHeaderOrFooterSections(ref sectionsList);
            headerHeight = sectionsList.Where(x => x.isOnRDLCHeader).Sum(x => x.SectionHeight);
            footerHeight = sectionsList.Where(x => x.isOnRDLCFooter).Sum(x => x.SectionHeight);
            bodyHeight = sectionsList.Where(x => !(x.isOnRDLCHeader) && !(x.isOnRDLCFooter)).Sum(x => x.SectionHeight);

            if (headerHeight == 0)
                headerHeight = 1500;
            if (bodyHeight == 0)
                bodyHeight = 1500;
            if (footerHeight == 0)
                footerHeight = 1500;
            headerHeight = headerHeight / 1000;
            bodyHeight = bodyHeight / 1000;
            footerHeight = footerHeight / 1000;
        }

        /// <summary>
        /// Mark sections that are on header or footer. Can be overwriten by user
        /// </summary>
        /// <param name="sectionList">Section list, ordered by processing order, ascending</param>
        public void markHeaderOrFooterSections(ref List<Section> sectionList)
        {
            sectionList = sectionList.OrderBy(x => x.processingOrder).ToList();
            for (int i = 0; i < sectionList.Count; i++)
            {
                Section sec = sectionList[i];
                if (sec.isOnRDLCFooter)
                    break;
                if (!sec.isOnRDLCHeader && sec.userChangeOnHeaderFooter)  //user can overwrite program logic
                    break;
                else if (!sec.PrintOnEveryPage)
                    break;
                sec.isOnRDLCHeader = true;
            }

            for (int i = sectionList.Count - 1; i >= 0; i--)
            {
                Section sec = sectionList[i];
                if (sec.isOnRDLCHeader)
                    break;
                if (!sec.isOnRDLCFooter && sec.userChangeOnHeaderFooter)  //user can overwrite program logic
                    break;
                if (!sec.PrintOnEveryPage)
                    break;
                sec.isOnRDLCFooter = true;
            }
        }

        /// <summary>
        /// Resets marks on section. Used to reset user interaction
        /// </summary>
        /// <param name="sectionList">Section list to reset</param>
        public void resetHeaderOrFooterSections(ref List<Section> sectionList)
        {
            foreach (Section sec in sectionList)
            {
                sec.isOnRDLCHeader = false;
                sec.isOnRDLCFooter = false;
            }
            markHeaderOrFooterSections(ref sectionList);
        }

        /// <summary>
        /// Sets or clears header and footer properties in the whole sectionList, from which sec is a member
        /// </summary>
        /// <param name="sectionList">Section list, ordered by processing order, ascending</param>
        /// <param name="sec">section where change was made</param>
        public void updateSectionRDLCPropertiesOnHeaderChange(ref List<Section> sectionsList, Section sec)
        {
            if (sectionsList == null || sectionsList.Count == 0)
                return;

            int sectionIndex = sectionsList.IndexOf(sec);
            if (sectionIndex == -1)
                return;

            for (int i = 0; i < sectionsList.Count; i++)
            {
                Section foundSection = sectionsList[i];
                // sections before this and this section
                if (i <= sectionIndex)
                {
                    if (sec.isOnRDLCHeader)
                    {
                        foundSection.isOnRDLCHeader = true;
                        foundSection.isOnRDLCFooter = false;
                    }
                }
                // sections after this section
                else
                {
                    if (!sec.isOnRDLCHeader)
                        foundSection.isOnRDLCHeader = false;
                }
            }
        }

        /// <summary>
        /// Sets or clears header and footer properties in the whole sectionList, from which sec is a member
        /// </summary>
        /// <param name="sectionList">Section list, ordered by processing order, ascending</param>
        /// <param name="sec">section where change was made</param>
        public void updateSectionRDLCPropertiesOnFooterChange(ref List<Section> sectionsList, Section sec)
        {
            if (sectionsList == null || sectionsList.Count == 0)
                return;

            int sectionIndex = sectionsList.IndexOf(sec);
            if (sectionIndex == -1)
                return;

            for (int i = 0; i < sectionsList.Count; i++)
            {
                Section foundSection = sectionsList[i];
                // sections before this and this section
                if (i >= sectionIndex)
                {
                    if (sec.isOnRDLCFooter)
                    {
                        foundSection.isOnRDLCFooter = true;
                        foundSection.isOnRDLCHeader = false;
                    }
                }
                // sections after this section
                else
                {
                    if (!sec.isOnRDLCFooter)
                        foundSection.isOnRDLCFooter = false;
                }
            }
        }

        //generate data used for getData/setData
        public string generateSetData(ref List<SectionControl> sectionControls, List<Section> sectionsList)
        {
            int index = 0;
            sectionControls.ForEach(x => x.indexInSetData = 0); //  first reset any previous dataSet
            
            foreach (Section sec in sectionsList)
            {
                if (sec.isOnRDLCHeader || sec.isOnRDLCFooter)
                {
                    List<SectionControl> controlsInSec = sectionControls.FindAll(x => x.parentSectionId == sec.sectionId);
                    if (!(controlsInSec == null || controlsInSec.Count == 0))
                    {
                        foreach (SectionControl secCtrl in controlsInSec)
                        {
                            if (secCtrl.indexInSetData == 0)  //only if the controls hasn't already been set
                            {
                                index++;
                                if (!secCtrl.duplicate)
                                {
                                    secCtrl.indexInSetData = index;
                                }
                                else  // duplicates set originals if not already set
                                {
                                    if (!(secCtrl.originalSectionControl == null))  //fix - shapes don't have originals set
                                    {
                                        if (secCtrl.originalSectionControl.indexInSetData == 0)
                                        {
                                            secCtrl.originalSectionControl.indexInSetData = index;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

/*            =Fields!No_SalesInvHdr.Value+Chr(177) +
 Fields!No_SalesInvHdr.Value */
            string getData = "";
            List<SectionControl> sectionControlsInSetData = sectionControls.FindAll(x => x.indexInSetData > 0).OrderBy(x => x.indexInSetData).ToList();
            if (!(sectionControlsInSetData == null || sectionControlsInSetData.Count == 0))
            {
                foreach (SectionControl secCtrl in sectionControlsInSetData)
                {
                    if (getData == "")
                        getData += "=";
                    else
                        getData += " + Chr(177) +\r\n";

                    getData += "Cstr(Fields!" + secCtrl.nameOfPart + ".Value)";
                }
            }


            return getData;
        }

        // header vizualization
        public string generateHeaderVizualization(List<SectionControl> sectionControls, List<Section> sectionsList, ref int uniqueIndex)
        {
            

            string headerVizualization = "";
            int zIndex = 0; // setData textbox has value 0
            int posYSectionSum = 0;

            foreach (Section sec in sectionsList)
            {
                if (sec.isOnRDLCHeader)
                {
                    List<SectionControl> controlsInSec = sectionControls.FindAll(x => x.parentSectionId == sec.sectionId);
                    if (!(controlsInSec == null || controlsInSec.Count == 0))
                    {
                        foreach (SectionControl secCtrl in controlsInSec)
                        {
                            if ((secCtrl.controlType == "TextBox") || (secCtrl.controlType == "Label"))  //shapes are ignored
                            {
                                zIndex++;
                                uniqueIndex++;
                                headerVizualization += generateTextbox(secCtrl, sectionControls, zIndex, posYSectionSum,uniqueIndex,true);
                            }
                            else if (secCtrl.controlType == "PictureBox")
                            {
                                zIndex++;
                                uniqueIndex++;
                                headerVizualization += generateImage(secCtrl, sectionControls, zIndex, posYSectionSum, uniqueIndex, true);
                            }
                        }
                    }
                    posYSectionSum += sec.SectionHeight;
                }
            }
            return headerVizualization;
        }

        //body vizualization
        public string generateBodyVizualization(List<SectionControl> sectionControls, List<Section> sectionsList, ref int uniqueIndex)
        {


            string bodyVizualization = "";
            int zIndex = 0; //body tablix has setData tablix
            int posYSectionSum = 0;

            foreach (Section sec in sectionsList)
            {
                if (!(sec.isOnRDLCHeader || sec.isOnRDLCFooter))
                {
                    List<SectionControl> controlsInSec = sectionControls.FindAll(x => x.parentSectionId == sec.sectionId);
                    if (!(controlsInSec == null || controlsInSec.Count == 0))
                    {
                        foreach (SectionControl secCtrl in controlsInSec)
                        {
                            if ((secCtrl.controlType == "TextBox") || (secCtrl.controlType == "Label"))  //shapes are ignored
                            {
                                zIndex++;
                                uniqueIndex++;
                                bodyVizualization += generateTextbox(secCtrl, sectionControls, zIndex, posYSectionSum,uniqueIndex,false);
                            }
                            else if (secCtrl.controlType == "PictureBox")
                            {
                                zIndex++;
                                uniqueIndex++;
                                bodyVizualization += generateImage(secCtrl, sectionControls, zIndex, posYSectionSum, uniqueIndex, false);
                            }
                        }
                    }
                    posYSectionSum += sec.SectionHeight;
                }
            }
            return bodyVizualization;
        }

        //footer vizualization
        public string generateFooterVizualization(List<SectionControl> sectionControls, List<Section> sectionsList, ref int uniqueIndex)
        {


            string footerVizualization = "";
            int zIndex = -1; // footer is completely empty
            int posYSectionSum = 0;

            foreach (Section sec in sectionsList)
            {
                if (sec.isOnRDLCFooter)
                {
                    List<SectionControl> controlsInSec = sectionControls.FindAll(x => x.parentSectionId == sec.sectionId);
                    if (!(controlsInSec == null || controlsInSec.Count == 0))
                    {
                        foreach (SectionControl secCtrl in controlsInSec)
                        {
                            if ((secCtrl.controlType == "TextBox") || (secCtrl.controlType == "Label"))  //shapes are ignored
                            {
                                zIndex++;
                                uniqueIndex++;
                                footerVizualization += generateTextbox(secCtrl, sectionControls, zIndex, posYSectionSum,uniqueIndex,true);
                            }
                            else if (secCtrl.controlType == "PictureBox")
                            {
                                zIndex++;
                                uniqueIndex++;
                                footerVizualization += generateImage(secCtrl, sectionControls, zIndex, posYSectionSum, uniqueIndex, true);
                            }
                        }
                    }
                    posYSectionSum += sec.SectionHeight;
                }
            }

            if (!(footerVizualization == ""))  //if nothing is on footer, then reportItems tag should not be included
            {
                footerVizualization = @"
          <ReportItems> 
            " + footerVizualization + @"
          </ReportItems>";
            }

            return footerVizualization;
        }

        //create a textbox string to add
        public string generateTextbox(SectionControl secCtrl, List<SectionControl> sectionControls, int zIndex, int posYSectionSum, int uniqueIndex, bool headerOrFooter)
        {
            string textboxTemplate = @"<Textbox Name=""%1"">
  <CanGrow>true</CanGrow>
  <KeepTogether>true</KeepTogether>
  <Paragraphs>
    <Paragraph>
      <TextRuns>
        <TextRun>
          <Value EvaluationMode=""Auto"">=%2</Value>
          <Style>
            <FontFamily>Segoe UI</FontFamily>
            <FontSize>8pt</FontSize>%8
          </Style>
        </TextRun>
      </TextRuns>
      <Style />
    </Paragraph>
  </Paragraphs>
  <Top>%3cm</Top>
  <Left>%4cm</Left>
  <Height>%5cm</Height>
  <Width>%6cm</Width>
  <ZIndex>%7</ZIndex>
  <Style>
    <Border>
      <Style>None</Style>
    </Border>
  </Style>
</Textbox>";

            textboxTemplate = textboxTemplate.Replace("%1", "Textbox" + uniqueIndex + "_" + secCtrl.nameOfPart);
            if (headerOrFooter)
            {
                if (!secCtrl.duplicate)
                    textboxTemplate = textboxTemplate.Replace("%2", "Code.GetData(" + secCtrl.indexInSetData.ToString() + ",1)");
                else
                    textboxTemplate = textboxTemplate.Replace("%2", "Code.GetData(" + secCtrl.originalSectionControl.indexInSetData.ToString() + ",1)");
            }
            else
            {
                if (!secCtrl.duplicate)
                    textboxTemplate = textboxTemplate.Replace("%2", "Fields!" + secCtrl.nameOfPart.ToString() + ".Value");
                else
                    textboxTemplate = textboxTemplate.Replace("%2", "Fields!" + secCtrl.originalSectionControl.nameOfPart.ToString() + ".Value");
            }
            textboxTemplate = textboxTemplate.Replace("%3", Invariant((decimal)(secCtrl.controlYstart + posYSectionSum) / 1000));
            textboxTemplate = textboxTemplate.Replace("%4", Invariant((decimal)secCtrl.controlXstart / 1000));
            textboxTemplate = textboxTemplate.Replace("%5", Invariant((decimal)secCtrl.controlHeight / 1000));
            textboxTemplate = textboxTemplate.Replace("%6", Invariant((decimal)secCtrl.controlWidth / 1000));
            textboxTemplate = textboxTemplate.Replace("%7", zIndex.ToString());
            if (secCtrl.fontBold)
                textboxTemplate = textboxTemplate.Replace("%8", "\r\n            <FontWeight>Bold</FontWeight>");
            else
                textboxTemplate = textboxTemplate.Replace("%8", "");

            if (headerOrFooter)
                textboxTemplate = AddBlanksToLines(textboxTemplate, 12);
            else
                textboxTemplate = AddBlanksToLines(textboxTemplate, 28);
            
            return textboxTemplate;

        }

        //header and lines have different indentations, add indent as neccesary
        public string AddBlanksToLines(string input, int noOfBlanksToAdd)
        {
            string output="";
            string blanksToAdd = new string(' ', noOfBlanksToAdd);
            using (StringReader reader = new StringReader(input))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    //if (!(line.Trim() == (""))) //new line exception
                        line = blanksToAdd + line;
                    output += "\r\n" +  line;
                }
                return output;
            }
        }

        public string generateImage(SectionControl secCtrl, List<SectionControl> sectionControls, int zIndex, int posYSectionSum, int uniqueIndex, bool headerOrFooter)
        {
            string textboxTemplate = @"
<Image Name=""%1"">
  <Source>Database</Source>
  <Value>=Convert.ToBase64String(%2)</Value>
  <MIMEType>image/bmp</MIMEType>
  <Sizing>FitProportional</Sizing>
  <Top>%3cm</Top>
  <Left>%4cm</Left>
  <Height>%5cm</Height>
  <Width>%6cm</Width>
  <ZIndex>%7</ZIndex>
  <DataElementOutput>NoOutput</DataElementOutput>
</Image>";

            textboxTemplate = textboxTemplate.Replace("%1", "Picture" + uniqueIndex + "_" + secCtrl.nameOfPart);
            if (headerOrFooter)
            {
                if (!secCtrl.duplicate)
                    textboxTemplate = textboxTemplate.Replace("%2", "Code.GetData(" + secCtrl.indexInSetData.ToString() + ",1)");
                else
                    textboxTemplate = textboxTemplate.Replace("%2", "Code.GetData(" + secCtrl.originalSectionControl.indexInSetData.ToString() + ",1)");
            }
            else
            {
                if (!secCtrl.duplicate)
                    textboxTemplate = textboxTemplate.Replace("%2", "Fields!" + secCtrl.nameOfPart.ToString() + ".Value");
                else
                    textboxTemplate = textboxTemplate.Replace("%2", "Fields!" + secCtrl.originalSectionControl.nameOfPart.ToString() + ".Value");
            }
            textboxTemplate = textboxTemplate.Replace("%3", Invariant((decimal)(secCtrl.controlYstart + posYSectionSum) / 1000));
            textboxTemplate = textboxTemplate.Replace("%4", Invariant((decimal)secCtrl.controlXstart / 1000));
            textboxTemplate = textboxTemplate.Replace("%5", Invariant((decimal)secCtrl.controlHeight / 1000));
            textboxTemplate = textboxTemplate.Replace("%6", Invariant((decimal)secCtrl.controlWidth / 1000));
            textboxTemplate = textboxTemplate.Replace("%7", zIndex.ToString());

            if (headerOrFooter)
                textboxTemplate = AddBlanksToLines(textboxTemplate, 12);
            else
                textboxTemplate = AddBlanksToLines(textboxTemplate, 28);

            return textboxTemplate;

        }
    }
}
