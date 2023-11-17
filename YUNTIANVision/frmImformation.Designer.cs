namespace YUNTIANVision
{
    partial class frmImformation
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmImformation));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.lbResult = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.清除数据ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lbCt = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lbOKPercent = new System.Windows.Forms.Label();
            this.lbNGNum = new System.Windows.Forms.Label();
            this.lbOKNum = new System.Windows.Forms.Label();
            this.lbAllNum = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(49, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(91, 42);
            this.label1.TabIndex = 0;
            this.label1.Text = "总数:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(49, 80);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(107, 42);
            this.label2.TabIndex = 0;
            this.label2.Text = "OK数:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label4.Location = new System.Drawing.Point(49, 149);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(110, 42);
            this.label4.TabIndex = 0;
            this.label4.Text = "NG数:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label5.Location = new System.Drawing.Point(49, 215);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(91, 42);
            this.label5.TabIndex = 0;
            this.label5.Text = "良率:";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.tableLayoutPanel1);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(711, 482);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "结果";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.lbResult, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.pictureBox1, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 35);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 75F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(705, 444);
            this.tableLayoutPanel1.TabIndex = 3;
            // 
            // lbResult
            // 
            this.lbResult.AutoSize = true;
            this.lbResult.BackColor = System.Drawing.Color.Lime;
            this.lbResult.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbResult.Font = new System.Drawing.Font("微软雅黑", 16.125F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbResult.Location = new System.Drawing.Point(3, 333);
            this.lbResult.Name = "lbResult";
            this.lbResult.Size = new System.Drawing.Size(346, 111);
            this.lbResult.TabIndex = 0;
            this.lbResult.Text = "OK";
            this.lbResult.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panel1
            // 
            this.panel1.ContextMenuStrip = this.contextMenuStrip1;
            this.panel1.Controls.Add(this.lbCt);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.lbOKPercent);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.lbNGNum);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.lbOKNum);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.lbAllNum);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(355, 3);
            this.panel1.Name = "panel1";
            this.tableLayoutPanel1.SetRowSpan(this.panel1, 2);
            this.panel1.Size = new System.Drawing.Size(347, 438);
            this.panel1.TabIndex = 0;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.清除数据ToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(185, 42);
            // 
            // 清除数据ToolStripMenuItem
            // 
            this.清除数据ToolStripMenuItem.Name = "清除数据ToolStripMenuItem";
            this.清除数据ToolStripMenuItem.Size = new System.Drawing.Size(184, 38);
            this.清除数据ToolStripMenuItem.Text = "清除数据";
            this.清除数据ToolStripMenuItem.Click += new System.EventHandler(this.清除数据ToolStripMenuItem_Click);
            // 
            // lbCt
            // 
            this.lbCt.AutoSize = true;
            this.lbCt.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbCt.Location = new System.Drawing.Point(243, 285);
            this.lbCt.Name = "lbCt";
            this.lbCt.Size = new System.Drawing.Size(38, 42);
            this.lbCt.TabIndex = 2;
            this.lbCt.Text = "0";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label3.Location = new System.Drawing.Point(49, 285);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(162, 42);
            this.label3.TabIndex = 0;
            this.label3.Text = "耗时(ms):";
            // 
            // lbOKPercent
            // 
            this.lbOKPercent.AutoSize = true;
            this.lbOKPercent.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbOKPercent.Location = new System.Drawing.Point(176, 215);
            this.lbOKPercent.Name = "lbOKPercent";
            this.lbOKPercent.Size = new System.Drawing.Size(38, 42);
            this.lbOKPercent.TabIndex = 2;
            this.lbOKPercent.Text = "0";
            // 
            // lbNGNum
            // 
            this.lbNGNum.AutoSize = true;
            this.lbNGNum.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbNGNum.Location = new System.Drawing.Point(176, 149);
            this.lbNGNum.Name = "lbNGNum";
            this.lbNGNum.Size = new System.Drawing.Size(38, 42);
            this.lbNGNum.TabIndex = 2;
            this.lbNGNum.Text = "0";
            // 
            // lbOKNum
            // 
            this.lbOKNum.AutoSize = true;
            this.lbOKNum.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbOKNum.Location = new System.Drawing.Point(176, 80);
            this.lbOKNum.Name = "lbOKNum";
            this.lbOKNum.Size = new System.Drawing.Size(38, 42);
            this.lbOKNum.TabIndex = 2;
            this.lbOKNum.Text = "0";
            // 
            // lbAllNum
            // 
            this.lbAllNum.AutoSize = true;
            this.lbAllNum.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbAllNum.Location = new System.Drawing.Point(176, 19);
            this.lbAllNum.Name = "lbAllNum";
            this.lbAllNum.Size = new System.Drawing.Size(38, 42);
            this.lbAllNum.TabIndex = 2;
            this.lbAllNum.Text = "0";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Image = global::YUNTIANVision.Properties.Resources.云田视觉;
            this.pictureBox1.Location = new System.Drawing.Point(3, 3);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(346, 327);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 2;
            this.pictureBox1.TabStop = false;
            // 
            // frmImformation
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(711, 482);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "frmImformation";
            this.Text = "frmOneImformation";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmImformation_FormClosed);
            this.Load += new System.EventHandler(this.frmImformation_Load);
            this.groupBox1.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.contextMenuStrip1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label lbCt;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lbOKPercent;
        private System.Windows.Forms.Label lbNGNum;
        private System.Windows.Forms.Label lbOKNum;
        private System.Windows.Forms.Label lbAllNum;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label lbResult;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem 清除数据ToolStripMenuItem;
    }
}