using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FactoryHelperManager
{
    /// <summary>
    /// 使用类的属性名对应DataTable中的字段名 
    /// </summary>
    public  class TableToModelByAttributeName
    {

        /// <summary>
        /// DataRow扩展方法：将DataRow类型转化为指定类型的实体
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <returns></returns>
        public static T ToModel<T>(DataRow dr) where T : class, new()
        {
            return ToModel<T>(dr, true);
        }

        /// <summary>
        /// 将DataRow类型转化为指定类型的实体
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <returns></returns>
        public static T ToModel<T>(DataRow dr, bool dateTimeToString) where T : class, new()
        {
            if (dr != null)
                return ToList<T>(dr.Table, dateTimeToString).First();
            return null;
        }

        /// <summary>
        /// 将DataTable类型转化为指定类型的实体集合
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <returns></returns>
        public static List<T> ToList<T>(DataTable dt) where T : class, new()
        {
            return ToList<T>(dt, true);
        }

        /// <summary>
        ///将DataTable类型转化为指定类型的实体集合
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="dateTimeToString">是否需要将日期转换为字符串，默认为转换,值为true</param>
        /// <returns></returns>
        public static List<T> ToList<T>(DataTable dt, bool dateTimeToString) where T : class, new()
        {
            List<T> list = new List<T>();
            if (dt != null)
            {
                List<PropertyInfo> infos = new List<PropertyInfo>();
                Array.ForEach(typeof(T).GetProperties(), p =>
                {
                    if (dt.Columns.Contains(p.Name) == true)
                    {
                        infos.Add(p);
                    }
                });
                SetListByModelAttributeName<T>(list, infos, dt, dateTimeToString);

            }
            return list;
        }

        #region 私有方法

        private static void SetListByModelAttributeName<T>(List<T> list, List<PropertyInfo> infos, DataTable dt, bool dateTimeToString) where T : class, new()
        {
            foreach (DataRow dr in dt.Rows)
            {
                T model = new T();
                infos.ForEach(p =>
                {
                    if (dr[p.Name] != DBNull.Value)
                    {
                        object tempValue = dr[p.Name];
                        if (dr[p.Name].GetType() == typeof(DateTime) && dateTimeToString)
                        {
                            tempValue = dr[p.Name].ToString();
                        }
                        try
                        {
                            Type type = null;
                            if (p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                            {
                                type = p.PropertyType.GetGenericArguments()[0];
                            }
                            else
                            {
                                type = p.PropertyType;
                            }

                            if (type == typeof(DateTime)&&dateTimeToString)
                            {
                                p.SetValue(model, tempValue, null);
                            }
                            else 
                            {
                                p.SetValue(model, Convert.ChangeType(tempValue, type), null);
                            }
                        }
                        catch
                        {
                        }
                    }
                });
                list.Add(model);
            }
        }

        #endregion

        //public enum TableToModelType
        //{
        //    /// <summary>
        //    /// 匹配属性名称
        //    /// </summary>
        //    UsingAttributeName = 0,

        //    /// <summary>
        //    /// 匹配属性说明（）
        //    /// </summary>
        //    UsingAttributeDescription = 1,
        //}

    }

    
}
