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
using System.Windows.Media;
using static YUNTIANVision.frmTwoCamera;
using System.IO;

namespace YUNTIANVision
{
    public delegate void clearSqlDelegate2();
    public partial class frmTwoImformation : Form
    {
        public static event clearSqlDelegate2 clearSqlEvent;
        public frmTwoImformation()
        {
            InitializeComponent();
            frmTwoCamera.myShowTwoDelegate1 += new showTwoDelegate1(showImformation1);
            frmTwoCamera.myShowTwoDelegate2 += new showTwoDelegate2(showImformation2);
            frmTwoCamera.myCloseTwoDelegate += new closeTwoDelegate(closeImformation);
            frmMain.controlEvent += new controlDelegate(runControl);
        }
        /// <summary>
        /// 产品总数
        /// </summary>
        public static double AllNum1 = 0;
        /// <summary>
        /// 产品图像处理时间
        /// </summary>
        public static int ct1;
        /// <summary>
        /// OK总数
        /// </summary>
        public static double OKNum1 = 0;
        /// <summary>
        /// NG总数
        /// </summary>
        public static double NGNum1 = 0;
        /// <summary>
        /// 产品总数
        /// </summary>
        public static double AllNum2 = 0;
        /// <summary>
        /// 产品图像处理时间
        /// </summary>
        public static int ct2;
        /// <summary>
        /// OK总数
        /// </summary>
        public static double OKNum2 = 0;
        /// <summary>
        /// NG总数
        /// </summary>
        public static double NGNum2 = 0;
        private delegate void myDelegateRes1(string allnum, string oknum, string ngnum, string ct, string res, System.Drawing.Color color);
        private myDelegateRes1 GetMyDelegateRes1;
        private delegate void myDelegateRes2(string allnum, string oknum, string ngnum, string ct, string res, System.Drawing.Color color);
        private myDelegateRes2 GetMyDelegateRes2;
        private void showRes1(string allnum, string oknum, string ngnum, string ct, string res, System.Drawing.Color color)
        {
            lbAllNum1.Text = allnum;
            lbOKNum1.Text = oknum;
            lbNGNum1.Text = ngnum;
            lbOKPercent1.Text = (Convert.ToInt32(oknum) / Convert.ToInt32(allnum) * 100).ToString("F2") + "%";
            lbCt1.Text = ct;
            lbResult1.Text = res;
            lbResult1.BackColor = color;
        }
        private void showRes2(string allnum, string oknum, string ngnum, string ct, string res, System.Drawing.Color color)
        {
            lbAllNum2.Text = allnum;
            lbOKNum2.Text = oknum;
            lbNGNum2.Text = ngnum;
            lbOKPercent2.Text = (Convert.ToInt32(oknum) / Convert.ToInt32(allnum) * 100).ToString("F2") + "%";
            lbCt2.Text = ct;
            lbResult2.Text = res;
            lbResult2.BackColor = color;
        }
        private void showImformation1()
        {
            string res;
            System.Drawing.Color color;
            if (frmTwoCamera.result1=="OK")
            {
                res = "OK";
                color = System.Drawing.Color.Green;
            }
            else
            {
                res = "NG";
                color = System.Drawing.Color.Red;
            }
            lbCt1.BeginInvoke(GetMyDelegateRes1,AllNum1.ToString(), OKNum1.ToString(), NGNum1.ToString(), ct1.ToString(), res, color);
        }
        private void showImformation2()
        {
            string res;
            System.Drawing.Color color;
            if (frmTwoCamera.result2=="OK")
            {
                res = "OK";
                color = System.Drawing.Color.Green;
            }
            else
            {
                res = "NG";
                color = System.Drawing.Color.Red;
            }
            lbCt2.BeginInvoke(GetMyDelegateRes2, AllNum2.ToString(), OKNum2.ToString(), NGNum2.ToString(), ct2.ToString(), res, color);
        }
        private void frmTwoImformation_Load(object sender, EventArgs e)
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
            GetMyDelegateRes1 = new myDelegateRes1(showRes1);
            GetMyDelegateRes2= new myDelegateRes2(showRes2);
            
