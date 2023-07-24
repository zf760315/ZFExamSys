using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Security.AccessControl;
using System.Text;

namespace FactoryHelperManager
{
    public enum MethodStatus : uint
    {
        Success = 0,    //Success
        AccessDenied = 2,   //Access denied
        UnknownFailure = 8,     //Unknown failure
        InvalidName = 9,    //Invalid name
        InvalidLevel = 10,  //Invalid level
        InvalidParameter = 21,  //Invalid parameter
        DuplicateShare = 22,    //Duplicate share
        RedirectedPath = 23,    //Redirected path
        UnknownDevice = 24,     //Unknown device or directory
        NetNameNotFound = 25    //Net name not found
    }

    public enum ShareType : uint
    {
        DiskDrive = 0x0,    //Disk Drive
        PrintQueue = 0x1,   //Print Queue
        Device = 0x2,   //Device
        IPC = 0x3,  //IPC
        DiskDriveAdmin = 0x80000000,    //Disk Drive Admin
        PrintQueueAdmin = 0x80000001,   //Print Queue Admin
        DeviceAdmin = 0x80000002,   //Device Admin
        IpcAdmin = 0x80000003   //IPC Admin
    }

    public enum AccessPrivileges : uint
    {
        /// <summary>
        /// 列出文件夹/读取数据
        /// </summary>
        FILE_READ_DATA = 0x00000001,
        /// <summary>
        /// 创建文件/写入数据
        /// </summary>
        FILE_WRITE_DATA = 0x00000002,
        /// <summary>
        /// 创建文件夹/附加数据
        /// </summary>
        FILE_APPEND_DATA = 0x00000004,
        /// <summary>
        /// 读取扩展属性
        /// </summary>
        FILE_READ_EA = 0x00000008,
        /// <summary>
        /// 写入扩展属性
        /// </summary>
        FILE_WRITE_EA = 0x00000010,
        /// <summary>
        /// 遍历文件夹/执行文件
        /// </summary>
        FILE_EXECUTE = 0x00000020,
        /// <summary>
        /// 删除子文件夹及文件
        /// </summary>
        FILE_DELETE_CHILD = 0x00000040,
        /// <summary>
        /// 读取属性
        /// </summary>
        FILE_READ_ATTRIBUTES = 0x00000080,
        /// <summary>
        /// 写入属性
        /// </summary>
        FILE_WRITE_ATTRIBUTES = 0x00000100,
        /// <summary>
        /// 删除
        /// </summary>
        DELETE = 0x00010000,
        /// <summary>
        /// 读取权限
        /// </summary>
        READ_CONTROL = 0x00020000,
        /// <summary>
        /// 更改权限
        /// </summary>
        WRITE_DAC = 0x00040000,
        /// <summary>
        /// 取得所有权
        /// </summary>
        WRITE_OWNER = 0x00080000,
        /// <summary>
        /// 无任何权限
        /// </summary>
        SYNCHRONIZE = 0x00100000,
        /// <summary>
        /// 所有权限
        /// </summary>
        Full = AccessPrivileges.DELETE | AccessPrivileges.FILE_APPEND_DATA | AccessPrivileges.FILE_DELETE_CHILD | AccessPrivileges.FILE_EXECUTE
        | AccessPrivileges.FILE_READ_ATTRIBUTES | AccessPrivileges.FILE_READ_DATA | AccessPrivileges.FILE_READ_EA | AccessPrivileges.FILE_WRITE_ATTRIBUTES
        | AccessPrivileges.FILE_WRITE_DATA | AccessPrivileges.FILE_WRITE_EA | AccessPrivileges.READ_CONTROL | AccessPrivileges.SYNCHRONIZE
        | AccessPrivileges.WRITE_DAC | AccessPrivileges.WRITE_OWNER
    }

    enum AceFlags : uint
    {
        NonInheritAce = 0,
        ObjectInheritAce = 1,
        ContainerInheritAce = 2,
        NoPropagateInheritAce = 4,
        InheritOnlyAce = 8,
        InheritedAce = 16
    }

    [Flags]
    enum AceType : uint
    {
        AccessAllowed = 0,
        AccessDenied = 1,
        Audit = 2
    }

