using MvCamCtrl.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YUNTIANVision
{
    public class CameraData
    {
        public int RowNumber { get; set; }
        public string CameraName { get; set; }
        public string SerialPort { get; set; }
        public MyCamera.MV_CC_DEVICE_INFO cameraInfo { get; set; }
    }
}
