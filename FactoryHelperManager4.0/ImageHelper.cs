using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;

namespace FactoryHelperManager
{
    public class ImageHelper
    {
        /// <summary>
        /// 从指定的文件读取图像。
        /// </summary>
        /// <param name="fileName">要读取图像的文件名（全路径名称）。</param>
        /// <returns>图像位图。</returns>
        public static Bitmap ReadImageFromFile(string fileName)
        {
            try
            {
                Byte[] buffer = File.ReadAllBytes(fileName);
                MemoryStream ms = new MemoryStream(buffer);
                
                ms.Position = 0;
                Bitmap srcImage = Image.FromStream(ms) as Bitmap;
                Bitmap desImage = CloneImage(srcImage);
                ms.Close();
                return desImage;
            }
            catch { }
            return null;
        }

        [System.Runtime.InteropServices.DllImport("Gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);


        /// <summary>
        /// 将 Bitmap 转化为 BitmapSource
        /// </summary>
        /// <param name="bmp"/>要转换的 Bitmap
        /// <returns>转换后的 BitmapSource</returns>
        public static BitmapSource ToBitmapSource(Bitmap bmp)
        {
            IntPtr hBitmap = bmp.GetHbitmap();
            try
            {
                return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                DeleteObject(hBitmap);
            }
        }

        /// <summary>
        /// 将 BitmapSource 转化为 byte
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static byte[] BitmapSourceToByte(BitmapSource bitmap)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                var encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                encoder.Save(ms);
                return ms.GetBuffer();
            }
        }

