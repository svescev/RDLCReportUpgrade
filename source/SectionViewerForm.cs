using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace $safeprojectname$
{
    public partial class SectionViewerForm : Form
    {
        List<SectionControl> sectionControls;
        List<Section> sectionsList;
        Panel formPanel;
        ReportUpgradeFunctions repUpgFunc = new ReportUpgradeFunctions();


        public SectionViewerForm()
        {
            InitializeComponent();
        }

        public void setReportData(ref List<Section> secList, ref List<SectionControl> secCtrlList)
        {
            sectionsList = secList;
            sectionControls = secCtrlList;
        }

        public void drawSectionStructure()
        {
            //Panel formPanel = new Panel();
            formPanel = new Panel();
            formPanel.Dock = DockStyle.Fill;
            this.Controls.Add(formPanel);
            formPanel.AutoScroll = true;

            int posX = 10;
            int posY = 10;
            //List<Section> orderedSectionList = sectionsList.OrderBy(x => x.processingOrder).ToList();
            for (int i = 0; i < sectionsList.Count;i++ )
            //foreach (Section sec in orderedSectionList)
            {
                Section sec = sectionsList[i];
                SectionGroupBox sectionGroup = new SectionGroupBox(ref sec);
                sectionGroup.Width = CentimeterToPixel((double)sec.SectionWidth / 1000) + 170;
                sectionGroup.Height = CentimeterToPixel((double)(sec.SectionHeight + 423) / 1000);
                sectionGroup.Location = new Point(posX, posY);
                sectionGroup.BackColor = Color.LightGray;
                sectionGroup.Text = sec.SectionType;
                formPanel.Controls.Add(sectionGroup);

                Panel sectionViewGroup = new Panel();
                sectionViewGroup.Width = CentimeterToPixel((double)sec.SectionWidth / 1000);
                sectionViewGroup.Height = CentimeterToPixel((double)(sec.SectionHeight) / 1000);
                sectionViewGroup.Location = new Point(0, CentimeterToPixel(0.423));
                sectionViewGroup.BackColor = Color.White;
                sectionGroup.Controls.Add(sectionViewGroup);

                List<SectionControl> controlsOnSec = sectionControls.FindAll(x => x.parentSectionId == sec.sectionId);
                if (!(controlsOnSec == null))
                {
                    foreach (SectionControl secCtrl in controlsOnSec)
                    {
                        Label sectionControlLabel = new Label();
                        sectionControlLabel.Size = new Size(CentimeterToPixel((double)secCtrl.controlWidth / 1000),
                            CentimeterToPixel((double)secCtrl.controlHeight / 1000));
                        sectionControlLabel.Location = new Point(CentimeterToPixel((double)secCtrl.controlXstart / 1000),
                            CentimeterToPixel((double)secCtrl.controlYstart / 1000));

                        sectionControlLabel.BackColor = Color.LightBlue;
                        sectionControlLabel.Text = secCtrl.sourceExpr;
                        //font of control
                        FontStyle fStyle;
                        if (secCtrl.fontBold)
                            fStyle = FontStyle.Bold;
                        else
                            fStyle = FontStyle.Regular;
                        sectionControlLabel.Font = new System.Drawing.Font(sectionControlLabel.Font.FontFamily, 7,fStyle);
                        sectionControlLabel.BorderStyle = BorderStyle.FixedSingle;

                        sectionViewGroup.Controls.Add(sectionControlLabel);
                    }
                }

                TableLayoutPanel tlp1 = new TableLayoutPanel();
                //tlp1.Dock = DockStyle.Fill;
                tlp1.RowCount = 1;
                tlp1.ColumnCount = 2;
                //tlp1.ColumnStyles = 50;
                tlp1.Location = new Point(CentimeterToPixel((double)sec.SectionWidth / 1000), 0);
                tlp1.AutoSize = true;
                tlp1.CellBorderStyle = TableLayoutPanelCellBorderStyle.OutsetDouble;

                CheckBox headerCheckBox = new CheckBox();
                headerCheckBox.AutoSize = true;
                headerCheckBox.Text = "Header";
                headerCheckBox.BackColor = Color.LightSlateGray;
                headerCheckBox.Checked = sec.isOnRDLCHeader;
                headerCheckBox.Click += new System.EventHandler(buttonHeader_Click);

                CheckBox footerCheckBox = new CheckBox();
                footerCheckBox.AutoSize = true;
                footerCheckBox.Text = "Footer";
                footerCheckBox.BackColor = Color.LightSlateGray;
                footerCheckBox.Checked = sec.isOnRDLCFooter;
                footerCheckBox.Click += new System.EventHandler(buttonFooter_Click);

                tlp1.Controls.Add(headerCheckBox, 0, 0);
                tlp1.Controls.Add(footerCheckBox, 1, 0);
                sectionGroup.Controls.Add(tlp1);

                posY += CentimeterToPixel((double)(sec.SectionHeight + 423) / 1000);
            }
        }

        /// <summary>
        /// http://stackoverflow.com/questions/4767617/centimeter-to-pixel
        /// needed to show labels in correct sizes
        /// </summary>
        /// <param name="Centimeter"></param>
        /// <returns></returns>
        int CentimeterToPixel(double Centimeter)
        {
            double pixel = -1;
            using (Graphics g = this.CreateGraphics())
            {
                pixel = Centimeter * g.DpiY / 2.54d;
            }
            return (int)pixel;
        }


        private void buttonHeader_Click(object sender, EventArgs e)
        {
            //set header value
            Section sec = ((SectionGroupBox)(((TableLayoutPanel)((CheckBox)sender).Parent).Parent)).sec;
            sec.isOnRDLCHeader = ((CheckBox)sender).Checked;
            sec.userChangeOnHeaderFooter = true;

            //call method to set header footer on connected sections
            repUpgFunc.updateSectionRDLCPropertiesOnHeaderChange(ref sectionsList, sec);

            //call method to refresh values on properties checkboxes
            refreshSectionProperties();
        }

        private void buttonFooter_Click(object sender, EventArgs e)
        {
            //set header value
            Section sec = ((SectionGroupBox)(((TableLayoutPanel)((CheckBox)sender).Parent).Parent)).sec;
            sec.isOnRDLCFooter = ((CheckBox)sender).Checked;
            sec.userChangeOnHeaderFooter = true;

            //call method to set header footer on connected sections
            repUpgFunc.updateSectionRDLCPropertiesOnFooterChange(ref sectionsList, sec);

            //call method to refresh values on properties checkboxes
            refreshSectionProperties();
        }


        private void refreshSectionProperties()
        {
            foreach (SectionGroupBox sgb in this.Controls[0].Controls)
            {
                ((CheckBox)sgb.Controls[1].Controls[0]).Checked = sgb.sec.isOnRDLCHeader;
                ((CheckBox)sgb.Controls[1].Controls[1]).Checked = sgb.sec.isOnRDLCFooter;
            }
        }
    }

    public partial class SectionGroupBox : GroupBox
    {
        public Section sec;

        public SectionGroupBox() { }

        public SectionGroupBox(ref Section sectoAssgn) 
        {
            sec = sectoAssgn;
        }
    }
}
