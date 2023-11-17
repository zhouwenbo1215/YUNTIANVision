using HalconDotNet;
using MvCamCtrl.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YUNTIANVision
{
    class CameraBase
    {
        // 标识显示窗口
        public HWindowControl showWindow;
        /// <summary>
        /// 图片左上角row坐标
        /// </summary>
        public double row_left;
        /// <summary>
        /// 图片左上角column坐标
        /// </summary>
        public double column_left;
        /// <summary>
        /// 图片右下角row坐标
        /// </summary>
        public double row_right;
        /// <summary>
        /// 图片右下角column坐标
        /// </summary>
        public double column_right;
        /// <summary>
        /// 相机拍到的图像变量
        /// </summary>
        public HObject ho_image;
        /// <summary>
        /// ho_image的宽
        /// </summary>
        public HTuple hv_Width = new HTuple();
        /// <summary>
        /// ho_image的宽
        /// </summary>
        public HTuple hv_Height = new HTuple();
        public MyCamera cam_device = new MyCamera();
        public int nRet = MyCamera.MV_OK;
        /// <summary>
        /// 通用设备信息
        /// </summary>
        public MyCamera.MV_CC_DEVICE_INFO stDevInfo;
        public MyCamera.MVCC_INTVALUE stParam;
        public MyCamera.MV_FRAME_OUT_INFO_EX FrameInfo;
        public UInt32 nPayloadSize;
        public IntPtr pBufForDriver;
    }
}
