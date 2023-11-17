using HalconDotNet;
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
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using log4net.DateFormatter;

namespace YUNTIANVision
{
    public delegate void showXYorGrayDelegate3(string xy,string gray);
    public partial class frmTwoCamera_copy : Form
    {
        public frmTwoCamera_copy()
        {
            frmMain.closeAll += new closeDelegate(frmTwoClose);
            frmMain.controlEvent += new controlDelegate(runControl);
            frmNGSignalSet.loadNGEvent += new loadNGSignalDic(addNGDic1);
            frmNGSignalSet.loadNGEvent += new loadNGSignalDic(addNGDic2);
            frmNGSignalSet.loadNGEvent += new loadNGSignalDic(addNGorOK1);
            frmNGSignalSet.loadNGEvent += new loadNGSignalDic(addNGorOK2);
            frmCameraExposeSet.cameraExposeSetEvent += new cameraExposeSet(exposeSet);
            frmCameraExposeSet.cameraExposeCloseEvent += new cameraExposeClose(exposeClose);
            frmSet.newConfigEvent += new newConfigDel(loadNewFile);
            InitializeComponent();
        }
        private static readonly object obj = new object();
        // 是否关闭程序
        private volatile static bool isRunning = true;
        #region 定义对象
        Thread listeningSerilPortThread = null;
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
        frmNGSignalSet set2;
        /// <summary>
        /// 深度学习句柄
        /// </summary>
        IntPtr studyHandle;
        /// <summary>
        /// 节点数量
        /// </summary>
        int test_num;
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
        public static event showXYorGrayDelegate3 showXYorGrayEvent;
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
        /// 串口2的对象
        /// </summary>
        PanasonicMewtocol panasonicMewtocol2 = new PanasonicMewtocol();
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
        /// <summary>
        /// 相机1线程开启标志
        /// </summary>
        bool m_bRunThread1 = false;
        /// <summary>
        /// 相机2线程开启标志
        /// </summary>
        bool m_bRunThread2 = false;
        bool m_bReady = false;
        //bool m_bGrabImage = false;
        AutoResetEvent auto = new AutoResetEvent(false);
        string configPath = IniHelper.SaveSetIni.Read("深度学习文件", "配置文件路径");
        string xy = null;
        string gray = null;
        #endregion


       // 相机1变量
        bool m_bTwostart1 = false;

        List<CameraBase> camraList = new List<CameraBase>();

        bool isSet1 = false;
        bool isSet2 = false;

        // 装载窗口
        List<HWindowControl> windowControls = new List<HWindowControl>();

        // 相机2参数
        bool m_bTwostart2 = false;

        public AutoResetEvent[] autoResetEvents = null;

        /// <summary>
        /// 打开所有相机
        /// </summary>
        private Task openCameraList() //private async void openCameraList()
        {
            return new Task(() => {
                try
                {
                    MyCamera.MV_CC_DEVICE_INFO_LIST stDevList = new MyCamera.MV_CC_DEVICE_INFO_LIST();
                    int nRet = MyCamera.MV_CC_EnumDevices_NET(MyCamera.MV_GIGE_DEVICE | MyCamera.MV_USB_DEVICE, ref stDevList);
                    for (int i = 0; i < stDevList.nDeviceNum; i++)
                    {
                        CameraBase camerabase = new CameraBase();

                        camerabase.stDevInfo = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(stDevList.pDeviceInfo[i], typeof(MyCamera.MV_CC_DEVICE_INFO));
                        nRet = camerabase.cam_device.MV_CC_CreateDevice_NET(ref camerabase.stDevInfo);
                        if (MyCamera.MV_OK != nRet)
                        {
                            tbLog1.Invoke(GetMyDelegateLog, tbLog1, "相机打开失败...");
                        }
                        // ch:打开设备 | en:Open device
                        nRet = camerabase.cam_device.MV_CC_OpenDevice_NET();
                        if (MyCamera.MV_OK != nRet)
                        {
                            tbLog2.Invoke(GetMyDelegateLog, tbLog2, "相机打开失败...");
                        }
                        // ch:开启抓图 | en:start grab
                        nRet = camerabase.cam_device.MV_CC_StartGrabbing_NET();
                        if (MyCamera.MV_OK != nRet)
                        {
                            tbLog2.Invoke(GetMyDelegateLog, tbLog2, "相机取图失败...");
                        }

                        // ch:获取包大小 || en: Get Payload Size
                        camerabase.stParam = new MyCamera.MVCC_INTVALUE();
                        nRet = camerabase.cam_device.MV_CC_GetIntValue_NET("PayloadSize", ref camerabase.stParam);
                        if (MyCamera.MV_OK == nRet)
                        {
                            if(i == 0)
                            {
                                m_bTwostart1 = true;
                                tbLog1.Invoke(GetMyDelegateLog, tbLog1, "相机1初始化完成...");
                            }else if (i == 1)
                            {
                                m_bTwostart2 = true;
                                tbLog2.Invoke(GetMyDelegateLog, tbLog2, "相机2初始化完成...");
                            }
                            
                        }
                        else tbLog2.BeginInvoke(GetMyDelegateLog, tbLog1, "相机初始化失败...");
                        camerabase.nPayloadSize = camerabase.stParam.nCurValue;
                        camerabase.pBufForDriver = Marshal.AllocHGlobal((int)camerabase.nPayloadSize);
                        camerabase.FrameInfo = new MyCamera.MV_FRAME_OUT_INFO_EX();

                        camraList.Add(camerabase);
                    }
                    autoResetEvents[0].Set();
                }
                catch (Exception ex)
                {
                    tbLog2.Invoke(GetMyDelegateLog, tbLog2, "相机初始化失败...");
                    MessageBox.Show("相机打开失败");
                    LogHelper.WriteLog(ex.ToString());
                }
            });
        }

        /// <summary>
        /// 关闭所有相机
        /// </summary>
        private void closeCameraList()
        {
            try
            {
                for(int i = 0; i < camraList.Count(); i++)
                {
                    CameraBase camera = camraList[i];
                    Marshal.FreeHGlobal(camera.pBufForDriver);

                    // ch:停止抓图 | en:Stop grab image
                    int nRet = camera.cam_device.MV_CC_StopGrabbing_NET();
                    if (MyCamera.MV_OK != nRet)
                    {
                        LogHelper.WriteLog("关闭设备失败" + camera.stDevInfo);
                    }

                    // ch:关闭设备 | en:Close device
                    nRet = camera.cam_device.MV_CC_CloseDevice_NET();
                    if (MyCamera.MV_OK != nRet)
                    {
                        LogHelper.WriteLog("关闭设备失败"+camera.stDevInfo);
                    }

                    // ch:销毁设备 | en:Destroy device
                    nRet = camera.cam_device.MV_CC_DestroyDevice_NET();
                    if (MyCamera.MV_OK == nRet)
                    {
                        //TODO:线程开始
                        m_bTwostart1 = false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                LogHelper.WriteLog(ex.ToString());
            }
        }

        /// <summary>
        /// 相机采集图像
        /// </summary>
        /// <param name="cameraBase"></param>
        /// <param name="image"></param>
        private void grabHoImage(ref CameraBase cameraBase, out HObject image)
        {
            HOperatorSet.GenEmptyObj(out image);
            try
            {
                int nRet = cameraBase.cam_device.MV_CC_GetOneFrameTimeout_NET(cameraBase.pBufForDriver, cameraBase.nPayloadSize, ref cameraBase.FrameInfo, 1000);
                image.Dispose();
                if (IniHelper.SaveSetIni.Read("相机设置", "采图设置") == "黑白" || String.IsNullOrEmpty(IniHelper.SaveSetIni.Read("相机设置", "采图设置")))
                    HOperatorSet.GenImage1Extern(out image, "byte", cameraBase.FrameInfo.nWidth, cameraBase.FrameInfo.nHeight, cameraBase.pBufForDriver, 0);
                else if (IniHelper.SaveSetIni.Read("相机设置", "采图设置") == "彩色")
                    HOperatorSet.GenImageInterleaved(out image, cameraBase.pBufForDriver, "rgb", cameraBase.FrameInfo.nWidth, cameraBase.FrameInfo.nHeight,
                    0, "byte", cameraBase.FrameInfo.nWidth, cameraBase.FrameInfo.nHeight, 0, 0, -1, 0);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.ToString());
            }
        }

        /// <summary>
        ///  好像是显示图像用
        /// </summary>
        /// <param name="cameraBase"></param>
        /// <param name="image"></param>
        private void ImageToHalcon(ref CameraBase cameraBase, HObject image)
        {
            try
            {
                cameraBase.hv_Height.Dispose(); 
                cameraBase.hv_Width.Dispose();
                HOperatorSet.GetImageSize(image, out cameraBase.hv_Width, out cameraBase.hv_Height);
                HalconHelper.imageLocation(cameraBase.hv_Width, cameraBase.hv_Height, cameraBase.showWindow.Width, cameraBase.showWindow.Height, out cameraBase.row_left, out cameraBase.column_left, out cameraBase.row_right, out cameraBase.column_right);
                HOperatorSet.SetPart(cameraBase.showWindow.HalconWindow, cameraBase.row_left, cameraBase.column_left, cameraBase.row_right, cameraBase.column_right);
                HOperatorSet.DispObj(image, cameraBase.showWindow.HalconWindow);    
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.ToString());
            }
        } 

