using HalconDotNet;
using HslCommunication;
using HslCommunication.Profinet.Panasonic;
using LW.ZOOM;
using MvCamCtrl.NET;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using static HTCSharpDemo.Program;
using Newtonsoft.Json;
using static YUNTIANVision.NGTypePara;
using System.Globalization;
using YUNTIANVision.HTDLModel;
using Path = System.IO.Path;
using System.Text;
using System.Windows.Shapes;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using Timer = System.Windows.Forms.Timer;
using Rex.UI;
using System.Diagnostics;
using System.Windows.Media.Imaging;
using System.Drawing;
using Rectangle = System.Drawing.Rectangle;

namespace YUNTIANVision
{

    public delegate void updateDataView(int index);

    public enum HWindow_Location
    {
        LeftWindow = 0,   //左窗口
        RightWindow = 1,    // 右窗口
    };

    public partial class frmTwoCamera : Form
    {
        public frmTwoCamera()
        {
            frmMain.closeAll += new closeDelegate(frmTwoClose);
            frmMain.controlEvent += new controlDelegate(runControl);      
            frmCameraExposeSet.cameraExposeSetEvent += new cameraExposeSet(exposeSet);
            frmCameraExposeSet.cameraExposeCloseEvent += new cameraExposeClose(exposeClose);
            frmSet.newConfigEvent += new newConfigDel(loadNewFile);
            frmSet.nowisDebugMode += new nowisDebugMode(changeDebugMode);
            frmSet.continueDebugMode += new continueDebugMode(contineDebugMode);
            updateDataViewData += updateDV;
            InitializeComponent();
        }


        #region 定义对象

        Thread grabOneCameraThread = null;
        Thread grabTwoCameraThread = null;
        Thread listenSerialPortThread = null;
        Thread runListenSerialPort2Thread = null;
        Thread grabImage = null;

        // <summary>
        /// 相机1OK类型
        /// </summary>
        List<string> list_OKType_classId_left = new List<string>();
        // <summary>
        /// 相机2OK类型
        /// </summary>
        List<string> list_OKType_classId_right = new List<string>();

        string rootDirectory1 = AppDomain.CurrentDomain.BaseDirectory + "NGTypeSignalSet2_1.txt";
        string rootDirectory2 = AppDomain.CurrentDomain.BaseDirectory + "NGTypeSignalSet2_2.txt";
        frmNGSignalSet set1;
        TreeFileHelper treeFileHelper1;
        frmNGSignalSet set2;
        TreeFileHelper treeFileHelper2;

        /// <summary>
        /// 深度学习句柄
        /// </summary>
        IntPtr studyHandle;
        IntPtr studyHandle2;
        /// <summary>
        /// 手动触发拍照
        /// </summary>
        private static volatile bool isImage = false;
        /// <summary>
        /// 产品总数
        /// </summary>
        double AllNum1 = 0;
        /// <summary>
        /// 产品图像处理时间
        /// </summary>
        int ct1;
        /// <summary>
        /// OK总数
        /// </summary>
        double OKNum1 = 0;
        /// <summary>
        /// NG总数
        /// </summary>
        double NGNum1 = 0;
        /// <summary>
        /// 产品总数
        /// </summary>
        double AllNum2 = 0;
        /// <summary>
        /// 产品图像处理时间
        /// </summary>
        int ct2;
        /// <summary>
        /// OK总数
        /// </summary>
        double OKNum2 = 0;
        /// <summary>
        /// NG总数
        /// </summary>
        double NGNum2 = 0;
        /// <summary>
        /// 显示相机的操作log
        /// </summary>
        /// <param name="textBox"></param>
        /// <param name="log"></param>
        private delegate void myDelegateLog(TextBox textBox, string log, Exception ex);
        private static myDelegateLog GetMyDelegateLog;
        /// <summary>
        /// 显示通讯状态委托
        /// </summary>
        /// <param name="label"></param>
        /// <param name="status"></param>
        /// <param name="color"></param>
        private delegate void myDelegatePlc(Label label, string status, System.Drawing.Color color);
        private myDelegatePlc GetMyDelegatePlc;
        private delegate void myDelegateRunControl();
        /// <summary>
        /// 串口1的对象
        /// </summary>
        PanasonicMewtocol panasonicMewtocol1 = new PanasonicMewtocol();
        /// <summary>
        /// 相机1ng信号
        /// </summary>
        public static Dictionary<string, string> dic_defectClassificationswithPLCaddress_left = new Dictionary<string, string>();
        /// <summary>
        /// 相机2ng信号
        /// </summary>
        public static Dictionary<string, string> dic_defectClassificationswithPLCaddress_right = new Dictionary<string, string>();

        string xy = null;
        string gray = null;
        updateDataView updateDataViewData = null;

        private ManualResetEventSlim event1 = new ManualResetEventSlim(false);
        private ManualResetEventSlim event2 = new ManualResetEventSlim(false);
        private CountdownEvent countdown = new CountdownEvent(2);
        private CountdownEvent countdownTwo = new CountdownEvent(1);

        // 白天清零时间
        string day_clearDataTime;
        // 晚上清零时间
        string night_clearDataTime;
        // 是否启用清零
        bool isUseClearDataTime;

        /// <summary>
        /// dl配置文件路径
        /// </summary>
        string configPath;
        string configPath2;

        /// <summary>
        /// 定时器定时触发拍照
        /// </summary>
        private Timer timer;

        //判断是否最小化
        private volatile bool isMiniSizeState = false;

        LinkedList<string> logLines_left = new LinkedList<string>();
        LinkedList<string> logLines_right = new LinkedList<string>();
        int maxLogLines = 15; // 设置最大保存的日志条数

        //双信号
        volatile bool hasTwoSignal = false;

        int dingwei_num_left = -1;
        int dingwei_num_right = -1;
        #endregion

        #region 加载方案
        IntPtr ptr1 = IntPtr.Zero;
        IntPtr ptr2 = IntPtr.Zero;

        /// <summary>
        /// 读图模式图片路径
        /// </summary>
        string director_left;
        string director_right;
        List<string> imagePathFiles_left = new List<string>();
        List<string> imagePathFiles_right = new List<string>();
        //切换到图片触发模式
        private  bool trigerDLuseImage = false;
        //图片触发模式准备完成
        private volatile bool trigerDLuseImage_isReady = false;
        private bool window_left_pictruePath_isReady = false;
        private bool window_right_pictruePath_isReady = false;
        int pictrueIndex_left = 0;
        int pictrueIndex_right = 0;

        Dictionary<string, dataViewChangeModel> dataViewChangeModel_left;
        Dictionary<string, dataViewChangeModel> dataViewChangeModel_right;

        //相机列表
        List<MyCamera> cameraList = new List<MyCamera>();
        //决定取用哪个相机
        volatile int cameraIndex = 0;

        int delayTime_leftwindow = 0;
        int delayTime_rightwindow = 0;

        int rotAngel_left;
        int rotAngel_right;
        #endregion

        #region load/close事件

        private void frmTwoCamera_Load(object sender, EventArgs e)
        {
            createHead1();
            createHead2();
            string tmp_dingwei_num_left = IniHelper.SaveSetIni.Read("定位个数", "左窗口定位个数");
            if (string.IsNullOrEmpty(tmp_dingwei_num_left))
            {
                IniHelper.SaveSetIni.Write("定位个数", "左窗口定位个数", "1");
                dingwei_num_left = 1;
            }
            else
            {
                if (IsNumber(tmp_dingwei_num_left))
                {
                    dingwei_num_left = int.Parse(tmp_dingwei_num_left);
                }
            }
            string tmp_dingwei_num_right = IniHelper.SaveSetIni.Read("定位个数", "右窗口定位个数");
            if (string.IsNullOrEmpty(tmp_dingwei_num_right))
            {
                IniHelper.SaveSetIni.Write("定位个数", "右窗口定位个数", "1");
                dingwei_num_right = 1;
            }
            else
            {
                if (IsNumber(tmp_dingwei_num_right))
                {
                    dingwei_num_right = int.Parse(tmp_dingwei_num_right);
                }
            }
            string corner_left = IniHelper.SaveSetIni.Read("旋转角度", "左窗口图像旋转角度");
            if (string.IsNullOrEmpty(corner_left))
            {
                IniHelper.SaveSetIni.Write("旋转角度", "左窗口图像旋转角度", "0");
                rotAngel_left = 0;
            }
            else
            {
                if (IsNumber(corner_left))
                {
                    rotAngel_left = int.Parse(corner_left);
                }
            }
            string corner_right = IniHelper.SaveSetIni.Read("旋转角度", "右窗口图像旋转角度");
            if (string.IsNullOrEmpty(corner_right))
            {
                IniHelper.SaveSetIni.Write("旋转角度", "右窗口图像旋转角度", "0");
                rotAngel_right = 0;
            }
            else
            {
                if (IsNumber(corner_right))
                {
                    rotAngel_right = int.Parse(corner_right);
                }
            }

            string changeViews = IniHelper.SaveSetIni.Read("左右相机对换", "对换相机");
            if (string.IsNullOrEmpty(changeViews))
            {
                IniHelper.SaveSetIni.Write("左右相机对换", "对换相机", "F");
            }
            else
            {
                if (changeViews.Equals("T"))
                {
                    cameraIndex = 1;
                    checkBox_camera.Checked = true;
                }
            }

            bool b_model1isNull = false; //configPath1为空
            bool b_model2isNull = false; //configPath2为空
            treeFileHelper1 = new TreeFileHelper();     
            treeFileHelper2 = new TreeFileHelper();

            configPath = IniHelper.SaveSetIni.Read("深度学习文件", "配置文件路径");
            if (!string.IsNullOrEmpty(configPath))
            { 
                treeFileHelper1.filePath = configPath;
                addNGDic1();
                addNGorOK1();                
            }
            else
            {
                b_model1isNull = true;
                if (tbLog1.IsHandleCreated)
                    tbLog1.BeginInvoke(GetMyDelegateLog, tbLog1, "深度学习配置文件路径不存在",null);
                MessageBox.Show("深度学习文件配置文件路径不存在");
            }

            configPath2 = IniHelper.SaveSetIni.Read("深度学习文件", "配置文件路径1");
            if (!string.IsNullOrEmpty(configPath2))
            {
                treeFileHelper2.filePath = configPath2;
                addNGDic2();
                addNGorOK2();               
                hasTwoModel = true;
                
            }
            else if(!b_model1isNull)
            {
                treeFileHelper2 = treeFileHelper1;
                treeFileHelper2.filePath = configPath;
                addNGDic2();
                addNGorOK2();              
                hasTwoModel = false; 
                b_model2isNull = true; //路径2是空
                configPath2 = configPath;
            }
            else
            {
                b_model2isNull = true; //路径2是空
                b_model1isNull = true; //路径1是空
                if (tbLog1.IsHandleCreated)
                    tbLog1.BeginInvoke(GetMyDelegateLog, tbLog1, "深度学习配置文件路径2不存在",null);
                MessageBox.Show("深度学习文件配置文件路径2不存在");
            }
            set1 = new frmNGSignalSet("NG2", treeFileHelper1);
            set1.loadNGEvent -= new loadNGSignalDic(addNGDic1);
            set1.loadNGEvent += new loadNGSignalDic(addNGDic1);
            set1.loadNGEvent -= new loadNGSignalDic(addNGorOK1);
            set1.loadNGEvent += new loadNGSignalDic(addNGorOK1);
            set1.updateClassIdisOKStateEvent -= new updateClassIdisOKState(updateSet1ClassIsOk);
            set1.updateClassIdisOKStateEvent += new updateClassIdisOKState(updateSet1ClassIsOk);

            set2 = new frmNGSignalSet("NG3", treeFileHelper2);
            set2.loadNGEvent -= new loadNGSignalDic(addNGDic2);
            set2.loadNGEvent += new loadNGSignalDic(addNGDic2);
            set2.loadNGEvent -= new loadNGSignalDic(addNGorOK2);
            set2.loadNGEvent += new loadNGSignalDic(addNGorOK2);
            set2.updateClassIdisOKStateEvent -= new updateClassIdisOKState(updateSet2ClassIsOk);
            set2.updateClassIdisOKStateEvent += new updateClassIdisOKState(updateSet2ClassIsOk);

            hWindowControl1.MouseWheel -= new System.Windows.Forms.MouseEventHandler(this.my_MouseWheel);
            hWindowControl1.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.my_MouseWheel);

            hWindowControl2.MouseWheel -= new System.Windows.Forms.MouseEventHandler(this.my_MouseWhee2);
            hWindowControl2.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.my_MouseWhee2);

            // 在窗口加载时绑定 Resize 事件
            this.Resize += Form_Resize;

            UpDownBase left_updown = (UpDownBase)numericUpDown_left;
            left_updown.TextChanged -= new EventHandler(left_updown_TextChanged);
            left_updown.TextChanged += new EventHandler(left_updown_TextChanged);
            UpDownBase right_updown = (UpDownBase)numericUpDown_right;
            right_updown.TextChanged -= new EventHandler(right_updown_TextChanged);
            right_updown.TextChanged += new EventHandler(right_updown_TextChanged);

            GetMyDelegateLog = new myDelegateLog(showLog);
            GetMyDelegatePlc = new myDelegatePlc(showPlcStatus);

            loadCodeSet();
            loadResult();

            initDataOnTimeAutoClear();

            loadOneCameraSet();
            loadTwoCameraSet();

            dataGridView_left.RowHeadersVisible = false;
            dataGridView_right.RowHeadersVisible = false;

