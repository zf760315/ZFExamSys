using ZFExamSys.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ZFExamSys.UserControls
{
    /// <summary>
    /// ImageControl.xaml 的交互逻辑
    /// </summary>
    public partial class ImageControl : UserControl
    {
        public ImageControl()
        {
            InitializeComponent();
            //DataContext = ImageModel;
        }

        //public static readonly DependencyProperty ImageModelProperty = 
        //    DependencyProperty.Register("ImageModel", typeof(ImageViewModel), typeof(ImageControl));

        //public ImageViewModel ImageModel
        //{
        //    get { return (ImageViewModel)GetValue(ImageModelProperty); }
        //    set { SetValue(ImageModelProperty, value); }
        //}
    }
}
