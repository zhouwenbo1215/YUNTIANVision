using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;
using static YUNTIANVision.frmTwoCamera;

namespace YUNTIANVision
{
    public partial class frmImformation : Form
    {
        public frmImformation()
        {
            InitializeComponent();
            frmOneCamera.myShowDelegate += new showOneDelegate(showImformation1);
            frmOneCamera.myCloseTableDelegate += new closeOneDelegate(closeImformation);
            frmTwoCamera.myShowTwoDelegate += new showTwoDelegate(showImformation2);
            frmTwoCamera.myCloseTwoDelegate += new closeTwoDelegate(closeImformation);
            
        }
        /// <summary>
        /// 产品总数
        /// </summary>
        public static double AllNum = 0;
        /// <summary>
        /// 产品图像处理时间
        /// </summary>
        public static int ct;
        /// <summary>
        /// OK总数
        /// </summary>
        public static double OKNum = 0;
        /// <summary>
        /// NG总数
        /// </summary>
        public static double NGNum = 0;
        private delegate void myDelegateRes(string allnum, string oknum, string ngnum, string ct, string res, Color color);
        private myDelegateRes GetMyDelegateRes;
        private void frmImformation_Load(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(IniHelper.SaveSetIni.Read("生产信息", "生产总数")))
                AllNum = Convert.ToInt32(IniHelper.SaveSetIni.Read("生产信息", "生产总数"));
            if (!String.IsNullOrEmpty(IniHelper.SaveSetIni.Read("生产信息", "OK总数")))
                OKNum = Convert.ToInt32(IniHelper.SaveSetIni.Read("生产信息", "OK总数"));
            if (!String.IsNullOrEmpty(IniHelper.SaveSetIni.Read("生产信息", "NG总数")))
                NGNum = Convert.ToInt32(IniHelper.SaveSetIni.Read("生产信息", "NG总数"));
            lbAllNum.Text = AllNum.ToString();
            lbOKNum.Text = OKNum.ToString();
            lbNGNum.Text = NGNum.ToString();
            if (OKNum != 0 && AllNum != 0)
                lbOKPercent.Text = (OKNum / AllNum * 100).ToString("F2") + "%";
            lbResult.Text = "NG";
            lbResult.BackColor = Color.Red;
            GetMyDelegateRes = new myDelegateRes(showRes);
            frmOneCamera.myRunControlDelegate += new runControlDelegate(runControl);
        }
        private void showRes(string allnum, string oknum, string ngnum, string ct, string res, Color color)
        {
            lbAllNum.Text = allnum;
            lbOKNum.Text = oknum;
            lbNGNum.Text = ngnum;
            lbOKPercent.Text = (OKNum / AllNum * 100).ToString("F2") + "%";
            lbCt.Text = ct;
            lbResult.Text = res;
            lbResult.BackColor = color;
        }
        private void showImformation1()
        {
            string res;
            Color color;
            if (HTCSharpDemo.Program.quexians1.Count==0)
            {
                if(HTCSharpDemo.Program.Result1.dingweiNum1==1)
                {
                    if (HTCSharpDemo.Program.Result1.fenleis1.Contains("OK"))
                    {
                        res = "OK";
                        color = Color.Green;
                    }
                    else
                    {
                        res = "NG";
                        color = Color.Red;
                    }
                }
                else
                {
                    res = "定位失败";
                    color = Color.Red;
                }
            }
            else
            {
                res = "NG";
                color = Color.Red;
            }
            lbCt.BeginInvoke(GetMyDelegateRes, AllNum.ToString(), OKNum.ToString(), NGNum.ToString(), ct.ToString(), res, color);
        }
        private void showImformation2()
        {
            string res;
            Color color;
            if (HTCSharpDemo.Program.quexians1.Count==0&& HTCSharpDemo.Program.quexians2.Count == 0)
            {
                if (HTCSharpDemo.Program.Result1.dingweiNum1 == 1)
                {
                    res = "OK";
                    color = Color.Green;
                }
                else
                {
                    res = "定位失败";
                    color = Color.Red;
                }
            }
            else
            {
                res = "NG";
                color = Color.Red;
            }
            lbCt.BeginInvoke(GetMyDelegateRes, AllNum.ToString(), OKNum.ToString(), NGNum.ToString(), ct.ToString(), res, color);
        }
        private void closeImformation()
        {
            this.Close();
        }


        private void frmImformation_FormClosed(object sender, FormClosedEventArgs e)
        {
            frmOneCamera.myShowDelegate -= new showOneDelegate(showImformation1);
            frmOneCamera.myCloseTableDelegate -= new closeOneDelegate(closeImformation);
            frmTwoCamera.myShowTwoDelegate -= new showTwoDelegate(showImformation2);
            frmTwoCamera.myCloseTwoDelegate -= new closeTwoDelegate(closeImformation);
            frmOneCamera.myRunControlDelegate -= new runControlDelegate(runControl);
        }

        private void 清除数据ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("确认是否清除?", "警告", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
            {
                LogHelper.WriteLog($"已清空历史数据,总数:{lbAllNum.Text};OK数:{lbOKNum.Text};NG数:{lbNGNum.Text};良率:{lbOKPercent.Text}");
                lbAllNum.Text = "0";
                lbOKNum.Text = "0";
                lbNGNum.Text = "0";
                lbOKPercent.Text = "0";
                lbCt.Text = "0";
                lbResult.BackColor = Color.Red;
                lbResult.Text = "NG";
                IniHelper.SaveSetIni.Write("生产信息", "生产总数", "0");
                IniHelper.SaveSetIni.Write("生产信息", "OK总数", "0");
                IniHelper.SaveSetIni.Write("生产信息", "NG总数", "0");
            }
        }

        private void runControl()
        {
            if (!frmMain.m_bPause && !frmMain.m_bResume)
                this.panel1.BeginInvoke(new Action(() => {contextMenuStrip1.Enabled = false;}));
            if (frmMain.m_bPause && !frmMain.m_bResume)
                this.panel1.BeginInvoke(new Action(() =>{contextMenuStrip1.Enabled = true;}));
        }
    }
}