    public class UserPrivileges
    {
        public string UserAccount { get; set; }
        public List<string> Privileges { get; set; }
        public string Domain { get; set; }
        public object ObjPrivileges
        {
            get;
            set;
        }
    }

    public class ShareHelper
    {
        static string[] filedesc = {"FILE_READ_DATA", "FILE_WRITE_DATA", "FILE_APPEND_DATA", "FILE_READ_EA",
                                    "FILE_WRITE_EA", "FILE_EXECUTE", "FILE_DELETE_CHILD", "FILE_READ_ATTRIBUTES",
                                    "FILE_WRITE_ATTRIBUTES", " ", " ", " ",
                                    " ", " ", " ", " ",
                                    "DELETE ", "READ_CONTROL", "WRITE_DAC", "WRITE_OWNER",
                                    "SYNCHRONIZE ", " ", " "," ",
                                    "ACCESS_SYSTEM_SECURITY", "MAXIMUM_ALLOWED", " "," ",
                                    "GENERIC_ALL", "GENERIC_EXECUTE", "GENERIC_WRITE","GENERIC_READ"};

        private ManagementObject mWinShareObject;

        private ShareHelper(ManagementObject obj) { mWinShareObject = obj; }

        #region Wrap Win32_Share properties

        public uint AccessMask
        {
            get { return Convert.ToUInt32(mWinShareObject["AccessMask"]); }
        }

        public bool AllowMaximum
        {
            get { return Convert.ToBoolean(mWinShareObject["AllowMaximum"]); }
        }

        public string Caption
        {
            get { return Convert.ToString(mWinShareObject["Caption"]); }
        }

        public string Description
        {
            get { return Convert.ToString(mWinShareObject["Description"]); }
        }

        public DateTime InstallDate
        {
            get { return Convert.ToDateTime(mWinShareObject["InstallDate"]); }
        }

        public uint MaximumAllowed
        {
            get { return Convert.ToUInt32(mWinShareObject["MaximumAllowed"]); }
        }

        public string Name
        {
            get { return Convert.ToString(mWinShareObject["Name"]); }
        }

        public string Path
        {
            get { return Convert.ToString(mWinShareObject["Path"]); }
        }

        public string Status
        {
            get { return Convert.ToString(mWinShareObject["Status"]); }
        }

        public ShareType Type
        {
            get { return (ShareType)Convert.ToUInt32(mWinShareObject["Type"]); }
        }

        #endregion

        #region Wrap Methods

        /// <summary>
        /// 删除共享
        /// </summary>
        /// <returns></returns>
        public MethodStatus Delete()
        {
            object result = mWinShareObject.InvokeMethod("Delete", new object[] { });
            uint r = Convert.ToUInt32(result);

            return (MethodStatus)r;
        }

        /// <summary>
        /// 创建共享
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="maximumAllowed"></param>
        /// <param name="description"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static MethodStatus Create(string path, string name, ShareType type, uint maximumAllowed, string description, string password)
        {
            ManagementClass mc = new ManagementClass("Win32_Share");
            object[] parameters = new object[] { path, name, (uint)type, maximumAllowed, description, password, null };

            object result = mc.InvokeMethod("Create", parameters);
            uint r = Convert.ToUInt32(result);

            return (MethodStatus)r;
        }

        #endregion

        public static IList<ShareHelper> GetAllShares()
        {
            IList<ShareHelper> result = new List<ShareHelper>();
            ManagementClass mc = new ManagementClass("Win32_Share");
            ManagementObjectCollection moc = mc.GetInstances();

            foreach (ManagementObject mo in moc)
            {
                ShareHelper share = new ShareHelper(mo);
                result.Add(share);
            }

            return result;
        }

        public static ShareHelper GetNamedShare(string name)
        {
            IList<ShareHelper> shares = GetAllShares();

            foreach (ShareHelper s in shares)
                if (s.Name == name)
                    return s;

            return null;
        }

