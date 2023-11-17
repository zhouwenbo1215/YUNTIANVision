namespace YUNTIANVision
{
    partial class ModelCameraSet
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.dataGridModel = new System.Windows.Forms.DataGridView();
            this.dataGridCamera = new System.Windows.Forms.DataGridView();
            this.dataGridTotal = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.pbox_Icon)).BeginInit();
            this.titlepanel.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridModel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridCamera)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridTotal)).BeginInit();
            this.SuspendLayout();
            // 
            // titlepanel
            // 
            this.titlepanel.Size = new System.Drawing.Size(848, 40);
            // 
            // titlelabel
            // 
            this.titlelabel.Size = new System.Drawing.Size(152, 27);
            this.titlelabel.Text = "模型相机设置页";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.dataGridTotal, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 40);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(848, 574);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Controls.Add(this.dataGridModel, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.dataGridCamera, 1, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(842, 281);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // dataGridModel
            // 
            this.dataGridModel.AllowUserToOrderColumns = true;
            this.dataGridModel.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridModel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridModel.Location = new System.Drawing.Point(3, 3);
            this.dataGridModel.Name = "dataGridModel";
            this.dataGridModel.RowHeadersWidth = 51;
            this.dataGridModel.RowTemplate.Height = 27;
            this.dataGridModel.Size = new System.Drawing.Size(415, 275);
            this.dataGridModel.TabIndex = 0;
            // 
            // dataGridCamera
            // 
            this.dataGridCamera.AllowUserToOrderColumns = true;
            this.dataGridCamera.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridCamera.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridCamera.Location = new System.Drawing.Point(424, 3);
            this.dataGridCamera.Name = "dataGridCamera";
            this.dataGridCamera.RowHeadersWidth = 51;
            this.dataGridCamera.RowTemplate.Height = 27;
            this.dataGridCamera.Size = new System.Drawing.Size(415, 275);
            this.dataGridCamera.TabIndex = 1;
            // 
            // dataGridTotal
            // 
            this.dataGridTotal.AllowUserToOrderColumns = true;
            this.dataGridTotal.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridTotal.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridTotal.Location = new System.Drawing.Point(3, 290);
            this.dataGridTotal.Name = "dataGridTotal";
            this.dataGridTotal.RowHeadersWidth = 51;
            this.dataGridTotal.RowTemplate.Height = 27;
            this.dataGridTotal.Size = new System.Drawing.Size(842, 281);
            this.dataGridTotal.TabIndex = 1;
            // 
            // ModelCameraSet
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(848, 614);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "ModelCameraSet";
            this.Title = "模型相机设置页";
            this.TitleSize = new System.Drawing.Size(152, 27);
            this.Controls.SetChildIndex(this.titlepanel, 0);
            this.Controls.SetChildIndex(this.tableLayoutPanel1, 0);
            ((System.ComponentModel.ISupportInitialize)(this.pbox_Icon)).EndInit();
            this.titlepanel.ResumeLayout(false);
            this.titlepanel.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridModel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridCamera)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridTotal)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.DataGridView dataGridModel;
        private System.Windows.Forms.DataGridView dataGridCamera;
        private System.Windows.Forms.DataGridView dataGridTotal;
    }
}