using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Collections.Generic;

namespace $safeprojectname$
{
    partial class ReportUpgradeForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.buttonImport = new System.Windows.Forms.Button();
            this.labelOriginal = new System.Windows.Forms.Label();
            this.textBoxOriginalText = new System.Windows.Forms.TextBox();
            this.buttonDecustruct = new System.Windows.Forms.Button();
            this.buttonExport = new System.Windows.Forms.Button();
            this.buttonTransform = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.labelTransformed = new System.Windows.Forms.Label();
            this.textBoxTransformed = new System.Windows.Forms.TextBox();
            this.buttonViewSections = new System.Windows.Forms.Button();
            this.labelReportStructure = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonImport
            // 
            this.buttonImport.Location = new System.Drawing.Point(11, 10);
            this.buttonImport.Margin = new System.Windows.Forms.Padding(2);
            this.buttonImport.Name = "buttonImport";
            this.buttonImport.Size = new System.Drawing.Size(85, 25);
            this.buttonImport.TabIndex = 0;
            this.buttonImport.Text = "Import file";
            this.buttonImport.UseVisualStyleBackColor = true;
            this.buttonImport.Click += new System.EventHandler(this.buttonImport_Click);
            // 
            // labelOriginal
            // 
            this.labelOriginal.AutoSize = true;
            this.labelOriginal.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelOriginal.Location = new System.Drawing.Point(2, 0);
            this.labelOriginal.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelOriginal.Name = "labelOriginal";
            this.labelOriginal.Size = new System.Drawing.Size(204, 16);
            this.labelOriginal.TabIndex = 1;
            this.labelOriginal.Text = "Original Text";
            // 
            // textBoxOriginalText
            // 
            this.textBoxOriginalText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxOriginalText.Location = new System.Drawing.Point(2, 18);
            this.textBoxOriginalText.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxOriginalText.Multiline = true;
            this.textBoxOriginalText.Name = "textBoxOriginalText";
            this.textBoxOriginalText.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxOriginalText.Size = new System.Drawing.Size(204, 297);
            this.textBoxOriginalText.TabIndex = 2;
            this.textBoxOriginalText.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox_KeyDown);
            // 
            // buttonDecustruct
            // 
            this.buttonDecustruct.Location = new System.Drawing.Point(100, 11);
            this.buttonDecustruct.Margin = new System.Windows.Forms.Padding(2);
            this.buttonDecustruct.Name = "buttonDecustruct";
            this.buttonDecustruct.Size = new System.Drawing.Size(85, 25);
            this.buttonDecustruct.TabIndex = 4;
            this.buttonDecustruct.Text = "Deconstruct";
            this.buttonDecustruct.UseVisualStyleBackColor = true;
            this.buttonDecustruct.Click += new System.EventHandler(this.buttonDecustruct_Click);
            // 
            // buttonExport
            // 
            this.buttonExport.Location = new System.Drawing.Point(367, 11);
            this.buttonExport.Margin = new System.Windows.Forms.Padding(2);
            this.buttonExport.Name = "buttonExport";
            this.buttonExport.Size = new System.Drawing.Size(85, 25);
            this.buttonExport.TabIndex = 5;
            this.buttonExport.Text = "Export file";
            this.buttonExport.UseVisualStyleBackColor = true;
            this.buttonExport.Click += new System.EventHandler(this.buttonExport_Click);
            // 
            // buttonTransform
            // 
            this.buttonTransform.Location = new System.Drawing.Point(189, 11);
            this.buttonTransform.Margin = new System.Windows.Forms.Padding(2);
            this.buttonTransform.Name = "buttonTransform";
            this.buttonTransform.Size = new System.Drawing.Size(85, 25);
            this.buttonTransform.TabIndex = 7;
            this.buttonTransform.Text = "Transform";
            this.buttonTransform.UseVisualStyleBackColor = true;
            this.buttonTransform.Click += new System.EventHandler(this.buttonTransform_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 38F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 38F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 24F));
            this.tableLayoutPanel1.Controls.Add(this.textBoxOriginalText, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.listBox1, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.labelOriginal, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.labelTransformed, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.textBoxTransformed, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.labelReportStructure, 2, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(11, 53);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(2);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 16F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(548, 317);
            this.tableLayoutPanel1.TabIndex = 8;
            // 
            // listBox1
            // 
            this.listBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBox1.FormattingEnabled = true;
            this.listBox1.Location = new System.Drawing.Point(418, 18);
            this.listBox1.Margin = new System.Windows.Forms.Padding(2);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(128, 297);
            this.listBox1.TabIndex = 9;
            this.listBox1.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
            // 
            // labelTransformed
            // 
            this.labelTransformed.AutoSize = true;
            this.labelTransformed.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelTransformed.Location = new System.Drawing.Point(210, 0);
            this.labelTransformed.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelTransformed.Name = "labelTransformed";
            this.labelTransformed.Size = new System.Drawing.Size(204, 16);
            this.labelTransformed.TabIndex = 9;
            this.labelTransformed.Text = "Transformed Text";
            // 
            // textBoxTransformed
            // 
            this.textBoxTransformed.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxTransformed.Location = new System.Drawing.Point(210, 18);
            this.textBoxTransformed.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxTransformed.Multiline = true;
            this.textBoxTransformed.Name = "textBoxTransformed";
            this.textBoxTransformed.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxTransformed.Size = new System.Drawing.Size(204, 297);
            this.textBoxTransformed.TabIndex = 10;
            this.textBoxTransformed.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox_KeyDown);
            // 
            // buttonViewSections
            // 
            this.buttonViewSections.Location = new System.Drawing.Point(278, 11);
            this.buttonViewSections.Margin = new System.Windows.Forms.Padding(2);
            this.buttonViewSections.Name = "buttonViewSections";
            this.buttonViewSections.Size = new System.Drawing.Size(85, 25);
            this.buttonViewSections.TabIndex = 9;
            this.buttonViewSections.Text = "View sections";
            this.buttonViewSections.UseVisualStyleBackColor = true;
            this.buttonViewSections.Click += new System.EventHandler(this.viewSections_Click);
            // 
            // labelReportStructure
            // 
            this.labelReportStructure.AutoSize = true;
            this.labelReportStructure.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelReportStructure.Location = new System.Drawing.Point(419, 0);
            this.labelReportStructure.Name = "labelReportStructure";
            this.labelReportStructure.Size = new System.Drawing.Size(126, 16);
            this.labelReportStructure.TabIndex = 11;
            this.labelReportStructure.Text = "Report Structure";
            // 
            // ReportUpgradeForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(568, 380);
            this.Controls.Add(this.buttonViewSections);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.buttonTransform);
            this.Controls.Add(this.buttonExport);
            this.Controls.Add(this.buttonDecustruct);
            this.Controls.Add(this.buttonImport);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "ReportUpgradeForm";
            this.Text = "Classic To RDLC Report Upgrade";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonImport;
        private System.Windows.Forms.Label labelOriginal;
        private System.Windows.Forms.TextBox textBoxOriginalText;
        private System.Windows.Forms.Button buttonDecustruct;
        private Button buttonExport;
        private Button buttonTransform;
        private TableLayoutPanel tableLayoutPanel1;
        private ListBox listBox1;
        private Label labelTransformed;
        private TextBox textBoxTransformed;
        private Button buttonViewSections;
        private Label labelReportStructure;
    }
}

;