﻿using HalconDotNet;
using HslCommunication;
using HslCommunication.Profinet.Panasonic;
using INI;
using log4net;
using LW.ZOOM;
using MvCamCtrl.NET;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;
using System.Text.RegularExpressions;
using System.Data.SQLite;
using Microsoft.VisualBasic.Logging;
using static HTCSharpDemo.Program;
using static log4net.Appender.ColoredConsoleAppender;
using Newtonsoft.Json;
using static YUNTIANVision.NGTypePara;
using System.Windows.Shapes;
using System.Configuration;
using System.Linq.Expressions;
using System.Globalization;

namespace YUNTIANVision
{
    public delegate void showXYorGrayDelegate2(string xy,string gray);
    public delegate void updateDataView(int index);
    public delegate void onTimeClearData(bool setNGText);

    public partial class frmTwoCamera : Form
    {
        public frmTwoCamera()
        {
            frmMain.closeAll += new closeDelegate(frmTwoClose);
            frmMain.controlEvent += new controlDelegate(runControl);      
            frmCameraExposeSet.cameraExposeSetEvent += new cameraExposeSet(exposeSet);
            frmCameraExposeSet.cameraExposeCloseEvent += new cameraExposeClose(exposeClose);
            frmSet.newConfigEvent += new newConfigDel(loadNewFile);
            frmSet.newConfigEvent2 += new newConfigDel(loadNewFile2);
            updateDataViewData += updateDV;
            onTimeClearData += onTimeclearAllData;
            InitializeComponent();
        }
        #region 定义对象

        Thread grabOneCameraThread = null;
        Thread grabTwoCameraThread = null;
        Thread listenSerialPortThread = null;
        Thread grabImage = null;

        // <summary>
        /// 相机1OK类型
        /// </summary>
        List<string> OKType1 = new List<string>();
        /// <summary>
        /// 相机1NG类型
        /// </summary>
        Dictionary<string, string> NGType1 = new Dictionary<string, string>();
        // <summary>
        /// 相机2OK类型
        /// </summary>
        List<string> OKType2 = new List<string>();
        /// <summary>
        /// 相机2NG类型
        /// </summary>
        Dictionary<string, string> NGType2 = new Dictionary<string, string>();

        string rootDirectory1 = AppDomain.CurrentDomain.BaseDirectory + "NGTypeSignalSet2_1.txt";
        string rootDirectory2 = AppDomain.CurrentDomain.BaseDirectory + "NGTypeSignalSet2_2.txt";
        frmNGSignalSet set1;
        TreeFileHelper treeFileHelper1;
        TreeFileHelper.NodeInfos nodeInfos1;
        frmNGSignalSet set2;
        TreeFileHelper treeFileHelper2;
        TreeFileHelper.NodeInfos nodeInfos2;

        /// <summary>
        /// 深度学习句柄
        /// </summary>
        IntPtr studyHandle;
        IntPtr studyHandle2;
        /// <summary>
        /// 节点数量
        /// </summary>
        int test_num;
        /// <summary>
        /// 节点2数量
        /// </summary>
        int test_num2;
        /// <summary>
        /// 手动触发拍照
        /// </summary>
        bool isImage;
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
        public static event showXYorGrayDelegate2 showXYorGrayEvent;
        /// <summary>
        /// 显示相机的操作log
        /// </summary>
        /// <param name="textBox"></param>
        /// <param name="log"></param>
        private delegate void myDelegateLog(TextBox textBox, string log);
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
        /// 相机1测试结果
        /// </summary>
        string result1 = null;
        /// <summary>
        /// 相机2测试结果
        /// </summary>
        string result2 = null;
        /// <summary>
        /// 相机1ng信号
        /// </summary>
        public static Dictionary<string, string> ngDic1 = new Dictionary<string, string>();
        /// <summary>
        /// 相机2ng信号
        /// </summary>
        public static Dictionary<string, string> ngDic2 = new Dictionary<string, string>();

        string xy = null;
        string gray = null;
        updateDataView updateDataViewData = null;
        onTimeClearData onTimeClearData = null;

        private static ManualResetEventSlim event1 = new ManualResetEventSlim(false);
        private static CountdownEvent countdown = new CountdownEvent(2);

        private static volatile bool b_isDiskFull = false; //存图设置

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

        #region 加载方案
        IntPtr ptr1 = IntPtr.Zero;
        IntPtr ptr2 = IntPtr.Zero;

        private static volatile bool b_cam1OpenSucess = true;
        private static volatile bool b_cam2OpenSucess = true;

        #endregion

        #region load/close事件

