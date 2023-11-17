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
using YUNTIANVision.Properties;
using System.IO;

namespace YUNTIANVision
{
    public partial class frmReg : Form
    {
        public frmReg()
        {
            InitializeComponent();
        }

        
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            pictureBox1.Image = new Bitmap(Resources.显示);
            textBox3.PasswordChar = (char)0;
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            pictureBox1.Image = new Bitmap(Resources.隐藏_);
            textBox3.PasswordChar = '*';
        }

        private void frmReg_Load(object sender, EventArgs e)
        {
            textBox2.Focus();
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

        private void button1_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(textBox2.Text) || String.IsNullOrEmpty(textBox3.Text))
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
                    command.CommandText = "select * from userinfo where 用户名=@用户名";
                    command.Parameters.Add(new SQLiteParameter("@用户名", textBox2.Text.Trim()));
                    object rst = command.ExecuteScalar();
                    if (rst != null)
                    {
                        MessageBox.Show("用户名已存在");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    LogHelper.WriteLog(ex.Message);
                }
                try
                {
                    SQLiteCommand command = new SQLiteCommand();
                    command.Connection = SqlHelper.Connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = "insert into userinfo(用户名,密码) values(@用户名,@密码)";
                    command.Parameters.Add(new SQLiteParameter("@用户名",textBox2.Text.Trim()));
                    command.Parameters.Add(new SQLiteParameter("@密码", textBox3.Text.Trim()));
                    command.ExecuteScalar();
                    MessageBox.Show("注册成功");
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    LogHelper.WriteLog(ex.Message);
                }
            }
        }

        private void frmReg_FormClosed(object sender, FormClosedEventArgs e)
        {
            closeDb();
        }
    }
}