            Task openCamsTask = new Task(() =>
            {
                //开相机
                openOneHikVisionCam();
                openSerilPort(); //串口1
            });
            Task loadDLModelTask = new Task(() =>
           {
               if (progressBar1.IsHandleCreated)
                   progressBar1.BeginInvoke(new Action(() => { progressBar1.Value = 40; }));
               // 1.加载模型
               if (tbLog1.IsHandleCreated)
                   tbLog1.BeginInvoke(GetMyDelegateLog, tbLog1, "模型加载中，请稍后...", null);
               if (tbLog2.IsHandleCreated)
                   tbLog2.BeginInvoke(GetMyDelegateLog, tbLog2, "模型加载中，请稍后...", null);
               // 1.加载模型
               bool b_sucess = loadDlModel(b_model1isNull, b_model2isNull);
               if(progressBar1.IsHandleCreated)
                    progressBar1.BeginInvoke(new Action(() => { progressBar1.Value = 100; }));
               if (tbLog1.IsHandleCreated)
                   tbLog1.BeginInvoke(GetMyDelegateLog, tbLog1, "模型加载完成...", null);
               if (tbLog2.IsHandleCreated)
                   tbLog2.BeginInvoke(GetMyDelegateLog, tbLog2, "模型加载完成...", null);

               // 2.开线程监听
               grabOneCameraThread = new Thread(runGrabOneCameraThread);
               grabOneCameraThread.IsBackground = true;
               grabOneCameraThread.Start();
               grabTwoCameraThread = new Thread(rungrabTwoCameraThread);
               grabTwoCameraThread.IsBackground = true;
               grabTwoCameraThread.Start();
               listenSerialPortThread = new Thread(runListenSerialPortThread);
               listenSerialPortThread.IsBackground = true;
               listenSerialPortThread.Start();
           });
            openCamsTask.Start();
            loadDLModelTask.Start();
        }


        private void frmTwoClose()
        {
            this.Close();
        }

        private void frmTwoCamera_FormClosed(object sender, FormClosedEventArgs e)
        {

        }


        private void frmTwoCamera_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 关闭测试模式
            IniHelper.SaveSetIni.Write("测试模式", "测试模式", "F");

            isRunning = false;
            // 触发线程2和线程3执行,让这两个线程运行完
            event1.Set();
            event2.Set();
            // 两个相机的线程此时应该关闭
            if (grabOneCameraThread != null)
                grabOneCameraThread.Join();
            if (grabTwoCameraThread != null)
                grabTwoCameraThread.Join();
            if (listenSerialPortThread != null)
                try
                {
                    listenSerialPortThread.Abort();
                }
                catch (ThreadAbortException)
                {
                   
                }
            
                Task closeCams = new Task(() =>
            {
                closeOneHikVisionCam();
            });
            closeCams.Start();

            Task closePlcs = new Task(() =>
            {
                if (panasonicMewtocol1.IsOpen())
                {
                    panasonicMewtocol1.Write(tbReadySignal1.Text, false);
                    if (hasTwoSignal)//双信号
                    {
                        panasonicMewtocol1.Write(tbReadySignal2.Text, false);
                    }
                    panasonicMewtocol1?.Close();
                }
            });
            closePlcs.Start();

            Task closeDlHandle = new Task(() =>
            {
                if (studyHandle != IntPtr.Zero)
                    HTCSharpDemo.Program.ReleaseTree(studyHandle);
                if (hasTwoModel)
                {
                    if (studyHandle2 != IntPtr.Zero && studyHandle != studyHandle2)
                        HTCSharpDemo.Program.ReleaseTree(studyHandle2);
                }
            });
            closeDlHandle.Start();

            this.Resize -= Form_Resize;
            UpDownBase left_updown = (UpDownBase)numericUpDown_left;
            UpDownBase right_updown = (UpDownBase)numericUpDown_right;
            left_updown.TextChanged -= new EventHandler(left_updown_TextChanged);
            right_updown.TextChanged -= new EventHandler(right_updown_TextChanged);

            Task.WaitAll(closeCams, closePlcs, closeDlHandle);
            IniHelper.SaveSetIni.Write("布局", "窗口数量", "2");
            frmMain.controlEvent -= new controlDelegate(runControl);
            if(set1 != null)
            {
                set1.loadNGEvent -= new loadNGSignalDic(addNGDic1);
                set1.loadNGEvent -= new loadNGSignalDic(addNGorOK1);
                set1.updateClassIdisOKStateEvent -= new updateClassIdisOKState(updateSet1ClassIsOk);
            }
            if(set2 != null)
            {
                set2.loadNGEvent -= new loadNGSignalDic(addNGDic2);
                set2.loadNGEvent -= new loadNGSignalDic(addNGorOK2);
                set2.updateClassIdisOKStateEvent -= new updateClassIdisOKState(updateSet2ClassIsOk);
            }          
            frmSet.newConfigEvent -= new newConfigDel(loadNewFile);
            frmMain.closeAll -= new closeDelegate(frmTwoClose);
            hWindowControl1.MouseWheel -= new System.Windows.Forms.MouseEventHandler(this.my_MouseWheel);
            hWindowControl2.MouseWheel -= new System.Windows.Forms.MouseEventHandler(this.my_MouseWhee2);
        }

        // Resize 事件处理程序
        private void Form_Resize(object sender, EventArgs e)
        {
            // 判断窗体的宽度和高度
            if (this.Size.Width < 50 && this.Size.Height < 50)
            {
                isMiniSizeState = true;
            }
            else
            {
                isMiniSizeState = false;
            }
        }
        #endregion

        #region 初始化设置
        void initDataOnTimeAutoClear()
        {
            day_clearDataTime = IniHelper.SaveSetIni.Read("数据自动清零", "白天清零时间");
            if (day_clearDataTime == "")
            {
                IniHelper.SaveSetIni.Write("数据自动清零", "白天清零时间", "08:00:00");
                day_clearDataTime = "08:00:00";
            }
            night_clearDataTime = IniHelper.SaveSetIni.Read("数据自动清零", "晚上清零时间");
            if (night_clearDataTime == "")
            {
                IniHelper.SaveSetIni.Write("数据自动清零", "晚上清零时间", "20:00:00");
                night_clearDataTime = "20:00:00";
            }
            string isUse = IniHelper.SaveSetIni.Read("数据自动清零", "数据清零生效");
            if (isUse == "")
            {
                IniHelper.SaveSetIni.Write("数据自动清零", "数据清零生效", "T");
                isUseClearDataTime = true;
            }
            else if (isUse == "T")
            {
                isUseClearDataTime = true;
            }
            else if (isUse == "F")
            {
                isUseClearDataTime = false;
            }
        }

        // 初始化读码设置
        private void loadCodeSet()
        {
            string isOpen = IniHelper.SaveSetIni.Read("双相机读码设置", "是否启用读码");
            if (String.IsNullOrEmpty(isOpen))
            {
                cbEnableCode.Checked = false;
                tbCodeAddress.Enabled = false;
                tbCodeLength.Enabled = false;
                tbCodeResult.Enabled = false;
            }
            else
            {
                if (isOpen == "T")
                {
                    cbEnableCode.Checked = true;
                    tbCodeAddress.Enabled = true;
                    tbCodeLength.Enabled = true;
                    tbCodeResult.Enabled = true;
                }
                else if (isOpen == "F")
                {
                    cbEnableCode.Checked = false;
                    tbCodeAddress.Enabled = false;
                    tbCodeLength.Enabled = false;
                    tbCodeResult.Enabled = false;
                }
                tbCodeAddress.Text = IniHelper.SaveSetIni.Read("双相机读码设置", "读码地址");
                tbCodeLength.Text = IniHelper.SaveSetIni.Read("双相机读码设置", "读码长度");
            }
        }

        // 初始化相机1设置
        private void loadOneCameraSet()
        {
            for (int i = 0; i < SerialPort.GetPortNames().Length; i++)
            {
                cbPortName1.Items.Add(SerialPort.GetPortNames()[i]);
                if (!String.IsNullOrEmpty(IniHelper.SaveSetIni.Read("串口1设置", "串口号")))
                {
                    if (SerialPort.GetPortNames()[i] == IniHelper.SaveSetIni.Read("串口1设置", "串口号"))
                        cbPortName1.SelectedIndex = i;
                }
                else
                    cbPortName1.SelectedIndex = 0;
            }
            tbBaudRate1.Text = IniHelper.SaveSetIni.Read("串口1设置", "波特率");
            tbDateBits1.Text = IniHelper.SaveSetIni.Read("串口1设置", "数据位");
            tbStopBits1.Text = IniHelper.SaveSetIni.Read("串口1设置", "停止位");
            cbParity1.Items.Add("无");
            cbParity1.Items.Add("奇校验");
            cbParity1.Items.Add("偶校验");
            cbParity1.SelectedIndex = 0;
            for (int i = 0; i < cbParity1.Items.Count; i++)
            {
                if (!String.IsNullOrEmpty(IniHelper.SaveSetIni.Read("串口1设置", "校验位")))
                {
                    if (cbParity1.Items[i].ToString() == IniHelper.SaveSetIni.Read("串口1设置", "校验位"))
                        cbParity1.SelectedIndex = i;
                }
                else
                    cbParity1.SelectedIndex = 0;
            }
            tbGrabImageSignal1.Text = IniHelper.SaveSetIni.Read("相机1PLC设置", "拍照信号");
            tbOKSignal1.Text = IniHelper.SaveSetIni.Read("相机1PLC设置", "OK信号");
            tbReadySignal1.Text = IniHelper.SaveSetIni.Read("相机1PLC设置", "准备信号");
            textBox_time_left.Text = IniHelper.SaveSetIni.Read("拍照延迟时间设置", "左窗口相机延迟拍照时间");
            numericUpDown_left.Value = rotAngel_left;
        }

        // 初始化相机2设置
        private void loadTwoCameraSet()
        {
            tbOKSignal2.Text = IniHelper.SaveSetIni.Read("相机2PLC设置", "OK信号");
            tbReadySignal2.Text = IniHelper.SaveSetIni.Read("相机2PLC设置", "准备信号");
            tbGrabImageSignal2.Text = IniHelper.SaveSetIni.Read("相机2PLC设置", "拍照信号");

            textBox_time_right.Text = IniHelper.SaveSetIni.Read("拍照延迟时间设置", "右窗口相机延迟拍照时间");
            numericUpDown_right.Value = rotAngel_right;


            /// 双信号设置 ///
            string twoSignal = IniHelper.SaveSetIni.Read("双信号设置", "是否启用");
            if (!String.IsNullOrEmpty(twoSignal) && twoSignal == "T")
            {
                controlSignal2(true);
            }
            else
            {
                controlSignal1(false);
                controlSignal2(false);
            }
        }

        #endregion

        #region 运行线程

        private void runListenSerialPortThread()
        {
            string imgSavePath = IniHelper.SaveSetIni.Read("图片设置", "保存路径");

            while (isRunning)
            {
                while (frmMain.m_bPause && isRunning)
                {
                    if (frmMain.m_bResume)
                    {
                        frmMain.m_bPause = false;
                        frmMain.m_bResume = false;
                        break;
                    }

                    textBox_time_left.Invoke(new Action(() => {
                        if (!string.IsNullOrEmpty(textBox_time_left.Text) && IsNumber(textBox_time_left.Text))
                        {
                            delayTime_leftwindow = Convert.ToInt32(textBox_time_left.Text);
                            IniHelper.SaveSetIni.Write("拍照延迟时间设置", "左窗口相机延迟拍照时间",textBox_time_left.Text);
                        }
                    }));
                    textBox_time_right.Invoke(new Action(() => {
                        if (!string.IsNullOrEmpty(textBox_time_right.Text) && IsNumber(textBox_time_right.Text))
                        {
                            delayTime_rightwindow = Convert.ToInt32(textBox_time_right.Text);
                            IniHelper.SaveSetIni.Write("拍照延迟时间设置", "右窗口相机延迟拍照时间", textBox_time_right.Text);
                        }
                    }));

                    string corner_left_string =  IniHelper.SaveSetIni.Read("旋转角度", "左窗口图像旋转角度");
                    if (IsNumber(corner_left_string))
                    {
                        rotAngel_left = int.Parse(corner_left_string);
                    }
                    string corner_right_string = IniHelper.SaveSetIni.Read("旋转角度", "右窗口图像旋转角度");
                    if (IsNumber(corner_right_string))
                    {
                        rotAngel_right = int.Parse(corner_right_string);
                    }

                    string tmp_dingwei_num_left = IniHelper.SaveSetIni.Read("定位个数", "左窗口定位个数");
                    if (IsNumber(tmp_dingwei_num_left))
                    {
                        dingwei_num_left = int.Parse(tmp_dingwei_num_left);
                    }
                    string tmp_dingwei_num_right = IniHelper.SaveSetIni.Read("定位个数", "右窗口定位个数");
                    if (IsNumber(tmp_dingwei_num_right))
                    {
                        dingwei_num_right = int.Parse(tmp_dingwei_num_right);
                    }
                    Thread.Sleep(20);
                }
                if (isRunning)
                {
                    OperateResult<bool> res = null;
                    if (hasTwoSignal)//双信号
                    {
                        //相机1
                        if (panasonicMewtocol1.IsOpen())
                        {
                            res = panasonicMewtocol1.ReadBool(tbGrabImageSignal1.Text);
                        }
                        if ((res != null && res.Content) || isImage)
                        {
                            //回写plc
                            panasonicMewtocol1.Write(tbGrabImageSignal1.Text, false);

                            // 触发线程2执行
                            event1.Set();

                            // 等待线程2和线程3完成
                            countdown.Wait();

                            // 重置事件和倒计时器
                            countdown.Reset();
                        }
                    }
                    else
                    {
                        if (panasonicMewtocol1.IsOpen())
                        {
                            res = panasonicMewtocol1.ReadBool(tbGrabImageSignal1.Text);
                        }
                        if ((res != null && res.Content) || isImage)
                        {
                            //回写plc
                            panasonicMewtocol1.Write(tbGrabImageSignal1.Text, false);

                            // 触发线程2和线程3执行
                            event1.Set();
                            event2.Set();

                            // 等待线程2和线程3完成
                            countdown.Wait();

                            // 重置事件和倒计时器
                            countdown.Reset();
                        }
                    }

                    Thread.Sleep(10);
                    // 后处理
                    postProcess(imgSavePath);
                }
            }
        }

        private void runListenSerialPortTwoThread()
        {
            while (isRunning && hasTwoSignal)
            {
                while (frmMain.m_bPause && isRunning)
                {
                    if (frmMain.m_bResume)
                    {
                        frmMain.m_bPause = false;
                        frmMain.m_bResume = false;
                        break;
                    }
                }
                OperateResult<bool> res = null;
                if (panasonicMewtocol1.IsOpen())
                {
                    res = panasonicMewtocol1.ReadBool(tbGrabImageSignal2.Text);
                }
                if ((res != null && res.Content) || isImage)
                {
                    //回写plc
                    panasonicMewtocol1.Write(tbGrabImageSignal2.Text, false);

                    // 触发线程2和线程3执行
                    if (hasTwoSignal)
                    {
                        event2.Set();
                    }

                    // 等待线程2和线程3完成
                    countdownTwo.Wait();

                    // 重置事件和倒计时器
                    countdownTwo.Reset();
                }
                Thread.Sleep(10);
            }
        }

        void clearTotalNum()
        {
            AllNum1 = 0;
            lbOKNum1.Text = "0";
            OKNum1 = 0;
            lbNGNum1.Text = "0";
            NGNum1 = 0;
            lbOKPercent1.Text = "0";
            lbCt1.Text = "0";
            lbResult1.BackColor = System.Drawing.Color.Green;
            lbResult1.Text = "OK";
            lbAllNum2.Text = "0";
            AllNum2 = 0;
            lbOKNum2.Text = "0";
            OKNum2 = 0;
            lbNGNum2.Text = "0";
            NGNum2 = 0;
            lbOKPercent2.Text = "0";
            lbCt2.Text = "0";
            lbResult2.BackColor = System.Drawing.Color.Green;
            lbResult2.Text = "OK";
        }

        void postProcess(string imgSavePath)
        {
            string isUse = IniHelper.SaveSetIni.Read("数据自动清零", "数据清零生效");
            if (isUse == "")
            {
                isUseClearDataTime = true;
            }
            else if (isUse == "T")
            {
                isUseClearDataTime = true;
            }
            else if (isUse == "F")
            {
                isUseClearDataTime = false;
            }
            // 数据到点清零
            if (isUseClearDataTime)
            {
                day_clearDataTime = IniHelper.SaveSetIni.Read("数据自动清零", "白天清零时间");
                night_clearDataTime = IniHelper.SaveSetIni.Read("数据自动清零", "晚上清零时间");
                DateTime now = DateTime.Now;
                IFormatProvider ifp = new CultureInfo("zh-CN", true);
                DateTime day_time = DateTime.ParseExact(day_clearDataTime, "HH:mm:ss", ifp);
                DateTime night_time = DateTime.ParseExact(night_clearDataTime, "HH:mm:ss", ifp);

                TimeSpan daytimeDifference = (day_time - now).Duration();
                TimeSpan nighttimeDifference = (night_time - now).Duration();
                if (daytimeDifference.Hours == 0 && daytimeDifference.Minutes == 0)
                {
                    if (daytimeDifference.Seconds < 4)
                    {
                        writeLogBeforeClear();
                        //计数清零
                        this.BeginInvoke(new Action(() =>
                        {
                            clearTotalNum();
                            cleartowGirdViewData();
                        }));
                    }
                }
                else if (nighttimeDifference.Hours == 0 && nighttimeDifference.Minutes == 0)
                {
                    if (nighttimeDifference.Seconds < 4)
                    {
                        writeLogBeforeClear();
                        //计数清零
                        this.BeginInvoke(new Action(() =>
                        {
                            clearTotalNum();
                            cleartowGirdViewData();
                        }));
                    }
                }
            }

            //图片压缩
            //DateTime nowTime = DateTime.Now;
            //if (nowTime.Hour == 16 && nowTime.Minute == 07 && nowTime.Second == 0) //上午4点
            //{
            //    try
            //    {
            //        // 获取当前工作目录
            //        string currentDirectory = Directory.GetCurrentDirectory();

            //        // 创建Process对象
            //        using (Process process = new Process())
            //        {

            //            // 设置进程信息
            //            //@"E:\YUNTIANVision\YUNTIANVision\bin\x64\Debug\PicturePress\图片自动压缩.exe";
            //            process.StartInfo.FileName = @".\PicturePress\图片自动压缩.exe";//Path.Combine(currentDirectory, "PictruePress", "图片自动压缩.exe");

            //            process.StartInfo.UseShellExecute = false;

            //            //@"E:\YUNTIANVision\YUNTIANVision\bin\x64\Debug\PicturePress";
            //            process.StartInfo.WorkingDirectory = @".\PicturePress";//Path.Combine(currentDirectory, "PictruePress");

            //            process.StartInfo.CreateNoWindow = true;

            //            //process.StartInfo.Verb = "runas"; //管理员权限

            //            // 启动进程，不等待执行完毕
            //            process.Start();
            //        }

            //        if (tbLog1.IsHandleCreated)
            //            tbLog1.BeginInvoke(GetMyDelegateLog, tbLog1, "开始图片压缩...", null);
            //    }
            //    catch (Exception ex)
            //    {
            //        if (tbLog1.IsHandleCreated)
            //            tbLog1.BeginInvoke(GetMyDelegateLog, tbLog1, ex.Message, ex);
            //    }
            //}
        }
        static bool IsNumber(string input)
        {
            Regex regex = new Regex(@"^[0-9]+$");
            return regex.IsMatch(input);
        }

        private void runGrabOneCameraThread()
        {
            while (isRunning)
            {
                // 等待线程plc监控线程触发
                event1.Wait();
                HTCSharpDemo.Program.ImageHt imageHt = new HTCSharpDemo.Program.ImageHt();
                MyCamera.MV_FRAME_OUT stFrameOut = new MyCamera.MV_FRAME_OUT();
                try
                {
                    if (isRunning)
                    {

                        DateTime startTime = DateTime.Now;
                        HTCSharpDemo.Program DeepLearning = new HTCSharpDemo.Program();
                        // 非图片触发
                        if (!trigerDLuseImage_isReady)
                        {
                            Thread.Sleep(delayTime_leftwindow);
                            tbLog1.BeginInvoke(GetMyDelegateLog, tbLog1, "相机1收到拍照信号...", null);
                            tbLog1.BeginInvoke(GetMyDelegateLog, tbLog1, "相机1开始拍照...",null);
                            getImgAndDisply(cameraList[cameraIndex % 2], ref hWindowControl1, out ho_image1, ref stFrameOut,HWindow_Location.LeftWindow);
                            ConvertToChannelImageIntPtr(ho_image1, out imageHt);
                            tbLog1.BeginInvoke(GetMyDelegateLog, tbLog1, "相机1拍照结束...", null);
                        }
                        else
                        {
                            if (rotAngel_left != 0)
                            {
                                HTuple angle = new HTuple(rotAngel_left);
                                HOperatorSet.RotateImage(ho_image1, out ho_image1, angle, "constant");
                            }
                            ConvertToChannelImageIntPtr(ho_image1, out imageHt);
                            HTuple hv_Width = new HTuple();
                            HTuple hv_Height = new HTuple();
                            HOperatorSet.GetImageSize(ho_image1, out hv_Width, out hv_Height);
                            /// <summary>图片左上角row坐标</summary>
                            double row1;
                            /// <summary>图片左上角column坐标</summary>
                            double column1;
                            /// <summary>图片右下角row坐标 </summary>
                            double row2;
                            /// <summary>图片右下角column坐标</summary>
                            double column2;
                            HalconHelper.imageLocation(hv_Width, hv_Height, hWindowControl1.Width, hWindowControl1.Height, out row1, out column1, out row2, out column2);
                            hv_Width.Dispose(); 
                            hv_Height.Dispose();
                            if (!isMiniSizeState)
                                HOperatorSet.SetPart(this.hWindowControl1.HalconWindow, row1, column1, row2, column2);
                            HOperatorSet.ClearWindow(this.hWindowControl1.HalconWindow);
                            HOperatorSet.DispObj(ho_image1, this.hWindowControl1.HalconWindow);
                        }

                        tbLog1.BeginInvoke(GetMyDelegateLog, tbLog1, "深度学习处理开始...", null);
                        Dictionary<int, NodeInfo> dic_nodeIdWithNodeInfo = treeFileHelper1.dic_nodeIdWithNodeInfo;
                        List<string> detectedNGItemsNGtype = DeepLearning.DeepStudy1(ptr1, imageHt, dic_nodeIdWithNodeInfo.Count, ref dic_nodeIdWithNodeInfo, list_OKType_classId_left);
                        tbLog1.BeginInvoke(GetMyDelegateLog, tbLog1, "深度学习处理结束...", null);
                        AllNum1++;
                        IniHelper.SaveSetIni.Write("相机1生产信息", "生产总数", AllNum1.ToString());

                        string result1 = "";
                        //需要定位
                        if (treeFileHelper1.dingweiNodeId != int.MinValue)//需要启用定位
                        {
                            if (DeepLearning.res1.ContainsKey(treeFileHelper1.dingweiNodeId) && DeepLearning.res1[treeFileHelper1.dingweiNodeId].Count == dingwei_num_left) //定位到了
                            {
                                bool hasNoNG = true;
                                foreach (var result in DeepLearning.res1)
                                {
                                    if (result.Key != treeFileHelper1.dingweiNodeId)
                                    {
                                        List<result> results = result.Value;
                                        foreach (result r in results)
                                        {
                                            //手动过滤的节点
                                            if (list_OKType_classId_left.Contains(r.class_id))
                                                continue;
                                            if (r.class_id.Equals("OK") || r.class_id.Equals("ok") || r.class_id.Equals("Ok"))
                                                continue;
                                            if (r.class_id.Equals("其他"))
                                                continue;

                                            NGNum1++;
                                            result1 = "NG";
                                            IniHelper.SaveSetIni.Write("相机1生产信息", "NG总数", NGNum1.ToString());
                                            if (!trigerDLuseImage_isReady)
                                                tellPLCResult1(results[0].class_id,result1);
                                            hasNoNG = false;
                                            break;
                                        }
                                        if (!hasNoNG)
                                        {
                                            break;
                                        }
                                    }
                                }
                                if (hasNoNG)
                                {
                                    // 没有NG
                                    OKNum1++;
                                    result1 = "OK";
                                    IniHelper.SaveSetIni.Write("相机1生产信息", "OK总数", OKNum1.ToString());
                                    if (!trigerDLuseImage_isReady)
                                        tellPLCResult1("",result1);
                                }
                            }
                            else //定位失败
                            {
                                NGNum1++;
                                result1 = "定位失败";
                                IniHelper.SaveSetIni.Write("相机1生产信息", "NG总数", NGNum1.ToString());
                                //定位节点特殊，NGTypeSignal.txt 中NGType为"1"
                                if (!trigerDLuseImage_isReady)
                                    tellPLCResult1("1", result1);
                            }
                        }
                        else
                        {
                            bool hasNoNG = true;
                            foreach (var result in DeepLearning.res1)
                            {
                                if (result.Key != treeFileHelper1.dingweiNodeId)
                                {
                                    List<result> results = result.Value;
                                    foreach (result r in results)
                                    {
                                        if (list_OKType_classId_left.Contains(r.class_id))
                                            continue;
                                        if (r.class_id.Equals("OK") || r.class_id.Equals("ok")
                                            || r.class_id.Equals("Ok"))
                                            continue;
                                        if (r.class_id.Equals("其他"))
                                            continue;

                                        NGNum1++;
                                        result1 = "NG";
                                        IniHelper.SaveSetIni.Write("相机1生产信息", "NG总数", NGNum1.ToString());
                                        if (!trigerDLuseImage_isReady)
                                            tellPLCResult1(results[0].class_id, result1);
                                        hasNoNG = false;
                                        break;
                                    }
                                    if (!hasNoNG)
                                    {
                                        break;
                                    }
                                }
                            }
                            if (hasNoNG)
                            {
                                // 没有NG
                                OKNum1++;
                                result1 = "OK";
                                IniHelper.SaveSetIni.Write("相机1生产信息", "OK总数", OKNum1.ToString());
                                if (!trigerDLuseImage_isReady)
                                    tellPLCResult1("", result1);
                            }
                        }

                        showNGLocation1(DeepLearning.res1,result1);

                        DateTime endTime = DateTime.Now;
                        TimeSpan ct = endTime - startTime;
                        ct1 = (int)ct.TotalMilliseconds;

                        tbLog1.BeginInvoke(GetMyDelegateLog, tbLog1, "正在输出相机1结果...", null);
                        updatTotalData1(result1);
                        updateGridViewData1(DeepLearning.res1,result1);
                        tbLog1.BeginInvoke(GetMyDelegateLog, tbLog1, "相机1结果输出完成...", null);

                        string okStatus = IniHelper.SaveSetIni.Read("图片路径", "OK图片标志");
                        string ngStatus = IniHelper.SaveSetIni.Read("图片路径", "NG图片标志");
                        if (okStatus == "T" || ngStatus == "T")
                        {
                            tbLog1.BeginInvoke(GetMyDelegateLog, tbLog1, "正在保存图片...",null);
                            saveImage1(okStatus, ngStatus, result1, detectedNGItemsNGtype);
                        }   
                    }
                }
                catch(Exception ex)
                {
                    tbLog1.BeginInvoke(GetMyDelegateLog, tbLog1, "相机1运行错误日志:"+ex.Message, ex);
                    if (ex.Message.Contains("4056"))
                    {
                        tbLog1.BeginInvoke(GetMyDelegateLog, tbLog1, "相机1取图失败,请检查..." , null);
                    }
                }
                finally
                {
                    if (!trigerDLuseImage_isReady)
                    {
                        isImage = false;
                        if((cameraIndex % 2) < cameraList.Count)
                            cameraList[cameraIndex % 2].MV_CC_FreeImageBuffer_NET(ref stFrameOut);
                        if (imageHt.data != IntPtr.Zero)
                        {
                            Marshal.FreeHGlobal(imageHt.data);
                            imageHt.data = IntPtr.Zero;
                        }
                    }
                    else
                    {
                        isImage = false;
                        if (imageHt.data != IntPtr.Zero)
                        {
                            Marshal.FreeHGlobal(imageHt.data);
                            imageHt.data = IntPtr.Zero;
                        }
                    }

                    // 通知线程plc线程完成
                    event1.Reset();
                    countdown.Signal();
                }
            }
        }

        private void rungrabTwoCameraThread()
        {
            while (isRunning)
            {
                // 等待plc线程触发
                event2.Wait();
                HTCSharpDemo.Program.ImageHt imageHt = new HTCSharpDemo.Program.ImageHt();
                MyCamera.MV_FRAME_OUT stFrameOut = new MyCamera.MV_FRAME_OUT();
                try
                {
                    if (isRunning)
                    {
                        DateTime startTime = DateTime.Now;
                        HTCSharpDemo.Program DeepLearning = new HTCSharpDemo.Program();
                        // 非图片触发
                        if (!trigerDLuseImage_isReady)
                        {
                            Thread.Sleep(delayTime_rightwindow);
                            tbLog2.BeginInvoke(GetMyDelegateLog, tbLog2, "相机2收到拍照信号...", null);
                            tbLog2.BeginInvoke(GetMyDelegateLog, tbLog2, "相机2开始拍照...", null);
                            getImgAndDisply(cameraList[(cameraIndex+1) % 2], ref hWindowControl2, out ho_image2, ref stFrameOut,HWindow_Location.RightWindow);
                            ConvertToChannelImageIntPtr(ho_image2, out imageHt);
                            tbLog2.BeginInvoke(GetMyDelegateLog, tbLog2, "相机2拍照结束...", null);
                        }
                        else
                        {
                            if (rotAngel_right != 0)
                            {
                                HTuple angle = new HTuple(rotAngel_right);
                                HOperatorSet.RotateImage(ho_image2, out ho_image2, angle, "constant");
                            }
                            tbLog2.BeginInvoke(GetMyDelegateLog, tbLog2, "从目录中取图执行开始...", null);
                            ConvertToChannelImageIntPtr(ho_image2, out imageHt);
                            hv_Width2.Dispose(); hv_Height2.Dispose();
                            HOperatorSet.GetImageSize(ho_image2, out hv_Width2, out hv_Height2);
                            HalconHelper.imageLocation(hv_Width2, hv_Height2, hWindowControl2.Width, hWindowControl2.Height, out row3, out column3, out row4, out column4);
                            if (!isMiniSizeState)
                                HOperatorSet.SetPart(this.hWindowControl2.HalconWindow, row3, column3, row4, column4);
                            HOperatorSet.ClearWindow(this.hWindowControl2.HalconWindow);
                            HOperatorSet.DispObj(ho_image2, this.hWindowControl2.HalconWindow);
                        }

                        tbLog2.BeginInvoke(GetMyDelegateLog, tbLog2, "深度学习处理开始...", null);
                        Dictionary<int, NodeInfo> dic_nodeIdWithNodeInfo = treeFileHelper2.dic_nodeIdWithNodeInfo;
                        List<string> detectedNGItemsNGtype = DeepLearning.DeepStudy2(ptr2, imageHt, dic_nodeIdWithNodeInfo.Count, ref dic_nodeIdWithNodeInfo, list_OKType_classId_right);
                        tbLog2.BeginInvoke(GetMyDelegateLog, tbLog2, "深度学习处理完成...", null);
                        AllNum2++;
                        IniHelper.SaveSetIni.Write("相机2生产信息", "生产总数", AllNum2.ToString());

                        string result2 = "";
                        if (treeFileHelper2.dingweiNodeId != int.MinValue)//需要启用定位
                        {
                            if (DeepLearning.res2.ContainsKey(treeFileHelper2.dingweiNodeId) && DeepLearning.res2[treeFileHelper2.dingweiNodeId].Count == dingwei_num_right) //定位到了
                            {
                                bool hasNoNG = true;
                                foreach (var result in DeepLearning.res2)
                                {
                                    if (result.Key != treeFileHelper2.dingweiNodeId)
                                    {
                                        List<result> results = result.Value;
                                        foreach (result r in results)
                                        {
                                            if (list_OKType_classId_right.Contains(r.class_id))
                                                continue;
                                            if (r.class_id.Equals("OK") || r.class_id.Equals("ok") || r.class_id.Equals("Ok"))
                                                continue;
                                            if (r.class_id.Equals("其他"))
                                                continue;

                                            NGNum2++;
                                            result2 = "NG";
                                            IniHelper.SaveSetIni.Write("相机2生产信息", "NG总数", NGNum1.ToString());
                                            if (!trigerDLuseImage_isReady)
                                                tellPLCResult2(r.class_id,result2);
                                            hasNoNG = false;
                                            break;
                                        }
                                        if (!hasNoNG)
                                        {
                                            break;
                                        }
                                    }
                                }
                                if (hasNoNG)
                                {
                                    // 没有NG
                                    OKNum2++;
                                    result2 = "OK";
                                    IniHelper.SaveSetIni.Write("相机2生产信息", "OK总数", OKNum2.ToString());
                                    if (!trigerDLuseImage_isReady)
                                        tellPLCResult2("",result2);
                                }
                            }
                            else //定位失败
                            {
                                NGNum2++;
                                result2 = "定位失败";
                                IniHelper.SaveSetIni.Write("相机1生产信息", "NG总数", NGNum2.ToString());
                                //定位节点特殊，NGTypeSignal.txt 中NGType为"1"
                                if (!trigerDLuseImage_isReady)
                                    tellPLCResult2("1",result2);
                            }
                        }
                        else
                        {
                            bool hasNoNG = true;
                            foreach (var result in DeepLearning.res2)
                            {
                                if (result.Key != treeFileHelper2.dingweiNodeId)
                                {
                                    List<result> results = result.Value;
                                    foreach (result r in results)
                                    {
                                        if (list_OKType_classId_right.Contains(r.class_id))
                                            continue;
                                        if (r.class_id.Equals("OK") || r.class_id.Equals("ok")
                                            || r.class_id.Equals("Ok"))
                                            continue;
                                        if (r.class_id.Equals("其他"))
                                            continue;

                                        NGNum2++;
                                        result2 = "NG";
                                        IniHelper.SaveSetIni.Write("相机2生产信息", "NG总数", NGNum1.ToString());
                                        if (!trigerDLuseImage_isReady)
                                            tellPLCResult2(r.class_id, result2);
                                        hasNoNG = false;
                                        break;
                                    }
                                    if (!hasNoNG)
                                    {
                                        break;
                                    }
                                }
                            }
                            if (hasNoNG)
                            {
                                // 没有NG
                                OKNum2++;
                                result2 = "OK";
                                IniHelper.SaveSetIni.Write("相机2生产信息", "OK总数", OKNum2.ToString());
                                if (!trigerDLuseImage_isReady)
                                    tellPLCResult2("", result2);
                            }
                        }
                        showNGLocation2(DeepLearning.res2,result2);
                        //showRes2(result2);

                        DateTime endTime = DateTime.Now;
                        TimeSpan ct = endTime - startTime;
                        ct2 = (int)ct.TotalMilliseconds;
                        if (!trigerDLuseImage_isReady)
                            tbLog2.BeginInvoke(GetMyDelegateLog, tbLog2, "正在输出相机2结果...", null);
                        else
                            tbLog2.BeginInvoke(GetMyDelegateLog, tbLog2, "正在输出处理结果...", null);

                        updatTotalData2(result2);
                        updateGridViewData2(DeepLearning.res2,result2);

                        if (!trigerDLuseImage_isReady)
                            tbLog2.BeginInvoke(GetMyDelegateLog, tbLog2, "相机2结果输出完成...", null);
                        else
                            tbLog2.BeginInvoke(GetMyDelegateLog, tbLog2, "结果输出完成...", null);
                        string okStatus = IniHelper.SaveSetIni.Read("图片路径", "OK图片标志");
                        string ngStatus = IniHelper.SaveSetIni.Read("图片路径", "NG图片标志");
                        if (okStatus == "T" || ngStatus == "T")
                        {
                            tbLog2.BeginInvoke(GetMyDelegateLog, tbLog2, "正在保存图片...", null);
                            saveImage2(okStatus, ngStatus, result2, detectedNGItemsNGtype);
                        }

                        
                    }
                }catch(Exception ex)
                {
                    tbLog2.BeginInvoke(GetMyDelegateLog, tbLog2,"相机2运行错误日志:"+ex.Message, ex);
                    if (ex.Message.Contains("4056"))
                    {
                        tbLog2.BeginInvoke(GetMyDelegateLog, tbLog2, "相机2取图失败,请检查...", null);
                    }
                }
                finally
                {
                    if (!trigerDLuseImage_isReady)
                    {
                        isImage = false;
                        if(((cameraIndex + 1) % 2) < cameraList.Count)
                            cameraList[(cameraIndex + 1) % 2].MV_CC_FreeImageBuffer_NET(ref stFrameOut);
                        // 内存管理
                        if (imageHt.data != IntPtr.Zero)
                        {
                            Marshal.FreeHGlobal(imageHt.data);
                            imageHt.data = IntPtr.Zero;
                        }
                    }
                    else
                    {
                        isImage = false;
                        // 内存管理
                        if (imageHt.data != IntPtr.Zero)
                        {
                            Marshal.FreeHGlobal(imageHt.data);
                            imageHt.data = IntPtr.Zero;
                        }
                    }
                    event2.Reset();
                    // 通知plc线程完成
                    if(!hasTwoSignal) //单信号
                        countdown.Signal();
                    else  //双信号
                        countdownTwo.Signal();
                }    
             }
        }
        #endregion

        #region 功能设置
        // 加载模型
        private bool loadDlModel(bool modlePath1isNull, bool modlePath2isNull)
        {
            if (!modlePath1isNull) {
                Dictionary<int, NodeInfo> dic_nodeIdWithNodeInfo = treeFileHelper1.dic_nodeIdWithNodeInfo;
                HTCSharpDemo.Program.loadDeepStudyHandle(configPath, ref studyHandle,ref dic_nodeIdWithNodeInfo);
                ptr1 = studyHandle;
            }
            if (!modlePath2isNull)
            {
                Dictionary<int, NodeInfo> dic_nodeIdWithNodeInfo = treeFileHelper2.dic_nodeIdWithNodeInfo;
                HTCSharpDemo.Program.loadDeepStudyHandle(configPath2, ref studyHandle2, ref dic_nodeIdWithNodeInfo);
                ptr2 = studyHandle2;
            }
            else if (!modlePath1isNull)
            {
                configPath2 = configPath;
                studyHandle2 = studyHandle;
                treeFileHelper2 = treeFileHelper1;
                ptr2 = ptr1;
            }
            else
                return false;
            loadCom1();
            return true;
        }
        #endregion

        #region    海康相机1变量
        
        /// <summary>
        /// 相机1拍到的图像变量
        /// </summary>
        HObject ho_image1;
        ///// <summary>
        ///// ho_image1的宽
        ///// </summary>
        //HTuple hv_Width1 = new HTuple();
        ///// <summary>
        ///// ho_image1的高
        ///// </summary>
        //HTuple hv_Height1 = new HTuple();
        int nRet1 = MyCamera.MV_OK;

        private volatile bool isRunning = true;

        /// <summary>
        /// 是否是双模型
        /// </summary>
        bool hasTwoModel = false;

        #endregion 海康相机1变量

        #region 海康相机1SDK
        private  void openOneHikVisionCam()
        {
            try
            {
                // ch:枚举设备 | en:Enum device
                MyCamera.MV_CC_DEVICE_INFO_LIST stDevList = new MyCamera.MV_CC_DEVICE_INFO_LIST();
                nRet1 = MyCamera.MV_CC_EnumDevices_NET(MyCamera.MV_GIGE_DEVICE | MyCamera.MV_USB_DEVICE, ref stDevList);
                frmMain.cameraNum = (int)stDevList.nDeviceNum;
                if (frmMain.cameraNum < 2)
                    return;
                for(int i = 0; i< stDevList.nDeviceNum; i++)
                {
                    MyCamera.MV_CC_DEVICE_INFO stDevInfo = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(stDevList.pDeviceInfo[i], typeof(MyCamera.MV_CC_DEVICE_INFO));

                    MyCamera device = new MyCamera();

                    // ch:创建设备 | en:Create device
                    nRet1 = device.MV_CC_CreateDevice_NET(ref stDevInfo) + nRet1;

                    // ch:打开设备 | en:Open device
                    nRet1 = device.MV_CC_OpenDevice_NET() + nRet1;

                    //关闭触发模式
                    nRet1 = device.MV_CC_SetEnumValue_NET("TriggerMode", 0);

                    int temp = i + 1;
                    string setName = "相机" + temp + "设置";
                    string exposeTime = IniHelper.SaveSetIni.Read(setName, "曝光量");
                    int exposureTime = int.MinValue;
                    if (string.IsNullOrEmpty(exposeTime))
                    {
                        IniHelper.SaveSetIni.Write(setName, "曝光量", "500");
                        exposureTime = 500;
                    }
                    else
                    {
                        try
                        {
                            exposureTime = int.Parse(exposeTime);
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    // ch:设置曝光模式为手动 | en:Set exposure mode to manual
                    nRet1 = device.MV_CC_SetEnumValue_NET("ExposureMode", 1);

                    // ch:设置曝光时间为5000微秒(5毫秒)

                    nRet1 = device.MV_CC_SetFloatValue_NET("ExposureTime", exposureTime);

                    // ch:开启抓图 | en:start grab
                    nRet1 = device.MV_CC_StartGrabbing_NET() + nRet1;

                    if (MyCamera.MV_OK == nRet1)
                    {
                        cameraList.Add(device);
                    }
                    
                    if (MyCamera.MV_OK == nRet1 && tbLog1.IsHandleCreated)
                    {
                        tbLog1.BeginInvoke(GetMyDelegateLog, tbLog1, "相机"+i+"初始化完成...", null);
                    }
                    else if (tbLog1.IsHandleCreated)
                        tbLog1.BeginInvoke(GetMyDelegateLog, tbLog1, "相机"+i+"初始化失败...", null);
                }
                if (cameraList.Count < 2)
                {
                    tbLog1.BeginInvoke(GetMyDelegateLog, tbLog1, "相机未准备好", null);
                    return;
                }
            }
            catch (Exception ex)
            {
                if(tbLog1.IsHandleCreated)
                    tbLog1.BeginInvoke(GetMyDelegateLog, tbLog1, "相机初始化失败:" + ex.Message,ex);
            }
        }

        private void closeOneHikVisionCam()
        {
            try
            {
                for(int i = 0;i < cameraList.Count; i++)
                {
                    MyCamera camera = cameraList[i];
                    // ch:停止抓图 | en:Stop grab image
                    nRet1 = camera.MV_CC_StopGrabbing_NET();
                    // ch:关闭设备 | en:Close device
                    nRet1 = camera.MV_CC_CloseDevice_NET();
                    // ch:销毁设备 | en:Destroy device
                    nRet1 = camera.MV_CC_DestroyDevice_NET();
                } 
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                LogHelper.WriteLog("关闭相机1错误日志:"+ex.Message,ex);
            }
        }
        #endregion   

        #region  海康相机2变量   
        bool m_bTwostart2 = false;
        /// <summary>
        /// 图片左上角row坐标
        /// </summary>
        double row3;
        /// <summary>
        /// 图片左上角column坐标
        /// </summary>
        double column3;
        /// <summary>
        /// 图片右下角row坐标
        /// </summary>
        double row4;
        /// <summary>
        /// 图片右下角column坐标
        /// </summary>
        double column4;

        /// <summary>
        /// 相机2拍到的图像变量
        /// </summary>
        HObject ho_image2;
        /// <summary>
        /// ho_image2的宽
        /// </summary>
        HTuple hv_Width2 = new HTuple();
        /// <summary>
        /// ho_image2的宽
        /// </summary>
        HTuple hv_Height2 = new HTuple();
        int nRet2 = MyCamera.MV_OK;
        MyCamera.MV_CC_DEVICE_INFO stDevInfo2; // 通用设备信息
        MyCamera.MVCC_INTVALUE stParam2;
        MyCamera.MV_FRAME_OUT_INFO_EX FrameInfo2;
        UInt32 nPayloadSize2;
        IntPtr pBufForDriver2;
        #endregion 

        private bool IsColorPixelFormat(MyCamera.MvGvspPixelType enType)
        {
            switch (enType)
            {
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_RGB8_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BGR8_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_RGBA8_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BGRA8_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_YUV422_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_YUV422_YUYV_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGR8:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerRG8:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGB8:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerBG8:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGB10:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGB10_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerBG10:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerBG10_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerRG10:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerRG10_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGR10:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGR10_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGB12:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGB12_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerBG12:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerBG12_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerRG12:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerRG12_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGR12:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGR12_Packed:
                    return true;
                default:
                    return false;
            }
        }
        private bool IsMonoPixelFormat(MyCamera.MvGvspPixelType enType)
        {
            switch (enType)
            {
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono8:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono10:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono10_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono12:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono12_Packed:
                    return true;
                default:
                    return false;
            }
        }
        private void getImgAndDisply(MyCamera device,ref HSmartWindowControl hWindowControl, out HObject image,ref MyCamera.MV_FRAME_OUT stFrameOut, HWindow_Location window_Location)
        {
            HOperatorSet.GenEmptyObj(out image);
            image.Dispose();

            int nRet = MyCamera.MV_OK;

            IntPtr pImageBuf = IntPtr.Zero;
            int nImageBufSize = 0;

            nRet = device.MV_CC_GetImageBuffer_NET(ref stFrameOut, 1000);
            bool isAllocatedMemory = false;
            if (MyCamera.MV_OK == nRet)
            {
                if (IsColorPixelFormat(stFrameOut.stFrameInfo.enPixelType))
                {
                    if (stFrameOut.stFrameInfo.enPixelType == MyCamera.MvGvspPixelType.PixelType_Gvsp_RGB8_Packed)
                    {
                        pImageBuf = stFrameOut.pBufAddr;
                    }
                    else
                    {
                        if (IntPtr.Zero == pImageBuf || nImageBufSize < (stFrameOut.stFrameInfo.nWidth * stFrameOut.stFrameInfo.nHeight * 3))
                        {
                            if (pImageBuf != IntPtr.Zero)
                            {
                                Marshal.FreeHGlobal(pImageBuf);
                                pImageBuf = IntPtr.Zero;
                            }

                            pImageBuf = Marshal.AllocHGlobal((int)stFrameOut.stFrameInfo.nWidth * stFrameOut.stFrameInfo.nHeight * 3);
                            if (IntPtr.Zero == pImageBuf)
                            {
                                return;
                            }
                            nImageBufSize = stFrameOut.stFrameInfo.nWidth * stFrameOut.stFrameInfo.nHeight * 3;
                            // 分配内存了
                            isAllocatedMemory = true;
                        }

                        MyCamera.MV_PIXEL_CONVERT_PARAM stPixelConvertParam = new MyCamera.MV_PIXEL_CONVERT_PARAM();

                        stPixelConvertParam.pSrcData = stFrameOut.pBufAddr;//源数据
                        stPixelConvertParam.nWidth = stFrameOut.stFrameInfo.nWidth;//图像宽度
                        stPixelConvertParam.nHeight = stFrameOut.stFrameInfo.nHeight;//图像高度
                        stPixelConvertParam.enSrcPixelType = stFrameOut.stFrameInfo.enPixelType;//源数据的格式
                        stPixelConvertParam.nSrcDataLen = stFrameOut.stFrameInfo.nFrameLen;

                        stPixelConvertParam.nDstBufferSize = (uint)nImageBufSize;
                        stPixelConvertParam.pDstBuffer = pImageBuf;//转换后的数据
                        stPixelConvertParam.enDstPixelType = MyCamera.MvGvspPixelType.PixelType_Gvsp_RGB8_Packed;
                        nRet = device.MV_CC_ConvertPixelType_NET(ref stPixelConvertParam);//格式转换
                        if (MyCamera.MV_OK != nRet)
                        {
                            return;
                        }
                    }
                    try
                    {
                        HOperatorSet.GenImageInterleaved(out image, (HTuple)pImageBuf, (HTuple)"rgb", (HTuple)stFrameOut.stFrameInfo.nWidth, (HTuple)stFrameOut.stFrameInfo.nHeight, -1, "byte", 0, 0, 0, 0, -1, 0);
                    }
                    catch (Exception ex)
                    {
                        tbLog1.BeginInvoke(GetMyDelegateLog, tbLog1, "相机取图失败:"+ex.Message, ex);
                        MessageBox.Show("相机取*彩色图*失败,请检查");
                        return;
                    }
                }
                else if (IsMonoPixelFormat(stFrameOut.stFrameInfo.enPixelType))
                {
                    if (stFrameOut.stFrameInfo.enPixelType == MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono8)
                    {
                        pImageBuf = stFrameOut.pBufAddr;       
                    }
                    else
                    {
                        if (IntPtr.Zero == pImageBuf || nImageBufSize < (stFrameOut.stFrameInfo.nWidth * stFrameOut.stFrameInfo.nHeight))
                        {
                            if (pImageBuf != IntPtr.Zero)
                            {
                                Marshal.FreeHGlobal(pImageBuf);
                                pImageBuf = IntPtr.Zero;
                            }

                            pImageBuf = Marshal.AllocHGlobal((int)stFrameOut.stFrameInfo.nWidth * stFrameOut.stFrameInfo.nHeight);
                            if (IntPtr.Zero == pImageBuf)
                            {
                                return;
                            }
                            nImageBufSize = stFrameOut.stFrameInfo.nWidth * stFrameOut.stFrameInfo.nHeight;
                            // 分配内存了
                            isAllocatedMemory = true;
                        }

                        MyCamera.MV_PIXEL_CONVERT_PARAM stPixelConvertParam = new MyCamera.MV_PIXEL_CONVERT_PARAM();

                        stPixelConvertParam.pSrcData = stFrameOut.pBufAddr;//源数据
                        stPixelConvertParam.nWidth = stFrameOut.stFrameInfo.nWidth;//图像宽度
                        stPixelConvertParam.nHeight = stFrameOut.stFrameInfo.nHeight;//图像高度
                        stPixelConvertParam.enSrcPixelType = stFrameOut.stFrameInfo.enPixelType;//源数据的格式
                        stPixelConvertParam.nSrcDataLen = stFrameOut.stFrameInfo.nFrameLen;

                        stPixelConvertParam.nDstBufferSize = (uint)nImageBufSize;
                        stPixelConvertParam.pDstBuffer = pImageBuf;//转换后的数据
                        stPixelConvertParam.enDstPixelType = MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono8;
                        nRet = device.MV_CC_ConvertPixelType_NET(ref stPixelConvertParam);//格式转换
                        if (MyCamera.MV_OK != nRet)
                        {
                            return;
                        }
                     }
                    try
                    {
                        HOperatorSet.GenImage1Extern(out image, "byte", stFrameOut.stFrameInfo.nWidth, stFrameOut.stFrameInfo.nHeight, pImageBuf, IntPtr.Zero);
                    }
                    catch (System.Exception ex)
                    {
                        tbLog1.BeginInvoke(GetMyDelegateLog, tbLog1, "相机取图失败:"+ex.Message, ex);
                        MessageBox.Show("相机取*黑白图*失败，请检查");
                        return;
                    }
                }
                else
                {
                    device.MV_CC_FreeImageBuffer_NET(ref stFrameOut);
                }

                if(window_Location == HWindow_Location.LeftWindow)
                {
                    if (rotAngel_left != 0)
                    {
                        HTuple angle = new HTuple(rotAngel_left);
                        HOperatorSet.RotateImage(image, out image, angle, "constant");
                    }
                } else if(window_Location == HWindow_Location.RightWindow)
                {
                    if (rotAngel_right != 0)
                    {
                        HTuple angle = new HTuple(rotAngel_right);
                        HOperatorSet.RotateImage(image, out image, angle, "constant");
                    }
                }
                HTuple imageWidth, imageHeight;
                HOperatorSet.GetImageSize(image, out imageWidth, out imageHeight);
                HalconHelper.imageLocation(imageWidth, imageHeight, hWindowControl.Width, hWindowControl.Height, out row3, out column3, out row4, out column4);
                imageHeight.Dispose();
                imageWidth.Dispose();
                if (!isMiniSizeState)
                    HOperatorSet.SetPart(hWindowControl.HalconWindow, row3, column3, row4, column4);
                HOperatorSet.ClearWindow(hWindowControl.HalconWindow);
                HOperatorSet.DispObj(image, hWindowControl.HalconWindow);
            }
            if (pImageBuf != IntPtr.Zero && isAllocatedMemory)
            {
                Marshal.FreeHGlobal(pImageBuf);
                pImageBuf = IntPtr.Zero;
            }
        }

        private void ConvertToChannelImageIntPtr(HObject image, out HTCSharpDemo.Program.ImageHt imageHt)
        {
            HTuple type, width, height, channels;
            HOperatorSet.GetImageType(image, out type);
            HOperatorSet.GetImageSize(image, out width, out height);
            HOperatorSet.CountChannels(image, out channels);

            imageHt = new HTCSharpDemo.Program.ImageHt();
            if (channels == 3 && type.S == "byte")
            {
                HTuple redPtr, greenPtr, bluePtr;
                HOperatorSet.GetImagePointer3(image, out redPtr, out greenPtr, out bluePtr, out type, out width, out height);

                int imageSize = width.I * height.I;
                byte[] imageData = new byte[imageSize * 3];

                int pixelCount = width * height;
                byte[] red = new byte[pixelCount];
                byte[] green = new byte[pixelCount];
                byte[] blue = new byte[pixelCount];
                Marshal.Copy(redPtr, red, 0, pixelCount);
                Marshal.Copy(greenPtr, green, 0, pixelCount);
                Marshal.Copy(bluePtr, blue, 0, pixelCount);
                for (int i = 0; i < pixelCount; i++)
                {
                    imageData[i * 3] = blue[i];
                    imageData[i * 3 + 1] = green[i];
                    imageData[i * 3 + 2] = red[i];
                }

                // 将数组转换为IntPtr
                IntPtr imageDataIntPtr = Marshal.AllocHGlobal(imageData.Length);
                Marshal.Copy(imageData, 0, imageDataIntPtr, imageData.Length);

                imageHt.data = imageDataIntPtr;
                imageHt.width = width.I;
                imageHt.height = height.I;
                imageHt.channels = 3;
                imageHt.width_step = imageHt.width * 3;
            } else if (channels == 1 && type.S == "byte")
            {
                HTuple imagePtr;
                HOperatorSet.GetImagePointer1(image, out imagePtr, out type,out width,out height);

                int imageSize = width.I * height.I;
                byte[] imageData = new byte[imageSize];

                // 将单通道数据复制到数组中
                Marshal.Copy(imagePtr, imageData, 0, imageSize);

                // 将数组转换为IntPtr
                IntPtr imageDataIntPtr = Marshal.AllocHGlobal(imageData.Length);
                Marshal.Copy(imageData, 0, imageDataIntPtr, imageData.Length);

                imageHt.data = imageDataIntPtr;
                imageHt.width = width.I;
                imageHt.height = height.I;
                imageHt.channels = 1;
                imageHt.width_step = imageHt.width;
            } else
            {
                throw new Exception("Halcon转换的图像格式不正确");
            }
        }

        #region 委托

        private void updateDV(int index)
        {
            if (index == 3)
            {
                // 3.清空datagridView
                if (dataGridView_left.Rows.Count > 0)
                {
                    dataGridView_left.Rows.Clear();
                }
                // 4. 添加新模型节点
                addRows(treeFileHelper1,3,list_OKType_classId_left);
            }
            else if (index == 4)
            {
                // 3.清空datagridView
                if (dataGridView_right.Rows.Count > 0)
                {
                    dataGridView_right.Rows.Clear();
                }
                // 4. 添加新模型节点
                addRows(treeFileHelper2,4,list_OKType_classId_right);
            }
        }

        // 委托显示相机日志
        private void showLog(TextBox textBox, string log, Exception ex)
        {
            if(textBox == tbLog1)
            {
                // 添加新的日志行
                if (ex != null)
                {
                    logLines_left.AddLast(DateTime.Now.ToString("yyyy/MM/dd-HH:mm:ss:fff") + ":" + log + ex.Message + "\r\n");
                    LogHelper.WriteLog(log, ex);
                }
                else
                {
                    logLines_left.AddLast(DateTime.Now.ToString("yyyy/MM/dd-HH:mm:ss:fff") + ":" + log + "\r\n");
                    LogHelper.WriteLog(log);
                }
                // 如果超过最大日志条数，移除最早的日志行
                if (logLines_left.Count > maxLogLines)
                {
                    logLines_left.RemoveFirst();
                }
                tbLog1.Text = string.Join(Environment.NewLine, logLines_left);
                tbLog1.SelectionStart = tbLog1.TextLength;
                tbLog1.ScrollToCaret();
                tbLog1.Refresh();
            }
            else if(textBox == tbLog2)
            {
                if (ex != null)
                {
                    logLines_right.AddLast(DateTime.Now.ToString("yyyy/MM/dd-HH:mm:ss:fff") + ":" + log + ex.Message + "\r\n");
                    LogHelper.WriteLog(log, ex);
                }
                else
                {
                    logLines_right.AddLast(DateTime.Now.ToString("yyyy/MM/dd-HH:mm:ss:fff") + ":" + log + "\r\n");
                    LogHelper.WriteLog(log);
                }
                // 如果超过最大日志条数，移除最早的日志行
                if (logLines_right.Count > maxLogLines)
                {
                    logLines_right.RemoveFirst();
                }
                tbLog2.Text = string.Join(Environment.NewLine, logLines_right);
                tbLog2.SelectionStart = tbLog2.TextLength;
                tbLog2.ScrollToCaret();
                tbLog2.Refresh();
            }
            //textBox.AppendText(DateTime.Now.ToString("yyyy/MM/dd-HH:mm:ss:fff") + ":" + log + "\r\n");
            //if (ex == null)
            //    LogHelper.WriteLog(log);
            //else
            //    LogHelper.WriteLog(log, ex);
            //textBox.SelectionStart = tbLog1.TextLength;
            //textBox.ScrollToCaret();
            //textBox.SelectionStart = tbLog2.TextLength;
            //textBox.ScrollToCaret();
        }

       // 委托显示通讯状态
        private void showPlcStatus(Label label, string status, System.Drawing.Color color)
        {
            label.BackColor = color;
            label.Text = status;
        }
        #endregion

        /// <summary>
        /// 计数定时清零
        /// </summary>
        void countClearanceSetting(bool setNGText)
        {
            LogHelper.WriteLog($"已清空历史数据,总数:{lbAllNum1.Text};OK数:{lbOKNum1.Text};NG数:{lbNGNum1.Text};良率:{lbOKPercent1.Text}");
            lbAllNum1.Text = "0";
            AllNum1 = 0;
            lbOKNum1.Text = "0";
            OKNum1 = 0;
            lbNGNum1.Text = "0";
            NGNum1 = 0;
            lbOKPercent1.Text = "0";
            lbCt1.Text = "0";
            if (setNGText)
            {
                lbResult1.BackColor = System.Drawing.Color.Red;
                lbResult1.Text = "NG";
            }
            IniHelper.SaveSetIni.Write("相机1生产信息", "生产总数", "0");
            IniHelper.SaveSetIni.Write("相机1生产信息", "OK总数", "0");
            IniHelper.SaveSetIni.Write("相机1生产信息", "NG总数", "0");
            LogHelper.WriteLog($"已清空历史数据,总数:{lbAllNum2.Text};OK数:{lbOKNum2.Text};NG数:{lbNGNum2.Text};良率:{lbOKPercent2.Text}");
            lbAllNum2.Text = "0";
            AllNum2 = 0;
            lbOKNum2.Text = "0";
            OKNum2 = 0;
            lbNGNum2.Text = "0";
            NGNum2 = 0;
            lbOKPercent2.Text = "0";
            lbCt2.Text = "0";
            if (setNGText)
            {
                lbResult2.BackColor = System.Drawing.Color.Red;
                lbResult2.Text = "NG";
            }
            IniHelper.SaveSetIni.Write("相机2生产信息", "生产总数", "0");
            IniHelper.SaveSetIni.Write("相机2生产信息", "OK总数", "0");
            IniHelper.SaveSetIni.Write("相机2生产信息", "NG总数", "0");
            dataGridView_left.Rows.Clear();
            dataGridView_right.Rows.Clear();
            if (!String.IsNullOrEmpty(configPath) && dataGridView_left.Rows.Count < 2 && dataGridView_right.Rows.Count < 2)
            {
                addRows(treeFileHelper1,3,list_OKType_classId_left);
                if (!string.IsNullOrEmpty(configPath2) && !configPath2.Equals(configPath))
                {
                    addRows(treeFileHelper2, 4,list_OKType_classId_right);
                } else
                {
                    addRows(treeFileHelper2, 4,list_OKType_classId_right);
                }   
            }
        }

        #region 相机1读取图片
        private void 读取图片ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                this.openFileDialog1.Filter = "BMP图片|*.BMP|所有图片|*.*";
                if (this.openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    int winWidth = hWindowControl1.Width;
                    int winHeight = hWindowControl1.Height;
                    hWindowControl1.HalconWindow.ClearWindow();
                    HOperatorSet.ReadImage(out ho_image1, this.openFileDialog1.FileName);
                    HTuple hv_Width = new HTuple();
                    HTuple hv_Height = new HTuple();
                    HOperatorSet.GetImageSize(ho_image1, out hv_Width, out hv_Height);
                    /// <summary>图片左上角row坐标</summary>
                    double row1;
                    /// <summary>图片左上角column坐标</summary>
                    double column1;
                    /// <summary>图片右下角row坐标 </summary>
                    double row2;
                    /// <summary>图片右下角column坐标</summary>
                    double column2;
                    HalconHelper.imageLocation(hv_Width, hv_Height, winWidth, winHeight, out row1, out column1, out row2, out column2);
                    hv_Width.Dispose();
                    hv_Height.Dispose();
                    if (!isMiniSizeState)
                        HOperatorSet.SetPart(this.hWindowControl1.HalconWindow, row1, column1, row2, column2);
                    HOperatorSet.DispObj(ho_image1, this.hWindowControl1.HalconWindow);
                }
            }
            catch (Exception ex)
            {
                ho_image1.Dispose();
                MessageBox.Show("读图失败");
                LogHelper.WriteLog("错误日志记录"+ex.Message, ex);
            }
        }
        #endregion

        #region 串口1通讯
        private void btOpenPlc_Click(object sender, EventArgs e)
        {
            if (!panasonicMewtocol1.IsOpen())
                openSerilPort();
        }
        private void btClosePlc_Click(object sender, EventArgs e)
        {
            panasonicMewtocol1.Write(tbReadySignal1.Text, false);
            panasonicMewtocol1?.Close();
            controlCom1(true);
            controlSignal1(true);
            showLog(tbLog1, "串口1断开通讯...",null);
            btOpenPlc1.Enabled = true;
            btClosePlc1.Enabled = false;
            btAddNGSet1.Enabled = true;
            contextMenuStrip1.Enabled = false;
        }

        private bool loadCom1()
        {
            if (!Directory.Exists(IniHelper.SaveSetIni.Read("图片设置", "保存路径")))
            {
                if (tbLog1.IsHandleCreated)
                    tbLog1.BeginInvoke(GetMyDelegateLog, tbLog1, "图片保存路径未设置", null);
                return false;
            }
            if(!File.Exists(IniHelper.SaveSetIni.Read("深度学习文件", "配置文件路径"))){
                if (tbLog1.IsHandleCreated)
                    tbLog1.BeginInvoke(GetMyDelegateLog, tbLog1, "深度学习配置文件路径未设置", null);
                return false;
            }
            if (String.IsNullOrEmpty(tbOKSignal1.Text) || String.IsNullOrEmpty(tbReadySignal1.Text) || String.IsNullOrEmpty(tbGrabImageSignal1.Text))
            {
                if (tbLog1.IsHandleCreated)
                    tbLog1.BeginInvoke(GetMyDelegateLog, tbLog1, "请先输入PLC地址", null);
                return false;
            }
            if (dic_defectClassificationswithPLCaddress_left.Count <= 0)
            {
                if (tbLog1.IsHandleCreated)
                    tbLog1.BeginInvoke(GetMyDelegateLog, tbLog1, "请至少输入一个NG信号地址", null);
                return false;
            }
            OperateResult result1 = panasonicMewtocol1.Write(tbReadySignal1.Text, true);
            if (!result1.IsSuccess)
            {
                if (tbLog1.IsHandleCreated)
                {
                    tbLog1.BeginInvoke(GetMyDelegateLog, tbLog1, "准备信号发送失败，请检查串口", null);
                }
                return false;
            }
            return true;
        }
        private void startCom1()
        {
            tbLog1.BeginInvoke(GetMyDelegateLog, tbLog1, "相机1已就绪...", null);
            IniHelper.SaveSetIni.Write("相机1PLC设置", "拍照信号", tbGrabImageSignal1.Text);
            IniHelper.SaveSetIni.Write("相机1PLC设置", "OK信号", tbOKSignal1.Text);
            IniHelper.SaveSetIni.Write("相机1PLC设置", "准备信号", tbReadySignal1.Text);
            if (cbEnableCode.Checked)
            {
                IniHelper.SaveSetIni.Write("双相机读码设置", "读码地址", tbCodeAddress.Text);
                IniHelper.SaveSetIni.Write("双相机读码设置", "读码长度", tbCodeLength.Text);
            }
            btClosePlc1.Enabled = false;
            cbEnableCode.Enabled = false;
            controlSignal1(false);
        }
        private void openSerilPort()
        {
            try
            {
                panasonicMewtocol1?.Close();
                panasonicMewtocol1.SerialPortInni(sp =>
                {
                    sp.PortName = cbPortName1.Text;
                    sp.BaudRate = Convert.ToInt32(tbBaudRate1.Text);
                    sp.DataBits = Convert.ToInt32(tbDateBits1.Text); ;
                    sp.StopBits = Convert.ToInt32(tbStopBits1.Text) == 0 ? System.IO.Ports.StopBits.None : (Convert.ToInt32(tbStopBits1.Text) == 1 ? System.IO.Ports.StopBits.One : System.IO.Ports.StopBits.Two);
                    sp.Parity = cbParity1.SelectedIndex == 0 ? System.IO.Ports.Parity.None : (cbParity1.SelectedIndex == 1 ? System.IO.Ports.Parity.Odd : System.IO.Ports.Parity.Even);
                });
                panasonicMewtocol1.Open();
                if (panasonicMewtocol1.IsOpen())
                {
                    tbLog1.BeginInvoke(GetMyDelegateLog, tbLog1, "串口1打开成功...", null);
                    this.BeginInvoke(new Action(()=>{
                        btOpenPlc1.Enabled = false;
                        btClosePlc1.Enabled = true;
                        controlCom1(false);
                    }));
                }
                else
                {
                    this.BeginInvoke(new Action(() =>
                    {
                        btClosePlc1.Enabled = false;
                        btOpenPlc1.Enabled = true;
                        controlCom1(true);
                        MessageBox.Show($"{cbPortName1.Text}拒绝访问,请换个COM口连接...");
                    }));
                    tbLog1.BeginInvoke(GetMyDelegateLog, tbLog1, $"{cbPortName1.Text}拒绝访问,请换个COM口连接...", null);  
                }
            }
            catch (Exception ex)
            {
                this.BeginInvoke(new Action(() =>
                {
                    btClosePlc1.Enabled = false;
                    btOpenPlc1.Enabled = true;
                }));            
                tbLog1.BeginInvoke(GetMyDelegateLog,tbLog1,"串口打开失败"+ex.Message, ex); 
            }
        }

        #endregion

        #region 相机2缩放
        bool m_bZoom2 = false;
        ZoomImage zi2 = null;
        private void 缩放ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (ho_image2 != null && ho_image2.CountObj() != 0)
            {
                m_bZoom2 = true;
            }
        }
        #endregion

        #region 相机2读取图片
        private void 读取图片ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                this.openFileDialog1.Filter = "BMP图片|*.BMP|所有图片|*.*";
                if (this.openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    int winWidth = hWindowControl2.Width;
                    int winHeight = hWindowControl2.Height;
                    hWindowControl2.HalconWindow.ClearWindow();
                    HOperatorSet.ReadImage(out ho_image2, this.openFileDialog1.FileName);
                    hv_Width2.Dispose(); hv_Height2.Dispose();
                    HOperatorSet.GetImageSize(ho_image2, out hv_Width2, out hv_Height2);
                    HalconHelper.imageLocation(hv_Width2, hv_Height2, winWidth, winHeight, out row3, out column3, out row4, out column4);
                    if (!isMiniSizeState)
                        HOperatorSet.SetPart(this.hWindowControl2.HalconWindow, row3, column3, row4, column4);
                    HOperatorSet.DispObj(ho_image2, this.hWindowControl2.HalconWindow);
                    //zi2 = new ZoomImage(row3, column3, row4, column4, hv_Width2, hv_Height2, this.hWindowControl2);
                }
            }
            catch (Exception ex)
            {
                ho_image2.Dispose();
                hv_Width2.Dispose();
                hv_Height2.Dispose();
                MessageBox.Show("相机2读图失败");
                LogHelper.WriteLog("相机2读图失败",ex);
            }
        }
        #endregion

        private void startCom2()
        {
            showLog(tbLog2, "相机2已就绪...", null);

            IniHelper.SaveSetIni.Write("相机2PLC设置", "准备信号", tbReadySignal2.Text);
            IniHelper.SaveSetIni.Write("相机2PLC设置", "OK信号", tbOKSignal2.Text);
            IniHelper.SaveSetIni.Write("相机2PLC设置", "拍照信号", tbGrabImageSignal2.Text);

            btAddNGSet2.Enabled = false;
            contextMenuStrip2.Enabled = false;
        }

        #region 自动运行过程中禁止操作软件

        private void runControl(string str)
        {
            showLog(tbLog1, str, null);
            showLog(tbLog2, str, null);
            if (str == "继续开始自动运行...")
            {
                btClosePlc1.Enabled = false;
                contextMenuStrip1.Enabled = false;
                contextMenuStrip2.Enabled = false;
                btAddNGSet1.Enabled = false;
                controlSignal1(false);
                controlSignal2(false);
                if (cbEnableCode.Checked)
                    controlCodeSet(false);
                      
                btClearDate.Enabled = false;
                btHandTest.Enabled = true;
                cbEnableCode.Enabled = false;
            }
            else if (str == "停止...")
            {
                contextMenuStrip1.Enabled = true;
                contextMenuStrip2.Enabled = true;
                btAddNGSet1.Enabled = true;
                controlSignal1(true);
                controlSignal2(true);
                if (cbEnableCode.Checked)
                    controlCodeSet(true);
                btClosePlc1.Enabled = true;
                btClearDate.Enabled = true;
                btHandTest.Enabled = false; 
                cbEnableCode.Enabled = true;
            }
            if (panasonicMewtocol1.IsOpen())
            {
                if (str == "继续开始自动运行...")
                {
                    Task.Run(() =>
                    {
                        panasonicMewtocol1.Write(tbReadySignal1.Text, true);
                        
                        panasonicMewtocol1.Write(tbReadySignal2.Text, true);
                    });    
                }
                else if (str == "停止...")
                {
                    Task.Run(() =>
                    {
                        panasonicMewtocol1.Write(tbReadySignal1.Text, false);

                        panasonicMewtocol1.Write(tbReadySignal2.Text, false);
                    }); 
                }
            }
        }

        #endregion

        #region 控制串口相关控件
        private void controlCom1(bool islogin)
        {
            cbPortName1.Enabled = islogin;
            tbBaudRate1.Enabled = islogin;
            tbDateBits1.Enabled = islogin;
            tbStopBits1.Enabled = islogin;
            cbParity1.Enabled = islogin;
            btOpenPlc1.Enabled = islogin;
            //btClosePlc1.Enabled = islogin;
        }

        private void controlSignal1(bool islogin)
        {
            tbReadySignal1.Enabled = islogin;
            tbGrabImageSignal1.Enabled = islogin;
            tbOKSignal1.Enabled = islogin;
            btAddNGSet1.Enabled = islogin;
            textBox_time_left.Enabled = islogin;
            numericUpDown_left.Enabled = islogin;
            textBox_time_right.Enabled = islogin;
            numericUpDown_right.Enabled = islogin;
            cb_isTwoSignal.Enabled = islogin;
        }
        private void controlSignal2(bool islogin)
        {
            tbGrabImageSignal2.Enabled= islogin;
            tbReadySignal2.Enabled= islogin;
            tbOKSignal2.Enabled = islogin;
            btAddNGSet2.Enabled = islogin;
            textBox_time_left.Enabled = islogin;
            numericUpDown_left.Enabled = islogin;
            textBox_time_right.Enabled = islogin;
            numericUpDown_right.Enabled = islogin;
            cb_isTwoSignal.Enabled = islogin;
        }

        private void controlCodeSet(bool islogin)
        {
            tbCodeAddress.Enabled = islogin;
            tbCodeLength.Enabled = islogin;
            tbCodeResult.Enabled = islogin;
        }

        #endregion

        #region 相机1NG信号设置
        string status = null;
        private void button1_Click(object sender, EventArgs e)
        {
            status = "Cam1";
            set1.treeFileHelper = treeFileHelper1;
            set1.setClassSetOK(ref list_OKType_classId_left);
            set1.Text = "相机1NG信号设置";
            set1.ShowDialog();
        }
        #endregion

        #region 相机2NG信号设置
        private void btAddNGSet2_Click(object sender, EventArgs e)
        {
            status = "Cam2";
            set2.treeFileHelper = treeFileHelper2;
            set2.setClassSetOK(ref list_OKType_classId_right);
            set2.Text = "相机2NG信号设置";
            set2.ShowDialog();

        }
        #endregion

        #region 是否启用读码事件
        private void cbIsEnableCode_CheckedChanged(object sender, EventArgs e)
        {
            if (cbEnableCode.Checked)
            {
                tbCodeAddress.Enabled = true;
                tbCodeLength.Enabled = true;
                tbCodeResult.Enabled = true;
                IniHelper.SaveSetIni.Write("双相机读码设置", "是否启用读码", "T");
            }
            else
            {
                tbCodeAddress.Enabled = false;
                tbCodeLength.Enabled = false;
                tbCodeResult.Enabled = false;
                IniHelper.SaveSetIni.Write("双相机读码设置", "是否启用读码", "F");
            }
        }
        #endregion

        #region 初始化检测结果
        private void loadResult()
        {
            if (!String.IsNullOrEmpty(IniHelper.SaveSetIni.Read("相机1生产信息", "生产总数")))
                AllNum1 = Convert.ToInt32(IniHelper.SaveSetIni.Read("相机1生产信息", "生产总数"));
            if (!String.IsNullOrEmpty(IniHelper.SaveSetIni.Read("相机1生产信息", "OK总数")))
                OKNum1 = Convert.ToInt32(IniHelper.SaveSetIni.Read("相机1生产信息", "OK总数"));
            if (!String.IsNullOrEmpty(IniHelper.SaveSetIni.Read("相机1生产信息", "NG总数")))
                NGNum1 = Convert.ToInt32(IniHelper.SaveSetIni.Read("相机1生产信息", "NG总数"));
            lbAllNum1.Text = AllNum1.ToString();
            lbOKNum1.Text = OKNum1.ToString();
            lbNGNum1.Text = NGNum1.ToString();
            if (OKNum1 != 0 && AllNum1 != 0)
                lbOKPercent1.Text = (OKNum1 / AllNum1 * 100).ToString("F2") + "%";
            lbResult1.Text = "NG";
            lbResult1.BackColor = System.Drawing.Color.Red;
            if (!String.IsNullOrEmpty(IniHelper.SaveSetIni.Read("相机2生产信息", "生产总数")))
                AllNum2 = Convert.ToInt32(IniHelper.SaveSetIni.Read("相机2生产信息", "生产总数"));
            if (!String.IsNullOrEmpty(IniHelper.SaveSetIni.Read("相机2生产信息", "OK总数")))
                OKNum2 = Convert.ToInt32(IniHelper.SaveSetIni.Read("相机2生产信息", "OK总数"));
            if (!String.IsNullOrEmpty(IniHelper.SaveSetIni.Read("相机2生产信息", "NG总数")))
                NGNum2 = Convert.ToInt32(IniHelper.SaveSetIni.Read("相机2生产信息", "NG总数"));
            lbAllNum2.Text = AllNum2.ToString();
            lbOKNum2.Text = OKNum2.ToString();
            lbNGNum2.Text = NGNum2.ToString();
            if (OKNum2 != 0 && AllNum2 != 0)
                lbOKPercent2.Text = (OKNum2 / AllNum2 * 100).ToString("F2") + "%";
            lbResult2.Text = "NG";
            lbResult2.BackColor = System.Drawing.Color.Red;

        }
        #endregion


        #region 手动测试
        private void btHandTest_Click(object sender, EventArgs e)
        {
            if (trigerDLuseImage)
            {
                string[] searchPatterns = { "*.jpg", "*.bmp","*.png"};
                foreach (string pattern in searchPatterns)
                {
                    string[] files = Directory.GetFiles(director_left, pattern);
                    if(files == null)
                    {
                        MessageBox.Show("请选择一个文件路径后重试");
                        return;
                    }
                    imagePathFiles_left.AddRange(files);
                }
                if (imagePathFiles_left.Count > 0)
                {
                    window_left_pictruePath_isReady = true;
                }
                foreach (string pattern in searchPatterns)
                {
                    string[] files = Directory.GetFiles(director_right, pattern);
                    if (files == null)
                    {
                        MessageBox.Show("请选择一个文件路径后重试");
                        return;
                    }
                    imagePathFiles_right.AddRange(files);
                }
                if(imagePathFiles_right.Count > 0)
                {
                    window_right_pictruePath_isReady = true;
                }
                if(!(window_left_pictruePath_isReady && window_right_pictruePath_isReady))
                {
                    MessageBox.Show("请检查图片路径，调试模式-读图片测试未准备好，请检查目录是否有图片");
                    return;
                }
                //触发模式准备完成
                trigerDLuseImage_isReady = true;

                if (pictrueIndex_left == imagePathFiles_left.Count)
                    pictrueIndex_left = 0;
                string filePath = imagePathFiles_left[pictrueIndex_left];
                HOperatorSet.ReadImage(out ho_image1, filePath);
                pictrueIndex_left++;

                if (pictrueIndex_right == imagePathFiles_right.Count)
                    pictrueIndex_right = 0;
                string tmpfilePath = imagePathFiles_right[pictrueIndex_right];
                HOperatorSet.ReadImage(out ho_image2, tmpfilePath);
                pictrueIndex_right++;

                // 触发线程2和线程3执行
                event1.Set();
                event2.Set();

                if (hasTwoSignal)
                {
                    countdown.Reset(1);
                    countdownTwo.Reset(1);
                }

                // 等待线程2和线程3完成
                countdown.Wait();
                if (hasTwoSignal)
                    countdownTwo.Wait();

                // 重置事件和倒计时器
                //event1.Reset();
                // event2.Reset();
                countdown.Reset();
                if(hasTwoSignal)
                    countdownTwo.Reset();
            }
            else
            {
                isImage = true;
            }
        }
        #endregion

        #region 清除数据
        private void btClearDate_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("确认是否清除?", "警告", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
            {
                //数据清零
                countClearanceSetting(false);
            }
        }

        #endregion

        #region 加载深度学习配置文件到表格
        private void addRows(TreeFileHelper treeFileHelper, int datagridIndex,List<string> l_OKTypes)
        {
            DataGridView tempGridView = null;
            if (datagridIndex == 3)
                tempGridView = dataGridView_left;
            else if (datagridIndex == 4)
                tempGridView = dataGridView_right;
            else
                return;

            int columnIndex = 1; // 列索引从 0 开始计数
            List<string> columnValues = new List<string>(); //保存某一列的所有值
            foreach (DataGridViewRow row in tempGridView.Rows)
            {
                if (row.Cells[columnIndex].Value != null)
                {
                    string cellValue = row.Cells[columnIndex].Value.ToString();
                    columnValues.Add(cellValue);
                }
            }

            foreach (var item in treeFileHelper.dic_nodeIdWithNodeInfo)
            {
                if (item.Value.NodeType == 1)
                {
                    if (item.Value.ParentsNodeId == -1 && !columnValues.Contains("定位失败"))
                    {
                        tempGridView.Rows.Add(item.Value.NodeName, "定位失败", "0", "--", "0", "0", "--", "--");
                    }     
                }
                else
                {
                    foreach (var item1 in item.Value.ClassNames)
                    {
                        if (!columnValues.Contains(item1) && !item1.Equals("OK", StringComparison.OrdinalIgnoreCase) && !item1.StartsWith("其他") 
                            && !l_OKTypes.Contains(item1))
                        {
                            tempGridView.Rows.Add(item.Value.NodeName, item1, "0", "--", "0", "0", "--", "--"); 
                        }
                    }
                }
            }
        }
        #endregion

        #region 添加表头
        private void createHead1()
        {
            dataGridView_left.RowHeadersVisible = false;
            dataGridView_left.Enabled = false;
            DataGridViewColumnCollection columns = dataGridView_left.Columns;
            // 节点表头列
            DataGridViewColumn nodeColumn = new DataGridViewTextBoxColumn();
            nodeColumn.HeaderText = "节点";
            columns.Add(nodeColumn);
            // 类型表头列
            DataGridViewColumn typeColumn = new DataGridViewTextBoxColumn();
            typeColumn.HeaderText = "类型";
            columns.Add(typeColumn);
            // 个数表头列
            DataGridViewColumn countColumn = new DataGridViewTextBoxColumn();
            countColumn.HeaderText = "个数";
            columns.Add(countColumn);
            // 判定表头列
            DataGridViewColumn judgeColumn = new DataGridViewTextBoxColumn();
            judgeColumn.HeaderText = "判定";
            columns.Add(judgeColumn);
            // 总数表头列
            DataGridViewColumn totalColumn = new DataGridViewTextBoxColumn();
            totalColumn.HeaderText = "总数";
            columns.Add(totalColumn);
            // 占比表头列
            DataGridViewColumn proportionColumn = new DataGridViewTextBoxColumn();
            proportionColumn.HeaderText = "占比";
            columns.Add(proportionColumn);
            // 面积表头列
            DataGridViewColumn areaColumn = new DataGridViewTextBoxColumn();
            areaColumn.HeaderText = "面积";
            columns.Add(areaColumn);
            // 分数表头列
            DataGridViewColumn scoreColumn = new DataGridViewTextBoxColumn();
            scoreColumn.HeaderText = "分数";
            columns.Add(scoreColumn);
        }
        private void createHead2()
        {
            dataGridView_right.RowHeadersVisible = false;
            dataGridView_right.Enabled = false;
            DataGridViewColumnCollection columns = dataGridView_right.Columns;
            // 节点表头列
            DataGridViewColumn nodeColumn = new DataGridViewTextBoxColumn();
            nodeColumn.HeaderText = "节点";
            columns.Add(nodeColumn);
            // 类型表头列
            DataGridViewColumn typeColumn = new DataGridViewTextBoxColumn();
            typeColumn.HeaderText = "类型";
            columns.Add(typeColumn);
            // 个数表头列
            DataGridViewColumn countColumn = new DataGridViewTextBoxColumn();
            countColumn.HeaderText = "个数";
            columns.Add(countColumn);
            // 判定表头列
            DataGridViewColumn judgeColumn = new DataGridViewTextBoxColumn();
            judgeColumn.HeaderText = "判定";
            columns.Add(judgeColumn);
            // 总数表头列
            DataGridViewColumn totalColumn = new DataGridViewTextBoxColumn();
            totalColumn.HeaderText = "总数";
            columns.Add(totalColumn);
            // 占比表头列
            DataGridViewColumn proportionColumn = new DataGridViewTextBoxColumn();
            proportionColumn.HeaderText = "占比";
            columns.Add(proportionColumn);
            // 面积表头列
            DataGridViewColumn areaColumn = new DataGridViewTextBoxColumn();
            areaColumn.HeaderText = "面积";
            columns.Add(areaColumn);
            // 分数表头列
            DataGridViewColumn scoreColumn = new DataGridViewTextBoxColumn();
            scoreColumn.HeaderText = "分数";
            columns.Add(scoreColumn);
        }
        #endregion

        #region 将ok跟ng类型存入字典中
        public void addNGorOK1()
        {
            try
            {
                if (!File.Exists(rootDirectory1))
                {
                    MessageBox.Show("请先设置好相机1plc的NG信号");
                    return;
                }
                list_OKType_classId_left.Clear();
                NGTypePara.NGType ngTypes = new NGTypePara.NGType();
                string jsonFromFile = File.ReadAllText(rootDirectory1);
                ngTypes = JsonConvert.DeserializeObject<NGType>(jsonFromFile);
                if (ngTypes != null && dic_defectClassificationswithPLCaddress_left.Count != 0)
                {
                    foreach (var item in ngTypes.NGTypeConfigs)
                    {
                        if (item.isOK || item.NGType.StartsWith("其他"))
                        {
                            if (!list_OKType_classId_left.Contains(item.NGType))
                                list_OKType_classId_left.Add(item.NGType);
                        }
                    }
                    //更新dataView
                    addRows(treeFileHelper1, 3, list_OKType_classId_left);
                }
                else
                {
                    tbLog1.BeginInvoke(GetMyDelegateLog, tbLog1, "NGTypeSignalSet2_1.txt 内容为空，请重新生成", null);
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("将ok跟ng类型存入字典中失败",ex);
                MessageBox.Show(ex.ToString());
            }
        }
        public void addNGorOK2()
        {
            try
            {
                if (!File.Exists(rootDirectory2))
                {
                    MessageBox.Show("请先设置好相机2plc的NG信号");
                    return;
                }
                list_OKType_classId_right.Clear();
                //NGType2.Clear();
                NGTypePara.NGType ngTypes = new NGTypePara.NGType();
                string jsonFromFile = File.ReadAllText(rootDirectory2);
                ngTypes = JsonConvert.DeserializeObject<NGType>(jsonFromFile);
                if (ngTypes != null && dic_defectClassificationswithPLCaddress_right.Count != 0)
                {
                    foreach (var item in ngTypes.NGTypeConfigs)
                    {
                        if (item.isOK || item.NGType.StartsWith("其他"))
                        {
                            if (!list_OKType_classId_right.Contains(item.NGType))
                                list_OKType_classId_right.Add(item.NGType);
                        }
                    }
                    addRows(treeFileHelper2, 4, list_OKType_classId_right);
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("将ok跟ng类型存入字典中失败", ex);
                MessageBox.Show(ex.ToString());
            }
        }
        #endregion

        #region 将ng输出类型对应的plc地址添加进字典

        private void addNGDic1()
        {
            dic_defectClassificationswithPLCaddress_left.Clear();
            NGTypePara.OutSignalSets signalSets = new NGTypePara.OutSignalSets();
            string jsonFromFile = null;
            string path = AppDomain.CurrentDomain.BaseDirectory + "OutType2_1.txt";
            try
            {
                jsonFromFile = File.ReadAllText(path);
            } catch(FileNotFoundException ex)
            {
                File.Create(path).Close();
                jsonFromFile = File.ReadAllText(path);
            }
            signalSets = JsonConvert.DeserializeObject<OutSignalSets>(jsonFromFile);
            if (signalSets != null)
            {
                foreach (var item in signalSets.outsignals)
                {
                    if (item.isEnable)
                        dic_defectClassificationswithPLCaddress_left.Add(item.OutSignalName, item.OutSignal);
                }
            }
        }

        private void addNGDic2()
        {
            dic_defectClassificationswithPLCaddress_right.Clear();
            NGTypePara.OutSignalSets signalSets = new NGTypePara.OutSignalSets();
            string jsonFromFile = null;
            string path = AppDomain.CurrentDomain.BaseDirectory + "OutType2_2.txt";
            try
            {
                jsonFromFile = File.ReadAllText(path);
            }
            catch (FileNotFoundException ex)
            {
                File.Create(path).Close();
                jsonFromFile = File.ReadAllText(path);
            }
            signalSets = JsonConvert.DeserializeObject<OutSignalSets>(jsonFromFile);
            if (signalSets != null)
            {
                foreach (var item in signalSets.outsignals)
                {
                    if (item.isEnable)
                        dic_defectClassificationswithPLCaddress_right.Add(item.OutSignalName, item.OutSignal);
                }
            }
        }
        #endregion

        #region 判定OK还是NG
        string dingweiSignal1;
        string ngSignal1;
        private async void tellPLCResult1(string jiance_result_class_id,string result)
        {
            await Task.Run(() =>
            {              
                try
                {
                    //打开了才发送信号
                    if (!panasonicMewtocol1.IsOpen())
                        return;
                    switch (result)
                    {
                        case "OK":
                            {
                                do
                                {
                                    panasonicMewtocol1.Write(tbOKSignal1.Text, true);
                                    Thread.Sleep(10);
                                } while (!panasonicMewtocol1.ReadBool(tbOKSignal1.Text).Content);
                                tbLog1.BeginInvoke(GetMyDelegateLog, tbLog1, $"{tbOKSignal1.Text}信号次发送成功", null);
                                Thread.Sleep(10);
                                do
                                {
                                    panasonicMewtocol1.Write(tbOKSignal1.Text, false);
                                    Thread.Sleep(10);
                                } while (panasonicMewtocol1.ReadBool(tbOKSignal1.Text).Content);
                                tbLog1.BeginInvoke(GetMyDelegateLog, tbLog1, $"{tbOKSignal1.Text}信号次断开成功", null);
                                break;
                            }
                        case "NG":
                            {
                                NGTypePara.NGType ngTypes = new NGTypePara.NGType();
                                string jsonFromFile = File.ReadAllText(rootDirectory1);
                                ngTypes = JsonConvert.DeserializeObject<NGType>(jsonFromFile);

                                foreach (var item1 in ngTypes.NGTypeConfigs)
                                {
                                    if (item1.NGType.Equals(jiance_result_class_id))
                                    {
                                        ngSignal1 = dic_defectClassificationswithPLCaddress_left[item1.OutType];
                                        break;
                                    }
                                }
                                do
                                {
                                    panasonicMewtocol1.Write(ngSignal1, true);
                                    Thread.Sleep(10);
                                } while (!panasonicMewtocol1.ReadBool(ngSignal1).Content);
                                tbLog1.BeginInvoke(GetMyDelegateLog, tbLog1, $"{ngSignal1}信号次发送成功", null);
                                Thread.Sleep(10);
                                do
                                {
                                    panasonicMewtocol1.Write(ngSignal1, false);
                                    Thread.Sleep(10);
                                } while (panasonicMewtocol1.ReadBool(ngSignal1).Content);
                                tbLog1.BeginInvoke(GetMyDelegateLog, tbLog1, $"{ngSignal1}信号次断开成功", null);
                                break;
                            }
                        case "定位失败":
                            {
                                NGTypePara.NGType ngTypes = new NGTypePara.NGType();
                                string jsonFromFile = File.ReadAllText(rootDirectory1);
                                ngTypes = JsonConvert.DeserializeObject<NGType>(jsonFromFile);

                                foreach (var item1 in ngTypes.NGTypeConfigs)
                                {
                                    if (item1.NGType.Equals(jiance_result_class_id))
                                    {
                                        dingweiSignal1 = dic_defectClassificationswithPLCaddress_left[item1.OutType];
                                        break;
                                    }
                                }
                                do
                                {
                                    panasonicMewtocol1.Write(dingweiSignal1, true);
                                    Thread.Sleep(10);
                                } while (!panasonicMewtocol1.ReadBool(dingweiSignal1).Content);
                                tbLog1.BeginInvoke(GetMyDelegateLog, tbLog1, $"{dingweiSignal1}信号次发送成功", null);
                                Thread.Sleep(10);
                                do
                                {
                                    panasonicMewtocol1.Write(dingweiSignal1, false);
                                    Thread.Sleep(10);
                                } while (panasonicMewtocol1.ReadBool(dingweiSignal1).Content);
                                tbLog1.BeginInvoke(GetMyDelegateLog, tbLog1, $"{dingweiSignal1}信号次断开成功", null);
                                break;
                            }
                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {
                    tbLog1.BeginInvoke(GetMyDelegateLog, tbLog1, $"发送信号给PLC失败"+ex.Message,ex);
                }
            });
        }
        string dingweiSignal2;
        string ngSignal2;
        private async void tellPLCResult2(string jiance_result_class_id,string result)
        {
            await Task.Run(() =>
            {
                try
                {
                    //打开了才发送信号
                    if (!panasonicMewtocol1.IsOpen())
                        return;
                    switch (result)
                    {
                        case "OK":
                            {
                                string cam2OKSignalAddress;
                                if (string.IsNullOrEmpty(tbOKSignal2.Text))
                                {
                                    cam2OKSignalAddress = tbOKSignal1.Text;
                                }
                                else
                                {
                                    cam2OKSignalAddress = tbOKSignal2.Text;
                                }

                                do
                                {
                                    panasonicMewtocol1.Write(cam2OKSignalAddress, true);
                                    Thread.Sleep(10);
                                } while (!panasonicMewtocol1.ReadBool(cam2OKSignalAddress).Content);
                                tbLog1.BeginInvoke(GetMyDelegateLog, tbLog2, $"{cam2OKSignalAddress}信号次发送成功", null);
                                Thread.Sleep(10);
                                do
                                {
                                    panasonicMewtocol1.Write(cam2OKSignalAddress, false);
                                    Thread.Sleep(10);
                                } while (panasonicMewtocol1.ReadBool(cam2OKSignalAddress).Content);
                                tbLog2.BeginInvoke(GetMyDelegateLog, tbLog2, $"{cam2OKSignalAddress}信号次断开成功", null);
                                break;
                            }
                        case "NG":
                            {
                                NGTypePara.NGType ngTypes = new NGTypePara.NGType();
                                string jsonFromFile = File.ReadAllText(rootDirectory1);
                                ngTypes = JsonConvert.DeserializeObject<NGType>(jsonFromFile);

                                foreach (var item1 in ngTypes.NGTypeConfigs)
                                {
                                    if (item1.NGType.Equals(jiance_result_class_id))
                                    {
                                        ngSignal2 = dic_defectClassificationswithPLCaddress_right[item1.OutType];
                                        break;
                                    }
                                }
                                do
                                {
                                    panasonicMewtocol1.Write(ngSignal2, true);
                                    Thread.Sleep(10);
                                } while (!panasonicMewtocol1.ReadBool(ngSignal2).Content);
                                tbLog2.BeginInvoke(GetMyDelegateLog, tbLog2, $"{ngSignal2}信号次发送成功", null);
                                Thread.Sleep(10);
                                do
                                {
                                    panasonicMewtocol1.Write(ngSignal2, false);
                                    Thread.Sleep(10);
                                } while (panasonicMewtocol1.ReadBool(ngSignal2).Content);
                                tbLog2.BeginInvoke(GetMyDelegateLog, tbLog2, $"{ngSignal2}信号次断开成功", null);

                                break;
                            }
                        case "定位失败":
                            {
                                NGTypePara.NGType ngTypes = new NGTypePara.NGType();
                                string jsonFromFile = File.ReadAllText(rootDirectory2);
                                ngTypes = JsonConvert.DeserializeObject<NGType>(jsonFromFile);

                                foreach (var item1 in ngTypes.NGTypeConfigs)
                                {
                                    //定位信号为"1"
                                    if (item1.NGType == jiance_result_class_id)
                                    {
                                        dingweiSignal2 = dic_defectClassificationswithPLCaddress_right[item1.OutType];
                                        break;
                                    }

                                }
                                do
                                {
                                    panasonicMewtocol1.Write(dingweiSignal2, true);
                                    Thread.Sleep(10);
                                } while (!panasonicMewtocol1.ReadBool(dingweiSignal2).Content);
                                tbLog2.BeginInvoke(GetMyDelegateLog, tbLog2, $"{dingweiSignal2}信号次发送成功", null);
                                Thread.Sleep(10);
                                do
                                {
                                    panasonicMewtocol1.Write(dingweiSignal2, false);
                                    Thread.Sleep(10);
                                } while (panasonicMewtocol1.ReadBool(dingweiSignal2).Content);
                                tbLog2.BeginInvoke(GetMyDelegateLog, tbLog2, $"{dingweiSignal2}信号次断开成功", null);
                                break;
                            }
                        default:
                            break;
                    }
                }
                catch(Exception ex)
                {
                    tbLog2.BeginInvoke(GetMyDelegateLog, tbLog2, $"发送信号给PLC失败" + ex.Message, ex);
                }
                
            });
        }
        #endregion

        #region 渲染图片
        private async void showNGLocation1(Dictionary<int, List<result>> res1,string result)
        {
            await Task.Run(() =>
            {
                try
                {
                    if (result == "OK")
                    {
                        if (treeFileHelper1.dingweiNodeId != int.MinValue) //有定位
                        {
                            for (int i = 0; i < dingwei_num_left; i++)
                            {
                                HTCSharpDemo.Program.result res2 = res1[treeFileHelper1.dingweiNodeId][i];
                                HalconHelper.showRoi(hWindowControl1.HalconWindow, "green", res2.y, res2.x, res2.y + res2.height, res2.x + res2.width);
                            }
                        }
                        HalconHelper.showResultString(hWindowControl1.HalconWindow, 0, 0, 72, "green", "OK");
                    }
                    else
                    {
                        //思路：把不是OK的检测项放一起
                        Dictionary<Rectangle, int> keyValuePairs = new Dictionary<Rectangle, int>();
                        List<Rectangle> ok_list = new List<Rectangle>();
                        if (res1.Count != 0)
                        {
                            if (treeFileHelper1.dingweiNodeId != int.MinValue)//需要定位
                            {
                                foreach (var item in res1.Keys)
                                {
                                    List<result> tmp_result = res1[item];
                                    for (int j = 0; j < tmp_result.Count; j++)
                                    {
                                        Rectangle rectangle = new Rectangle();
                                        HTCSharpDemo.Program.result res2 = tmp_result[j];
                                        rectangle.X = res2.y;
                                        rectangle.Y = res2.x;
                                        rectangle.Width = res2.width;
                                        rectangle.Height = res2.height;
                                        if (res2.class_id == "OK" && !ok_list.Contains(rectangle))
                                        {
                                            ok_list.Add(rectangle);
                                        }
                                        if (!keyValuePairs.ContainsKey(rectangle))
                                        {
                                            keyValuePairs.Add(rectangle, 1);
                                        }
                                        else
                                        {
                                            keyValuePairs[rectangle]++;
                                        }
                                    }   
                                }
                                foreach (Rectangle key in keyValuePairs.Keys)
                                {
                                    if (keyValuePairs[key] == 1)
                                    {
                                        HalconHelper.showRoi(hWindowControl1.HalconWindow, "green", key.X, key.Y, key.X + key.Height, key.Y + key.Width);
                                    }
                                    else
                                    {
                                        if (ok_list.Contains(key))
                                        {
                                            HalconHelper.showRoi(hWindowControl1.HalconWindow, "green", key.X, key.Y, key.X + key.Height, key.Y + key.Width);
                                        }
                                        else
                                        {
                                            HalconHelper.showRoi(hWindowControl1.HalconWindow, "red", key.X, key.Y, key.X + key.Height, key.Y + key.Width);
                                        }
                                    }
                                }
                            }
                            else //不需要定位
                            {
                                foreach (var item in res1.Keys)
                                {
                                    List<result> tmp_result = res1[item];
                                    for (int j = 0; j < tmp_result.Count; j++)
                                    {
                                        Rectangle rectangle = new Rectangle();
                                        HTCSharpDemo.Program.result res2 = tmp_result[j];
                                        rectangle.X = res2.y;
                                        rectangle.Y = res2.x;
                                        rectangle.Width = res2.width;
                                        rectangle.Height = res2.height;
                                        if (res2.class_id == "OK" && !ok_list.Contains(rectangle))
                                        {
                                            ok_list.Add(rectangle);
                                        }
                                        if (!keyValuePairs.ContainsKey(rectangle))
                                        {
                                            keyValuePairs.Add(rectangle, 1);
                                        }
                                        else
                                        {
                                            keyValuePairs[rectangle]++;
                                        }
                                    }
                                }
                                foreach (Rectangle key in keyValuePairs.Keys)
                                {
                                    if (ok_list.Contains(key))
                                    {
                                        HalconHelper.showRoi(hWindowControl1.HalconWindow, "green", key.X, key.Y, key.X + key.Height, key.Y + key.Width);
                                    }
                                    else
                                    {
                                        HalconHelper.showRoi(hWindowControl1.HalconWindow, "red", key.X, key.Y, key.X + key.Height, key.Y + key.Width);
                                    }
                                }
                            }
                            HalconHelper.showResultString(hWindowControl1.HalconWindow, 0, 0, 72, "red", result);
                        }
                    }

                    if (res1.ContainsKey(treeFileHelper1.quexianNodeId))
                    {
                        foreach (var item in res1[treeFileHelper1.quexianNodeId])
                        {
                            HalconHelper.showRoi(hWindowControl1.HalconWindow, "red", item.y, item.x, item.y + item.height, item.x + item.width);
                        }
                    }
                    foreach (var item in res1)
                    {
                        if (treeFileHelper1.listDingweiId.Contains(item.Key))
                        {
                            foreach (var item1 in item.Value)
                            {
                                HalconHelper.showRoi(hWindowControl1.HalconWindow, "red", item1.y, item1.x, item1.y + item1.height, item1.x + item1.width);
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    tbLog1.BeginInvoke(GetMyDelegateLog, tbLog1, $"渲染NG失败" + ex.Message, ex);
                }
                
            });
        }
        private async void showNGLocation2(Dictionary<int, List<result>> res3,string result)
        {
            await Task.Run(() =>
            {
                try
                {
                    if (result == "OK")
                    {
                        if (treeFileHelper2.dingweiNodeId != int.MinValue) //有定位
                        {
                            for (int i = 0; i < dingwei_num_right; i++)
                            {
                                HTCSharpDemo.Program.result res2 = res3[treeFileHelper2.dingweiNodeId][i];
                                HalconHelper.showRoi(hWindowControl2.HalconWindow, "green", res2.y, res2.x, res2.y + res2.height, res2.x + res2.width);
                            }
                        }
                        HalconHelper.showResultString(hWindowControl2.HalconWindow, 0, 0, 72, "green", "OK");
                    }
                    else
                    {
                        //思路：把不是OK的检测项放一起
                        Dictionary<Rectangle, int> keyValuePairs = new Dictionary<Rectangle, int>();
                        List<Rectangle> ok_list = new List<Rectangle>();
                        if (res3.Count != 0)
                        {
                            if (treeFileHelper2.dingweiNodeId != int.MinValue)//需要定位
                            {
                                foreach (var item in res3.Keys)
                                {
                                    List<result> tmp_result = res3[item];
                                    for (int j = 0; j < tmp_result.Count; j++)
                                    {
                                        Rectangle rectangle = new Rectangle();
                                        HTCSharpDemo.Program.result res2 = tmp_result[j];
                                        rectangle.X = res2.y;
                                        rectangle.Y = res2.x;
                                        rectangle.Width = res2.width;
                                        rectangle.Height = res2.height;
                                        if (res2.class_id == "OK" && !ok_list.Contains(rectangle))
                                        {
                                            ok_list.Add(rectangle);
                                        }
                                        if (!keyValuePairs.ContainsKey(rectangle))
                                        {
                                            keyValuePairs.Add(rectangle, 1);
                                        }
                                        else
                                        {
                                            keyValuePairs[rectangle]++;
                                        }
                                    }
                                }
                                foreach (Rectangle key in keyValuePairs.Keys)
                                {
                                    if (keyValuePairs[key] == 1)
                                    {
                                        HalconHelper.showRoi(hWindowControl2.HalconWindow, "green", key.X, key.Y, key.X + key.Height, key.Y + key.Width);
                                    }
                                    else
                                    {
                                        if (ok_list.Contains(key))
                                        {
                                            HalconHelper.showRoi(hWindowControl2.HalconWindow, "green", key.X, key.Y, key.X + key.Height, key.Y + key.Width);
                                        }
                                        else
                                        {
                                            HalconHelper.showRoi(hWindowControl2.HalconWindow, "red", key.X, key.Y, key.X + key.Height, key.Y + key.Width);
                                        }
                                    }
                                }
                            }
                            else //不需要定位
                            {
                                foreach (var item in res3.Keys)
                                {
                                    List<result> tmp_result = res3[item];
                                    for (int j = 0; j < tmp_result.Count; j++)
                                    {
                                        Rectangle rectangle = new Rectangle();
                                        HTCSharpDemo.Program.result res2 = tmp_result[j];
                                        rectangle.X = res2.y;
                                        rectangle.Y = res2.x;
                                        rectangle.Width = res2.width;
                                        rectangle.Height = res2.height;
                                        if (res2.class_id == "OK" && !ok_list.Contains(rectangle))
                                        {
                                            ok_list.Add(rectangle);
                                        }
                                        if (!keyValuePairs.ContainsKey(rectangle))
                                        {
                                            keyValuePairs.Add(rectangle, 1);
                                        }
                                        else
                                        {
                                            keyValuePairs[rectangle]++;
                                        }
                                    }
                                }
                                foreach (Rectangle key in keyValuePairs.Keys)
                                {
                                    if (ok_list.Contains(key))
                                    {
                                        HalconHelper.showRoi(hWindowControl2.HalconWindow, "green", key.X, key.Y, key.X + key.Height, key.Y + key.Width);
                                    }
                                    else
                                    {
                                        HalconHelper.showRoi(hWindowControl2.HalconWindow, "red", key.X, key.Y, key.X + key.Height, key.Y + key.Width);
                                    }
                                }
                            }
                        }
                        HalconHelper.showResultString(hWindowControl2.HalconWindow, 0, 0, 72, "red", result);

                        if (res3.ContainsKey(treeFileHelper2.quexianNodeId))
                        {
                            foreach (var item in res3[treeFileHelper2.quexianNodeId])
                            {
                                HalconHelper.showRoi(hWindowControl2.HalconWindow, "red", item.y, item.x, item.y + item.height, item.x + item.width);
                            }
                        }
                        foreach (var item in res3)
                        {
                            if (treeFileHelper2.listDingweiId.Contains(item.Key))
                            {
                                foreach (var item1 in item.Value)
                                {
                                    HalconHelper.showRoi(hWindowControl2.HalconWindow, "red", item1.y, item1.x, item1.y + item1.height, item1.x + item1.width);
                                }
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    tbLog1.BeginInvoke(GetMyDelegateLog, tbLog2, $"渲染NG失败" + ex.Message, ex);
                }
                
            });
        }
        #endregion

        #region 显示结果
        private void updatTotalData1(string result)
        {
            this.BeginInvoke(new Action(() =>
            {
                lbAllNum1.Text = AllNum1.ToString();
                lbOKNum1.Text = OKNum1.ToString();
                lbNGNum1.Text = NGNum1.ToString();
                if (AllNum1 != 0)
                    lbOKPercent1.Text = (OKNum1 / AllNum1 * 100).ToString("F2") + "%";
                else
                    lbOKPercent1.Text = "0.00%";
                lbCt1.Text = ct1.ToString();
                if (result == "OK")
                {
                    lbResult1.Text = "OK";
                    lbResult1.BackColor = System.Drawing.Color.Green;
                }
                else
                {
                    lbResult1.Text = "NG";
                    lbResult1.BackColor = System.Drawing.Color.Red;
                }
            }));
        }
        private void updatTotalData2(string result)
        {
            this.BeginInvoke(new Action(() =>
            {
                lbAllNum2.Text = AllNum2.ToString();
                lbOKNum2.Text = OKNum2.ToString();
                lbNGNum2.Text = NGNum2.ToString();
                if (AllNum2 != 0)
                    lbOKPercent2.Text = (OKNum2 / AllNum2 * 100).ToString("F2") + "%";
                else
                    lbOKPercent2.Text = "0.00%";
                lbCt2.Text = ct2.ToString();
                if (result == "OK")
                {
                    lbResult2.Text = "OK";
                    lbResult2.BackColor = System.Drawing.Color.Green;
                }
                else
                {
                    lbResult2.Text = "NG";
                    lbResult2.BackColor = System.Drawing.Color.Red;
                }
            }));
        }
        #endregion


         void writeLogBeforeClear()
        {
             Task.Run(() =>
            {
                LogHelper.WriteLog($"已清空历史数据,总数:{lbAllNum1.Text};OK数:{lbOKNum1.Text};NG数:{lbNGNum1.Text};良率:{lbOKPercent1.Text}");
                IniHelper.SaveSetIni.Write("相机1生产信息", "生产总数", "0");
                IniHelper.SaveSetIni.Write("相机1生产信息", "OK总数", "0");
                IniHelper.SaveSetIni.Write("相机1生产信息", "NG总数", "0");
                LogHelper.WriteLog($"已清空历史数据,总数:{lbAllNum2.Text};OK数:{lbOKNum2.Text};NG数:{lbNGNum2.Text};良率:{lbOKPercent2.Text}");
                IniHelper.SaveSetIni.Write("相机2生产信息", "生产总数", "0");
                IniHelper.SaveSetIni.Write("相机2生产信息", "OK总数", "0");
                IniHelper.SaveSetIni.Write("相机2生产信息", "NG总数", "0");
            });
            
        }

        private void cleartowGirdViewData()
        {
            lbAllNum1.Text = "0";
            AllNum1 = 0;
            lbOKNum1.Text = "0";
            OKNum1 = 0;
            lbNGNum1.Text = "0";
            NGNum1 = 0;
            lbOKPercent1.Text = "0";
            lbCt1.Text = "0";
            lbResult1.BackColor = System.Drawing.Color.Red;
            lbResult1.Text = "NG";            
            lbAllNum2.Text = "0";
            AllNum2 = 0;
            lbOKNum2.Text = "0";
            OKNum2 = 0;
            lbNGNum2.Text = "0";
            NGNum2 = 0;
            lbOKPercent2.Text = "0";
            lbCt2.Text = "0";
            lbResult2.BackColor = System.Drawing.Color.Red;
            lbResult2.Text = "NG";

            clearGridViewData3();
            clearGridViewData4();
        }
        private void clearGridViewData3()
        {
            foreach (DataGridViewRow row in dataGridView_left.Rows)
            {
                Object cellObj = row.Cells[0].Value;
                if (cellObj != null && !string.IsNullOrEmpty(cellObj.ToString()))
                {
                    row.Cells[2].Value = "0";
                    row.Cells[3].Value = "--";
                    row.Cells[4].Value = "0";
                    row.Cells[5].Value = "0";
                    row.Cells[6].Value = "--";
                    row.Cells[7].Value = "--";
                }  
            }
        }

        private void clearGridViewData4()
        {
            foreach (DataGridViewRow row in dataGridView_right.Rows)
            {
                Object cellObj = row.Cells[0].Value;
                if (cellObj != null && !string.IsNullOrEmpty(cellObj.ToString()))
                {
                    row.Cells[2].Value = "0";
                    row.Cells[3].Value = "--";
                    row.Cells[4].Value = "0";
                    row.Cells[5].Value = "0";
                    row.Cells[6].Value = "--";
                    row.Cells[7].Value = "--";
                }
            }
        }

        #region 更新结果列表
        private void updateGridViewData1(Dictionary<int, List<result>> res1,string result)
        {
            this.BeginInvoke(new Action(() =>
            {
                string configPath = IniHelper.SaveSetIni.Read("深度学习文件", "配置文件路径");
                var originalCellStyle = dataGridView_left.Rows[0].Cells[0].Style;
                foreach (DataGridViewRow row in dataGridView_left.Rows)
                    row.Cells[3].Style = originalCellStyle;
                try
                {
                    foreach (DataGridViewRow row in this.dataGridView_left.Rows)
                    {
                        if (dataGridView_left.Rows.Count - 1 > row.Index)
                        {
                            double ngdingweiNum = 0;
                            bool isAdd = false;
                            if (row.Cells[0].Value != null && !String.IsNullOrWhiteSpace(row.Cells[0].Value.ToString()) && row.Cells[0].Value != DBNull.Value)
                            {
                                if ((string)row.Cells[1].Value == "定位失败")
                                {
                                    isAdd = true;
                                    if (row.Cells[2].Value != null && !String.IsNullOrWhiteSpace(row.Cells[2].Value.ToString()) && row.Cells[2].Value != DBNull.Value && Regex.IsMatch(row.Cells[2].Value.ToString(), @"^\d+(\.\d+)?$"))
                                        ngdingweiNum = Convert.ToInt32(row.Cells[2].Value);
                                    if (res1.Count != 0 && res1[treeFileHelper1.dingweiNodeId].Count == dingwei_num_left)
                                        row.Cells[3].Value = "--";
                                    else
                                    {
                                        row.Cells[2].Value = ngdingweiNum + 1;
                                        row.Cells[3].Value = "NG";
                                        var cellStyle = new DataGridViewCellStyle();
                                        cellStyle.BackColor = System.Drawing.Color.Red;
                                        row.Cells[3].Style = cellStyle;
                                    }
                                    row.Cells[6].Value = "--";
                                    row.Cells[7].Value = "--";
                                }
                            }
                            double ngNum = 0;

                            row.Cells[4].Value = AllNum1;
                            foreach (var item in res1)
                            {
                                if (item.Value.Count > 0)
                                {
                                    bool isAdd1 = false;
                                    foreach (var item1 in item.Value)
                                    {
                                        if (row.Cells[1].Value != null && !String.IsNullOrWhiteSpace(row.Cells[1].Value.ToString()) && row.Cells[1].Value != DBNull.Value &&
                                        row.Cells[0].Value != null && !String.IsNullOrWhiteSpace(row.Cells[0].Value.ToString()) && row.Cells[0].Value != DBNull.Value)
                                        {
                                            if (item1.class_id == (string)row.Cells[1].Value && !isAdd1)
                                            {
                                                isAdd = true;
                                                isAdd1 = true;
                                                if (row.Cells[2].Value != null && !String.IsNullOrWhiteSpace(row.Cells[2].Value.ToString()) && row.Cells[2].Value != DBNull.Value && Regex.IsMatch(row.Cells[2].Value.ToString(), @"^\d+(\.\d+)?$"))
                                                    ngNum = Convert.ToDouble(row.Cells[2].Value.ToString());
                                                //结果出现次数
                                                if (result == "OK")
                                                {
                                                    row.Cells[2].Value = ngNum;
                                                    var cellStyle = new DataGridViewCellStyle();
                                                    cellStyle.BackColor = System.Drawing.Color.Green;
                                                    row.Cells[3].Style = cellStyle;
                                                    row.Cells[3].Value = "OK"; //判定
                                                }
                                                else
                                                {
                                                    row.Cells[2].Value = ngNum + 1;
                                                    var cellStyle = new DataGridViewCellStyle();
                                                    cellStyle.BackColor = System.Drawing.Color.Red;
                                                    row.Cells[3].Style = cellStyle;
                                                    row.Cells[3].Value = "NG";
                                                }
                                                row.Cells[6].Value = item1.area;  //面积
                                                row.Cells[7].Value = item1.score; //分数
                                            }
                                        }
                                    }
                                }
                            }
                            if (!isAdd)
                            {
                                if (row.Cells[2].Value != null && !String.IsNullOrWhiteSpace(row.Cells[1].Value.ToString()) && row.Cells[1].Value != DBNull.Value && Regex.IsMatch(row.Cells[2].Value.ToString(), @"^\d+(\.\d+)?$"))
                                    ngNum = Convert.ToDouble(row.Cells[2].Value.ToString());
                                row.Cells[2].Value = ngNum;
                                row.Cells[3].Value = "--";
                                row.Cells[6].Value = "--";
                                row.Cells[7].Value = "--";
                            }
                            if (AllNum1 == 0)
                                row.Cells[5].Value = "0.00%";
                            else
                                row.Cells[5].Value = (Convert.ToDouble(row.Cells[2].Value) / AllNum1 * 100).ToString("F2") + "%"; //百分比
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog("更新结果列表1失败",ex);
                }
            }));     
        }

        private void updateGridViewData2(Dictionary<int, List<result>> res2, string result)
        {
            this.BeginInvoke(new Action(() =>
            {
                string configPath = IniHelper.SaveSetIni.Read("深度学习文件", "配置文件路径");
                var originalCellStyle = dataGridView_right.Rows[0].Cells[0].Style;
                foreach (DataGridViewRow row in dataGridView_right.Rows)
                    row.Cells[3].Style = originalCellStyle;
                try
                {
                    foreach (DataGridViewRow row in this.dataGridView_right.Rows)
                    {
                        if (dataGridView_right.Rows.Count - 1 > row.Index)
                        {
                            double ngdingweiNum = 0;
                            bool isAdd = false;
                            if (row.Cells[0].Value != null && !String.IsNullOrWhiteSpace(row.Cells[0].Value.ToString()) && row.Cells[0].Value != DBNull.Value)
                            {
                                if ((string)row.Cells[1].Value == "定位失败")
                                {
                                    isAdd = true;
                                    if (row.Cells[2].Value != null && !String.IsNullOrWhiteSpace(row.Cells[2].Value.ToString()) && row.Cells[2].Value != DBNull.Value && Regex.IsMatch(row.Cells[2].Value.ToString(), @"^\d+(\.\d+)?$"))
                                        ngdingweiNum = Convert.ToInt32(row.Cells[2].Value);
                                    if (res2.Count != 0 && res2[treeFileHelper2.dingweiNodeId].Count == dingwei_num_right)
                                        row.Cells[3].Value = "--";
                                    else
                                    {
                                        row.Cells[2].Value = ngdingweiNum + 1;
                                        row.Cells[3].Value = "NG";
                                        var cellStyle = new DataGridViewCellStyle();
                                        cellStyle.BackColor = System.Drawing.Color.Red;
                                        row.Cells[3].Style = cellStyle;
                                    }
                                    row.Cells[6].Value = "--";
                                    row.Cells[7].Value = "--";
                                }
                            }
                            double ngNum = 0;
                            row.Cells[4].Value = AllNum2;
                            foreach (var item in res2)
                            {
                                if (item.Value.Count > 0)
                                {
                                    bool isAdd1 = false;
                                    foreach (var item1 in item.Value)
                                    {
                                        if (row.Cells[1].Value != null && !String.IsNullOrWhiteSpace(row.Cells[1].Value.ToString()) && row.Cells[1].Value != DBNull.Value &&
                                        row.Cells[0].Value != null && !String.IsNullOrWhiteSpace(row.Cells[0].Value.ToString()) && row.Cells[0].Value != DBNull.Value)
                                        {
                                            if (item1.class_id == (string)row.Cells[1].Value && !isAdd1)
                                            {
                                                isAdd = true;
                                                isAdd1 = true;
                                                if (row.Cells[2].Value != null && !String.IsNullOrWhiteSpace(row.Cells[2].Value.ToString()) && row.Cells[2].Value != DBNull.Value && Regex.IsMatch(row.Cells[2].Value.ToString(), @"^\d+(\.\d+)?$"))
                                                    ngNum = Convert.ToDouble(row.Cells[2].Value.ToString());
                                                //结果出现次数
                                                if (result == "OK")
                                                {
                                                    row.Cells[2].Value = ngNum;
                                                    var cellStyle = new DataGridViewCellStyle();
                                                    cellStyle.BackColor = System.Drawing.Color.Green;
                                                    row.Cells[3].Style = cellStyle;
                                                    row.Cells[3].Value = "OK"; //判定
                                                }
                                                else
                                                {
                                                    row.Cells[2].Value = ngNum + 1;
                                                    var cellStyle = new DataGridViewCellStyle();
                                                    cellStyle.BackColor = System.Drawing.Color.Red;
                                                    row.Cells[3].Style = cellStyle;
                                                    row.Cells[3].Value = "NG";
                                                }
                                                row.Cells[6].Value = item1.area;  //面积
                                                row.Cells[7].Value = item1.score; //分数
                                            }
                                        }
                                    }
                                }
                            }
                            if (!isAdd)
                            {
                                if (row.Cells[2].Value != null && !String.IsNullOrWhiteSpace(row.Cells[1].Value.ToString()) && row.Cells[1].Value != DBNull.Value && Regex.IsMatch(row.Cells[2].Value.ToString(), @"^\d+(\.\d+)?$"))
                                    ngNum = Convert.ToDouble(row.Cells[2].Value.ToString());
                                row.Cells[2].Value = ngNum;
                                row.Cells[3].Value = "--";
                                row.Cells[6].Value = "--";
                                row.Cells[7].Value = "--";
                            }
                            if (AllNum2 == 0)
                                row.Cells[5].Value = "0.00%";
                            else
                                row.Cells[5].Value = (Convert.ToDouble(row.Cells[2].Value) / AllNum2 * 100).ToString("F2") + "%"; //百分比
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog("更新结果列表2失败", ex);
                }
            }));
        }

        #endregion

        #region 异步保存图片
        private async void saveImage1(string okStatus, string ngStatus, string result, List<string> NGItems)
        {
            await Task.Run(() =>
                {
                    try
                    {
                        string imageName = null;
                        bool is_enableCode = false;
                        //读控件的值
                        if (cbEnableCode.InvokeRequired)
                        {
                            cbEnableCode.Invoke(new Action(() =>
                            {
                                is_enableCode = cbEnableCode.Checked;
                            }));
                        }
                        string address = "";
                        string lenghth = "";
                        //读控件的值
                        if (tbCodeAddress.InvokeRequired)
                        {
                            tbCodeAddress.Invoke(new Action(() =>
                            {
                                address = tbCodeAddress.Text;
                            }));
                        }
                        if (tbCodeLength.InvokeRequired)
                        {
                            tbCodeLength.Invoke(new Action(() =>
                            {
                                lenghth = tbCodeLength.Text;
                            }));
                        }

                        if (is_enableCode && !string.IsNullOrEmpty(address) && !string.IsNullOrEmpty(lenghth))
                        {
                            imageName = panasonicMewtocol1.ReadString(address, ushort.Parse(lenghth)).Content;
                            imageName = imageName.Replace(":", "");
                            imageName = imageName.Replace(".", "");
                            this.BeginInvoke(new Action(() => { tbCodeResult.Text = imageName; }));
                        }
                        else
                            imageName = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");
                        string savePath = IniHelper.SaveSetIni.Read("图片设置", "保存路径");
                        string nowDataTime = DateTime.Now.ToString("yyyyMMdd");

                        if (okStatus == "T" && result == "OK")
                        {
                            string path = savePath + "\\" + "相机1" + "\\" + nowDataTime + "\\" + "OK" + "\\";
                            if (!Directory.Exists(path))
                                Directory.CreateDirectory(path);
                            string imagePath = HalconHelper.fileNameIsExist(path, imageName);
                            HalconHelper.saveImage(hWindowControl1.HalconWindow, ho_image1, imagePath);
                        }
                        else if (ngStatus == "T" && result == "NG")
                        {
                            foreach (var item in NGItems)
                            {
                                string path = savePath + "\\" + "相机1" + "\\" + nowDataTime + "\\" + $"{item}" + "\\";
                                if (!Directory.Exists(path))
                                    Directory.CreateDirectory(path);
                                string imagePath = HalconHelper.fileNameIsExist(path, imageName);
                                HalconHelper.saveImage(hWindowControl1.HalconWindow, ho_image1, imagePath);
                            }
                        }
                    }
                    catch(Exception e)
                    {
                        tbLog1.BeginInvoke(GetMyDelegateLog, tbLog1, "读码错误："+e.Message, e);
                    }
                    
                });        
        }
        private async void saveImage2(string okStatus, string ngStatus, string result, List<string> NGItems1)
        {
            await Task.Run(() =>
            {
                try
                {
                    string imageName = null;
                    bool is_enableCode = false;
                    //读控件的值
                    if (cbEnableCode.InvokeRequired)
                    {
                        cbEnableCode.Invoke(new Action(() =>
                        {
                            is_enableCode = cbEnableCode.Checked;
                        }));
                    }
                    string address = "";
                    string lenghth = "";
                    //读控件的值
                    if (tbCodeAddress.InvokeRequired)
                    {
                        tbCodeAddress.Invoke(new Action(() =>
                        {
                            address = tbCodeAddress.Text;
                        }));
                    }
                    if (tbCodeLength.InvokeRequired)
                    {
                        tbCodeLength.Invoke(new Action(() =>
                        {
                            lenghth = tbCodeLength.Text;
                        }));
                    }

                    if (is_enableCode && !string.IsNullOrEmpty(address) && !string.IsNullOrEmpty(lenghth))
                    {
                        imageName = panasonicMewtocol1.ReadString(address, ushort.Parse(lenghth)).Content;
                        imageName = imageName.Replace(":", "");
                        imageName = imageName.Replace(".", "");
                        this.BeginInvoke(new Action(() => { tbCodeResult.Text = imageName; }));
                    }
                    else
                        imageName = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");
                    string savePath = IniHelper.SaveSetIni.Read("图片设置", "保存路径");
                    string nowDataTime = DateTime.Now.ToString("yyyyMMdd");

                    if (okStatus == "T" && result == "OK")
                    {
                        string path = savePath + "\\" + "相机2" + "\\" + nowDataTime + "\\" + "OK" + "\\";
                        if (!Directory.Exists(path))
                            Directory.CreateDirectory(path);
                        string imagePath = HalconHelper.fileNameIsExist(path, imageName);
                        HalconHelper.saveImage(hWindowControl2.HalconWindow, ho_image2, imagePath);
                    }
                    else if (ngStatus == "T" && result == "NG")
                    {
                        foreach (var item in NGItems1)
                        {
                            string path = savePath + "\\" + "相机2" + "\\" + nowDataTime + "\\" + $"{item}" + "\\";
                            if (!Directory.Exists(path))
                                Directory.CreateDirectory(path);
                            string imagePath = HalconHelper.fileNameIsExist(path, imageName);
                            HalconHelper.saveImage(hWindowControl2.HalconWindow, ho_image2, imagePath);
                        }
                    }
                }
                catch (Exception e)
                {
                    tbLog2.BeginInvoke(GetMyDelegateLog, tbLog2, "读码错误：" + e.Message, e);
                }
            });
        }
        #endregion

        #region 实施设置曝光
        string camStatus;
        volatile bool currentIsSetExpose = false;
        private void 实时读取设置曝光ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentIsSetExpose = true;
            camStatus = "相机1";
            grabImage = new Thread(grabImageThread);
            grabImage.IsBackground = true;
            grabImage.Start();
            frmCameraExposeSet exposeSet = new frmCameraExposeSet("相机1");
            exposeSet.ShowDialog();
        }

        private void grabImageThread()
        {
            try
            {
                while (isRunning && currentIsSetExpose)
                 {
                    switch (camStatus)
                    {
                        case "相机1":
                            MyCamera.MV_FRAME_OUT stFrameOut = new MyCamera.MV_FRAME_OUT();
                            getImgAndDisply(cameraList[cameraIndex % 2], ref hWindowControl1, out ho_image1,ref stFrameOut, HWindow_Location.LeftWindow);
                            cameraList[cameraIndex % 2].MV_CC_FreeImageBuffer_NET(ref stFrameOut);
                            break;
                        case "相机2":
                            MyCamera.MV_FRAME_OUT stFrameOut1 = new MyCamera.MV_FRAME_OUT();
                            getImgAndDisply(cameraList[(cameraIndex + 1) % 2], ref hWindowControl2, out ho_image2,ref stFrameOut1, HWindow_Location.RightWindow);
                            cameraList[(cameraIndex + 1) % 2].MV_CC_FreeImageBuffer_NET(ref stFrameOut1);
                            break;
                        default:
                            break;
                    }
                 }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("相机曝光失败",ex);
            }
        }
        private void exposeSet(float expose,string camName)
        {
            switch (camName)
            {
                case "相机1":
                    cameraList[cameraIndex % 2].MV_CC_SetEnumValue_NET("ExposureAuto", 0);
                    nRet1 = cameraList[cameraIndex % 2].MV_CC_SetFloatValue_NET("ExposureTime", expose);
                    if (nRet1 == MyCamera.MV_OK)
                    {
                        IniHelper.SaveSetIni.Write("相机1设置", "曝光量", expose.ToString());
                        showLog(tbLog1, $"相机1设置曝光成功:{expose}", null);
                    }
                    else
                    {
                        showLog(tbLog1, "相机1设置曝光失败...",null);
                        MessageBox.Show("Set Exposure Time Fail!");
                    }
                    break;
                case "相机2":
                    cameraList[(cameraIndex + 1) % 2].MV_CC_SetEnumValue_NET("ExposureAuto", 0);
                    nRet2 = cameraList[(cameraIndex + 1) % 2].MV_CC_SetFloatValue_NET("ExposureTime", expose);
                    if (nRet2 == MyCamera.MV_OK)
                    {
                        IniHelper.SaveSetIni.Write("相机2设置", "曝光量", expose.ToString());
                    }
                    else
                    {
                        showLog(tbLog2, $"相机2设置设置失败...", null);
                        MessageBox.Show("Set Exposure Time Fail!");
                    }
                    break;
                default:
                    break;
            }
        }
        private void exposeClose()
        {
            currentIsSetExpose = false;
            grabImage?.Join();
        }
        private void 实时设置曝光ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentIsSetExpose = true;
            camStatus = "相机2";
            grabImage = new Thread(grabImageThread);
            grabImage.IsBackground = true;
            grabImage.Start();
            frmCameraExposeSet exposeSet = new frmCameraExposeSet("相机2");
            exposeSet.ShowDialog();
        }
        #endregion

        private void writeJsonFileForNewPathFile(string path1,string path2)
        {
            // 模型1
            //更新ngdic
            addNGDic1();
            treeFileHelper1.filePath = path1;
            NGTypePara.NGType ngTypes = new NGTypePara.NGType();
            foreach (var item1 in treeFileHelper1.dic_nodeIdWithNodeInfo)
            {
                if (item1.Value.ParentsNodeId > -1 && item1.Value.NodeType == 1)
                    continue;
                foreach (var item2 in item1.Value.ClassNames)
                {
                    if (item2.StartsWith("其他") || item2.Equals("OK") || item2.Equals("Ok")
                        || item2.Equals("ok"))
                    {
                        NGTypePara.NGTypeConfig nGTypePara = new NGTypePara.NGTypeConfig();
                        nGTypePara.Node = item1.Value.NodeName;
                        nGTypePara.NGType = item2;
                        nGTypePara.OutType = "缺陷类别1";
                        nGTypePara.isOK = true;
                        ngTypes.NGTypeConfigs.Add(nGTypePara);
                    }else
                    {
                        NGTypePara.NGTypeConfig nGTypePara = new NGTypePara.NGTypeConfig();
                        nGTypePara.Node = item1.Value.NodeName;
                        nGTypePara.NGType = item2;
                        nGTypePara.OutType = "缺陷类别1";
                        nGTypePara.isOK = false;
                        ngTypes.NGTypeConfigs.Add(nGTypePara);
                    }
                }
            }
            string json = JsonConvert.SerializeObject(ngTypes, Formatting.Indented);
            File.WriteAllText(rootDirectory1, json);

            // 模型2
            //更新ngdic
            addNGDic2();
            treeFileHelper2.filePath = path2;
            NGTypePara.NGType ngTypes2 = new NGTypePara.NGType();
            foreach (var item1 in treeFileHelper2.dic_nodeIdWithNodeInfo)
            {
                if (item1.Value.ParentsNodeId > -1 && item1.Value.NodeType == 1)
                    continue;
                foreach (var item2 in item1.Value.ClassNames)
                {
                    if (item2.StartsWith("其他") || item2.Equals("OK") || item2.Equals("Ok") 
                        || item2.Equals("ok"))
                    {
                        NGTypePara.NGTypeConfig nGTypePara = new NGTypePara.NGTypeConfig();
                        nGTypePara.Node = item1.Value.NodeName;
                        nGTypePara.NGType = item2;
                        nGTypePara.OutType = "缺陷类别1";
                        nGTypePara.isOK = true;
                        ngTypes2.NGTypeConfigs.Add(nGTypePara);
                    }
                    else{
                        NGTypePara.NGTypeConfig nGTypePara = new NGTypePara.NGTypeConfig();
                        nGTypePara.Node = item1.Value.NodeName;
                        nGTypePara.NGType = item2;
                        nGTypePara.OutType = "缺陷类别1";
                        nGTypePara.isOK = false;
                        ngTypes2.NGTypeConfigs.Add(nGTypePara);
                    }
                }
            }
            string json2 = JsonConvert.SerializeObject(ngTypes2, Formatting.Indented);
            File.WriteAllText(rootDirectory2, json2);
        }

        #region 更改深度学习配置文件后重写信号输出配置文件
        private void loadNewFile(List<string> paths)
        {
            Task.Run(() =>
            {   
                if(paths.Count == 1)
                {
                    string newPath = paths[0];
                    // 1.写文件
                    writeJsonFileForNewPathFile(newPath, newPath);

                    // 2.释放模型空间
                    if (studyHandle != IntPtr.Zero)
                        HTCSharpDemo.Program.ReleaseTree(studyHandle);
                    if (studyHandle2 != IntPtr.Zero && studyHandle != studyHandle2)
                        HTCSharpDemo.Program.ReleaseTree(studyHandle2);
                    studyHandle = IntPtr.Zero;
                    studyHandle2 = IntPtr.Zero;
                    ptr1 = IntPtr.Zero;
                    ptr2 = IntPtr.Zero;

                    // 3.加载新模型
                    for (int i = 0; i < 100; i++)
                    {
                        progressBar1.BeginInvoke(new Action(() => { progressBar1.Value = i; }));
                        if (i == 40)
                        {
                            configPath = newPath;
                            Dictionary<int, NodeInfo> dic_nodeIdWithNodeInfo = treeFileHelper1.dic_nodeIdWithNodeInfo;
                            if (!(HTCSharpDemo.Program.loadDeepStudyHandle(configPath, ref studyHandle,ref dic_nodeIdWithNodeInfo)))
                            {
                                progressBar1.BeginInvoke(new Action(() =>
                                {
                                    progressBar1.Value = 0;
                                }));
                                MessageBox.Show("方案加载失败");
                                break;
                            }
                            else
                            {
                                progressBar1.BeginInvoke(new Action(() =>
                                {
                                    ptr1 = studyHandle;
                                    ptr2 = studyHandle;
                                    configPath = newPath;
                                    configPath2 = newPath;
                                    startCom1();
                                    startCom2();
                                    btClearDate.Enabled = false;
                                }));
                            }
                        }
                    }

                    //4. 更新表格
                    dataGridView_left.BeginInvoke(updateDataViewData, 3);
                    dataGridView_right.BeginInvoke(updateDataViewData, 4);

                } else if(paths.Count == 2)
                {
                    string newPath = paths[0];
                    string newPath2 = paths[1];
                    // 1.写文件
                    writeJsonFileForNewPathFile(newPath, newPath2);

                    // 2.释放模型空间
                    if (studyHandle != IntPtr.Zero)
                        HTCSharpDemo.Program.ReleaseTree(studyHandle);
                    if (studyHandle2 != IntPtr.Zero && studyHandle != studyHandle2)
                        HTCSharpDemo.Program.ReleaseTree(studyHandle2);
                    studyHandle = IntPtr.Zero;
                    studyHandle2 = IntPtr.Zero;
                    ptr1 = IntPtr.Zero;
                    ptr2 = IntPtr.Zero;

                    // 3.加载新模型
                    for (int i = 0; i < 100; i++)
                    {
                        progressBar1.BeginInvoke(new Action(() => { progressBar1.Value = i; }));
                        if (i == 40)
                        {
                            configPath = newPath;
                            Dictionary<int, NodeInfo> dic_nodeIdWithNodeInfo1 = treeFileHelper1.dic_nodeIdWithNodeInfo;
                            Dictionary<int, NodeInfo> dic_nodeIdWithNodeInfo2 = treeFileHelper2.dic_nodeIdWithNodeInfo;
                            if (!(HTCSharpDemo.Program.loadDeepStudyHandle(configPath, ref studyHandle,ref dic_nodeIdWithNodeInfo1) 
                            && HTCSharpDemo.Program.loadDeepStudyHandle(configPath2, ref studyHandle2, ref dic_nodeIdWithNodeInfo2)))
                            {
                                progressBar1.BeginInvoke(new Action(() =>
                                {
                                    progressBar1.Value = 0;
                                }));
                                MessageBox.Show("方案加载失败");
                                break;
                            }
                            else
                            {
                                progressBar1.BeginInvoke(new Action(() =>
                                {
                                    ptr1 = studyHandle;
                                    ptr2 = studyHandle2;
                                    configPath = newPath;
                                    configPath2 = newPath2;
                                    startCom1();
                                    startCom2();
                                    btClearDate.Enabled = false;
                                }));
                            }
                        }
                    }

                    //4. 更新表格
                    dataGridView_left.BeginInvoke(updateDataViewData, 3);
                    dataGridView_right.BeginInvoke(updateDataViewData, 4);
                }   
            });
        }
        #endregion

        private void tabControl2_Leave(object sender, EventArgs e)
        {
            IniHelper.SaveSetIni.Write("相机1PLC设置", "拍照信号", tbGrabImageSignal1.Text);
            IniHelper.SaveSetIni.Write("相机1PLC设置", "OK信号", tbOKSignal1.Text);
            IniHelper.SaveSetIni.Write("相机1PLC设置", "准备信号", tbReadySignal1.Text);
        }

        private void tabControl1_Leave(object sender, EventArgs e)
        {
            IniHelper.SaveSetIni.Write("相机2PLC设置", "准备信号", tbReadySignal2.Text);
            IniHelper.SaveSetIni.Write("相机2PLC设置", "OK信号", tbOKSignal2.Text);
            IniHelper.SaveSetIni.Write("相机2PLC设置", "拍照信号", tbGrabImageSignal2.Text);
        }

        private void 读取图片目录toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();

            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                director_left = folderBrowserDialog.SelectedPath;
                string[] searchPatterns = { "*.jpg", "*.bmp" ,"*.png"};
                foreach (string pattern in searchPatterns)
                {
                    string[] files = Directory.GetFiles(director_left, pattern);
                    imagePathFiles_left.AddRange(files);
                } // 假设你只想读取 jpg 文件
                if (imagePathFiles_left.Count == 0)
                {
                    MessageBox.Show("当前目录中没有图片，增加图片或切换目录");
                }
                else
                {
                    //选择图片后直接进入调试模式
                    changeDebugMode(true);
                }
            }
        }

        private void 读取图片目录toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                director_right = folderBrowserDialog.SelectedPath;
                string[] searchPatterns = { "*.jpg", "*.bmp" ,"*.png"};
                foreach (string pattern in searchPatterns)
                {
                    string[] files = Directory.GetFiles(director_right, pattern);
                    imagePathFiles_right.AddRange(files);
                }
                if (imagePathFiles_right.Count == 0)
                {
                    MessageBox.Show("当前目录中没有图片，增加图片或切换目录");
                }
            }
        }


        private void changeDebugMode(bool isDebug)
        {
            if (isDebug)
            {
                MessageBox.Show("当前切换到调试模式，请设置窗口的图片目录");
                IniHelper.SaveSetIni.Write("测试模式", "测试模式", "T");
                trigerDLuseImage = true;
            }
            else
            {
                MessageBox.Show("当前切换到运行模式，将使用相机和串口触发运行");
                IniHelper.SaveSetIni.Write("测试模式", "测试模式", "F");
                trigerDLuseImage = false;
                trigerDLuseImage_isReady = false;
                window_left_pictruePath_isReady = false;
                window_right_pictruePath_isReady = false;
            }
        }

        private void contineDebugMode(bool isContinueDebug)
        {
            if (trigerDLuseImage && isContinueDebug)
            {
                // 创建定时器
                timer = new Timer();
                timer.Interval = 1000;
                timer.Tick += Timer_Tick;
                timer.Start();
            }
            else
            {
                if (timer!=null && timer.Enabled)
                {
                    // 当条件不为 OK 且定时器正在运行时，停止定时器并销毁
                    timer.Stop();
                    timer.Dispose();
                }
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            btHandTest.PerformClick();
        }

        private void updateSet1ClassIsOk(string node_name,string class_id,bool isCheck)
        {

            if (isCheck)
            {
                if (!list_OKType_classId_left.Contains(class_id))
                    list_OKType_classId_left.Add(class_id);
                int index = int.MinValue;
                foreach (DataGridViewRow row in dataGridView_left.Rows)
                {
                    if (row.Cells[1].Value != null && row.Cells[1].Value.Equals(class_id))
                    {
                        index = row.Index;
                        break;
                    }
                }
                if (index != int.MinValue)
                {
                    if (dataViewChangeModel_left == null)
                    {
                        dataViewChangeModel_left = new Dictionary<string, dataViewChangeModel>();
                    }
                    if (!dataViewChangeModel_left.ContainsKey(class_id))
                    {
                        dataViewChangeModel model = new dataViewChangeModel();
                        model.index = index;
                        model.row = dataGridView_left.Rows[index];
                        dataViewChangeModel_left.Add(class_id, model);
                    }

                    dataGridView_left.Rows.RemoveAt(index);
                    dataGridView_left.Refresh();
                }
            }
            else //从okType中移除
            {
                if (list_OKType_classId_left.Contains(class_id))
                {
                    list_OKType_classId_left.Remove(class_id);
                    if (dataViewChangeModel_left != null && dataViewChangeModel_left.ContainsKey(class_id))
                    {
                        dataViewChangeModel model = dataViewChangeModel_left[class_id];
                        // 将行插入指定位置
                        dataGridView_left.Rows.Insert(0, model.row); // 在索引为 0 的位置插入行
                        dataGridView_left.Refresh();
                    }
                    else if (dataViewChangeModel_left == null || !dataViewChangeModel_left.ContainsKey(class_id))
                    {
                        //默认不显示情况，默认没有这个row
                        DataGridViewRow row = new DataGridViewRow();
                        for (int i = 0; i < 8; i++)
                        {
                            DataGridViewTextBoxCell cell = new DataGridViewTextBoxCell();
                            row.Cells.Add(cell);
                        }
                        row.Cells[0].Value = node_name;
                        row.Cells[1].Value = class_id;
                        row.Cells[2].Value = "0";
                        row.Cells[3].Value = "--";
                        row.Cells[4].Value = "0";
                        row.Cells[5].Value = "0";
                        row.Cells[6].Value = "--";
                        row.Cells[7].Value = "--";

                        dataGridView_left.Rows.Insert(0, row);
                        dataGridView_left.Refresh();
                    }
                }
            }
            
        }

        private void my_MouseWhee2(object sender, MouseEventArgs e)
        {
            //HSmartWindowControl控件的区域
            System.Drawing.Rectangle rect = hWindowControl2.RectangleToScreen(hWindowControl2.ClientRectangle);
            //滚动时，鼠标悬停在在HSmartWindowControl控件上
            if (rect.Contains(Cursor.Position))
            {
                //缩放
                hWindowControl2.HSmartWindowControl_MouseWheel(sender, e);
            }
        }

        private void my_MouseWheel(object sender, MouseEventArgs e)
        {
            //HSmartWindowControl控件的区域
            System.Drawing.Rectangle rect = hWindowControl1.RectangleToScreen(hWindowControl1.ClientRectangle);
            //滚动时，鼠标悬停在在HSmartWindowControl控件上
            if (rect.Contains(Cursor.Position))
            {
                //缩放
                hWindowControl1.HSmartWindowControl_MouseWheel(sender, e);
            }
        }
        private void updateSet2ClassIsOk(string node_name, string class_id, bool isCheck)
        {
            if (isCheck)
            {
                if (!list_OKType_classId_right.Contains(class_id))
                    list_OKType_classId_right.Add(class_id);
                int index = int.MinValue;
                foreach (DataGridViewRow row in dataGridView_right.Rows)
                {
                    if (row.Cells[1].Value != null && row.Cells[1].Value.Equals(class_id))
                    {
                        index = row.Index;
                        break;
                    }
                }
                if (index != int.MinValue)
                {
                    if (dataViewChangeModel_right == null)
                    {
                        dataViewChangeModel_right = new Dictionary<string, dataViewChangeModel>();
                    }
                    if (!dataViewChangeModel_right.ContainsKey(class_id))
                    {
                        dataViewChangeModel model = new dataViewChangeModel();
                        model.index = index;
                        model.row = dataGridView_right.Rows[index];
                        dataViewChangeModel_right.Add(class_id, model);
                    }

                    dataGridView_right.Rows.RemoveAt(index);
                    dataGridView_right.Refresh();
                }
            }
            else
            {
                if (list_OKType_classId_right.Contains(class_id))
                {
                    list_OKType_classId_right.Remove(class_id);
                    if (dataViewChangeModel_right != null && dataViewChangeModel_right.ContainsKey(class_id))
                    {
                        dataViewChangeModel model = dataViewChangeModel_right[class_id];
                        // 将行插入指定位置
                        dataGridView_right.Rows.Insert(0, model.row); // 在索引为 0 的位置插入行
                        dataGridView_right.Refresh();
                    }
                    else if (dataViewChangeModel_right == null || !dataViewChangeModel_right.ContainsKey(class_id))
                    {
                        //默认不显示情况，默认没有这个row
                        DataGridViewRow row = new DataGridViewRow();
                        for (int i = 0; i < 8; i++)
                        {
                            DataGridViewTextBoxCell cell = new DataGridViewTextBoxCell();
                            row.Cells.Add(cell);
                        }
                        row.Cells[0].Value = node_name;
                        row.Cells[1].Value = class_id;
                        row.Cells[2].Value = "0";
                        row.Cells[3].Value = "--";
                        row.Cells[4].Value = "0";
                        row.Cells[5].Value = "0";
                        row.Cells[6].Value = "--";
                        row.Cells[7].Value = "--";

                        dataGridView_right.Rows.Insert(0, row);
                        dataGridView_right.Refresh();
                    }
                }
            }
        }

        private void checkBox_camera_CheckedChanged(object sender, EventArgs e)
        {
            if(checkBox_camera.Checked)
            {
                cameraIndex = 1;
            }
            else
            {
                cameraIndex = 0;
            }
        }

        void left_updown_TextChanged(object sender, EventArgs e)
        {
            UpDownBase up = (UpDownBase)sender;
            if (!string.IsNullOrEmpty(up.Text))
            {
                //旋转角度
                if (IsNumber(up.Text))
                    IniHelper.SaveSetIni.Write("旋转角度", "左窗口图像旋转角度",up.Text);
            }
        }

        void right_updown_TextChanged(object sender, EventArgs e)
        {
            UpDownBase up = (UpDownBase)sender;
            if (!string.IsNullOrEmpty(up.Text))
            {
                //旋转角度
                if (IsNumber(up.Text))
                    IniHelper.SaveSetIni.Write("旋转角度", "右窗口图像旋转角度", up.Text);
            }
        }

        private void cb_isTwoSignal_CheckedChanged(object sender, EventArgs e)
        {
            if (cb_isTwoSignal.Checked)
            {
                //双信号
                countdown.Reset(1);
                countdownTwo.Reset(1);
                hasTwoSignal = true;
                OperateResult result1 = panasonicMewtocol1.Write(tbReadySignal2.Text, true);
                if (!result1.IsSuccess)
                {
                    if (tbLog2.IsHandleCreated)
                    {
                        tbLog2.BeginInvoke(GetMyDelegateLog, tbLog2, "准备信号2发送失败，请检查串口", null);
                    }
                }
                Thread runListenSerialPort2Thread = new Thread(runListenSerialPortTwoThread);
                runListenSerialPort2Thread.IsBackground = true;
                runListenSerialPort2Thread.Start();
            }
            else
            {
                //单信号
                countdown.Reset(2);
                countdownTwo.Reset(0);
                hasTwoSignal = false;
            }
        }
    }
}