        #region load/close事件
        
        private void frmTwoCamera_Load(object sender, EventArgs e)
        {
            InitializeComponent();
            isRunning = true;
            createHead1();
            createHead2();
            dataGridView3.RowHeadersVisible = false;
            dataGridView4.RowHeadersVisible = false;

            set1 = new frmNGSignalSet("NG2");
            addNGDic1();
            addNGorOK1();
            set2 = new frmNGSignalSet("NG3");
            addNGDic2();
            addNGorOK2();
            loadOneCameraSet();
            loadTwoCameraSet();
            loadOnePort();
            loadCodeSet();
            loadResult();

            autoResetEvents = new AutoResetEvent[]
            {
                new AutoResetEvent(false),
                new AutoResetEvent(false)
            };
            //装载窗体
            windowControls.Add(hWindowControl1);
            windowControls.Add(hWindowControl2);
            
            // 异步打开多相机
            Task openCams = openCameraList();
            openCams.Start();
            GetMyDelegateLog = new myDelegateLog(showLog);
            GetMyDelegatePlc = new myDelegatePlc(showPlcStatus);            
            //Thread.Sleep(1000);
            Task loadDLmodle = btLoadConfig_Click(sender, e);

            listeningSerilPortThread = new Thread(listenSerilPortThread);
            listeningSerilPortThread.IsBackground = true;
            listeningSerilPortThread.Start();

            
        }

        private void frmTwoClose()
        {
                this.Close();
        }

        private void frmTwoCamera_FormClosed(object sender, FormClosedEventArgs e)
        {
           // 去掉了 
        }
        #endregion

        #region 初始化读码设置
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
        #endregion

        #region 委托显示相机日志
        private void showLog(TextBox textBox, string log)
        {
            textBox.AppendText(DateTime.Now.ToString("yyyy/MM/dd-HH:mm:ss:fff") + ":" + log + "\r\n");
            LogHelper.WriteLog(log);
            textBox.SelectionStart = tbLog1.TextLength;
            textBox.ScrollToCaret();
            textBox.SelectionStart = tbLog2.TextLength;
            textBox.ScrollToCaret();
        }
        #endregion

        #region 委托显示通讯状态

        private void showPlcStatus(Label label, string status, System.Drawing.Color color)
        {
            label.BackColor = color;
            label.Text = status;
        }

        #endregion

        #region 初始化相机1设置
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
        #endregion

