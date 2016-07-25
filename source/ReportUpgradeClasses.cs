using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace $safeprojectname$
{
    public class Part
    {
        public List<Part> children;
        public int startPos;
        public int endPos;
        public string nameOfPart;
        public string valueOfPart;
        public string transformedValue;
        public int parentChildOffset;  // begin position for nameOfPart
        public int parentCount;
        public int partLength;

        public Part(string input)
        {
            nameOfPart = "Imported text";
            valueOfPart = input;
            startPos = 0;
            endPos = input.Length;
            children = new List<Part>();
            parentChildOffset = 0;
            parentCount = 0;
            transformedValue = input;
            partLength = input.Length;
        }

        public Part()
        {
            nameOfPart = "";
            valueOfPart = "";
            startPos = 0;
            endPos = 0;
            children = new List<Part>();
            parentChildOffset = 0;
            parentCount = 0;
        }

        /// <summary>
        ///  deconstruct method - parses the input file, and creates a structure based on it.
        ///  Recursive call.
        /// </summary>
        public void FindChildren()
        {
            int searchFromPos = startPos - parentChildOffset;
            int fromPos = 0;
            int toPos = 0;
            children = new List<Part>();  //drop current children (if any)
            while (FindNextChildPositions(searchFromPos, ref fromPos, ref toPos))
            {
                Part child = new Part(transformedValue.Substring(searchFromPos, toPos - searchFromPos));  //always search from transformed values -in case the values have already been changed
                child.nameOfPart = RemoveBlanksAndNewLines(transformedValue.Substring(searchFromPos, fromPos - 1 - searchFromPos));
                child.startPos = fromPos + 1;
                child.endPos = toPos - 1;
                child.parentChildOffset = searchFromPos;
                child.parentCount = parentCount + 1;
                children.Add(child);
                child.FindChildren();

                searchFromPos = toPos + 1;
            }
        }

        /// <summary>
        /// find next child position
        /// </summary>
        /// <param name="searchFromPos">starting position within parent</param>
        /// <param name="fromPos">position of starting bracket (filled in method)</param>
        /// <param name="toPos">position of ending bracked (filled in method)</param>
        /// <returns>is child found?</returns>
        public bool FindNextChildPositions(int searchFromPos, ref int fromPos, ref int toPos)
        {
            Regex reg1 = new Regex("{");
            Match mat1 = reg1.Match(transformedValue, searchFromPos);
            if (!mat1.Success)
                return false;
            fromPos = mat1.Index;

            int openBr = 1;
            int charCount = 0;
            while (openBr != 0)
            {
                charCount += 1;
                if (charCount == 1000000)
                    return false;
                else if (transformedValue.Length <= (fromPos + charCount))
                    return false;
                if (transformedValue[fromPos + charCount] == '{')
                    openBr += 1;
                if (transformedValue[fromPos + charCount] == '}')
                    openBr -= 1;
            }
            toPos = fromPos + charCount + 1;
            return true;
        }

        /// <summary>
        ///  remove blanks and new lines from string and returnes the modified string. Used for name of Part
        /// </summary>
        /// <param name="input">input string</param>
        /// <returns></returns>
        public string RemoveBlanksAndNewLines(string input)
        {
            return Regex.Replace(input, "[\t\r\n ]", "");
        }

        /// <summary>
        /// refresh transformed values after transformations
        /// </summary>
        public void RefreshTransformedValues()
        {
            int correctedoffset = 0;
            int oldTransValueLength;
            int originalLength = valueOfPart.Length;
            foreach (Part child in children)
            {
                oldTransValueLength = transformedValue.Length;
                string oldtransformedValue = transformedValue;
                child.RefreshTransformedValues();
                oldTransValueLength = transformedValue.Length;

                int changedChildLength = 0;
                if (!(child.partLength == child.transformedValue.Length))
                    changedChildLength = -child.transformedValue.Length + child.partLength;

                System.String str1 = transformedValue;
                System.String firstPart2 = str1.Substring(0, child.parentChildOffset + correctedoffset);

                string firstPart = transformedValue.Substring(0, child.parentChildOffset + correctedoffset);
                string secondPart = child.transformedValue;
                string thirdPart = transformedValue.Substring(child.parentChildOffset + correctedoffset + child.transformedValue.Length + changedChildLength);
                transformedValue = firstPart + secondPart + thirdPart;
                bool Equal = transformedValue == valueOfPart;

                int realOldLength = firstPart.Length + child.partLength + thirdPart.Length;

                child.parentChildOffset += correctedoffset;
                child.partLength = child.transformedValue.Length;
                correctedoffset += transformedValue.Length - realOldLength;
            }
        }

        /// <summary>
        /// custum ToString method for this object
        /// </summary>
        /// <returns></returns> 
        public override string ToString()
        {
            string indent = "   ";
            string indents = "";
            for (int i = 0; i < parentCount; i++)
            {
                indents = indents + indent;
            }
            if (nameOfPart != "")
                return indents + nameOfPart;
            else
                return indents + ".";
        }

        /// <summary>
        /// find part by exact name
        /// </summary>
        /// <param name="name">name of part to find</param>
        /// <returns>found part or null</returns>
        public Part FindPartByName(string name)
        {
            if (nameOfPart == name)
                return this;
            foreach (Part child in children)
            {
                Part match = null;
                match = child.FindPartByName(name);
                if (match != null)
                    return match;
            }
            return null;
        }

        /// <summary>
        /// find part by name, search only direct children
        /// </summary>
        /// <param name="name">name of part to find</param>
        /// <returns>found part or null</returns>
        public Part FindChildByName(string name)
        {
            foreach (Part child in children)
            {
                if (child.nameOfPart == name)
                    return child;
            }
            return null;
        }

        /// <summary>
        /// changes all occurances of the input to a new value.
        /// Recursivelly called on children
        /// </summary>
        /// <param name="oldValue">old value</param>
        /// <param name="newValue">new value</param>
        public void ChangeValuesInPart(string oldValue, string newValue)
        {
            int oldTransValueLength = transformedValue.Length;
            transformedValue = Regex.Replace(transformedValue, oldValue, newValue);
            //refreshed = true;
            foreach (Part child in children)
            {
                child.ChangeValuesInPart(oldValue, newValue);
            }

            int fromPos = 0;
            int toPos = 0;
            if (FindNextChildPositions(0, ref fromPos, ref toPos))
            {
                nameOfPart = RemoveBlanksAndNewLines(transformedValue.Substring(0, fromPos - 1));
                startPos = fromPos + 1;
                endPos = toPos - 1;
            }
        }

        /// <summary>
        /// method for reading properties from parts. Used in both Section and SectionControl
        /// </summary>
        /// <param name="line">read line</param>
        /// <param name="pattern">pattern to search</param>
        /// <param name="isMultiLine">indicates, whether the line is part of a multiline property</param>
        /// <param name="multiLineControl">name of the multiline property</param>
        /// <returns>found property value</returns>
        public string ReadSubdataFromLine(string line, string pattern, ref bool isMultiLine, ref string multiLineControl)
        {
            int indexBegin;
            int indexEnd;
            if (isMultiLine)
            {
                if (!(pattern == multiLineControl))
                {
                    return "";
                }
                indexBegin = 0;
                indexEnd = line.IndexOf("]");
                if (indexEnd > 0)
                {
                    isMultiLine = false;
                    multiLineControl = "";
                    return line.Substring(indexBegin, indexEnd - indexBegin).Trim();  // skip ']' char, no new line
                }
                return Environment.NewLine + line.Substring(indexBegin).Trim();
            }
            else if (line.Contains(pattern))
            {
                indexBegin = line.IndexOf(pattern) + pattern.Length;
                indexEnd = line.IndexOf("}", indexBegin);
                if (indexEnd < 1)
                    indexEnd = line.IndexOf(";", indexBegin);
                if (line.IndexOf("=[", indexBegin - 1) > 0)
                {
                    isMultiLine = true;
                    multiLineControl = pattern;
                    indexBegin = line.IndexOf("=[", indexBegin - 1) + 2;  // skip '[' char
                }
                if (isMultiLine)
                    return line.Substring(indexBegin).Trim();
                else
                    return line.Substring(indexBegin, indexEnd - indexBegin).Trim();
            }
            return "";
        }
    }



    public class DataItem : Part
    {
        //dataitem specific fields
        public string dataItemVarName;
        public int dataItemIndent = 0;
        public int dataItemId = 0;
        public int indexOfDataitem = 0;
        
        public DataItem(Part fromPart)
        {
            nameOfPart = fromPart.nameOfPart;
            valueOfPart = fromPart.transformedValue;
            startPos = fromPart.startPos;
            endPos = fromPart.endPos;
            parentChildOffset = fromPart.parentChildOffset;
            parentCount = fromPart.parentCount;
            children = fromPart.children;
        }

        public void ParseDataItemProperties()
        {
            Part prop = FindChildByName("PROPERTIES");
            //using (StringReader reader = new StringReader(valueOfPart))
            using (StringReader reader = new StringReader(prop.transformedValue.Substring(prop.startPos)))
            {
                int lineNo = 0;
                bool skipLine;                
                string line;
                bool isMultiLine = false;
                string multiLineControl = "";
                string foundString = "";
                while ((line = reader.ReadLine()) != null)
                {
                    lineNo++;
                    skipLine = false;
                    //string onPreSection = readCALCodeFromLine(line, "OnPreSection=", reader);


                    if (lineNo == 1)
                        skipLine = true;
                    foundString = ReadSubdataFromLine(line, "DataItemIndent=", ref isMultiLine, ref multiLineControl);
                    if (!((skipLine) || (foundString == "")))
                    {
                        dataItemIndent += Convert.ToInt32(foundString);
                        skipLine = true;
                    }
                    foundString = ReadSubdataFromLine(line, "DataItemVarName=", ref isMultiLine, ref multiLineControl);
                    if (!((skipLine) || (foundString == "")))
                    {
                        dataItemVarName += foundString;
                        skipLine = true;
                    }
                    //fix 2 - obsolete properties
                    foundString = ReadSubdataFromLine(line, "NewPagePerRecord=", ref isMultiLine, ref multiLineControl);
                    if (!((skipLine) || (foundString == "")))
                    {
                        skipLine = true;
                    }
                    foundString = ReadSubdataFromLine(line, "NewPagePerGroup=", ref isMultiLine, ref multiLineControl);
                    if (!((skipLine) || (foundString == "")))
                    {
                        skipLine = true;
                    }
                    foundString = ReadSubdataFromLine(line, "GroupTotalFields=", ref isMultiLine, ref multiLineControl);
                    if (!((skipLine) || (foundString == "")))
                    {
                        skipLine = true;
                    }

                    if (!skipLine)
                        transformedValue += line + Environment.NewLine;

                }
            }
            ComposeTransformedValue();
        }

        public void ComposeTransformedValue()
        {
            string indent = "";
            if (dataItemIndent > 0)
                indent  = dataItemIndent.ToString();
            transformedValue = "    {" + dataItemId + ";" + indent + ";DataItem ;" + dataItemVarName + ";" + Environment.NewLine + transformedValue;
        }

    }

    public class Section : Part
    {
        public string SectionType;
        public int SectionWidth;
        public int SectionHeight;
        public bool PrintOnEveryPage;
        public int dataitemIndex;
        public List<SectionControl> ChildSectionControls;
        public int sectionId;
        public int processingOrder;  // used for processing, sections are orders based on type (already ordered in input file) and sections on children dataitems
        public bool isOnRDLCHeader = false;
        public bool isOnRDLCFooter = false;
        public bool userChangeOnHeaderFooter = false;

        public Section(Part fromPart)
        {
            nameOfPart = fromPart.nameOfPart;
            valueOfPart = fromPart.transformedValue;
            startPos = fromPart.startPos;
            endPos = fromPart.endPos;
            parentChildOffset = fromPart.parentChildOffset;
            parentCount = fromPart.parentCount;
            List<SectionControl> ChildSectionControls = new List<SectionControl>();
        }

        public void ParseSectionProperties()
        {
            using (StringReader reader = new StringReader(valueOfPart))
            {
                string line;
                bool isMultiLine = false;
                string multiLineControl = "";
                string sectionWidthString="";
                string sectionHeightString="";
                while ((line = reader.ReadLine()) != null)
                {
                    //string onPreSection = readCALCodeFromLine(line, "OnPreSection=", reader);

                    line = line.Trim();  //remove begin and end whitespace
                    SectionType += ReadSubdataFromLine(line, "SectionType=", ref isMultiLine, ref multiLineControl);
                    sectionWidthString += ReadSubdataFromLine(line, "SectionWidth=", ref isMultiLine, ref multiLineControl);
                    sectionHeightString += ReadSubdataFromLine(line, "SectionHeight=", ref isMultiLine, ref multiLineControl);
                    PrintOnEveryPage = PrintOnEveryPage || ReadSubdataFromLine(line, "PrintOnEveryPage=", ref isMultiLine, ref multiLineControl).Equals("Yes");
                    
                }
                if (!(sectionWidthString == ""))
                    SectionWidth = Convert.ToInt32(sectionWidthString);
                if (!(sectionHeightString == ""))
                    SectionHeight = Convert.ToInt32(sectionHeightString);
            }

        }

        //experimental -not finished
        public string readCALCodeFromLine(string line, string pattern, StringReader reader)
        {
            string output = "";
            string visibilityPrototype = "";
            string allCodeLines = "";
            if (line.Contains(pattern))
            {
                int matchingIndex = line.IndexOf("BEGIN");
                //while (((line = reader.ReadLine()) != null) && (!line.Contains("END;")))
                //while(reader.read)
                while (((line = reader.ReadLine()) != null) && (!(line.IndexOf("END;") == matchingIndex)))  //read all the code lines
                {
                    output += line;
                    if (line.Contains("CurrReport.SHOWOUTPUT("))
                    {
                    visibilityPrototype = line.Substring(line.IndexOf("CurrReport.SHOWOUTPUT(") + "CurrReport.SHOWOUTPUT(".Length, 
                        line.IndexOf(")") - (line.IndexOf("CurrReport.SHOWOUTPUT(") + "CurrReport.SHOWOUTPUT(".Length));
                    }
                    allCodeLines += line.Trim() + " ";
                }
                allCodeLines = allCodeLines.Trim();
                char[] delimeter = new char[] {';'};
                string[] codeLines = allCodeLines.Split(delimeter, StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < codeLines.Length; i++)
                {
                    if (codeLines[i].Contains("CurrReport.SHOWOUTPUT"))
                    {
                        //IF PrintToExcel THEN CurrReport.SHOWOUTPUT(FALSE) ELSE CurrReport.SHOWOUTPUT(PrintAmountsInLCY)
                        //(?:IF[( ]*([^)]+)[ )]*THEN[ ]*)*CurrReport.SHOWOUTPUT[( ]*([^)]+)[ )]*(?:ELSE[ (]*CurrReport.SHOWOUTPUT[( ]*([^)]+)[ )]*)*

                        Regex reg = new Regex("(?:IF[( ]*([^)]+)[ )]*THEN[ ]*)*CurrReport.SHOWOUTPUT[( :=]*([^)]+)[ )]*(?:ELSE[ (]*CurrReport.SHOWOUTPUT[( :=]*([^)]+)[ )]*)*");
                        Match mat1 = reg.Match(codeLines[i]);
                        //List<string> resultArr = 
                        GroupCollection grCol = mat1.Groups;
                    }
                }
            }
            return output;
        }

        /// <summary>
        /// assigns an int to each section types, used in ordering, less means that section type is processed sooner
        /// </summary>
        /// <returns></returns>
        public int sectionTypeAsInt()
        {
            switch (SectionType)
            {
                case "Header": return 1;
                case "TransHeader": return 2;
                case "GroupHeader": return 3;
                case "Body": return 4;
                case "GroupFooter": return 5;
                case "TransFooter": return 6;
                case "Footer": return 7;
                default: return -1;
            }
        }


    }

    public class SectionControl : Part
    {
        public string controlId = "";
        public int indentOfControl = 1;
        public string controlType;
        public int controlXstart;
        public int controlYstart;
        public int controlWidth;
        public int controlHeight;
        public string sourceExpr = "";
        public string captionML = "";
        public string parentControl = "";
        public bool fontBold = false;
        public string newTextConstant;  //link to new text constant - if necessary
        public int dataitemIndex;  //link to dataitem
        public string optionCaptionML = "";
        public bool duplicate = false;  //duplicate controls don't get added to dataset, SetData
        public int parentSectionId = 0;  //link to parent section
        public SectionControl originalSectionControl; // connects duplicates to original controls
        public int indexInSetData = 0;  // link to SetData structure

        public SectionControl(Part fromPart)
        {
            nameOfPart = fromPart.nameOfPart;
            valueOfPart = fromPart.transformedValue;
            startPos = fromPart.startPos;
            endPos = fromPart.endPos;
            parentChildOffset = fromPart.parentChildOffset;
            parentCount = fromPart.parentCount;
        }

        public void ParseSectionControlProperties()
        {
            using (StringReader reader = new StringReader(valueOfPart))
            {
                bool beginRead = false;
                string line;
                bool isMultiLine = false;
                string multiLineControl = "";
                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Trim();  //remove begin and end whitespace
                    int indexBegin;
                    int indexEnd;
                    if (line.Contains("{"))  //first line logic is different, the following fields have to be specially parsed
                    {
                        beginRead = true;
                        indexBegin = line.IndexOf("{") + 1;
                        indexEnd = line.IndexOf(";", indexBegin);
                        controlId = line.Substring(indexBegin, indexEnd - indexBegin).Replace(" ", "");
                        indexBegin = indexEnd + 1;
                        indexEnd = line.IndexOf(";", indexBegin);
                        controlType = line.Substring(indexBegin, indexEnd - indexBegin).Replace(" ", "");

                        indexBegin = indexEnd + 1;
                        indexEnd = line.IndexOf(";", indexBegin);
                        controlXstart = Convert.ToInt32(line.Substring(indexBegin, indexEnd - indexBegin).Replace(" ", ""));
                        indexBegin = indexEnd + 1;
                        indexEnd = line.IndexOf(";", indexBegin);
                        controlYstart = Convert.ToInt32(line.Substring(indexBegin, indexEnd - indexBegin).Replace(" ", ""));
                        indexBegin = indexEnd + 1;
                        indexEnd = line.IndexOf(";", indexBegin);
                        controlWidth = Convert.ToInt32(line.Substring(indexBegin, indexEnd - indexBegin).Replace(" ", ""));
                        indexBegin = indexEnd + 1;
                        indexEnd = line.IndexOf(";", indexBegin);
                        controlHeight = Convert.ToInt32(line.Substring(indexBegin, indexEnd - indexBegin).Replace(" ", ""));
                    }
                    if (beginRead)
                    {
                        sourceExpr += ReadSubdataFromLine(line, "SourceExpr=", ref isMultiLine, ref multiLineControl);
                        parentControl += ReadSubdataFromLine(line, "ParentControl=", ref isMultiLine, ref multiLineControl).Trim();
                        optionCaptionML += ReadSubdataFromLine(line, "OptionCaptionML=", ref isMultiLine, ref multiLineControl).Trim();  //sequence important!  optionCaption is mre restricitive than Caption
                        captionML += ReadSubdataFromLine(line, "CaptionML=", ref isMultiLine, ref multiLineControl);
                        fontBold = fontBold || (ReadSubdataFromLine(line, "FontBold=", ref isMultiLine, ref multiLineControl) == "Yes");
                    }
                }
            }
        }

        /// <summary>
        /// Used for controls in the dataitems
        /// </summary>
        public void ComposeTransformedValueInDataitems()
        {
            //fix 5 - transform forbidden characters in name >>>
            Regex pattern = new Regex("[^a-zA-Z0-9]");
            nameOfPart = pattern.Replace(nameOfPart, "_");
            //fix 5 <<<

            string transformedValueIndent = "";
            transformedValue += "    { " + controlId + ";" + indentOfControl.ToString();
            transformedValueIndent = new string(' ', transformedValue.Length);
            //fix 5 >>>
            transformedValue += ";Column  ;" + nameOfPart + ";" + System.Environment.NewLine; // - orig line
            //transformedValue += ";Column  ;" + pattern.Replace(nameOfPart,"_") + ";" + System.Environment.NewLine;
            //fix 5 <<<
            if (!(sourceExpr == ""))
                transformedValue += transformedValueIndent + "SourceExpr=" + sourceExpr + ";" + System.Environment.NewLine;
            transformedValue += transformedValueIndent + "}";
        }

        /// <summary>
        /// Used for controls in the requestform
        /// </summary>
        public void ComposeTransformedValueInRequestPage()
        {
            //fix 5 - transform forbidden characters in name >>>
            Regex pattern = new Regex("[^a-zA-Z0-9]");
            //fix 5 <<<
            
            string transformedValueIndent = "";
            transformedValue += "      { " + controlId + ";" + indentOfControl.ToString();
            transformedValueIndent = new string(' ', transformedValue.Length);
            //fix 5 >>>
            //transformedValue += ";Field  ;" + nameOfPart + System.Environment.NewLine;  //mychanges
            transformedValue += ";Field  ;" + pattern.Replace(nameOfPart, "_") + System.Environment.NewLine;  //mychanges
            //fix 5 <<<
            if (!(sourceExpr == ""))
                transformedValue += transformedValueIndent + "SourceExpr=" + sourceExpr + ";" + System.Environment.NewLine;
            if (!(captionML == ""))
                transformedValue += transformedValueIndent + "CaptionML=[" + captionML + "];" + System.Environment.NewLine;
            if (!(optionCaptionML == ""))
                transformedValue += transformedValueIndent + "OptionCaptionML=[" + optionCaptionML + "];" + System.Environment.NewLine;
            transformedValue += transformedValueIndent + "}";
        }

        // get caption, in use when generating text constants
        public string GetCaption(string LanguageCode)
        {
            if (captionML == "")
                return "";
            
            int indexBegin ;
            int indexEnd;

            Regex reg1 = new Regex(LanguageCode +  "=");
            //Match mat1 = reg1.Match(captionML);
            Match mat1 = Regex.Match(captionML, LanguageCode + "=");
            if (mat1.Success)
            {
                indexBegin = mat1.Index + LanguageCode.Length + 1;
            }
            else
            {
                mat1 = Regex.Match(captionML, "[A-Z]{3}=");
                indexBegin = mat1.Index + LanguageCode.Length + 1;
            }

            mat1 = Regex.Match(captionML.Substring(indexBegin), ";");
            if (mat1.Success)
                indexEnd = mat1.Index;
            else
                indexEnd = captionML.Substring(indexBegin).Length;
            return captionML.Substring(indexBegin, indexEnd);
        }

        public string RemoveSpecChars(string input)
        {
            return Regex.Replace(input, "[^0-9a-zA-Z]+", "");
        }
    }

    public class DataItemHierarhy
    {
        public int id;
        public int indent;
        public List<DataItemHierarhy> children;
        public DataItemHierarhy parent;
        
        public DataItemHierarhy()
        {
            id = -1;
            indent = -1;
            children = new List<DataItemHierarhy>();
        }

        public DataItemHierarhy(int NewId, int newIndent)
        {
            id = NewId;
            indent = newIndent;
            children = new List<DataItemHierarhy>();
        }

        /// <summary>
        /// Add new DataItem to hierarchy
        /// </summary>
        /// <param name="NewId">DataItem id</param>
        /// <param name="newIndent">DataItem indent</param>
        /// <returns>added DataItem in Hierarchy</returns>
        public DataItemHierarhy AddDataItem(int NewId,int newIndent)
        {
            DataItemHierarhy item = new DataItemHierarhy(NewId, newIndent);
            if (item.indent > indent) //child (always first child)
            {
                item.parent = this;
                children.Add(item);
            }
            else if (item.indent == indent) //sibling
            {
                item.parent = this.parent;
                item.parent.children.Add(item);
            }
            else  //indent is smaller, search in parent, recursive
            {
                return parent.AddDataItem(NewId, newIndent);
            }
            return item;
        }

        /// <summary>
        /// Order sections for processing
        /// </summary>
        /// <param name="sectionsList">List of sections to order</param>
        /// <param name="nextOrderNo">Lat used processing order no. 1 when called on start, is recursivelly increased</param>
        public void TraverseHierarchyAndOrderSections(ref List<Section> sectionsList, ref int nextOrderNo)
        {
            string path = id.ToString();
            List<Section> dataitemSections = sectionsList.FindAll(x => (x.dataitemIndex == id) && (new[] { "Header", "TransHeader", "GroupHeader", "Body" }.Contains(x.SectionType)));
            //dataitemSections.OrderBy()
            //    dataitemSections.Sort()
            foreach (Section sec in dataitemSections)
            {
                sec.processingOrder = nextOrderNo;
                nextOrderNo++;
            }
            foreach (DataItemHierarhy child in children)
            {
                child.TraverseHierarchyAndOrderSections(ref sectionsList, ref nextOrderNo);
            }
            dataitemSections = sectionsList.FindAll(x => (x.dataitemIndex == id) && (new[] { "GroupFooter", "TransFooter", "Footer" }.Contains(x.SectionType)));
            foreach (Section sec in dataitemSections)
            {
                sec.processingOrder = nextOrderNo;
                nextOrderNo++;
            }
        }
    }
}
