using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YUNTIANVision
{
    internal static class Program
    {
        
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (!HslCommunication.Authorization.SetAuthorizationCode("f562cc4c-4772-4b32-bdcd-f3e122c534e3"))
            {
                MessageBox.Show("授权失败，当前程序只能使用8小时");
                return;
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmMain());
        }
    }
}