            if (!frmMain.m_bPause && !frmMain.m_bResume)
            {
                btHandTest.Enabled = false;
                btClearDate.Enabled = true;
            }
            else if (frmMain.m_bPause && !frmMain.m_bResume)
            {
                btHandTest.Enabled = true;
                btClearDate.Enabled = false;
            }
        }
        private void closeImformation()
        {
            this.Close();
        }

        private void frmTwoImformation_FormClosed(object sender, FormClosedEventArgs e)
        {
            frmTwoCamera.myShowTwoDelegate1 -= new showTwoDelegate1(showImformation1);
            frmTwoCamera.myShowTwoDelegate2 -= new showTwoDelegate2(showImformation2);
            frmTwoCamera.myCloseTwoDelegate -= new closeTwoDelegate(closeImformation);
            frmMain.controlEvent -= new controlDelegate(runControl);
        }
        private void runControl(string str)
        {
            if (str == "继续开始自动运行...")
                this.groupBox1.BeginInvoke(new Action(() => { btHandTest.Enabled = true;
                    btClearDate.Enabled = false;
                }));
            if (str == "停止...")
                this.groupBox1.BeginInvoke(new Action(() => {
                    btHandTest.Enabled = false;
                    btClearDate.Enabled = true;
                }));
        }
        /// <summary>
        /// 手动触发拍照
        /// </summary>
        public static bool isImage;
        private void button1_Click(object sender, EventArgs e)
        {
            if(!frmTwoCamera.m_bRunThread1||!frmTwoCamera.m_bRunThread2)
            {
                MessageBox.Show("请先启动");
                return;
            }
            isImage = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("确认是否清除?", "警告", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
            {
                LogHelper.WriteLog($"已清空历史数据,总数:{lbAllNum1.Text};OK数:{lbOKNum1.Text};NG数:{lbNGNum1.Text};良率:{lbOKPercent1.Text}");
                lbAllNum1.Text = "0";
                lbOKNum1.Text = "0";
                lbNGNum1.Text = "0";
                lbOKPercent1.Text = "0";
                lbCt1.Text = "0";
                lbResult1.BackColor = System.Drawing.Color.Red;
                lbResult1.Text = "NG";
                IniHelper.SaveSetIni.Write("相机1生产信息", "生产总数", "0");
                IniHelper.SaveSetIni.Write("相机1生产信息", "OK总数", "0");
                IniHelper.SaveSetIni.Write("相机1生产信息", "NG总数", "0");
                LogHelper.WriteLog($"已清空历史数据,总数:{lbAllNum2.Text};OK数:{lbOKNum2.Text};NG数:{lbNGNum2.Text};良率:{lbOKPercent2.Text}");
                lbAllNum2.Text = "0";
                lbOKNum2.Text = "0";
                lbNGNum2.Text = "0";
                lbOKPercent2.Text = "0";
                lbCt2.Text = "0";
                lbResult2.BackColor = System.Drawing.Color.Red;
                lbResult2.Text = "NG";
                IniHelper.SaveSetIni.Write("相机2生产信息", "生产总数", "0");
                IniHelper.SaveSetIni.Write("相机2生产信息", "OK总数", "0");
                IniHelper.SaveSetIni.Write("相机2生产信息", "NG总数", "0");
                loadDb();
                loadTable();
            }
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

        private void loadTable()
        {
            if (SqlHelper.ExistTable("TwoCameraNGTableOne", SqlHelper.DBPath) )
            {
                try
                {
                    string query = @"delete from TwoCameraNGTableOne";
                    SQLiteCommand cmd1 = new SQLiteCommand(query);
                    cmd1.Connection = SqlHelper.Connection;
                    cmd1.CommandType = CommandType.Text;
                    cmd1.CommandText = query;
                    cmd1.ExecuteNonQuery();

                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog(ex.ToString());
                    MessageBox.Show(ex.ToString());
                }
            }
            if (SqlHelper.ExistTable("TwoCameraNGTableTwo", SqlHelper.DBPath))
            {

                string query1 = @"delete from TwoCameraNGTableTwo";
                SQLiteCommand cmd2 = new SQLiteCommand(query1);
                cmd2.Connection = SqlHelper.Connection;
                cmd2.CommandType = CommandType.Text;
                cmd2.CommandText = query1;
                cmd2.ExecuteNonQuery();
                clearSqlEvent?.Invoke();
            }
        }
    }
}
