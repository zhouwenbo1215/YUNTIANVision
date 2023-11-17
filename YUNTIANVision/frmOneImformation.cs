using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static YUNTIANVision.frmTwoCamera;
using System.IO;

namespace YUNTIANVision
{
    public delegate void clearSqlDelegate1();
    public partial class frmOneImformation : Form
    {
        public static event clearSqlDelegate1 clearSqlEvent;
        public frmOneImformation()
        {
            InitializeComponent();
            frmOneCamera.myShowDelegate += new showOneDelegate(showImformation1);
            frmOneCamera.myCloseTableDelegate += new closeOneDelegate(closeImformation);
            frmMain.controlEvent += new controlDelegate(runControl);
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
            lbResult.BackColor = Color.Red;
            GetMyDelegateRes = new myDelegateRes(showRes);
            if (frmMain.m_bPause && !frmMain.m_bResume)
                button1.Enabled = false;
            else
                button1.Enabled = true;
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
            if (frmOneCamera.result=="OK")
            {
                res = "OK";
                color = Color.Green;
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
            frmMain.controlEvent -= new controlDelegate(runControl);
        }

        private void runControl(string str)
        {
            if (str=="继续开始自动运行...")
                this.BeginInvoke(new Action(() => {button2.Enabled = false;
                    button1.Enabled = true;
                }));
            else if (str == "停止...")
                this.BeginInvoke(new Action(() =>{button2.Enabled = true;
                    button1.Enabled = false;
                }));
        }
        private void loadDb()
        {
            SqlHelper.DBPath = AppDomain.CurrentDomain.BaseDirectory + "YT.db";
            string strConnectString = "data source=" + SqlHelper.DBPath;
            if (File.Exists(SqlHelper.DBPath))
            {
                try
                {
                    SqlHelper.Connection = new SQLiteConnection(strConnectString);
                    SqlHelper.Connection.Open();
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog(ex.ToString());
                    MessageBox.Show(ex.ToString());
                }
            }
            else
            {
                try
                {
                    SQLiteConnection.CreateFile(SqlHelper.DBPath);
                    SqlHelper.Connection = new SQLiteConnection(strConnectString);
                    SqlHelper.Connection.Open();
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog(ex.ToString());
                    MessageBox.Show(ex.ToString());
                }
            }
        }
        string configPath = IniHelper.SaveSetIni.Read("深度学习文件", "配置文件路径");
        private void loadTable()
        {
            if (SqlHelper.ExistTable("NGTable", SqlHelper.DBPath))
            {
                try
                {
                    string query = @"delete from NGTable";
                    SQLiteCommand cmd1 = new SQLiteCommand(query);
                    cmd1.Connection = SqlHelper.Connection;
                    cmd1.CommandType = CommandType.Text;
                    cmd1.CommandText = query;
                    cmd1.ExecuteNonQuery();
                    clearSqlEvent?.Invoke();
                   
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog(ex.ToString());
                    MessageBox.Show(ex.ToString());
                }
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("确认是否清除?", "警告", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
            {
                LogHelper.WriteLog($"已清空历史数据,总数:{lbAllNum.Text};OK数:{lbOKNum.Text};NG数:{lbNGNum.Text};良率:{lbOKPercent.Text}");
                lbAllNum.Text = "0";
                AllNum = 0;
                lbOKNum.Text = "0";
                OKNum = 0;
                lbNGNum.Text = "0";
                NGNum = 0;
                lbOKPercent.Text = "0";
                lbCt.Text = "0";
                lbResult.BackColor = Color.Red;
                lbResult.Text = "NG";
                IniHelper.SaveSetIni.Write("单相机生产信息", "生产总数", "0");
                IniHelper.SaveSetIni.Write("单相机生产信息", "OK总数", "0");
                IniHelper.SaveSetIni.Write("单相机生产信息", "NG总数", "0");
                loadDb();
                loadTable();
            }
        }
        /// <summary>
        /// 手动触发拍照
        /// </summary>
        public static bool isImage;
        private void button1_Click(object sender, EventArgs e)
        {
            if(!frmOneCamera.m_bRunThread)
            {
                MessageBox.Show("请先启动");
                return;
            }
            isImage = true;
        }

    }
}
