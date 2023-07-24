using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FactoryHelperManager
{
    /// <summary>
    /// 读取和写文件中节点的键值对
    /// </summary>
    public static class IniParse
    {
        // 声明INI文件的写操作函数 WritePrivateProfileString()
        [System.Runtime.InteropServices.DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        // 声明INI文件的读操作函数 GetPrivateProfileString()
        [System.Runtime.InteropServices.DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, System.Text.StringBuilder retVal, int size, string filePath);

        private static string _sPath = string.Empty;

        /// <summary>
        /// C:\Users\Administrator\AppData\Local\Temp
        /// </summary>
        private static string tempPath = Path.GetTempPath();

        /// <summary>
        /// config默认路径
        /// </summary>
        public static string sPath
        {
            get
            {
                if (string.IsNullOrEmpty(_sPath))
                {
                    return Path.Combine(tempPath, "config.ini");
                }
                else
                {
                    return _sPath;
                }
            }
            set
            {
                _sPath = value;
            }
        }

        /// <summary>
        /// 将键值对写入对应的节点里
        /// </summary>
        /// <param name="section">节点名称</param>
        /// <param name="key">键名称</param>
        /// <param name="value">值</param>
        /// <param name="filePath">配置文件名称（全路径名称）</param>
        public static void WriteValue(string section, string key, string value,string filePath = "")
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                // section=配置节，key=键名，value=键值，path=路径
                WritePrivateProfileString(section, key, value, filePath);
            }
            else
            {
                // section=配置节，key=键名，value=键值，path=路径
                WritePrivateProfileString(section, key, value, sPath);
            }
        }

        /// <summary>
        /// 读取对应节点中对应键的值
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="filePath">配置文件名称（全路径名称）</param>
        /// <returns></returns>
        public static string ReadValue(string section, string key, string filePath = "")
        {

            // 每次从ini中读取多少字节
            System.Text.StringBuilder temp = new System.Text.StringBuilder(255);
            if (!string.IsNullOrEmpty(filePath))
            {
                // section=配置节，key=键名，temp=上面，path=路径
                GetPrivateProfileString(section, key, "", temp, 255, filePath);
            }
            else
            {
                // section=配置节，key=键名，temp=上面，path=路径
                GetPrivateProfileString(section, key, "", temp, 255, sPath);
            }

            return temp.ToString();
        }

    }
}
