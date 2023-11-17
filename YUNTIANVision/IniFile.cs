using System;
using System.Text;

using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace INI
{
    //封装的时候，发现大小，路径这些比较麻烦，每次都一样，这样只留一个节和键出来。大小，路径都是固定的，在实例化时传入就好。
    //封装的时候有很多重复的参数，像路径文件名，文件大小，可以封装到方法里面，不用每次都传相同的参数，而且它的名字太长记不住
    //封装方法时可以把方法名简化。
    /// <summary>
    /// 配置文件*.ini读写器。
    /// </summary>
    /// 
    //也可以把System.Runtime.InteropServices它作为命名空间拿出来，少写点代码，也好阅读。  具体来引用可以节省内存
    public class IniFile
    {

        #region API函数声明

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key,
            string val, string filePath);


        //需要调用GetPrivateProfileString的重载
        [DllImport("kernel32", EntryPoint = "GetPrivateProfileString")]
        private static extern long GetPrivateProfileString(string section, string key,
            string def, StringBuilder retVal, int size, string filePath);

        [DllImport("kernel32", EntryPoint = "GetPrivateProfileString")]
        private static extern uint GetPrivateProfileStringA(string section, string key,
            string def, Byte[] retVal, int size, string filePath);

        //[DllImport("kernel32")]
        //private static extern int GetPrivateProfileString(
        //string section, string key, string def, StringBuilder value, int size, string file);

        #endregion


        private string _File = "";//反复使用的相同参数可以作封装。

        private int _Size = 255;

        /// <summary>
        /// 值的最大长度，不能小于0。
        /// </summary>
        public int Size
        {
            get
            {
                return _Size;
            }
            set
            {
                if (value >= 0)
                {
                    _Size = value;
                }
            }

        }

        /// <summary>
        /// 构造函数，值的最大长度默认为255。
        /// </summary>
        /// <param name="file">配置文件。</param>
        public IniFile(string filepath)
        {
            _File = filepath;
        }

        /// <summary>
        /// 构造函数。
        /// </summary>
        /// <param name="file">配置文件。</param>
        /// <param name="size">值的最大长度。</param>
        public IniFile(string filepath, int size)
        {
            _File = filepath;
            Size = size; 
        }

        /// <summary>
        /// 写配置文件。
        /// </summary>
        /// <param name="section">条目。</param>
        /// <param name="key">键。</param>
        /// <param name="value">值。</param>
        public void Write(string section, string key, string value)
        {
            WritePrivateProfileString(section, key, value, _File);
        }

        /// <summary>
        /// 读配置文件，读取失败返回""。
        /// </summary>
        /// <param name="section">条目。</param>
        /// <param name="key">键。</param>
        /// <returns>值。</returns>
        public string Read(string section, string key)//这里发现大小，路径这些比较麻烦，每次都一样，这样只留一个节和键出来。
        {
            StringBuilder value = new StringBuilder(Size);
            GetPrivateProfileString(section, key, "", value, Size, _File);
            return value.ToString();
        }

        //------------------------------------------------------------------------------
        //------------------------------------------------------------------------------
        //https://www.baidu.com/link?url=JIyi3Y6vNZz3sYktWs5X7Rt0ARokl9nRxY-uYeqQx1yF1k_PK19RMdnPLn784AhdGL9vUDf5jf9cMD1eTJnJiq&wd=&eqid=9b55f8ec0007038e00000006629e9f44
        //以下为静态方法，需要全部统一。
        private static string filePath = "";
        public static void SetFilePath(String filepath)
        {
            filePath = filepath;
        }
        #region 读Ini文件

        public static string ReadIniData(string Section, string Key, string NoText)
        {
            return ReadIniData(Section, Key, NoText, filePath);
        }
        public static string ReadIniData(string Section, string Key, string NoText, string iniFilePath)
        {
            if (File.Exists(iniFilePath))
            {
                StringBuilder temp = new StringBuilder(1024);
                GetPrivateProfileString(Section, Key, NoText, temp, 1024, iniFilePath);
                return temp.ToString();
            }
            else
            {
                return String.Empty;
            }
        }

        #endregion

        #region 写Ini文件

        public static bool WriteIniData(string Section, string Key, string Value)
        {
            return WriteIniData(Section, Key, Value, filePath);
        }
        public static bool WriteIniData(string Section, string Key, string Value, string iniFilePath)
        {
            if (File.Exists(iniFilePath))
            {
                long OpStation = WritePrivateProfileString(Section, Key, Value, iniFilePath);
                if (OpStation == 0)
                    return false;
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }
        #endregion
        //------------------------------------------------------------------------------
        //------------------------------------------------------------------------------
        //以下是遍历读取节和键
        //重载Key下面的ReadKeys  为读出节下面的键和健值。存用什么类型数据结构？
        public static List<string> ReadSections()
        {
            return ReadSections(filePath);
        }

        public static List<string> ReadSections(string iniFilename)
        {
            List<string> result = new List<string>();
            Byte[] buf = new Byte[65536];
            uint len = GetPrivateProfileStringA(null, null, null, buf, buf.Length, iniFilename);
            int j = 0;
            for (int i = 0; i < len; i++)
                if (buf[i] == 0)
                {
                    result.Add(Encoding.Default.GetString(buf, j, i - j));
                    j = i + 1;
                }
            return result;
        }

        public static List<string> ReadKeys(String SectionName)
        {
            return ReadKeys(SectionName, filePath);
        }

        public static List<string> ReadKeys(string SectionName, string iniFilename)
        {
            List<string> result = new List<string>();
            Byte[] buf = new Byte[65536];
            uint len = GetPrivateProfileStringA(SectionName, null, null, buf, buf.Length, iniFilename);
            int j = 0;
            for (int i = 0; i < len; i++)
                if (buf[i] == 0)
                {
                    result.Add(Encoding.Default.GetString(buf, j, i - j));
                    j = i + 1;
                }
            return result;
        }
    }
}
