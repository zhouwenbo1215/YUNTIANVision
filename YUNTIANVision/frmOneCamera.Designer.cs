namespace YUNTIANVision
{
    partial class frmOneCamera
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmOneCamera));
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.读取图片ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.读取图片目录ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.tbLog = new System.Windows.Forms.TextBox();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.hWindowControl1 = new HalconDotNet.HSmartWindowControl();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cbGrayOrRgb = new System.Windows.Forms.ComboBox();
            this.label12 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.btAddNGSignal = new System.Windows.Forms.Button();
            this.tbReadySignal = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.tbOKSignal = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.tbGrabImageSignal = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btClosePlc = new System.Windows.Forms.Button();
            this.btOpenPlc = new System.Windows.Forms.Button();
            this.tbStopBits = new System.Windows.Forms.TextBox();
            this.cbPortName = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.tbBaudRate = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cbParity = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tbDateBits = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.tbCodeResult = new System.Windows.Forms.TextBox();
            this.label15 = new System.Windows.Forms.Label();
            this.tbCodeLength = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.tbCodeAddress = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.cbEnableCode = new System.Windows.Forms.CheckBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel6 = new System.Windows.Forms.TableLayoutPanel();
            this.tabControl2 = new System.Windows.Forms.TabControl();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.lbResult = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.btClearData = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.lbCt = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.lbOKNum = new System.Windows.Forms.Label();
            this.lbOKPercent = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.lbNGNum = new System.Windows.Forms.Label();
            this.lbAllNum = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.btHandTest = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.contextMenuStrip1.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.panel1.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.tableLayoutPanel6.SuspendLayout();
            this.tabControl2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.groupBox6.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.panel2.SuspendLayout();
            this.tableLayoutPanel5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.读取图片ToolStripMenuItem,
            this.toolStripMenuItem1,
            this.读取图片目录ToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(149, 70);
            // 
            // 读取图片ToolStripMenuItem
            // 
            this.读取图片ToolStripMenuItem.Name = "读取图片ToolStripMenuItem";
            this.读取图片ToolStripMenuItem.Size = new System.Drawing.Size(148, 22);
            this.读取图片ToolStripMenuItem.Text = "读取图片";
            this.读取图片ToolStripMenuItem.Click += new System.EventHandler(this.读取图片ToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(148, 22);
            this.toolStripMenuItem1.Text = "实时设置曝光";
            this.toolStripMenuItem1.Click += new System.EventHandler(this.toolStripMenuItem1_Click);
            // 
            // 读取图片目录ToolStripMenuItem
            // 
            this.读取图片目录ToolStripMenuItem.Name = "读取图片目录ToolStripMenuItem";
            this.读取图片目录ToolStripMenuItem.Size = new System.Drawing.Size(148, 22);
            this.读取图片目录ToolStripMenuItem.Text = "读取图片目录";
            this.读取图片目录ToolStripMenuItem.Click += new System.EventHandler(this.读取图片目录ToolStripMenuItem_Click);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.tbLog);
            this.groupBox4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox4.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBox4.Location = new System.Drawing.Point(2, 469);
            this.groupBox4.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox4.Size = new System.Drawing.Size(675, 79);
            this.groupBox4.TabIndex = 1;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Log显示";
            // 
            // tbLog
            // 
            this.tbLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbLog.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tbLog.Location = new System.Drawing.Point(2, 18);
            this.tbLog.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tbLog.Multiline = true;
            this.tbLog.Name = "tbLog";
            this.tbLog.ReadOnly = true;
            this.tbLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbLog.Size = new System.Drawing.Size(671, 59);
            this.tbLog.TabIndex = 2;
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel2.Controls.Add(this.tabControl1, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.groupBox4, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.panel1, 1, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 85F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 10F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(970, 550);
            this.tableLayoutPanel2.TabIndex = 2;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tabControl1.Location = new System.Drawing.Point(2, 2);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(675, 463);
            this.tabControl1.TabIndex = 2;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.hWindowControl1);
            this.tabPage1.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tabPage1.Location = new System.Drawing.Point(4, 26);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage1.Size = new System.Drawing.Size(667, 433);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "图像显示";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // hWindowControl1
            // 
            this.hWindowControl1.AllowDrop = true;
            this.hWindowControl1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.hWindowControl1.AutoValidate = System.Windows.Forms.AutoValidate.EnableAllowFocusChange;
            this.hWindowControl1.ContextMenuStrip = this.contextMenuStrip1;
            this.hWindowControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.hWindowControl1.HDoubleClickToFitContent = true;
            this.hWindowControl1.HDrawingObjectsModifier = HalconDotNet.HSmartWindowControl.DrawingObjectsModifier.None;
            this.hWindowControl1.HImagePart = new System.Drawing.Rectangle(0, 0, 640, 480);
            this.hWindowControl1.HKeepAspectRatio = true;
            this.hWindowControl1.HMoveContent = true;
            this.hWindowControl1.HZoomContent = HalconDotNet.HSmartWindowControl.ZoomContent.WheelForwardZoomsIn;
            this.hWindowControl1.Location = new System.Drawing.Point(2, 2);
            this.hWindowControl1.Margin = new System.Windows.Forms.Padding(0);
            this.hWindowControl1.Name = "hWindowControl1";
            this.hWindowControl1.Size = new System.Drawing.Size(663, 429);
            this.hWindowControl1.TabIndex = 0;
            this.hWindowControl1.WindowSize = new System.Drawing.Size(663, 429);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.tableLayoutPanel1);
            this.tabPage2.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tabPage2.Location = new System.Drawing.Point(4, 26);
            this.tabPage2.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage2.Size = new System.Drawing.Size(667, 433);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "相关设置";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.groupBox1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.groupBox3, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.groupBox2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.groupBox5, 0, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(2, 2);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 5;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(663, 429);
            this.tableLayoutPanel1.TabIndex = 3;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cbGrayOrRgb);
            this.groupBox1.Controls.Add(this.label12);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(2, 2);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox1.Size = new System.Drawing.Size(659, 38);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "相关设置:";
            // 
            // cbGrayOrRgb
            // 
            this.cbGrayOrRgb.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbGrayOrRgb.FormattingEnabled = true;
            this.cbGrayOrRgb.Location = new System.Drawing.Point(116, 20);
            this.cbGrayOrRgb.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.cbGrayOrRgb.Name = "cbGrayOrRgb";
            this.cbGrayOrRgb.Size = new System.Drawing.Size(50, 25);
            this.cbGrayOrRgb.TabIndex = 4;
            this.cbGrayOrRgb.SelectedIndexChanged += new System.EventHandler(this.cbGrayOrRgb_SelectedIndexChanged);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("微软雅黑", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label12.Location = new System.Drawing.Point(14, 21);
            this.label12.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(96, 20);
            this.label12.TabIndex = 0;
            this.label12.Text = "图片采集设置:";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.btAddNGSignal);
            this.groupBox3.Controls.Add(this.tbReadySignal);
            this.groupBox3.Controls.Add(this.label10);
            this.groupBox3.Controls.Add(this.label9);
            this.groupBox3.Controls.Add(this.tbOKSignal);
            this.groupBox3.Controls.Add(this.label8);
            this.groupBox3.Controls.Add(this.tbGrabImageSignal);
            this.groupBox3.Controls.Add(this.label7);
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox3.Location = new System.Drawing.Point(2, 128);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox3.Size = new System.Drawing.Size(659, 38);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "PLC地址设置";
            // 
            // btAddNGSignal
            // 
            this.btAddNGSignal.Font = new System.Drawing.Font("微软雅黑", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btAddNGSignal.Location = new System.Drawing.Point(604, 22);
            this.btAddNGSignal.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btAddNGSignal.Name = "btAddNGSignal";
            this.btAddNGSignal.Size = new System.Drawing.Size(23, 23);
            this.btAddNGSignal.TabIndex = 13;
            this.btAddNGSignal.Text = "+";
            this.btAddNGSignal.UseVisualStyleBackColor = true;
            this.btAddNGSignal.Click += new System.EventHandler(this.button1_Click);
            // 
            // tbReadySignal
            // 
            this.tbReadySignal.Font = new System.Drawing.Font("微软雅黑", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tbReadySignal.Location = new System.Drawing.Point(51, 22);
            this.tbReadySignal.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tbReadySignal.Name = "tbReadySignal";
            this.tbReadySignal.Size = new System.Drawing.Size(59, 25);
            this.tbReadySignal.TabIndex = 10;
            this.tbReadySignal.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("微软雅黑", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label10.Location = new System.Drawing.Point(10, 24);
            this.label10.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(40, 20);
            this.label10.TabIndex = 14;
            this.label10.Text = "准备:";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("微软雅黑", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label9.Location = new System.Drawing.Point(508, 24);
            this.label9.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(89, 20);
            this.label9.TabIndex = 14;
            this.label9.Text = "NG信号设置:";
            // 
            // tbOKSignal
            // 
            this.tbOKSignal.Font = new System.Drawing.Font("微软雅黑", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tbOKSignal.Location = new System.Drawing.Point(393, 24);
            this.tbOKSignal.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tbOKSignal.Name = "tbOKSignal";
            this.tbOKSignal.Size = new System.Drawing.Size(59, 25);
            this.tbOKSignal.TabIndex = 12;
            this.tbOKSignal.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("微软雅黑", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label8.Location = new System.Drawing.Point(348, 26);
            this.label8.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(32, 20);
            this.label8.TabIndex = 14;
            this.label8.Text = "OK:";
            // 
            // tbGrabImageSignal
            // 
            this.tbGrabImageSignal.Font = new System.Drawing.Font("微软雅黑", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tbGrabImageSignal.Location = new System.Drawing.Point(213, 22);
            this.tbGrabImageSignal.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tbGrabImageSignal.Name = "tbGrabImageSignal";
            this.tbGrabImageSignal.Size = new System.Drawing.Size(59, 25);
            this.tbGrabImageSignal.TabIndex = 11;
            this.tbGrabImageSignal.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("微软雅黑", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label7.Location = new System.Drawing.Point(168, 24);
            this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(40, 20);
            this.label7.TabIndex = 14;
            this.label7.Text = "相机:";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btClosePlc);
            this.groupBox2.Controls.Add(this.btOpenPlc);
            this.groupBox2.Controls.Add(this.tbStopBits);
            this.groupBox2.Controls.Add(this.cbPortName);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.tbBaudRate);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.cbParity);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.tbDateBits);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(2, 44);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox2.Size = new System.Drawing.Size(659, 38);
            this.groupBox2.TabIndex = 4;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "PLC设置";
            // 
            // btClosePlc
            // 
            this.btClosePlc.BackColor = System.Drawing.Color.Transparent;
            this.btClosePlc.Font = new System.Drawing.Font("微软雅黑", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btClosePlc.Location = new System.Drawing.Point(889, 16);
            this.btClosePlc.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btClosePlc.Name = "btClosePlc";
            this.btClosePlc.Size = new System.Drawing.Size(75, 29);
            this.btClosePlc.TabIndex = 9;
            this.btClosePlc.Text = "关闭串口";
            this.btClosePlc.UseVisualStyleBackColor = false;
            this.btClosePlc.Click += new System.EventHandler(this.btClosePlc_Click_1);
            // 
            // btOpenPlc
            // 
            this.btOpenPlc.BackColor = System.Drawing.Color.Transparent;
            this.btOpenPlc.Font = new System.Drawing.Font("微软雅黑", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btOpenPlc.Location = new System.Drawing.Point(812, 16);
            this.btOpenPlc.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btOpenPlc.Name = "btOpenPlc";
            this.btOpenPlc.Size = new System.Drawing.Size(75, 29);
            this.btOpenPlc.TabIndex = 9;
            this.btOpenPlc.Text = "打开串口";
            this.btOpenPlc.UseVisualStyleBackColor = false;
            this.btOpenPlc.Click += new System.EventHandler(this.btOpenPlc_Click_1);
            // 
            // tbStopBits
            // 
            this.tbStopBits.Font = new System.Drawing.Font("微软雅黑", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tbStopBits.Location = new System.Drawing.Point(552, 20);
            this.tbStopBits.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tbStopBits.Name = "tbStopBits";
            this.tbStopBits.Size = new System.Drawing.Size(52, 25);
            this.tbStopBits.TabIndex = 5;
            this.tbStopBits.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // cbPortName
            // 
            this.cbPortName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbPortName.Font = new System.Drawing.Font("微软雅黑", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.cbPortName.FormattingEnabled = true;
            this.cbPortName.Location = new System.Drawing.Point(60, 20);
            this.cbPortName.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.cbPortName.Name = "cbPortName";
            this.cbPortName.Size = new System.Drawing.Size(70, 27);
            this.cbPortName.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("微软雅黑", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(656, 22);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(54, 20);
            this.label2.TabIndex = 12;
            this.label2.Text = "校验位:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("微软雅黑", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label4.Location = new System.Drawing.Point(498, 22);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(54, 20);
            this.label4.TabIndex = 9;
            this.label4.Text = "停止位:";
            // 
            // tbBaudRate
            // 
            this.tbBaudRate.Font = new System.Drawing.Font("微软雅黑", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tbBaudRate.Location = new System.Drawing.Point(221, 20);
            this.tbBaudRate.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tbBaudRate.Name = "tbBaudRate";
            this.tbBaudRate.Size = new System.Drawing.Size(62, 25);
            this.tbBaudRate.TabIndex = 3;
            this.tbBaudRate.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("微软雅黑", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label3.Location = new System.Drawing.Point(340, 22);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(54, 20);
            this.label3.TabIndex = 7;
            this.label3.Text = "数据位:";
            // 
            // cbParity
            // 
            this.cbParity.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbParity.Font = new System.Drawing.Font("微软雅黑", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.cbParity.FormattingEnabled = true;
            this.cbParity.Location = new System.Drawing.Point(726, 20);
            this.cbParity.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.cbParity.Name = "cbParity";
            this.cbParity.Size = new System.Drawing.Size(62, 27);
            this.cbParity.TabIndex = 6;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("微软雅黑", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label5.Location = new System.Drawing.Point(10, 22);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(54, 20);
            this.label5.TabIndex = 5;
            this.label5.Text = "串口号:";
            // 
            // tbDateBits
            // 
            this.tbDateBits.Font = new System.Drawing.Font("微软雅黑", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tbDateBits.Location = new System.Drawing.Point(393, 20);
            this.tbDateBits.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tbDateBits.Name = "tbDateBits";
            this.tbDateBits.Size = new System.Drawing.Size(50, 25);
            this.tbDateBits.TabIndex = 4;
            this.tbDateBits.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("微软雅黑", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label6.Location = new System.Drawing.Point(168, 22);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(54, 20);
            this.label6.TabIndex = 3;
            this.label6.Text = "波特率:";
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.tbCodeResult);
            this.groupBox5.Controls.Add(this.label15);
            this.groupBox5.Controls.Add(this.tbCodeLength);
            this.groupBox5.Controls.Add(this.label14);
            this.groupBox5.Controls.Add(this.tbCodeAddress);
            this.groupBox5.Controls.Add(this.label13);
            this.groupBox5.Controls.Add(this.cbEnableCode);
            this.groupBox5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox5.Location = new System.Drawing.Point(2, 86);
            this.groupBox5.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox5.Size = new System.Drawing.Size(659, 38);
            this.groupBox5.TabIndex = 5;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "读码设置";
            // 
            // tbCodeResult
            // 
            this.tbCodeResult.Font = new System.Drawing.Font("微软雅黑", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tbCodeResult.Location = new System.Drawing.Point(572, 18);
            this.tbCodeResult.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tbCodeResult.Name = "tbCodeResult";
            this.tbCodeResult.ReadOnly = true;
            this.tbCodeResult.Size = new System.Drawing.Size(328, 25);
            this.tbCodeResult.TabIndex = 19;
            this.tbCodeResult.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Font = new System.Drawing.Font("微软雅黑", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label15.Location = new System.Drawing.Point(502, 20);
            this.label15.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(68, 20);
            this.label15.TabIndex = 20;
            this.label15.Text = "读码结果:";
            // 
            // tbCodeLength
            // 
            this.tbCodeLength.Font = new System.Drawing.Font("微软雅黑", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tbCodeLength.Location = new System.Drawing.Point(238, 18);
            this.tbCodeLength.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tbCodeLength.Name = "tbCodeLength";
            this.tbCodeLength.Size = new System.Drawing.Size(52, 25);
            this.tbCodeLength.TabIndex = 8;
            this.tbCodeLength.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Font = new System.Drawing.Font("微软雅黑", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label14.Location = new System.Drawing.Point(168, 20);
            this.label14.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(79, 20);
            this.label14.TabIndex = 18;
            this.label14.Text = "读码长度：";
            // 
            // tbCodeAddress
            // 
            this.tbCodeAddress.Font = new System.Drawing.Font("微软雅黑", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tbCodeAddress.Location = new System.Drawing.Point(409, 18);
            this.tbCodeAddress.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tbCodeAddress.Name = "tbCodeAddress";
            this.tbCodeAddress.Size = new System.Drawing.Size(74, 25);
            this.tbCodeAddress.TabIndex = 9;
            this.tbCodeAddress.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("微软雅黑", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label13.Location = new System.Drawing.Point(340, 20);
            this.label13.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(68, 20);
            this.label13.TabIndex = 16;
            this.label13.Text = "读码地址:";
            // 
            // cbEnableCode
            // 
            this.cbEnableCode.AutoSize = true;
            this.cbEnableCode.Location = new System.Drawing.Point(14, 22);
            this.cbEnableCode.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.cbEnableCode.Name = "cbEnableCode";
            this.cbEnableCode.Size = new System.Drawing.Size(99, 21);
            this.cbEnableCode.TabIndex = 7;
            this.cbEnableCode.Text = "是否启用读码";
            this.cbEnableCode.UseVisualStyleBackColor = true;
            this.cbEnableCode.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.tableLayoutPanel3);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(681, 2);
            this.panel1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.panel1.Name = "panel1";
            this.tableLayoutPanel2.SetRowSpan(this.panel1, 2);
            this.panel1.Size = new System.Drawing.Size(287, 546);
            this.panel1.TabIndex = 3;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 1;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Controls.Add(this.tableLayoutPanel6, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this.groupBox6, 0, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 2;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 70F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(287, 546);
            this.tableLayoutPanel3.TabIndex = 0;
            // 
            // tableLayoutPanel6
            // 
            this.tableLayoutPanel6.ColumnCount = 1;
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel6.Controls.Add(this.tabControl2, 0, 0);
            this.tableLayoutPanel6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel6.Location = new System.Drawing.Point(3, 166);
            this.tableLayoutPanel6.Name = "tableLayoutPanel6";
            this.tableLayoutPanel6.RowCount = 1;
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 377F));
            this.tableLayoutPanel6.Size = new System.Drawing.Size(281, 377);
            this.tableLayoutPanel6.TabIndex = 4;
            // 
            // tabControl2
            // 
            this.tabControl2.Controls.Add(this.tabPage3);
            this.tabControl2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl2.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tabControl2.Location = new System.Drawing.Point(2, 2);
            this.tabControl2.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabControl2.Name = "tabControl2";
            this.tabControl2.SelectedIndex = 0;
            this.tabControl2.Size = new System.Drawing.Size(277, 373);
            this.tabControl2.TabIndex = 0;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.dataGridView1);
            this.tabPage3.Location = new System.Drawing.Point(4, 26);
            this.tabPage3.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage3.Size = new System.Drawing.Size(269, 343);
            this.tabPage3.TabIndex = 0;
            this.tabPage3.Text = "结果显示";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // dataGridView1
            // 
            this.dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridView1.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(2, 2);
            this.dataGridView1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersWidth = 82;
            this.dataGridView1.RowTemplate.Height = 37;
            this.dataGridView1.Size = new System.Drawing.Size(265, 339);
            this.dataGridView1.TabIndex = 0;
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.tableLayoutPanel4);
            this.groupBox6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox6.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBox6.Location = new System.Drawing.Point(2, 2);
            this.groupBox6.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox6.Size = new System.Drawing.Size(283, 159);
            this.groupBox6.TabIndex = 3;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "结果";
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 2;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.Controls.Add(this.lbResult, 0, 1);
            this.tableLayoutPanel4.Controls.Add(this.panel2, 1, 0);
            this.tableLayoutPanel4.Controls.Add(this.pictureBox1, 0, 0);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(2, 18);
            this.tableLayoutPanel4.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 1;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 75F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 10F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(279, 139);
            this.tableLayoutPanel4.TabIndex = 3;
            // 
            // lbResult
            // 
            this.lbResult.AutoSize = true;
            this.lbResult.BackColor = System.Drawing.Color.Lime;
            this.lbResult.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbResult.Font = new System.Drawing.Font("微软雅黑", 16.125F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbResult.Location = new System.Drawing.Point(2, 104);
            this.lbResult.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lbResult.Name = "lbResult";
            this.lbResult.Size = new System.Drawing.Size(135, 35);
            this.lbResult.TabIndex = 0;
            this.lbResult.Text = "OK";
            this.lbResult.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.tableLayoutPanel5);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(141, 2);
            this.panel2.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.panel2.Name = "panel2";
            this.tableLayoutPanel4.SetRowSpan(this.panel2, 2);
            this.panel2.Size = new System.Drawing.Size(136, 135);
            this.panel2.TabIndex = 0;
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.ColumnCount = 2;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel5.Controls.Add(this.btClearData, 1, 5);
            this.tableLayoutPanel5.Controls.Add(this.label1, 0, 1);
            this.tableLayoutPanel5.Controls.Add(this.lbCt, 1, 4);
            this.tableLayoutPanel5.Controls.Add(this.label11, 0, 4);
            this.tableLayoutPanel5.Controls.Add(this.lbOKNum, 1, 1);
            this.tableLayoutPanel5.Controls.Add(this.lbOKPercent, 1, 3);
            this.tableLayoutPanel5.Controls.Add(this.label16, 0, 2);
            this.tableLayoutPanel5.Controls.Add(this.label17, 0, 3);
            this.tableLayoutPanel5.Controls.Add(this.lbNGNum, 1, 2);
            this.tableLayoutPanel5.Controls.Add(this.lbAllNum, 1, 0);
            this.tableLayoutPanel5.Controls.Add(this.label18, 0, 0);
            this.tableLayoutPanel5.Controls.Add(this.btHandTest, 0, 5);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel5.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 6;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(136, 135);
            this.tableLayoutPanel5.TabIndex = 3;
            // 
            // btClearData
            // 
            this.btClearData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btClearData.Location = new System.Drawing.Point(70, 112);
            this.btClearData.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btClearData.Name = "btClearData";
            this.btClearData.Size = new System.Drawing.Size(64, 21);
            this.btClearData.TabIndex = 4;
            this.btClearData.Text = "清除数据";
            this.btClearData.UseVisualStyleBackColor = true;
            this.btClearData.Click += new System.EventHandler(this.btClearData_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(2, 22);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(64, 22);
            this.label1.TabIndex = 0;
            this.label1.Text = "OK数量:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbCt
            // 
            this.lbCt.AutoSize = true;
            this.lbCt.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbCt.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbCt.Location = new System.Drawing.Point(70, 88);
            this.lbCt.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lbCt.Name = "lbCt";
            this.lbCt.Size = new System.Drawing.Size(64, 22);
            this.lbCt.TabIndex = 2;
            this.lbCt.Text = "0";
            this.lbCt.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label11.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label11.Location = new System.Drawing.Point(2, 88);
            this.label11.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(64, 22);
            this.label11.TabIndex = 0;
            this.label11.Text = "耗时(ms):";
            this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbOKNum
            // 
            this.lbOKNum.AutoSize = true;
            this.lbOKNum.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbOKNum.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbOKNum.Location = new System.Drawing.Point(70, 22);
            this.lbOKNum.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lbOKNum.Name = "lbOKNum";
            this.lbOKNum.Size = new System.Drawing.Size(64, 22);
            this.lbOKNum.TabIndex = 2;
            this.lbOKNum.Text = "0";
            this.lbOKNum.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbOKPercent
            // 
            this.lbOKPercent.AutoSize = true;
            this.lbOKPercent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbOKPercent.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbOKPercent.Location = new System.Drawing.Point(70, 66);
            this.lbOKPercent.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lbOKPercent.Name = "lbOKPercent";
            this.lbOKPercent.Size = new System.Drawing.Size(64, 22);
            this.lbOKPercent.TabIndex = 2;
            this.lbOKPercent.Text = "0";
            this.lbOKPercent.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label16.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label16.Location = new System.Drawing.Point(2, 44);
            this.label16.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(64, 22);
            this.label16.TabIndex = 0;
            this.label16.Text = "NG数量:";
            this.label16.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label17.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label17.Location = new System.Drawing.Point(2, 66);
            this.label17.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(64, 22);
            this.label17.TabIndex = 0;
            this.label17.Text = "良率:";
            this.label17.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbNGNum
            // 
            this.lbNGNum.AutoSize = true;
            this.lbNGNum.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbNGNum.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbNGNum.Location = new System.Drawing.Point(70, 44);
            this.lbNGNum.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lbNGNum.Name = "lbNGNum";
            this.lbNGNum.Size = new System.Drawing.Size(64, 22);
            this.lbNGNum.TabIndex = 2;
            this.lbNGNum.Text = "0";
            this.lbNGNum.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbAllNum
            // 
            this.lbAllNum.AutoSize = true;
            this.lbAllNum.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbAllNum.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbAllNum.Location = new System.Drawing.Point(70, 0);
            this.lbAllNum.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lbAllNum.Name = "lbAllNum";
            this.lbAllNum.Size = new System.Drawing.Size(64, 22);
            this.lbAllNum.TabIndex = 2;
            this.lbAllNum.Text = "0";
            this.lbAllNum.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label18.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label18.Location = new System.Drawing.Point(2, 0);
            this.label18.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(64, 22);
            this.label18.TabIndex = 0;
            this.label18.Text = "生产总数:";
            this.label18.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btHandTest
            // 
            this.btHandTest.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btHandTest.Location = new System.Drawing.Point(2, 112);
            this.btHandTest.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btHandTest.Name = "btHandTest";
            this.btHandTest.Size = new System.Drawing.Size(64, 21);
            this.btHandTest.TabIndex = 3;
            this.btHandTest.Text = "手动测试";
            this.btHandTest.UseVisualStyleBackColor = true;
            this.btHandTest.Click += new System.EventHandler(this.btHandTest_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Image = global::YUNTIANVision.Properties.Resources.云田视觉;
            this.pictureBox1.Location = new System.Drawing.Point(2, 2);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(135, 100);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 2;
            this.pictureBox1.TabStop = false;
            // 
            // frmOneCamera
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(970, 550);
            this.Controls.Add(this.tableLayoutPanel2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "frmOneCamera";
            this.Text = "frmOneCamera";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmOneCamera_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmOneCamera_FormClosed);
            this.Load += new System.EventHandler(this.frmOneCamera_Load);
            this.contextMenuStrip1.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel6.ResumeLayout(false);
            this.tabControl2.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.groupBox6.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel4.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.tableLayoutPanel5.ResumeLayout(false);
            this.tableLayoutPanel5.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem 读取图片ToolStripMenuItem;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.TextBox tbLog;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TextBox tbReadySignal;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox tbGrabImageSignal;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox cbGrayOrRgb;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Button btAddNGSignal;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btClosePlc;
        private System.Windows.Forms.Button btOpenPlc;
        private System.Windows.Forms.TextBox tbStopBits;
        private System.Windows.Forms.ComboBox cbPortName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbBaudRate;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cbParity;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbDateBits;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.TextBox tbCodeResult;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.TextBox tbCodeLength;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.TextBox tbCodeAddress;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.CheckBox cbEnableCode;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.Label lbResult;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        private System.Windows.Forms.Button btClearData;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lbCt;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label lbOKNum;
        private System.Windows.Forms.Label lbOKPercent;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label lbNGNum;
        private System.Windows.Forms.Label lbAllNum;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Button btHandTest;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel6;
        private System.Windows.Forms.TabControl tabControl2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.TextBox tbOKSignal;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ToolStripMenuItem 读取图片目录ToolStripMenuItem;
        private HalconDotNet.HSmartWindowControl hWindowControl1;
    }
}