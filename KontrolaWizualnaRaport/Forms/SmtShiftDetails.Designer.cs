namespace KontrolaWizualnaRaport
{
    partial class SmtShiftDetails
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.dataGridViewModelSummary = new System.Windows.Forms.DataGridView();
            this.dataGridViewShiftDetails = new System.Windows.Forms.DataGridView();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.dataGridViewLinesSummary = new System.Windows.Forms.DataGridView();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewModelSummary)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewShiftDetails)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewLinesSummary)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1079, 61);
            this.panel1.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label1.Location = new System.Drawing.Point(25, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(57, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "Nazwa";
            // 
            // dataGridViewModelSummary
            // 
            this.dataGridViewModelSummary.AllowUserToAddRows = false;
            this.dataGridViewModelSummary.AllowUserToDeleteRows = false;
            this.dataGridViewModelSummary.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewModelSummary.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewModelSummary.Location = new System.Drawing.Point(3, 3);
            this.dataGridViewModelSummary.Name = "dataGridViewModelSummary";
            this.dataGridViewModelSummary.Size = new System.Drawing.Size(194, 324);
            this.dataGridViewModelSummary.TabIndex = 1;
            // 
            // dataGridViewShiftDetails
            // 
            this.dataGridViewShiftDetails.AllowUserToAddRows = false;
            this.dataGridViewShiftDetails.AllowUserToDeleteRows = false;
            this.dataGridViewShiftDetails.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewShiftDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewShiftDetails.Location = new System.Drawing.Point(0, 61);
            this.dataGridViewShiftDetails.Name = "dataGridViewShiftDetails";
            this.dataGridViewShiftDetails.Size = new System.Drawing.Size(879, 660);
            this.dataGridViewShiftDetails.TabIndex = 2;
            this.dataGridViewShiftDetails.RowPostPaint += new System.Windows.Forms.DataGridViewRowPostPaintEventHandler(this.dataGridViewShiftDetails_RowPostPaint);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Controls.Add(this.dataGridViewLinesSummary, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.dataGridViewModelSummary, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Right;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(879, 61);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(200, 660);
            this.tableLayoutPanel1.TabIndex = 3;
            // 
            // dataGridViewLinesSummary
            // 
            this.dataGridViewLinesSummary.AllowUserToAddRows = false;
            this.dataGridViewLinesSummary.AllowUserToDeleteRows = false;
            this.dataGridViewLinesSummary.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewLinesSummary.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewLinesSummary.Location = new System.Drawing.Point(3, 333);
            this.dataGridViewLinesSummary.Name = "dataGridViewLinesSummary";
            this.dataGridViewLinesSummary.Size = new System.Drawing.Size(194, 324);
            this.dataGridViewLinesSummary.TabIndex = 4;
            // 
            // SmtShiftDetails
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1079, 721);
            this.Controls.Add(this.dataGridViewShiftDetails);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.panel1);
            this.Name = "SmtShiftDetails";
            this.Text = "SmtShiftDetails";
            this.Load += new System.EventHandler(this.SmtShiftDetails_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewModelSummary)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewShiftDetails)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewLinesSummary)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.DataGridView dataGridViewModelSummary;
        private System.Windows.Forms.DataGridView dataGridViewShiftDetails;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.DataGridView dataGridViewLinesSummary;
    }
}