using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.ComponentModel;

namespace FactoryHelperManager
{
    public class WindowsHelper
    {

        [DllImport("User32.dll")]
        public static extern bool ShowWindowAsync(IntPtr hWnd, int cmdShow);

        /// <summary>
        /// 设置指定窗口的显示状态。
        /// </summary>
        /// <param name="hWnd">指定的窗口句柄。</param>
        /// <param name="nCmdShow">指定窗口如何显示。</param>
        /// <returns>如果窗口当前可见,则返回值为非零。如果窗口当前被隐藏,则返回值为零。</returns>
        [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "ShowWindow", CharSet = CharSet.Auto)]
        public static extern int ShowWindow(IntPtr hWnd, int nCmdShow);

        /// <summary>
        /// 设置指定窗口的显示状态。
        /// </summary>
        /// <param name="hWnd">指定的窗口句柄。</param>
        /// <param name="nCmdShow">指定窗口如何显示。</param>
        /// <returns>如果窗口当前可见,则返回值为非零。如果窗口当前被隐藏,则返回值为零。</returns>
        [DllImport("user32.dll", EntryPoint = "ShowWindow", CharSet = CharSet.Auto)]
        public static extern int ShowWindow(IntPtr hWnd, SW nCmdShow);

        /// <summary>
        /// 在窗口列表中查找符合指定条件的第一个子窗口。该函数获得一个窗口的句柄,该窗口的类名和窗口名与给定的字符串相匹配。这个函数查找子窗口,从排在给定的子窗口后面的下一个子窗口开始。在查找时不区分大小写。
        /// </summary>
        /// <param name="hwndParent">要查找子窗口的父窗口句柄。如果hwndParent为NULL,则函数以桌面窗口为父窗口,查找桌面窗口的所有子窗口。</param>
        /// <param name="hwndChildAfter">子窗口句柄。查找从在Z序中的下一个子窗口开始。子窗口必须为hwndParent窗口的直接子窗口而非后代窗口。如果HwndChildAfter为NULL,查找从hwndParent的第一个子窗口开始。如果hwndParent和 hwndChildAfter同时为NULL,则函数查找所有的顶层窗口及消息窗口。</param>
        /// <param name="lpszClass">指向一个指定了类名的空结束字符串,或一个标识类名字符串的成员的指针。如果该参数为一个成员,他必须为前次调用theGlobaIAddAtom函数产生的全局成员。该成员为16位,必须位于lpClassName的低16位,高位必须为0。</param>
        /// <param name="lpszWindow">指向一个指定了窗口名(窗口标题)的空结束字符串。如果该参数为NULL,则为所有窗口全匹配。</param>
        /// <returns>找到的窗口的句柄。如未找到符合窗口,则返回零。会设置GetLastError。如果函数成功,返回值为具有指定类名和窗口名的窗口句柄。如果函数失败,返回值为NULL。若想获得更多错误信息,请调用GetLastError函数。</returns>
        [DllImport("user32.dll", EntryPoint = "FindWindowEx")]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        /// <summary>
        ///该函数获得一个顶层窗口的句柄，该窗口的类名和窗口名与给定的字符串相匹配。这个函数不查找子窗口。在查找时不区分大小写
        /// </summary>
        /// <param name="IpClassName">指向一个指定了类名的空结束字符串，或一个标识类名字符串的成员的指针。</param>
        /// <param name="IpWindowName">指向一个指定了窗口名（窗口标题）的空结束字符串。如果该参数为空，则为所有窗口全匹配</param>
        /// <returns>如果函数成功，返回值为具有指定类名和窗口名的窗口句柄；如果函数失败，返回值为NULL</returns>
        [DllImport("user32.dll", EntryPoint = "FindWindow", CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindow(string IpClassName, string IpWindowName);

        /// <summary>
        /// 将创建指定窗口的线程设置到前台,并且激活该窗口。键盘输入转向该窗口,并为用户改各种可视的记号。系统给创建前台窗口的线程分配的权限稍高于其他线程。
        /// </summary>
        /// <param name="hwnd">将要设置前台的窗口句柄。</param>
        /// <returns>如果窗口设入了前台,返回值为非零;如果窗口未被设入前台,返回值为零。</returns>
        [DllImport("user32", SetLastError = true)]
        public static extern int SetForegroundWindow(IntPtr hwnd);

        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        public static extern void SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, int lParam);
        //WindowsHelper.SendMessage(childHwnd, WindowsHelper.BM_CLICK, IntPtr.Zero, 0);//发送点击childHwnd控件

        public const int WM_CLOSE = 0x10;//关闭
        public const int BM_CLICK = 0xF5;//点击

        /// <summary>
        /// 定义ShowWindow函数的nCmdShow参数常量,指定窗口如何显示。
        /// </summary>
        [Flags]
        public enum SW
        {
            /// <summary>
            /// 隐藏窗口并激活其他窗口。
            /// </summary>
            HIDE = 0,
            /// <summary>
            /// 最大化指定的窗口。
            /// </summary>
            MAXIMIZE = 3,
            /// <summary>
            /// 最小化指定的窗口并且激活在Z序中的下一个顶层窗口。
            /// </summary>
            MINIMIZE = 6,
            /// <summary>
            /// 激活并显示窗口。如果窗口最小化或最大化,则系统将窗口恢复到原来的尺寸和位置。在恢复最小化窗口时,应该指定这个标志。
            /// </summary>
            RESTORE = 9,
            /// <summary>
            /// 在窗口原来的位置以原来的尺寸激活和显示窗口。
            /// </summary>
            SHOW = 5,
            /// <summary>
            /// 依据在STARTUPINFO结构中指定的SW_FLAG标志设定显示状态,STARTUPINFO 结构是由启动应用程序的程序传递给CreateProcess函数的。
            /// </summary>
            SHOWDEFAULT = 10,
            /// <summary>
            /// 激活窗口并将其最大化。
            /// </summary>
            SHOWMAXIMIZED = 3,
            /// <summary>
            /// 激活窗口并将其最小化。
            /// </summary>
            SHOWMINIMIZED = 2,
            /// <summary>
            /// 窗口最小化,激活窗口仍然维持激活状态。
            /// </summary>
            SHOWMINNOACTIVE = 7,
            /// <summary>
            /// 以窗口原来的状态显示窗口,激活窗口仍然维持激活状态。
            /// </summary>
            SHOWNA = 8,
            /// <summary>
            /// 以窗口最近一次的大小和状态显示窗口,激活窗口仍然维持激活状态。
            /// </summary>
            SHOWNOACTIVATE = 4,
            /// <summary>
            /// 激活并显示一个窗口。如果窗口被最小化或最大化,系统将其恢复到原来的尺寸和大小。应用程序在第一次显示窗口的时候应该指定此标志。
            /// </summary>
            SHOWNORMAL = 1
        }

        /// <summary>
        /// 将已启动的程序展示到最前方
        /// </summary>
        /// <param name="setProcessName">进程名称</param>
        public static Process SetWindForeGround(string setProcessName)
        {
            Process[] processArr = Process.GetProcessesByName(setProcessName); //获取已打开的进程
            Process findProcess = null;
            List<Process> listProcess = processArr.ToList();
            if (listProcess != null && listProcess.Count == 1)
            {
                findProcess = listProcess[0];
            }
            else if (listProcess != null && listProcess.Count > 1)
            {
                listProcess.Sort(new DateTimeComparer());
                int index = listProcess.FindLastIndex(s => string.IsNullOrEmpty(s.MainWindowTitle));
                findProcess = listProcess.Find(s => s.SessionId == listProcess[index].SessionId);
            }

            if (findProcess != null)
            {
                ShowWindow(findProcess.MainWindowHandle, 1);   //显示窗口
                SetForegroundWindow(findProcess.MainWindowHandle);  //将窗口放置最前端
                listProcess.ForEach(p =>
                {
                    if (p != findProcess)
                    {
                        try
                        {
                            p.Kill();
                        }
                        catch { }
                    }
                });
            }

            return findProcess;
        }

        /// <summary>
        /// 启动程序
        /// </summary>
        /// <param name="setProcessName">exe程序名称</param>
        /// <returns></returns>
        public static Process StartProcess(string setProcessName)
        {
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = string.Format("{0}.exe", setProcessName);
            Process findProcess = null;

            info.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
            try
            {
                findProcess = Process.Start(info);
            }
            catch (Win32Exception ex)
            {
                LogHelper.WriteMessage(string.Format("系统找不到指定的文件。/r{0}", ex.ToString()));
            }
            return findProcess;
        }

        /// <summary>
        /// 启动程序或将已启动的程序展示到最前方
        /// </summary>
        /// <param name="setProcessName">进程名称</param>
        public static Process SetWindForeGroundByProccessName(string setProcessName)
        {
            Process[] processArr = Process.GetProcessesByName(setProcessName); //获取已打开的进程
            Process findProcess = null;
            List<Process> listProcess = processArr.ToList();
            if (listProcess != null && listProcess.Count == 1)
            {
                findProcess = listProcess[0];
            }
            else if (listProcess != null && listProcess.Count > 1)
            {
                listProcess.Sort(new DateTimeComparer());
                int index = listProcess.FindLastIndex(s => string.IsNullOrEmpty(s.MainWindowTitle));
                findProcess = listProcess.Find(s => s.SessionId == listProcess[index].SessionId);
            }

            if (findProcess != null)
            {
                ShowWindowAsync(findProcess.MainWindowHandle, 1);   //显示窗口
                SetForegroundWindow(findProcess.MainWindowHandle);  //将窗口放置最前端
                listProcess.ForEach(p =>
                {
                    if (p != findProcess)
                    {
                        try
                        {
                            p.Kill();
                        }
                        catch { }
                    }
                });
            }
            else
            {
                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = string.Format("{0}.exe", setProcessName);

                info.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
                try
                {
                    findProcess = Process.Start(info);
                }
                catch (Win32Exception ex)
                {
                    LogHelper.WriteMessage(string.Format("系统找不到指定的文件。/r{0}", ex.ToString()));
                }
            }

            return findProcess;
        }

        /// <summary>
        /// 启动程序或将已启动的程序展示到最前方
        /// </summary>
        /// <param name="lpszWindow">启动程序的标题名称</param>
        public static void SetWindForeGroundByWindowTitle(string lpszWindow)
        {
            IntPtr hwnd = FindWindowEx(IntPtr.Zero, IntPtr.Zero, null, lpszWindow);
            if (hwnd != IntPtr.Zero)
            {
                ShowWindow(hwnd, SW.SHOWNOACTIVATE);
                SetForegroundWindow(hwnd);
            }
        }

        /// <summary>
        /// 时间比较器
        /// </summary>
        public class DateTimeComparer : IComparer<Process>
        {
            public int Compare(Process x, Process y)
            {
                return DateTime.Compare(x.StartTime, y.StartTime);
            }
        }

        /// <summary>
        /// 获取exe文件的版本
        /// </summary>
        /// <param name="exePath"></param>
        /// <returns></returns>
        public FileVersionInfo getExeVersion(string exePath)
        {
            FileVersionInfo fileVerInfo = FileVersionInfo.GetVersionInfo(exePath);
            return fileVerInfo;
        }
    }
}
