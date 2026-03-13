namespace 对账平台
{
    partial class 下载支付宝账单
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.lblBillDate = new System.Windows.Forms.Label();
            this.dtpBillDate = new System.Windows.Forms.DateTimePicker();
            this.btnDownloadBill = new System.Windows.Forms.Button();
            this.dgvBillData = new System.Windows.Forms.DataGridView();
            this.lblBillType = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dgvBillData)).BeginInit();
            this.SuspendLayout();
            // 
            // lblBillDate
            // 
            this.lblBillDate.AutoSize = true;
            this.lblBillDate.Location = new System.Drawing.Point(30, 33);
            this.lblBillDate.Name = "lblBillDate";
            this.lblBillDate.Size = new System.Drawing.Size(65, 12);
            this.lblBillDate.TabIndex = 0;
            this.lblBillDate.Text = "账单日期：";
            // 
            // dtpBillDate
            // 
            this.dtpBillDate.CustomFormat = "yyyy-MM-dd";
            this.dtpBillDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpBillDate.Location = new System.Drawing.Point(89, 30);
            this.dtpBillDate.Name = "dtpBillDate";
            this.dtpBillDate.ShowUpDown = true;
            this.dtpBillDate.Size = new System.Drawing.Size(200, 21);
            this.dtpBillDate.TabIndex = 1;
            // 
            // btnDownloadBill
            // 
            this.btnDownloadBill.Location = new System.Drawing.Point(89, 70);
            this.btnDownloadBill.Name = "btnDownloadBill";
            this.btnDownloadBill.Size = new System.Drawing.Size(180, 30);
            this.btnDownloadBill.TabIndex = 2;
            this.btnDownloadBill.Text = "下载账单并直接入库";
            this.btnDownloadBill.UseVisualStyleBackColor = true;
            this.btnDownloadBill.Click += new System.EventHandler(this.btnDownloadBill_Click);
            // 
            // dgvBillData
            // 
            this.dgvBillData.AllowUserToAddRows = false;
            this.dgvBillData.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dgvBillData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvBillData.Location = new System.Drawing.Point(30, 150);
            this.dgvBillData.MultiSelect = false;
            this.dgvBillData.Name = "dgvBillData";
            this.dgvBillData.ReadOnly = true;
            this.dgvBillData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvBillData.Size = new System.Drawing.Size(700, 300);
            this.dgvBillData.TabIndex = 5;
            // 
            // lblBillType
            // 
            this.lblBillType.AutoSize = true;
            this.lblBillType.Location = new System.Drawing.Point(30, 10);
            this.lblBillType.Name = "lblBillType";
            this.lblBillType.Size = new System.Drawing.Size(113, 12);
            this.lblBillType.TabIndex = 6;
            this.lblBillType.Text = "账单类型：交易账单";
            // 
            // 下载支付宝账单
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(760, 480);
            this.Controls.Add(this.lblBillType);
            this.Controls.Add(this.dgvBillData);
            this.Controls.Add(this.btnDownloadBill);
            this.Controls.Add(this.dtpBillDate);
            this.Controls.Add(this.lblBillDate);
            this.Name = "下载支付宝账单";
            this.Text = "支付宝账单下载入库工具";
            ((System.ComponentModel.ISupportInitialize)(this.dgvBillData)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblBillDate;
        private System.Windows.Forms.DateTimePicker dtpBillDate;
        private System.Windows.Forms.Button btnDownloadBill;
        private System.Windows.Forms.DataGridView dgvBillData;
        private System.Windows.Forms.Label lblBillType;
    }
}