using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using ZFExamSys.Enum;

namespace ZFExamSys.Unity.Converters
{
    public class ResourceTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ResourceType)
            {
                //将enum转换成Descript
                var field = value.GetType().GetField(value.ToString());
                var customAttribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
                return customAttribute == null ? value.ToString() : ((DescriptionAttribute)customAttribute).Description;
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
