using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace $safeprojectname$
{
    public partial class ReportUpgradeForm : Form
    {
        Part allReports;
        List<SectionControl> sectionControls;
        List<Section> sectionsList;
        List<DataItem> dataItemList;
        public ReportUpgradeForm()
        {
            InitializeComponent();
        }

        private void buttonImport_Click(object sender, EventArgs e)
        {
            Stream myStream = null;
            OpenFileDialog dial1 = new OpenFileDialog();
            //dial1.InitialDirectory = "C:\\";
            dial1.Filter = "txt files (*.txt)|*.txt";
            dial1.FilterIndex = 1;
            dial1.RestoreDirectory = true;

            if (dial1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if ((myStream = dial1.OpenFile()) != null)
                    {
                        string ImportedText;
                        Encoding enc = Encoding.GetEncoding(852);  // set as DOS Central European
                        StreamReader strRead1 = new StreamReader(myStream, enc);
                        ImportedText = strRead1.ReadToEnd();
                        allReports = new Part(ImportedText);
                        textBoxOriginalText.Text = allReports.valueOfPart;
                        textBoxTransformed.Text = allReports.transformedValue;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error has occured: " + ex);
                }
            }
        }

        private void buttonDecustruct_Click(object sender, EventArgs e)
        {
            //try
            //{

            allReports.FindChildren();

            // some limitations
            if (!(allReports.children.Count == 1) || !(allReports.children[0].nameOfPart.Contains("Report")))
            {
                MessageBox.Show("You can only import a txt file with one report object",
                        "Error",MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            listBox1.Items.Clear();
            listBox1.Items.Add(allReports);
            FillListWithChildren(allReports);

            MessageBox.Show("Done");
            /*}
            catch (Exception ex)
            {
                MessageBox.Show("An error has occured: " + ex);
            }*/
        }

        private void buttonExport_Click(object sender, EventArgs e)
        {
            try
            {
                Stream myStream = null;
                SaveFileDialog dial2 = new SaveFileDialog();
                dial2.Filter = "txt files (*.txt)|*.txt";
                dial2.FilterIndex = 1;
                dial2.RestoreDirectory = true;

                if (dial2.ShowDialog() == DialogResult.OK)
                {
                    if ((myStream = dial2.OpenFile()) != null)
                    {
                        Encoding enc = Encoding.GetEncoding(852); // set as DOS Central European
                        StreamWriter strWrite1 = new StreamWriter(myStream, enc);
                        strWrite1.Write(allReports.transformedValue);
                        strWrite1.Close();

                        MessageBox.Show("Successfully exported");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error has occured: " + ex);
            }
        }

        private void FillListWithChildren(Part par)
        {
            foreach (Part child in par.children)
            {
                listBox1.Items.Add(child);
                FillListWithChildren(child);
            }
        }

        private void buttonTransform_Click(object sender, EventArgs e)
        {
            //try
            //{

            ReportUpgradeFunctions RepUpgFunc = new ReportUpgradeFunctions();
            int NewID = 1110000;

            foreach (Part report in allReports.children)
            {
                // TRANSFORM DATAITEMS INTO DATASET
                RepUpgFunc.RefreshDataset(report, ref NewID, ref sectionControls, ref sectionsList, ref dataItemList);
                RefreshChanges(allReports);

                //REQUESTFORM->REQUESTPAGE
                RepUpgFunc.RefreshRequestForm(report, ref NewID);
                RefreshChanges(allReports);

                //GENERATE RDLC
                RepUpgFunc.CreateRDLCPart(report, sectionControls, ref sectionsList);
                RefreshChanges(allReports);
            }
            RefreshChanges(allReports);

            textBoxOriginalText.Text = allReports.valueOfPart;
            textBoxTransformed.Text = allReports.transformedValue;

            

            MessageBox.Show("Done");
            /*}
            catch (Exception ex)
            {
                MessageBox.Show("An error has occured: " + ex);
            }*/
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Part ploc = (Part)listBox1.SelectedItem;

            textBoxOriginalText.Text = ploc.valueOfPart;
            textBoxTransformed.Text = ploc.transformedValue;
        }

        private void RefreshChanges(Part p3)
        {
            p3.RefreshTransformedValues();
            listBox1.Items.Clear();
            listBox1.Items.Add(p3);
            FillListWithChildren(p3);
        }

        /// <summary>
        /// Method so that CTRL + A works on a multiline textbox controls on a form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && (e.KeyCode == Keys.A))
            {
                if (sender != null)
                    ((TextBox)sender).SelectAll();
                e.Handled = true;
            }
        }

        private void viewSections_Click(object sender, EventArgs e)
        {
            //error handling
            if (sectionsList == null || sectionsList.Count == 0)
            {
                MessageBox.Show("No sections exists fer the report.\n\rReport must be transformed, before the sections can be shown",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            SectionViewerForm f2 = new SectionViewerForm();
            f2.setReportData(ref sectionsList, ref sectionControls);

            f2.FormClosed += new FormClosedEventHandler(childForm_FormClosed);

            f2.drawSectionStructure();
            f2.Show();

        }

        private void childForm_FormClosed(object sender, EventArgs e)
        {
            ReportUpgradeFunctions repUpgFunc = new ReportUpgradeFunctions();
            repUpgFunc.CreateRDLCPart(allReports.children[0], sectionControls, ref sectionsList);
            RefreshChanges(allReports);
        }



    }
}
