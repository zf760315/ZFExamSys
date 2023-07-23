using Command;
using FactoryHelperManager;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace ZFExamSys.ViewModels
{
    public class ImageViewModel:BindableBase
    {
        private const double BigZoom = 3;

        private const double SmallZoom = -0.5;

        private readonly Cursor cursor = Cursors.Hand;


        #region 字段

        private string title = "图片查看器";

        private List<BitmapImage> images = new List<BitmapImage>();

        private int index = 0;

        private BitmapImage actbitmapImage = null;

        private double top = 0;

        private double left = 0;

        private double width = 0;

        private double height = 0;

        private double addZoom = 0.1;

        private double zoom = 0;

        #endregion

        #region 属性

        /// <summary>
        /// 窗口标题
        /// </summary>
        public string Title
        {
            get { return title; }
            set { SetProperty(ref title, value); }
        }

        /// <summary>
        /// 图像集合
        /// </summary>
        public List<BitmapImage> Images
        {
            get { return images; }
            set { SetProperty(ref images, value); }
        }

        /// <summary>
        /// 当前图片index
        /// </summary>
        public int Index
        {
            get { return index; }
            set { SetProperty(ref index, value); }
        }

        /// <summary>
        /// 显示图像
        /// </summary>
        public BitmapImage ActbitmapImage
        {
            get { return actbitmapImage; }
            set { SetProperty(ref actbitmapImage, value); }
        }

        /// <summary>
        /// 距离顶端位置
        /// </summary>
        public double Top
        {
            get { return top; }
            set { SetProperty(ref top, value); }
        }

        /// <summary>
        /// 距离左端位置
        /// </summary>
        public double Left
        {
            get { return left; }
            set { SetProperty(ref left, value); }
        }

        /// <summary>
        /// 图像宽度
        /// </summary>
        public double Width
        {
            get { return width; }
            set { SetProperty(ref width, value); }
        }

        /// <summary>
        /// 图像高度
        /// </summary>
        public double Height
        {
            get { return height; }
            set { SetProperty(ref height, value); }
        }

        /// <summary>
        /// 缩放增量值
        /// </summary>
        public double AddZoom
        {
            get { return addZoom; }
            set { SetProperty(ref addZoom, value); }
        }

        /// <summary>
        /// 缩放值
        /// </summary>
        public double Zoom
        {
            get { return zoom; }
            set { SetProperty(ref zoom, value); }
        }

        #endregion

        #region 事件绑定

        private DelegateCommand _loadCommand;

        /// <summary>
        /// 加载图片
        /// </summary>
        public DelegateCommand LoadCommand =>
            _loadCommand ?? (_loadCommand = new DelegateCommand(LoadPic));

        private DelegateCommand<object> _mouseDownCommand;

        /// <summary>
        /// 鼠标按下
        /// </summary>
        public DelegateCommand<object> MouseDownCommand =>
            _mouseDownCommand ?? (_mouseDownCommand = new DelegateCommand<object>(MouseDown));

        private DelegateCommand<object> _previewMouseMoveCommand;

        /// <summary>
        /// 鼠标拖动
        /// </summary>
        public DelegateCommand<object> PreviewMouseMoveCommand =>
            _previewMouseMoveCommand ?? (_previewMouseMoveCommand = new DelegateCommand<object>(PreviewMouseMove));

        private DelegateCommand<object> _mouseUpCommand;

        /// <summary>
        /// 鼠标抬起
        /// </summary>
        public DelegateCommand<object> MouseUpCommand =>
            _mouseUpCommand ?? (_mouseUpCommand = new DelegateCommand<object>(MouseUp));

        private DelegateCommand<object> _previewMouseWheelCommand;

        /// <summary>
        /// 鼠标滚轮
        /// </summary>
        public DelegateCommand<object> PreviewMouseWheelCommand =>
            _previewMouseWheelCommand ?? (_previewMouseWheelCommand = new DelegateCommand<object>(PreviewMouseWheel));

        #endregion

        public void LoadPic()
        {
            string bdp = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ModelPic");
            if (Directory.Exists(bdp))
            {
                string[] files = Directory.GetFiles(bdp);
                LoadImages(files);
            }
        }

        /// <summary>
        /// 切换当前图片
        /// </summary>
        /// <param name="select"></param>
        public void ChangeSelect(int select)
        {
            if (select < 0 || images.Count < 1)
                return;
            if (select >= images.Count)
                select = images.Count - 1;

            ActbitmapImage = Images[select];
            Width = actbitmapImage.PixelWidth;
            Height = actbitmapImage.Height;
        }

        /// <summary>
        /// 加载文件到集合中
        /// </summary>
        /// <param name="files"></param>
        public void LoadImages(string[] files)
        {
            images.Clear();
            foreach(string fileName in files)
            {
                if (File.Exists(fileName))
                {
                    BitmapImage bitmap = new BitmapImage(new Uri(fileName));
                    images.Add(bitmap);
                }
            }
            
            index = 0;
            ChangeSelect(index);
        }

        Point prePoint;
        public void MouseDown(object o)
        {
            if(o is ExCommandParameter ecp)
            {
                (ecp.Sender as FrameworkElement).Cursor = cursor;
                if (ecp.EventArgs is MouseEventArgs e)
                {
                    prePoint = e.GetPosition(LogicalTreeHelper.GetParent(ecp.Sender) as FrameworkElement);
                }
            }
        }

        public void PreviewMouseMove(object o)
        {
            if(o is ExCommandParameter ecp)
            {
                if(ecp.EventArgs is MouseEventArgs e)
                {
                    if ((ecp.Parameter as FrameworkElement).Cursor == cursor)
                    {
                        Point point = e.GetPosition(ecp.Sender as FrameworkElement);
                        if (prePoint != null)
                        {
                            if (point.X != prePoint.X || prePoint.Y != point.Y)
                            {
                                Left += point.X - prePoint.X;
                                Top += point.Y - prePoint.Y;
                                prePoint = point;
                            }
                        }
                    }
                }
            }
        }

        public void MouseUp(object o)
        {
            if (o is ExCommandParameter ecp)
            {
                (ecp.Sender as FrameworkElement).Cursor = Cursors.Arrow;
            }
        }

        public void PreviewMouseWheel(object o)
        {
            if (o is ExCommandParameter ecp)
            {
                if (ecp.EventArgs is MouseWheelEventArgs e)
                {
                    if (e.Delta > 0)
                    {
                        if (Math.Round(Zoom + AddZoom, 4) <= BigZoom)
                        {
                            Zoom = Math.Round(Zoom + AddZoom, 4);
                        }
                    }
                    else
                    {
                        if (Math.Round(Zoom - AddZoom, 4) >= SmallZoom)
                        {
                            Zoom = Math.Round(Zoom - AddZoom, 4);
                        }
                    }

                    Point point = e.GetPosition(e.Device.Target);
                    Left -= Math.Round(ActbitmapImage.PixelWidth * (1 + Zoom) * point.X / Width - point.X, 4);
                    Top -= Math.Round(ActbitmapImage.PixelHeight * (1 + Zoom) * point.Y / Height - point.Y, 4);

                    Width = Math.Round(ActbitmapImage.PixelWidth * (1 + Zoom), 4);
                    Height = Math.Round(ActbitmapImage.PixelHeight * (1 + Zoom), 4);
                }
            }
        }
    }
}
