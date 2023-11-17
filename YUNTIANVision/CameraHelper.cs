using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using HalconDotNet;
using log4net;
using MvCamCtrl.NET;

namespace YUNTIANVision
{
    class HikCameraHelper
    {
        MyCamera[] device;
        MyCamera.MV_CC_DEVICE_INFO_LIST stDevList;
        int nRet = MyCamera.MV_OK;
        MyCamera.MV_CC_DEVICE_INFO stDevInfo; // 通用设备信息
        MyCamera.MVCC_INTVALUE stParam;
        MyCamera.MV_FRAME_OUT_INFO_EX FrameInfo;
        UInt32 nPayloadSize;
        IntPtr pBufForDriver;
        private bool m_bOneStart = false;
        public void enumCamNum(out uint CamNum)
        {
            CamNum = 0;
            // ch:枚举设备 | en:Enum device
            stDevList = new MyCamera.MV_CC_DEVICE_INFO_LIST();
            nRet = MyCamera.MV_CC_EnumDevices_NET(MyCamera.MV_GIGE_DEVICE | MyCamera.MV_USB_DEVICE, ref stDevList);
            frmMain.cameraNum = (int)stDevList.nDeviceNum;
            if (MyCamera.MV_OK != nRet)
            {
                Console.WriteLine("Enum device failed:{0:x8}", nRet);
            }
            else
            {
                CamNum = stDevList.nDeviceNum;
                device = new MyCamera[CamNum];
            }
        }
        public void OpenCamera(uint CamIndex)
        {
            try
            {
                stDevInfo = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(stDevList.pDeviceInfo[CamIndex], typeof(MyCamera.MV_CC_DEVICE_INFO));
                // ch:创建设备 | en:Create device
                nRet = device[CamIndex].MV_CC_CreateDevice_NET(ref stDevInfo);
                if (MyCamera.MV_OK != nRet)
                {
                    Console.WriteLine("Create device failed:{0:x8}", nRet);
                }

                // ch:打开设备 | en:Open device
                nRet = device[CamIndex].MV_CC_OpenDevice_NET();
                if (MyCamera.MV_OK != nRet)
                {
                    Console.WriteLine("Open device failed:{0:x8}", nRet);
                }

                // ch:开启抓图 | en:start grab
                nRet = device[CamIndex].MV_CC_StartGrabbing_NET();
                if (MyCamera.MV_OK != nRet)
                {
                    Console.WriteLine("Start grabbing failed:{0:x8}", nRet);
                }

                // ch:获取包大小 || en: Get Payload Size
                stParam = new MyCamera.MVCC_INTVALUE();
                nRet = device[CamIndex].MV_CC_GetIntValue_NET("PayloadSize", ref stParam);
                if (MyCamera.MV_OK != nRet)
                {
                    m_bOneStart = true;
                }
                nPayloadSize = stParam.nCurValue;
                pBufForDriver = Marshal.AllocHGlobal((int)nPayloadSize);
                FrameInfo = new MyCamera.MV_FRAME_OUT_INFO_EX();
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message);
                MessageBox.Show(ex.Message);
            }
        }
        public void CloseCamera(uint CamIndex)
        {
            try
            {
                Marshal.FreeHGlobal(pBufForDriver);

                // ch:停止抓图 | en:Stop grab image
                nRet = device[CamIndex].MV_CC_StopGrabbing_NET();
                if (MyCamera.MV_OK != nRet)
                {
                    Console.WriteLine("Stop grabbing failed{0:x8}", nRet);
                }

                // ch:关闭设备 | en:Close device
                nRet = device[CamIndex].MV_CC_CloseDevice_NET();
                if (MyCamera.MV_OK != nRet)
                {
                    Console.WriteLine("Close device failed{0:x8}", nRet);
                }

                // ch:销毁设备 | en:Destroy device
                nRet = device[CamIndex].MV_CC_DestroyDevice_NET();
                if (MyCamera.MV_OK == nRet)
                {
                    m_bOneStart = false;
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message);
                MessageBox.Show(ex.Message);
            }
        }
        public bool isOpen()
        {
            return m_bOneStart;
        }
        public void GrabGrayImage(uint CamIndex,out HObject image)
        {
            HOperatorSet.GenEmptyObj(out image);
            try
            {
                nRet = device[CamIndex].MV_CC_GetOneFrameTimeout_NET(pBufForDriver, nPayloadSize, ref FrameInfo, 1000);
                image.Dispose();
                HOperatorSet.GenImage1Extern(out image, "byte", FrameInfo.nWidth, FrameInfo.nHeight, pBufForDriver, 0);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message);
                MessageBox.Show(ex.Message);
            }
        }
        public void GrabRgbImage(uint CamIndex,out HObject image)
        {
            HOperatorSet.GenEmptyObj(out image);
            try
            {
                nRet = device[CamIndex].MV_CC_GetOneFrameTimeout_NET(pBufForDriver, nPayloadSize, ref FrameInfo, 1000);
                image.Dispose();
                HOperatorSet.GenImageInterleaved(out image, pBufForDriver, "rgb", FrameInfo.nWidth, FrameInfo.nHeight,
                    0, "byte", FrameInfo.nWidth, FrameInfo.nHeight, 0, 0, -1, 0);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message);
                MessageBox.Show(ex.Message);
            }
        }
        public void SetPara(uint CamIndex,float expose)
        {
            try
            {
                device[CamIndex].MV_CC_SetEnumValue_NET("ExposureAuto", 0);
                nRet = device[CamIndex].MV_CC_SetFloatValue_NET("ExposureTime", expose);
                if (nRet == MyCamera.MV_OK)
                {
                    LogHelper.WriteLog($"相机曝光设置成功:{expose}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                LogHelper.WriteLog($"相机曝光设置失败:{expose}");
            }
        }
        public void GetPara(uint CamIndex, ref MyCamera.MVCC_FLOATVALUE stParam)
        {
            try
            {
                stParam = new MyCamera.MVCC_FLOATVALUE();
                nRet = device[CamIndex].MV_CC_GetFloatValue_NET("ExposureTime", ref stParam);
                if (MyCamera.MV_OK == nRet)
                {
                    LogHelper.WriteLog($"相机曝光获取成功:{stParam}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                LogHelper.WriteLog($"相机曝光获取失败:{stParam}");
            }
        }
    }
}