        private void frmTwoCamera_Load(object sender, EventArgs e)
        {
            createHead1();
            createHead2();

            configPath = IniHelper.SaveSetIni.Read("深度学习文件", "配置文件路径");
            bool b_model1isNull = false; //configPath1为空
            bool b_model2isNull = false; //两个路径都为空
            if (!string.IsNullOrEmpty(configPath))
            {
                set1 = new frmNGSignalSet("NG2", configPath);
                treeFileHelper1 = set1.getTreeFileHelper();
                nodeInfos1 = treeFileHelper1.LoadTreeFile(configPath);
                addNGDic1();
                addNGorOK1();
                addRows(configPath, treeFileHelper1);
            }
            else
            {
                b_model1isNull = true;
                tbLog1.BeginInvoke(GetMyDelegateLog, tbLog1, "深度学习配置文件路径不存在");
                MessageBox.Show("深度学习文件配置文件路径不存在");
            }
            configPath2 = IniHelper.SaveSetIni.Read("深度学习文件", "配置文件路径1");
            
            if (!string.IsNullOrEmpty(configPath2))
            {
                set2 = new frmNGSignalSet("NG3", configPath2);
                addNGDic2();
                addNGorOK2();
                treeFileHelper2 = set2.getTreeFileHelper();
                nodeInfos2 = treeFileHelper1.LoadTreeFile(configPath2);
                hasTwoModel = true;
                addRows(configPath2, treeFileHelper2);
            }
            else if(!b_model1isNull)
            {
                set2 = new frmNGSignalSet("NG3", configPath);
                addNGDic2();
                addNGorOK2();
                treeFileHelper2 = set2.getTreeFileHelper();
                nodeInfos2 = nodeInfos1;
                b_model2isNull = true;
                addRows(configPath, treeFileHelper2);
                hasTwoModel = false;
            }
            else
            {
                b_model2isNull = true; //两个路径都为空
                tbLog1.BeginInvoke(GetMyDelegateLog, tbLog1, "深度学习配置文件路径2不存在");
                MessageBox.Show("深度学习文件配置文件路径2不存在");
            }

            // btLoadConfig_Click(sender, e);

            set1.loadNGEvent += new loadNGSignalDic(addNGDic1);
            set2.loadNGEvent += new loadNGSignalDic(addNGDic2);
            set1.loadNGEvent += new loadNGSignalDic(addNGorOK1);
            set2.loadNGEvent += new loadNGSignalDic(addNGorOK2);
            GetMyDelegateLog = new myDelegateLog(showLog);
            GetMyDelegatePlc = new myDelegatePlc(showPlcStatus);


            loadCodeSet();
            loadResult();

            initDataOnTimeAutoClear();

            loadOneCameraSet();
            loadTwoCameraSet();

            dataGridView3.RowHeadersVisible = false;
            dataGridView4.RowHeadersVisible = false;

            Task openCamsTask = new Task(() =>
            {
                //开相机
                openOneHikVisionCam();
                openTwoHikVisionCam();
                loadOnePort(); //串口1
            });
            Task loadDLModelTask = new Task(() =>
           {
               // 1.加载模型
               tbLog1.BeginInvoke(GetMyDelegateLog, tbLog1, "模型加载中，请稍后...");
               tbLog2.BeginInvoke(GetMyDelegateLog, tbLog2, "模型加载中，请稍后...");

               bool b_sucess = loadDlModel(b_model1isNull, b_model2isNull);

               tbLog1.BeginInvoke(GetMyDelegateLog, tbLog1, "模型加载完成...");
               tbLog2.BeginInvoke(GetMyDelegateLog, tbLog2, "模型加载完成...");
               // 2.开线程
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
            isRunning = false;
            // 两个相机的线程此时应该关闭
            Thread.Sleep(100); //等待让两个相机线程能感知到
            if (grabOneCameraThread != null)
                grabOneCameraThread.Join();
            if(grabTwoCameraThread != null)
                grabTwoCameraThread.Join();
            if(listenSerialPortThread != null)
                listenSerialPortThread.Join();

            Task closeCams = new Task(() =>
            {
                closeOneHikVisionCam();
                closeTwoHikVisionCam();
            });
            closeCams.Start();

            Task closePlcs = new Task(() =>
            {
                if (panasonicMewtocol1.IsOpen())
                {
                    panasonicMewtocol1.Write(tbReadySignal1.Text, false);
                    if (hasTwoModel)//双信号
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
                    if (studyHandle2 != IntPtr.Zero)
                        HTCSharpDemo.Program.ReleaseTree(studyHandle2);
                }
            });
            closeDlHandle.Start();

            Task.WaitAll(closeCams, closePlcs, closeDlHandle);
            IniHelper.SaveSetIni.Write("布局", "窗口数量", "2");
            frmMain.controlEvent -= new controlDelegate(runControl);
            set1.loadNGEvent -= new loadNGSignalDic(addNGDic1);
            set2.loadNGEvent -= new loadNGSignalDic(addNGDic2);
            set1.loadNGEvent -= new loadNGSignalDic(addNGorOK1);
            set2.loadNGEvent -= new loadNGSignalDic(addNGorOK2);
            frmSet.newConfigEvent -= new newConfigDel(loadNewFile);
            frmSet.newConfigEvent2 -= new newConfigDel(loadNewFile2);
            frmMain.closeAll -= new closeDelegate(frmTwoClose);
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
            if (!String.IsNullOrEmpty(IniHelper.SaveSetIni.Read("相机1", "NG信号1")))
                lbAddNGSet1.Text = IniHelper.SaveSetIni.Read("相机1", "NG信号1");
            cbGrayOrRgb1.Items.Add("黑白");
            cbGrayOrRgb1.Items.Add("彩色");
            if (String.IsNullOrEmpty(IniHelper.SaveSetIni.Read("相机设置", "采图设置")))
                cbGrayOrRgb1.SelectedIndex = 0;
            else
            {
                if (IniHelper.SaveSetIni.Read("相机设置", "采图设置") == "彩色")
                    cbGrayOrRgb1.SelectedIndex = 1;
                else
                    cbGrayOrRgb1.SelectedIndex = 0;
            }

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
        }

        // 初始化相机2设置
        private void loadTwoCameraSet()
        {
            tbOKSignal2.Text = IniHelper.SaveSetIni.Read("相机2PLC设置", "OK信号");
            tbReadySignal2.Text = IniHelper.SaveSetIni.Read("相机2PLC设置", "准备信号");
            tbGrabImageSignal2.Text = IniHelper.SaveSetIni.Read("相机2PLC设置", "拍照信号");

            /// 双信号设置 ///
            string twoSignal = IniHelper.SaveSetIni.Read("双信号设置", "是否启用");
            if (!String.IsNullOrEmpty(twoSignal) && twoSignal == "T")
            {
                controlSignal2(true);
            }
            else
            {
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
                }
                OperateResult<bool> res = panasonicMewtocol1.ReadBool(tbGrabImageSignal1.Text);
                if (res.Content || isImage)
                {
                    // 触发线程2和线程3执行
                    event1.Set();

                    // 等待线程2和线程3完成
                    countdown.Wait();

                    // 重置事件和倒计时器
                    event1.Reset();
                    countdown.Reset();
                }
                else
                {
                    //检查磁盘空间
                    if (!isFreeSpace(imgSavePath))
                    {
                        //空间满了
                        b_isDiskFull = true;
                    }
                    else
                        b_isDiskFull = false;

                    // 数据到点清零
                    if (isUseClearDataTime)
                    {
                        DateTime now = DateTime.Now;
                        IFormatProvider ifp = new CultureInfo("zh-CN", true);
                        DateTime day_time = DateTime.ParseExact(day_clearDataTime, "HH:mm:ss", ifp);
                        DateTime night_time = DateTime.ParseExact(night_clearDataTime, "HH:mm:ss", ifp);

                        TimeSpan daytimeDifference = (day_time - now).Duration();
                        TimeSpan nighttimeDifference = (night_time - now).Duration();
                        if (daytimeDifference.Hours == 0 && daytimeDifference.Seconds == 0)
                        {
                            if (daytimeDifference.Seconds < 2)
                            {
                                //计数清零
                                this.BeginInvoke(onTimeClearData, false);
                            }
                        }
                        else if (nighttimeDifference.Hours == 0 && nighttimeDifference.Seconds == 0)
                        {
                            if (nighttimeDifference.Seconds < 2)
                            {
                                //计数清零
                                this.BeginInvoke(onTimeClearData, false);
                            }
                        }
                    }
                }
            }
        }

        private void runGrabOneCameraThread()
        {
            try
            {
                while (isRunning)
                {
                    // 等待线程plc监控线程触发
                    event1.Wait();

                    this.BeginInvoke(new Action(() => { btHandTest.Enabled = false; }));
                    tbLog1.BeginInvoke(GetMyDelegateLog, tbLog1, "相机1收到拍照信号...");
                    DateTime startTime;
                    DateTime endTime;
                    HTCSharpDemo.Program DeepLearning = new HTCSharpDemo.Program();
                    startTime = DateTime.Now;
                    tbLog1.BeginInvoke(GetMyDelegateLog, tbLog1, "相机1开始拍照...");

                    grabOneImage(out ho_image1);
                    oneImageToHalcon(ho_image1);
                    tbLog1.BeginInvoke(GetMyDelegateLog, tbLog1, "相机1拍照结束...");

                    HTCSharpDemo.Program.ImageHt imageHt = new HTCSharpDemo.Program.ImageHt();
                    imageHt.data = pBufForDriver1;
                    imageHt.width = hv_Width1;
                    imageHt.height = hv_Height1;
                    string grabImageSet = IniHelper.SaveSetIni.Read("相机设置", "采图设置");
                    if (grabImageSet == "黑白")
                    {
                        imageHt.channels = 1;
                        imageHt.width_step = imageHt.width;
                    }
                    else if (grabImageSet == "彩色")
                    {
                        imageHt.channels = 3;
                        imageHt.width_step = imageHt.width * 3;
                    }
                    DeepLearning.DeepStudy1(configPath, ptr1, imageHt, test_num, treeFileHelper1);

                    tbLog1.BeginInvoke(GetMyDelegateLog, tbLog1, "深度学习处理结束...");
                    AllNum1++;
                    IniHelper.SaveSetIni.Write("相机1生产信息", "生产总数", AllNum1.ToString());
                    if (DeepLearning.res1[treeFileHelper1.dingweiNodeId].Count == 1)
                    {
                        if (!DeepLearning.NGItems1.Intersect(NGType1.Keys).Any())
                        {
                            OKNum1++;
                            result1 = "OK";
                            IniHelper.SaveSetIni.Write("相机1生产信息", "OK总数", OKNum1.ToString());
                        }
                        else
                        {
                            NGNum1++;
                            result1 = "NG";
                            IniHelper.SaveSetIni.Write("相机1生产信息", "NG总数", NGNum1.ToString());
                        }
                    }
                    else
                    {
                        NGNum1++;
                        result1 = "定位失败";
                        IniHelper.SaveSetIni.Write("相机1生产信息", "NG总数", NGNum1.ToString());
                    }
                    ProcResult1(DeepLearning.NGItems1);
                    showNGLocation1(DeepLearning.res1);
                    endTime = DateTime.Now;
                    TimeSpan ct = endTime - startTime;
                    ct1 = (int)ct.TotalMilliseconds;

                    tbLog1.BeginInvoke(GetMyDelegateLog, tbLog1, "正在输出相机1结果...");
                    showResult1();
                    referenceTable1(DeepLearning.res1);
                    tbLog1.BeginInvoke(GetMyDelegateLog, tbLog1, "相机1结果输出完成...");

                    string okStatus = IniHelper.SaveSetIni.Read("图片路径", "OK图片标志");
                    string ngStatus = IniHelper.SaveSetIni.Read("图片路径", "NG图片标志");
                    if (okStatus == "T" || ngStatus == "T")
                    {
                        tbLog1.BeginInvoke(GetMyDelegateLog, tbLog1, "正在保存图片...");
                        saveImage1(okStatus, ngStatus, result1, DeepLearning.NGItems1);
                    }

                    HOperatorSet.GetImageSize(ho_image1, out hv_Width1, out hv_Height1);
                    HalconHelper.imageLocation(hv_Width1, hv_Height1, hWindowControl1.Width, hWindowControl1.Height, out row1, out column1, out row2, out column2);
                    zi1 = new ZoomImage(row1, column1, row2, column2, hv_Width1, hv_Height1, this.hWindowControl1);

                    if (isImage)
                    {
                        isImage = false;
                        // 处理期间btHandTest = false
                        this.BeginInvoke(new Action(() => { btHandTest.Enabled = true; }));
                    }

                    // 通知线程plc线程完成
                    countdown.Signal();
                }

            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message);
            }
        }

        private void rungrabTwoCameraThread()
        {
            try
            {
                while (isRunning)
                {
                    // 等待plc线程触发
                    event1.Wait();

                    tbLog2.BeginInvoke(GetMyDelegateLog, tbLog2, "相机2收到拍照信号...");
                    DateTime startTime;
                    DateTime endTime;
                    int step = 1;
                    HTCSharpDemo.Program DeepLearning = new HTCSharpDemo.Program();

                    startTime = DateTime.Now;

                    tbLog2.BeginInvoke(GetMyDelegateLog, tbLog2, "相机2开始拍照...");
                    grabTwoImage(out ho_image2);
                    twoImageToHalcon(ho_image2);
                    tbLog2.BeginInvoke(GetMyDelegateLog, tbLog2, "相机2拍照结束...");

                    HTCSharpDemo.Program.ImageHt imageHt = new HTCSharpDemo.Program.ImageHt();
                    imageHt.data = pBufForDriver2;
                    imageHt.width = hv_Width2;
                    imageHt.height = hv_Height2;
                    if (IniHelper.SaveSetIni.Read("相机设置", "采图设置") == "黑白")
                    {
                        imageHt.channels = 1;
                        imageHt.width_step = imageHt.width;
                    }
                    else if (IniHelper.SaveSetIni.Read("相机设置", "采图设置") == "彩色")
                    {
                        imageHt.channels = 3;
                        imageHt.width_step = imageHt.width * 3;
                    }
                    //TreeFileHelper.NodeInfos nodeInfo = new TreeFileHelper.NodeInfos();
                    //nodeInfo.NodeInfo = TreeFileHelper.LoadTreeFile(configPath).NodeInfo;
                    tbLog2.BeginInvoke(GetMyDelegateLog, tbLog2, "深度学习处理开始...");
                    DeepLearning.DeepStudy2(configPath, ptr2, imageHt, test_num2, treeFileHelper2);
                    tbLog2.BeginInvoke(GetMyDelegateLog, tbLog2, "深度学习处理完成...");
                    AllNum2++;
                    IniHelper.SaveSetIni.Write("相机2生产信息", "生产总数", AllNum2.ToString());
                    if (DeepLearning.res2[treeFileHelper2.dingweiNodeId].Count == 1)
                    {
                        if (!DeepLearning.NGItems2.Intersect(NGType2.Keys).Any())
                        {
                            OKNum2++;
                            result2 = "OK";
                            IniHelper.SaveSetIni.Write("相机2生产信息", "OK总数", OKNum2.ToString());
                        }
                        else
                        {
                            NGNum2++;
                            result2 = "NG";
                            IniHelper.SaveSetIni.Write("相机2生产信息", "NG总数", NGNum2.ToString());
                        }
                    }
                    else
                    {
                        NGNum2++;
                        result2 = "定位失败";
                        IniHelper.SaveSetIni.Write("相机2生产信息", "NG总数", NGNum2.ToString());
                    }
                    ProcResult2(DeepLearning.NGItems2);
                    showNGLocation2(DeepLearning.res2);
                    endTime = DateTime.Now;
                    TimeSpan ct = endTime - startTime;
                    ct2 = (int)ct.TotalMilliseconds;
    
                    tbLog2.BeginInvoke(GetMyDelegateLog, tbLog2, "正在输出相机2结果...");
                    showResult2();
                    referenceTable2(DeepLearning.res2);
                    tbLog2.BeginInvoke(GetMyDelegateLog, tbLog2, "相机2结果输出完成...");
                     
                    string okStatus = IniHelper.SaveSetIni.Read("图片路径", "OK图片标志");
                    string ngStatus = IniHelper.SaveSetIni.Read("图片路径", "NG图片标志");
                    if (okStatus == "T" || ngStatus == "T")
                    {
                        tbLog2.BeginInvoke(GetMyDelegateLog, tbLog2, "正在保存图片...");
                        saveImage2(okStatus, ngStatus, result2, DeepLearning.NGItems2);
                    }
               
                    HOperatorSet.GetImageSize(ho_image2, out hv_Width2, out hv_Height2);
                    HalconHelper.imageLocation(hv_Width2, hv_Height2, hWindowControl2.Width, hWindowControl2.Height, out row3, out column3, out row4, out column4);
                    zi2 = new ZoomImage(row3, column3, row4, column4, hv_Width2, hv_Height2, this.hWindowControl2);

                    // 通知plc线程完成
                    countdown.Signal();
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message);
            }
        }
        #endregion

