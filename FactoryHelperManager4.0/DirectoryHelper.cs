using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Management;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Data;

namespace FactoryHelperManager
{
    public class DirectoryHelper
    {

        /// <summary>
        /// 创建共享目录
        /// </summary>
        /// <param name="sharePath">共享名</param>
        /// <param name="shareName">共享路径</param>
        public static void setShareNetFolder(string sharePath, string shareName="ShareSqliteName")
        {
            Process p = new Process();  //创建进程对象
            p.StartInfo.FileName = "cmd";            //启动进程名称

            if (!Directory.Exists(sharePath))
            {

            }

            //执行的命令
            p.StartInfo.Arguments = string.Format(" /c net share {0}={1}", shareName, sharePath);
            //窗口状态为隐藏
            //p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            //启动进程不创建窗口
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.UseShellExecute = false;   //从可执行文件创建进程
            p.StartInfo.RedirectStandardOutput = true;  //将输出写入流中
            p.Start();   //启动进程
            p.WaitForExit();  //设置无限期等待关联的进程退出
        }

        /// <summary>
        /// 设置文件夹共享
        /// </summary>
        /// <param name="FolderPath">文件夹路径</param>
        /// <param name="ShareName">共享名</param>
        /// <param name="Description">共享注释</param>
        /// <returns></returns>
        public static int ShareNetFolder(string folderPath, string shareName, string description)
        {
            try
            {
                ManagementClass managementClass = new ManagementClass("Win32_Share");
                ManagementBaseObject inParams = managementClass.GetMethodParameters("Create");
                ManagementBaseObject outParams;
                inParams["Description"] = description;
                inParams["Name"] = shareName;
                inParams["Path"] = folderPath;
                inParams["Type"] = 0x0;
                outParams = managementClass.InvokeMethod("Create", inParams, null);
                if ((uint)(outParams.Properties["ReturnValue"].Value) != 0)
                {
                    throw new Exception("Unable to share directory.");
                }
            }
            catch
            {
                return -1;
            }
            return 0;
        }

        /// <summary>
        /// 取消文件夹共享
        /// </summary>
        /// <param name="ShareName">文件夹的共享名</param>
        /// <returns></returns>
        public static int CancelShareNetFolder(string shareName)
        {
            try
            {
                SelectQuery selectQuery = new SelectQuery("Select * from Win32_Share Where Name = '" + shareName + "'");
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(selectQuery);
                foreach (ManagementObject mo in searcher.Get())
                {
                    mo.InvokeMethod("Delete", null, null);
                }
            }
            catch
            {
                return -1;
            }
            return 0;
        }

        public static void CreateDirecory(string folderPath)
        {
            Const.DirectoryCreate(folderPath);
        }

        public static void SetFileRole(string folderPath)
        {
            DirectorySecurity fsec = new DirectorySecurity();
            fsec.AddAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.FullControl,
                InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
            Directory.SetAccessControl(folderPath, fsec);

        }

        public static void CreateSharedFolder(string FolderPath, string ShareName, string Description)
        {
            try
            {
                ManagementClass managementClass = new ManagementClass("Win32_Share");

                // Create ManagementBaseObjects for in and out parameters
                ManagementBaseObject inParams = managementClass.GetMethodParameters("Create");

                ManagementBaseObject outParams;

                // Set the input parameters
                inParams["Description"] = Description;
                inParams["Name"] = ShareName;
                inParams["Path"] = FolderPath;
                inParams["Type"] = 0x0; // Disk Drive

                //Another Type:
                // DISK_DRIVE = 0x0
                // PRINT_QUEUE = 0x1
                // DEVICE = 0x2
                // IPC = 0x3
                // DISK_DRIVE_ADMIN = 0x80000000
                // PRINT_QUEUE_ADMIN = 0x80000001
                // DEVICE_ADMIN = 0x80000002
                // IPC_ADMIN = 0x8000003

                //inParams["MaximumAllowed"] = 2;
                inParams["Password"] = "123456";

                NTAccount everyoneAccount = new NTAccount(null, "EVERYONE");
                SecurityIdentifier sid = (SecurityIdentifier)everyoneAccount.Translate(typeof(SecurityIdentifier));
                byte[] sidArray = new byte[sid.BinaryLength];
                sid.GetBinaryForm(sidArray, 0);

                ManagementObject everyone = new ManagementClass("Win32_Trustee");
                everyone["Domain"] = null;
                everyone["Name"] = "EVERYONE";
                everyone["SID"] = sidArray;

                ManagementObject dacl = new ManagementClass("Win32_Ace");
                dacl["AccessMask"] = 2032127;
                dacl["AceFlags"] = 3;
                dacl["AceType"] = 0;
                dacl["Trustee"] = everyone;

                ManagementObject securityDescriptor = new ManagementClass("Win32_SecurityDescriptor");
                securityDescriptor["ControlFlags"] = 4; //SE_DACL_PRESENT 
                securityDescriptor["DACL"] = new object[] { dacl };

                inParams["Access"] = securityDescriptor;

                // Invoke the "create" method on the ManagementClass object
                outParams = managementClass.InvokeMethod("Create", inParams, null);

                // Check to see if the method invocation was successful
                var result = (uint)(outParams.Properties["ReturnValue"].Value);
                switch (result)
                {
                    case 0:
                        Console.WriteLine("Folder successfuly shared.");
                        break;
                    case 2:
                        Console.WriteLine("Access Denied");
                        break;
                    case 8:
                        Console.WriteLine("Unknown Failure");
                        break;
                    case 9:
                        Console.WriteLine("Invalid Name");
                        break;
                    case 10:
                        Console.WriteLine("Invalid Level");
                        break;
                    case 21:
                        Console.WriteLine("Invalid Parameter");
                        break;
                    case 22:
                        Console.WriteLine("Duplicate Share");
                        break;
                    case 23:
                        Console.WriteLine("Redirected Path");
                        break;
                    case 24:
                        Console.WriteLine("Unknown Device or Directory");
                        break;
                    case 25:
                        Console.WriteLine("Net Name Not Found");
                        break;
                    default:
                        Console.WriteLine("Folder cannot be shared.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error:" + ex.Message);
            }
        }

        public static void RemoveSharedFolder(string ShareName)
        {
            try
            {
                // Create a ManagementClass object
                ManagementClass managementClass = new ManagementClass("Win32_Share");
                ManagementObjectCollection shares = managementClass.GetInstances();
                foreach (ManagementObject share in shares)
                {
                    if (Convert.ToString(share["Name"]).Equals(ShareName))
                    {
                        var result = share.InvokeMethod("Delete", new object[] { });

                        // Check to see if the method invocation was successful
                        if (Convert.ToInt32(result) != 0)
                        {
                            Console.WriteLine("Unable to unshare directory.");
                        }
                        else
                        {
                            Console.WriteLine("Folder successfuly unshared.");
                        }
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error:" + ex.Message);
            }
        }

        public static void getc()
        {
            SQLiteHelper.connectionString = string.Format("Data Source={0};", @"\\192.168.1.59\ShareSqliteName\QuestionManager.db");
            DataSet ds = SQLiteHelper.Query("select * from QuestionDry");
            ManagementScope ms = new ManagementScope(@"\\192.168.1.59");
            ConnectionOptions conn = new ConnectionOptions();
            conn.Username = "";
            conn.Password = "";
            ms.Options = conn;
            ms.Connect();
        }

    }
}
