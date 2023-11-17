using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static YUNTIANVision.TreeFileHelper;
using System.Xml.Linq;
using System.Data.SQLite;

namespace YUNTIANVision
{
    public delegate void newConfigDel(List<string> paths);
    public delegate void nowisDebugMode(bool isDebug);
    public delegate void continueDebugMode(bool isContinueDebug);
    public partial class frmSet : Form
    {
        public static event newConfigDel newConfigEvent;
        public static event nowisDebugMode nowisDebugMode;
        public static event continueDebugMode continueDebugMode;
        string s_HTdlFilePath1, s_HTdlFilePath2;
        bool m_HTdlFileChanged = false;
        public frmSet()
        {
            InitializeComponent();
        }
        private void frmPathSet_Load(object sender, EventArgs e)
        {
            textBox2.Text = IniHelper.SaveSetIni.Read("图片设置", "保存路径");
            tb_dl_path1.Text = IniHelper.SaveSetIni.Read("深度学习文件", "配置文件路径");
            s_HTdlFilePath1 = tb_dl_path1.Text;
            tb_dl_path2.Text = IniHelper.SaveSetIni.Read("深度学习文件", "配置文件路径1");
            s_HTdlFilePath2 = tb_dl_path2.Text;
            textBox7.Text = IniHelper.SaveSetIni.Read("自动删除图片", "天数");
            string ok = IniHelper.SaveSetIni.Read("图片路径", "OK图片标志");
            string ng = IniHelper.SaveSetIni.Read("图片路径", "NG图片标志");
            string debugStr = IniHelper.SaveSetIni.Read("测试模式", "测试模式");
            if (ok == "T")
                checkBox1.Checked = true;
            if (ok == "F")
                checkBox1.Checked = false;
            if (ng == "T")
                checkBox2.Checked = true;
            if (ng == "F")
                checkBox2.Checked = false;
            if (string.IsNullOrEmpty(debugStr) || debugStr.Equals("F"))
                checkBox3.Checked = false;
            else if (debugStr.Equals("T"))
                checkBox3.Checked = true;
            comboBox1.Items.Add("原图");
            comboBox1.Items.Add("渲染图");
            string imageType = IniHelper.SaveSetIni.Read("图片设置","保存类型");
            if(String.IsNullOrEmpty(imageType))
                comboBox1.SelectedIndex = 0;
            else
            {
                if (imageType == "原图")
                    comboBox1.SelectedIndex = 0;
                else
                    comboBox1.SelectedIndex = 1;
            }
            day_dateTimePicker.Text = IniHelper.SaveSetIni.Read("数据自动清零", "白天清零时间");
            night_dateTimePicker.Text = IniHelper.SaveSetIni.Read("数据自动清零", "晚上清零时间");
            bool isUseData = false;
            string temp = IniHelper.SaveSetIni.Read("数据自动清零", "数据清零生效");
            if (temp == "T") isUseData = true;
            else if (temp == "F") isUseData = false;
            cb_dataAutoClear.Checked = isUseData;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(textBox2.Text) || String.IsNullOrEmpty(tb_dl_path1.Text) || String.IsNullOrEmpty(textBox7.Text))
            {
                MessageBox.Show("请填写好路径跟自动删除图片时间");
                return;
            }
            if (!Directory.Exists(textBox2.Text))
            {
                bool m_success = true;
                try
                {
                    Directory.CreateDirectory(textBox2.Text);
                }catch(Exception ex)
                {
                    m_success = false;
                    MessageBox.Show("存图路径:" + textBox2.Text + " 不存在");
                }
                if(m_success)
                    IniHelper.PictureOnTimePresssIni.Write("设置", "原始图片路径", textBox2.Text);
            }
            if(!File.Exists(tb_dl_path1.Text))
            {
                MessageBox.Show("文件路径不存在，请重新设置");
                return;
            }
            IniHelper.SaveSetIni.Write("图片设置", "保存路径", textBox2.Text);
            if (!string.IsNullOrEmpty(tb_dl_path1.Text.Trim()))
                IniHelper.SaveSetIni.Write("深度学习文件", "配置文件路径", tb_dl_path1.Text);
            if (!string.IsNullOrEmpty(tb_dl_path2.Text.Trim()))
                IniHelper.SaveSetIni.Write("深度学习文件", "配置文件路径1", tb_dl_path2.Text);
            IniHelper.SaveSetIni.Write("自动删除图片", "天数", textBox7.Text);
            IniHelper.SaveSetIni.Write("图片设置","保存类型",comboBox1.Text);
            if (checkBox1.Checked)
                IniHelper.SaveSetIni.Write("图片路径", "OK图片标志", "T");
            else
                IniHelper.SaveSetIni.Write("图片路径", "OK图片标志", "F");
            if (checkBox2.Checked)
                IniHelper.SaveSetIni.Write("图片路径", "NG图片标志", "T");
            else
                IniHelper.SaveSetIni.Write("图片路径", "NG图片标志", "F");

