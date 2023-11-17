using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YUNTIANVision
{
    internal class LogHelper
    {
        private LogHelper()
        {

        }

        public static ILog logError = LogManager.GetLogger("ErrorLog");
        public static ILog logInfor = LogManager.GetLogger("InforLog");

        public static void SetConfig()
        {
            log4net.Config.XmlConfigurator.Configure();
        }

        public static void SetConfig(FileInfo configFile)
        {
            log4net.Config.XmlConfigurator.Configure(configFile);
        }

        /// <summary>
        /// 写入异常信息日志
        /// </summary>
        /// <param name="info">异常内容</param>
        /// <param name="ex">异常</param>
        public static void WriteLog(string info, Exception ex)
        {
            if (logError.IsErrorEnabled)
            {
                logError.Error(info, ex);
            }
        }

        /// <summary>
        /// 记录普通信息日志
        /// </summary>
        /// <param name="info">信息</param>
        public static void WriteLog(string info)
        {
            if (logInfor.IsErrorEnabled)
            {
                logInfor.Info(info);
            }
        }
    }
}
