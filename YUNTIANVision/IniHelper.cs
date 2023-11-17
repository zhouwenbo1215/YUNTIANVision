using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using INI;

namespace YUNTIANVision
{
    internal class IniHelper
    {
        /// <summary>
        /// 应用程序保存数据的INI文件
        /// </summary>
        public static IniFile SaveSetIni = new IniFile("./SaveSetIni.ini");

        /// <summary>
        /// 应用程序定时压缩图片的INI文件
        /// </summary>
        public static IniFile PictureOnTimePresssIni = new IniFile("./PictruePress/config.ini");
    }
}