        #region 功能设置
        // 加载模型
        private bool loadDlModel(bool modlePath1isNull,bool twomodleisNull)
        {
            if (!modlePath1isNull) {
                HTCSharpDemo.Program.loadDeepStudyHandle(configPath, ref studyHandle, ref test_num, treeFileHelper1);
                ptr1 = studyHandle;
            }
            if (!twomodleisNull)
            {
                HTCSharpDemo.Program.loadDeepStudyHandle(configPath2, ref studyHandle2, ref test_num2, treeFileHelper2);
                ptr2 = studyHandle2;
            }
            else if (!modlePath1isNull)
            {
                configPath2 = configPath;
                studyHandle2 = studyHandle;
                test_num2 = test_num;
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
        /// 图片左上角row坐标
        /// </summary>
        double row1;
        /// <summary>
        /// 图片左上角column坐标
        /// </summary>
        double column1;
        /// <summary>
        /// 图片右下角row坐标
        /// </summary>
        double row2;
        /// <summary>
        /// 图片右下角column坐标
        /// </summary>
        double column2;
        /// <summary>
        /// 相机1拍到的图像变量
        /// </summary>
        HObject ho_image1;
        /// <summary>
        /// ho_image1的宽
        /// </summary>
        HTuple hv_Width1 = new HTuple();
        /// <summary>
        /// ho_image1的高
        /// </summary>
        HTuple hv_Height1 = new HTuple();
        MyCamera device1 = new MyCamera();
        int nRet1 = MyCamera.MV_OK;
        MyCamera.MV_CC_DEVICE_INFO stDevInfo1; // 通用设备信息
        MyCamera.MVCC_INTVALUE stParam1;
        MyCamera.MV_FRAME_OUT_INFO_EX FrameInfo1;
        UInt32 nPayloadSize1;
        IntPtr pBufForDriver1;

        private volatile static bool isRunning = true;

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

                stDevInfo1 = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(stDevList.pDeviceInfo[0], typeof(MyCamera.MV_CC_DEVICE_INFO));
                // ch:创建设备 | en:Create device
                nRet1 = device1.MV_CC_CreateDevice_NET(ref stDevInfo1) + nRet1;

                // ch:打开设备 | en:Open device
                nRet1 = device1.MV_CC_OpenDevice_NET() + nRet1;

                // ch:开启抓图 | en:start grab
                nRet1 = device1.MV_CC_StartGrabbing_NET() + nRet1;

                // ch:获取包大小 || en: Get Payload Size
                stParam1 = new MyCamera.MVCC_INTVALUE();
                nRet1 = device1.MV_CC_GetIntValue_NET("PayloadSize", ref stParam1) + nRet1;
                if (MyCamera.MV_OK == nRet1)
                {
                    tbLog1.BeginInvoke(GetMyDelegateLog, tbLog1, "相机1初始化完成...");
                }
                else
                {
                    tbLog1.BeginInvoke(GetMyDelegateLog, tbLog1, "相机1初始化失败...");
                    b_cam1OpenSucess = false;
                }
                nPayloadSize1 = stParam1.nCurValue;
                pBufForDriver1 = Marshal.AllocHGlobal((int)nPayloadSize1);
                FrameInfo1 = new MyCamera.MV_FRAME_OUT_INFO_EX();
            }
            catch (Exception ex)
            {
                tbLog1.BeginInvoke(GetMyDelegateLog, tbLog1, "相机1初始化失败...");
                LogHelper.WriteLog(ex.Message);
            }
        }

        private void closeOneHikVisionCam()
        {
            try
            {
                Marshal.FreeHGlobal(pBufForDriver1);

                if (b_cam1OpenSucess)
                {
                    // ch:停止抓图 | en:Stop grab image
                    nRet1 = device1.MV_CC_StopGrabbing_NET();
                }

                // ch:关闭设备 | en:Close device
                nRet1 = device1.MV_CC_CloseDevice_NET();

                // ch:销毁设备 | en:Destroy device
                nRet1 = device1.MV_CC_DestroyDevice_NET();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                LogHelper.WriteLog(ex.ToString());
            }
        }
        private void grabOneImage(out HObject image)
        {
            HOperatorSet.GenEmptyObj(out image);
            try
            {
                nRet1 = device1.MV_CC_GetOneFrameTimeout_NET(pBufForDriver1, nPayloadSize1, ref FrameInfo1, 1000);
                image.Dispose();
                if (IniHelper.SaveSetIni.Read("相机设置", "采图设置") == "黑白" || String.IsNullOrEmpty(IniHelper.SaveSetIni.Read("相机设置", "采图设置")))
                    HOperatorSet.GenImage1Extern(out image, "byte", FrameInfo1.nWidth, FrameInfo1.nHeight, pBufForDriver1, 0);
                else if (IniHelper.SaveSetIni.Read("相机设置", "采图设置") == "彩色")
                    HOperatorSet.GenImageInterleaved(out image, pBufForDriver1, "rgb", FrameInfo1.nWidth, FrameInfo1.nHeight,
                    0, "byte", FrameInfo1.nWidth, FrameInfo1.nHeight, 0, 0, -1, 0);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.ToString());
            }
        }
        bool isSet1 = false;
        private void oneImageToHalcon(HObject image)
        {
            try
            {
                hv_Height1.Dispose(); hv_Width1.Dispose();
                HOperatorSet.GetImageSize(image, out hv_Width1, out hv_Height1);
                HalconHelper.imageLocation(hv_Width1, hv_Height1, hWindowControl1.Width, hWindowControl1.Height, out row1, out column1, out row2, out column2);
                if(!isSet1)
                {
                    isSet1 = true;
                    HOperatorSet.SetPart(this.hWindowControl1.HalconWindow, row1, column1, row2, column2);
                }
                HOperatorSet.DispObj(image, this.hWindowControl1.HalconWindow);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.ToString());
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
        MyCamera device2 = new MyCamera();
        int nRet2 = MyCamera.MV_OK;
        MyCamera.MV_CC_DEVICE_INFO stDevInfo2; // 通用设备信息
        MyCamera.MVCC_INTVALUE stParam2;
        MyCamera.MV_FRAME_OUT_INFO_EX FrameInfo2;
        UInt32 nPayloadSize2;
        IntPtr pBufForDriver2;
        #endregion 

        #region 海康相机2SDK
        private void openTwoHikVisionCam()
        {
             try
                {
                    MyCamera.MV_CC_DEVICE_INFO_LIST stDevList = new MyCamera.MV_CC_DEVICE_INFO_LIST();
                    nRet2 = MyCamera.MV_CC_EnumDevices_NET(MyCamera.MV_GIGE_DEVICE | MyCamera.MV_USB_DEVICE, ref stDevList);
                    
                    stDevInfo2 = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(stDevList.pDeviceInfo[1], typeof(MyCamera.MV_CC_DEVICE_INFO));
                    // ch:创建设备 | en:Create device
                    nRet2 = device2.MV_CC_CreateDevice_NET(ref stDevInfo2)+ nRet2;
                    

                    // ch:打开设备 | en:Open device
                    nRet2 = device2.MV_CC_OpenDevice_NET() + nRet2;
                    

                    // ch:开启抓图 | en:start grab
                    nRet2 = device2.MV_CC_StartGrabbing_NET() + nRet2;
                    

                    // ch:获取包大小 || en: Get Payload Size
                    stParam2 = new MyCamera.MVCC_INTVALUE();
                    nRet2 = device2.MV_CC_GetIntValue_NET("PayloadSize", ref stParam2) + nRet2;
                    if (MyCamera.MV_OK == nRet2)
                    {
                        m_bTwostart2 = true;
                        tbLog2.BeginInvoke(GetMyDelegateLog, tbLog2, "相机2初始化完成...");
                    }
                    else
                    {
                        tbLog2.BeginInvoke(GetMyDelegateLog, tbLog2, "相机2初始化失败...");
                        b_cam2OpenSucess = false;
                    }
                    nPayloadSize2 = stParam2.nCurValue;
                    pBufForDriver2 = Marshal.AllocHGlobal((int)nPayloadSize2);
                    FrameInfo2 = new MyCamera.MV_FRAME_OUT_INFO_EX();
               }
                catch (Exception ex)
                {
                    tbLog2.BeginInvoke(GetMyDelegateLog, tbLog2, "相机2初始化失败...");
                    MessageBox.Show("相机2打开失败");
                    LogHelper.WriteLog(ex.ToString());
                }
        }
        private void closeTwoHikVisionCam()
        {
            try
            {
                Marshal.FreeHGlobal(pBufForDriver2);

                if (b_cam2OpenSucess)
                {
                    // ch:停止抓图 | en:Stop grab image
                    nRet2 = device2.MV_CC_StopGrabbing_NET();
                }
                
                // ch:关闭设备 | en:Close device
                nRet2 = device2.MV_CC_CloseDevice_NET();

                // ch:销毁设备 | en:Destroy device
                nRet2 = device2.MV_CC_DestroyDevice_NET();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                LogHelper.WriteLog(ex.ToString());
            }
        }
        private void grabTwoImage(out HObject image)
        {
            HOperatorSet.GenEmptyObj(out image);
            try
            {
                nRet2 = device2.MV_CC_GetOneFrameTimeout_NET(pBufForDriver2, nPayloadSize2, ref FrameInfo2, 1000);
                image.Dispose();
                string tmp = IniHelper.SaveSetIni.Read("相机设置", "采图设置");
                if (tmp == "黑白" || String.IsNullOrEmpty(tmp))
                    HOperatorSet.GenImage1Extern(out image, "byte", FrameInfo2.nWidth, FrameInfo2.nHeight, pBufForDriver2, 0);
                else if (tmp == "彩色")
                    HOperatorSet.GenImageInterleaved(out image, pBufForDriver2, "rgb", FrameInfo2.nWidth, FrameInfo2.nHeight,
                    0, "byte", FrameInfo2.nWidth, FrameInfo2.nHeight, 0, 0, -1, 0);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.ToString());
            }
        }
        bool isSet2 = false;
        private void twoImageToHalcon(HObject image)
        {
            try
            {
                hv_Height2.Dispose(); hv_Width2.Dispose();
                HOperatorSet.GetImageSize(image, out hv_Width2, out hv_Height2);
                HalconHelper.imageLocation(hv_Width2, hv_Height2, hWindowControl2.Width, hWindowControl2.Height, out row3, out column3, out row4, out column4);
                if(!isSet2)
                {
                    isSet2 = true;
                     HOperatorSet.SetPart(this.hWindowControl2.HalconWindow, row3, column3, row4, column4);
                }
                HOperatorSet.DispObj(image, this.hWindowControl2.HalconWindow);
            }
            catch (Exception ex)
            {
                
                LogHelper.WriteLog(ex.ToString());
            }
        }
        #endregion


        


