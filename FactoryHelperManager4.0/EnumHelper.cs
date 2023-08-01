using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FactoryHelperManager
{
    public class EnumHelper
    {
        public static List<T> GetDescriptionByEnum<T>()
        {
            List<T> list = new List<T>();

            Type t = typeof(T);
            FieldInfo[] fieldInfos = t.GetFields();
            
            foreach (FieldInfo field in fieldInfos)
            {
                if (field.FieldType.IsEnum)
                {
                    object[] objs = field.GetCustomAttributes(typeof(DescriptionAttribute), false);    //获取描述属性
                    if (objs!=null && objs.Length > 0)   
                    {
                        DescriptionAttribute descriptionAttribute = (DescriptionAttribute)objs[0];
                        list.Add((T)field.GetValue(null));
                    }
                }
            }
            return list;
        }

        public static T GetEnumByDescription<T>(string description) where T : class
        {
            FieldInfo[] fields = typeof(T).GetFields();
            foreach (FieldInfo field in fields)
            {
                object[] objs = field.GetCustomAttributes(typeof(DescriptionAttribute), false);    //获取描述属性
                if (objs.Length > 0 && (objs[0] as DescriptionAttribute).Description == description)
                {
                    return (T)field.GetValue(null);
                }
            }

            throw new ArgumentException(string.Format("{0} 未能找到对应的枚举.", description), "Description");
        }
    }
}
