using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using NumLockWatch;
using System.Runtime.CompilerServices;

namespace UsbBrowser
{
    static class UsbBrowser
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var form1 = new NumLockWatch.Form1();
            Application.Run();
        }
    }
}