        #region 初始化相机2设置
        private void loadTwoCameraSet()
        {
            panasonicMewtocol2 = new PanasonicMewtocol();
            tbOKSignal2.Text = IniHelper.SaveSetIni.Read("相机2PLC设置", "OK信号");
            tbReadySignal2.Text = IniHelper.SaveSetIni.Read("相机2PLC设置", "准备信号");
            tbGrabImageSignal2.Text = IniHelper.SaveSetIni.Read("相机2PLC设置", "拍照信号");
            for (int i = 0; i < SerialPort.GetPortNames().Length; i++)
            {
                cbPortName2.Items.Add(SerialPort.GetPortNames()[i]);
            }
            for (int i = 0; i < SerialPort.GetPortNames().Length; i++)
            {
                if (!String.IsNullOrEmpty(IniHelper.SaveSetIni.Read("串口2设置", "串口号")))
                {
                    if (SerialPort.GetPortNames()[i] == IniHelper.SaveSetIni.Read("串口2设置", "串口号"))
                        cbPortName2.SelectedIndex = i;
                }
                else
                    cbPortName2.SelectedIndex = 0;
            }
            tbBaudRate2.Text = IniHelper.SaveSetIni.Read("串口2设置", "波特率");
            tbDateBits2.Text = IniHelper.SaveSetIni.Read("串口2设置", "数据位");
            tbStopBits2.Text = IniHelper.SaveSetIni.Read("串口2设置", "停止位");
            cbParity2.Items.Add("无");
            cbParity2.Items.Add("奇校验");
            cbParity2.Items.Add("偶校验");
            cbParity2.SelectedIndex = 0;
            for (int i = 0; i < cbParity2.Items.Count; i++)
            {
                if (!String.IsNullOrEmpty(IniHelper.SaveSetIni.Read("串口2设置", "校验位")))
                {
                    if (cbParity2.Items[i].ToString() == IniHelper.SaveSetIni.Read("串口2设置", "校验位"))
                        cbParity2.SelectedIndex = i;
                }
                else
                    cbParity2.SelectedIndex = 0;
            }
            string enable = IniHelper.SaveSetIni.Read("串口2设置", "是否启用");
            if (!String.IsNullOrEmpty(enable))
            {
                if (enable == "T")
                    cbEnableCom2.Checked = true;
                else
                {
                    cbEnableCom2.Checked = false;
                    controlCom2(false);
                    controlSignal2(false);
                }
            }
            else
            {
                cbEnableCom2.Checked = false;
                controlCom2(false);
            }
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
        /// 相机1收到拍照信号，开始处理
        /// </summary>
        void processCamera1()
        {
            CameraBase cameraBase = null;
            if (camraList.Count > 0)
            {
                cameraBase = camraList[0];
            }
            tbLog1.Invoke(GetMyDelegateLog, tbLog1, "相机1收到拍照信号...");
            DateTime startTime;
            DateTime endTime;
            startTime = DateTime.Now;
            tbLog1.Invoke(GetMyDelegateLog, tbLog1, "相机1开始拍照...");
            grabHoImage(ref cameraBase,out cameraBase.ho_image);
            ImageToHalcon(ref cameraBase, cameraBase.ho_image);
            tbLog1.Invoke(GetMyDelegateLog, tbLog1, "相机1拍照结束...");

            HTCSharpDemo.Program.ImageHt imageHt = new HTCSharpDemo.Program.ImageHt();
            imageHt.data = cameraBase.pBufForDriver;
            imageHt.width = cameraBase.hv_Width;
            imageHt.height = cameraBase.hv_Height;
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
            HTCSharpDemo.Program dl = new HTCSharpDemo.Program();
            dl.DeepStudy1(configPath, ptr1, imageHt, test_num);
            //HTCSharpDemo.Program.DeepStudy1(configPath, ptr1, imageHt, test_num);

            tbLog1.Invoke(GetMyDelegateLog, tbLog1, "深度学习处理结束...");
            AllNum1++;
            IniHelper.SaveSetIni.Write("相机1生产信息", "生产总数", AllNum1.ToString());
            if (dl.res1[TreeFileHelper.dingweiNodeId].Count == 1)
            {
                if (!dl.NGItems1.Intersect(NGType1.Keys).Any())
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
            ProcResult1(dl);
            showNGLocation1(dl);
            endTime = DateTime.Now;
            TimeSpan ct = endTime - startTime;
            ct1 = (int)ct.TotalMilliseconds;

            tbLog1.Invoke(GetMyDelegateLog, tbLog1, "正在输出相机1结果...");
            showResult1();
            referenceTable1(dl);
            tbLog1.Invoke(GetMyDelegateLog, tbLog1, "相机1结果输出完成...");

            string okStatus = IniHelper.SaveSetIni.Read("图片路径", "OK图片标志");
            string ngStatus = IniHelper.SaveSetIni.Read("图片路径", "NG图片标志");
            if (okStatus == "T" || ngStatus == "T")
            {
                tbLog1.Invoke(GetMyDelegateLog, tbLog1, "正在保存图片...");
                saveImage1(cameraBase,okStatus, ngStatus, result1,dl);
            }
        }

        void processCamera2()
        {
            CameraBase cameraBase = null;
            if (camraList.Count > 1)
            {
                cameraBase = camraList[1];
            }
            tbLog2.Invoke(GetMyDelegateLog, tbLog2, "相机2收到拍照信号...");
            DateTime startTime;
            DateTime endTime;
            startTime = DateTime.Now;
            tbLog2.Invoke(GetMyDelegateLog, tbLog2, "相机2开始拍照...");
            grabHoImage(ref cameraBase, out cameraBase.ho_image);
            ImageToHalcon(ref cameraBase, cameraBase.ho_image);
            tbLog2.Invoke(GetMyDelegateLog, tbLog2, "相机2拍照结束...");

            HTCSharpDemo.Program.ImageHt imageHt = new HTCSharpDemo.Program.ImageHt();
            imageHt.data = cameraBase.pBufForDriver;
            imageHt.width = cameraBase.hv_Width;
            imageHt.height = cameraBase.hv_Height;
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
            TreeFileHelper.NodeInfos nodeInfo = new TreeFileHelper.NodeInfos();
            nodeInfo.NodeInfo = TreeFileHelper.LoadTreeFile(configPath).NodeInfo;
            HTCSharpDemo.Program dl = new HTCSharpDemo.Program();
            dl.DeepStudy2(configPath, ptr2, imageHt, nodeInfo.NodeInfo.Length);
            tbLog2.Invoke(GetMyDelegateLog, tbLog2, "深度学习处理完成...");
            AllNum2++;
            IniHelper.SaveSetIni.Write("相机2生产信息", "生产总数", AllNum2.ToString());
            if (dl.res2[TreeFileHelper.dingweiNodeId].Count == 1)
            {
                if (!dl.NGItems2.Intersect(NGType2.Keys).Any())
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
            ProcResult2(dl);
            showNGLocation2(dl);
            endTime = DateTime.Now;
            TimeSpan ct = endTime - startTime;
            ct2 = (int)ct.TotalMilliseconds;

            tbLog2.Invoke(GetMyDelegateLog, tbLog2, "正在输出相机2结果...");
            showResult2();
            referenceTable2(dl);
            tbLog2.Invoke(GetMyDelegateLog, tbLog2, "相机2结果输出完成...");

            string okStatus = IniHelper.SaveSetIni.Read("图片路径", "OK图片标志");
            string ngStatus = IniHelper.SaveSetIni.Read("图片路径", "NG图片标志");
            if (okStatus == "T" || ngStatus == "T")
            {
                tbLog2.Invoke(GetMyDelegateLog, tbLog2, "正在保存图片...");
                saveImage2(cameraBase,okStatus, ngStatus, result2,dl);
            }
        }


        //if (!isFreeSpace(configPath))
        //{
        //    while (true)
        //    {
        //        if (isFreeSpace(IniHelper.SaveSetIni.Read("深度学习文件", "配置文件路径")))
        //            break;
        //    }
        //}
        

        // 具体处理深度学习数据
        private void dealWithProcesdur()
        {
            var t1 = new Task(() => processCamera1());
            var t2 = new Task(() => processCamera2()); //修改为最新
            var complexTask = Task.WhenAll(t1, t2);
            var exceptionHandler = complexTask.ContinueWith(t =>
            {

                for (int i = 0; i < camraList.Count; i++)
                {
                    CameraBase cameraBase = camraList[i];
                    HalconDotNet.HWindowControl hWindow = windowControls[i];
                    HOperatorSet.GetImageSize(cameraBase.ho_image, out cameraBase.hv_Width, out cameraBase.hv_Height);
                    HalconHelper.imageLocation(cameraBase.hv_Width, cameraBase.hv_Height, cameraBase.showWindow.Width, cameraBase.showWindow.Height, out cameraBase.row_left, out cameraBase.column_left, out cameraBase.row_right, out cameraBase.column_right);
                    if (i == 0)
                    {
                        zi1 = new ZoomImage(cameraBase.row_left, cameraBase.column_left, cameraBase.row_right, cameraBase.column_right, cameraBase.hv_Width, cameraBase.hv_Height, hWindow);
                    }
                    else if (i == 1)
                    {
                        zi2 = new ZoomImage(cameraBase.row_left, cameraBase.column_left, cameraBase.row_right, cameraBase.column_right, cameraBase.hv_Width, cameraBase.hv_Height, hWindow);
                    }

                }
                if (isImage)
                {
                    isImage = false;
                    this.Invoke(new Action(() => { btHandTest.Enabled = true; }));
                }
            },
                    TaskContinuationOptions.ExecuteSynchronously
                );
            t1.Start();
            t2.Start();
            Task.WaitAll(t1, t2);
        }
        
        //定义8点钟清理数据
        void clearDataAt8hour()
        {
            string hour = DateTime.Now.Hour.ToString(); //获取小时
            string minute = DateTime.Now.Minute.ToString(); //获取分钟
            string second = DateTime.Now.Second.ToString();//获取秒
            bool isRightHour = hour == "8" || hour == "20";
            if(isRightHour  && minute =="0" && second == "0")
            {
                btClearDate_Click(null,null);
            }
        }

        #region 串口监听线程
        private void listenSerilPortThread()
        {
            AutoResetEvent.WaitAll(autoResetEvents);
            for(int i = 0; i < camraList.Count; i++)
            {
                camraList[i].showWindow = windowControls[i];
            }
            string configPath = IniHelper.SaveSetIni.Read("深度学习文件", "配置文件路径");
            while (isRunning)
            {
                //clearDataAt8hour();
                OperateResult<bool> res = panasonicMewtocol1.ReadBool(tbGrabImageSignal1.Text);
                Thread.Sleep(10);
                dealWithProcesdur();
                //if (!res.Content && !isImage)
                //{
                //    Thread.Sleep(20);
                //}
                //else
                //{   // 判断是手动触发还是plc触发
                //    if (isImage)
                //    {
                //        lock (obj)
                //        {
                //            // 处理期间btHandTest = false
                //            this.Invoke(new Action(() => { btHandTest.Enabled = false; }));
                //            dealWithProcesdur();
                //        }
                //    }
                //    if (res.Content)
                //    {
                //        dealWithProcesdur();
                //    }    
                // }
            }
        }
        #endregion

        #region 相机1缩放
        bool m_bZoom1 = false;
        ZoomImage zi1 = null;
        private void 缩放ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (camraList.Count() > 0)
            {
                CameraBase cameraBase = camraList[0];
                if (cameraBase.ho_image != null && cameraBase.ho_image.CountObj() != 0)
                {
                    m_bZoom1 = true;
                }
            }
        }
        #endregion

        #region 相机1还原
        private void 还原ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (camraList.Count() > 0)
            {
                CameraBase cameraBase = camraList[0];
                if (cameraBase.ho_image != null && cameraBase.ho_image.CountObj() != 0)
                {
                    m_bZoom1 = true;
                    hWindowControl1.HalconWindow.ClearWindow();
                    HOperatorSet.GetImageSize(cameraBase.ho_image, out cameraBase.hv_Width, out cameraBase.hv_Height);
                    HalconHelper.imageLocation(cameraBase.hv_Width, cameraBase.hv_Height, hWindowControl1.Width, hWindowControl1.Height, out cameraBase.row_left, out cameraBase.column_left, out cameraBase.row_right, out cameraBase.column_right);
                    HOperatorSet.SetPart(hWindowControl1.HalconWindow, cameraBase.row_left, cameraBase.column_left, cameraBase.row_right, cameraBase.column_right);
                    HOperatorSet.DispObj(cameraBase.ho_image, hWindowControl1.HalconWindow);
                    zi1 = new ZoomImage(cameraBase.row_left, cameraBase.column_left, cameraBase.row_right, cameraBase.column_right, cameraBase.hv_Width, cameraBase.hv_Height, this.hWindowControl1);
                }

            }
        }
        #endregion

        #region 相机1读取图片
        private void 读取图片ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                this.openFileDialog1.Filter = "BMP图片|*.BMP|所有图片|*.*";
                if (this.openFileDialog1.ShowDialog() == DialogResult.OK && camraList.Count>0)
                {
                    CameraBase cameraBase = camraList[0];
                    int winWidth = hWindowControl1.Width;
                    int winHeight = hWindowControl1.Height;
                    hWindowControl1.HalconWindow.ClearWindow();
                    HOperatorSet.ReadImage(out cameraBase.ho_image, this.openFileDialog1.FileName);
                    cameraBase.hv_Width.Dispose(); cameraBase.hv_Height.Dispose();
                    HOperatorSet.GetImageSize(cameraBase.ho_image, out cameraBase.hv_Width, out cameraBase.hv_Height);
                    HalconHelper.imageLocation(cameraBase.hv_Width, cameraBase.hv_Height, winWidth, winHeight, out cameraBase.row_left, out cameraBase.column_left, out cameraBase.row_right, out cameraBase.column_right);
                    HOperatorSet.SetPart(this.hWindowControl1.HalconWindow, cameraBase.row_left, cameraBase.column_left, cameraBase.row_right, cameraBase.column_right);
                    HOperatorSet.DispObj(cameraBase.ho_image, this.hWindowControl1.HalconWindow);
                    zi1 = new ZoomImage(cameraBase.row_left, cameraBase.column_left, cameraBase.row_right, cameraBase.column_right, cameraBase.hv_Width, cameraBase.hv_Height, this.hWindowControl1);
                   
                    //从catch中取出的
                    cameraBase.ho_image.Dispose();
                    cameraBase.hv_Width.Dispose();
                    cameraBase.hv_Height.Dispose();
                }
            }
            catch (Exception ex)
            {  
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
            if(!panasonicMewtocol2.IsOpen())
            {
                if (m_bRunThread2)
                    m_bRunThread2 = false;
                tbOKSignal2.Enabled = true;
                btAddNGSet2.Enabled = true;
                if (cbEnableCom2.Checked)
                    controlSignal2(true);
            }
            panasonicMewtocol1.Write(tbReadySignal1.Text, false);
            panasonicMewtocol1?.Close();
            if (m_bRunThread1)
                m_bRunThread1 = false;
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
            //if (!m_bTwostart1)
            //{
            //    showLog(tbLog1, "相机1未初始化...");
            //    MessageBox.Show("相机1未初始化");
            //    return false;
            //}
            OperateResult result1 = panasonicMewtocol1.Write(tbReadySignal1.Text, true);
            if (!result1.IsSuccess)
            {
                MessageBox.Show("准备信号发送失败，请检查串口");
                return false;
            }
            return true;
        }
        private void startCom1()
        {
            m_bRunThread1 = true;
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
                panasonicMewtocol1 = new PanasonicMewtocol();
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

                    IniHelper.SaveSetIni.Write("串口1设置", "串口号", cbPortName1.Text);
                    IniHelper.SaveSetIni.Write("串口1设置", "波特率", tbBaudRate1.Text);
                    IniHelper.SaveSetIni.Write("串口1设置", "数据位", tbDateBits1.Text);
                    IniHelper.SaveSetIni.Write("串口1设置", "停止位", tbStopBits1.Text);
                    IniHelper.SaveSetIni.Write("串口1设置", "校验位", cbParity1.Text);
                    showLog(tbLog1, "串口1打开成功...");
                    controlCom1(false);
                    btOpenPlc1.Enabled = false;
                    btClosePlc1.Enabled = true;
                }
                else
                {
                    btClosePlc1.Enabled = false;
                    btOpenPlc1.Enabled = true;
                    controlCom1(true);
                    showLog(tbLog1, $"{cbPortName1.Text}拒绝访问,请换个COM口连接...");
                    MessageBox.Show($"{cbPortName1.Text}拒绝访问,请换个COM口连接...");
                }
            }
            catch (Exception ex)
            {
                btClosePlc1.Enabled = false;
                btOpenPlc1.Enabled = true;
                showLog(tbLog1, ex.Message);
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
            CameraBase cameraBase = null;
            if (camraList.Count > 0)
            {
                cameraBase = camraList[0];
            }else
            {
                try
                {
                    HTuple graval = new HTuple();
                    xy = $"像素坐标:{(int)e.Y},{(int)e.X}";
                    if (cameraBase!=null && cameraBase.ho_image != null)
                    {
                        if (e.Y < 0 || e.X < 0 || e.Y > cameraBase.hv_Height - 1 || e.X > cameraBase.hv_Width - 1)
                            gray = "像素灰度:-";
                        else
                        {
                            graval.Dispose();
                            HOperatorSet.GetGrayval(cameraBase.ho_image, e.Y, e.X, out graval);
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
                }
                if (m_bZoom1)
                {
                    if (m_bMouseLeft1)
                    {
                        double offsetX = e.X - zi1.StartX;
                        double offsetY = e.Y - zi1.StartY;
                        zi1.moveImage(offsetX, offsetY, cameraBase.ho_image);
                    }
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
            CameraBase cameraBase = null;
            if (camraList.Count > 0)
            {
                cameraBase = camraList[0];
            }
            else
            {
                cameraBase = new CameraBase();
            }
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
                zi1.zoomImage(e.X, e.Y, scale, cameraBase.ho_image);
            }
        }
        #endregion

        #region 相机2缩放
        bool m_bZoom2 = false;
        ZoomImage zi2 = null;
        private void 缩放ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            CameraBase cameraBase = null;
            if (camraList.Count > 1)
            {
                cameraBase = camraList[1];
            }
            else
            {
                cameraBase = new CameraBase();
            }
            if (cameraBase!=null && cameraBase.ho_image != null && cameraBase.ho_image.CountObj() != 0)
            {
                m_bZoom2 = true;
            }
        }
        #endregion

        #region 相机2还原
        private void 还原ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            CameraBase cameraBase = null;
            if (camraList.Count > 1)
            {
                cameraBase = camraList[1];
            }
            else
            {
                cameraBase = new CameraBase();
            }
            if (cameraBase.ho_image != null && cameraBase.ho_image.CountObj() != 0)
            {
                m_bZoom2 = true;
                hWindowControl2.HalconWindow.ClearWindow();
                HOperatorSet.GetImageSize(cameraBase.ho_image, out cameraBase.hv_Width, out cameraBase.hv_Height);
                HalconHelper.imageLocation(cameraBase.hv_Width, cameraBase.hv_Height, hWindowControl2.Width, hWindowControl2.Height, out cameraBase.row_left, out cameraBase.column_left, out cameraBase.row_right, out cameraBase.column_right);
                HOperatorSet.SetPart(hWindowControl2.HalconWindow, cameraBase.row_left, cameraBase.column_left, cameraBase.row_right, cameraBase.column_right);
                HOperatorSet.DispObj(cameraBase.ho_image, hWindowControl2.HalconWindow);
                zi2 = new ZoomImage(cameraBase.row_left, cameraBase.column_left, cameraBase.row_right, cameraBase.column_right, cameraBase.hv_Width, cameraBase.hv_Height, this.hWindowControl2);
            }
        }
        #endregion

        #region 相机2读取图片
        private void 读取图片ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                this.openFileDialog1.Filter = "BMP图片|*.BMP|所有图片|*.*";
                if (this.openFileDialog1.ShowDialog() == DialogResult.OK && camraList.Count > 1)
                {
                    CameraBase cameraBase = camraList[1];
                    int winWidth = hWindowControl2.Width;
                    int winHeight = hWindowControl2.Height;
                    hWindowControl2.HalconWindow.ClearWindow();
                    HOperatorSet.ReadImage(out cameraBase.ho_image, this.openFileDialog1.FileName);
                    cameraBase.hv_Width.Dispose(); cameraBase.hv_Height.Dispose();
                    HOperatorSet.GetImageSize(cameraBase.ho_image, out cameraBase.hv_Width, out cameraBase.hv_Height);
                    HalconHelper.imageLocation(cameraBase.hv_Width, cameraBase.hv_Height, winWidth, winHeight, out cameraBase.row_left, out cameraBase.column_left, out cameraBase.row_right, out cameraBase.column_right);
                    HOperatorSet.SetPart(this.hWindowControl2.HalconWindow, cameraBase.row_left, cameraBase.column_left, cameraBase.row_right, cameraBase.column_right);
                    HOperatorSet.DispObj(cameraBase.ho_image, this.hWindowControl2.HalconWindow);
                    zi2 = new ZoomImage(cameraBase.row_left, cameraBase.column_left, cameraBase.row_right, cameraBase.column_right, cameraBase.hv_Width, cameraBase.hv_Height, this.hWindowControl2);

                    //从catch中取出的
                    cameraBase.ho_image.Dispose();
                    cameraBase.hv_Width.Dispose();
                    cameraBase.hv_Height.Dispose();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                LogHelper.WriteLog(ex.ToString());
            }
        }
        #endregion

