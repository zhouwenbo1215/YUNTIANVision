namespace YUNTIANVision
{
    partial class frmCameraExposeSet
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmCameraExposeSet));
            this.tbExpose = new System.Windows.Forms.TextBox();
            this.btSetExpose = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // tbExpose
            // 
            this.tbExpose.Font = new System.Drawing.Font("微软雅黑", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tbExpose.Location = new System.Drawing.Point(98, 20);
            this.tbExpose.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tbExpose.Name = "tbExpose";
            this.tbExpose.Size = new System.Drawing.Size(130, 25);
            this.tbExpose.TabIndex = 5;
            this.tbExpose.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // btSetExpose
            // 
            this.btSetExpose.Font = new System.Drawing.Font("微软雅黑", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btSetExpose.Location = new System.Drawing.Point(102, 65);
            this.btSetExpose.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btSetExpose.Name = "btSetExpose";
            this.btSetExpose.Size = new System.Drawing.Size(50, 26);
            this.btSetExpose.TabIndex = 7;
            this.btSetExpose.Text = "设置";
            this.btSetExpose.UseVisualStyleBackColor = true;
            this.btSetExpose.Click += new System.EventHandler(this.btSetExpose_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("微软雅黑", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(20, 22);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(68, 20);
            this.label1.TabIndex = 4;
            this.label1.Text = "曝光时间:";
            // 
            // frmCameraExposeSet
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(278, 120);
            this.Controls.Add(this.tbExpose);
            this.Controls.Add(this.btSetExpose);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmCameraExposeSet";
            this.Text = "相机曝光实时设置";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmCameraExposeSet_FormClosing);
            this.Load += new System.EventHandler(this.frmCameraExposeSet_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbExpose;
        private System.Windows.Forms.Button btSetExpose;
        private System.Windows.Forms.Label label1;
    }
}