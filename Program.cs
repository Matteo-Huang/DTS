using System;
using System.Collections.Generic;
using System.Linq;
//using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using  System.Diagnostics;

namespace DST_CLIENT
{
    static class Program
    {
        /// <summary>
        /// 應用程式的主入口點。
        /// </summary>
        [STAThread]
        static void Main()
        {
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Client());

             //get the name of our process
            string proc = Process.GetCurrentProcess().ProcessName;
             //get the list of all processes by that name
            Process[] processes = Process.GetProcessesByName(proc);
            // if there is more than one process
            if (processes.Length > 1)
            {
                //MessageBox.Show("Application is already running");
                return;
            }
            else
                Application.Run(new Client());
        }
    }
}
