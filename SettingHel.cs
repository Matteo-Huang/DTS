using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace DST_CLIENT
{
    public static class SettingHel
    {
        /// <summary>
        /// 設置應用程序開機自動運行
        /// </summary>
        /// <param name="fileName">應用程序的文件名</param>
        /// <param name="isAutoRun">是否自動運行,為false時，取消自動運行</param>
        /// <exception cref="system.Exception">設置不成功時抛出異常</exception>
        /// <returns>返回1成功，非1不成功</returns>
        public static String SetAutoRun(string fileName, bool isAutoRun)
        {
            string reSet = string.Empty;
            RegistryKey reg = null;
            try
            {
                if (!System.IO.File.Exists(fileName))
                {
                    reSet = "該文件不存在!";
                }
                string name = fileName.Substring(fileName.LastIndexOf(@"\") + 1);
                reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                if (reg == null)
                {
                    reg = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");
                }
                if (isAutoRun)
                {
                    reg.SetValue(name, fileName);
                    reSet = "1";
                }
                else
                {
                    reg.SetValue(name, false);
                }

            }
            catch (Exception ex)
            {
                //“記錄異常”
            }
            finally
            {
                if (reg != null)
                {
                    reg.Close();
                }
            }
            return reSet;
        }

    }

}