        #region 委托
        private void onTimeclearAllData(bool setNGText)
        {
            countClearanceSetting(setNGText);
        }

        private void updateDV(int index)
        {
            if (index == 3)
            {
                // 3.清空datagridView
                if (dataGridView3.Rows.Count > 0)
                {
                    dataGridView3.Rows.Clear();
                }
                // 4. 添加新模型节点
                addRows(configPath, treeFileHelper1);
            }
            else if (index == 4)
            {
                // 3.清空datagridView
                if (dataGridView4.Rows.Count > 0)
                {
                    dataGridView4.Rows.Clear();
                }
                // 4. 添加新模型节点
                addRows(configPath2, treeFileHelper2);
            }
        }

        // 委托显示相机日志
        private void showLog(TextBox textBox, string log)
        {
            textBox.AppendText(DateTime.Now.ToString("yyyy/MM/dd-HH:mm:ss:fff") + ":" + log + "\r\n");
            LogHelper.WriteLog(log);
            textBox.SelectionStart = tbLog1.TextLength;
            textBox.ScrollToCaret();
            textBox.SelectionStart = tbLog2.TextLength;
            textBox.ScrollToCaret();
        }

       // 委托显示通讯状态
        private void showPlcStatus(Label label, string status, System.Drawing.Color color)
        {
            label.BackColor = color;
            label.Text = status;
        }
        #endregion

        #region 采图设置
        private void cbGrayOrRgb1_SelectedIndexChanged(object sender, EventArgs e)
        {
            IniHelper.SaveSetIni.Write("相机设置", "采图设置", cbGrayOrRgb1.Text);
        }

        #endregion

        #region 步骤枚举

        enum STEP1
        {
            IMAGE_SIGNAL = 1,
            GRAB_IMAGE,
            IMAGE_PROC,
            OUTPUT_RESULT,
            SAVE_IMAGE
        }
        enum STEP2
        {
            IMAGE_SIGNAL = 1,
            GRAB_IMAGE,
            IMAGE_PROC,
            OUTPUT_RESULT,
            SAVE_IMAGE
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
            dataGridView3.Rows.Clear();
            dataGridView4.Rows.Clear();
            if (!String.IsNullOrEmpty(configPath) && dataGridView3.Rows.Count < 2 && dataGridView4.Rows.Count < 2)
            {
                addRows(configPath, treeFileHelper1);
                if (!string.IsNullOrEmpty(configPath2))
                {
                    addRows(configPath2, treeFileHelper2);
                } else
                {
                    addRows(configPath, treeFileHelper2);
                }
                
            }
        }

