using MvCamCtrl.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace YUNTIANVision
{
    class CamerasHikvisionManagement
    {
        /// <summary>
        /// 所有相机
        /// </summary>
        private List<CamerasBase> cameraList = new List<CamerasBase>();

        private Dictionary<string,int> cameraShowWindow = new Dictionary<string,int>();

        /// <summary>
        /// 获取所有海康相机
        /// </summary>
        public void SearchAllCameras()
        {
            MyCamera.MV_CC_DEVICE_INFO_LIST mDeviceList = new MyCamera.MV_CC_DEVICE_INFO_LIST();
            if (MyCamera.MV_CC_EnumDevices_NET(MyCamera.MV_GIGE_DEVICE | MyCamera.MV_USB_DEVICE, ref mDeviceList) != 0)
            {
                MessageBox.Show("查找设备失败");
                return;
            }
            // ch:在窗体列表中显示设备名 | en:Display device name in the form list
            for (int i = 0; i < mDeviceList.nDeviceNum; i++)
            {
                CamerasBase _camerasBase = new CamerasBase();
                MyCamera.MV_CC_DEVICE_INFO device = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(mDeviceList.pDeviceInfo[i], typeof(MyCamera.MV_CC_DEVICE_INFO));
                if (device.nTLayerType == MyCamera.MV_GIGE_DEVICE)
                {
                    IntPtr buffer = Marshal.UnsafeAddrOfPinnedArrayElement(device.SpecialInfo.stGigEInfo, 0);
                    MyCamera.MV_GIGE_DEVICE_INFO gigeInfo = (MyCamera.MV_GIGE_DEVICE_INFO)Marshal.PtrToStructure(buffer, typeof(MyCamera.MV_GIGE_DEVICE_INFO));
                    if (gigeInfo.chUserDefinedName != "")
                    {
                        _camerasBase.mCameraNo = "Hikvision: " + gigeInfo.chUserDefinedName + " (" + gigeInfo.chSerialNumber + ")";
                    }
                    else
                    {
                        _camerasBase.mCameraNo = "Hikvision: " + gigeInfo.chManufacturerName + " " + gigeInfo.chModelName + " (" + gigeInfo.chSerialNumber + ")";
                    }
                    _camerasBase.mSerialNo = gigeInfo.chSerialNumber;
                }
                else if (device.nTLayerType == MyCamera.MV_USB_DEVICE)
                {
                    IntPtr buffer = Marshal.UnsafeAddrOfPinnedArrayElement(device.SpecialInfo.stUsb3VInfo, 0);
                    MyCamera.MV_USB3_DEVICE_INFO usbInfo = (MyCamera.MV_USB3_DEVICE_INFO)Marshal.PtrToStructure(buffer, typeof(MyCamera.MV_USB3_DEVICE_INFO));
                    if (usbInfo.chUserDefinedName != "")
                    {
                        _camerasBase.mCameraNo = "Hikvision: " + usbInfo.chUserDefinedName + " (" + usbInfo.chSerialNumber + ")";
                    }
                    else
                    {
                        _camerasBase.mCameraNo = ("Hikvision: " + usbInfo.chManufacturerName + " " + usbInfo.chModelName + " (" + usbInfo.chSerialNumber + ")");
                    }
                    _camerasBase.mSerialNo = usbInfo.chSerialNumber;
                }

                cameraList.Add(_camerasBase);
            }
        }
        /// <summary>
        /// 根据串口号获取相机信息
        /// </summary>
        public CamerasBase FindBySerailNum(string serialNum)
        {
            if (cameraList.Count == 0)
            {
                return default(CamerasBase);
            }
            for (int i = 0; i < cameraList.Count; i++)
            {
                if (cameraList[i].mSerialNo == serialNum)
                {
                    return cameraList[i];
                }
            }
            return default(CamerasBase);
        }
        /// <summary>
        /// 根据组号获取组内相机信息
        /// </summary>
        public List<CamerasBase> FindByGroupId(int groupId)
        {
            List<CamerasBase> camerasByGroups = new List<CamerasBase>();
            for (int i = 0; i < cameraList.Count; i++)
            {
                if (cameraList[i].groupId == groupId)
                {
                    camerasByGroups.Add(cameraList[i]);
                }
            }
            return camerasByGroups;
        }
    }
}