        public static MethodStatus SetPrivileges(string path, List<UserPrivileges> listPrivilege)
        {
            ManagementObject mo = new ManagementObject(string.Format("Win32_LogicalFileSecuritySetting.Path='{0}'", path));
            ManagementBaseObject outParams = mo.InvokeMethod("GetSecurityDescriptor", null, null);

            if ((uint)outParams.Properties["ReturnValue"].Value != 0)
            {
                return MethodStatus.NetNameNotFound;
            }
            ManagementBaseObject Descriptor = (ManagementBaseObject)outParams.Properties["Descriptor"].Value;

            List<ManagementBaseObject> newDacl = new List<ManagementBaseObject>();

            foreach (UserPrivileges up in listPrivilege)
            {
                ManagementClass trustee = new ManagementClass("win32_trustee");
                trustee.Properties["Name"].Value = up.UserAccount;
                trustee.Properties["Domain"].Value = null;

                ManagementClass ace = new ManagementClass("win32_ace");
                ace.Properties["AccessMask"].Value = up.ObjPrivileges; //AccessPrivileges.FileReadData | AccessPrivileges.FileReadAttributes | AccessPrivileges.FileReadEA
                                                                       //| AccessPrivileges.ReadControl | AccessPrivileges.FileExecute;
                ace.Properties["AceFlags"].Value = AceFlags.ObjectInheritAce | AceFlags.ContainerInheritAce | AceFlags.NoPropagateInheritAce;
                ace.Properties["AceType"].Value = AceType.AccessAllowed;
                ace.Properties["Trustee"].Value = trustee;
                newDacl.Add(ace);
            }

            ManagementBaseObject inParams = mo.GetMethodParameters("SetSecurityDescriptor");
            Descriptor.Properties["Dacl"].Value = newDacl.ToArray();

            inParams["Descriptor"] = Descriptor;
            ManagementBaseObject ret = mo.InvokeMethod("SetSecurityDescriptor", inParams, null);

            uint returnValue = (uint)ret.Properties["ReturnValue"].Value;
            return (MethodStatus)returnValue;
        }

        public static List<UserPrivileges> GetPrivileges(string path)
        {
            List<UserPrivileges> list = new List<UserPrivileges>();
            ManagementPath mPath = new ManagementPath();
            mPath.Server = ".";
            mPath.NamespacePath = @"root\cimv2";
            mPath.RelativePath = @"Win32_LogicalFileSecuritySetting.Path='" + path + "'"; // using tmp as folder name

            ManagementObject lfs = new ManagementObject(mPath);

            ManagementBaseObject outParams = lfs.InvokeMethod("GetSecurityDescriptor", null, null);
            if (((uint)(outParams.Properties["ReturnValue"].Value)) == 0)
            {

                ManagementBaseObject Descriptor = ((ManagementBaseObject)(outParams.Properties["Descriptor"].Value));

                ManagementBaseObject[] DaclObject = ((ManagementBaseObject[])(Descriptor.Properties["Dacl"].Value));

                foreach (ManagementBaseObject mbo in DaclObject)
                {
                    UserPrivileges up = new UserPrivileges();
                    ManagementBaseObject Trustee = ((ManagementBaseObject)(mbo["Trustee"]));
                    up.Domain = Trustee.Properties["Domain"].Value == null ? "" : Trustee.Properties["Domain"].Value.ToString();
                    up.UserAccount = Trustee.Properties["Name"].Value.ToString();

                    uint mask = (uint)mbo["AccessMask"];

                    int[] m = { (int)mask };

                    BitArray ba = new BitArray(m);

                    int i = 0;

                    IEnumerator baEnum = ba.GetEnumerator();
                    up.Privileges = new List<string>();
                    while (baEnum.MoveNext())
                    {
                        if ((bool)baEnum.Current)
                            up.Privileges.Add(filedesc[i].Trim());
                        i++;
                    }
                    list.Add(up);
                }
            }

            List<UserPrivileges> listNew = new List<UserPrivileges>();
            foreach (var up in list)
            {
                UserPrivileges upNew = listNew.Where(x => x.UserAccount == up.UserAccount).FirstOrDefault();
                if (upNew != null)
                {
                    upNew.Privileges.AddRange(up.Privileges);
                    upNew.Privileges = upNew.Privileges.Distinct().ToList();
                }
                else
                {
                    listNew.Add(up);
                }
            }
            return listNew;
        }

