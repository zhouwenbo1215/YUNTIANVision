namespace YUNTIANVision
{
    partial class frmOneTable
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btOKToNG = new System.Windows.Forms.Button();
            this.tbAddOK = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btDelOK = new System.Windows.Forms.Button();
            this.cbOKItem = new System.Windows.Forms.ComboBox();
            this.btAddOK = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btNGToOK = new System.Windows.Forms.Button();
            this.tbAddNG = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btAddNG = new System.Windows.Forms.Button();
            this.btDelNG = new System.Windows.Forms.Button();
            this.cbNGItem = new System.Windows.Forms.ComboBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.button1 = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.tabPage2.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.tabControl1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(6);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 90F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(628, 800);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tabControl1.Location = new System.Drawing.Point(4, 4);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(4);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(620, 712);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.dataGridView1);
            this.tabPage1.Location = new System.Drawing.Point(8, 45);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(4);
            this.tabPage1.Size = new System.Drawing.Size(604, 659);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "结果显示";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // dataGridView1
            // 
            this.dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridView1.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(4, 4);
            this.dataGridView1.Margin = new System.Windows.Forms.Padding(4);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersWidth = 82;
            this.dataGridView1.RowTemplate.Height = 37;
            this.dataGridView1.Size = new System.Drawing.Size(596, 651);
            this.dataGridView1.TabIndex = 0;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.tableLayoutPanel3);
            this.tabPage2.Location = new System.Drawing.Point(8, 45);
            this.tabPage2.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(4);
            this.tabPage2.Size = new System.Drawing.Size(604, 659);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "结果类型设置";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 1;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.Controls.Add(this.groupBox1, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.groupBox2, 0, 1);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(4, 4);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 2;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(596, 651);
            this.tableLayoutPanel3.TabIndex = 4;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btOKToNG);
            this.groupBox1.Controls.Add(this.tbAddOK);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.btDelOK);
            this.groupBox1.Controls.Add(this.cbOKItem);
            this.groupBox1.Controls.Add(this.btAddOK);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(590, 319);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "OK类型设置";
            // 
            // btOKToNG
            // 
            this.btOKToNG.Location = new System.Drawing.Point(316, 33);
            this.btOKToNG.Name = "btOKToNG";
            this.btOKToNG.Size = new System.Drawing.Size(193, 60);
            this.btOKToNG.TabIndex = 4;
            this.btOKToNG.Text = "转为NG类型";
            this.btOKToNG.UseVisualStyleBackColor = true;
            this.btOKToNG.Click += new System.EventHandler(this.button2_Click_1);
            // 
            // tbAddOK
            // 
            this.tbAddOK.Location = new System.Drawing.Point(59, 128);
            this.tbAddOK.Margin = new System.Windows.Forms.Padding(4);
            this.tbAddOK.Name = "tbAddOK";
            this.tbAddOK.Size = new System.Drawing.Size(148, 39);
            this.tbAddOK.TabIndex = 3;
            this.tbAddOK.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(43, 48);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(121, 31);
            this.label1.TabIndex = 0;
            this.label1.Text = "OK类型：";
            // 
            // btDelOK
            // 
            this.btDelOK.BackColor = System.Drawing.Color.Red;
            this.btDelOK.Location = new System.Drawing.Point(215, 214);
            this.btDelOK.Margin = new System.Windows.Forms.Padding(4);
            this.btDelOK.Name = "btDelOK";
            this.btDelOK.Size = new System.Drawing.Size(106, 52);
            this.btDelOK.TabIndex = 2;
            this.btDelOK.Text = "删除";
            this.btDelOK.UseVisualStyleBackColor = false;
            this.btDelOK.Click += new System.EventHandler(this.button3_Click);
            // 
            // cbOKItem
            // 
            this.cbOKItem.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbOKItem.FormattingEnabled = true;
            this.cbOKItem.Location = new System.Drawing.Point(169, 48);
            this.cbOKItem.Margin = new System.Windows.Forms.Padding(4);
            this.cbOKItem.Name = "cbOKItem";
            this.cbOKItem.Size = new System.Drawing.Size(120, 39);
            this.cbOKItem.TabIndex = 1;
            // 
            // btAddOK
            // 
            this.btAddOK.BackColor = System.Drawing.Color.Lime;
            this.btAddOK.Location = new System.Drawing.Point(59, 214);
            this.btAddOK.Margin = new System.Windows.Forms.Padding(4);
            this.btAddOK.Name = "btAddOK";
            this.btAddOK.Size = new System.Drawing.Size(106, 52);
            this.btAddOK.TabIndex = 2;
            this.btAddOK.Text = "添加";
            this.btAddOK.UseVisualStyleBackColor = false;
            this.btAddOK.Click += new System.EventHandler(this.button2_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btNGToOK);
            this.groupBox2.Controls.Add(this.tbAddNG);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.btAddNG);
            this.groupBox2.Controls.Add(this.btDelNG);
            this.groupBox2.Controls.Add(this.cbNGItem);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(3, 328);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(590, 320);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "NG类型设置";
            // 
            // btNGToOK
            // 
            this.btNGToOK.Location = new System.Drawing.Point(316, 47);
            this.btNGToOK.Name = "btNGToOK";
            this.btNGToOK.Size = new System.Drawing.Size(193, 60);
            this.btNGToOK.TabIndex = 4;
            this.btNGToOK.Text = "转为OK类型";
            this.btNGToOK.UseVisualStyleBackColor = true;
            this.btNGToOK.Click += new System.EventHandler(this.button3_Click_1);
            // 
            // tbAddNG
            // 
            this.tbAddNG.Location = new System.Drawing.Point(59, 138);
            this.tbAddNG.Margin = new System.Windows.Forms.Padding(4);
            this.tbAddNG.Name = "tbAddNG";
            this.tbAddNG.Size = new System.Drawing.Size(148, 39);
            this.tbAddNG.TabIndex = 3;
            this.tbAddNG.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(43, 58);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(124, 31);
            this.label2.TabIndex = 0;
            this.label2.Text = "NG类型：";
            // 
            // btAddNG
            // 
            this.btAddNG.BackColor = System.Drawing.Color.Lime;
            this.btAddNG.Location = new System.Drawing.Point(59, 224);
            this.btAddNG.Margin = new System.Windows.Forms.Padding(4);
            this.btAddNG.Name = "btAddNG";
            this.btAddNG.Size = new System.Drawing.Size(106, 52);
            this.btAddNG.TabIndex = 2;
            this.btAddNG.Text = "添加";
            this.btAddNG.UseVisualStyleBackColor = false;
            this.btAddNG.Click += new System.EventHandler(this.button4_Click);
            // 
            // btDelNG
            // 
            this.btDelNG.BackColor = System.Drawing.Color.Red;
            this.btDelNG.Location = new System.Drawing.Point(215, 224);
            this.btDelNG.Margin = new System.Windows.Forms.Padding(4);
            this.btDelNG.Name = "btDelNG";
            this.btDelNG.Size = new System.Drawing.Size(106, 52);
            this.btDelNG.TabIndex = 2;
            this.btDelNG.Text = "删除";
            this.btDelNG.UseVisualStyleBackColor = false;
            this.btDelNG.Click += new System.EventHandler(this.button5_Click);
            // 
            // cbNGItem
            // 
            this.cbNGItem.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbNGItem.FormattingEnabled = true;
            this.cbNGItem.Location = new System.Drawing.Point(171, 58);
            this.cbNGItem.Margin = new System.Windows.Forms.Padding(4);
            this.cbNGItem.Name = "cbNGItem";
            this.cbNGItem.Size = new System.Drawing.Size(120, 39);
            this.cbNGItem.TabIndex = 1;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 75F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel2.Controls.Add(this.progressBar1, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.button1, 1, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(6, 726);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(6);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(616, 68);
            this.tableLayoutPanel2.TabIndex = 1;
            // 
            // progressBar1
            // 
            this.progressBar1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.progressBar1.Location = new System.Drawing.Point(6, 6);
            this.progressBar1.Margin = new System.Windows.Forms.Padding(6);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(450, 56);
            this.progressBar1.Step = 1;
            this.progressBar1.TabIndex = 0;
            // 
            // button1
            // 
            this.button1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.button1.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button1.Location = new System.Drawing.Point(468, 6);
            this.button1.Margin = new System.Windows.Forms.Padding(6);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(142, 56);
            this.button1.TabIndex = 1;
            this.button1.Text = "加载方案";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // frmOneTable
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(628, 800);
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "frmOneTable";
            this.Text = "frmOneTable";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmOneTable_FormClosed);
            this.Load += new System.EventHandler(this.frmOneTable_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.tabPage2.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Button btDelOK;
        private System.Windows.Forms.Button btAddOK;
        private System.Windows.Forms.ComboBox cbOKItem;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbAddOK;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox tbAddNG;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btAddNG;
        private System.Windows.Forms.Button btDelNG;
        private System.Windows.Forms.ComboBox cbNGItem;
        private System.Windows.Forms.Button btOKToNG;
        private System.Windows.Forms.Button btNGToOK;
    }
}