            bool b_changemodel = false;
            List<string> newDLfilePaths = new List<string>();
            if (m_HTdlFileChanged)
            {
                if (!string.IsNullOrEmpty(tb_dl_path1.Text.Trim()))
                {
                    newDLfilePaths.Add(tb_dl_path1.Text.Trim());
                    b_changemodel = true;
                }
            }
            if (m_HTdlFileChanged) //s_HTdlFilePath2
            {
                if (!string.IsNullOrEmpty(tb_dl_path2.Text.Trim()))
                {
                    newDLfilePaths.Add(tb_dl_path2.Text.Trim());
                    b_changemodel = true;
                }
            }
            m_HTdlFileChanged = false;
            if (newDLfilePaths.Count() > 0)
                newConfigEvent?.Invoke(newDLfilePaths);

            if (b_changemodel)
                MessageBox.Show("请重新设置NG信号");
            if(!string.IsNullOrEmpty(day_dateTimePicker.Value.ToString("HH:mm:ss")))
                IniHelper.SaveSetIni.Write("数据自动清零", "白天清零时间", day_dateTimePicker.Value.ToString("HH:mm:ss"));
            if(!string.IsNullOrEmpty(night_dateTimePicker.Value.ToString("HH:mm:ss")))
                IniHelper.SaveSetIni.Write("数据自动清零", "晚上清零时间", night_dateTimePicker.Value.ToString("HH:mm:ss"));
            this.Close();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                this.textBox2.Text = folderBrowserDialog1.SelectedPath;
                //if (!string.IsNullOrEmpty(this.textBox2.Text))
                //    IniHelper.PictureOnTimePresssIni.Write("设置", "原始图片路径", textBox2.Text);
            }
        }
        private void button7_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "tree文件|*.tree";
            openFileDialog1.Title = "设置配置文件路径";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                this.tb_dl_path1.Text = openFileDialog1.FileName;
                //更改了路径
                m_HTdlFileChanged = true;
            }
        }

        private void cb_dataAutoClear_CheckedChanged(object sender, EventArgs e)
        {
            if (cb_dataAutoClear.Checked)
            {
                IniHelper.SaveSetIni.Write("数据自动清零", "数据清零生效", "T");
            }
            else
            {
                IniHelper.SaveSetIni.Write("数据自动清零", "数据清零生效", "F");
            }
        }

        private void checkBox3_Click(object sender, EventArgs e)
        {
            nowisDebugMode?.Invoke(checkBox3.Checked);
        }

        private void checkBox4_Click(object sender, EventArgs e)
        {
            if(checkBox4.Checked)
            {
                nowisDebugMode?.Invoke(true);
                continueDebugMode?.Invoke(true);
            }
            else
            {
                nowisDebugMode?.Invoke(false);
                continueDebugMode?.Invoke(false);
            }       
        }

        private void bt_dl_path2_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "tree文件|*.tree";
            openFileDialog1.Title = "设置配置文件路径";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                this.tb_dl_path2.Text = openFileDialog1.FileName;
                //更改了路径
                m_HTdlFileChanged = true;
            }
        }
    }
}
