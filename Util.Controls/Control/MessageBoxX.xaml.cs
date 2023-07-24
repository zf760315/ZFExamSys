using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Util.Controls;

namespace System.Windows
{
    /// <summary>
    /// MessageBoxXxaml.xaml 的交互逻辑
    /// </summary>
    public partial class MessageBoxX : WindowBase
    {
        /// <summary>
        /// 结果
        /// </summary>
        public ResultType Result { get; private set; }

        public EnumNotifyType enumNotifyType;

        private static readonly Dictionary<string, Brush> _Brushes = new Dictionary<string, Brush>();

        public MessageBoxX(EnumNotifyType type, string mes)
        {
            enumNotifyType = type;
            InitializeComponent();
            this.txtMessage.Text = mes;
            this.btnOK.Content = "确认";
            //type
            btnCancel.Visibility = Visibility.Collapsed;
            btnNo.Visibility = Visibility.Collapsed;
            this.SetForeground(type);

            Title = type.GetDescription();
            switch (type)
            {
                //错误
                case EnumNotifyType.Error:
                    this.ficon.Text = "\ue644";
                    break;
                //警告
                case EnumNotifyType.Warning:
                    this.ficon.Text = "\ue60b";
                    break;
                //提示
                case EnumNotifyType.Info:
                    this.ficon.Text = "\ue659";
                    break;
                //询问
                case EnumNotifyType.Question:
                    this.ficon.Text = "\ue60e";
                    this.btnOK.Content = "是";
                    this.btnNo.Visibility = Visibility.Visible;
                    this.btnCancel.Visibility = Visibility.Visible;
                    break;
                //确认
                case EnumNotifyType.Confirm:
                    this.ficon.Text = "\ue60e";
                    //this.btnNo.Visibility = Visibility.Visible;
                    this.btnCancel.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void SetForeground(EnumNotifyType type)
        {
            string key = type.ToSafeString() + "Foreground";
            if (!_Brushes.ContainsKey(key))
            {
                var b = this.TryFindResource(key) as Brush;
                _Brushes.Add(key, b);
            }
            this.Foreground = _Brushes[key];
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            this.Result = enumNotifyType != EnumNotifyType.Question ? ResultType.Ok : ResultType.Yes;
            this.Close();
            e.Handled = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Result = ResultType.Cancel;
            this.Close();
            e.Handled = true;
        }

        private void btnNo_Click(object sender, RoutedEventArgs e)
        {
            this.Result = ResultType.No;
            this.Close();
            e.Handled = true;
        }

        /********************* public static method **************************/

        /// <summary>
        /// 提示错误消息
        /// </summary>
        public static ResultType Error(string mes, Window owner = null)
        {
            return ShowMessage(EnumNotifyType.Error, mes, owner);
        }

        /// <summary>
        /// 提示普通消息
        /// </summary>
        public static ResultType Info(string mes, Window owner = null)
        {
            return ShowMessage(EnumNotifyType.Info, mes, owner);
        }

        /// <summary>
        /// 提示警告消息
        /// </summary>
        public static ResultType Warning(string mes, Window owner = null)
        {
            return ShowMessage(EnumNotifyType.Warning, mes, owner);
        }

        /// <summary>
        /// 提示询问消息
        /// </summary>
        public static ResultType Question(string mes, Window owner = null)
        {
            return ShowMessage(EnumNotifyType.Question, mes, owner);
        }

        /// <summary>
        /// 确认消息
        /// </summary>
        public static ResultType Confirm(string mes, Window owner = null)
        {
            return ShowMessage(EnumNotifyType.Confirm, mes, owner);
        }

        /// <summary>
        /// 显示提示消息框，
        /// owner指定所属父窗体，默认参数值为null，则指定主窗体为父窗体。
        /// </summary>
        public static ResultType ShowMessage(EnumNotifyType type, string mes, Window owner = null)
        {

            ResultType res = ResultType.Cancel;

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                MessageBoxX nb = new MessageBoxX(type, mes) { Title = type.GetDescription() };
                nb.Owner = owner ?? ControlHelper.GetTopWindow();
                nb.ShowDialog();
                res = nb.Result;
            }));

            //MessageBoxX nb = new MessageBoxX(type, mes) { Title = type.GetDescription() };
            //nb.Owner = owner ?? ControlHelper.GetTopWindow();
            //nb.ShowDialog();
            //res = nb.Result;

            return res;
        }

        /// <summary>
        /// 通知消息类型
        /// </summary>
        public enum EnumNotifyType
        {
            [Description("错误")]
            Error,

            [Description("警告")]
            Warning,

            [Description("提示信息")]
            Info,

            [Description("询问信息")]
            Question,

            [Description("确认信息")]
            Confirm,
        }

        public enum ResultType
        {
            [Description("确定")]
            Ok,
            [Description("是")]
            Yes,
            [Description("否")]
            No,
            [Description("取消")]
            Cancel,
        }
    }
}
