using MvCamCtrl.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace YUNTIANVision
{
    public class DataManagerCenter
    {
        //模型
        private List<ModelData> dataList = new List<ModelData>();

        List<MyCamera.MV_CC_DEVICE_INFO> stDevInfos = CamerasHikvision.getCameraInfos();
    }
}
