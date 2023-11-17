using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YUNTIANVision
{
    public delegate void delegateSavePath();
    public partial class frmPathSet : Form
    {
        public static event delegateSavePath mySavePathEvent;
        public frmPathSet()
        {
            InitializeComponent();
        }
        private void frmPathSet_Load(object sender, EventArgs e)
        {
            textBox2.Text = IniHelper.SaveSetIni.Read("图片设置", "保存路径");
            textBox6.Text = IniHelper.SaveSetIni.Read("深度学习文件", "配置文件路径");
            textBox7.Text = IniHelper.SaveSetIni.Read("自动删除图片", "天数");
            string ok = IniHelper.SaveSetIni.Read("图片路径", "OK图片标志");
            string ng = IniHelper.SaveSetIni.Read("图片路径", "NG图片标志");
            if (ok == "T")
                checkBox1.Checked = true;
            if (ok == "F")
                checkBox1.Checked = false;
            if (ng == "T")
                checkBox2.Checked = true;
            if (ng == "F")
                checkBox2.Checked = false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(textBox2.Text) || String.IsNullOrEmpty(textBox6.Text) || String.IsNullOrEmpty(textBox7.Text))
            {
                MessageBox.Show("请填写好路径跟自动删除图片时间");
                return;
            }
            IniHelper.SaveSetIni.Write("图片设置", "保存路径", textBox2.Text);
            IniHelper.SaveSetIni.Write("深度学习文件", "配置文件路径", textBox6.Text);
            IniHelper.SaveSetIni.Write("自动删除图片", "天数", textBox7.Text);
            if (checkBox1.Checked)
                IniHelper.SaveSetIni.Write("图片路径", "OK图片标志", "T");
            else
                IniHelper.SaveSetIni.Write("图片路径", "OK图片标志", "F");
            if (checkBox2.Checked)
                IniHelper.SaveSetIni.Write("图片路径", "NG图片标志", "T");
            else
                IniHelper.SaveSetIni.Write("图片路径", "NG图片标志", "F");
            mySavePathEvent?.Invoke();
            TreeFileHelper.LoadTreeFile(IniHelper.SaveSetIni.Read("深度学习文件", "配置文件路径"));
            this.Close();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                this.textBox2.Text = folderBrowserDialog1.SelectedPath;
        }
        private void button7_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "tree文件|*.tree";
            openFileDialog1.Title = "设置配置文件路径";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
                this.textBox6.Text = openFileDialog1.FileName;
        }

    }
}
