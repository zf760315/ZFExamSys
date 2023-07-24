using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace FactoryHelperManager
{
    public class ProcessHelper
    {
        /// <summary>
        /// 结束进程
        /// </summary>
        /// <param name="processName">进程名称</param>
        public static void KillProcess(string processName)
        {
            Process.GetProcesses().Where(w => w.ProcessName.ToUpper() == processName.ToUpper() && !string.IsNullOrEmpty(w.MainWindowTitle)).ToList().ForEach(delegate (Process proc)
            {
                try
                {
                    proc.Kill();
                    proc.Close();
                }
                catch { }
            });
        }
    }
}
