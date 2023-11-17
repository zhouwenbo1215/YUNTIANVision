using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YUNTIANVision
{
    public delegate void cameraExposeSet(float exposeTime,string camName);
    public delegate void cameraExposeClose();
    public partial class frmCameraExposeSet : Form
    {
        public static event cameraExposeSet cameraExposeSetEvent;
        public static event cameraExposeClose cameraExposeCloseEvent;
        string CamName;
        public frmCameraExposeSet(string camName)
        {
            InitializeComponent();
            this.CamName = camName;
        }


        private void btSetExpose_Click(object sender, EventArgs e)
        {
            if(String.IsNullOrEmpty(tbExpose.Text))
            {
                MessageBox.Show("请输入需要设置的曝光时间");
                return;
            }
            if(!Regex.IsMatch(tbExpose.Text, @"^\d+(\.\d+)?$"))
            {
                MessageBox.Show("请输入正确的数值");
                return;
            }
                    cameraExposeSetEvent?.Invoke(float.Parse(tbExpose.Text),CamName);
        }

        private void frmCameraExposeSet_Load(object sender, EventArgs e)
        {
            switch (CamName)
            {
                case "单相机":
                    tbExpose.Text = IniHelper.SaveSetIni.Read("相机设置", "曝光量");
                    break;
                case "相机1":
                    tbExpose.Text = IniHelper.SaveSetIni.Read("相机1设置", "曝光量");
                    break;
                case "相机2":
                    tbExpose.Text = IniHelper.SaveSetIni.Read("相机2设置", "曝光量");
                    break;
                default:
                    break;
            }
        }

        private void frmCameraExposeSet_FormClosing(object sender, FormClosingEventArgs e)
        {
            cameraExposeCloseEvent?.Invoke();
        }
    }
}
