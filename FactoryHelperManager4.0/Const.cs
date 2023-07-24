using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace FactoryHelperManager
{
    public static class Const
    {
        #region access数据库

        /// <summary>
        /// access数据库密码
        /// </summary>
        public static string passWord = "huaxingedu.cn";

        /// <summary>
        /// access数据库路径
        /// </summary>
        public static string dataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data.mdb");

        /// <summary>
        /// 加密access数据库连接字符串
        /// </summary>
        public static string DataAccessConnectStringFormat = "Provider=Microsoft.ACE.OLEDB.12.0;Jet OLEDB:Database Password={0};User Id=admin;Data Source='{1}';Persist Security Info=True";

        /// <summary>
        /// dbf数据路径
        /// </summary>
        public static string dbfDataPath = "";

        /// <summary>
        /// dbf数据连接字符串
        /// </summary>
        public static string DataDBFConnectionStringFormat = @"Provider=vfpoledb;Data Source={0};Collating Sequence=machine;";
        #endregion

        /// <summary>
        /// 本系统采用的编码。
        /// </summary>
        public readonly static Encoding DefaultEncoding = Encoding.GetEncoding("gb2312");

        /// <summary>
        /// 随机码。
        /// </summary>
        public static string Guid
        {
            get { return System.Guid.NewGuid().ToString("N").Substring(0, 4); }
        }


        #region 文件或文件夹

        /// <summary>
        /// 创建文件夹时删除同名文件。
        /// </summary>
        /// <param name="dirPath"></param>
        public static void DirectoryCreate(string dirPath)
        {
            try
            {
                if (!Directory.Exists(dirPath))
                {
                    if (File.Exists(dirPath))
                    {
                        File.Delete(dirPath);
                    }
                    Directory.CreateDirectory(dirPath);
                }
            }

            catch (Exception ex)
            {
                //Logger.LogError(ex);
                throw;
            }
        }

        /// <summary>
        /// 将答案文件夹中的图层中考试要比对的图片拷到题目目录下(且和data后缀文件相同的图层id）
        /// </summary>
        /// <param name="sourceDirectoryPath">原目标</param>
        /// <param name="desDirectoryPath">复制后目标</param>
        /// <param name="overwrite">是否覆盖</param>
        /// <param name="extends">要复制的后缀数组，为null时复制所有</param>
        public static void DirectoryCopy(string sourceDirectoryPath, string desDirectoryPath, bool overwrite, string[] extends)
        {
            string[] files = Directory.GetFiles(sourceDirectoryPath);
            List<string> starts = new List<string>();
            foreach (string p in files)
            {
                if (p.EndsWith(".data"))
                {
                    string path = Path.GetFileNameWithoutExtension(p);
                    string[] st = path.Split('-');
                    starts.Add(st[0]);
                }
            }
            DirectoryCopy(sourceDirectoryPath, desDirectoryPath, overwrite, extends, starts.ToArray(),true);
        }

        /// <summary>
        /// 复制文件夹里的文件到另一个文件夹（通过后缀）
        /// </summary>
        /// <param name="sourceDirectoryPath">原目标</param>
        /// <param name="desDirectoryPath">复制后目标</param>
        /// <param name="overwrite">是否覆盖</param>
        /// <param name="extends">要复制的后缀数组，为null时复制所有</param>
        /// <param name="starts">要复制的文件开始名称，为null时复制所有</param>
        /// <param name="includeChild">是否包含子文件夹</param>
        public static void DirectoryCopy(string sourceDirectoryPath, string desDirectoryPath, bool overwrite, string[] extends, string[] starts,bool includeChild)
        {
            try
            {
                string[] files = includeChild ?
                                 Directory.GetFiles(sourceDirectoryPath, "*", SearchOption.AllDirectories) :
                                 Directory.GetFiles(sourceDirectoryPath, "*", SearchOption.TopDirectoryOnly);
                if (files != null)
                {
                    if (!Directory.Exists(desDirectoryPath))
                    {
                        DirectoryCreate(desDirectoryPath);
                    }
                    List<string> listCopyFilePath = new List<string>();
                    foreach (string p in files)
                    {
                        bool isfind = true;
                        if (extends != null)
                        {
                            isfind = extends.ToList().Exists(e => p.EndsWith(e));
                        }
                        if (starts != null && isfind)
                        {
                            isfind = starts.ToList().Exists(s => Path.GetFileName(p).StartsWith(s));
                        }
                        if (isfind)
                            listCopyFilePath.Add(p);
                    }
                    if (listCopyFilePath != null && listCopyFilePath.Count > 0)
                    {
                        listCopyFilePath.ForEach(p => File.Copy(p, Path.Combine(desDirectoryPath, Path.GetFileName(p)), overwrite));
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// 删除文件夹
        /// </summary>
        /// <param name="sourceDirectoryPath">原目标</param>
        /// <param name="desDirectoryPath">复制后目标</param>
        /// <param name="overwrite">是否覆盖</param>
        public static void DirectoryDelete(string sourceDirectoryPath)
        {
            try
            {
                string[] files = Directory.GetFiles(sourceDirectoryPath);
                string[] directories = Directory.GetDirectories(sourceDirectoryPath);
                if (files != null)
                {
                    foreach (string p in files)
                    {
                        FileDelete(p);
                    }
                }
                if (directories != null && directories.Length > 0)
                {
                    foreach (string d in directories)
                    {
                        try
                        {
                            DirectoryDelete(d);
                        }
                        catch { LogHelper.WriteMessage("删除文件夹失败：\r\n" + d); }
                    }
                    Directory.Delete(sourceDirectoryPath);
                }
                else
                {
                    try
                    {
                        Directory.Delete(sourceDirectoryPath);
                    }
                    catch { LogHelper.WriteMessage("删除文件夹失败：\r\n" + sourceDirectoryPath); }
                }
            }
            catch { }
        }

        /// <summary>
        /// 删除文件夹中的所有以“XX”名称开头的文件（包括子文件夹）
        /// </summary>
        /// <param name="sourceDirectoryPath">文件夹路径</param>
        /// <param name="fileName">文件名称，为空时，不匹配文件名称</param>
        /// <param name="includeChild">是否包含子文件夹</param>
        public static void FileDeleteByFileName(string sourceDirectoryPath, string fileName, bool includeChild)
        {
            try
            {
                string search = sourceDirectoryPath + "*\\" + fileName + "*";
                string[] files = includeChild?
                                 Directory.GetFiles(sourceDirectoryPath, search, SearchOption.AllDirectories): 
                                 Directory.GetFiles(sourceDirectoryPath, search,SearchOption.TopDirectoryOnly);

                if (files != null)
                {
                    foreach (string p in files)
                    {
                        FileDelete(p);
                    }
                }
            }
            catch (Exception ex) { }

        }

        /// <summary>
        /// 删除指定路径下的文件
        /// </summary>
        /// <param name="filePath"></param>
        private static void FileDelete(string filePath)
        {
            try
            {
                File.SetAttributes(filePath, FileAttributes.Normal);
                double sectors = Math.Ceiling(new FileInfo(filePath).Length / 512.0);
                byte[] dummyBuffer = new byte[512];
                RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
                FileStream inputStream = new FileStream(filePath, FileMode.Open);
                inputStream.Position = 0;
                for (int sectorsWritten = 0; sectorsWritten < sectors; sectorsWritten++)
                {
                    rng.GetBytes(dummyBuffer);
                    inputStream.Write(dummyBuffer, 0, dummyBuffer.Length);
                    sectorsWritten++;
                }
                inputStream.SetLength(0);
                inputStream.Close();
                DateTime dt = new DateTime(2049, 1, 1, 0, 0, 0);
                File.SetCreationTime(filePath, dt);
                File.SetLastAccessTime(filePath, dt);
                File.SetLastWriteTime(filePath, dt);
                File.Delete(filePath);
            }
            catch { LogHelper.WriteMessage("删除文件失败：\r\n" + filePath); }
        }

        /// <summary>
        /// 删除文件夹中的所有以extension名称结尾的文件（除去名称为“FileName”，包括子文件夹）
        /// </summary>
        /// <param name="sourceDirectoryPath">文件夹路径</param>
        /// <param name="extensions">后缀名</param>
        /// <param name="fileName">不能删除的文件名称，为空时，删除所有</param>
        /// <param name="includeChild">是否包含子文件夹</param>
        public static void FileDeleteExtension(string sourceDirectoryPath, string[] extensions, string fileName, bool includeChild)
        {
            try
            {
                if (extensions != null && extensions.Length > 0)
                {
                    List<string> listFiles = new List<string>();
                    extensions.ToList().ForEach(ext =>
                    {
                        string search = sourceDirectoryPath + "*" + ext;
                        string[] files = includeChild ?
                                 Directory.GetFiles(sourceDirectoryPath, search, SearchOption.AllDirectories) :
                                 Directory.GetFiles(sourceDirectoryPath, search, SearchOption.TopDirectoryOnly);
                        listFiles.AddRange(files.ToList());
                    });

                    if (listFiles != null && listFiles.Count > 0)
                    {
                        listFiles = listFiles.FindAll(f => Path.GetFileNameWithoutExtension(f) != fileName);
                        foreach (string p in listFiles)
                        {
                            FileDelete(p);
                        }
                    }
                }
            }
            catch (Exception ex) { }

        }

        #endregion

        #region 设置文本框的文本类型

        /// <summary>
        /// 文本框的文本类型,为 SetTextBoxTextType 方法提供参数。
        /// </summary>
        public enum TextType
        {
            /// <summary>
            /// 所有字符(包括汉字)
            /// </summary>
            All = 0,

            /// <summary>
            /// 整数,0~9的数字
            /// </summary>
            Integer,

            /// <summary>
            /// 小数,0~9的数字和小数点
            /// </summary>
            Decimal,

            /// <summary>
            /// 负数,0~9的数字,"-"和小数点
            /// </summary>
            NegativeNumber,

            /// <summary>
            /// A~Z,a~z的所有字母
            /// </summary>
            Letter,

            /// <summary>
            /// 数字和字母
            /// </summary>
            NumberAndLetter,

            /// <summary>
            /// 所有的ASCII在0~127字符
            /// </summary>
            Char,

            /// <summary>
            /// 除数字和字母外的所有字符
            /// </summary>
            Other
        }

        /// <summary>
        /// 设置文本框的文本类型,请在文本框的KeyPress事件中调用此方法。
        /// </summary>
        /// <param name="e">文本框的 KeyPress 事件的 KeyPressEventArgs 参数。</param>
        /// <param name="textMode">文本框的文本类型,为 ControlUtil.TextTyp e值之一,此参数决定文本框中可输入的文本类型。</param>
        public static void SetTextBoxTextType(KeyPressEventArgs e, TextType textType)
        {
            //不处理控制字符
            if (e.KeyChar < 32)
            {
                return;
            }

            switch (textType)
            {
                //允许所有字符(包括汉字)
                case TextType.All:
                    break;

                //仅允许0~9的数字
                case TextType.Integer:
                    if (e.KeyChar < 48 || e.KeyChar > 57)
                    {
                        e.Handled = true;
                    }
                    break;

                //仅允许0~9的数字和小数点
                case TextType.Decimal:
                    if (e.KeyChar < 48 && e.KeyChar != 46 || e.KeyChar > 57)
                    {
                        e.Handled = true;
                    }
                    break;

                //仅允许0~9的数字,"-"和小数点
                case TextType.NegativeNumber:
                    if (e.KeyChar < 45 || e.KeyChar == 47 || e.KeyChar > 57)
                    {
                        e.Handled = true;
                    }
                    break;

                //仅允许A~Z,a~z的所有字母
                case TextType.Letter:
                    if (e.KeyChar < 65 || e.KeyChar > 90 && e.KeyChar < 97 || e.KeyChar > 122)
                    {
                        e.Handled = true;
                    }
                    break;

                //仅允许数字和字母
                case TextType.NumberAndLetter:
                    if (e.KeyChar < 48 || e.KeyChar > 57 && e.KeyChar < 65
                        || e.KeyChar > 90 && e.KeyChar < 97 || e.KeyChar > 122)
                    {
                        e.Handled = true;
                    }
                    break;

                //所有的ASCII在0~127字符
                case TextType.Char:
                    if (e.KeyChar > 127)
                    {
                        e.Handled = true;
                    }
                    break;

                //允许除数字和字母外的所有字符
                case TextType.Other:
                    if (!(e.KeyChar < 48 || e.KeyChar > 57 && e.KeyChar < 65
                        || e.KeyChar > 90 && e.KeyChar < 97 || e.KeyChar > 122))
                    {
                        e.Handled = true;
                    }
                    break;
            }
        }

        #endregion

        /// <summary>
        /// 根据是否必须调用 Control.Invoke 方法执行指定委托。
        /// </summary>
        /// <param name="control"></param>
        /// <param name="action"></param>
        public static void SafeInvoke(this Control control, EventHandler action)
        {
            if (control != null && !control.Disposing && !control.IsDisposed)
            {
                if (control.InvokeRequired)
                {
                    control.Invoke(action);
                }
                else
                {
                    action(null, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// 去空
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ReplaceEmpty(string str)
        {
            if (string.IsNullOrEmpty(str))
                return "";
            return str.Replace(" ", "").Replace("\r", "").Replace("\n", "").Replace("\t", "").Replace("\f", "").Replace("\a", "").Replace("　", "");//alter by xie (加剔除全半角)
        }

        /// <summary>
        /// 字符串剔除各种空格及数字全半角转换
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ReplaceString(string str)
        {
            return ToDBC(str.Replace(" ", "").Replace("　", "").Trim().Replace("\t", "").Replace("\r", "").Replace("\n", "").Replace("\a", "").Replace("\t", "").Replace("\f", "")); //去除字符串两端的空白字符或其他预定义字符(\r、\n)
        }

        /// <summary>
        /// 将字符串中的全角字符转换为半角
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToDBC(string input)
        {
            int count = 0;
            char[] c = input.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
                if (c[i] == 12288)　//全角空格编码
                {
                    c[i] = (char)32; continue;  //转为半角空格
                }
                if (c[i] > 65280 && c[i] < 65375)
                    c[i] = (char)(c[i] - 65248); //全角数字0-9，字母a-z，A-Z，各种符号与对应的半角相差65248
                if (c[i] == 8220 || c[i] == 8221 || c[i] == 34)
                {
                    count++;
                    c[i] = count % 2 == 1 ? (char)8820 : (char)8821;
                }
            }
            return new string(c);
        }

        #region 以文本文件的编码方式读取该文本文件的所有文本

        /// <summary>
        /// 以文本文件的编码方式读取该文本文件的所有文本。
        /// </summary>
        /// <param name="fileName">需要读取的文本文件名。</param>
        /// <returns>若读取成功，返回该文本文件的所有文件，否则，返回 null。</returns>
        public static string ReadAllFormatText(string fileName)
        {
            if (File.Exists(fileName))
            {
                try
                {
                    byte[] bytes = File.ReadAllBytes(fileName);
                    int length = bytes.Length;

                    if (length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
                    {
                        return Encoding.UTF8.GetString(bytes, 3, length - 3);
                    }
                    else if (IsUTF8Bytes(bytes))
                    {
                        return Encoding.UTF8.GetString(bytes, 0, length);
                    }
                    if (length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF)
                    {
                        return Encoding.BigEndianUnicode.GetString(bytes, 2, length - 2);
                    }
                    else if (length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
                    {
                        return Encoding.Unicode.GetString(bytes, 2, length - 2);
                    }
                    int[] cs = { 7, 5, 4, 3, 2, 1, 0, 6, 14, 30, 62, 126 };
                    for (int i = 0; i < length; i++)
                    {
                        int bits = -1;
                        for (int j = 0; j < 6; j++)
                        {
                            if (bytes[i] >> cs[j] == cs[j + 6])
                            {
                                bits = j;
                                break;
                            }
                        }
                        if (bits == -1)
                        {
                            return Encoding.Default.GetString(bytes);
                        }
                        while (bits-- > 0)
                        {
                            i++;
                            if (i == length || bytes[i] >> 6 != 2)
                            {
                                return Encoding.Default.GetString(bytes);
                            }
                        }
                    }
                    return Encoding.Default.GetString(bytes);
                }
                catch
                {
                }
            }
            return null;
        }

        #region IsUTF8Bytes

        /// <summary>
        /// 判断指定的字节数据是否为不带 BOM 的 UTF-8 格式数据。
        /// </summary>
        /// <param name="bytes">要判断格式的字节数组。</param>
        /// <returns>若指定的字节数据的格式为不带 BOM 的 UTF-8 格式，则返回 true，否则，返回 false。</returns>
        private static bool IsUTF8Bytes(byte[] bytes)
        {
            /*/             
                UTF-8 是一种变长字节编码方式。对于某一个字符的 UTF-8 编码，
                如果只有一个字节则其最高二进制位为 0；如果是多字节，其第一
                个字节从最高位开始，连续的二进制位值为 1 的个数决定了其编码
                的位数，其余各字节均以 10 开头。UTF-8 最多可用到 6 个字节。 
                如： 
                1 字节：0xxxxxxx 
                2 字节：110xxxxx 10xxxxxx 
                3 字节：1110xxxx 10xxxxxx 10xxxxxx 
                4 字节：11110xxx 10xxxxxx 10xxxxxx 10xxxxxx 
                5 字节：111110xx 10xxxxxx 10xxxxxx 10xxxxxx 10xxxxxx 
                6 字节：1111110x 10xxxxxx 10xxxxxx 10xxxxxx 10xxxxxx 10xxxxxx 
             
            //*/

            if (bytes != null)
            {
                //当前正在分析的字符应还有的字节数
                int charByteCounter = 1;

                //当前正在分析的字节
                byte currentByte;
                for (int i = 0; i < bytes.Length; i++)
                {
                    currentByte = bytes[i];
                    if (charByteCounter == 1)
                    {
                        //0x80 = 1000 0000
                        if (currentByte >= 0x80)
                        {
                            //判断当前字符的字节数 
                            while (((currentByte <<= 1) & 0x80) != 0)
                            {
                                charByteCounter++;
                            }

                            //首位若为非0，则至少以2个1开始，如：110xxxxx - 1111110x 
                            if (charByteCounter == 1 || charByteCounter > 6)
                            {
                                return false;
                            }
                        }
                    }
                    else //0xC0 = 1100 0000
                    {
                        //若为 UTF-8，此时必须以10开头
                        if ((currentByte & 0xC0) != 0x80)
                        {
                            return false;
                        }
                        charByteCounter--;
                    }
                }

                if (charByteCounter > 1)
                {
                    return false;
                }
                return true;
            }
            return false;
        }

        #endregion

        #endregion

        /// <summary>
        /// 模糊匹配
        /// </summary>
        /// <param name="s1">被查找的字符串</param>
        /// <param name="s2">要查找的匹配字</param>
        /// <param name="n">起始查找位置</param>
        /// <returns></returns>
        public static bool MpMatchStr(string s1, string s2, int n)
        {
            bool result = true;
            for (int j = 0; j < s2.Length; j++)
            {
                int findIndex = n;
                bool isFind = false;
                for (int i = findIndex; i < s1.Length; i++)
                {
                    if (s1[i] == s2[j])
                    {
                        findIndex = i;
                        isFind = true;
                        break;
                    }
                }
                result = result && isFind;
                if (!result)
                {
                    break;
                }
            }
            return result;
        }

    }
}