        /// <summary>
        /// 设置windows环境变量
        /// </summary>
        /// <param name="name">变量名称</param>
        /// <param name="value">变量值</param>
        public static void SetEnvironmentVariable(string name, string value)
        {
            RegistryKey regLocalMachine = Registry.LocalMachine;
            RegistryKey regSYSTEM = regLocalMachine.OpenSubKey("SYSTEM", true);//打开HKEY_LOCAL_MACHINE下的SYSTEM
            RegistryKey regControlSet001 = regSYSTEM.OpenSubKey("ControlSet001", true);
            RegistryKey regControl = regControlSet001.OpenSubKey("Control", true);
            RegistryKey regManager = regControl.OpenSubKey("Session Manager", true);
            RegistryKey regEnvironment = regManager.OpenSubKey("Environment", true);
            regEnvironment.SetValue(name, value);
        }

        /// <summary>
        /// Adds an ACL entry on the specified directory for the specified account.
        /// This function was taken directly from MSDN.  It adds security rights to a folder
        /// </summary>
        /// <param name="FileName"></param>
        /// <param name="Account">like @"BUILTIN\Administrators" or @"BUILTIN\Users" </param>
        /// <param name="Rights">like FileSystemRights.FullControl</param>
        /// <param name="ControlType">like AccessControlType.Allow</param>
        public static void AddDirectorySecurity(string FileName, string Account, FileSystemRights Rights, AccessControlType ControlType)
        {
            // Create a new DirectoryInfo object.
            DirectoryInfo dInfo = new DirectoryInfo(FileName);

            // Get a DirectorySecurity object that represents the
            // current security settings.
            DirectorySecurity dSecurity = dInfo.GetAccessControl();

            // Add the FileSystemAccessRule to the security settings.
            dSecurity.AddAccessRule(new FileSystemAccessRule(Account, Rights, ControlType));

            // Set the new access settings.
            dInfo.SetAccessControl(dSecurity);
        }

        /// <summary>
        /// Adds an ACL entry on the specified directory for the specified account.
        /// This function was taken directly from MSDN.  It adds security rights to a file
        /// </summary>
        /// <param name="FileName"></param>
        /// <param name="Account">like @"BUILTIN\Administrators" or @"BUILTIN\Users" </param>
        /// <param name="Rights">like FileSystemRights.FullControl</param>
        /// <param name="ControlType">like AccessControlType.Allow</param>
        public static void AddFileSecurity(string FileName, string Account, FileSystemRights Rights, AccessControlType ControlType)
        {
            // Create a new FileInfo object.
            FileInfo fInfo = new FileInfo(FileName);

            // Get a FileSecurity object that represents the
            // current security settings.
            FileSecurity fSecurity = fInfo.GetAccessControl();

            // Add the FileSystemAccessRule to the security settings.
            fSecurity.AddAccessRule(new FileSystemAccessRule(Account,
                                                            Rights,
                                                            ControlType));

            // Set the new access settings.
            fInfo.SetAccessControl(fSecurity);
        }


        /// <summary>
        /// 读取windows 系统日志
        /// </summary>
        /// <returns></returns>
        public static string ReadWindowsLog()
        {
            string[] logs = new string[] { "Application", "System", "Security" };

            /*清空所有日志*/
            //EventLog eventlog = new EventLog();
            //foreach (var item in logs)
            //{
            //    eventlog.Log = item;
            //    eventlog.Clear();  
            //}

            /*清空所有日志*/

            StringBuilder sb = new StringBuilder();

            foreach (string log in logs)
            {
                EventLog myLog = new EventLog();
                myLog.Log = log;
                //myLog.MachineName = "rondi-agt0qf9op";
                foreach (EventLogEntry entry in myLog.Entries)
                {
                    //EventLogEntryType枚举包括：
                    //Error 错误事件。
                    //FailureAudit 失败审核事件。
                    //Information 信息事件。
                    //SuccessAudit 成功审核事件。
                    //Warning 警告事件。
                    if (entry.EntryType == EventLogEntryType.Error || entry.EntryType == EventLogEntryType.Warning)
                    {
                        sb.Append(log);
                        sb.Append(entry.EntryType.ToString());
                        sb.Append(entry.TimeWritten.ToString());
                        sb.Append(entry.Message + "\r\n");
                    }
                }
            }
            return sb.ToString();
        }

    }
}
