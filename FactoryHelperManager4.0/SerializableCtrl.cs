using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace FactoryHelperManager
{
    /// <summary>
    /// 序列化对象、反序列化对象类
    /// </summary>
    public class SerializableCtrl
    {
        /// <summary>
        /// 序列化对象到指定文件
        /// </summary>
        /// <param name="t">进行序列化的对象</param>
        /// <param name="filePath">指定文件路径含文件名</param>
        public static void ExamSerialize<T>(T t, string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                byte[] serializeBytes;
                using (MemoryStream ms = new MemoryStream())
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(ms, t);
                    serializeBytes = ms.ToArray();
                }

                using (FileStream fs = new FileStream(filePath, FileMode.Create))
                {
                    using (DeflateStream compressedzipStream = new DeflateStream(fs, CompressionMode.Compress, true))
                    {
                        compressedzipStream.Write(serializeBytes, 0, serializeBytes.Length);
                        compressedzipStream.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("序列化{0}对象错误！", t.GetType().Name), ex);
            }
        }

        /// <summary>
        /// 序列化对象到字节数组
        /// </summary>
        /// <param name="t">进行序列化的对象</param>
        public static byte[] ExamSerialize<T>(T t)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(ms, t);
                    return ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("序列化{0}对象错误！", t.GetType().Name), ex);
            }
        }

        /// <summary>
        /// 从指定文件反序列化对象
        /// </summary>
        /// <param name="filePath">指定的文件</param>
        /// <param name="readCount">每次从流中读取的最大字节数</param>
        /// <returns></returns>
        public static T ExamDeserialize<T>(string filePath, int readCount)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    throw new Exception("指定文件不存在！");
                }

                byte[] deserializeDecompressBytes;
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    using (DeflateStream zipStream = new DeflateStream(fs, CompressionMode.Decompress))
                    {
                        ReadAllBytesFromStream(zipStream, out deserializeDecompressBytes, readCount);

                        zipStream.Close();
                    }
                }

                using (MemoryStream ms = new MemoryStream(deserializeDecompressBytes))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    return (T)bf.Deserialize(ms);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("反序列化{0}对象错误！", typeof(T).Name), ex);
            }
        }

        /// <summary>
        /// 从指定字节数组反序列化对象
        /// </summary>
        /// <param name="filePath">指定的字节数组</param>
        /// <returns></returns>
        public static T ExamDeserialize<T>(byte[] bytes)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    return (T)bf.Deserialize(ms);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("反序列化{0}对象错误！", typeof(T).Name), ex);
            }
        }

        /// <summary>
        /// 读取流里面的所有字节，并返回流的总长度。
        /// </summary>
        /// <param name="stream">流</param>
        /// <param name="buffer">返回的字节数组</param>
        /// <param name="readCount">每次从流中读取的最大字节数</param>
        /// <param name="capacity">字节数组初始容量</param>
        /// <param name="loadFactor">字节数组容量增长数</param>
        /// <returns>流的总长度</returns>
        private static int ReadAllBytesFromStream(Stream stream, out byte[] buffer, int readCount)
        {
            List<byte> bytes = new List<byte>();
            byte[] readBytes = new byte[readCount];
            int count = 0;
            while ((count = stream.Read(readBytes, 0, readCount)) == readCount)
            {
                bytes.AddRange(readBytes);
            }
            if (count > 0)
            {
                bytes.AddRange(readBytes.Take(count));
            }

            buffer = bytes.ToArray();
            readBytes = null;
            bytes = null;

            return buffer.Length;
        }
    }
}
