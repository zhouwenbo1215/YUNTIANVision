using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using YUNTIANVision.Properties;
using HalconDotNet;
using MvCamCtrl.NET;
using System.Xml.Linq;
using INI;
using System.Runtime.InteropServices;
using HslCommunication.Profinet.Panasonic;
using System.IO.Ports;
using System.IO;
using System.Threading;

namespace YUNTIANVision
{
    public delegate void closeDelegate();
    public delegate void controlDelegate(string str);
    public partial class frmMain : Form
    {
        public static event closeDelegate closeAll;
        public static event controlDelegate controlEvent;
        /// <summary>
        /// 相机数量
        /// </summary>
        public static int cameraNum;
        public static bool m_bRunOrStop;
        public static string  XY;
        public static string grayValue;
        public frmMain()
        {
            InitializeComponent();
        }
        #region load/close事件
        private void Form1_Load(object sender, EventArgs e)
        {
            toolStripStatusLabel4.Text = DateTime.Now.ToString();
            this.timer1.Start();
            timer1.Interval = 1000;
            if(String.IsNullOrEmpty(IniHelper.SaveSetIni.Read("布局","窗口数量")))
                loadOneCamera();
            else
            {
                if(Convert.ToInt32(IniHelper.SaveSetIni.Read("布局", "窗口数量"))==2)
                    loadTwoCamera();
                else
                    loadOneCamera();
            }
            toolStripButton1.Enabled = false;
            toolStripSplitButton1.Enabled = false;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("确认是否退出?", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Error) == DialogResult.OK)
            {
                timer1.Stop();
                closeAll?.Invoke();
                Dispose();
                Application.Exit();
            }
            else
                e.Cancel = true;
        }
        #endregion

        #region 计时器
        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                timer1.Enabled = false;
                toolStripStatusLabel4.Text = DateTime.Now.ToString();
                int day = Convert.ToInt32(IniHelper.SaveSetIni.Read("自动删除图片", "天数"));
                if (String.IsNullOrEmpty(IniHelper.SaveSetIni.Read("自动删除图片", "天数")))
                    day = 30;
                DateTime thirtyDaysAgo = DateTime.Now.AddDays(-day);
                // 指定要清理的目录
                string path = IniHelper.SaveSetIni.Read("图片设置", "保存路径");
                if (!String.IsNullOrEmpty(IniHelper.SaveSetIni.Read("自动删除图片", "天数")))
                {
                    if (Directory.Exists(path))
                    {
                        string[] direcArr = Directory.GetDirectories(path);
                        foreach (var item in direcArr)
                        {
                            if (Directory.GetCreationTime(item) < thirtyDaysAgo)
                                Directory.Delete(item,true);
                        }
                    }
                }
                timer1.Enabled = true;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.ToString());
                MessageBox.Show(ex.ToString());
            }
            
        }
        #endregion

        #region 运行/停止
        bool m_bRun;
        public static bool m_bPause = false;
        public static bool m_bResume = false;
        private void tsbRun_Click(object sender, EventArgs e)
        {
            if (m_bRun)
            {
                tsbRun.Text = "停止";      //软件运行
                tsbRun.Image = new Bitmap(Resources.暂停);
                if (m_bPause) m_bResume = true;
                controlEvent?.Invoke("继续开始自动运行...");
                runControl("继续开始自动运行...");
            }
            else
            {
                tsbRun.Text = "自动运行";        //软件暂停
                tsbRun.Image = new Bitmap(Resources.播放);
                m_bPause = true;
                m_bRunOrStop = true;
                controlEvent?.Invoke("停止...");
                runControl("停止...");
            }
            
            
            m_bRun = !m_bRun;
        }
        #endregion

        #region 初始化单个相机halcon界面
        private void loadOneCamera()
        {
            panel1.Controls.Clear();
            frmOneCamera Onecamera = new frmOneCamera();
            Onecamera.TopLevel = false;
            Onecamera.FormBorderStyle = FormBorderStyle.None;
            Onecamera.Dock = DockStyle.Fill;
            Onecamera.Show();
            this.panel1.Controls.Add(Onecamera);
        }
        #endregion

        #region 初始化两个相机halcon界面

        private void loadTwoCamera()
        {
            panel1.Controls.Clear();
            frmTwoCamera Twocamera = new frmTwoCamera();
            Twocamera.TopLevel = false;
            Twocamera.FormBorderStyle = FormBorderStyle.None;
            Twocamera.Dock = DockStyle.Fill;
            Twocamera.Show();
            this.panel1.Controls.Add(Twocamera);
        }

        #endregion

        #region 切换为单个相机界面
        private  void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            try
            {
                toolStripButton1.Enabled = false;
                toolStripSplitButton1.Enabled = false;
                closeAll?.Invoke();
                tsbRun.Text = "停止";      //软件运行
                tsbRun.Image = new Bitmap(Resources.播放);
                if (m_bPause) m_bResume = true;

                IniHelper.SaveSetIni.Write("布局", "窗口数量", "1");

                panel1.Controls.Clear();
                frmOneCamera Onecamera = new frmOneCamera();
                Onecamera.TopLevel = false;
                Onecamera.FormBorderStyle = FormBorderStyle.None;
                Onecamera.Dock = DockStyle.Fill;
                this.panel1.Controls.Add(Onecamera);
                Onecamera.Show();

                XY = "像素坐标:--";
                grayValue = "像素灰度:-";
            }
            catch (Exception ex)
            {MessageBox.Show(ex.ToString());}
        }
        #endregion

        #region 切换为两个相机界面
        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            try
            {
                toolStripButton1.Enabled = false;
                toolStripSplitButton1.Enabled = false;
                closeAll?.Invoke();
                tsbRun.Text = "停止";      //软件运行
                tsbRun.Image = new Bitmap(Resources.播放);
                if (m_bPause) m_bResume = true;

                IniHelper.SaveSetIni.Write("布局", "窗口数量", "2");

                panel1.Controls.Clear();
                frmTwoCamera twoCamera = new frmTwoCamera();
                twoCamera.TopLevel = false;
                twoCamera.FormBorderStyle = FormBorderStyle.None;
                twoCamera.Dock = DockStyle.Fill;
                this.panel1.Controls.Add(twoCamera);
                twoCamera.Show();

                XY = "像素坐标:--";
                grayValue = "像素灰度:-";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                LogHelper.WriteLog(ex.ToString());
            }
        }
        #endregion

        #region 保存图片路径设置
        private void toolStripButton1_Click_1(object sender, EventArgs e)
        {
            frmSet frm = new frmSet();
            frm.Show();
        }
        #endregion

        #region 官网链接
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.gdytv.com/");
        }
        #endregion

        #region 运行过程中禁止操作软件
        private void runControl(string str)
        {
            if (str=="继续开始自动运行...")
            {
                this.panel1.Invoke(new Action(() =>
                {
                    toolStripButton1.Enabled = false;
                    toolStripSplitButton1.Enabled = false;
                }));
            }
            else if (str == "停止...")
            {
                this.panel1.Invoke(new Action(() =>
                {
                    toolStripButton1.Enabled = true;
                    toolStripSplitButton1.Enabled = true;
                }));
            }
        }
        #endregion

        #region 异步显示灰度跟坐标

        private void showXYAndGray(string xy,string gray)
        {
             this.Invoke(new Action(() =>
            {
                toolStripStatusLabel6.Text = xy;
                toolStripStatusLabel7.Text = gray;
            }));
        }
        #endregion
    }
}
