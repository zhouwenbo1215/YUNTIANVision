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
using System.Data.SQLite;
using System.IO;

namespace YUNTIANVision
{
    public partial class frmUserLog : Form
    {
        public frmUserLog()
        {
            InitializeComponent();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            frmReg reg = new frmReg();
            reg.ShowDialog();
        }
        Dictionary<string, string> UserList =new Dictionary<string, string>();
        private void button1_Click(object sender, EventArgs e)
        {
            UserList.Clear();
            if(String.IsNullOrEmpty(textBox1.Text)||String.IsNullOrEmpty(textBox3.Text))
            {
                MessageBox.Show("用户名或密码为空");
            }
            else
            {
                try
                {
                    SQLiteCommand command = new SQLiteCommand();
                    command.Connection = SqlHelper.Connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = "select * from userinfo";
                    SQLiteDataAdapter sda = new SQLiteDataAdapter(command);
                    DataTable dataTable = new DataTable();
                    sda.Fill(dataTable);
                    foreach (DataRow item in dataTable.Rows)
                    {
                        string name = item["用户名"].ToString();
                        string pass = item["密码"].ToString();
                        UserList.Add(name,pass) ;
                    }
                    if (UserList.ContainsKey(textBox1.Text) && UserList[textBox1.Text]==textBox3.Text)
                    {
                        IniHelper.SaveSetIni.Write("操作员","账户",textBox1.Text);
                        this.Hide();
                        frmMain main = new frmMain();
                        main.Show();
                    }
                    else
                    {
                        MessageBox.Show("账号或者密码错误");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    LogHelper.WriteLog(ex.Message) ;
                }
                
            }
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            pictureBox1.Image = new Bitmap(Resources.显示);
            textBox3.PasswordChar=(char)0;
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            pictureBox1.Image = new Bitmap(Resources.隐藏_);
            textBox3.PasswordChar = '*';
        }

        private void frmUserLog_Load(object sender, EventArgs e)
        {
            textBox1.Focus();
            textBox1.Text = IniHelper.SaveSetIni.Read("操作员", "账户");
            loadDb();
            loadTable();
        }
        #region 数据库打开关闭

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

        private void closeDb()
        {
            if (SqlHelper.Connection.State == ConnectionState.Open)
                SqlHelper.Connection.Close();
        }

        #endregion

        private void loadTable()
        {
            if (!SqlHelper.ExistTable("userinfo", SqlHelper.DBPath))
            {
                try
                {
                    SQLiteCommand command = new SQLiteCommand();
                    command.Connection = SqlHelper.Connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = "create table userinfo(用户名 varchar(10),密码 varchar(10))";
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog(ex.ToString());
                    MessageBox.Show(ex.ToString());
                }
            }
        }
    }
}
