using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HalconDotNet;
using INI;
using MvCamCtrl.NET;
using HslCommunication.Profinet.Panasonic;
using System.IO.Ports;
using System.Threading;
using LW.ZOOM;
using log4net;
using System.IO;
using HslCommunication;
using System.Xml.Linq;
using System.Configuration;
using System.Windows.Media;
using static log4net.Appender.ColoredConsoleAppender;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using static YUNTIANVision.NGTypePara;
using static YUNTIANVision.TreeFileHelper;
using static HTCSharpDemo.Program;
using System.Globalization;
using System.Diagnostics.Eventing.Reader;
using YUNTIANVision.HTDLModel;
using System.Management.Instrumentation;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Drawing.Imaging;
using System.Reflection;
using System.Windows.Media.Media3D;
using System.Windows;
using MessageBox = System.Windows.Forms.MessageBox;
using static System.Data.Entity.Infrastructure.Design.Executor;
using System.Timers;
using Newtonsoft.Json.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Diagnostics;
using Color = System.Drawing.Color;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace YUNTIANVision
{
    public delegate void showXYorGrayDelegate1(string xy, string gray);
    public partial class frmOneCamera : Form
    {
        public static event showXYorGrayDelegate1 showXYorGrayEvent;
        public frmOneCamera()
        {
            InitializeComponent();
            frmMain.closeAll += new closeDelegate(formClose);
            frmMain.controlEvent += new controlDelegate(runControl);
            configPath = IniHelper.SaveSetIni.Read("深度学习文件", "配置文件路径");
            treeFileHelper = new TreeFileHelper();
            set = new frmNGSignalSet("NG1", treeFileHelper);
            set.loadNGEvent -= new loadNGSignalDic(addNGDic);
            set.loadNGEvent += new loadNGSignalDic(addNGDic);
            set.loadNGEvent -= new loadNGSignalDic(addNGorOK);
            set.loadNGEvent += new loadNGSignalDic(addNGorOK);
            set.updateClassIdisOKStateEvent -= new updateClassIdisOKState(updateSetClassIsOk);
            set.updateClassIdisOKStateEvent += new updateClassIdisOKState(updateSetClassIsOk);
            frmCameraExposeSet.cameraExposeSetEvent -= new cameraExposeSet(exposeSet);
            frmCameraExposeSet.cameraExposeSetEvent += new cameraExposeSet(exposeSet);
            frmCameraExposeSet.cameraExposeCloseEvent -= new cameraExposeClose(exposeClose);
            frmCameraExposeSet.cameraExposeCloseEvent += new cameraExposeClose(exposeClose);
            frmSet.newConfigEvent -= new newConfigDel(loadNewFile);
            frmSet.newConfigEvent += new newConfigDel(loadNewFile);
            frmSet.nowisDebugMode -= new nowisDebugMode(changeDebugMode);
            frmSet.nowisDebugMode += new nowisDebugMode(changeDebugMode);
            hWindowControl1.MouseWheel -= new System.Windows.Forms.MouseEventHandler(this.my_MouseWheel);
            hWindowControl1.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.my_MouseWheel);
            // 在窗口加载时绑定 Resize 事件
            this.Resize += Form_Resize;
        }


        private void changeDebugMode(bool isDebug)
        {
            if (isDebug)
            {
                tbLog.BeginInvoke(GetMyDelegate, "当前切换到调试模式，请设置窗口的图片目录",null);
                IniHelper.SaveSetIni.Write("测试模式", "测试模式", "T");
                trigerDLuseImage = true;
            }
            else
            {
                tbLog.BeginInvoke(GetMyDelegate, "当前切换到运行模式，将使用相机和串口触发运行",null);
                IniHelper.SaveSetIni.Write("测试模式", "测试模式", "F");
                trigerDLuseImage = false;
                trigerDLuseImage_isReady = false;
                window_left_pictruePath_isReady = false;
            }
        }

        #region 定义对象
        /// <summary>
        /// ng信号种类
        /// </summary>
        public static Dictionary<string, string> ngdic = new Dictionary<string, string>();

        /// <summary>
        /// OK类型
        /// </summary>
        List<string> list_OKType_classId_left = new List<string>();
        /// <summary>
        /// NG类型
        /// </summary>
        Dictionary<string, string> NGType = new Dictionary<string, string>();
        string rootDirectory = AppDomain.CurrentDomain.BaseDirectory + "\\" + "NGTypeSignalSet1.txt";
        frmNGSignalSet set;
        /// <summary>
        /// 显示操作日志的委托
        /// </summary>
        /// <param name="log"></param>
        private delegate void myDelegate(string log, Exception ex);
        private myDelegate GetMyDelegate;
        /// <summary>
        /// 实时显示线程
        /// </summary>
        Thread realTimeThread;
        /// <summary>
        /// 分类节点里面的对象
        /// </summary>
        string fenleiType = null;
        /// <summary>
        /// 判定结果
        /// </summary>
        string result;
        /// <summary>
        /// 深度学习节点数量
        /// </summary>
        int test_num;
        /// <summary>
        /// 单相机深度学习句柄
        /// </summary>
        IntPtr studyHandle;
        /// <summary>
        /// 手动触发拍照指令
        /// </summary>
        bool isImage;
        /// <summary>
        /// 相机初始化完成标志
        /// </summary>
        bool m_bOneStart1 = false;
        /// <summary>
        /// 串口对象
        /// </summary>
        PanasonicMewtocol panasonicMewtocol = new PanasonicMewtocol();
        /// <summary>
        /// 相机1拍到的图像变量
        /// </summary>
        HObject ho_image1 = new HObject();
        /// <summary>
        /// 图片宽
        /// </summary>
        HTuple hv_Width1 = new HTuple();
        /// <summary>
        /// 图片高
        /// </summary>
        HTuple hv_Height1 = new HTuple();
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
        /// 产品总数
        /// </summary>
        double AllNum = 0;
        /// <summary>
        /// 产品图像处理时间
        /// </summary>
        int ct;
        /// <summary>
        /// OK总数
        /// </summary>
        double OKNum = 0;
        /// <summary>
        /// NG总数
        /// </summary>
        double NGNum = 0;
        MyCamera device1 = new MyCamera();
        int nRet1 = MyCamera.MV_OK;
        MyCamera.MV_CC_DEVICE_INFO stDevInfo1; // 通用设备信息
        MyCamera.MVCC_INTVALUE stParam1;
        MyCamera.MV_FRAME_OUT_INFO_EX FrameInfo1;
        UInt32 nPayloadSize1;
        IntPtr pBufForDriver1;

        // 白天清零时间
        string day_clearDataTime;
        // 晚上清零时间
        string night_clearDataTime;
        // 是否启用清零
        bool isUseClearDataTime;

        TreeFileHelper treeFileHelper;

        Thread listenSerialPortThread = null;
        private ManualResetEventSlim event1 = new ManualResetEventSlim(false);
        private CountdownEvent countdown = new CountdownEvent(1);

        volatile bool currentIsSetExpose = false;
        /// <summary>
        /// 配置文件路径
        /// </summary>
        string configPath;

        /// <summary>
        /// 读图模式图片路径
        /// </summary>
        string director_left;
        List<string> imagePathFiles_left = new List<string>();
        //切换到图片触发模式
        private bool trigerDLuseImage = false;
        //图片触发模式准备完成
        private volatile bool trigerDLuseImage_isReady = false;
        int pictrueIndex_left = 0;

        private bool window_left_pictruePath_isReady = false;
        #endregion

        private volatile bool isonRunning = true;

        Dictionary<string, dataViewChangeModel> dataViewChangeModel_left;

        //判断是否最小化
        private volatile bool isMiniSizeState = false;

        LinkedList<string> logLines = new LinkedList<string>();
        int maxLogLines = 15; // 设置最大保存的日志条数

        void initDataOnTimeAutoClear()
        {
            day_clearDataTime = IniHelper.SaveSetIni.Read("数据自动清零", "白天清零时间");
            if (string.IsNullOrEmpty(day_clearDataTime))
            {
                IniHelper.SaveSetIni.Write("数据自动清零", "白天清零时间", "08:00:00");
                day_clearDataTime = "08:00:00";
            }
            night_clearDataTime = IniHelper.SaveSetIni.Read("数据自动清零", "晚上清零时间");
            if (string.IsNullOrEmpty(night_clearDataTime))
            {
                IniHelper.SaveSetIni.Write("数据自动清零", "晚上清零时间", "20:00:00");
                night_clearDataTime = "20:00:00";
            }
            string isUse = IniHelper.SaveSetIni.Read("数据自动清零", "数据清零生效");
            if (string.IsNullOrEmpty(isUse))
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

        #region load/close事件
        private void frmOneCamera_Load(object sender, EventArgs e)
        {
            createHead();

            configPath = IniHelper.SaveSetIni.Read("深度学习文件", "配置文件路径");
            bool b_model1isNull = false; //configPath1为空
            treeFileHelper = new TreeFileHelper();
            if (!string.IsNullOrEmpty(configPath))
            {
                treeFileHelper.filePath = configPath;
                addNGDic();
                addNGorOK();
                // 添加项
                addRows(treeFileHelper, list_OKType_classId_left);
            }
            else
            {
                b_model1isNull = true;
                if (tbLog.IsHandleCreated)
                    tbLog.BeginInvoke(GetMyDelegate, "深度学习配置文件路径不存在", null);
                MessageBox.Show("深度学习文件配置文件路径不存在");
            }

            GetMyDelegate = new myDelegate(showLog);
            loadOneCameraSet();

            loadCodeSet();

            imformationShowLoad();
            runControl("继续开始自动运行");
            dataGridView1.ReadOnly = false;

            isonRunning = true;

            initDataOnTimeAutoClear();

            configPath = IniHelper.SaveSetIni.Read("深度学习文件", "配置文件路径");
            Task openCamsTask = new Task(() =>
            {
                //开相机
                openOneHikVisionCam();
                loadPort(); //串口
            });
            Task loadDLModelTask = new Task(() =>
            {
                //progressBar1.BeginInvoke(new Action(() => { progressBar1.Value = 40; }));
                // 1.加载模型
                if (tbLog.IsHandleCreated)
                    tbLog.BeginInvoke(GetMyDelegate, "模型加载中，请稍后...", null);
                // 1.加载模型
                bool b_sucess = loadDlModel(b_model1isNull);
                //progressBar1.BeginInvoke(new Action(() => { progressBar1.Value = 100; }));
                if (tbLog.IsHandleCreated)
                    tbLog.BeginInvoke(GetMyDelegate, "模型加载完成...", null);


                // 2.开线程监听
                grabOneCameraThread = new Thread(runGrabOneCameraThread);
                grabOneCameraThread.IsBackground = true;
                grabOneCameraThread.Start();
                listenSerialPortThread = new Thread(runListenSerialPortThread);
                listenSerialPortThread.IsBackground = true;
                listenSerialPortThread.Start();

            });
            openCamsTask.Start();
            loadDLModelTask.Start();
            startCom();
        }

        private void runListenSerialPortThread()
        {
            while (isonRunning)
            {
                while (frmMain.m_bPause && isonRunning)
                {
                    if (frmMain.m_bResume)
                    {
                        frmMain.m_bPause = false;
                        frmMain.m_bResume = false;
                        break;
                    }
                    Thread.Sleep(20);
                }
                OperateResult<bool> res = null;
                if (panasonicMewtocol.IsOpen())
                    res = panasonicMewtocol.ReadBool(tbGrabImageSignal.Text);
                if ((res != null && res.Content) || isImage)
                {
                    //回写plc
                    panasonicMewtocol.Write(tbGrabImageSignal.Text, false);

                    // 触发线程执行
                    event1.Set();

                    // 等待线程完成
                    countdown.Wait();
                    // TOOD Thread.Sleep(200);
                    // 重置事件和倒计时器
                    //event1.Reset();
                    // event2.Reset();
                    countdown.Reset();
                }
                // 后处理
                postProcess();
                Thread.Sleep(20);
            }
        }

        public void formClose()
        {
            this.Close();
        }
        private void frmOneCamera_FormClosed(object sender, FormClosedEventArgs e)
        {
            // 关闭测试模式
            IniHelper.SaveSetIni.Write("测试模式", "测试模式", "F");

            closeOneHikVisionCam();
            panasonicMewtocol.Write(tbReadySignal.Text, false);
            panasonicMewtocol?.Close();
            if (studyHandle != IntPtr.Zero)
                HTCSharpDemo.Program.ReleaseTree(studyHandle);
            IniHelper.SaveSetIni.Write("布局", "窗口数量", "1");

            this.Resize -= Form_Resize;
            frmMain.controlEvent -= new controlDelegate(runControl);
            set.loadNGEvent -= new loadNGSignalDic(addNGDic);
            set.loadNGEvent -= new loadNGSignalDic(addNGorOK);
            frmSet.newConfigEvent -= new newConfigDel(loadNewFile);
            frmMain.closeAll -= new closeDelegate(formClose);
            set.updateClassIdisOKStateEvent -= new updateClassIdisOKState(updateSetClassIsOk);
            frmCameraExposeSet.cameraExposeSetEvent -= new cameraExposeSet(exposeSet);
            frmCameraExposeSet.cameraExposeCloseEvent -= new cameraExposeClose(exposeClose);
            frmSet.nowisDebugMode -= new nowisDebugMode(changeDebugMode);
            hWindowControl1.MouseWheel -= new System.Windows.Forms.MouseEventHandler(this.my_MouseWheel);
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

        #region 海康相机SDK
        private async void openOneHikVisionCam()
        {
            await Task.Run(() =>
            {
                try
                {
                    // ch:枚举设备 | en:Enum device
                    MyCamera.MV_CC_DEVICE_INFO_LIST stDevList = new MyCamera.MV_CC_DEVICE_INFO_LIST();
                    nRet1 = MyCamera.MV_CC_EnumDevices_NET(MyCamera.MV_GIGE_DEVICE | MyCamera.MV_USB_DEVICE, ref stDevList);
                    frmMain.cameraNum = (int)stDevList.nDeviceNum;

                    if (stDevList.nDeviceNum == 0) 
                        return;
                    stDevInfo1 = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(stDevList.pDeviceInfo[0], typeof(MyCamera.MV_CC_DEVICE_INFO));
                    // ch:创建设备 | en:Create device
                    nRet1 = device1.MV_CC_CreateDevice_NET(ref stDevInfo1); 

                    // ch:打开设备 | en:Open device
                    nRet1 = device1.MV_CC_OpenDevice_NET();

                    //触发模式关闭
                    nRet1 = device1.MV_CC_SetEnumValue_NET("TriggerMode", 0);

                    string exposeTime = IniHelper.SaveSetIni.Read("相机设置", "曝光量");
                    int exposureTime = int.MinValue;
                    if (string.IsNullOrEmpty(exposeTime))
                    {
                        IniHelper.SaveSetIni.Write("相机设置", "曝光量", "500");
                        exposureTime = 500;
                    }
                    else
                    {
                        try
                        {
                            exposureTime = int.Parse(exposeTime);
                        }
                        catch(Exception ex)
                        {
                            
                        }
                    }
                    // ch:设置曝光模式为手动 | en:Set exposure mode to manual
                    nRet1 = device1.MV_CC_SetEnumValue_NET("ExposureMode", 1);

                    // ch:设置曝光时间为5000微秒(5毫秒)

                    nRet1 = device1.MV_CC_SetFloatValue_NET("ExposureTime", exposureTime);

                    // ch:开启抓图 | en:start grab
                    nRet1 = device1.MV_CC_StartGrabbing_NET();

                    // ch:获取包大小 || en: Get Payload Size
                    stParam1 = new MyCamera.MVCC_INTVALUE();
                    nRet1 = device1.MV_CC_GetIntValue_NET("PayloadSize", ref stParam1);
                    if (MyCamera.MV_OK == nRet1)
                    {
                        m_bOneStart1 = true;
                        tbLog.BeginInvoke(GetMyDelegate, "相机初始化成功", null);
                    }
                    else tbLog.BeginInvoke(GetMyDelegate, "相机初始化失败...", null);
                    nPayloadSize1 = stParam1.nCurValue;
                    pBufForDriver1 = Marshal.AllocHGlobal((int)nPayloadSize1);
                    FrameInfo1 = new MyCamera.MV_FRAME_OUT_INFO_EX();
                }
                catch (Exception ex)
                {
                    tbLog.BeginInvoke(GetMyDelegate, "相机初始化失败..."+ex.Message, ex);
                }
            });
        }

        private void closeOneHikVisionCam()
        {
            try
            {
                Marshal.FreeHGlobal(pBufForDriver1);

                // ch:停止抓图 | en:Stop grab image
                nRet1 = device1.MV_CC_StopGrabbing_NET();
                if (MyCamera.MV_OK != nRet1)
                {
                    Console.WriteLine("Stop grabbing failed{0:x8}", nRet1);
                }

                // ch:关闭设备 | en:Close device
                nRet1 = device1.MV_CC_CloseDevice_NET();
                if (MyCamera.MV_OK != nRet1)
                {
                    Console.WriteLine("Close device failed{0:x8}", nRet1);
                }

                // ch:销毁设备 | en:Destroy device
                nRet1 = device1.MV_CC_DestroyDevice_NET();
                if (MyCamera.MV_OK == nRet1)
                {
                    m_bOneStart1 = false;
                }
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
        bool isSet = false;
        private void oneImageToHalcon(HObject image)
        {
            try
            {
                hv_Height1.Dispose(); hv_Width1.Dispose();
                hWindowControl1.HalconWindow.ClearWindow();
                HOperatorSet.GetImageSize(image, out hv_Width1, out hv_Height1);
                //this.hWindowControl1.ImagePart = new Rectangle(0,0,hv_Width1,hv_Height1);
                HalconHelper.imageLocation(hv_Width1, hv_Height1, hWindowControl1.Width, hWindowControl1.Height, out row1, out column1, out row2, out column2);
                if (!isSet)
                {
                    isSet = true;
                    // 检查窗口是否最小化
                    if (!isMiniSizeState)
                        HOperatorSet.SetPart(this.hWindowControl1.HalconWindow, row1, column1, row2, column2);
                }
                HOperatorSet.DispObj(image, this.hWindowControl1.HalconWindow);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                LogHelper.WriteLog(ex.ToString());
            }
        }
        #endregion

        #region 初始化读码设置
        private void loadCodeSet()
        {
            string isOpen = IniHelper.SaveSetIni.Read("单相机读码设置", "是否启用");
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
                tbCodeAddress.Text = IniHelper.SaveSetIni.Read("单相机读码设置", "读码地址");
                tbCodeLength.Text = IniHelper.SaveSetIni.Read("单相机读码设置", "读码长度");
            }
        }
        #endregion

        #region 委托显示日志
        private void showLog(string log,Exception ex)
        {
            // 添加新的日志行
            if (ex != null)
            {
                logLines.AddLast(DateTime.Now.ToString("yyyy/MM/dd-HH:mm:ss:fff") + ":" + log + ex.Message + "\r\n");
                LogHelper.WriteLog(log, ex);
            }
            else
            {
                logLines.AddLast(DateTime.Now.ToString("yyyy/MM/dd-HH:mm:ss:fff") + ":" + log + "\r\n");
                LogHelper.WriteLog(log);
            }
            // 如果超过最大日志条数，移除最早的日志行
            if (logLines.Count > maxLogLines)
            {
                logLines.RemoveFirst();
            }
            tbLog.Text = string.Join(Environment.NewLine, logLines);
            tbLog.SelectionStart = tbLog.TextLength;
            tbLog.ScrollToCaret();
            tbLog.Refresh();

            //this.tbLog.AppendText(DateTime.Now.ToString("yyyy/MM/dd-HH:mm:ss:fff") + ":" + log + "\r\n");
            //if (ex == null)
            //    LogHelper.WriteLog(log);
            //else
            //    LogHelper.WriteLog(log, ex);
            //tbLog.SelectionStart = tbLog.TextLength;
            //tbLog.ScrollToCaret();
        }
        #endregion

        #region 初始化相机的各种参数
        private void loadOneCameraSet()
        {
            if (!String.IsNullOrEmpty(IniHelper.SaveSetIni.Read("相机1", "NG信号1")))
                label9.Text = IniHelper.SaveSetIni.Read("相机1", "NG信号1");
            btHandTest.Enabled = false;
            cbGrayOrRgb.Items.Add("黑白");
            cbGrayOrRgb.Items.Add("彩色");
            if (String.IsNullOrEmpty(IniHelper.SaveSetIni.Read("相机设置", "采图设置")))
                cbGrayOrRgb.SelectedIndex = 0;
            else
            {
                if (IniHelper.SaveSetIni.Read("相机设置", "采图设置") == "彩色")
                    cbGrayOrRgb.SelectedIndex = 1;
                else
                    cbGrayOrRgb.SelectedIndex = 0;
            }
            for (int i = 0; i < SerialPort.GetPortNames().Length; i++)
            {
                cbPortName.Items.Add(SerialPort.GetPortNames()[i]);
                if (!String.IsNullOrEmpty(IniHelper.SaveSetIni.Read("串口设置", "串口号")))
                {
                    if (SerialPort.GetPortNames()[i] == IniHelper.SaveSetIni.Read("串口设置", "串口号"))
                        cbPortName.SelectedIndex = i;
                }
                else
                    cbPortName.SelectedIndex = 0;
            }
            tbBaudRate.Text = IniHelper.SaveSetIni.Read("串口设置", "波特率");
            tbDateBits.Text = IniHelper.SaveSetIni.Read("串口设置", "数据位");
            tbStopBits.Text = IniHelper.SaveSetIni.Read("串口设置", "停止位");
            cbParity.Items.Add("无");
            cbParity.Items.Add("奇校验");
            cbParity.Items.Add("偶校验");
            if (String.IsNullOrEmpty(IniHelper.SaveSetIni.Read("串口设置", "校验位")))
                cbParity.SelectedIndex = 0;
            else
            {
                for (int i = 0; i < cbParity.Items.Count; i++)
                {
                    if (IniHelper.SaveSetIni.Read("串口设置", "校验位") == cbParity.Items[i].ToString())
                        cbParity.SelectedIndex = i;
                }
            }
            tbReadySignal.Text = IniHelper.SaveSetIni.Read("串口设置", "准备信号");
            tbGrabImageSignal.Text = IniHelper.SaveSetIni.Read("串口设置", "拍照信号");
            tbOKSignal.Text = IniHelper.SaveSetIni.Read("串口设置", "OK信号");
        }
        #endregion

        #region 串口通讯
        /// <summary>
        /// 只有一个串口的时候接受plc拍照信号的线程
        /// </summary>
        Thread grabOneCameraThread = null;
        private void btOpenPlc_Click_1(object sender, EventArgs e)
        {
            panasonicMewtocol?.Close();
            loadPort();
        }

        private void btClosePlc_Click_1(object sender, EventArgs e)
        {
            panasonicMewtocol.Write(tbReadySignal.Text, false);
            panasonicMewtocol?.Close();
            showLog("关闭串口...",null);
            btClosePlc.Enabled = false;
            btOpenPlc.Enabled = true;
            tbReadySignal.Enabled = true;
            tbGrabImageSignal.Enabled = true;
            cbPortName.Enabled = true;
            cbParity.Enabled = true;
            tbBaudRate.Enabled = true;
            tbDateBits.Enabled = true;
            tbStopBits.Enabled = true;
            btAddNGSignal.Enabled = true;
        }

        private bool loadSet()
        {
            if (String.IsNullOrEmpty(tbGrabImageSignal.Text) || String.IsNullOrEmpty(tbReadySignal.Text) || String.IsNullOrEmpty(tbOKSignal.Text))
            {
                MessageBox.Show("请输入PLC地址");
                return false;
            }
            if (ngdic.Count == 0)
            {
                MessageBox.Show("请至少输入一个NG地址");
                return false;
            }
            if (!Directory.Exists(IniHelper.SaveSetIni.Read("图片设置", "保存路径")) ||
                !File.Exists(IniHelper.SaveSetIni.Read("深度学习文件", "配置文件路径")))

            {
                MessageBox.Show("请先设置好文件路径");
                return false;
            }
            if (cbEnableCode.Checked)
            {
                if (String.IsNullOrEmpty(tbCodeLength.Text) || String.IsNullOrEmpty(tbCodeAddress.Text))
                {
                    MessageBox.Show("请先输入读码地址或长度");
                    return false;
                }
                IniHelper.SaveSetIni.Write("单相机读码设置", "读码地址", tbCodeAddress.Text);
                IniHelper.SaveSetIni.Write("单相机读码设置", "读码长度", tbCodeLength.Text);
            }
            if (!panasonicMewtocol.IsOpen() || !m_bOneStart1)
            {
                MessageBox.Show("串口未打开或相机未初始化");
                return false;
            }
            OperateResult result = panasonicMewtocol.Write(tbReadySignal.Text, true);
            if (!result.IsSuccess)
            {
                MessageBox.Show("准备信号发送失败,请检查通讯设置");
                return false;
            }

            return true;
        }

        private void startCom()
        {
            IniHelper.SaveSetIni.Write("串口设置", "拍照信号", tbGrabImageSignal.Text);
            IniHelper.SaveSetIni.Write("串口设置", "准备信号", tbReadySignal.Text);
            IniHelper.SaveSetIni.Write("串口设置", "OK信号", tbOKSignal.Text);
            tbReadySignal.Enabled = false;
            tbGrabImageSignal.Enabled = false;
            tbOKSignal.Enabled = false;
            btAddNGSignal.Enabled = false;
            cbGrayOrRgb.Enabled = false;
            btClosePlc.Enabled = false;

            btHandTest.Enabled = true;
            btClearData.Enabled = false;
            cbEnableCode.Enabled = false;
            showLog("启动成功...",null);
        }
        private void loadPort()
        {
            try
            {
                panasonicMewtocol?.Close();
                panasonicMewtocol.SerialPortInni(sp =>
                {
                    sp.PortName = cbPortName.Text;
                    sp.BaudRate = Convert.ToInt32(tbBaudRate.Text);
                    sp.DataBits = Convert.ToInt32(tbDateBits.Text); ;
                    sp.StopBits = Convert.ToInt32(tbStopBits.Text) == 0 ? System.IO.Ports.StopBits.None : (Convert.ToInt32(tbStopBits.Text) == 1 ? System.IO.Ports.StopBits.One : System.IO.Ports.StopBits.Two);
                    sp.Parity = cbParity.SelectedIndex == 0 ? System.IO.Ports.Parity.None : (cbParity.SelectedIndex == 1 ? System.IO.Ports.Parity.Odd : System.IO.Ports.Parity.Even);
                });
                OperateResult operateResult = panasonicMewtocol.Open();
                if (operateResult.IsSuccess)
                {
                    //showLog("串口打开成功...");
                    btOpenPlc.Enabled = false;
                    btClosePlc.Enabled = true;
                    cbPortName.Enabled = false;
                    cbParity.Enabled = false;
                    tbBaudRate.Enabled = false;
                    tbDateBits.Enabled = false;
                    tbStopBits.Enabled = false;
                    IniHelper.SaveSetIni.Write("串口设置", "串口号", cbPortName.Text);
                    IniHelper.SaveSetIni.Write("串口设置", "波特率", tbBaudRate.Text);
                    IniHelper.SaveSetIni.Write("串口设置", "数据位", tbDateBits.Text);
                    IniHelper.SaveSetIni.Write("串口设置", "停止位", tbStopBits.Text);
                    IniHelper.SaveSetIni.Write("串口设置", "校验位", cbParity.Text);
                }
                else
                {
                    this.BeginInvoke(new Action(() =>
                    {
                        btClosePlc.Enabled = false;
                        btOpenPlc.Enabled = true;
                        MessageBox.Show($"{cbPortName.Text}拒绝访问,请换个COM口连接...");
                    }));
                    tbLog.BeginInvoke(GetMyDelegate, $"{cbPortName.Text}拒绝访问,请换个COM口连接...",null);
                }
            }
            catch (Exception ex)
            {
                this.BeginInvoke(new Action(() =>
                {
                    btClosePlc.Enabled = false;
                    btOpenPlc.Enabled = true;
                }));
                tbLog.BeginInvoke(GetMyDelegate, "串口设置失败", ex);
            }
        }

        #endregion

        #region 步骤枚举

        enum STEP
        {
            IMAGE_SIGNAL = 1,
            GRAB_IMAGE,
            IMAGE_PROC,
            OUTPUT_RESULT,
            SAVE_IMAGE
        }

        #endregion

        public void HObject2Bpp24(HObject ho_image, out Bitmap res24)
        {

            //HOperatorSet.WriteImage(ho_image, "bmp", 0, @"E:\Dick\CASE\Type C R1.3&0.9\data\20200520\NG原图\CCD1\ex\xiaok1.bmp");
            HTuple width0, height0, type, width, height;
            //获取图像尺寸
            HOperatorSet.GetImageSize(ho_image, out width0, out height0);
            // 创建交错格式图像
            HOperatorSet.InterleaveChannels(ho_image, out HObject InterImage, "argb", "match", 255);  //"rgb", 4 * width0, 0     "argb", "match", 255

            //获取交错格式图像指针
            HOperatorSet.GetImagePointer1(InterImage, out HTuple Pointer, out type, out width, out height);
            IntPtr ptr = Pointer;
            //构建新Bitmap图像
            Bitmap res32 = new Bitmap(width / 4, height, width, System.Drawing.Imaging.PixelFormat.Format32bppArgb, ptr);  // Format32bppArgb     Format24bppRgb


            //32位Bitmap转24位
            res24 = new Bitmap(res32.Width, res32.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            Graphics graphics = Graphics.FromImage(res24);
            graphics.DrawImage(res32, new Rectangle(0, 0, res32.Width, res32.Height));


            res32.Dispose();
        }
        public void HObject2Bpp8_(HObject image, out Bitmap res)
        {
            HTuple hpoint, type, width, height;

            const int Alpha = 255;

            HOperatorSet.GetImagePointer1(image, out hpoint, out type, out width, out height);

            res = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            ColorPalette pal = res.Palette;
            for (int i = 0; i <= 255; i++)
            {
                pal.Entries[i] = System.Drawing.Color.FromArgb(Alpha, i, i, i);
            }
            res.Palette = pal;
            Rectangle rect = new Rectangle(0, 0, width, height);
            BitmapData bitmapData = res.LockBits(rect, ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            int PixelSize = Bitmap.GetPixelFormatSize(bitmapData.PixelFormat) / 8;

            IntPtr ptr1 = bitmapData.Scan0;
            IntPtr ptr2 = hpoint;
            int bytes = width * height;
            byte[] rgbvalues = new byte[bytes];
            System.Runtime.InteropServices.Marshal.Copy(ptr2, rgbvalues, 0, bytes);
            System.Runtime.InteropServices.Marshal.Copy(rgbvalues, 0, ptr1, bytes);



            res.UnlockBits(bitmapData);

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
            }
            else if (channels == 1 && type.S == "byte")
            {
                HTuple imagePtr;
                HOperatorSet.GetImagePointer1(image, out imagePtr, out type, out width, out height);

                int dataSize = width * height * Marshal.SizeOf<byte>();  // 计算图像数据的总大小（bytes）
                IntPtr dataCopy = Marshal.AllocHGlobal(dataSize);         // 分配一块内存用于复制图像数据
                byte[] imageData = new byte[dataSize];                    // 创建一个字节数组用于临时存储图像数据
                Marshal.Copy(imagePtr, imageData, 0, dataSize);            // 复制图像数据到临时数组

                Marshal.Copy(imageData, 0, dataCopy, dataSize);           // 将临时数组中的数据复制到新的内存地址

                imageHt = new ImageHt();
                imageHt.data = dataCopy;                                // 设置图像的数据首地址为复制后的内存地址
                imageHt.width = width;                                  // 图像的宽度
                imageHt.height = height;                                // 图像的高度
                imageHt.channels = 1;                                   // Halcon图像默认为单通道
                imageHt.width_step = width * Marshal.SizeOf<byte>();    // 图像每行的步长（bytes）

                // 释放临时数组分配的内存
                imageData = null;

                //int imageSize = width.I * height.I;
                //byte[] imageData = new byte[imageSize];

                //// 将单通道数据复制到数组中
                //Marshal.Copy(imagePtr, imageData, 0, imageSize);

                //// 将数组转换为IntPtr
                //IntPtr imageDataIntPtr = Marshal.AllocHGlobal(imageData.Length);
                //Marshal.Copy(imageData, 0, imageDataIntPtr, imageData.Length);

                //imageHt.data = imageDataIntPtr;
                //imageHt.width = width.I;
                //imageHt.height = height.I;
                //imageHt.channels = 1;
                //imageHt.width_step = imageHt.width;
            }
            else
            {
                throw new Exception("Halcon转换的图像格式不正确");
            }
        }

        /// <summary>
        /// 保存汇图格式图像
        /// </summary>
        /// <param name="imageHt"></param>
        /// <param name="filePath"></param>
        public void SaveImageHt(ImageHt imageHt, string filePath)
        {
            int dataSize = imageHt.width * imageHt.height * imageHt.channels * Marshal.SizeOf<byte>();
            byte[] imageData = new byte[dataSize];

            Marshal.Copy(imageHt.data, imageData, 0, dataSize);

            Bitmap bitmap = new Bitmap(imageHt.width, imageHt.height, imageHt.width_step, PixelFormat.Format8bppIndexed, imageHt.data);

            // 为位图创建调色板
            ColorPalette palette = bitmap.Palette;
            for (int i = 0; i < 256; i++)
            {
                palette.Entries[i] = Color.FromArgb(i, i, i);
            }
            bitmap.Palette = palette;

            // 锁定位图数据
            BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, imageHt.width, imageHt.height), ImageLockMode.WriteOnly, bitmap.PixelFormat);

            // 将图像数据复制到位图
            Marshal.Copy(imageData, 0, bmpData.Scan0, dataSize);

            // 解锁位图数据
            bitmap.UnlockBits(bmpData);

            // 保存位图到磁盘
            bitmap.Save(filePath, ImageFormat.Bmp);

            // 释放位图对象
            bitmap.Dispose();
        }


        #region 自动运行线程
        int imageNum = 1;
        private void runGrabOneCameraThread()
        {
            while (isonRunning)
            {
                // 等待线程plc监控线程触发
                event1.Wait();
                HTCSharpDemo.Program.ImageHt imageHt = new HTCSharpDemo.Program.ImageHt();
                MyCamera.MV_FRAME_OUT stFrameOut = new MyCamera.MV_FRAME_OUT();
                string configPath = IniHelper.SaveSetIni.Read("深度学习文件", "配置文件路径");
                try
                {
                    if (isonRunning)
                    {
                        
                        DateTime endtime;
                        HTCSharpDemo.Program DeepLearning = new HTCSharpDemo.Program();
                        DateTime startime = DateTime.Now;
                               
                        // 非图片触发
                        if (!trigerDLuseImage_isReady)
                        {
                            this.BeginInvoke(new Action(() => { btHandTest.Enabled = false; }));
                            tbLog.BeginInvoke(GetMyDelegate, "收到拍照信号...", null);
                            tbLog.BeginInvoke(GetMyDelegate, "开始拍照...", null);
                            getImgAndDisply(device1, ref hWindowControl1, out ho_image1, ref stFrameOut);
                            ConvertToChannelImageIntPtr(ho_image1, out imageHt);
                            //SaveImageHt(imageHt, "汇图图像.bmp");
                            tbLog.BeginInvoke(GetMyDelegate, $"拍照结束,已拍{imageNum++}次...", null);
                        }
                        else
                        {
                            string corner = IniHelper.SaveSetIni.Read("旋转角度", "角度");
                            if (!string.IsNullOrEmpty(corner))
                            {
                                double Rotation = double.Parse(corner);
                                HTuple angle = new HTuple(Rotation); // 旋转角度为90度
                                HOperatorSet.RotateImage(ho_image1, out ho_image1, angle, "constant");
                            }   
                            ConvertToChannelImageIntPtr(ho_image1, out imageHt);
                            hv_Width1.Dispose(); hv_Height1.Dispose();
                            HOperatorSet.GetImageSize(ho_image1, out hv_Width1, out hv_Height1);
                            HalconHelper.imageLocation(hv_Width1, hv_Height1, hWindowControl1.Width, hWindowControl1.Height, out row1, out column1, out row2, out column2);
                            if (!isMiniSizeState)
                                HOperatorSet.SetPart(this.hWindowControl1.HalconWindow, row1, column1, row2, column2);
                            HOperatorSet.ClearWindow(this.hWindowControl1.HalconWindow);
                            HOperatorSet.DispObj(ho_image1, this.hWindowControl1.HalconWindow);
                        }

                        Dictionary<int, NodeInfo> dic_nodeIdWithNodeInfo = treeFileHelper.dic_nodeIdWithNodeInfo;
                        List<string> detectedNGItemsNGtype = DeepLearning.DeepStudy1(studyHandle, imageHt, test_num, ref dic_nodeIdWithNodeInfo, list_OKType_classId_left);
                        tbLog.BeginInvoke(GetMyDelegate, "深度学习处理完成...",null);
                        AllNum++;
                        IniHelper.SaveSetIni.Write("单相机生产信息", "生产总数", AllNum.ToString());
                        //需要定位
                        if (treeFileHelper.dingweiNodeId != int.MinValue)//需要启用定位
                        {
                            if (DeepLearning.res1.ContainsKey(treeFileHelper.dingweiNodeId) && DeepLearning.res1[treeFileHelper.dingweiNodeId].Count == 1) //定位到了
                            {
                                bool hasNoNG = true;
                                foreach (var pair_result in DeepLearning.res1)
                                {
                                    if (pair_result.Key != treeFileHelper.dingweiNodeId)
                                    {
                                        List<result> results = pair_result.Value;
                                        foreach (result r in results)
                                        {
                                            if (list_OKType_classId_left.Contains(r.class_id))
                                                continue;
                                            if (r.class_id.Equals("OK") || r.class_id.Equals("ok") || r.class_id.Equals("Ok"))
                                                continue;
                                            if (r.class_id.Equals("其他"))
                                                continue;

                                            NGNum++;
                                            result = "NG";
                                            IniHelper.SaveSetIni.Write("相机1生产信息", "NG总数", NGNum.ToString());
                                            if (!trigerDLuseImage_isReady)
                                                tellPLCResult(results[0].class_id);
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
                                    OKNum++;
                                    result = "OK";
                                    IniHelper.SaveSetIni.Write("相机1生产信息", "OK总数", OKNum.ToString());
                                    if (!trigerDLuseImage_isReady)
                                        tellPLCResult("");
                                }
                            }
                            else //定位失败
                            {
                                NGNum++;
                                result = "定位失败";
                                IniHelper.SaveSetIni.Write("相机1生产信息", "NG总数", NGNum.ToString());
                                //定位节点特殊，NGTypeSignal.txt 中NGType为"1"
                                if (!trigerDLuseImage_isReady)
                                    tellPLCResult("1");
                            }
                        }
                        else
                        {
                            bool hasNoNG = true;
                            foreach (var pair_result in DeepLearning.res1)
                            {
                                if (pair_result.Key != treeFileHelper.dingweiNodeId)
                                {
                                    List<result> results = pair_result.Value;
                                    foreach (result r in results)
                                    {
                                        if (list_OKType_classId_left.Contains(r.class_id))
                                            continue;
                                        if (r.class_id.Equals("OK") || r.class_id.Equals("ok") || r.class_id.Equals("Ok"))
                                            continue;
                                        if (r.class_id.Equals("其他"))
                                            continue;

                                        NGNum++;
                                        result = "NG";
                                        IniHelper.SaveSetIni.Write("相机1生产信息", "NG总数", NGNum.ToString());
                                        if (!trigerDLuseImage_isReady)
                                            tellPLCResult(results[0].class_id);
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
                                OKNum++;
                                result = "OK";
                                IniHelper.SaveSetIni.Write("相机1生产信息", "OK总数", OKNum.ToString());
                                if (!trigerDLuseImage_isReady)
                                    tellPLCResult("");
                            }
                        }

                        showNGLocation(DeepLearning.res1);
                        endtime = DateTime.Now;
                        TimeSpan time = endtime - startime;
                        ct = (int)time.TotalMilliseconds;

                        tbLog.BeginInvoke(GetMyDelegate, "输出结果...", null);
                        showResult();
                        referenceTable(DeepLearning.res1);
                        tbLog.BeginInvoke(GetMyDelegate, "结果输出完成...", null);

                        string okStatus = IniHelper.SaveSetIni.Read("图片路径", "OK图片标志");
                        string ngStatus = IniHelper.SaveSetIni.Read("图片路径", "NG图片标志");
                        if (okStatus == "T" || ngStatus == "T")
                        {
                            tbLog.BeginInvoke(GetMyDelegate, "正在保存图片...",null);
                            saveImage(okStatus, ngStatus, result, detectedNGItemsNGtype);
                        }  
                    }
                }
                catch (Exception ex)
                {
                    tbLog.BeginInvoke(GetMyDelegate, "流程运行错误:"+ex.Message, ex);
                    if (ex.Message.Contains("4056"))
                    {
                        tbLog.BeginInvoke(GetMyDelegate, "相机取图失败，请检查...", null);
                    }
                }
                finally
                {
                    this.BeginInvoke(new Action(() => { btHandTest.Enabled = true; }));
                    if (!trigerDLuseImage_isReady)
                    {
                        isImage = false;
                        device1.MV_CC_FreeImageBuffer_NET(ref stFrameOut);
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
        #endregion

        // 将 ImageHt 中的图像数据保存到本地
        public void SaveImage(ImageHt image, string savePath)
        {
            if (image.channels == 1)
            {
                // 创建 Bitmap 对象并关联图像数据的指针
                Bitmap bmp = new Bitmap(image.width, image.height, image.width_step, System.Drawing.Imaging.PixelFormat.Format8bppIndexed, image.data);

                // 保存图片到指定路径
                bmp.Save(savePath, ImageFormat.Jpeg);
            }
            else if (image.channels == 3)
            {
                // 创建 Bitmap 对象并关联图像数据的指针
                Bitmap bmp = new Bitmap(image.width, image.height, image.width_step, System.Drawing.Imaging.PixelFormat.Format24bppRgb, image.data);

                // 保存图片到指定路径
                bmp.Save(savePath, ImageFormat.Jpeg);
            }

        }

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

        private void getImgAndDisply(MyCamera device, ref HSmartWindowControl hWindowControl, out HObject image, ref MyCamera.MV_FRAME_OUT stFrameOut)
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
                    catch (System.Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
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
                        MessageBox.Show(ex.ToString());
                        return;
                    }
                }
                else
                {
                    device.MV_CC_FreeImageBuffer_NET(ref stFrameOut);
                }

                string corner = IniHelper.SaveSetIni.Read("旋转角度", "角度");
                if (!string.IsNullOrEmpty(corner))
                {
                    double Rotation = double.Parse(corner);
                    if (Rotation != 0)
                    {
                        HTuple angle = new HTuple(Rotation);
                        HOperatorSet.RotateImage(image, out image, angle, "constant");
                    }
                }
                hWindowControl.HalconWindow.ClearWindow();
                HTuple imageWidth, imageHeight;
                HOperatorSet.GetImageSize(image, out imageWidth, out imageHeight);
                HalconHelper.imageLocation(imageWidth, imageHeight, hWindowControl.Width, hWindowControl.Height, out row1, out column1, out row2, out column2);
                imageHeight.Dispose();
                imageWidth.Dispose();
                if (!isMiniSizeState)
                    HOperatorSet.SetPart(hWindowControl.HalconWindow, row1, column1, row2, column2);
                HOperatorSet.DispObj(image, hWindowControl.HalconWindow);
            }
            if (pImageBuf != IntPtr.Zero && isAllocatedMemory)
            {
                Marshal.FreeHGlobal(pImageBuf);
                pImageBuf = IntPtr.Zero;
            }
        }

        void countClearanceSetting(bool setNGText)
        {
            string configpath = IniHelper.SaveSetIni.Read("深度学习文件", "配置文件路径");
            LogHelper.WriteLog($"已清空历史数据,总数:{lbAllNum.Text};OK数:{lbOKNum.Text};NG数:{lbNGNum.Text};良率:{lbOKPercent.Text}");
            lbAllNum.Text = "0";
            AllNum = 0;
            lbOKNum.Text = "0";
            OKNum = 0;
            lbNGNum.Text = "0";
            NGNum = 0;
            lbOKPercent.Text = "0";
            lbCt.Text = "0";
            if (setNGText)
            {
                lbResult.BackColor = System.Drawing.Color.Red;
                lbResult.Text = "NG";
            }
            IniHelper.SaveSetIni.Write("单相机生产信息", "生产总数", "0");
            IniHelper.SaveSetIni.Write("单相机生产信息", "OK总数", "0");
            IniHelper.SaveSetIni.Write("单相机生产信息", "NG总数", "0");
            dataGridView1.Rows.Clear();
            addRows(treeFileHelper, list_OKType_classId_left);
        }
        #region 缩放
        bool m_bZoom = false;
        ZoomImage zi = null;
        private void 缩放ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ho_image1 != null && ho_image1.CountObj() != 0)
            {
                m_bZoom = true;
            }
        }
        #endregion

        #region 鼠标抬起移动事件
        bool m_bMouseLeft = false;
        private void hWindowControl1_HMouseDown(object sender, HMouseEventArgs e)
        {
            if (m_bZoom)
            {
                if (e.Button == MouseButtons.Left)
                {
                    hWindowControl1.Cursor = Cursors.Hand;
                    zi.StartX = e.X;
                    zi.StartY = e.Y;
                    m_bMouseLeft = true;
                }
            }
        }
        string xy = null;
        string gray = null;
        private void hWindowControl1_HMouseMove(object sender, HMouseEventArgs e)
        {
            try
            {
                HTuple graval = new HTuple();
                xy = $"像素坐标:{(int)e.Y},{(int)e.X}";
                if (ho_image1 != null)
                {
                    if (e.Y < 0 || e.X < 0 || e.Y > hv_Height1 - 1 || e.X > hv_Width1 - 1)
                        gray = "像素灰度:-";
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
                showXYorGrayEvent?.Invoke(xy, gray);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                LogHelper.WriteLog(ex.Message);
            }
            if (m_bZoom)
            {
                if (m_bMouseLeft)
                {
                    double offsetX = e.X - zi.StartX;
                    double offsetY = e.Y - zi.StartY;

                    zi.moveImage(offsetX, offsetY, ho_image1);
                }
            }
        }

        private void hWindowControl1_HMouseUp(object sender, HMouseEventArgs e)
        {
            if (m_bZoom)
            {
                this.hWindowControl1.Cursor = Cursors.Arrow;
                m_bMouseLeft = false;
            }
        }

        private void hWindowControl1_HMouseWheel(object sender, HMouseEventArgs e)
        {
            if (m_bZoom)
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

                zi.zoomImage(e.X, e.Y, scale, ho_image1);
            }
        }
        #endregion


        #region 读取图片
        private void 读取图片ToolStripMenuItem_Click(object sender, EventArgs e)
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
                    HOperatorSet.GetImageSize(ho_image1, out hv_Width1, out hv_Height1);
                    HalconHelper.imageLocation(hv_Width1, hv_Height1, winWidth, winHeight, out row1, out column1, out row2, out column2);
                    if (!isMiniSizeState)
                        HOperatorSet.SetPart(this.hWindowControl1.HalconWindow, row1, column1, row2, column2);
                    HOperatorSet.DispObj(ho_image1, this.hWindowControl1.HalconWindow);
                }
            }
            catch (Exception ex)
            {
                ho_image1.Dispose();
                hv_Width1.Dispose();
                hv_Height1.Dispose();
                LogHelper.WriteLog(ex.ToString());
                MessageBox.Show(ex.ToString());
            }
        }
        #endregion

        #region 运行过程中禁止操作软件
        private void runControl(string str)
        {
            showLog(str,null);
            if (str == "继续开始自动运行...")
            {
                btClosePlc.Enabled = false;
                contextMenuStrip1.Enabled = false;
                cbGrayOrRgb.Enabled = false;
                btAddNGSignal.Enabled = false;
                cbEnableCode.Enabled = false;
                if (cbEnableCode.Checked)
                {
                    tbCodeAddress.Enabled = false;
                    tbCodeLength.Enabled = false;
                    tbCodeResult.Enabled = false;
                }
                tbReadySignal.Enabled = false;
                tbGrabImageSignal.Enabled = false;
                tbOKSignal.Enabled = false;
                btAddNGSignal.Enabled = false;
                btHandTest.Enabled = true;
                btClearData.Enabled = false;
                contextMenuStrip1.Enabled = false;

            }
            else if (str == "停止...")
            {
                btClosePlc.Enabled = true;
                contextMenuStrip1.Enabled = true;
                cbGrayOrRgb.Enabled = true;
                btAddNGSignal.Enabled = true;
                cbEnableCode.Enabled = true;
                if (cbEnableCode.Checked)
                {
                    tbCodeAddress.Enabled = true;
                    tbCodeLength.Enabled = true;
                    tbCodeResult.Enabled = true;
                }
                tbReadySignal.Enabled = true;
                tbOKSignal.Enabled = true;
                tbGrabImageSignal.Enabled = true;
                btAddNGSignal.Enabled = true;
                btHandTest.Enabled = false;
                btClearData.Enabled = true;
                contextMenuStrip1.Enabled = true;
            }
            //if (studyHandle == IntPtr.Zero)
            //    btLoadConfig.Enabled = true;
            //else
            //    btLoadConfig.Enabled = false;
            if (panasonicMewtocol.IsOpen())
            {
                if (str == "继续开始自动运行...")
                    panasonicMewtocol.Write(tbReadySignal.Text, true);
                else if (str == "停止...")
                    panasonicMewtocol.Write(tbReadySignal.Text, false);
            }
        }
        #endregion

        #region 采图设置
        private void cbGrayOrRgb_SelectedIndexChanged(object sender, EventArgs e)
        {
            IniHelper.SaveSetIni.Write("相机设置", "采图设置", cbGrayOrRgb.Text);
        }
        #endregion

        #region NG信号设置
        private void button1_Click(object sender, EventArgs e)
        {
            set.treeFileHelper = treeFileHelper;
            set.setClassSetOK(ref list_OKType_classId_left);
            set.ShowDialog();
        }
        #endregion

        #region 将ng输出类型对应的plc地址添加进字典

        private void addNGDic()
        {
            ngdic.Clear();
            NGTypePara.OutSignalSets signalSets = new NGTypePara.OutSignalSets();
            string path = AppDomain.CurrentDomain.BaseDirectory + "OutType1.txt";
            string jsonFromFile = File.ReadAllText(path);
            signalSets = JsonConvert.DeserializeObject<OutSignalSets>(jsonFromFile);
            if (signalSets != null)
            {
                foreach (var item in signalSets.outsignals)
                {
                    if (item.isEnable)
                        ngdic.Add(item.OutSignalName, item.OutSignal);
                }
            }
        }
        #endregion

        #region 将ok跟ng类型存入字典中
        public void addNGorOK()
        {
            try
            {
                if (!File.Exists(rootDirectory))
                {
                    MessageBox.Show("请先设置好plc的NG信号");
                    return;
                }
                list_OKType_classId_left.Clear();
                NGType.Clear();
                NGTypePara.NGType ngTypes = new NGTypePara.NGType();
                string jsonFromFile = File.ReadAllText(rootDirectory);
                ngTypes = JsonConvert.DeserializeObject<NGType>(jsonFromFile);
                if (ngTypes != null && ngdic.Count != 0)
                {
                    foreach (var item in ngTypes.NGTypeConfigs)
                    {
                        if (item.isOK || item.NGType.StartsWith("其他") || item.NGType.StartsWith("OK"))
                        {
                            if (!list_OKType_classId_left.Contains(item.NGType))
                                list_OKType_classId_left.Add(item.NGType);
                        }
                        else
                        {
                            if (!NGType.ContainsKey(item.NGType))
                                NGType.Add(item.NGType, ngdic[item.OutType]);
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

        #region 是否启用读码
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (cbEnableCode.Checked)
            {
                tbCodeAddress.Enabled = true;
                tbCodeLength.Enabled = true;
                tbCodeResult.Enabled = true;
                IniHelper.SaveSetIni.Write("单相机读码设置", "是否启用", "T");
            }
            else
            {
                tbCodeAddress.Enabled = false;
                tbCodeLength.Enabled = false;
                tbCodeResult.Enabled = false;
                IniHelper.SaveSetIni.Write("单相机读码设置", "是否启用", "F");
            }
        }
        #endregion

        #region 实时采集(相机曝光设置)
        private void runGrabImage()
        {
            try
            {
                while (currentIsSetExpose)
                {
                    //grabOneImage(out ho_image1);
                    //oneImageToHalcon(ho_image1);
                    MyCamera.MV_FRAME_OUT stFrameOut = new MyCamera.MV_FRAME_OUT();
                    getImgAndDisply(device1, ref hWindowControl1, out ho_image1, ref stFrameOut);
                    device1.MV_CC_FreeImageBuffer_NET(ref stFrameOut);
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message);
            }
        }
        #endregion

        #region 实时设置曝光
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (!m_bOneStart1)
            {
                MessageBox.Show("相机1未打开,请先初始化相机");
                return;
            }
            currentIsSetExpose = true;
            realTimeThread = new Thread(runGrabImage);
            realTimeThread.IsBackground = true;
            realTimeThread.Start();
            frmCameraExposeSet cameraExposeSet = new frmCameraExposeSet("单相机");
            cameraExposeSet.Show();
        }
        #endregion

        #region 相机曝光事件
        private void exposeSet(float expose, string cc)
        {
            device1.MV_CC_SetEnumValue_NET("ExposureAuto", 0);
            nRet1 = device1.MV_CC_SetFloatValue_NET("ExposureTime", expose);
            if (nRet1 == MyCamera.MV_OK)
            {
                IniHelper.SaveSetIni.Write("相机设置", "曝光量", expose.ToString());
                showLog("相机曝光设置成功...", null);
                LogHelper.WriteLog($"相机曝光设置成功:{expose}");
            }
            else
            {
                showLog("相机曝光设置失败...", null);
                LogHelper.WriteLog("相机曝光设置失败...");
                MessageBox.Show("Set Exposure Time Fail!");
            }
        }
        #endregion

        #region 暂停相机曝光窗口时触发关闭实时采集线程
        private void exposeClose()
        {
            currentIsSetExpose = false;
            realTimeThread?.Join();
        }
        #endregion

        void onTimeClear()
        {
            string isUse = IniHelper.SaveSetIni.Read("数据自动清零", "数据清零生效");
            if (string.IsNullOrEmpty(isUse) || isUse.Equals("F"))
                isUseClearDataTime = false;
            else if (isUse.Equals("T"))
                isUseClearDataTime = true;

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
                        //计数清零
                        this.BeginInvoke(new Action(() =>
                        {
                            countClearanceSetting(false);
                        }));
                    }
                }
                else if (nighttimeDifference.Hours == 0 && nighttimeDifference.Minutes == 0)
                {
                    if (nighttimeDifference.Seconds < 4)
                    {
                        //计数清零
                        this.BeginInvoke(new Action(() =>
                        {
                            countClearanceSetting(false);
                        }));
                    }
                }
            }
        }

        void postProcess()
        {
            string isUse = IniHelper.SaveSetIni.Read("数据自动清零", "数据清零生效");
            if (string.IsNullOrEmpty(isUse) || isUse.Equals("F"))
                isUseClearDataTime = false;
            else if (isUse.Equals("T"))
                isUseClearDataTime = true;

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
                            clearGridViewData1();
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
                            clearGridViewData1();
                        }));
                    }
                }
            }

            ////图片压缩
            //DateTime nowTime = DateTime.Now;
            //if (nowTime.Hour == 4 && nowTime.Minute == 0 && nowTime.Second == 0) //上午4点
            //{
            //    try
            //    {
            //        // 获取当前工作目录
            //        string currentDirectory = Directory.GetCurrentDirectory();
            //        // 构建相对路径
            //        string relativePath = Path.Combine(currentDirectory, "PictruePress/pictruePress.exe");
            //        // 解析相对路径为绝对路径
            //        string absolutePath = Path.GetFullPath(relativePath);

            //        // 创建Process对象
            //        using (Process process = new Process())
            //        {
            //            // 设置进程信息
            //            process.StartInfo.FileName = absolutePath;

            //            // 启动进程，不等待执行完毕
            //            process.Start();

            //            if (tbLog.IsHandleCreated)
            //                tbLog.BeginInvoke(GetMyDelegate, "开始图片压缩...");
            //        } 
            //    }
            //    catch (Exception ex)
            //    {
            //        if (tbLog.IsHandleCreated)
            //            tbLog.BeginInvoke(GetMyDelegate, ex.Message);
            //    }
            //}
        }
        void writeLogBeforeClear()
        {
            LogHelper.WriteLog($"已清空历史数据,总数:{lbAllNum.Text};OK数:{lbOKNum.Text};NG数:{lbNGNum.Text};良率:{lbOKPercent.Text}");
            IniHelper.SaveSetIni.Write("单相机生产信息", "生产总数", "0");
            IniHelper.SaveSetIni.Write("单相机生产信息", "OK总数", "0");
            IniHelper.SaveSetIni.Write("单相机生产信息", "NG总数", "0");
        }
        void clearTotalNum()
        {
            lbAllNum.Text = "0";
            AllNum = 0;
            lbOKNum.Text = "0";
            OKNum = 0;
            lbNGNum.Text = "0";
            NGNum = 0;
            lbOKPercent.Text = "0";
            lbCt.Text = "0";
            lbResult.BackColor = System.Drawing.Color.Green;
            lbResult.Text = "OK";
        }

        private void clearGridViewData1()
        {
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                object cellObj = row.Cells[0].Value;
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

        #region 异步保存图片
        private async void saveImage(string okStatus, string ngStatus, string result, List<string> NGItems)
        {
            await Task.Run(() =>
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
                    imageName = panasonicMewtocol.ReadString(address, ushort.Parse(lenghth)).Content;
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
                    string path = savePath + "\\" + nowDataTime + "\\" + "OK" + "\\";
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);
                    string imagePath = HalconHelper.fileNameIsExist(path, imageName);
                    HalconHelper.saveImage(hWindowControl1.HalconWindow, ho_image1, imagePath);
                }
                else if (ngStatus == "T" && result == "NG")
                {
                    foreach (var item in NGItems)
                    {
                        string path = savePath + "\\" + nowDataTime + "\\" + $"{item}" + "\\";
                        if (!Directory.Exists(path))
                            Directory.CreateDirectory(path);
                        string imagePath = HalconHelper.fileNameIsExist(path, imageName);
                        HalconHelper.saveImage(hWindowControl1.HalconWindow, ho_image1, imagePath);
                    }
                }
            });
        }
        #endregion

        #region 结果显示部分初始化
        private void imformationShowLoad()
        {
            if (!String.IsNullOrEmpty(IniHelper.SaveSetIni.Read("单相机生产信息", "生产总数")))
                AllNum = Convert.ToInt32(IniHelper.SaveSetIni.Read("单相机生产信息", "生产总数"));
            if (!String.IsNullOrEmpty(IniHelper.SaveSetIni.Read("单相机生产信息", "OK总数")))
                OKNum = Convert.ToInt32(IniHelper.SaveSetIni.Read("单相机生产信息", "OK总数"));
            if (!String.IsNullOrEmpty(IniHelper.SaveSetIni.Read("单相机生产信息", "NG总数")))
                NGNum = Convert.ToInt32(IniHelper.SaveSetIni.Read("单相机生产信息", "NG总数"));
            lbAllNum.Text = AllNum.ToString();
            lbOKNum.Text = OKNum.ToString();
            lbNGNum.Text = NGNum.ToString();
            if (AllNum != 0)
                lbOKPercent.Text = (OKNum / AllNum * 100).ToString("F2") + "%";
            lbResult.Text = "NG";
            lbResult.BackColor = System.Drawing.Color.Red;
            if (frmMain.m_bPause && !frmMain.m_bResume)
                btHandTest.Enabled = false;
            else
                btHandTest.Enabled = true;
        }
        #endregion

        #region 显示结果
        private void showResult()
        {
            this.BeginInvoke(new Action(() =>
            {
                lbAllNum.Text = AllNum.ToString();
                lbOKNum.Text = OKNum.ToString();
                lbNGNum.Text = NGNum.ToString();
                if (AllNum != 0)
                    lbOKPercent.Text = (OKNum / AllNum * 100).ToString("F2") + "%";
                else
                    lbOKPercent.Text = "0.00%";
                lbCt.Text = ct.ToString();
                if (result == "OK")
                {
                    lbResult.Text = "OK";
                    lbResult.BackColor = System.Drawing.Color.Green;
                }
                else
                {
                    lbResult.Text = "NG";
                    lbResult.BackColor = System.Drawing.Color.Red;
                }
            }));
        }
        #endregion

        #region 手动测试
        private void btHandTest_Click(object sender, EventArgs e)
        {
            if (trigerDLuseImage)
            {
                string[] searchPatterns = { "*.jpg", "*.bmp","*.png" };
                foreach (string pattern in searchPatterns)
                {
                    string[] files = Directory.GetFiles(director_left, pattern);
                    if (files == null)
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

                if (!(window_left_pictruePath_isReady))
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

                // 触发线程执行
                event1.Set();

                // 等待线程完成
                countdown.Wait();

                // 重置事件和倒计时器
                countdown.Reset();
            }
            else
            {
                isImage = true; 
            }
        }
        #endregion

        #region 清除数据
        private void btClearData_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("确认是否清除?", "警告", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
            {
                countClearanceSetting(false);
            }
        }
        #endregion

        #region 加载深度学习模型

        //private async void btLoadConfig_Click(object sender, EventArgs e)
        //{
        //    if (!loadSet())
        //        return;
        //    string configpath = IniHelper.SaveSetIni.Read("深度学习文件", "配置文件路径");
        //    treeFileHelper.filePath = configPath;
        //    //btLoadConfig.Enabled = false;
        //    CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        //    CancellationToken token = cancellationTokenSource.Token;
        //    await Task.Run(() =>
        //    {
        //        this.BeginInvoke(new Action(() => { addRows(treeFileHelper, list_OKType_classId_left); }));
        //        for (int i = 0; i < 100; i++)
        //        {
        //            tbLog.BeginInvoke(GetMyDelegate, "当前切换到调试模式，请设置窗口的图片目录");
        //            this.BeginInvoke(new Action(() => { progressBar1.Value = i; }));
        //            if (i == 40)
        //            {
        //                Dictionary<int, NodeInfo> dic_nodeIdWithNodeInfo = treeFileHelper.dic_nodeIdWithNodeInfo;
        //                if (!HTCSharpDemo.Program.loadDeepStudyHandle(configpath, ref studyHandle, ref dic_nodeIdWithNodeInfo))
        //                {
        //                    cancellationTokenSource.Cancel();
        //                    this.BeginInvoke(new Action(() =>
        //                    {
        //                        progressBar1.Value = 0;
        //                        btLoadConfig.Enabled = true;
        //                        btLoadConfig.Text = "加载方案";

        //                    }));
        //                    MessageBox.Show("方案加载失败");
        //                    break;
        //                }
        //                else
        //                {
        //                    this.BeginInvoke(new Action(() =>
        //                    {
        //                        btLoadConfig.Enabled = false;
        //                        btLoadConfig.Text = "加载完毕";
        //                        startCom();
        //                    }));
        //                }
        //            }
        //        }

        //    }, token);
        //}
        #endregion

        #region 更新结果列表
        private void referenceTable(Dictionary<int, List<result>> res1)
        {
            this.BeginInvoke(new Action(() =>
            {
                string configPath = IniHelper.SaveSetIni.Read("深度学习文件", "配置文件路径");
                var originalCellStyle = dataGridView1.Rows[0].Cells[0].Style;
                foreach (DataGridViewRow row in dataGridView1.Rows)
                    row.Cells[3].Style = originalCellStyle;
                try
                {
                    foreach (DataGridViewRow row in this.dataGridView1.Rows)
                    {
                        if (dataGridView1.Rows.Count - 1 > row.Index)
                        {
                            bool isAdd = false;
                            double ngdingweiNum = 0;
                            if (row.Cells[0].Value != null && !String.IsNullOrWhiteSpace(row.Cells[0].Value.ToString()) && row.Cells[0].Value != DBNull.Value)
                            {
                                if ((string)row.Cells[1].Value == "定位失败")
                                {
                                    isAdd = true;
                                    if (row.Cells[2].Value != null && !String.IsNullOrWhiteSpace(row.Cells[2].Value.ToString()) && row.Cells[2].Value != DBNull.Value && Regex.IsMatch(row.Cells[2].Value.ToString(), @"^\d+(\.\d+)?$"))
                                        ngdingweiNum = Convert.ToInt32(row.Cells[2].Value);
                                    if (res1[treeFileHelper.dingweiNodeId].Count == 1)
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

                            row.Cells[4].Value = AllNum;
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
                            if (AllNum == 0)
                            {
                                row.Cells[0].Value = "0.00%";
                            }
                            else
                                row.Cells[5].Value = (Convert.ToDouble(row.Cells[2].Value) / AllNum * 100).ToString("F2") + "%"; //百分比
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog(ex.ToString());
                }
            }));
        }
        #endregion

        #region 判定OK还是NG
        string dingweiSignal;
        string ngSignal;
        private async void tellPLCResult(string jiance_result_class_id)
        {
            await Task.Run(() =>
            {
                try
                {
                    //打开了才发送信号
                    if (!panasonicMewtocol.IsOpen())
                        return;
                    switch (result)
                    {
                        case "OK":
                            {
                                do
                                {
                                    panasonicMewtocol.Write(tbOKSignal.Text, true);
                                    Thread.Sleep(40);
                                } while (!panasonicMewtocol.ReadBool(tbOKSignal.Text).Content);
                                tbLog.BeginInvoke(GetMyDelegate, $"{tbOKSignal.Text}信号次发送成功", null);
                                do
                                {
                                    panasonicMewtocol.Write(tbOKSignal.Text, false);
                                    Thread.Sleep(10);
                                } while (panasonicMewtocol.ReadBool(tbOKSignal.Text).Content);
                                tbLog.BeginInvoke(GetMyDelegate, $"{tbOKSignal.Text}信号次断开成功", null);
                                break;
                            }
                        case "NG":
                            {
                                NGTypePara.NGType ngTypes = new NGTypePara.NGType();
                                string jsonFromFile = File.ReadAllText(rootDirectory);
                                ngTypes = JsonConvert.DeserializeObject<NGType>(jsonFromFile);

                                foreach (var item1 in ngTypes.NGTypeConfigs)
                                {
                                    if (item1.NGType.Equals(jiance_result_class_id))
                                    {
                                        ngSignal = ngdic[item1.OutType];
                                        break;
                                    }
                                }
                                do
                                {
                                    panasonicMewtocol.Write(ngSignal, true);
                                    Thread.Sleep(40);
                                } while (!panasonicMewtocol.ReadBool(ngSignal).Content);
                                tbLog.BeginInvoke(GetMyDelegate, $"{ngSignal}信号次发送成功", null);
                                do
                                {
                                    panasonicMewtocol.Write(ngSignal, false);
                                    Thread.Sleep(10);
                                } while (panasonicMewtocol.ReadBool(ngSignal).Content);
                                tbLog.BeginInvoke(GetMyDelegate, $"{ngSignal}信号次断开成功", null);
                                break;
                            }
                        case "定位失败":
                            {
                                NGTypePara.NGType ngTypes = new NGTypePara.NGType();
                                string jsonFromFile = File.ReadAllText(rootDirectory);
                                ngTypes = JsonConvert.DeserializeObject<NGType>(jsonFromFile);

                                foreach (var item1 in ngTypes.NGTypeConfigs)
                                {
                                    if (item1.NGType.Equals(jiance_result_class_id))
                                    {
                                        dingweiSignal = ngdic[item1.OutType];
                                        break;
                                    }
                                }
                                do
                                {
                                    panasonicMewtocol.Write(dingweiSignal, true);
                                    Thread.Sleep(40);
                                } while (!panasonicMewtocol.ReadBool(dingweiSignal).Content);
                                tbLog.BeginInvoke(GetMyDelegate, $"{dingweiSignal}信号次发送成功", null);
                                do
                                {
                                    panasonicMewtocol.Write(dingweiSignal, false);
                                    Thread.Sleep(10);
                                } while (panasonicMewtocol.ReadBool(dingweiSignal).Content);
                                tbLog.BeginInvoke(GetMyDelegate, $"{dingweiSignal}信号次断开成功", null);
                                break;
                            }
                        default:
                            break;
                    }
                }
                catch(Exception ex)
                {
                    tbLog.BeginInvoke(GetMyDelegate, "PLC发送信号失败" + ex.Message, ex);
                } 
            });
        }
        #endregion

        private bool loadDlModel(bool modlePath1isNull)
        {
            if (!modlePath1isNull)
            {
                Dictionary<int, NodeInfo> dic_nodeIdWithNodeInfo = treeFileHelper.dic_nodeIdWithNodeInfo;
                HTCSharpDemo.Program.loadDeepStudyHandle(configPath, ref studyHandle, ref dic_nodeIdWithNodeInfo);
                test_num = dic_nodeIdWithNodeInfo.Count;
            }
            else
                return false;
            loadSet();
            return true;
        }

        #region 保存深度学习路径后按照新路径重写输出信号配置文件
        private void loadNewFile(List<string> paths)
        {
            if (paths.Count == 0)
                return;
            //更新ngdic
            addNGDic();
            //取第一个path
            string path = paths[0];
            string rootDirectory1 = AppDomain.CurrentDomain.BaseDirectory + "\\" + "NGTypeSignalSet1.txt";

            NGTypePara.NGType ngTypes = new NGTypePara.NGType();
            treeFileHelper.filePath = path;
            configPath = path;
            Dictionary<int, NodeInfo> dic_nodeIdWithNodeInfo = treeFileHelper.dic_nodeIdWithNodeInfo;
            foreach (var item1 in dic_nodeIdWithNodeInfo)
            {
                if (item1.Value.ParentsNodeId > -1 && item1.Value.NodeType == 1)
                    continue;
                foreach (var item2 in item1.Value.ClassNames)
                {
                    //以其他开头默认true,对结果不影响
                    if (item2.StartsWith("其他") || item2.Equals("OK") || item2.Equals("Ok") 
                        || item2.Equals("ok"))
                    {
                        NGTypePara.NGTypeConfig nGTypePara = new NGTypePara.NGTypeConfig();
                        nGTypePara.Node = item1.Value.NodeName;
                        nGTypePara.NGType = item2;
                        if (ngdic.Count > 0)
                            nGTypePara.OutType = ngdic.Keys.First().ToString();
                        nGTypePara.isOK = true;
                        ngTypes.NGTypeConfigs.Add(nGTypePara);
                    }
                    else
                    {
                        NGTypePara.NGTypeConfig nGTypePara = new NGTypePara.NGTypeConfig();
                        nGTypePara.Node = item1.Value.NodeName;
                        nGTypePara.NGType = item2;
                        if (ngdic.Count > 0)
                            nGTypePara.OutType = ngdic.Keys.First().ToString();
                        nGTypePara.isOK = false;
                        ngTypes.NGTypeConfigs.Add(nGTypePara);
                    }
                }
            }
            string json = JsonConvert.SerializeObject(ngTypes, Formatting.Indented);
            File.WriteAllText(rootDirectory1, json);

            //更新dataview

        }
        #endregion

        #region 渲染图片
        private async void showNGLocation(Dictionary<int, List<result>> res1)
        {
            await Task.Run(() =>
            {
                try
                {
                    if (result == "OK")
                    {
                        if (treeFileHelper.dingweiNodeId != int.MinValue) //有定位
                        {
                            HTCSharpDemo.Program.result res2 = res1[treeFileHelper.dingweiNodeId][0];
                            HalconHelper.showRoi(hWindowControl1.HalconWindow, "green", res2.y, res2.x, res2.y + res2.height, res2.x + res2.width);
                        }
                        HalconHelper.showResultString(hWindowControl1.HalconWindow, 0, 0, 72, "green", "OK");
                    }
                    else
                    {
                        if (treeFileHelper.dingweiNodeId != int.MinValue)//需要定位
                        {
                            if (res1[treeFileHelper.dingweiNodeId].Count == 1) //定位到了
                            {
                                {
                                    HTCSharpDemo.Program.result res2 = res1[treeFileHelper.dingweiNodeId][0];
                                    HalconHelper.showRoi(hWindowControl1.HalconWindow, "red", res2.y, res2.x, res2.y + res2.height, res2.x + res2.width);
                                }
                            }
                            else //定位失败
                            {
                                // 什么也不做
                            }
                        }
                        else //不需要定位
                        {

                        }
                        HalconHelper.showResultString(hWindowControl1.HalconWindow, 0, 0, 72, "red", result);
                    }

                    if (res1.ContainsKey(treeFileHelper.quexianNodeId))
                    {
                        foreach (var item in res1[treeFileHelper.quexianNodeId])
                        {
                            HalconHelper.showRoi(hWindowControl1.HalconWindow, "red", item.y, item.x, item.y + item.height, item.x + item.width);
                        }
                    }
                    foreach (var item in res1)
                    {
                        if (treeFileHelper.listDingweiId.Contains(item.Key))
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
                    tbLog.BeginInvoke(GetMyDelegate, "渲染图像失败"+ex.Message, ex);
                }
                
            });
        }
        #endregion

        #region 加载深度学习配置文件到表格
        private void addRows(TreeFileHelper treeFileHelper, List<string> l_OKTypes)
        {
            foreach (var item in treeFileHelper.dic_nodeIdWithNodeInfo)
            {
                if (item.Value.NodeType == 1)
                {
                    if (item.Value.ParentsNodeId == -1 && !l_OKTypes.Contains("1"))
                        dataGridView1.Rows.Add(item.Value.NodeName, "定位失败", "0", "--", "0", "0", "--", "--");
                }
                else
                {
                    foreach (var item1 in item.Value.ClassNames)
                    {
                        if (!item1.Equals("OK", StringComparison.OrdinalIgnoreCase) && !item1.StartsWith("其他") && !l_OKTypes.Contains(item1))
                            dataGridView1.Rows.Add(item.Value.NodeName, item1, "0", "--", "0", "0", "--", "--");
                    }
                }
            }
        }
        #endregion

        #region 添加表头
        private void createHead()
        {
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.Enabled = false;
            DataGridViewColumnCollection columns = dataGridView1.Columns;
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

        #region 检查磁盘剩余空间
        private bool isFreeSpace(string path)
        {
            if (string.IsNullOrEmpty(path))
                return true;
            DriveInfo drive = new DriveInfo(System.IO.Path.GetPathRoot(path));
            if (drive.IsReady)
            {
                long freeSpace = drive.TotalFreeSpace;
                double freeSpaceInMB = freeSpace / (1024 * 1024);
                if (freeSpaceInMB < 6)
                    return false;
            }
            return true;
        }
        #endregion

        // 更新isOK事件 刷新
        private void updateSetClassIsOk(string node_name,string class_id, bool isCheck)
        {
            if (isCheck)
            {
                if(!list_OKType_classId_left.Contains(class_id))
                    list_OKType_classId_left.Add(class_id);
                int index = int.MinValue;
                string tmp_class_id;
                if (class_id.Equals("1"))
                    tmp_class_id = "定位失败";
                else
                {
                    tmp_class_id = class_id;
                }
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (row.Cells[1].Value != null && row.Cells[1].Value.Equals(tmp_class_id))
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
                    if (!dataViewChangeModel_left.ContainsKey(tmp_class_id))
                    {
                        dataViewChangeModel model = new dataViewChangeModel();
                        model.index = index;
                        model.row = dataGridView1.Rows[index];
                        dataViewChangeModel_left.Add(tmp_class_id, model);
                    }

                    dataGridView1.Rows.RemoveAt(index);
                    dataGridView1.Refresh();
                }
            }
            else //从okType中移除
            {
                if (list_OKType_classId_left.Contains(class_id))
                {
                    list_OKType_classId_left.Remove(class_id);
                    string tmp_class_id;
                    if (class_id.Equals("1"))
                        tmp_class_id = "定位失败";
                    else
                    {
                        tmp_class_id = class_id;
                    }
                    if (dataViewChangeModel_left != null && dataViewChangeModel_left.ContainsKey(tmp_class_id))
                    {
                        dataViewChangeModel model = dataViewChangeModel_left[tmp_class_id];
                        // 将行插入指定位置
                        dataGridView1.Rows.Insert(0, model.row); // 在索引为 0 的位置插入行
                        dataGridView1.Refresh();
                    }
                    else if (dataViewChangeModel_left == null || !dataViewChangeModel_left.ContainsKey(tmp_class_id))
                    {
                        //默认不显示情况，默认没有这个row
                        DataGridViewRow row = new DataGridViewRow();
                        for (int i = 0; i < 8; i++)
                        {
                            DataGridViewTextBoxCell cell = new DataGridViewTextBoxCell();
                            row.Cells.Add(cell);
                        }
                        row.Cells[0].Value = node_name;
                        row.Cells[1].Value = tmp_class_id;
                        row.Cells[2].Value = "0";
                        row.Cells[3].Value = "--";
                        row.Cells[4].Value = "0";
                        row.Cells[5].Value = "0";
                        row.Cells[6].Value = "--";
                        row.Cells[7].Value = "--";

                        dataGridView1.Rows.Insert(0, row);
                        dataGridView1.Refresh();
                    }
                }
            }
        }
        private void frmOneCamera_FormClosing(object sender, FormClosingEventArgs e)
        {
            isonRunning = false;
            event1.Set();
            Thread.Sleep(100);
            if (listenSerialPortThread != null)
                listenSerialPortThread.Join();
            if (grabOneCameraThread != null)
                grabOneCameraThread.Join();
        }

        private void 读取图片目录ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();

            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                director_left = folderBrowserDialog.SelectedPath;
                string[] searchPatterns = { "*.jpg", "*.bmp","*.png" };
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

        private void my_MouseWheel(object sender, MouseEventArgs e)
        {
            //HSmartWindowControl控件的区域
            Rectangle rect = hWindowControl1.RectangleToScreen(hWindowControl1.ClientRectangle);
            //滚动时，鼠标悬停在在HSmartWindowControl控件上
            if (rect.Contains(Cursor.Position))
            {
                //缩放
                hWindowControl1.HSmartWindowControl_MouseWheel(sender, e);
            }
        }
    }
}
