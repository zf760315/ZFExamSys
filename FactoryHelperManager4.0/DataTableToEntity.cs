
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Reflection;
using System.Text;

namespace FactoryHelperManager
{
    /// <summary>
    /// DataTable转实体类集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DataTableToEntity<T> where T : new()
    {
        /// <summary>
        /// table转实体集合
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static List<T> FillModel(DataTable dt)
        {
            List<T> result = new List<T>();
            if (dt == null || dt.Rows.Count == 0)
                return result;
            foreach (DataRow dr in dt.Rows)
            {
                try
                {
                    T res = new T();
                    for (int i = 0; i < dr.Table.Columns.Count; i++)
                    {
                        PropertyInfo propertyInfo = res.GetType().GetProperty(dr.Table.Columns[i].ColumnName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                        if (propertyInfo != null && dr[i] != DBNull.Value)
                        {
                            var value = dr[i];
                            switch (propertyInfo.PropertyType.FullName)
                            {
                                case "System.Decimal":
                                    propertyInfo.SetValue(res, Convert.ToDecimal(value), null); break;
                                case "System.String":
                                    propertyInfo.SetValue(res, value, null); break;
                                case "System.Int32":
                                    propertyInfo.SetValue(res, Convert.ToInt32(value), null); break;
                                case "System.Int64":
                                    propertyInfo.SetValue(res, Convert.ToInt32(value), null); break;
                                default:
                                    propertyInfo.SetValue(res, value, null); break;
                            }
                        }
                    }
                    result.Add(res);
                }
                catch (Exception ex)
                {
                    string msg = dr.Table.TableName + "表id：" + dr[0] + "表第二项值：" + dr[1] + " 导出异常,异常信息：" + ex.Message + "\r\n";
                    LogHelper.WriteMessage(msg);
                    //CommonMethod.SaveText(dr.Table.TableName+"表id：" + dr[0] + "表第二项值："+dr[1]+" 导出异常,异常信息："+ex.Message+"\r\n", Application.StartupPath + "\\outFiles" + "\\ErrorLog.txt");
                    continue;
                }
            }
            return result;
        }

        /// <summary>
        /// 读取json内容转成实体类集合
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static List<T> ReadDataToModel(string path)
        {
            StreamReader sr = new StreamReader(path);
            try
            {
                string temp = sr.ReadToEnd();

                return JsonConvert.DeserializeObject<List<T>>(temp);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                sr.Dispose();
                sr.Close();
            }
        }

        /// <summary>
        /// 实体类集合转table
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static DataTable FillDataTable(List<T> modelList)
        {
            if (modelList == null || modelList.Count == 0)
                return null;
            DataTable dt = CreatTable(modelList[0]);
            foreach (T model in modelList)
            {
                DataRow dr = dt.NewRow();
                foreach (PropertyInfo p in typeof(T).GetProperties())
                {
                    dr[p.Name] = p.GetValue(model, null);
                }
                dt.Rows.Add(dr);
            }
            return dt;
        }

        /// <summary>
        /// 根据实体创建table
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static DataTable CreatTable(T model)
        {
            DataTable dt = new DataTable(typeof(T).Name);
            foreach (PropertyInfo p in typeof(T).GetProperties())
            {
                dt.Columns.Add(new DataColumn(p.Name, p.PropertyType));
            }
            return dt;
        }

        #region 将DataTable的指定列的数据,转换为T类型的集合

        /// <summary>
        /// 将DataTable的指定列的数据,转换为&lt;T&gt;类型的集合。
        /// </summary>
        /// <typeparam name="T">要获取的DataTable的列集合的类型。</typeparam>
        /// <param name="dt">要获取列集合的DataTable。</param>
        /// <param name="useIndexOrName">指定是使用 columnIndex 参数,还是使用 columnName 参数。</param>
        /// <param name="columnIndex">要获取列集合的DataTable的从0开始的列索引。</param>
        /// <param name="columnName">要获取列集合的DataTable的列名。</param>
        /// <param name="removeRepetitive">指示是否移除重复的列数据。</param>
        /// <returns>DataTable的&lt;T&gt;类型的列集合,若不存在数据,则返回null。</returns>
        private static List<T> GetDataTableColumnListOf<T>(DataTable dt, bool useIndexOrName, int columnIndex, string columnName, bool removeRepetitive)
        {

            if (dt != null)
            {
                int count = dt.Rows.Count;
                if (count == 0 || (useIndexOrName ? (columnIndex < 0 || columnIndex >= dt.Columns.Count) : !dt.Columns.Contains(columnName)))
                {
                    return null;
                }

                //创建T类型的集合
                List<T> list = new List<T>();

                //获取T类型的类型
                Type TType = typeof(T);

                //获取T类型的转换器
                TypeConverter TConverter = TypeDescriptor.GetConverter(TType);

                //是否有非空数据
                bool hasNotEmptyData = false;

                for (int i = 0; i < count; i++)
                {
                    object obj = useIndexOrName ? dt.Rows[i][columnIndex] : dt.Rows[i][columnName];
                    if (obj != null)
                    {
                        //如果obj就是T类型,则可直接强制类型转换
                        if (obj is T)
                        {
                            T item = (T)obj;
                            if (!removeRepetitive || !list.Contains(item))
                            {
                                list.Add(item);
                                hasNotEmptyData = true;
                            }
                        }
                        else
                        {
                            //获取obj的类型
                            Type objType = obj.GetType();
                            if (string.IsNullOrEmpty(obj.ToString().Trim()))
                            {
                                if (!removeRepetitive)
                                {
                                    list.Add(default(T));
                                }
                            }
                            else if (TConverter != null && TConverter.CanConvertFrom(objType))
                            {
                                try
                                {
                                    T item = (T)TConverter.ConvertFrom(obj);
                                    if (!removeRepetitive || !list.Contains(item))
                                    {
                                        list.Add(item);
                                        hasNotEmptyData = true;
                                    }
                                }
                                catch { }
                            }
                            else
                            {
                                //获取obj类型的类型转换器
                                TypeConverter objConverter = TypeDescriptor.GetConverter(objType);

                                //如果类型转换器存在,且能转换为T类型
                                if (objConverter != null && objConverter.CanConvertTo(TType))
                                {
                                    try
                                    {
                                        //将obj转换为T类型
                                        T item = (T)objConverter.ConvertTo(obj, TType);
                                        if (!removeRepetitive || !list.Contains(item))
                                        {
                                            list.Add(item);
                                            hasNotEmptyData = true;
                                        }
                                    }
                                    catch { }
                                }
                            }
                        }
                    }
                    else
                    {
                        list.Add(default(T));
                    }
                }
                if (!hasNotEmptyData)
                {
                    return null;
                }
                return list.Count > 0 ? list : null;
            }
            return null;
        }

        /// <summary>
        /// 将DataTable的指定列的数据,转换为&lt;T&gt;类型的集合。
        /// </summary>
        /// <typeparam name="T">要获取的DataTable的列集合的类型。</typeparam>
        /// <param name="dt">要获取列集合的DataTable。</param>
        /// <param name="columnIndex">要获取列集合的DataTable的从0开始的列索引。</param>
        /// <returns>DataTable的&lt;T&gt;类型的列集合,若不存在数据,则返回null。</returns>
        public static List<T> GetDataTableColumnListOf<T>(DataTable dt, int columnIndex)
        {
            return GetDataTableColumnListOf<T>(dt, true, columnIndex, null, false);
        }

        /// <summary>
        /// 将DataTable的指定列的数据,转换为&lt;T&gt;类型的集合。
        /// </summary>
        /// <typeparam name="T">要获取的DataTable的列集合的类型。</typeparam>
        /// <param name="dt">要获取列集合的DataTable。</param>
        /// <param name="columnName">要获取列集合的DataTable的列名。</param>
        /// <returns>DataTable的&lt;T&gt;类型的列集合,若不存在数据,则返回null。</returns>
        public static List<T> GetDataTableColumnListOf<T>(DataTable dt, string columnName)
        {
            return GetDataTableColumnListOf<T>(dt, false, -1, columnName, false);
        }

        /// <summary>
        /// 将DataSet中第一张表的指定列的数据,转换为&lt;T&gt;类型的集合。
        /// </summary>
        /// <typeparam name="T">要获取的DataSet第一张表的列集合的类型。</typeparam>
        /// <param name="ds">要获取列集合的DataSet,该DataSet必须至少包含一张表。</param>
        /// <param name="columnIndex">要获取的DataSet第一张表的从0开始的列索引。</param>
        /// <returns>DataSet第一张表的&lt;T&gt;类型的列集合,若不存在数据,则返回null。</returns>
        public static List<T> GetDataTableColumnListOf<T>(DataSet ds, int columnIndex)
        {
            if (ds != null && ds.Tables.Count > 0)
            {
                return GetDataTableColumnListOf<T>(ds.Tables[0], true, columnIndex, null, false);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 将DataSet中第一张表的指定列的数据,转换为&lt;T&gt;类型的集合。
        /// </summary>
        /// <typeparam name="T">要获取的DataSet第一张表的列集合的类型。</typeparam>
        /// <param name="ds">要获取列集合的DataSet,该DataSet必须至少包含一张表。</param>
        /// <param name="columnName">要获取的DataSet第一张表的DataTable的列名。</param>
        /// <returns>DataSet第一张表的&lt;T&gt;类型的列集合,若不存在数据,则返回null。</returns>
        public static List<T> GetDataTableColumnListOf<T>(DataSet ds, string columnName)
        {
            if (ds != null && ds.Tables.Count > 0)
            {
                return GetDataTableColumnListOf<T>(ds.Tables[0], false, -1, columnName, false);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 将DataTable的指定列的数据,转换为&lt;T&gt;类型的集合。
        /// </summary>
        /// <typeparam name="T">要获取的DataTable的列集合的类型。</typeparam>
        /// <param name="dt">要获取列集合的DataTable。</param>
        /// <param name="columnIndex">要获取列集合的DataTable的从0开始的列索引。</param>
        /// <param name="removeRepetitive">指示是否移除重复的列数据。</param>
        /// <returns>DataTable的&lt;T&gt;类型的列集合,若不存在数据,则返回null。</returns>
        public static List<T> GetDataTableColumnListOf<T>(DataTable dt, int columnIndex, bool removeRepetitive)
        {
            return GetDataTableColumnListOf<T>(dt, true, columnIndex, null, removeRepetitive);
        }

        /// <summary>
        /// 将DataTable的指定列的数据,转换为&lt;T&gt;类型的集合。
        /// </summary>
        /// <typeparam name="T">要获取的DataTable的列集合的类型。</typeparam>
        /// <param name="dt">要获取列集合的DataTable。</param>
        /// <param name="columnName">要获取列集合的DataTable的列名。</param>
        /// <param name="removeRepetitive">指示是否移除重复的列数据。</param>
        /// <returns>DataTable的&lt;T&gt;类型的列集合,若不存在数据,则返回null。</returns>
        public static List<T> GetDataTableColumnListOf<T>(DataTable dt, string columnName, bool removeRepetitive)
        {
            return GetDataTableColumnListOf<T>(dt, false, -1, columnName, removeRepetitive);
        }

        /// <summary>
        /// 将DataSet中第一张表的指定列的数据,转换为&lt;T&gt;类型的集合。
        /// </summary>
        /// <typeparam name="T">要获取的DataSet第一张表的列集合的类型。</typeparam>
        /// <param name="ds">要获取列集合的DataSet,该DataSet必须至少包含一张表。</param>
        /// <param name="columnIndex">要获取的DataSet第一张表的从0开始的列索引。</param>
        /// <param name="removeRepetitive">指示是否移除重复的列数据。</param>
        /// <returns>DataSet第一张表的&lt;T&gt;类型的列集合,若不存在数据,则返回null。</returns>
        public static List<T> GetDataTableColumnListOf<T>(DataSet ds, int columnIndex, bool removeRepetitive)
        {
            if (ds != null && ds.Tables.Count > 0)
            {
                return GetDataTableColumnListOf<T>(ds.Tables[0], true, columnIndex, null, removeRepetitive);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 将DataSet中第一张表的指定列的数据,转换为&lt;T&gt;类型的集合。
        /// </summary>
        /// <typeparam name="T">要获取的DataSet第一张表的列集合的类型。</typeparam>
        /// <param name="ds">要获取列集合的DataSet,该DataSet必须至少包含一张表。</param>
        /// <param name="columnName">要获取的DataSet第一张表的DataTable的列名。</param>
        /// <param name="removeRepetitive">指示是否移除重复的列数据。</param>
        /// <returns>DataSet第一张表的&lt;T&gt;类型的列集合,若不存在数据,则返回null。</returns>
        public static List<T> GetDataTableColumnListOf<T>(DataSet ds, string columnName, bool removeRepetitive)
        {
            if (ds != null && ds.Tables.Count > 0)
            {
                return GetDataTableColumnListOf<T>(ds.Tables[0], false, -1, columnName, removeRepetitive);
            }
            else
            {
                return null;
            }
        }

        #endregion
    }
}