        /// <summary>
        /// 将 BitmapSource 转化为 Image
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static Image BitmapSourceToImage(BitmapSource bitmap)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                var encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                encoder.Save(ms);
                return Image.FromStream(ms);
            }
        }

       

        #region 克隆指定的图片

        /// <summary>
        /// 克隆指定的图片。
        /// </summary>
        /// <param name="srcImage">要进行克隆的源图片。</param>
        /// <returns>克隆后的图片。</returns>
        public static Bitmap CloneImage(Bitmap srcImage)
        {
            if (srcImage != null)
            {
                BitmapData srcData = srcImage.LockBits(new Rectangle(0, 0, srcImage.Width, srcImage.Height), ImageLockMode.ReadOnly, srcImage.PixelFormat);
                Bitmap bitmap = CloneImage(srcData);
                srcImage.UnlockBits(srcData);

                PixelFormat pixelFormat = srcImage.PixelFormat;
                if (((pixelFormat == PixelFormat.Format1bppIndexed) || (pixelFormat == PixelFormat.Format4bppIndexed)) || ((pixelFormat == PixelFormat.Format8bppIndexed) || (pixelFormat == PixelFormat.Indexed)))
                {
                    ColorPalette palette = srcImage.Palette;
                    ColorPalette palette2 = bitmap.Palette;
                    int length = palette.Entries.Length;
                    for (int i = 0; i < length; i++)
                    {
                        palette2.Entries[i] = palette.Entries[i];
                    }
                    bitmap.Palette = palette2;
                }

                return bitmap;
            }
            return null;
        }

        /// <summary>
        /// 克隆指定的图片。
        /// </summary>
        /// <param name="srcBitmapData">要进行克隆的源图片的 BitmapData。</param>
        /// <returns>克隆后的图片。</returns>
        public static Bitmap CloneImage(BitmapData srcBitmapData)
        {
            if (srcBitmapData != null)
            {
                int width = srcBitmapData.Width;
                int height = srcBitmapData.Height;
                Bitmap bitmap = new Bitmap(width, height, srcBitmapData.PixelFormat);
                BitmapData bitmapdata = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, bitmap.PixelFormat);

                //图像第一个像素的地址
                IntPtr ptr = srcBitmapData.Scan0;
                //图像数据总长度
                int length = Math.Abs(srcBitmapData.Stride) * height;
                //存储图像数据数组
                byte[] bytes = new byte[length];
                //将图像数据从内存拷贝到字节数组中
                Marshal.Copy(ptr, bytes, 0, length);

                //将字节数组复制到新图像数据的地址上
                Marshal.Copy(bytes, 0, bitmapdata.Scan0, length);

                bitmap.UnlockBits(bitmapdata);

                return bitmap;
            }
            return null;
        }

        #endregion

        ///// <summary>
        ///// 克隆指定的图片。
        ///// </summary>
        ///// <param name="srcImage">要进行克隆的目标图片。</param>
        ///// <returns>克隆后的图片。</returns>
        //public static Bitmap CloneImage(Bitmap srcImage)
        //{
        //    Bitmap desImage = null;
        //    System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
        //    using (MemoryStream ms = new MemoryStream())
        //    {
        //        bf.Serialize(ms, srcImage);
        //        ms.Position = 0;

        //        desImage = bf.Deserialize(ms) as Bitmap;
        //    }
        //    return desImage;
        //}

        /// <summary>
        /// 压缩图片
        /// </summary>
        /// <param name="photoFileName">图片名称（图片全路径名称）</param>
        /// <param name="newW">压缩后宽度</param>
        /// <param name="newH">压缩后高度</param>
        /// <param name="Mode"></param>
        /// <returns></returns>
        public static byte[] KiResizeImage(string photoFileName, int newW, int newH, int Mode=1)
        {
            byte[] data = null;
            try
            {
                using (Bitmap bmpOriginal = ReadImageFromFile(photoFileName))
                {
                    using (Bitmap bitmap = new Bitmap(bmpOriginal, newW, newH))
                    {
                        using (Graphics g = Graphics.FromImage(bitmap))
                        {
                            //插值算法的质量
                            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                            g.DrawImage(bmpOriginal, new Rectangle(0, 0, newW, newH), new Rectangle(0, 0, bmpOriginal.Width, bmpOriginal.Height), GraphicsUnit.Pixel);
                        }

                        using (MemoryStream stream = new MemoryStream())
                        {
                            bitmap.Save(stream, ImageFormat.Jpeg);
                            data = new byte[stream.Length];
                            stream.Seek(0, SeekOrigin.Begin);
                            
                            stream.Read(data, 0, Convert.ToInt32(stream.Length));
                        }
                    }
                }

                return data;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 压缩图片
        /// </summary>
        /// <param name="photoFileName">图片字节数组</param>
        /// <param name="newW">压缩后宽度</param>
        /// <param name="newH">压缩后高度</param>
        /// <param name="Mode"></param>
        /// <returns></returns>
        public static byte[] KiResizeImage(byte[] imageBytes, int newW, int newH, int Mode = 1)
        {
            byte[] data = null;
            try
            {
                using (Bitmap bmpOriginal = new Bitmap(new MemoryStream(imageBytes)))
                {
                    using (Bitmap bitmap = new Bitmap(bmpOriginal, newW, newH))
                    {
                        using (Graphics g = Graphics.FromImage(bitmap))
                        {
                            //插值算法的质量
                            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                            g.DrawImage(bmpOriginal, new Rectangle(0, 0, newW, newH), new Rectangle(0, 0, bmpOriginal.Width, bmpOriginal.Height), GraphicsUnit.Pixel);
                        }

                        using (MemoryStream stream = new MemoryStream())
                        {
                            bitmap.Save(stream, ImageFormat.Jpeg);
                            data = new byte[stream.Length];
                            stream.Seek(0, SeekOrigin.Begin);
                            stream.Read(data, 0, Convert.ToInt32(stream.Length));
                        }
                    }
                }
                return data;
            }
            catch
            {
                return null;
            }
        }

        public static Bitmap GetThumbnail(Bitmap b, int destHeight, int destWidth)
        {
            Image imgSource = b;
            ImageFormat thisFormat = imgSource.RawFormat;
            int sW = 0, sH = 0;
            // 按比例缩放           
            int sWidth = imgSource.Width;
            int sHeight = imgSource.Height;
            if (sHeight > destHeight || sWidth > destWidth)
            {
                if ((sWidth * destHeight) > (sHeight * destWidth))
                {
                    sW = destWidth;
                    sH = (destWidth * sHeight) / sWidth;
                }
                else
                {
                    sH = destHeight;
                    sW = (sWidth * destHeight) / sHeight;
                }
            }
            else
            {
                sW = sWidth;
                sH = sHeight;
            }
            Bitmap outBmp = new Bitmap(destWidth, destHeight);
            Graphics g = Graphics.FromImage(outBmp);
            g.Clear(Color.Transparent);
            // 设置画布的描绘质量         
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(imgSource, new Rectangle((destWidth - sW) / 2, (destHeight - sH) / 2, sW, sH), 0, 0, imgSource.Width, imgSource.Height, GraphicsUnit.Pixel);
            g.Dispose();

            // 以下代码为保存图片时，设置压缩质量     
            EncoderParameters encoderParams = new EncoderParameters();
            long[] quality = new long[1];
            quality[0] = 100;
            EncoderParameter encoderParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
            encoderParams.Param[0] = encoderParam;
            imgSource.Dispose();
            return outBmp;

        }

        /// <summary>
        /// 保存图片
        /// </summary>
        /// <param name="imageBytes">图片字节数</param>
        /// <param name="saveFileName">图片保存名称，全路径名称</param>
        /// <returns></returns>
        public static bool SaveImage(byte[] imageBytes,string saveFileName)
        {
            try
            {
                new Bitmap(new MemoryStream(imageBytes)).Save(saveFileName, ImageFormat.Jpeg);
            }
            catch
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 保存图片
        /// </summary>
        /// <param name="imageBytes">图片字节数</param>
        /// <param name="saveDirectoryName">保存的文件夹名称</param>
        /// <param name="saveFileName">文件名</param>
        /// <param name="imageFormat">保存的图片格式</param>
        /// <returns></returns>
        public static bool SaveImage(byte[] imageBytes, string saveDirectoryName,string saveFileName, ImageExtensionFormat imageFormat= ImageExtensionFormat.jpg)
        {
            try
            {
                if (!string.IsNullOrEmpty(saveDirectoryName))
                {
                    if (!Directory.Exists(saveDirectoryName))
                    {
                        Directory.CreateDirectory(saveDirectoryName);
                    }
                    string saveFilePath = string.Format("{0}.{1}",Path.Combine(saveDirectoryName, Path.GetFileNameWithoutExtension(saveFileName)),imageFormat.ToString().ToLower());
                    new Bitmap(new MemoryStream(imageBytes)).Save(saveFilePath);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 另存为图片
        /// </summary>
        /// <param name="imageBytes">图片名称，全路径名称</param>
        /// <param name="saveFileName">图片另存为名称，全路径名称</param>
        /// <returns></returns>
        public static bool SaveAsImage(string sourceFileName, string saveFileName)
        {
            try
            {
                ReadImageFromFile(sourceFileName).Save(saveFileName, ImageFormat.Jpeg);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public enum ImageExtensionFormat
        {
            /// <summary>
            /// jpg格式
            /// </summary>
            jpg,

            /// <summary>
            /// png格式
            /// </summary>
            png,

            /// <summary>
            /// jpeg格式
            /// </summary>
            jpeg,

            /// <summary>
            /// bmp格式
            /// </summary>
            bmp,
        }

        #region 海明相似度计算

        /// <summary>
        /// 获取图片的D-hash值
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static ulong GetHash(Image image)
        {
            int hashSize = 8;
            //图片缩小到9*8的尺寸
            var thumbImage = Resize(image, hashSize + 1, hashSize);
            //获取灰度图片，灰度图片即把rgb转换成0~255的值
            var grayImage = GetGrayScaleVersion(thumbImage);

            ulong hash = 0;

            //遍历9*8像素点，记录相邻像素之间的对边关系，产生8*8=64个对比关系，对应ulong的64位
            for (int x = 0; x < hashSize; x++)
            {
                for (int y = 0; y < hashSize; y++)
                {
                    //比较当前像素点与下一个像素点的对比关系，如果当前像素点值较大则为1，否则为0
                    var largerThanNext = Math.Abs(grayImage.GetPixel(y, x).R) > Math.Abs(grayImage.GetPixel(y + 1, x).R);
                    if (largerThanNext)
                    {
                        var currentIndex = x * hashSize + y;
                        hash |= (1UL << currentIndex);
                    }
                }
            }

            return hash;
        }

        /// <summary>
        /// 计算两个hash值之间的汉明距离(即相似度)
        /// </summary>
        /// <param name="hash1"></param>
        /// <param name="hash2"></param>
        /// <returns></returns>
        public static double GetBitmapSimilarity(ulong hash1, ulong hash2)
        {
            return (64 - BitCount(hash1 ^ hash2)) / 64.0;
        }

        /// <summary>
        /// Bitcounts array used for BitCount method (used in Similarity comparisons).
        /// Don't try to read this or understand it, I certainly don't. Credit goes to
        /// David Oftedal of the University of Oslo, Norway for this. 
        /// http://folk.uio.no/davidjo/computing.php
        /// </summary>
        private static byte[] bitCounts = {
         0,1,1,2,1,2,2,3,1,2,2,3,2,3,3,4,1,2,2,3,2,3,3,4,2,3,3,4,3,4,4,5,1,2,2,3,2,3,3,4,
         2,3,3,4,3,4,4,5,2,3,3,4,3,4,4,5,3,4,4,5,4,5,5,6,1,2,2,3,2,3,3,4,2,3,3,4,3,4,4,5,
         2,3,3,4,3,4,4,5,3,4,4,5,4,5,5,6,2,3,3,4,3,4,4,5,3,4,4,5,4,5,5,6,3,4,4,5,4,5,5,6,
         4,5,5,6,5,6,6,7,1,2,2,3,2,3,3,4,2,3,3,4,3,4,4,5,2,3,3,4,3,4,4,5,3,4,4,5,4,5,5,6,
         2,3,3,4,3,4,4,5,3,4,4,5,4,5,5,6,3,4,4,5,4,5,5,6,4,5,5,6,5,6,6,7,2,3,3,4,3,4,4,5,
         3,4,4,5,4,5,5,6,3,4,4,5,4,5,5,6,4,5,5,6,5,6,6,7,3,4,4,5,4,5,5,6,4,5,5,6,5,6,6,7,
         4,5,5,6,5,6,6,7,5,6,6,7,6,7,7,8
         };

        /// <summary>
        /// 计算ulong中位值为1的个数
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        private static uint BitCount(ulong num)
        {
            uint count = 0;
            for (; num > 0; num >>= 8)
                count += bitCounts[(num & 0xff)];
            return count;
        }

        /// <summary>
        /// 修改图片尺寸
        /// </summary>
        /// <param name="originalImage"></param>
        /// <param name="newWidth"></param>
        /// <param name="newHeight"></param>
        /// <returns></returns>
        public static Image Resize(Image originalImage, int newWidth, int newHeight)
        {
            Image smallVersion = new Bitmap(newWidth, newHeight);
            using (Graphics g = Graphics.FromImage(smallVersion))
            {
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.DrawImage(originalImage, 0, 0, newWidth, newHeight);
            }

            return smallVersion;
        }

        private static ColorMatrix ColorMatrix = new ColorMatrix(
         new float[][]
         {
         new float[] {.3f, .3f, .3f, 0, 0},
         new float[] {.59f, .59f, .59f, 0, 0},
         new float[] {.11f, .11f, .11f, 0, 0},
         new float[] {0, 0, 0, 1, 0},
         new float[] {0, 0, 0, 0, 1}
         });

        /// <summary>
        /// 获取灰度图片
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        public static Bitmap GetGrayScaleVersion(Image original)
        {
            //http://www.switchonthecode.com/tutorials/csharp-tutorial-convert-a-color-image-to-grayscale
            //create a blank bitmap the same size as original
            Bitmap newBitmap = new Bitmap(original.Width, original.Height);

            //get a graphics object from the new image
            using (Graphics g = Graphics.FromImage(newBitmap))
            {
                //create some image attributes
                ImageAttributes attributes = new ImageAttributes();

                //set the color matrix attribute
                attributes.SetColorMatrix(ColorMatrix);

                //draw the original image on the new image
                //using the grayscale color matrix
                g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height),
                0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);
            }
            return newBitmap;
        }

        #endregion

        #region 读取单像素直线

        //public static Bitmap DrawLineImage(Bitmap bitmap)
        //{
        //    int w = bitmap.Width;//原宽
        //    int h = bitmap.Height;//原高
        //    //x：原图左侧到非透明范围左侧的长度
        //    //y：原图头部到非透明范围头部的高度
        //    //width：裁剪后图片宽度
        //    //height：裁剪后图片高度
        //    int x = 0, y = 0, width = 0, height = 0;
        //    Bitmap bmp = new Bitmap(w, h);

        //    Color colCurr;
        //    try
        //    {
        //        //key 为一组连续值list中的apla值最大的y坐标
        //        Dictionary<int, List<int>> dic = null;
        //        Dictionary<int, List<int>> dicTemp = null;
        //        Graphics g = Graphics.FromImage(bmp);
        //        Brush brush = new SolidBrush(Color.Black);
        //        Pen pen = new Pen(brush,1);
        //        pen.DashStyle = DashStyle.Solid;
        //        for (int i=0;i<w;i++)
        //        {
        //            dicTemp = new Dictionary<int, List<int>>();
        //            List<int> listV = new List<int>();
        //            int maxInt = 0;
        //            int maxKey = 0;
        //            for (int j=0;j<h;j++)
        //            {
        //                colCurr = bitmap.GetPixel(i, j);
        //                if (colCurr.A!=0)
        //                {
        //                    listV.Add(j);
        //                    if(maxInt < colCurr.A)
        //                    {
        //                        maxInt = colCurr.A;
        //                        maxKey = j;
        //                    }
        //                }
        //                else
        //                {
        //                    if(listV!=null&&listV.Count>0)
        //                    {
        //                        dicTemp.Add(maxKey, listV);
        //                        listV = new List<int>();
        //                        maxKey = 0;
        //                        maxInt = 0;
        //                    }
        //                }
        //            }
        //            if(dicTemp!=null&&dicTemp.Keys.Count>0)
        //            {
        //                foreach(int key in dicTemp.Keys)
        //                {
        //                    g.FillRectangle(brush, i, key, 1,1);
        //                    if (dic != null && dic.Keys.Count > 0)
        //                    {
        //                        int preYstation = -1;
        //                        int distance = h;
        //                        foreach (int k in dic.Keys)
        //                        {
        //                            int dist1 = Math.Abs(key - dic[k][0]);
        //                            int dist2 = Math.Abs(key - dic[k][dic[k].Count - 1]);
        //                            if (dist1 > dist2)
        //                            {
        //                                if(distance>=dist2)
        //                                {
        //                                    distance = dist2;
        //                                    preYstation = k;
        //                                }
        //                            }
        //                            else
        //                            {
        //                                if (distance >= dist1)
        //                                {
        //                                    distance = dist1;
        //                                    preYstation = k;
        //                                }
        //                            }
        //                        }

        //                        if (preYstation != -1)
        //                        {
        //                            int start = preYstation > key ? key : preYstation;
        //                            int end = preYstation > key ? preYstation : key;
        //                            for (int m = start + 1; m < end; m++)
        //                            {
        //                                g.FillRectangle(brush, i - 1, m, 1, 1);
        //                            }
        //                        }
        //                    }
        //                }
        //                dic = dicTemp;
        //            } 
        //        }
        //        g.Dispose();
        //    }
        //    catch(Exception ex)
        //    {
        //        LogHelper.WriteMessage("错误信息：" + ex.Message);
        //    }
        //    bmp.Save(@"C:\Users\Administrator\Desktop\2222.png");
        //    return bmp;
        //}

        public static Bitmap DrawLineImage(Bitmap bitmap)
        {
            int w = bitmap.Width;//原宽
            int h = bitmap.Height;//原高
            Bitmap bmp = new Bitmap(w, h);

            try
            {
                //key 为一组连续值list中的apla值最大的y坐标
                //Dictionary<int, List<int>> dic = null;
                //Dictionary<int, List<int>> dicTemp = null;
                Graphics g = Graphics.FromImage(bmp);
                Brush brush = new SolidBrush(Color.Black);
                Pen pen = new Pen(brush, 1);
                pen.DashStyle = DashStyle.Solid;
                for (int i = 0; i < w; i++)
                {
                    if(i==130)
                    {

                    }
                    List<int> yPostions = GetYCenterPosition(i, h, bitmap);

                    foreach(int y in yPostions)
                    {
                        List<int> xPostions = GetXCenterPosition(y, w, bitmap);
                        if(xPostions!=null&&xPostions.Count>0)
                        {
                            foreach (int x in xPostions)
                            {
                                g.FillRectangle(brush, x, y, 1, 1);
                            }
                            //if(i<xPostions[0])
                            //{
                            //    i = xPostions[0];
                            //}
                        }
                    }
                }
                g.Dispose();
            }
            catch (Exception ex)
            {
                LogHelper.WriteMessage("错误信息：" + ex.Message);
            }
            bmp.Save(@"C:\Users\Administrator\Desktop\2222.png");
            return bmp;
        }

        /// <summary>
        /// 横向或纵向连续有两个空白区域，则说该区域存在分支
        /// </summary>
        public static int dValue = 2;

        /// <summary>
        /// 通过指定的y坐标，获取有x效区域段的中心点集合
        /// </summary>
        /// <param name="yPosition"></param>
        /// <param name="width"></param>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        private static List<int> GetXCenterPosition(int yPosition,int width,Bitmap bitmap)
        {
            List<int> xPositions = new List<int>();
            Color colCurr;
            List<int> temp = new List<int>();
            int tempDisc = 0;
            for (int i=0;i<width;i++)
            {
                colCurr = bitmap.GetPixel(i, yPosition);
                if (colCurr.A != 0)
                {
                    temp.Add(i);
                    tempDisc = 0;
                }
                else
                {
                    tempDisc++;
                }
                if (temp != null && temp.Count > 0 && (tempDisc >= dValue || i == width - 1))
                {
                    int center = (temp[0] + temp[temp.Count - 1]) / 2;
                    xPositions.Add(center);
                    temp.Clear();
                }
            }
            return xPositions;
        }

        /// <summary>
        /// 通过指定的x坐标，获取有y效区域段的中心点集合
        /// </summary>
        /// <param name="xPosition"></param>
        /// <param name="height"></param>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        private static List<int> GetYCenterPosition(int xPosition, int height, Bitmap bitmap)
        {
            List<int> yPositions = new List<int>();
            Color colCurr;
            List<int> temp = new List<int>();
            int tempDisc = 0;
            for (int i = 0; i < height ; i++)
            {
                colCurr = bitmap.GetPixel(xPosition, i);
                if (colCurr.A != 0)
                {
                    temp.Add(i);
                    tempDisc = 0;
                }
                else
                {
                    tempDisc++;
                }
                if (temp!=null&& temp.Count>0 && (tempDisc >= dValue||i==height-1))
                {
                    int center = (temp[0] + temp[temp.Count - 1]) / 2;
                    yPositions.Add(center);
                    temp.Clear();
                }
            }
            return yPositions;
        }

        #endregion
    }
}