        #region 相机2串口通讯
        private void btOpenPlc2_Click(object sender, EventArgs e)
        {
            if (!panasonicMewtocol2.IsOpen())
                loadTwoPort();
        }

        private void btClosePlc2_Click(object sender, EventArgs e)
        {
            panasonicMewtocol2.Write(tbReadySignal2.Text, false);
            panasonicMewtocol2?.Close();
            if (m_bRunThread2)
                m_bRunThread2 = false;
            showLog(tbLog2, "串口2断开通讯...");
            btOpenPlc2.Enabled = true;
            btClosePlc2.Enabled = false;
            btAddNGSet2.Enabled = true;
            controlCom2(true);
            controlSignal2(true);
        }
        private bool loadCom2()
        {
            if (cbEnableCom2.Checked)
            {
                if (!panasonicMewtocol2.IsOpen())
                {
                    MessageBox.Show("启动失败原因如下:1.如需用串口2通讯请先打开串口2；2.如不需串口2请关闭启用功能");
                    return false;
                }
            }
            else
            {
                if (!panasonicMewtocol1.IsOpen())
                {
                    MessageBox.Show("请先打开串口1");
                    return false;
                }
            }
            if (cbEnableCom2.Checked)
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
            }
            else
            {
                if (String.IsNullOrEmpty(tbOKSignal2.Text))
                {
                    MessageBox.Show("请先输入相机2PLC地址");
                    return false;
                }
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
            m_bRunThread2 = true;
            showLog(tbLog2, "相机2已就绪...");
            showLog(tbLog2, "软件已就绪...");
            if (!cbEnableCom2.Checked)
                IniHelper.SaveSetIni.Write("相机2PLC设置", "OK信号", tbOKSignal2.Text);
            else
            {
                IniHelper.SaveSetIni.Write("相机2PLC设置", "准备信号", tbReadySignal2.Text);
                IniHelper.SaveSetIni.Write("相机2PLC设置", "OK信号", tbOKSignal2.Text);
                IniHelper.SaveSetIni.Write("相机2PLC设置", "拍照信号", tbGrabImageSignal2.Text);
                controlSignal2(false);
            }
            tbOKSignal2.Enabled = false;
            btAddNGSet2.Enabled = false;
            cbEnableCom2.Enabled = false;
            contextMenuStrip2.Enabled = false;
        }
        private void loadTwoPort()
        {
            try
            {
                panasonicMewtocol2?.Close();
                panasonicMewtocol2.SerialPortInni(sp =>
                {
                    sp.PortName = cbPortName2.Text;
                    sp.BaudRate = Convert.ToInt32(tbBaudRate2.Text);
                    sp.DataBits = Convert.ToInt32(tbDateBits2.Text); ;
                    sp.StopBits = Convert.ToInt32(tbStopBits2.Text) == 0 ? System.IO.Ports.StopBits.None : (Convert.ToInt32(tbStopBits2.Text) == 1 ? System.IO.Ports.StopBits.One : System.IO.Ports.StopBits.Two);
                    sp.Parity = cbParity2.SelectedIndex == 0 ? System.IO.Ports.Parity.None : (cbParity2.SelectedIndex == 1 ? System.IO.Ports.Parity.Odd : System.IO.Ports.Parity.Even);
                });
                panasonicMewtocol2.Open();
                if (panasonicMewtocol2.IsOpen())
                {
                    IniHelper.SaveSetIni.Write("串口2设置", "串口号", cbPortName2.Text);
                    IniHelper.SaveSetIni.Write("串口2设置", "波特率", tbBaudRate2.Text);
                    IniHelper.SaveSetIni.Write("串口2设置", "数据位", tbDateBits2.Text);
                    IniHelper.SaveSetIni.Write("串口2设置", "停止位", tbStopBits2.Text);
                    IniHelper.SaveSetIni.Write("串口2设置", "校验位", cbParity2.Text);
                    showLog(tbLog2, "串口2打开成功...");
                    controlCom2(false);
                    btOpenPlc1.Enabled = false;
                    btClosePlc1.Enabled = true;
                }
                else
                {
                    btClosePlc1.Enabled = false;
                    btOpenPlc1.Enabled = true;
                    controlCom2(true);
                    showLog(tbLog2, $"{cbPortName2.Text}拒绝访问,请换个COM口连接...");
                    MessageBox.Show($"{cbPortName2.Text}拒绝访问,请换个COM口连接...");
                }
            }
            catch (Exception ex)
            {
                btClosePlc2.Enabled = false;
                btOpenPlc2.Enabled = true;
                showLog(tbLog2, ex.Message);
                MessageBox.Show(ex.Message);
            }
        }
            #endregion

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
            CameraBase cameraBase = null;
            if (camraList.Count > 1)
            {
                cameraBase = camraList[1];
            }
            else
            {
                try
                {
                    HTuple graval = new HTuple();
                    xy = $"像素坐标:{(int)e.Y},{(int)e.X}";
                    if (cameraBase != null && cameraBase.ho_image != null)
                    {
                        if (e.Y < 0 || e.X < 0 || e.Y > cameraBase.hv_Height - 1 || e.X > cameraBase.hv_Width - 1)
                            gray = "像素灰度:-";
                        else
                        {
                            graval.Dispose();
                            HOperatorSet.GetGrayval(cameraBase.ho_image, e.Y, e.X, out graval);
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
                }
                if (m_bZoom1)
                {
                    if (m_bMouseLeft2)
                    {
                        double offsetX = e.X - zi2.StartX;
                        double offsetY = e.Y - zi2.StartY;
                        zi2.moveImage(offsetX, offsetY, cameraBase.ho_image);
                    }
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
            CameraBase cameraBase = null;
            if (camraList.Count > 1)
            {
                cameraBase = camraList[1];
            }
            else
            {
                cameraBase = new CameraBase();
            }
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
                zi2.zoomImage(e.X, e.Y, scale, cameraBase.ho_image);
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
                if (cbEnableCode.Checked)
                    controlCodeSet(false);
                if(cbEnableCom2.Checked)
                {
                    controlCom2(false);
                    controlSignal2(false);
                }
                    
                btClearDate.Enabled = false;
                btHandTest.Enabled = true;
                tbOKSignal2.Enabled = false;
                btAddNGSet2.Enabled = false;
                cbEnableCode.Enabled = false;
                cbEnableCom2.Enabled = false;
            }
            else if (str == "停止...")
            {
                controlCameraSet1(true);
                contextMenuStrip1.Enabled = true;
                contextMenuStrip2.Enabled = true;
                btAddNGSet1.Enabled = true;
                controlSignal1(true);
                if (cbEnableCode.Checked)
                    controlCodeSet(true);
                if (cbEnableCom2.Checked)
                {
                    controlCom2(true);
                    controlSignal2(true);
                }
                btClosePlc1.Enabled = true;
                btClearDate.Enabled = true;
                btHandTest.Enabled = false;
                tbOKSignal2.Enabled = true;
                btAddNGSet2.Enabled = true;
                cbEnableCode.Enabled = true;
                cbEnableCom2.Enabled = true;
            }
            if (panasonicMewtocol1.IsOpen())
            {
                if (str == "继续开始自动运行...")
                {
                    panasonicMewtocol1.Write(tbReadySignal1.Text, true);
                    if (panasonicMewtocol2.IsOpen())
                        panasonicMewtocol2.Write(tbReadySignal2.Text, true);
                }
                else if (str == "停止...")
                {
                    panasonicMewtocol1.Write(tbReadySignal1.Text, false);
                    if (panasonicMewtocol2.IsOpen())
                        panasonicMewtocol2.Write(tbReadySignal2.Text, true);
                }
            }
        }

        #endregion

        #region 是否启用串口2
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (cbEnableCom2.Checked)
            {
                IniHelper.SaveSetIni.Write("串口2设置", "是否启用", "T");
                controlCom2(true);
                controlSignal2(true);
            }
            else
            {
                IniHelper.SaveSetIni.Write("串口2设置", "是否启用", "F");
                controlCom2(false);
                controlSignal2(false);
                btClosePlc2.Enabled = false;
                if (panasonicMewtocol2.IsOpen())
                    panasonicMewtocol2?.Close();
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
        private void controlCom2(bool islogin)
        {
            cbPortName2.Enabled = islogin;
            tbBaudRate2.Enabled = islogin;
            tbDateBits2.Enabled = islogin;
            tbStopBits2.Enabled = islogin;
            cbParity2.Enabled = islogin;
            btOpenPlc2.Enabled = islogin;
            btClosePlc2.Enabled = islogin;
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
            frmNGSignalSet signalSet = new frmNGSignalSet("NG2");
            signalSet.Text = "相机1NG信号设置";
            signalSet.ShowDialog();
        }
        #endregion

        #region 相机2NG信号设置
        private void btAddNGSet2_Click(object sender, EventArgs e)
        {
            status = "Cam2";
            frmNGSignalSet signalSet = new frmNGSignalSet("NG3");
            signalSet.Text = "相机2NG信号设置";
            signalSet.ShowDialog();
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
            this.Invoke(new Action(() =>
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
            this.Invoke(new Action(() =>
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
            if (!m_bRunThread1 || !m_bRunThread2)
            {
                MessageBox.Show("软件还未就绪");
                return;
            }
            lock (obj)
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
                string path= IniHelper.SaveSetIni.Read("深度学习文件", "配置文件路径");
                LogHelper.WriteLog($"已清空历史数据,总数:{lbAllNum1.Text};OK数:{lbOKNum1.Text};NG数:{lbNGNum1.Text};良率:{lbOKPercent1.Text}");
                lbAllNum1.Text = "0";
                AllNum1 = 0;
                lbOKNum1.Text = "0";
                OKNum1 = 0;
                lbNGNum1.Text = "0";
                NGNum1= 0;
                lbOKPercent1.Text = "0";
                lbCt1.Text = "0";
                lbResult1.BackColor = System.Drawing.Color.Red;
                lbResult1.Text = "NG";
                IniHelper.SaveSetIni.Write("相机1生产信息", "生产总数", "0");
                IniHelper.SaveSetIni.Write("相机1生产信息", "OK总数", "0");
                IniHelper.SaveSetIni.Write("相机1生产信息", "NG总数", "0");
                LogHelper.WriteLog($"已清空历史数据,总数:{lbAllNum2.Text};OK数:{lbOKNum2.Text};NG数:{lbNGNum2.Text};良率:{lbOKPercent2.Text}");
                lbAllNum2.Text = "0";
                AllNum2= 0;
                lbOKNum2.Text = "0";
                OKNum2= 0;
                lbNGNum2.Text = "0";
                NGNum2 = 0;
                lbOKPercent2.Text = "0";
                lbCt2.Text = "0";
                lbResult2.BackColor = System.Drawing.Color.Red;
                lbResult2.Text = "NG";
                IniHelper.SaveSetIni.Write("相机2生产信息", "生产总数", "0");
                IniHelper.SaveSetIni.Write("相机2生产信息", "OK总数", "0");
                IniHelper.SaveSetIni.Write("相机2生产信息", "NG总数", "0");
                dataGridView3.Rows.Clear();
                dataGridView3.Columns.Clear();
                dataGridView4.Rows.Clear();
                dataGridView4.Columns.Clear();
                createHead1();
                createHead2();
                if (!String.IsNullOrEmpty(path)&&dataGridView3.Rows.Count<2&&dataGridView4.Rows.Count<2)
                    addRows(path);
            }
        }

        #endregion

        #region 加载方案
        IntPtr ptr1 = IntPtr.Zero;
        IntPtr ptr2 = IntPtr.Zero;
        private async Task btLoadConfig_Click(object sender, EventArgs e)
        {
            //if (!loadCom1() || !loadCom2())
            //    return;
            if (!loadCom1())
                return;
            //string configpath = IniHelper.SaveSetIni.Read("深度学习文件", "配置文件路径");
            //TreeFileHelper.LoadTreeFile(configpath);
            btLoadConfig.Enabled = false;
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken token = cancellationTokenSource.Token;
            await Task.Run(() =>
            {
                string configpath = IniHelper.SaveSetIni.Read("深度学习文件", "配置文件路径");
                TreeFileHelper.LoadTreeFile(configpath);
                progressBar1.BeginInvoke(new Action(() => {
                    if(dataGridView3.Rows.Count < 2 && dataGridView4.Rows.Count < 2)
                    addRows(configpath);
                }));
                for (int i = 0; i < 100; i++)
                {
                    progressBar1.BeginInvoke(new Action(() => { progressBar1.Value = i; }));
                    if (i == 40)
                    {
                        if (!HTCSharpDemo.Program.loadDeepStudyHandle(IniHelper.SaveSetIni.Read("深度学习文件", "配置文件路径"), ref studyHandle, ref test_num))
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
                                btLoadConfig.Enabled = false;
                                btLoadConfig.Text = "加载完毕";
                                startCom1();
                                startCom2();
                                btClearDate.Enabled = false;
                                autoResetEvents[1].Set();
                            }));
                        }
                    }
                }
            }, token);
        }

        #endregion

        #region 加载深度学习配置文件到表格
        private void addRows(string configPath)
        {
            TreeFileHelper.NodeInfos node = new TreeFileHelper.NodeInfos();
            node.NodeInfo = TreeFileHelper.LoadTreeFile(configPath).NodeInfo;  
            foreach (var item in node.NodeInfo)
            {
                if (item.NodeType == 1)
                {
                    if (item.ParentsNodeId == -1)
                    {
                        dataGridView3.Rows.Add(item.NodeName, "定位失败", "0", "--", "0", "0", "--", "--");
                        dataGridView4.Rows.Add(item.NodeName, "定位失败", "0", "--", "0", "0", "--", "--");
                    }
                        
                }
                else
                {
                    foreach (var item1 in item.ClassNames)
                    {
                        if (!item1.Equals("OK", StringComparison.OrdinalIgnoreCase) && !TreeFileHelper.noAddItem.Contains(item1))
                        {
                            dataGridView3.Rows.Add(item.NodeName, item1, "0", "--", "0", "0", "--", "--");
                            dataGridView4.Rows.Add(item.NodeName, item1, "0", "--", "0", "0", "--", "--");
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
        private async void ProcResult1(HTCSharpDemo.Program dl)
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
                            tbLog1.Invoke(GetMyDelegateLog, tbLog1,$"{tbOKSignal1.Text}信号次发送成功");
                            Thread.Sleep(200);
                            do
                            {
                                panasonicMewtocol1.Write(tbOKSignal1.Text, false);
                                Thread.Sleep(30);
                            } while (panasonicMewtocol1.ReadBool(tbOKSignal1.Text).Content);
                            tbLog1.Invoke(GetMyDelegateLog, tbLog1,$"{tbOKSignal1.Text}信号次断开成功");
                            break;
                        }
                    case "NG":
                        {
                            bool status = false;
                            foreach (var item in dl.NGItems1)
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
                                        tbLog1.Invoke(GetMyDelegateLog, tbLog1, $"{ngSignal1}信号次发送成功");
                                        Thread.Sleep(200);
                                        do
                                        {
                                            panasonicMewtocol1.Write(ngSignal1, false);
                                            Thread.Sleep(30);
                                        } while (panasonicMewtocol1.ReadBool(ngSignal1).Content);
                                        tbLog1.Invoke(GetMyDelegateLog, tbLog1, $"{ngSignal1}信号次断开成功");
                                    }
                                }
                            }
                            break;
                        }
                    case "定位失败":
                        {
                            TreeFileHelper.NodeInfos nodeInfo = new TreeFileHelper.NodeInfos();
                            nodeInfo.NodeInfo = TreeFileHelper.LoadTreeFile(configPath).NodeInfo;
                            foreach (var item in nodeInfo.NodeInfo)
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
                            tbLog1.Invoke(GetMyDelegateLog, tbLog1, $"{dingweiSignal1}信号次发送成功");
                            Thread.Sleep(200);
                            do
                            {
                                panasonicMewtocol1.Write(dingweiSignal1, false);
                                Thread.Sleep(30);
                            } while (panasonicMewtocol1.ReadBool(dingweiSignal1).Content);
                            tbLog1.Invoke(GetMyDelegateLog, tbLog1, $"{dingweiSignal1}信号次断开成功");
                            break;
                        }
                    default:
                        break;
                }
            });
        }
        string dingweiSignal2;
        string ngSignal2;
        private async void ProcResult2(HTCSharpDemo.Program dl)
        {
            await Task.Run(() =>
            {
                string configPath = IniHelper.SaveSetIni.Read("深度学习文件", "配置文件路径");
                if(!panasonicMewtocol2.IsOpen())
                {
                    switch (result2)
                    {
                        case "OK":
                            {
                                do
                                {
                                    panasonicMewtocol1.Write(tbOKSignal2.Text, true);
                                    Thread.Sleep(30);
                                } while (!panasonicMewtocol1.ReadBool(tbOKSignal2.Text).Content);
                                tbLog1.Invoke(GetMyDelegateLog, tbLog2, $"{tbOKSignal2.Text}信号次发送成功");
                                Thread.Sleep(200);
                                do
                                {
                                    panasonicMewtocol1.Write(tbOKSignal2.Text, false);
                                    Thread.Sleep(30);
                                } while (panasonicMewtocol1.ReadBool(tbOKSignal2.Text).Content);
                                tbLog1.Invoke(GetMyDelegateLog, tbLog2, $"{tbOKSignal2.Text}信号次断开成功");
                                break;
                            }
                        case "NG":
                            {
                                bool status = false;
                                foreach (var item in dl.NGItems2)
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
                                            tbLog1.Invoke(GetMyDelegateLog, tbLog2, $"{ngSignal2}信号次发送成功");
                                            Thread.Sleep(200);
                                            do
                                            {
                                                panasonicMewtocol1.Write(ngSignal2, false);
                                                Thread.Sleep(30);
                                            } while (panasonicMewtocol1.ReadBool(ngSignal2).Content);
                                            tbLog1.Invoke(GetMyDelegateLog, tbLog2, $"{ngSignal2}信号次断开成功");
                                        }
                                    }
                                }
                                break;
                            }
                        case "定位失败":
                            {
                                TreeFileHelper.NodeInfos nodeInfo = new TreeFileHelper.NodeInfos();
                                nodeInfo.NodeInfo = TreeFileHelper.LoadTreeFile(configPath).NodeInfo;
                                foreach (var item in nodeInfo.NodeInfo)
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
                                tbLog1.Invoke(GetMyDelegateLog, tbLog2, $"{dingweiSignal2}信号次发送成功");
                                Thread.Sleep(200);
                                do
                                {
                                    panasonicMewtocol1.Write(dingweiSignal2, false);
                                    Thread.Sleep(30);
                                } while (panasonicMewtocol1.ReadBool(dingweiSignal2).Content);
                                tbLog1.Invoke(GetMyDelegateLog, tbLog2, $"{dingweiSignal2}信号次断开成功");
                                break;
                            }
                        default:
                            break;
                    }
                }
                else
                {
                    switch (result2)
                    {
                        case "OK":
                            {
                                do
                                {
                                    panasonicMewtocol2.Write(tbOKSignal2.Text, true);
                                    Thread.Sleep(30);
                                } while (!panasonicMewtocol2.ReadBool(tbOKSignal2.Text).Content);
                                tbLog1.Invoke(GetMyDelegateLog, tbLog2, $"{tbOKSignal2.Text}信号次发送成功");
                                Thread.Sleep(200);
                                do
                                {
                                    panasonicMewtocol2.Write(tbOKSignal2.Text, false);
                                    Thread.Sleep(30);
                                } while (panasonicMewtocol2.ReadBool(tbOKSignal2.Text).Content);
                                tbLog1.Invoke(GetMyDelegateLog, tbLog2, $"{tbOKSignal2.Text}信号次断开成功");
                                break;
                            }
                        case "NG":
                            {
                                bool status = false;
                                foreach (var item in dl.NGItems2)
                                {
                                    foreach (var item1 in NGType2)
                                    {
                                        if (item == item1.Key && !status)
                                        {
                                            status = true;
                                            ngSignal2 = NGType2[item];
                                            do
                                            {
                                                panasonicMewtocol2.Write(ngSignal2, true);
                                                Thread.Sleep(30);
                                            } while (!panasonicMewtocol2.ReadBool(ngSignal2).Content);
                                            tbLog1.Invoke(GetMyDelegateLog, tbLog2, $"{ngSignal2}信号次发送成功");
                                            Thread.Sleep(200);
                                            do
                                            {
                                                panasonicMewtocol2.Write(ngSignal2, false);
                                                Thread.Sleep(30);
                                            } while (panasonicMewtocol2.ReadBool(ngSignal2).Content);
                                            tbLog1.Invoke(GetMyDelegateLog, tbLog2, $"{ngSignal2}信号次断开成功");
                                        }
                                    }
                                }
                                break;
                            }
                        case "定位失败":
                            {
                                TreeFileHelper.NodeInfos nodeInfo = new TreeFileHelper.NodeInfos();
                                nodeInfo.NodeInfo = TreeFileHelper.LoadTreeFile(configPath).NodeInfo;
                                foreach (var item in nodeInfo.NodeInfo)
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
                                    panasonicMewtocol2.Write(dingweiSignal2, true);
                                    Thread.Sleep(30);
                                } while (!panasonicMewtocol2.ReadBool(dingweiSignal2).Content);
                                tbLog1.Invoke(GetMyDelegateLog, tbLog2, $"{dingweiSignal2}信号次发送成功");
                                Thread.Sleep(200);
                                do
                                {
                                    panasonicMewtocol2.Write(dingweiSignal2, false);
                                    Thread.Sleep(30);
                                } while (panasonicMewtocol2.ReadBool(dingweiSignal2).Content);
                                tbLog1.Invoke(GetMyDelegateLog, tbLog2, $"{dingweiSignal2}信号次断开成功");
                                break;
                            }
                        default:
                            break;
                    }
                }
            });
        }
        #endregion

        #region 渲染图片
        private async void showNGLocation1(HTCSharpDemo.Program dl)
        {
            await Task.Run(() =>
            {
                if (result1 == "OK")
                {
                    HTCSharpDemo.Program.result res2 = dl.res1[TreeFileHelper.dingweiNodeId][0];
                    HalconHelper.showRoi(hWindowControl1.HalconWindow, "green", res2.y, res2.x, res2.y + res2.height, res2.x + res2.width);
                    HalconHelper.showResultString(hWindowControl1.HalconWindow, 0, 0, 72, "green", "OK");


                }
                else
                {
                    if (dl.res1[TreeFileHelper.dingweiNodeId].Count == 1)
                    {
                        HTCSharpDemo.Program.result res2 = dl.res1[TreeFileHelper.dingweiNodeId][0];
                        HalconHelper.showRoi(hWindowControl1.HalconWindow, "red", res2.y, res2.x, res2.y + res2.height, res2.x + res2.width);
                    }
                    HalconHelper.showResultString(hWindowControl1.HalconWindow, 0, 0, 72, "red", result1);


                }

                if (dl.res1.ContainsKey(TreeFileHelper.quexianNodeId))
                {
                    foreach (var item in dl.res1[TreeFileHelper.quexianNodeId])
                    {
                        HalconHelper.showRoi(hWindowControl1.HalconWindow, "red", item.y, item.x, item.y + item.height, item.x + item.width);
                    }
                }
                foreach (var item in dl.res1)
                {
                    if (TreeFileHelper.listDingweiId.Contains(item.Key))
                    {
                        foreach (var item1 in item.Value)
                        {
                            HalconHelper.showRoi(hWindowControl1.HalconWindow, "red", item1.y, item1.x, item1.y + item1.height, item1.x + item1.width);
                        }
                    }
                }
            });
        }
        private async void showNGLocation2(HTCSharpDemo.Program dl)
        {
            await Task.Run(() =>
            {
                if (result2 == "OK")
                {
                    HTCSharpDemo.Program.result res2 = dl.res2[TreeFileHelper.dingweiNodeId][0];
                    HalconHelper.showRoi(hWindowControl2.HalconWindow, "green", res2.y, res2.x, res2.y + res2.height, res2.x + res2.width);
                    HalconHelper.showResultString(hWindowControl2.HalconWindow, 0, 0, 72, "green", "OK");


                }
                else
                {
                    if (dl.res2[TreeFileHelper.dingweiNodeId].Count == 1)
                    {
                        HTCSharpDemo.Program.result res2 = dl.res2[TreeFileHelper.dingweiNodeId][0];
                        HalconHelper.showRoi(hWindowControl2.HalconWindow, "red", res2.y, res2.x, res2.y + res2.height, res2.x + res2.width);
                    }
                    HalconHelper.showResultString(hWindowControl2.HalconWindow, 0, 0, 72, "red", result2);


                }

                if (dl.res2.ContainsKey(TreeFileHelper.quexianNodeId))
                {
                    foreach (var item in dl.res2[TreeFileHelper.quexianNodeId])
                    {
                        HalconHelper.showRoi(hWindowControl2.HalconWindow, "red", item.y, item.x, item.y + item.height, item.x + item.width);
                    }
                }
                foreach (var item in dl.res2)
                {
                    if (TreeFileHelper.listDingweiId.Contains(item.Key))
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
            this.Invoke(new Action(() =>
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
            this.Invoke(new Action(() =>
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
        private async void referenceTable1(HTCSharpDemo.Program dl)
        {
            await Task.Run(new Action(() =>
            {
                string configPath = IniHelper.SaveSetIni.Read("深度学习文件", "配置文件路径");
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
                                    if (dl.res1[TreeFileHelper.dingweiNodeId].Count == 1)
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
                            foreach (var item in dl.res1)
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
            }));
        }
        private async void referenceTable2(HTCSharpDemo.Program dl)
        {
            await Task.Run(new Action(() =>
            {
                string configPath = IniHelper.SaveSetIni.Read("深度学习文件", "配置文件路径");
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
                                    if (dl.res2[TreeFileHelper.dingweiNodeId].Count == 1)
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
                            foreach (var item in dl.res2)
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
            }));
        }
        #endregion

        #region 异步保存图片
        private async void saveImage1(CameraBase cameraBase, string okStatus, string ngStatus, string result, HTCSharpDemo.Program dl)
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
                    string path = savePath + "\\" +"相机1"+"\\"+ nowDataTime + "\\" + "OK" + "\\";
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);
                    string imagePath = HalconHelper.fileNameIsExist(path, imageName);
                    HalconHelper.saveImage(hWindowControl1.HalconWindow, cameraBase.ho_image, imagePath);
                }
                else if (ngStatus == "T" && result == "NG")
                {
                    foreach (var item in dl.NGItems1)
                    {
                        string path = savePath + "\\"+"相机1" + "\\" + nowDataTime + "\\" + $"{item}" + "\\";
                        if (!Directory.Exists(path))
                            Directory.CreateDirectory(path);
                        string imagePath = HalconHelper.fileNameIsExist(path, imageName);
                        HalconHelper.saveImage(hWindowControl1.HalconWindow, cameraBase.ho_image, imagePath);
                    }
                }
            });
        }
        private async void saveImage2(CameraBase cameraBase,string okStatus, string ngStatus, string result,HTCSharpDemo.Program dl)
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
                    HalconHelper.saveImage(hWindowControl1.HalconWindow, cameraBase.ho_image, imagePath);
                }
                else if (ngStatus == "T" && result == "NG")
                {
                    foreach (var item in dl.NGItems2)
                    {
                        string path = savePath + "\\" + "相机2" + "\\" + nowDataTime + "\\" + $"{item}" + "\\";
                        if (!Directory.Exists(path))
                            Directory.CreateDirectory(path);
                        string imagePath = HalconHelper.fileNameIsExist(path, imageName);
                        HalconHelper.saveImage(hWindowControl1.HalconWindow, cameraBase.ho_image, imagePath);
                    }
                }
            });
        }
        #endregion

        #region 检查磁盘剩余空间
        private bool isFreeSpace(string path)
        {
            string savePath = IniHelper.SaveSetIni.Read("图片设置", "保存路径");
            DriveInfo drive = new DriveInfo(System.IO.Path.GetPathRoot(savePath));
            if(drive.IsReady)
            {
                long freeSpace = drive.TotalFreeSpace;
                double freeSpaceInMB = freeSpace / (1024 * 1024);
                if (freeSpaceInMB < 6)
                    return false;
            }
            return true;
        }
        #endregion

        #region 实施设置曝光
        string camStatus;
        private void 实时读取设置曝光ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(!m_bTwostart1)
            {
                MessageBox.Show("相机1未打开");
                return;
            }
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
                while (true)
                 {
                    switch (camStatus)
                    {
                        case "相机1":
                            if(camraList.Count > 0)
                            {
                                CameraBase cameraBase = camraList[0];
                                grabHoImage(ref cameraBase, out cameraBase.ho_image);
                                //grabOneImage(out ho_image1);
                                ImageToHalcon(ref cameraBase, cameraBase.ho_image);
                            }
                            break;
                        case "相机2":
                            if(camraList.Count > 1)
                            {
                                CameraBase cameraBase = camraList[0];
                                grabHoImage(ref cameraBase, out cameraBase.ho_image);
                                ImageToHalcon(ref cameraBase, cameraBase.ho_image);
                            }
                            //grabTwoImage(out ho_image2);
                            //twoImageToHalcon(ho_image2);
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
                    if (camraList.Count > 0)
                    {
                        CameraBase cameraBase = camraList[0];
                        cameraBase.cam_device.MV_CC_SetEnumValue_NET("ExposureAuto", 0);
                        int nRet = cameraBase.cam_device.MV_CC_SetFloatValue_NET("ExposureTime", expose);
                        if (nRet == MyCamera.MV_OK)
                        {
                            IniHelper.SaveSetIni.Write("相机1设置", "曝光量", expose.ToString());
                            showLog(tbLog1, $"相机1设置曝光成功:{expose}");
                        }
                        else
                        {
                            showLog(tbLog1, "相机1设置曝光失败...");
                            MessageBox.Show("Set Exposure Time Fail!");
                        }
                    }
                    break;
                case "相机2":
                    if (camraList.Count > 1)
                    {
                        CameraBase cameraBase = camraList[1];
                        cameraBase.cam_device.MV_CC_SetEnumValue_NET("ExposureAuto", 0);
                        int nRet2 = cameraBase.cam_device.MV_CC_SetFloatValue_NET("ExposureTime", expose);
                        if (nRet2 == MyCamera.MV_OK)
                        {
                            IniHelper.SaveSetIni.Write("相机2设置", "曝光量", expose.ToString());
                        }
                        else
                        {
                            showLog(tbLog2, $"相机2设置设置失败...");
                            MessageBox.Show("Set Exposure Time Fail!");
                        }
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
            TreeFileHelper.NodeInfos nodeInfo = new TreeFileHelper.NodeInfos();
            nodeInfo.NodeInfo = TreeFileHelper.LoadTreeFile(path).NodeInfo;
            NGTypePara.NGType ngTypes = new NGTypePara.NGType();
            foreach (var item1 in nodeInfo.NodeInfo)
            {
                if (item1.ParentsNodeId > -1 && item1.NodeType == 1)
                    continue;
                foreach (var item2 in item1.ClassNames)
                {
                    if (!TreeFileHelper.noAddItem.Contains(item2))
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
            File.WriteAllText(rootDirectory2, json);
        }
        #endregion

        private void frmTwoCamera_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 两个相机的线程此时应该关闭
            isRunning = false;
            Thread.Sleep(100); //等待让两个相机线程能感知到

            listeningSerilPortThread.Join();

            closeCameraList();
            if (studyHandle != IntPtr.Zero)
            {
                HTCSharpDemo.Program.ReleaseTree(studyHandle);
                studyHandle = IntPtr.Zero;
            }
           
            IniHelper.SaveSetIni.Write("布局", "窗口数量", "2");
            frmMain.controlEvent -= new controlDelegate(runControl);
            frmNGSignalSet.loadNGEvent -= new loadNGSignalDic(addNGDic1);
            frmNGSignalSet.loadNGEvent -= new loadNGSignalDic(addNGDic2);
            frmNGSignalSet.loadNGEvent -= new loadNGSignalDic(addNGorOK1);
            frmNGSignalSet.loadNGEvent -= new loadNGSignalDic(addNGorOK2);
            frmSet.newConfigEvent -= new newConfigDel(loadNewFile);
            frmMain.closeAll -= new closeDelegate(frmTwoClose);
            panasonicMewtocol1.Write(tbReadySignal1.Text, false);
            panasonicMewtocol1?.Close();
        }
    }
}
