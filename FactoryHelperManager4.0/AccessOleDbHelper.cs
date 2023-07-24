using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;

namespace FactoryHelperManager
{
    public class AccessOleDbHelper
    {
        /// <summary>
        /// 连接字符串
        /// </summary>
        public string connectString
        {
            set
            {
            }
            get
            {
                return string.Format(Const.DataAccessConnectStringFormat, Const.passWord, Const.dataPath);
            }
        }

        public AccessOleDbHelper()
        {

        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="dataPath">access数据库路径</param>
        /// <param name="password">access数据库密码</param>
        public AccessOleDbHelper(string dataPath,string password="")
        {
            Const.dataPath = dataPath;
            Const.passWord = password;
            this.connectString= string.Format(Const.DataAccessConnectStringFormat, Const.passWord, Const.dataPath);
        }

        private static object o = new object();

        /// <summary>
        /// 读取取Access中所有的表既数据
        /// </summary>
        /// <param name="error">错误信息</param>
        /// <returns></returns>
        public Dictionary<string, DataTable> GetAccessAllTableAndData(ref string error)
        {
            Dictionary<string, DataTable> dsDic = new Dictionary<string, DataTable>();
            try
            {
                using (OleDbConnection connection = new OleDbConnection(connectString))
                {
                    connection.Open();
                    DataTable dt = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "table" });
                    if (dt != null && dt.Rows.Count >= 1)
                    {
                        DataSet ds = new DataSet();
                        foreach (DataRow dr in dt.Rows)
                        {
                            string tableName = dr["Table_Name"].ToString();
                            string selectSQL = string.Format(" SELECT * FROM {0}", tableName);
                            OleDbDataAdapter objDataAdapter = new OleDbDataAdapter(selectSQL, connection);
                            objDataAdapter.Fill(ds, tableName);
                            lock (o)
                            {
                                dsDic.Add(tableName, ds.Tables[0]);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteMessage("读取Access文件时错误：" + ex.Message);
                if (ex.Message.Contains("正由另一进程使用"))
                {
                    error = "请关闭打开的Access文件，再进行操作。";
                }
                else
                {
                    error = "读取Access文件时错误：" + ex.Message;
                }
            }
            return dsDic;
        }

        /// <summary>
        /// 获取DataTable对象
        /// </summary>
        /// <param name="connSting">数据库连接字符串</param>
        /// <param name="strSql">T-SQL语句,一般为以Select开头的查询语句</param>
        /// <returns>DataTable对象或null</returns>
        public DataTable GetDataTable(string strSql)
        {
            using (OleDbConnection oleConn = new OleDbConnection(connectString))
            {
                OleDbDataAdapter oleDa = new OleDbDataAdapter(strSql, oleConn);
                DataTable dt = new DataTable();
                try
                {
                    oleConn.Open();
                    oleDa.Fill(dt);
                    if (dt != null && !dt.Rows.Count.Equals(0))
                    {
                        return dt;
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (OleDbException ex)
                {
                    throw ex;
                }
            }
        }

        /// <summary>
        /// 对Access数据库或Excel文件指定指定的SQL语句
        /// </summary>
        /// <param name="connString">数据库连接字符串</param>
        /// <param name="strSql">T-SQL语句,一般为增删改语句</param>
        /// <param name="paramArray">OleDbParameter类型可变长参数数组</param>
        /// <returns>执行SQL语句所影响的行数</returns>
        public int ExcuteSQL(string strSql, params OleDbParameter[] paramArray)
        {
            using (OleDbConnection oleConn = new OleDbConnection(connectString))
            {
                using (OleDbCommand oleComm = new OleDbCommand(strSql, oleConn))
                {
                    try
                    {
                        oleConn.Open();
                        if (paramArray != null && paramArray.Length > 0)
                        {
                            oleComm.Parameters.AddRange(paramArray);
                        }
                        return oleComm.ExecuteNonQuery();
                    }
                    catch (OleDbException ex)
                    {
                        throw ex;
                    }
                }
            }
        }

        #region 批量执行添加修改、修改或删除操作

        /// <summary>
        /// 批量执行添加、修改或删除操作。
        /// </summary>
        /// <param name="sqlList">要执行添加、修改或删除操作的SQL语句列表。</param>
        /// <param name="parasList">要执行添加、修改或删除操作的SQL语句的对应参数列表。</param>
        /// <returns>返回添加、修改或删除操作所执行成功的数据条数。</returns>
        public int AddUpdateOrDeleteList(List<string> sqlList, List<OleDbParameter[]> parasList)
        {
            if (sqlList != null && parasList != null)
            {
                int totalCount = sqlList.Count;
                if (totalCount > 0 && totalCount == parasList.Count)
                {
                    //执行成功数据条数
                    int successCount = 0;
                    using (OleDbConnection oldConn = new OleDbConnection(connectString))
                    {
                        using (OleDbCommand oleComm = new OleDbCommand())
                        {
                            oleComm.Connection = oldConn;
                            oleComm.CommandType = CommandType.Text;

                            try
                            {
                                oldConn.Open();
                                for (int i = 0; i < totalCount; i++)
                                {
                                    string sql = sqlList[i];
                                    OleDbParameter[] paras = parasList[i];
                                    oleComm.CommandText = sql;
                                    if (paras != null)
                                    {
                                        oleComm.Parameters.AddRange(paras);
                                    }
                                    int result = oleComm.ExecuteNonQuery();
                                    if (result > 0)
                                    {
                                        successCount++;
                                    }
                                    oleComm.Parameters.Clear();
                                }
                            }
                            catch
                            {
                                return -1;
                            }
                            return successCount;
                        }
                    }
                }
            }
            return -1;
        }

        #endregion

        /// <summary>
        /// 检测access连接数据引擎版本
        /// </summary>
        /// <param name="accessFilePath"></param>
        /// <param name="passWord"></param>
        public static void CheckedOleDbConnection(string accessFilePath,string passWord="")
        {
            try
            {
                Const.dataPath = accessFilePath;
                Const.passWord = passWord;
                string cConnectString = string.Format(Const.DataAccessConnectStringFormat, Const.passWord, Const.dataPath);
               
                if(!string.IsNullOrEmpty(cConnectString))
                {
                    using (OleDbConnection oleConn = new OleDbConnection(cConnectString))
                    {
                        oleConn.Open();
                    }
                }
            }
            catch (Exception ex)
            {
                
                LogHelper.WriteMessage("系统access数据库引擎过低，修改连接数据库引擎为2003版。");
                Const.DataAccessConnectStringFormat = "Provider=Microsoft.Jet.OLEDB.4.0;Jet OLEDB:Database Password={0};User Id=admin;Data Source='{1}';Persist Security Info=True";
            }
            
        }

    }

    public class DbfOleDbHelper
    {

        /// <summary>
        /// 连接字符串
        /// </summary>
        public string connectString
        {
            set
            {
            }
            get
            {
                return string.Format(Const.DataAccessConnectStringFormat, Const.passWord, Const.dataPath);
            }
        }

        public DbfOleDbHelper()
        {

        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="dataPath">access数据库路径</param>
        public DbfOleDbHelper(string dataPath)
        {
            Const.dbfDataPath = dataPath;
            this.connectString = string.Format(Const.DataDBFConnectionStringFormat,Const.dbfDataPath);
        }

        /// <summary>
        /// 读取取DBF中的表数据
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public DataTable GetDBFAllTableAndData(ref string error)
        {
            DataTable dt = new DataTable();
            try
            {
                string filename = Path.GetFileNameWithoutExtension(Const.dbfDataPath);
                OleDbConnection conn = new OleDbConnection();
                conn.ConnectionString = connectString;
                conn.Open();
                string sql = @"select * from " + filename;
                OleDbDataAdapter da = new OleDbDataAdapter(sql, conn);
                da.Fill(dt);
            }
            catch (Exception ex)
            {
                LogHelper.WriteMessage("读取DBF文件时错误：" + ex.Message);
                if (ex.Message.Contains("正由另一进程使用"))
                {
                    error = "请关闭打开的DBF文件，再进行操作。";
                }
                else
                {
                    error = "读取DBF文件时错误：" + ex.Message;
                }
            }
            return dt;
        }

    }
}