        #region 相机1缩放
        bool m_bZoom1 = false;
        ZoomImage zi1 = null;
        private void 缩放ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ho_image1 != null && ho_image1.CountObj() != 0)
            {
                m_bZoom1 = true;
            }
        }
        #endregion

        #region 相机1还原
        private void 还原ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ho_image1 != null && ho_image1.CountObj() != 0)
            {
                m_bZoom1 = true;
                hWindowControl1.HalconWindow.ClearWindow();
                HOperatorSet.GetImageSize(ho_image1, out hv_Width1, out hv_Height1);
                HalconHelper.imageLocation(hv_Width1, hv_Height1, hWindowControl1.Width, hWindowControl1.Height, out row1, out column1, out row2, out column2);
                HOperatorSet.SetPart(hWindowControl1.HalconWindow, row1, column1, row2, column2);
                HOperatorSet.DispObj(ho_image1, hWindowControl1.HalconWindow);
                zi1 = new ZoomImage(row1, column1, row2, column2, hv_Width1, hv_Height1, this.hWindowControl1);
            }
        }
        #endregion

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
                    hv_Width1.Dispose(); hv_Height1.Dispose();
                    HOperatorSet.GetImageSize(ho_image1, out hv_Width1, out hv_Height1);
                    HalconHelper.imageLocation(hv_Width1, hv_Height1, winWidth, winHeight, out row1, out column1, out row2, out column2);
                    HOperatorSet.SetPart(this.hWindowControl1.HalconWindow, row1, column1, row2, column2);
                    HOperatorSet.DispObj(ho_image1, this.hWindowControl1.HalconWindow);
                    zi1 = new ZoomImage(row1, column1, row2, column2, hv_Width1, hv_Height1, this.hWindowControl1);
                }
            }
            catch (Exception ex)
            {
                ho_image1.Dispose();
                hv_Width1.Dispose();
                hv_Height1.Dispose();
                MessageBox.Show(ex.ToString());
                LogHelper.WriteLog(ex.ToString());
            }
        }
        #endregion

        #region 串口1通讯
        private void btOpenPlc_Click(object sender, EventArgs e)
        {
            if (!panasonicMewtocol1.IsOpen())
                loadOnePort();
        }
        private void btClosePlc_Click(object sender, EventArgs e)
        {
            panasonicMewtocol1.Write(tbReadySignal1.Text, false);
            panasonicMewtocol1?.Close();
            controlCom1(true);
            controlSignal1(true);
            showLog(tbLog1, "串口1断开通讯...");
            btOpenPlc1.Enabled = true;
            btClosePlc1.Enabled = false;
            btAddNGSet1.Enabled = true;
            contextMenuStrip1.Enabled = false;
        }

        private bool loadCom1()
        {
            if (!Directory.Exists(IniHelper.SaveSetIni.Read("图片设置", "保存路径")) ||
                !File.Exists(IniHelper.SaveSetIni.Read("深度学习文件", "配置文件路径")))
            {
                MessageBox.Show("请先设置好文件路径");
                return false;
            }
            if (String.IsNullOrEmpty(tbOKSignal1.Text) || String.IsNullOrEmpty(tbReadySignal1.Text) || String.IsNullOrEmpty(tbGrabImageSignal1.Text))
            {
                MessageBox.Show("请先输入PLC地址");
                return false;
            }
            if (ngDic1.Count <= 0)
            {
                MessageBox.Show("请至少输入一个NG信号地址");
                return false;
            }
            OperateResult result1 = panasonicMewtocol1.Write(tbReadySignal1.Text, true);
            if (!result1.IsSuccess)
            {
                showLog(tbLog1, "准备信号发送失败，请检查串口");
                return false;
            }
            return true;
        }
        private void startCom1()
        {
            showLog(tbLog1, "相机1已就绪...");
            IniHelper.SaveSetIni.Write("相机1PLC设置", "拍照信号", tbGrabImageSignal1.Text);
            IniHelper.SaveSetIni.Write("相机1PLC设置", "OK信号", tbOKSignal1.Text);
            IniHelper.SaveSetIni.Write("相机1PLC设置", "准备信号", tbReadySignal1.Text);
            if (cbEnableCode.Checked)
            {
                IniHelper.SaveSetIni.Write("双相机读码设置", "读码地址", tbCodeAddress.Text);
                IniHelper.SaveSetIni.Write("双相机读码设置", "读码长度", tbCodeLength.Text);
            }
            cbGrayOrRgb1.Enabled = false;
            btClosePlc1.Enabled = false;
            cbEnableCode.Enabled = false;
            controlSignal1(false);
        }
        private void loadOnePort()
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
                    //IniHelper.SaveSetIni.Write("串口1设置", "串口号", cbPortName1.Text);
                    //IniHelper.SaveSetIni.Write("串口1设置", "波特率", tbBaudRate1.Text);
                    //IniHelper.SaveSetIni.Write("串口1设置", "数据位", tbDateBits1.Text);
                    //IniHelper.SaveSetIni.Write("串口1设置", "停止位", tbStopBits1.Text);
                    //IniHelper.SaveSetIni.Write("串口1设置", "校验位", cbParity1.Text);
                    tbLog1.BeginInvoke(GetMyDelegateLog, tbLog1, "串口1打开成功...");
                    controlCom1(false);
                    btOpenPlc1.Enabled = false;
                    btClosePlc1.Enabled = true;
                }
                else
                {
                    btClosePlc1.Enabled = false;
                    btOpenPlc1.Enabled = true;
                    controlCom1(true);
                    tbLog1.BeginInvoke(GetMyDelegateLog, tbLog1, $"{cbPortName1.Text}拒绝访问,请换个COM口连接...");
                    MessageBox.Show($"{cbPortName1.Text}拒绝访问,请换个COM口连接...");
                }
            }
            catch (Exception ex)
            {
                btClosePlc1.Enabled = false;
                btOpenPlc1.Enabled = true;
                tbLog1.BeginInvoke(GetMyDelegateLog, tbLog1, ex.Message);
                MessageBox.Show(ex.Message);
            }
        }

        #endregion

        #region 相机1鼠标各类事件
        bool m_bMouseLeft1 = false;
        private void hWindowControl1_HMouseDown(object sender, HMouseEventArgs e)
        {
            if (m_bZoom1)
            {
                if (e.Button == MouseButtons.Left)
                {
                    hWindowControl1.Cursor = Cursors.Hand;
                    zi1.StartX = e.X;
                    zi1.StartY = e.Y;
                    m_bMouseLeft1 = true;
                }
            }
        }

        private void hWindowControl1_HMouseMove(object sender, HMouseEventArgs e)
        {
            try
            {
                HTuple graval = new HTuple(); 
                xy = $"像素坐标:{(int)e.Y},{(int)e.X}";
                if (ho_image1 != null)
                {
                    if (e.Y<0||e.X<0||e.Y>hv_Height1-1||e.X>hv_Width1-1)
                        gray= "像素灰度:-";
                    else
                    {
                        graval.Dispose();
                        HOperatorSet.GetGrayval(ho_image1, e.Y, e.X, out graval);
                        if (graval.Length == 1)
                            gray = $"像素灰度:{graval.I.ToString()}";
                        else if (graval.Length == 3)
                            gray = $"像素灰度:{graval[0].I.ToString()},{graval[1].I.ToString()},{graval[2].I.ToString()}";
                        graval.Dispose();
                    }
                }
                showXYorGrayEvent?.Invoke(xy,gray) ;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            if (m_bZoom1)
            {
                if (m_bMouseLeft1)
                {
                    double offsetX = e.X - zi1.StartX;
                    double offsetY = e.Y - zi1.StartY;
                    zi1.moveImage(offsetX, offsetY, ho_image1);
                }
            }
        }

        private void hWindowControl1_HMouseUp(object sender, HMouseEventArgs e)
        {
            if (m_bZoom1)
            {
                this.hWindowControl1.Cursor = Cursors.Arrow;
                m_bMouseLeft1 = false;
            }
        }

        private void hWindowControl1_HMouseWheel(object sender, HMouseEventArgs e)
        {
            if (m_bZoom1)
            {
                double scale;
                if (e.Delta > 0)
                {
                    scale = 0.9;
                }
                else
                {
                    scale = 1 / 0.9;
                }
                zi1.zoomImage(e.X, e.Y, scale, ho_image1);
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

        #region 相机2还原
        private void 还原ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (ho_image2 != null && ho_image2.CountObj() != 0)
            {
                m_bZoom2 = true;
                hWindowControl2.HalconWindow.ClearWindow();
                HOperatorSet.GetImageSize(ho_image1, out hv_Width1, out hv_Height1);
                HalconHelper.imageLocation(hv_Width2, hv_Height2, hWindowControl2.Width, hWindowControl2.Height, out row3, out column3, out row4, out column4);
                HOperatorSet.SetPart(hWindowControl2.HalconWindow, row3, column3, row4, column4);
                HOperatorSet.DispObj(ho_image2, hWindowControl2.HalconWindow);
                zi2 = new ZoomImage(row3, column3, row4, column4, hv_Width2, hv_Height2, this.hWindowControl2);
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
                    HOperatorSet.SetPart(this.hWindowControl2.HalconWindow, row3, column3, row4, column4);
                    HOperatorSet.DispObj(ho_image2, this.hWindowControl2.HalconWindow);
                    zi2 = new ZoomImage(row3, column3, row4, column4, hv_Width2, hv_Height2, this.hWindowControl2);
                }
            }
            catch (Exception ex)
            {
                ho_image2.Dispose();
                hv_Width2.Dispose();
                hv_Height2.Dispose();
                MessageBox.Show(ex.ToString());
                LogHelper.WriteLog(ex.ToString());
            }
        }
        #endregion


        private bool loadCom2()
        {
            if (String.IsNullOrEmpty(tbOKSignal2.Text) || String.IsNullOrEmpty(tbReadySignal2.Text) || String.IsNullOrEmpty(tbGrabImageSignal2.Text))
            {
                MessageBox.Show("请先输入相机2PLC地址");
                return false;
            }
            else if (ngDic2.Count <= 0)
            {
                MessageBox.Show("请至少输入一个NG信号地址");
                return false;
            }

            if (!Directory.Exists(IniHelper.SaveSetIni.Read("图片设置", "保存路径")) ||
                !File.Exists(IniHelper.SaveSetIni.Read("深度学习文件", "配置文件路径")))
            {
                MessageBox.Show("请先设置好文件路径");
                return false;
            }
            else if (!m_bTwostart2)
            {
                showLog(tbLog2, "相机2未初始化...");
                MessageBox.Show("相机2未初始化");
                return false;
            }
            return true;
        }

        private void startCom2()
        {
            showLog(tbLog2, "相机2已就绪...");
            showLog(tbLog2, "软件已就绪...");

            IniHelper.SaveSetIni.Write("相机2PLC设置", "准备信号", tbReadySignal2.Text);
            IniHelper.SaveSetIni.Write("相机2PLC设置", "OK信号", tbOKSignal2.Text);
            IniHelper.SaveSetIni.Write("相机2PLC设置", "拍照信号", tbGrabImageSignal2.Text);

            btAddNGSet2.Enabled = false;
            contextMenuStrip2.Enabled = false;
        }


        #region 相机2鼠标各类事件
        bool m_bMouseLeft2 = false;
        private void hWindowControl2_HMouseDown(object sender, HMouseEventArgs e)
        {
            if (m_bZoom2)
            {
                if (e.Button == MouseButtons.Left)
                {
                    hWindowControl2.Cursor = Cursors.Hand;
                    zi2.StartX = e.X;
                    zi2.StartY = e.Y;
                    m_bMouseLeft2 = true;
                }
            }
        }

        private void hWindowControl2_HMouseMove(object sender, HMouseEventArgs e)
        {
            try
            {
                HTuple graval = new HTuple();
                xy = $"像素坐标:{(int)e.Y},{(int)e.X}";
                if (ho_image2 != null)
                {
                    if (e.Y < 0 || e.X < 0 || e.Y > hv_Height2 - 1 || e.X > hv_Width2 - 1)
                        gray = "像素灰度:-";
                    else
                    {
                        graval.Dispose();
                        HOperatorSet.GetGrayval(ho_image2, e.Y, e.X, out graval);
                        if (graval.Length == 1)
                            gray = $"像素灰度:{graval.I.ToString()}";
                        else if (graval.Length == 3)
                            gray = $"像素灰度:{graval[0].I.ToString()},{graval[1].I.ToString()},{graval[2].I.ToString()}";
                        graval.Dispose();
                    }
                }
                showXYorGrayEvent?.Invoke(xy,gray);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            if (m_bZoom2)
            {
                if (m_bMouseLeft2)
                {
                    double offsetX = e.X - zi2.StartX;
                    double offsetY = e.Y - zi2.StartY;
                    zi2.moveImage(offsetX, offsetY, ho_image2);
                }
            }
        }

        private void hWindowControl2_HMouseUp(object sender, HMouseEventArgs e)
        {
            if (m_bZoom2)
            {
                this.hWindowControl2.Cursor = Cursors.Arrow;
                m_bMouseLeft2 = false;
            }
        }

        private void hWindowControl2_HMouseWheel(object sender, HMouseEventArgs e)
        {
            if (m_bZoom2)
            {
                double scale;
                if (e.Delta > 0)
                {
                    scale = 0.9;
                }
                else
                {
                    scale = 1 / 0.9;
                }
                zi2.zoomImage(e.X, e.Y, scale, ho_image2);
            }
        }
        #endregion

        #region 显示日志触发事件
        private void showLogEvent(string log)
        {
            if (status == null)
            {
                showLog(tbLog1, log);
                showLog(tbLog2, log);
            }
            if (status == "Cam2")
                showLog(tbLog2, log);
            else if(status =="Cam1")
                showLog(tbLog1, log);
        }
        #endregion

        #region 自动运行过程中禁止操作软件

        private void runControl(string str)
        {
            showLog(tbLog1, str);
            showLog(tbLog2, str);
            if (str == "继续开始自动运行...")
            {
                controlCameraSet1(false);
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
                controlCameraSet1(true);
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
        }
        private void controlSignal2(bool islogin)
        {
            tbGrabImageSignal2.Enabled= islogin;
            tbReadySignal2.Enabled= islogin;
            tbOKSignal2.Enabled = islogin;
            btAddNGSet2.Enabled = islogin;
        }

        private void controlCodeSet(bool islogin)
        {
            tbCodeAddress.Enabled = islogin;
            tbCodeLength.Enabled = islogin;
            tbCodeResult.Enabled = islogin;
        }

        private void controlCameraSet1(bool islogin)
        {
            cbGrayOrRgb1.Enabled= islogin;
        }

        #endregion

        #region 相机1NG信号设置
        string status = null;
        private void button1_Click(object sender, EventArgs e)
        {
            status = "Cam1";
            frmNGSignalSet signalSet = new frmNGSignalSet("NG2",configPath);
            signalSet.Text = "相机1NG信号设置";
            signalSet.ShowDialog();
        }
        #endregion

        #region 相机2NG信号设置
        private void btAddNGSet2_Click(object sender, EventArgs e)
        {
            status = "Cam2";
            if (string.IsNullOrEmpty(configPath2))
            {
                frmNGSignalSet signalSet = new frmNGSignalSet("NG3",configPath);
                signalSet.Text = "相机2NG信号设置";
                signalSet.ShowDialog();
            } else
            {
                frmNGSignalSet signalSet = new frmNGSignalSet("NG3", configPath2);
                signalSet.Text = "相机2NG信号设置";
                signalSet.ShowDialog();
            }
            
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

        #region 更新显示检测结果
        private void showRes1()
        {
            this.BeginInvoke(new Action(() =>
            {
                lbAllNum1.Text = AllNum1.ToString();
                lbOKNum1.Text = OKNum1.ToString();
                lbNGNum1.Text = NGNum1.ToString();
                lbOKPercent1.Text = (Convert.ToInt32(OKNum1) / Convert.ToInt32(AllNum1) * 100).ToString("F2") + "%";
                lbCt1.Text = ct1.ToString();
                if (result1 == "OK")
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

        private void showRes2()
        {
            this.BeginInvoke(new Action(() =>
            {
                lbAllNum2.Text = AllNum2.ToString();
                lbOKNum2.Text = OKNum2.ToString();
                lbNGNum2.Text = NGNum2.ToString();
                lbOKPercent2.Text = (Convert.ToInt32(OKNum2) / Convert.ToInt32(AllNum2) * 100).ToString("F2") + "%";
                lbCt2.Text = ct2.ToString();
                if (result2 == "OK")
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
            isImage = true;
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

        
        private async void btLoadConfig_Click(object sender, EventArgs e)
        {
            string configpath1 = IniHelper.SaveSetIni.Read("深度学习文件", "配置文件路径");
            nodeInfos1 = treeFileHelper1.LoadTreeFile(configpath1);
            if(nodeInfos1 == null)
            {
                tbLog1.BeginInvoke(GetMyDelegateLog, tbLog1, "深度学习配置文件路径不存在...");
            }

            string configpath2 = IniHelper.SaveSetIni.Read("深度学习文件", "配置文件路径1");
            if (!String.IsNullOrEmpty(configpath2))
            {
                nodeInfos2 = treeFileHelper2.LoadTreeFile(configpath2);
                tbLog2.BeginInvoke(GetMyDelegateLog, tbLog2, "已加载深度学习配置文件路径2");
                hasTwoModel = true;
            } else
            {
                nodeInfos2 = nodeInfos1;
                tbLog2.BeginInvoke(GetMyDelegateLog, tbLog2, "深度学习配置文件路径2不存在当前使用模型1");
                hasTwoModel = false;
            }
            btLoadConfig.Enabled = false;
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken token = cancellationTokenSource.Token;
            await Task.Run(() =>
            {
                progressBar1.BeginInvoke(new Action(() => {
                    if (dataGridView3.Rows.Count < 2 && dataGridView4.Rows.Count < 2)
                    {
                        addRows(configpath1,treeFileHelper1);
                        if (hasTwoModel)
                        {
                            addRows(configpath2, treeFileHelper2);
                        }
                        else
                        {
                            addRows(configpath1, treeFileHelper2);
                        }
                        
                    }
                }));
                for (int i = 0; i < 100; i++)
                {
                    progressBar1.BeginInvoke(new Action(() => { progressBar1.Value = i; }));
                    if (i == 40)
                    {
                        if (hasTwoModel)
                        {
                            if (!(HTCSharpDemo.Program.loadDeepStudyHandle(configpath1, ref studyHandle, ref test_num,treeFileHelper1) && (HTCSharpDemo.Program.loadDeepStudyHandle(configpath2, ref studyHandle2, ref test_num2,treeFileHelper2))))
                            {
                                cancellationTokenSource.Cancel();
                                progressBar1.BeginInvoke(new Action(() =>
                                {
                                    progressBar1.Value = 0;
                                    btLoadConfig.Enabled = true;
                                    btLoadConfig.Text = "加载方案";
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
                                    btLoadConfig.Enabled = false;
                                    btLoadConfig.Text = "加载完毕";
                                    startCom1();
                                    loadCom1();
                                    startCom2();
                                    btClearDate.Enabled = false;
                                }));
                            }
                        }
                        //单模型
                        else
                        {
                            if (!HTCSharpDemo.Program.loadDeepStudyHandle(configpath1, ref studyHandle, ref test_num,treeFileHelper1))
                            {
                                cancellationTokenSource.Cancel();
                                progressBar1.BeginInvoke(new Action(() =>
                                {
                                    progressBar1.Value = 0;
                                    btLoadConfig.Enabled = true;
                                    btLoadConfig.Text = "加载方案";
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
                                    configpath2 = configpath1;
                                    test_num2 = test_num;
                                    treeFileHelper2 = treeFileHelper1;
                                    btLoadConfig.Enabled = false;
                                    btLoadConfig.Text = "加载完毕";
                                    startCom1();
                                    startCom2();
                                    btClearDate.Enabled = false;
                                }));
                            }
                        }                        
                    }
                }
            }, token);
        }

        #endregion

        #region 加载深度学习配置文件到表格
        private void addRows(string configPath,TreeFileHelper treeFileHelper)
        {
            TreeFileHelper.NodeInfos node = new TreeFileHelper.NodeInfos();
            if (!File.Exists(configPath)) //不存在
            {
                return;
            } else
            {
                node.NodeInfo = treeFileHelper.LoadTreeFile(configPath).NodeInfo;
            }
            foreach (var item in node.NodeInfo)
            {
                if (item.NodeType == 1)
                {
                    if (item.ParentsNodeId == -1)
                    {
                        if(treeFileHelper == treeFileHelper1)
                        {
                            dataGridView3.Rows.Add(item.NodeName, "定位失败", "0", "--", "0", "0", "--", "--");
                        }
                        else if(treeFileHelper == treeFileHelper2)
                        {
                            dataGridView4.Rows.Add(item.NodeName, "定位失败", "0", "--", "0", "0", "--", "--");
                        }
                        
                    }
                        
                }
                else
                {
                    foreach (var item1 in item.ClassNames)
                    {
                        if (!item1.Equals("OK", StringComparison.OrdinalIgnoreCase) && !treeFileHelper.noAddItem.Contains(item1))
                        {
                            if (treeFileHelper == treeFileHelper1)
                            {
                                dataGridView3.Rows.Add(item.NodeName, item1, "0", "--", "0", "0", "--", "--");
                            }
                            else if (treeFileHelper == treeFileHelper2)
                            {
                                dataGridView4.Rows.Add(item.NodeName, item1, "0", "--", "0", "0", "--", "--");
                            } 
                        }
                    }
                }
            }
        }
        #endregion

        #region 添加表头
        private void createHead1()
        {
            dataGridView3.RowHeadersVisible = false;
            dataGridView3.Enabled = false;
            DataGridViewColumnCollection columns = dataGridView3.Columns;
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
            dataGridView4.RowHeadersVisible = false;
            dataGridView4.Enabled = false;
            DataGridViewColumnCollection columns = dataGridView4.Columns;
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
                OKType1.Clear();
                NGType1.Clear();
                NGTypePara.NGType ngTypes = new NGTypePara.NGType();
                string jsonFromFile = File.ReadAllText(rootDirectory1);
                ngTypes = JsonConvert.DeserializeObject<NGType>(jsonFromFile);
                if (ngTypes != null && ngDic1.Count != 0)
                {
                    foreach (var item in ngTypes.NGTypeConfigs)
                    {
                        if (item.isOK)
                        {
                            if (!OKType1.Contains(item.NGType))
                                OKType1.Add(item.NGType);
                        }
                        else
                        {
                            if (!NGType1.ContainsKey(item.NGType))
                                NGType1.Add(item.NGType, ngDic1[item.OutType]);
                        }
                    }
                }
            }
            catch (Exception EX)
            {
                LogHelper.WriteLog(EX.ToString());
                MessageBox.Show(EX.ToString());
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
                OKType2.Clear();
                NGType2.Clear();
                NGTypePara.NGType ngTypes = new NGTypePara.NGType();
                string jsonFromFile = File.ReadAllText(rootDirectory2);
                ngTypes = JsonConvert.DeserializeObject<NGType>(jsonFromFile);
                if (ngTypes != null && ngDic2.Count != 0)
                {
                    foreach (var item in ngTypes.NGTypeConfigs)
                    {
                        if (item.isOK)
                        {
                            if (!OKType2.Contains(item.NGType))
                                OKType2.Add(item.NGType);
                        }
                        else
                        {
                            if (!NGType2.ContainsKey(item.NGType))
                                NGType2.Add(item.NGType, ngDic2[item.OutType]);
                        }
                    }
                }
            }
            catch (Exception EX)
            {
                LogHelper.WriteLog(EX.ToString());
                MessageBox.Show(EX.ToString());
            }
        }
        #endregion

        #region 将ng输出类型对应的plc地址添加进字典

        private void addNGDic1()
        {
            if (ngDic1 != null)
                ngDic1.Clear();
            ngDic1 = set1.addDic();
        }

        private void addNGDic2()
        {
            if (ngDic2 != null)
                ngDic2.Clear();
            ngDic2 = set2.addDic();
        }
        #endregion

        #region 判定OK还是NG
        string dingweiSignal1;
        string ngSignal1;
        private async void ProcResult1(List<string> NGItems)
        {
            await Task.Run(() =>
            {
                string configPath = IniHelper.SaveSetIni.Read("深度学习文件", "配置文件路径");

                switch (result1)
                {
                    case "OK":
                        {
                            do
                            {
                                panasonicMewtocol1.Write(tbOKSignal1.Text, true);
                                Thread.Sleep(30);
                            } while (!panasonicMewtocol1.ReadBool(tbOKSignal1.Text).Content);
                            tbLog1.BeginInvoke(GetMyDelegateLog, tbLog1,$"{tbOKSignal1.Text}信号次发送成功");
                            Thread.Sleep(200);
                            do
                            {
                                panasonicMewtocol1.Write(tbOKSignal1.Text, false);
                                Thread.Sleep(30);
                            } while (panasonicMewtocol1.ReadBool(tbOKSignal1.Text).Content);
                            tbLog1.BeginInvoke(GetMyDelegateLog, tbLog1,$"{tbOKSignal1.Text}信号次断开成功");
                            break;
                        }
                    case "NG":
                        {
                            bool status = false;
                            foreach (var item in NGItems)
                            {
                                foreach (var item1 in NGType1)
                                {
                                    if (item == item1.Key && !status)
                                    {
                                        status = true;
                                        ngSignal1 = NGType1[item];
                                        do
                                        {
                                            panasonicMewtocol1.Write(ngSignal1, true);
                                            Thread.Sleep(30);
                                        } while (!panasonicMewtocol1.ReadBool(ngSignal1).Content);
                                        tbLog1.BeginInvoke(GetMyDelegateLog, tbLog1, $"{ngSignal1}信号次发送成功");
                                        Thread.Sleep(200);
                                        do
                                        {
                                            panasonicMewtocol1.Write(ngSignal1, false);
                                            Thread.Sleep(30);
                                        } while (panasonicMewtocol1.ReadBool(ngSignal1).Content);
                                        tbLog1.BeginInvoke(GetMyDelegateLog, tbLog1, $"{ngSignal1}信号次断开成功");
                                    }
                                }
                            }
                            break;
                        }
                    case "定位失败":
                        {
                            //TreeFileHelper.NodeInfos nodeInfo = new TreeFileHelper.NodeInfos();
                            //nodeInfo.NodeInfo = TreeFileHelper.LoadTreeFile(configPath).NodeInfo;
                            foreach (var item in nodeInfos1.NodeInfo)
                            {
                                if (item.ParentsNodeId == -1)
                                {
                                    NGTypePara.NGType ngTypes = new NGTypePara.NGType();
                                    string jsonFromFile = File.ReadAllText(rootDirectory1);
                                    ngTypes = JsonConvert.DeserializeObject<NGType>(jsonFromFile);
                                    foreach (var item1 in ngTypes.NGTypeConfigs)
                                    {
                                        if (item.ClassNames[0] == item1.NGType)
                                            dingweiSignal1 = ngDic1[item1.OutType];
                                    }
                                }
                            }
                            do
                            {
                                panasonicMewtocol1.Write(dingweiSignal1, true);
                                Thread.Sleep(30);
                            } while (!panasonicMewtocol1.ReadBool(dingweiSignal1).Content);
                            tbLog1.BeginInvoke(GetMyDelegateLog, tbLog1, $"{dingweiSignal1}信号次发送成功");
                            Thread.Sleep(200);
                            do
                            {
                                panasonicMewtocol1.Write(dingweiSignal1, false);
                                Thread.Sleep(30);
                            } while (panasonicMewtocol1.ReadBool(dingweiSignal1).Content);
                            tbLog1.BeginInvoke(GetMyDelegateLog, tbLog1, $"{dingweiSignal1}信号次断开成功");
                            break;
                        }
                    default:
                        break;
                }
            });
        }
        string dingweiSignal2;
        string ngSignal2;
        private async void ProcResult2(List<string> NGItems)
        {
            await Task.Run(() =>
            {
                string configPath = IniHelper.SaveSetIni.Read("深度学习文件", "配置文件路径");
                switch (result2)
                    {
                        case "OK":
                            {
                                string cam2OKSignalAddress;
                                if (string.IsNullOrEmpty(tbOKSignal2.Text))
                                {
                                    cam2OKSignalAddress = tbOKSignal1.Text;
                                } else
                                {
                                    cam2OKSignalAddress = tbOKSignal2.Text;
                                }
                                do
                                {
                                    panasonicMewtocol1.Write(cam2OKSignalAddress, true);
                                    Thread.Sleep(30);
                                } while (!panasonicMewtocol1.ReadBool(cam2OKSignalAddress).Content);
                                tbLog1.BeginInvoke(GetMyDelegateLog, tbLog2, $"{cam2OKSignalAddress}信号次发送成功");
                                Thread.Sleep(200);
                                do
                                {
                                    panasonicMewtocol1.Write(cam2OKSignalAddress, false);
                                    Thread.Sleep(30);
                                } while (panasonicMewtocol1.ReadBool(cam2OKSignalAddress).Content);
                                tbLog1.BeginInvoke(GetMyDelegateLog, tbLog2, $"{cam2OKSignalAddress}信号次断开成功");
                                break;
                            }
                        case "NG":
                            {
                                bool status = false;
                                foreach (var item in NGItems)
                                {
                                    foreach (var item1 in NGType2)
                                    {
                                        if (item == item1.Key && !status)
                                        {
                                            status = true;
                                            ngSignal2 = NGType2[item];
                                            do
                                            {
                                                panasonicMewtocol1.Write(ngSignal2, true);
                                                Thread.Sleep(30);
                                            } while (!panasonicMewtocol1.ReadBool(ngSignal2).Content);
                                            tbLog1.BeginInvoke(GetMyDelegateLog, tbLog2, $"{ngSignal2}信号次发送成功");
                                            Thread.Sleep(200);
                                            do
                                            {
                                                panasonicMewtocol1.Write(ngSignal2, false);
                                                Thread.Sleep(30);
                                            } while (panasonicMewtocol1.ReadBool(ngSignal2).Content);
                                            tbLog1.BeginInvoke(GetMyDelegateLog, tbLog2, $"{ngSignal2}信号次断开成功");
                                        }
                                    }
                                }
                                break;
                            }
                        case "定位失败":
                            {
                                //TreeFileHelper.NodeInfos nodeInfo = new TreeFileHelper.NodeInfos();
                                //nodeInfo.NodeInfo = TreeFileHelper.LoadTreeFile(configPath).NodeInfo;
                                foreach (var item in nodeInfos2.NodeInfo)
                                {
                                    if (item.ParentsNodeId == -1)
                                    {
                                        NGTypePara.NGType ngTypes = new NGTypePara.NGType();
                                        string jsonFromFile = File.ReadAllText(rootDirectory2);
                                        ngTypes = JsonConvert.DeserializeObject<NGType>(jsonFromFile);
                                        foreach (var item1 in ngTypes.NGTypeConfigs)
                                        {
                                            if (item.ClassNames[0] == item1.NGType)
                                                dingweiSignal2 = ngDic2[item1.OutType];
                                        }
                                    }
                                }
                                do
                                {
                                    panasonicMewtocol1.Write(dingweiSignal2, true);
                                    Thread.Sleep(30);
                                } while (!panasonicMewtocol1.ReadBool(dingweiSignal2).Content);
                                tbLog1.BeginInvoke(GetMyDelegateLog, tbLog2, $"{dingweiSignal2}信号次发送成功");
                                Thread.Sleep(200);
                                do
                                {
                                    panasonicMewtocol1.Write(dingweiSignal2, false);
                                    Thread.Sleep(30);
                                } while (panasonicMewtocol1.ReadBool(dingweiSignal2).Content);
                                tbLog1.BeginInvoke(GetMyDelegateLog, tbLog2, $"{dingweiSignal2}信号次断开成功");
                                break;
                            }
                        default:
                            break;
                    }
            });
        }
        #endregion

        #region 渲染图片
        private async void showNGLocation1(Dictionary<int, List<result>> res1)
        {
            await Task.Run(() =>
            {
                if (result1 == "OK")
                {
                    HTCSharpDemo.Program.result res2 = res1[treeFileHelper1.dingweiNodeId][0];
                    HalconHelper.showRoi(hWindowControl1.HalconWindow, "green", res2.y, res2.x, res2.y + res2.height, res2.x + res2.width);
                    HalconHelper.showResultString(hWindowControl1.HalconWindow, 0, 0, 72, "green", "OK");


                }
                else
                {
                    if (res1[treeFileHelper1.dingweiNodeId].Count == 1)
                    {
                        HTCSharpDemo.Program.result res2 = res1[treeFileHelper1.dingweiNodeId][0];
                        HalconHelper.showRoi(hWindowControl1.HalconWindow, "red", res2.y, res2.x, res2.y + res2.height, res2.x + res2.width);
                    }
                    HalconHelper.showResultString(hWindowControl1.HalconWindow, 0, 0, 72, "red", result1);


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
            });
        }
        private async void showNGLocation2(Dictionary<int, List<result>> res3)
        {
            await Task.Run(() =>
            {
                if (result2 == "OK")
                {
                    HTCSharpDemo.Program.result res2 = res3[treeFileHelper2.dingweiNodeId][0];
                    HalconHelper.showRoi(hWindowControl2.HalconWindow, "green", res2.y, res2.x, res2.y + res2.height, res2.x + res2.width);
                    HalconHelper.showResultString(hWindowControl2.HalconWindow, 0, 0, 72, "green", "OK");


                }
                else
                {
                    if (res3[treeFileHelper2.dingweiNodeId].Count == 1)
                    {
                        HTCSharpDemo.Program.result res2 = res3[treeFileHelper2.dingweiNodeId][0];
                        HalconHelper.showRoi(hWindowControl2.HalconWindow, "red", res2.y, res2.x, res2.y + res2.height, res2.x + res2.width);
                    }
                    HalconHelper.showResultString(hWindowControl2.HalconWindow, 0, 0, 72, "red", result2);
                }

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
            });
        }
        #endregion

        #region 显示结果
        private void showResult1()
        {
            this.BeginInvoke(new Action(() =>
            {
                lbAllNum1.Text = AllNum1.ToString();
                lbOKNum1.Text = OKNum1.ToString();
                lbNGNum1.Text = NGNum1.ToString();
                lbOKPercent1.Text = (OKNum1 / AllNum1 * 100).ToString("F2") + "%";
                lbCt1.Text = ct1.ToString();
                if (result1 == "OK")
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
        private void showResult2()
        {
            this.BeginInvoke(new Action(() =>
            {
                lbAllNum2.Text = AllNum2.ToString();
                lbOKNum2.Text = OKNum2.ToString();
                lbNGNum2.Text = NGNum2.ToString();
                lbOKPercent2.Text = (OKNum2 / AllNum2 * 100).ToString("F2") + "%";
                lbCt2.Text = ct2.ToString();
                if (result2 == "OK")
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

        #region 更新结果列表
        private void referenceTable1(Dictionary<int, List<result>> res1)
        {
            var originalCellStyle = dataGridView3.Rows[0].Cells[0].Style;
            foreach (DataGridViewRow row in dataGridView3.Rows)
                row.Cells[3].Style = originalCellStyle;
            try
            {
                foreach (DataGridViewRow row in this.dataGridView3.Rows)
                {
                    if (dataGridView3.Rows.Count - 1 > row.Index)
                    {
                        double ngdingweiNum = 0;
                        bool isAdd = false;
                        if (row.Cells[0].Value != null && !String.IsNullOrWhiteSpace(row.Cells[0].Value.ToString()) && row.Cells[0].Value != DBNull.Value)
                        {
                            if ((string)row.Cells[1].Value == "定位失败")
                            {
                                isAdd= true;
                                if (row.Cells[2].Value != null && !String.IsNullOrWhiteSpace(row.Cells[2].Value.ToString()) && row.Cells[2].Value != DBNull.Value && Regex.IsMatch(row.Cells[2].Value.ToString(), @"^\d+(\.\d+)?$"))
                                    ngdingweiNum = Convert.ToInt32(row.Cells[2].Value);
                                if (res1[treeFileHelper1.dingweiNodeId].Count == 1)
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
                                        if (item1.class_id == (string)row.Cells[1].Value&&!isAdd1)
                                        {
                                            isAdd = true;
                                            isAdd1= true;
                                            if (row.Cells[2].Value != null && !String.IsNullOrWhiteSpace(row.Cells[2].Value.ToString()) && row.Cells[2].Value != DBNull.Value && Regex.IsMatch(row.Cells[2].Value.ToString(), @"^\d+(\.\d+)?$"))
                                                ngNum = Convert.ToDouble(row.Cells[2].Value.ToString());
                                            //结果出现次数
                                            if (result1 == "OK")
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
                        row.Cells[5].Value = (Convert.ToDouble(row.Cells[2].Value) / AllNum1 * 100).ToString("F2") + "%"; //百分比
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.ToString());
            }
        }
        private  void referenceTable2(Dictionary<int, List<result>> res2)
        {
            var originalCellStyle = dataGridView4.Rows[0].Cells[0].Style;
            foreach (DataGridViewRow row in dataGridView4.Rows)
                row.Cells[3].Style = originalCellStyle;
            try
            {
                foreach (DataGridViewRow row in this.dataGridView4.Rows)
                {
                    if (dataGridView4.Rows.Count - 1 > row.Index)
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
                                if (res2[treeFileHelper2.dingweiNodeId].Count == 1)
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
                                        if (item1.class_id == (string)row.Cells[1].Value&&!isAdd1)
                                        {
                                            isAdd = true;
                                            isAdd1 = true;
                                            if (row.Cells[2].Value != null && !String.IsNullOrWhiteSpace(row.Cells[2].Value.ToString()) && row.Cells[2].Value != DBNull.Value && Regex.IsMatch(row.Cells[2].Value.ToString(), @"^\d+(\.\d+)?$"))
                                                ngNum = Convert.ToDouble(row.Cells[2].Value.ToString());
                                            //结果出现次数
                                            if (result2 == "OK")
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
                        row.Cells[5].Value = (Convert.ToDouble(row.Cells[2].Value) / AllNum2 * 100).ToString("F2") + "%"; //百分比
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.ToString());
            }
        }
        #endregion

        #region 异步保存图片
        private async void saveImage1(string okStatus, string ngStatus, string result, List<string> NGItems)
        {
            if (!b_isDiskFull)
            {
                await Task.Run(() =>
                {
                    string imageName = null;
                    if (cbEnableCode.Checked)
                    {
                        imageName = panasonicMewtocol1.ReadString(tbCodeAddress.Text, ushort.Parse(tbCodeLength.Text)).Content;
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
                        try
                        {
                            HalconHelper.saveImage(hWindowControl1.HalconWindow, ho_image1, imagePath);
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    else if (ngStatus == "T" && result == "NG")
                    {
                        foreach (var item in NGItems)
                        {
                            string path = savePath + "\\" + "相机1" + "\\" + nowDataTime + "\\" + $"{item}" + "\\";
                            if (!Directory.Exists(path))
                                Directory.CreateDirectory(path);
                            string imagePath = HalconHelper.fileNameIsExist(path, imageName);
                            try
                            {
                                HalconHelper.saveImage(hWindowControl1.HalconWindow, ho_image1, imagePath);
                            }
                            catch(Exception ex)
                            {

                            }
                            
                        }
                    }
                });
            }          
        }
        private async void saveImage2(string okStatus, string ngStatus, string result, List<string> NGItems1)
        {
            if (!b_isDiskFull)
            {
                await Task.Run(() =>
            {
                string imageName = null;
                if (cbEnableCode.Checked)
                {
                    imageName = panasonicMewtocol1.ReadString(tbCodeAddress.Text, ushort.Parse(tbCodeLength.Text)).Content;
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
                    try
                    {
                        HalconHelper.saveImage(hWindowControl2.HalconWindow, ho_image2, imagePath);
                    } catch(Exception ex)
                    {

                    }
                    
                }
                else if (ngStatus == "T" && result == "NG")
                {
                    foreach (var item in NGItems1)
                    {
                        string path = savePath + "\\" + "相机2" + "\\" + nowDataTime + "\\" + $"{item}" + "\\";
                        if (!Directory.Exists(path))
                            Directory.CreateDirectory(path);
                        string imagePath = HalconHelper.fileNameIsExist(path, imageName);
                        try
                        {
                            HalconHelper.saveImage(hWindowControl2.HalconWindow, ho_image2, imagePath);
                        }
                        catch(Exception ex)
                        {

                        }                       
                    }
                }
            });
            }
        }
        #endregion

        #region 检查磁盘剩余空间
        private bool isFreeSpace(string imgSavePath)
        {
            DriveInfo drive = new DriveInfo(System.IO.Path.GetPathRoot(imgSavePath));
            if(drive.IsReady)
            {
                long freeSpace = drive.TotalFreeSpace;
                // 剩余10G
                double freeSpaceInMB = freeSpace / (1024 * 1024 * 1024);
                if (freeSpaceInMB < 10)
                    return false;
                else
                    return true;
            }
            return false;
            
        }
        #endregion

        #region 实施设置曝光
        string camStatus;
        private void 实时读取设置曝光ToolStripMenuItem_Click(object sender, EventArgs e)
        {
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
                while (isRunning)
                 {
                    switch (camStatus)
                    {
                        case "相机1":
                            grabOneImage(out ho_image1);
                            oneImageToHalcon(ho_image1);
                            break;
                        case "相机2":
                            grabTwoImage(out ho_image2);
                            twoImageToHalcon(ho_image2);
                            break;
                        default:
                            break;
                    }
                 }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message);
            }
        }
        private void exposeSet(float expose,string camName)
        {
            switch (camName)
            {
                case "相机1":
                    device1.MV_CC_SetEnumValue_NET("ExposureAuto", 0);
                    nRet1 = device1.MV_CC_SetFloatValue_NET("ExposureTime", expose);
                    if (nRet1 == MyCamera.MV_OK)
                    {
                        IniHelper.SaveSetIni.Write("相机1设置", "曝光量", expose.ToString());
                        showLog(tbLog1, $"相机1设置曝光成功:{expose}");
                    }
                    else
                    {
                        showLog(tbLog1, "相机1设置曝光失败...");
                        MessageBox.Show("Set Exposure Time Fail!");
                    }
                    break;
                case "相机2":
                    device2.MV_CC_SetEnumValue_NET("ExposureAuto", 0);
                    nRet2 = device2.MV_CC_SetFloatValue_NET("ExposureTime", expose);
                    if (nRet2 == MyCamera.MV_OK)
                    {
                        IniHelper.SaveSetIni.Write("相机2设置", "曝光量", expose.ToString());
                    }
                    else
                    {
                        showLog(tbLog2, $"相机2设置设置失败...");
                        MessageBox.Show("Set Exposure Time Fail!");
                    }
                    break;
                default:
                    break;
            }
        }
        private void exposeClose()
        {
            grabImage?.Abort();
        }
        private void 实时设置曝光ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!m_bTwostart2)
            {
                MessageBox.Show("相机2未打开");
                return;
            }
            camStatus = "相机2";
            grabImage = new Thread(grabImageThread);
            grabImage.IsBackground = true;
            grabImage.Start();
            frmCameraExposeSet exposeSet = new frmCameraExposeSet("相机2");
            exposeSet.ShowDialog();
        }
        #endregion


        #region 更改深度学习配置文件后重写信号输出配置文件
        private void loadNewFile(string path)
        {
            Task.Run(() =>
            {
                TreeFileHelper.NodeInfos nodeInfo = new TreeFileHelper.NodeInfos();
                nodeInfo.NodeInfo = treeFileHelper1.LoadTreeFile(path).NodeInfo;
                NGTypePara.NGType ngTypes = new NGTypePara.NGType();
                foreach (var item1 in nodeInfo.NodeInfo)
                {
                    if (item1.ParentsNodeId > -1 && item1.NodeType == 1)
                        continue;
                    foreach (var item2 in item1.ClassNames)
                    {
                        if (!treeFileHelper1.noAddItem.Contains(item2))
                        {
                            NGTypePara.NGTypeConfig nGTypePara = new NGTypePara.NGTypeConfig();
                            nGTypePara.Node = item1.NodeName;
                            nGTypePara.NGType = item2;
                            nGTypePara.OutType = "缺陷类别1";
                            nGTypePara.isOK = false;
                            ngTypes.NGTypeConfigs.Add(nGTypePara);
                        }
                    }
                }
                string json = JsonConvert.SerializeObject(ngTypes, Formatting.Indented);
                File.WriteAllText(rootDirectory1, json);

                // 2.加载模型
                for (int i = 0; i < 100; i++)
                {
                    progressBar1.BeginInvoke(new Action(() => { progressBar1.Value = i; }));
                    if (i == 40)
                    {
                        configPath = path;
                        if (!(HTCSharpDemo.Program.loadDeepStudyHandle(configPath, ref studyHandle, ref test_num, treeFileHelper1)))
                        {
                            progressBar1.BeginInvoke(new Action(() =>
                            {
                                progressBar1.Value = 0;
                                btLoadConfig.Enabled = true;
                                btLoadConfig.Text = "加载方案";
                            }));
                            MessageBox.Show("方案加载失败");
                            break;
                        }
                        else
                        {
                            progressBar1.BeginInvoke(new Action(() =>
                            {
                                ptr1 = studyHandle;
                                btLoadConfig.Enabled = false;
                                btLoadConfig.Text = "新模型加载完毕";
                                startCom1();
                                btClearDate.Enabled = false;
                            }));
                        }
                    }
                }
                dataGridView3.BeginInvoke(updateDataViewData, 3);
            });
        }

        /// <summary>
        /// 加载深度学习文件2
        /// </summary>
        /// <param name="path"></param>
        private void loadNewFile2(string path)
        {
            Task.Run(() =>
            {
                TreeFileHelper.NodeInfos nodeInfo = new TreeFileHelper.NodeInfos();
                nodeInfo.NodeInfo = treeFileHelper2.LoadTreeFile(path).NodeInfo;
                NGTypePara.NGType ngTypes = new NGTypePara.NGType();
                foreach (var item1 in nodeInfo.NodeInfo)
                {
                    if (item1.ParentsNodeId > -1 && item1.NodeType == 1)
                        continue;
                    foreach (var item2 in item1.ClassNames)
                    {
                        if (!treeFileHelper2.noAddItem.Contains(item2))
                        {
                            NGTypePara.NGTypeConfig nGTypePara = new NGTypePara.NGTypeConfig();
                            nGTypePara.Node = item1.NodeName;
                            nGTypePara.NGType = item2;
                            nGTypePara.OutType = "缺陷类别1";
                            nGTypePara.isOK = false;
                            ngTypes.NGTypeConfigs.Add(nGTypePara);
                        }
                    }
                }
                string json = JsonConvert.SerializeObject(ngTypes, Formatting.Indented);
                File.WriteAllText(rootDirectory2, json);

                // 2.加载模型
                for (int i = 0; i < 100; i++)
                {
                    progressBar1.BeginInvoke(new Action(() => { progressBar1.Value = i; }));
                    if (i == 40)
                    {
                        configPath2 = path;
                        if (!(HTCSharpDemo.Program.loadDeepStudyHandle(configPath2, ref studyHandle2, ref test_num2, treeFileHelper2)))
                        {
                            progressBar1.BeginInvoke(new Action(() =>
                            {
                                progressBar1.Value = 0;
                                btLoadConfig.Enabled = true;
                                btLoadConfig.Text = "加载方案";
                            }));
                            MessageBox.Show("方案加载失败");
                            break;
                        }
                        else
                        {
                            progressBar1.BeginInvoke(new Action(() =>
                            {
                                ptr2 = studyHandle2;
                                btLoadConfig.Enabled = false;
                                btLoadConfig.Text = "新模型加载完毕";
                                startCom2();
                                btClearDate.Enabled = false;
                            }));
                        }
                    }
                }
                dataGridView4.BeginInvoke(updateDataViewData, 4);
                
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
    }
}
