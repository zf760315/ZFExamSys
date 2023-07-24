using Aspose.Cells;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace FactoryHelperManager
{
    public class ExcelDataHelper
    {

        /// <summary>
        /// 读取指定路径的Excel文件为DataTable
        /// </summary>
        /// <param name="fileName">文件全路径</param>
        /// <param name="firstRow">要开始读取的第一行在Excel中的行索引（默认会以此行各列数据作为DataTable的列名）</param>
        /// <param name="firstColumn">要开始读取的第一列在Excel中的列索引</param>
        /// <param name="invalidEndRowNumber">文件尾部无效行数目（该部分所有行数据将不会被读取）</param>
        /// <returns>返回DataTable</returns>
        public static DataTable ReadExcel(string fileName, string sheetName, int firstRow, int firstColumn, int invalidEndRowNumber)
        {
            Cells cells = null;
            try
            {
                Workbook workbook = new Workbook(fileName);
                if (workbook != null)
                {
                    Worksheet worksheet = workbook.Worksheets[sheetName];
                    if (worksheet != null)
                    {
                        cells = workbook.Worksheets[sheetName].Cells;
                    }

                }
            }
            catch { }
            if (cells == null)
            {
                return null;
            }
            DataTable dt = cells.ExportDataTable(firstRow, firstColumn, cells.MaxDataRow + 1 - invalidEndRowNumber, cells.MaxDataColumn + 1);
            return dt;
        }

        /// <summary>
        /// 读取指定路径的Excel文件为DataTable
        /// </summary>
        /// <param name="stream">文件流</param>
        /// <param name="firstRow">要开始读取的第一行在Excel中的行索引（默认会以此行各列数据作为DataTable的列名）</param>
        /// <param name="firstColumn">要开始读取的第一列在Excel中的列索引</param>
        /// <param name="invalidEndRowNumber">文件尾部无效行数目（该部分所有行数据将不会被读取）</param>
        /// <returns>返回DataTable</returns>
        public static DataTable ReadExcel(Stream stream, string sheetName, int firstRow, int firstColumn, int invalidEndRowNumber)
        {
            Cells cells = null;
            try
            {
                Workbook workbook = new Workbook(stream);
                if (workbook != null)
                {
                    Worksheet worksheet = workbook.Worksheets[sheetName];
                    if (worksheet != null)
                    {
                        cells = worksheet.Cells;
                    }
                    else
                    {
                        worksheet = workbook.Worksheets[0];
                        if (worksheet != null)
                        {
                            cells = worksheet.Cells;
                        }
                    }
                }
            }
            catch { }
            if (cells == null)
            {
                return null;
            }
            DataTable dt = cells.ExportDataTable(firstRow, firstColumn, cells.MaxDataRow + 1 - invalidEndRowNumber, cells.MaxDataColumn + 1);
            return dt;
        }

        #region 将指定Excel文件中的数据,读取到 DataTable 中

        /// <summary>
        /// 所有英文字母的字符串,此字段是常数。
        /// </summary>
        private const string AllLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        /// <summary>
        /// 将指定Excel文件中的数据,读取到 DataTable 中。
        /// </summary>
        /// <param name="excelFileName">要读取的Excel文件的全路径文件名。</param>
        /// <param name="sheetName">Excel的Sheet表名称，为空时默认为第一个Sheet</param>
        /// <param name="columnHeadStartRowIndex">Excel文件的列名起始行索引(从0开始),列名可以是合并的单元格,这将是 DataTable 的列名。</param>
        /// <param name="dataStartRowIndex">Excel文件的数据起始行索引(从0开始)。</param>
        /// <param name="dt">用于存储Excel文件数据的 DataTable,可以为null。</param>
        /// <param name="errorMessage">返回读取Excel文件时的错误信息。若未出错,则返回 string.Empty。</param>
        /// <param name="checkPKColumn">指示是否检测主键列(主键列的信息不能重复)。</param>
        /// <param name="pkColumnsIndex">若检测主键列(checkPKColumn为true),此数组包含主键列的列索引(从0开始)集合,若为null,将仅检测第0列。</param>
        /// <param name="allowEmptyPKColumnData">若检测主键列(checkPKColumn为true),此值指示是否允许空的主键列数据。</param>
        /// <returns>读取Excel文件中的数据成功,返回true,否则,返回false,错误信息将通过errorMessage返回。</returns>
        public static bool ReadExcelToDataTable(string excelFileName,string sheetName, int columnHeadStartRowIndex, int dataStartRowIndex, ref DataTable dt, ref string errorMessage, bool checkPKColumn, int[] pkColumnsIndex, bool allowEmptyPKColumnData)
        {
            if (dt != null)
            {
                dt.Dispose();
            }
            dt = new DataTable();

            try
            {
                dt.TableName = Path.GetFileNameWithoutExtension(excelFileName);
                //实例一个Workbook
                Workbook wb = new Workbook(excelFileName);
                Worksheet sheet = null;
                //实例一个sheet 这里面只针对excel中只有一个sheet
                if (string.IsNullOrEmpty(sheetName))
                {
                    sheet = wb.Worksheets[0];
                }
                else
                {
                    sheet = wb.Worksheets[sheetName];
                }

                if (sheet != null)
                {
                    Cells cells = sheet.Cells;
                    //列数
                    int columnCount = cells.MaxDataColumn + 1;
                    //行数(包含标题)
                    int rowCount = cells.Rows.Count;

                    #region 创建dt列名

                    //创建dt列名
                    for (int i = columnHeadStartRowIndex; i < dataStartRowIndex; i++)
                    {
                        for (int j = 0; j < columnCount; j++)
                        {
                            if (sheet.Cells[i, j].Value != null)
                            {
                                //处理合并居中的单元格
                                if (sheet.Cells[i, j].IsMerged)
                                {
                                    Range range = sheet.Cells[i, j].GetMergedRange();
                                    if (range.RowCount > 1)
                                    {
                                        if (!dt.Columns.Contains(sheet.Cells[i, j].StringValue))
                                        {
                                            dt.Columns.Add(sheet.Cells[i, j].StringValue);
                                        }
                                    }
                                    if (range.ColumnCount > 1)
                                    {
                                        for (int k = 0; k < range.ColumnCount; k++)
                                        {
                                            if (!dt.Columns.Contains(sheet.Cells[i + 1, j + k].StringValue))
                                            {
                                                dt.Columns.Add(sheet.Cells[i + 1, j + k].StringValue);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (!dt.Columns.Contains(sheet.Cells[i, j].StringValue))
                                    {
                                        dt.Columns.Add(sheet.Cells[i, j].StringValue);
                                    }
                                }
                            }
                        }
                    }

                    #endregion

                    #region 读取数据

                    int allNum = 0, rightNum = 0;
                    Dictionary<string, int> dicPKValues = new Dictionary<string, int>();
                    //读取数据
                    for (int i = dataStartRowIndex; i < rowCount; i++)
                    {
                        //如果检验主键列
                        if (checkPKColumn)
                        {
                            #region 校验主键列

                            //如果主键列索引未提供,则默认校验第0列
                            if (pkColumnsIndex == null || pkColumnsIndex.Length == 0)
                            {
                                pkColumnsIndex = new int[] { 0 };
                            }
                            //存储主键列内容以"-"相连的文本
                            StringBuilder pkValues = new StringBuilder();
                            //指示是否跳过当前行,若遇到主键列内容为空的,则跳过当前行
                            bool skip = false;
                            //以"-"拼接主键列的内容
                            foreach (int pkIndex in pkColumnsIndex)
                            {
                                if (sheet.Cells[i, pkIndex].Value == null)
                                {
                                    if (allowEmptyPKColumnData)
                                    {
                                        skip = true;
                                        break;
                                    }
                                    else
                                    {
                                        dt.Clear();
                                        errorMessage = string.Format("读取Excel失败，Excel中在“{0}”单元格有空的数据！", GetExcelCellName(i, pkIndex));
                                        return false;
                                    }
                                }
                                string value = sheet.Cells[i, pkIndex].StringValue.Trim();
                                if (string.IsNullOrEmpty(value))
                                {
                                    if (allowEmptyPKColumnData)
                                    {
                                        skip = true;
                                        break;
                                    }
                                    else
                                    {
                                        dt.Clear();
                                        errorMessage = string.Format("读取Excel失败，Excel中在“{0}”单元格有空的数据！", GetExcelCellName(i, pkIndex));
                                        return false;
                                    }
                                }
                                pkValues.Append(value).Append("-");
                            }
                            //跳过当前行
                            if (skip)
                            {
                                allNum++;
                                continue;
                            }
                            //去掉最后一个-
                            string pkName = pkValues.ToString().TrimEnd('-');
                            pkValues.Length = 0;
                            pkValues = null;
                            if (!dicPKValues.ContainsKey(pkName))
                            {
                                dicPKValues.Add(pkName, i + 1);
                            }
                            else
                            {
                                dt.Clear();
                                errorMessage = string.Format("读取Excel失败，Excel表中第{0}行与第{1}行数据重复，请认真核对！", i + 1, dicPKValues[pkName]);
                                return false;
                            }

                            #endregion
                        }
                        else
                        {
                            object obj = sheet.Cells[i, 0].StringValue;
                            if (obj != null)
                            {
                                if (string.IsNullOrEmpty(obj.ToString().Trim()))
                                {
                                    allNum++;
                                    continue;
                                }
                            }
                            else
                            {
                                continue;
                            }
                        }

                        #region 读取正确的数据

                        DataRow dr = dt.NewRow();
                        for (int j = 0; j < dt.Columns.Count; j++)
                        {
                            object obj = sheet.Cells[i, j].Value;
                            if (obj != null)
                            {
                                dr[j] = obj.ToString().Trim();
                            }

                        }
                        dt.Rows.Add(dr);
                        rightNum++;
                        allNum++;

                        #endregion

                    }
                    if (rightNum != allNum)
                    {
                        if (rightNum > 0)
                        {
                            errorMessage = string.Format("Excel表中原有数据{0}行，有效数据{1}行，请认真核对！", allNum, rightNum);
                            return true;
                        }
                        else
                        {
                            errorMessage = "读取Excel失败，未读取到任何有效的数据，请检查Excel文件内容！";
                            return false;
                        }
                    }

                    #endregion
                }
            }
            catch
            {
                dt.Clear();
                errorMessage = "读取Excel失败，请确保文件是正确的Excel文件格式或Excel文件未被使用！";
                return false;
            }
            int dataRowCount = dt.Rows.Count;
            errorMessage = dataRowCount > 0 ? string.Empty : "读取Excel失败，未读取到任何有效的数据，请检查Excel文件内容！";
            return dataRowCount > 0;
        }

        /// <summary>
        /// 将指定Excel文件中的数据,读取到 DataTable 中。
        /// </summary>
        /// <param name="excelFileName">要读取的Excel文件的全路径文件名。</param>
        /// <param name="sheetName">Excel的Sheet表名称，为空时默认为第一个Sheet</param>
        /// <param name="columnHeadStartRowIndex">Excel文件的列名起始行索引(从0开始),列名可以是合并的单元格,这将是 DataTable 的列名。</param>
        /// <param name="dataStartRowIndex">Excel文件的数据起始行索引(从0开始)。</param>
        /// <param name="dt">用于存储Excel文件数据的 DataTable,可以为null。</param>
        /// <param name="errorMessage">返回读取Excel文件时的错误信息。若未出错,则返回 string.Empty。</param>
        /// <param name="checkPKColumn">指示是否检测主键列(主键列的信息不能重复)。</param>
        /// <param name="pkColumnsIndex">若检测主键列(checkPKColumn为true),此数组包含主键列的列索引(从0开始)集合,若为null,将仅检测第0列。</param>
        /// <returns>读取Excel文件中的数据成功,返回true,否则,返回false,错误信息将通过errorMessage返回。</returns>
        public static bool ReadExcelToDataTable(string excelFileName,string sheetName, int columnHeadStartRowIndex, int dataStartRowIndex, ref DataTable dt, ref string errorMessage, bool checkPKColumn, int[] pkColumnsIndex)
        {
            return ReadExcelToDataTable(excelFileName, sheetName, columnHeadStartRowIndex, dataStartRowIndex, ref dt, ref errorMessage, checkPKColumn, pkColumnsIndex, false);
        }

        /// <summary>
        /// 将指定Excel文件中的数据,读取到 DataTable 中。
        /// </summary>
        /// <param name="excelFileName">要读取的Excel文件的全路径文件名。</param>
        /// <param name="sheetName">Excel的Sheet表名称，为空时默认为第一个Sheet</param>
        /// <param name="columnHeadStartRowIndex">Excel文件的列名起始行索引(从0开始),列名可以是合并的单元格,这将是 DataTable 的列名。</param>
        /// <param name="dataStartRowIndex">Excel文件的数据起始行索引(从0开始)。</param>
        /// <param name="dt">用于存储Excel文件数据的 DataTable,可以为null。</param>
        /// <param name="checkPKColumn">指示是否检测主键列(主键列的信息不能重复)。</param>
        /// <param name="pkColumnsIndex">若检测主键列(checkPKColumn为true),此数组包含主键列的列索引(从0开始)集合,若为null,将仅检测第0列。</param>
        /// <returns>读取Excel文件中的数据成功,返回true,否则,返回false。</returns>
        public static bool ReadExcelToDataTable(string excelFileName, string sheetName, int columnHeadStartRowIndex, int dataStartRowIndex, ref DataTable dt, bool checkPKColumn, int[] pkColumnsIndex)
        {
            string errorMessage = string.Empty;
            return ReadExcelToDataTable(excelFileName, sheetName, columnHeadStartRowIndex, dataStartRowIndex, ref dt, ref errorMessage, checkPKColumn, pkColumnsIndex, false);
        }

        /// <summary>
        /// 将指定Excel文件中的数据,读取到 DataTable 中。
        /// </summary>
        /// <param name="excelFileName">要读取的Excel文件的全路径文件名。</param>
        /// <param name="sheetName">Excel的Sheet表名称，为空时默认为第一个Sheet</param>
        /// <param name="columnHeadStartRowIndex">Excel文件的列名起始行索引(从0开始),列名可以是合并的单元格,这将是 DataTable 的列名。</param>
        /// <param name="dataStartRowIndex">Excel文件的数据起始行索引(从0开始)。</param>
        /// <param name="dt">用于存储Excel文件数据的 DataTable,可以为null。</param>
        /// <param name="checkPKColumn">指示是否检测主键列(主键列的信息不能重复)。</param>
        /// <param name="pkColumnsIndex">若检测主键列(checkPKColumn为true),此数组包含主键列的列索引(从0开始)集合,若为null,将仅检测第0列。</param>
        /// <param name="allowEmptyPKColumnData">若检测主键列(checkPKColumn为true),此值指示是否允许空的主键列数据。</param>
        /// <returns>读取Excel文件中的数据成功,返回true,否则,返回false。</returns>
        public static bool ReadExcelToDataTable(string excelFileName, string sheetName, int columnHeadStartRowIndex, int dataStartRowIndex, ref DataTable dt, bool checkPKColumn, int[] pkColumnsIndex, bool allowEmptyPKColumnData)
        {
            string errorMessage = string.Empty;
            return ReadExcelToDataTable(excelFileName, sheetName, columnHeadStartRowIndex, dataStartRowIndex, ref dt, ref errorMessage, checkPKColumn, pkColumnsIndex, allowEmptyPKColumnData);
        }

        /// <summary>
        /// 将指定Excel文件中的数据,读取到 DataTable 中。
        /// </summary>
        /// <param name="excelFileName">要读取的Excel文件的全路径文件名。</param>
        /// <param name="sheetName">Excel的Sheet表名称，为空时默认为第一个Sheet</param>
        /// <param name="columnHeadStartRowIndex">Excel文件的列名起始行索引(从0开始),列名可以是合并的单元格,这将是 DataTable 的列名。</param>
        /// <param name="dataStartRowIndex">Excel文件的数据起始行索引(从0开始)。</param>
        /// <param name="dt">用于存储Excel文件数据的 DataTable,可以为null。</param>
        /// <param name="errorMessage">返回读取Excel文件时的错误信息。若未出错,则返回 string.Empty。</param>
        /// <returns>读取Excel文件中的数据成功,返回true,否则,返回false,错误信息将通过errorMessage返回。</returns>
        public static bool ReadExcelToDataTable(string excelFileName, string sheetName, int columnHeadStartRowIndex, int dataStartRowIndex, ref DataTable dt, ref string errorMessage)
        {
            return ReadExcelToDataTable(excelFileName, sheetName, columnHeadStartRowIndex, dataStartRowIndex, ref dt, ref errorMessage, false, null);
        }

        /// <summary>
        /// 将指定Excel文件中的数据,读取到 DataTable 中。
        /// </summary>
        /// <param name="excelFileName">要读取的Excel文件的全路径文件名。</param>
        /// <param name="sheetName">Excel的Sheet表名称，为空时默认为第一个Sheet</param>
        /// <param name="columnHeadStartRowIndex">Excel文件的列名起始行索引(从0开始),列名可以是合并的单元格,这将是 DataTable 的列名。</param>
        /// <param name="dataStartRowIndex">Excel文件的数据起始行索引(从0开始)。</param>
        /// <param name="dt">用于存储Excel文件数据的 DataTable,可以为null。</param>
        /// <returns>读取Excel文件中的数据成功,返回true,否则,返回false。</returns>
        public static bool ReadExcelToDataTable(string excelFileName,string sheetName,int columnHeadStartRowIndex, int dataStartRowIndex, ref DataTable dt)
        {
            string errorMessage = string.Empty;
            return ReadExcelToDataTable(excelFileName, sheetName, columnHeadStartRowIndex, dataStartRowIndex, ref dt, ref errorMessage, false, null);
        }

        /// <summary>
        /// 将指定Excel文件中的数据,读取到 字典 中。
        /// </summary>
        /// <param name="excelFileName">要读取的Excel文件的全路径文件名。</param>
        /// <param name="columnHeadStartRowIndex">Excel文件的列名起始行索引(从0开始),列名可以是合并的单元格,这将是 DataTable 的列名。</param>
        /// <param name="dataStartRowIndex">Excel文件的数据起始行索引(从0开始)。</param>
        /// <param name="disDic">用于存储Excel文件每个sheet数据的 Dictionary,可以为null。</param>
        /// <param name="errorMessage">返回读取Excel文件时的错误信息。若未出错,则返回 string.Empty。</param>
        /// <param name="checkPKColumn">指示是否检测主键列(主键列的信息不能重复)。</param>
        /// <param name="pkColumnsIndex">若检测主键列(checkPKColumn为true),此数组包含主键列的列索引(从0开始)集合,若为null,将仅检测第0列。</param>
        /// <param name="allowEmptyPKColumnData">若检测主键列(checkPKColumn为true),此值指示是否允许空的主键列数据。</param>
        /// <returns>读取Excel文件中的数据成功,返回true,否则,返回false,错误信息将通过errorMessage返回。</returns>
        public static bool ReadExcelToDictionary(string excelFileName, int columnHeadStartRowIndex, int dataStartRowIndex, ref Dictionary<string, DataTable> refDisDic, ref string errorMessage)
        {
            if (refDisDic != null)
            {
                refDisDic.Clear();
            }

            refDisDic = new Dictionary<string, DataTable>();

            try
            {

                //实例一个Workbook
                Workbook wb = new Workbook(excelFileName);

                //实例一个sheet 这里面只针对excel中只有一个sheet
                //Worksheet sheet = wb.Worksheets[0];

                foreach (Worksheet sheet in wb.Worksheets)
                {
                    DataTable dt = new DataTable();


                    //sheet名称
                    //string sheetName = sheet.Name;

                    Cells cells = sheet.Cells;
                    //列数
                    int columnCount = cells.MaxDataColumn + 1;
                    //行数(包含标题)
                    int rowCount = cells.Rows.Count;

                    #region 创建dt列名

                    //创建dt列名
                    for (int i = columnHeadStartRowIndex; i < dataStartRowIndex; i++)
                    {
                        for (int j = 0; j < columnCount; j++)
                        {
                            if (sheet.Cells[i, j].Value != null)
                            {
                                //处理合并居中的单元格
                                if (sheet.Cells[i, j].IsMerged)
                                {
                                    Range range = sheet.Cells[i, j].GetMergedRange();
                                    if (range.RowCount > 1)
                                    {
                                        if (!dt.Columns.Contains(sheet.Cells[i, j].StringValue))
                                        {
                                            dt.Columns.Add(sheet.Cells[i, j].StringValue);
                                        }
                                    }
                                    if (range.ColumnCount > 1)
                                    {
                                        for (int k = 0; k < range.ColumnCount; k++)
                                        {
                                            if (!dt.Columns.Contains(sheet.Cells[i + 1, j + k].StringValue))
                                            {
                                                dt.Columns.Add(sheet.Cells[i + 1, j + k].StringValue);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (!dt.Columns.Contains(sheet.Cells[i, j].StringValue))
                                    {
                                        dt.Columns.Add(sheet.Cells[i, j].StringValue);
                                    }
                                }
                            }
                        }
                    }

                    #endregion

                    #region 读取数据

                    int allNum = 0, rightNum = 0;
                    Dictionary<string, int> dicPKValues = new Dictionary<string, int>();
                    //读取数据
                    for (int i = dataStartRowIndex; i < rowCount; i++)
                    {
                        object obj = sheet.Cells[i, 0].StringValue;
                        if (obj != null)
                        {
                            if (string.IsNullOrEmpty(obj.ToString().Trim()))
                            {
                                allNum++;
                                continue;
                            }
                        }
                        else
                        {
                            continue;
                        }


                        #region 读取正确的数据

                        DataRow dr = dt.NewRow();
                        for (int j = 0; j < dt.Columns.Count; j++)
                        {
                            object obj2 = sheet.Cells[i, j].Value;
                            if (obj2 != null)
                            {
                                dr[j] = obj2.ToString().Trim();
                            }

                        }
                        dt.Rows.Add(dr);
                        rightNum++;
                        allNum++;

                        #endregion

                    }
                    if (rightNum != allNum)
                    {
                        if (rightNum > 0)
                        {
                            errorMessage = string.Format("Excel表中原有数据{0}行，有效数据{1}行，请认真核对！", allNum, rightNum);

                            return true;
                        }
                        else
                        {
                            errorMessage = "读取Excel失败，未读取到任何有效的数据，请检查Excel文件内容！";

                            return false;
                        }
                    }

                    #endregion

                    refDisDic.Add(sheet.Name, dt);

                };
            }
            catch
            {
                refDisDic.Clear();
                errorMessage = "读取Excel失败，请确保文件是正确的Excel文件格式或Excel文件未被使用！";
                return false;
            }
            int sheetCount = refDisDic.Keys.Count;
            errorMessage = sheetCount > 0 ? string.Empty : "读取Excel失败，未读取到任何有效的数据，请检查Excel文件内容！";
            return sheetCount > 0;

        }

        /// <summary>
        /// 获取指定行列的Excel单元格的名称(如第0行,第1列则为B1单元格)。
        /// </summary>
        /// <param name="rowIndex">单元格在Excel中的从0开始的行索引。</param>
        /// <param name="colIndex">单元格在Excel中的从0开始的列索引。</param>
        /// <returns>该单元格的单元格名称(如第0行,第1列则为B1单元格)。</returns>
        public static string GetExcelCellName(int rowIndex, int colIndex)
        {
            if (rowIndex >= 0 && colIndex >= 0)
            {
                int times = colIndex / 26 - 1;
                int col = colIndex % 26;
                return string.Format("{0}{1}{2}", times > 0 ? AllLetters[times].ToString() : string.Empty, AllLetters[col], rowIndex + 1);
            }
            else
            {
                return string.Empty;
            }
        }

        #endregion

        #region 导出DataTable数据到Excel中

        /// <summary>
        /// 保存DataTable集合数据
        /// </summary>
        /// <param name="dicDataTable"></param>
        /// <param name="filePath"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static bool SaveDataTablesExcel(Dictionary<string, DataTable> dicDataTable, string filePath, ref string msg)
        {
            //创建一工作簿
            Workbook workbook = new Workbook();
            workbook.Worksheets.Clear();
            foreach (string item in dicDataTable.Keys)
            {
                workbook.Worksheets.Add(item);
            }

            for (int j = 0; j < workbook.Worksheets.Count; j++)
            {
                DataTable dt = dicDataTable[workbook.Worksheets[j].Name];
                //获取其工作表
                Worksheet sheet = workbook.Worksheets[j];
                //所有的单元格集合
                Cells cells = sheet.Cells;
                //表头行样式
                Style styleHead = workbook.Styles[workbook.Styles.Add()];
                styleHead.HorizontalAlignment = TextAlignmentType.Center;
                styleHead.Font.Name = "宋体";
                styleHead.Font.Size = 11;
                styleHead.Font.IsBold = true;
                styleHead.IsTextWrapped = true;
                styleHead.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
                styleHead.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;
                styleHead.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;
                styleHead.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;

                //数据行样式 
                Style styleData = workbook.Styles[workbook.Styles.Add()];
                styleData.HorizontalAlignment = TextAlignmentType.Center;
                styleData.Font.Name = "宋体";
                styleData.Font.Size = 10;
                styleData.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
                styleData.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;
                styleData.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;
                styleData.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;

                //列名
                List<string> columnNames = new List<string>();
                //列宽
                List<double> columnWidths = new List<double>();

                //DataTable的列数
                int colCount = dt.Columns.Count;
                //DataTable的行数
                int rowCount = dt.Rows.Count;

                int headRowHeight = 0;

                int dataRowHeight = 0;

                //表头行行高
                cells.SetRowHeight(0, headRowHeight > 0 ? headRowHeight : 18);
                //生成列名行
                for (int dgvColIndex = 0, cellColIndex = 0; dgvColIndex < colCount; dgvColIndex++)
                {
                    Cell cell = cells[0, cellColIndex];
                    //设置列名行的标题
                    cell.PutValue(cellColIndex < columnNames.Count && !string.IsNullOrEmpty(columnNames[cellColIndex]) ? columnNames[cellColIndex] : dt.Columns[dgvColIndex].ColumnName);
                    //设置列名行单元格样式
                    cell.SetStyle(styleHead);
                    //设置列名行单元格的列宽
                    cells.SetColumnWidth(cellColIndex, cellColIndex < columnWidths.Count && columnWidths[cellColIndex] > 0 ? columnWidths[cellColIndex] : 20);

                    cellColIndex++;
                }

                //生成数据行
                for (int cellRowIndex = 0; cellRowIndex < rowCount; cellRowIndex++)
                {
                    //数据行行高
                    cells.SetRowHeight(cellRowIndex + 1, dataRowHeight > 0 ? dataRowHeight : 12.75);
                    for (int dgvColIndex = 0, cellColIndex = 0; dgvColIndex < colCount; dgvColIndex++)
                    {
                        Cell cell = cells[cellRowIndex + 1, cellColIndex];
                        //设置数据行的内容
                        object data = dt.Rows[cellRowIndex][dgvColIndex];
                        cell.PutValue(data);
                        //设置数据行的样式
                        cell.SetStyle(styleData);
                        cellColIndex++;
                    }
                }
            }
            try
            {
                workbook.Save(filePath);
                return true;
            }
            catch { return false; }
        }

        /// <summary>
        /// 将指定DataTable中的所有行的指定列的数据,导入到指定的Excel文件中,并可以设置Excel的工作表名称、表头行行高、数据行行高、列名及列宽。
        /// </summary>
        /// <param name="dt">要导出数据到Excel文件中的DataTable。</param>
        /// <param name="excelFileName">要导出DataTable数据的Excel全文件名。</param>
        /// <param name="sheetName">Excel的工作表名称,若为null或string.Empty,则为默认的sheet1。</param>
        /// <param name="headRowHeight">Excel的表头行行高,若小于等于0,则为默认的18。</param>
        /// <param name="dataRowHeight">Excel的数据行行高,若小于等于0,则为默认的12.75。</param>
        /// <param name="ignoreDataTableColumnIndexList">忽略的DataTable的从0开始的列索引集合,这将指定DataTable中,哪些列将被忽略,不被导入到Excel中,若为null,则默认导出DataTable的所有列。</param>
        /// <param name="columnNameList">导出到Excel中的列名集合,这将指定Excel中的表头行的列名,若为null,则所有列的列名默认为DataTable的列名,若无需设置某列列名,则可在列名集合中,指定对应的列名为string.Empty,则该列的列名默认为DataTable的对应列列名。</param>
        /// <param name="columnWidthList">导出到Excel中的各列列宽集合,这将指定Excel中的表头行的列宽,若为null,则所有列的列宽默认为20,若无需设置某列列宽,则可在列宽集合中,指定对应的列宽为一个小于等于0的值(如-1),则该列的列宽默认为20。</param>
        /// <param name="errorMessage">导出成功,返回string.Empty,若导出失败,则返回指定的错误信息。</param>
        /// <returns>导出成功,返回true,否则,返回false。</returns>
        /// <exception cref="System.ArgumentNullException">DataTable不能为 null！</exception>
        public static bool OutputDataTableToExcel(Dictionary<DataTable, string> dtAndSheetName, string excelFileName, double headRowHeight, double dataRowHeight, IEnumerable<int> ignoreDataTableColumnIndexList, IEnumerable<string> columnNameList, IEnumerable<double> columnWidthList, ref string errorMessage,string primaryKey="")
        {

            //创建一工作簿
            Workbook workbook = new Workbook();
            workbook.Worksheets.Clear();
            int dtCount = 0;
            foreach (DataTable dt in dtAndSheetName.Keys)
            {
                if (dt == null)
                {
                    throw new ArgumentNullException("dt", "DataTable不能为 null！");
                }
                if (dt.Rows.Count == 0)
                {
                    errorMessage = dt.TableName + "没有任何数据可导出！";
                    continue;
                }
                else
                {
                    dtCount++;
                }

                Worksheet sheet = null;
                //获取其工作表 
                if (!string.IsNullOrEmpty(dtAndSheetName[dt]))
                {
                    workbook.Worksheets.Add(dtAndSheetName[dt]);
                    sheet = workbook.Worksheets[dtAndSheetName[dt]];
                }
                else
                {
                    workbook.Worksheets.Add(string.Format("sheet{0}", dtCount));
                    sheet = workbook.Worksheets[string.Format("sheet{0}", dtCount)];
                }
                //所有的单元格集合
                Cells cells = sheet.Cells;

                //表头行样式Styles[workbook.Styles.Add()]
                Style styleHead = workbook.CreateStyle();
                styleHead.HorizontalAlignment = TextAlignmentType.Center;
                styleHead.Font.Name = "宋体";
                styleHead.Font.Size = 11;
                styleHead.Font.IsBold = true;
                styleHead.IsTextWrapped = true;
                styleHead.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
                styleHead.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;
                styleHead.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;
                styleHead.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;

                //数据行样式 
                Style styleData = workbook.CreateStyle();
                styleData.HorizontalAlignment = TextAlignmentType.Center;
                styleData.VerticalAlignment = TextAlignmentType.Center;
                styleData.Font.Name = "宋体";
                styleData.Font.Size = 10;
                styleData.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
                styleData.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;
                styleData.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;
                styleData.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;

                //忽略的DataTable列索引
                List<int> ignoreColumnIndexs = new List<int>();
                if (ignoreDataTableColumnIndexList != null)
                {
                    int count = dt.Columns.Count;
                    foreach (int index in ignoreDataTableColumnIndexList)
                    {
                        if (index >= 0 && index < count && !ignoreColumnIndexs.Contains(index))
                        {
                            ignoreColumnIndexs.Add(index);
                        }
                    }
                }
                //列名
                List<string> columnNames = columnNameList != null ? new List<string>(columnNameList) : new List<string>();
                //列宽
                List<double> columnWidths = columnWidthList != null ? new List<double>(columnWidthList) : new List<double>();

                //DataTable的列数
                int colCount = dt.Columns.Count;
                //DataTable的行数
                int rowCount = dt.Rows.Count;

                //表头行行高
                cells.SetRowHeight(0, headRowHeight > 0 ? headRowHeight : 18);
                //生成列名行
                for (int dgvColIndex = 0, cellColIndex = 0; dgvColIndex < colCount; dgvColIndex++)
                {
                    if (ignoreColumnIndexs.Count > 0 && ignoreColumnIndexs.Contains(dgvColIndex))
                    {
                        //忽略此列
                        continue;
                    }
                    Cell cell = cells[0, cellColIndex];
                    //设置列名行的标题
                    cell.PutValue(cellColIndex < columnNames.Count && !string.IsNullOrEmpty(columnNames[cellColIndex]) ? columnNames[cellColIndex] : dt.Columns[dgvColIndex].ColumnName);
                    //设置列名行单元格样式
                    cell.SetStyle(styleHead);
                    //设置列名行单元格的列宽
                    cells.SetColumnWidth(cellColIndex, cellColIndex < columnWidths.Count && columnWidths[cellColIndex] > 0 ? columnWidths[cellColIndex] : 20);

                    cellColIndex++;
                }
                int mergeRowCount = 0;
                string primaryKeyStr = string.Empty;
                if (!string.IsNullOrEmpty(primaryKey))
                {
                    primaryKey = dt.Columns.Contains(primaryKey) ? primaryKey : dt.Columns[0].ColumnName;
                    primaryKeyStr = dt.Rows[0][primaryKey].ToString();
                }
                //生成数据行
                for (int cellRowIndex = 0; cellRowIndex < rowCount; cellRowIndex++)
                {
                    #region 判断主列是否要合并

                    bool isMege = false;
                    if (!string.IsNullOrEmpty(primaryKeyStr))
                    {
                        if (cellRowIndex == rowCount - 1)
                        {
                            isMege = primaryKeyStr == dt.Rows[cellRowIndex][primaryKey].ToString() ? true : false;
                            mergeRowCount++;
                        }
                        else
                        {
                            isMege = primaryKeyStr != dt.Rows[cellRowIndex][primaryKey].ToString() ? true : false;
                            primaryKeyStr = dt.Rows[cellRowIndex][primaryKey].ToString();
                        }
                    }

                    #endregion

                    //数据行行高
                    cells.SetRowHeight(cellRowIndex + 1, dataRowHeight > 0 ? dataRowHeight : 12.75);
                    for (int dgvColIndex = 0, cellColIndex = 0; dgvColIndex < colCount; dgvColIndex++)
                    {
                        if (ignoreColumnIndexs.Count > 0 && ignoreColumnIndexs.Contains(dgvColIndex))
                        {
                            //忽略此列
                            continue;
                        }

                        Cell cell = cells[cellRowIndex + 1, cellColIndex];
                        //设置数据行的内容
                        object data = dt.Rows[cellRowIndex][dgvColIndex];
                        if (data is DateTime)
                        {
                            cell.PutValue(string.Format("{0:yyyy-MM-dd HH:mm:ss}", data));
                        }
                        else
                        {
                            cell.PutValue(data);
                        }
                        //设置数据行的样式
                        cell.SetStyle(styleData);
                        cellColIndex++;

                        if (isMege)
                        {
                            int mergeRow = cellRowIndex == rowCount - 1 ? cellRowIndex - mergeRowCount+2 : cellRowIndex - mergeRowCount + 1;
                            List<DataRow> dataRows = dt.Rows.Cast<DataRow>().ToList().GetRange(mergeRow-1, mergeRowCount);
                            if (dataRows != null && dataRows.Count > 0)
                            {
                                bool needMege = dataRows.All(dr => dr[dgvColIndex].ToString() == dataRows[0][dgvColIndex].ToString());
                                if (needMege)
                                {
                                    cells.Merge(mergeRow, dgvColIndex, mergeRowCount, 1);
                                }
                                cells[cellRowIndex, dgvColIndex].SetStyle(styleData);
                            }
                        }

                    }
                    if (!string.IsNullOrEmpty(primaryKeyStr))
                    {
                        if (isMege)
                        {
                            mergeRowCount = 1;
                        }
                        else if (primaryKeyStr == dt.Rows[cellRowIndex][primaryKey].ToString())
                            mergeRowCount++;
                    }
                }
            }

            try
            {
                workbook.Save(excelFileName);
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = "导出Excel失败，请确保该Excel文件未被使用！";
            }


            return false;
        }

        /// <summary>
        /// 将指定DataTable中的所有行的指定列的数据,导入到指定的Excel文件中,并可以设置Excel的工作表名称、表头行行高、数据行行高、列名及列宽。
        /// </summary>
        /// <param name="dt">要导出数据到Excel文件中的DataTable。</param>
        /// <param name="excelFileName">要导出DataTable数据的Excel全文件名。</param>
        /// <param name="sheetName">Excel的工作表名称,若为null或string.Empty,则为默认的sheet1。</param>
        /// <param name="headRowHeight">Excel的表头行行高,若小于等于0,则为默认的18。</param>
        /// <param name="dataRowHeight">Excel的数据行行高,若小于等于0,则为默认的12.75。</param>
        /// <param name="ignoreDataTableColumnIndexList">忽略的DataTable的从0开始的列索引集合,这将指定DataTable中,哪些列将被忽略,不被导入到Excel中,若为null,则默认导出DataTable的所有列。</param>
        /// <param name="columnNameList">导出到Excel中的列名集合,这将指定Excel中的表头行的列名,若为null,则所有列的列名默认为DataTable的列名,若无需设置某列列名,则可在列名集合中,指定对应的列名为string.Empty,则该列的列名默认为DataTable的对应列列名。</param>
        /// <param name="columnWidthList">导出到Excel中的各列列宽集合,这将指定Excel中的表头行的列宽,若为null,则所有列的列宽默认为20,若无需设置某列列宽,则可在列宽集合中,指定对应的列宽为一个小于等于0的值(如-1),则该列的列宽默认为20。</param>
        /// <param name="errorMessage">导出成功,返回string.Empty,若导出失败,则返回指定的错误信息。</param>
        /// <param name="primaryKey">数据列主键,该列信息若有重复项，则合并该列单元格，其他列若有相同项，则合并相同数据。</param>
        /// <returns>导出成功,返回true,否则,返回false。</returns>
        /// <exception cref="System.ArgumentNullException">DataTable不能为 null！</exception>
        public static bool OutputDataTableToExcel(DataTable dt, string excelFileName, string sheetName, double headRowHeight, double dataRowHeight, IEnumerable<int> ignoreDataTableColumnIndexList, IEnumerable<string> columnNameList, IEnumerable<double> columnWidthList, ref string errorMessage, string primaryKey="")
        {
            Dictionary<DataTable, string> dtAndSheetName = new Dictionary<DataTable, string>();
            dtAndSheetName.Add(dt, sheetName);

            return OutputDataTableToExcel(dtAndSheetName, excelFileName, headRowHeight, dataRowHeight, ignoreDataTableColumnIndexList, columnNameList, columnWidthList, ref errorMessage,primaryKey);
        }

        /// <summary>
        /// 将指定DataTable中的所有行的数据,导入到指定的Excel文件中。
        /// </summary>
        /// <param name="dt">要导出数据到Excel文件中的DataTable控件。</param>
        /// <param name="excelFileName">要导出DataTable数据的Excel全文件名。</param>
        /// <param name="errorMessage">导出成功,返回string.Empty,若导出失败,则返回指定的错误信息。</param>
        /// <returns>导出成功,返回true,否则,返回false。</returns>
        /// <exception cref="System.ArgumentNullException">DataTable不能为 null！</exception>
        public static bool OutputDataTableToExcel(DataTable dt, string excelFileName, ref string errorMessage)
        {
            return OutputDataTableToExcel(dt, excelFileName, string.Empty, -1, -1, null, null, null, ref errorMessage);
        }

        /// <summary>
        /// 将指定DataTable中的所有行的数据,导入到指定的Excel文件中,并可以设置Excel的工作表名称。
        /// </summary>
        /// <param name="dt">要导出数据到Excel文件中的DataTable控件。</param>
        /// <param name="excelFileName">要导出DataTable数据的Excel全文件名。</param>
        /// <param name="sheetName">Excel的工作表名称,若为null或string.Empty,则为默认的sheet1。</param>
        /// <param name="errorMessage">导出成功,返回string.Empty,若导出失败,则返回指定的错误信息。</param>
        /// <returns>导出成功,返回true,否则,返回false。</returns>
        /// <exception cref="System.ArgumentNullException">DataTable不能为 null！</exception>
        public static bool OutputDataTableToExcel(DataTable dt, string excelFileName, string sheetName, ref string errorMessage)
        {
            return OutputDataTableToExcel(dt, excelFileName, sheetName, -1, -1, null, null, null, ref errorMessage);
        }

        /// <summary>
        /// 将指定DataTable中的所有行的数据,导入到指定的Excel文件中,并可以设置Excel的表头行行高及数据行行高。
        /// </summary>
        /// <param name="dt">要导出数据到Excel文件中的DataTable控件。</param>
        /// <param name="excelFileName">要导出DataTable数据的Excel全文件名。</param>
        /// <param name="headRowHeight">Excel的表头行行高,若小于等于0,则为默认的18。</param>
        /// <param name="dataRowHeight">Excel的数据行行高,若小于等于0,则为默认的12.75。</param>
        /// <param name="errorMessage">导出成功,返回string.Empty,若导出失败,则返回指定的错误信息。</param>
        /// <returns>导出成功,返回true,否则,返回false。</returns>
        /// <exception cref="System.ArgumentNullException">DataTable不能为 null！</exception>
        public static bool OutputDataTableToExcel(DataTable dt, string excelFileName, double headRowHeight, double dataRowHeight, ref string errorMessage)
        {
            return OutputDataTableToExcel(dt, excelFileName, string.Empty, headRowHeight, dataRowHeight, null, null, null, ref errorMessage);
        }

        /// <summary>
        /// 将指定DataTable中的所有行的数据,导入到指定的Excel文件中,并可以设置Excel的工作表名称、表头行行高及数据行行高。
        /// </summary>
        /// <param name="dt">要导出数据到Excel文件中的DataTable控件。</param>
        /// <param name="excelFileName">要导出DataTable数据的Excel全文件名。</param>
        /// <param name="sheetName">Excel的工作表名称,若为null或string.Empty,则为默认的sheet1。</param>
        /// <param name="headRowHeight">Excel的表头行行高,若小于等于0,则为默认的18。</param>
        /// <param name="dataRowHeight">Excel的数据行行高,若小于等于0,则为默认的12.75。</param>
        /// <param name="errorMessage">导出成功,返回string.Empty,若导出失败,则返回指定的错误信息。</param>
        /// <returns>导出成功,返回true,否则,返回false。</returns>
        /// <exception cref="System.ArgumentNullException">DataTable不能为 null！</exception>
        public static bool OutputDataTableToExcel(DataTable dt, string excelFileName, string sheetName, double headRowHeight, double dataRowHeight, ref string errorMessage)
        {
            return OutputDataTableToExcel(dt, excelFileName, sheetName, headRowHeight, dataRowHeight, null, null, null, ref errorMessage);
        }

        /// <summary>
        /// 将指定DataTable中的所有行的指定列的数据,导入到指定的Excel文件中。
        /// </summary>
        /// <param name="dt">要导出数据到Excel文件中的DataTable控件。</param>
        /// <param name="excelFileName">要导出DataTable数据的Excel全文件名。</param>
        /// <param name="ignoreDataTableColumnIndexList">忽略的DataTable的从0开始的列索引集合,这将指定DataTable中,哪些列将被忽略,不被导入到Excel中,若为null,则默认导出DataTable的所有列。</param>
        /// <param name="errorMessage">导出成功,返回string.Empty,若导出失败,则返回指定的错误信息。</param>
        /// <returns>导出成功,返回true,否则,返回false。</returns>
        /// <exception cref="System.ArgumentNullException">DataTable不能为 null！</exception>
        public static bool OutputDataTableToExcel(DataTable dt, string excelFileName, IEnumerable<int> ignoreDataTableColumnIndexList, ref string errorMessage)
        {
            return OutputDataTableToExcel(dt, excelFileName, string.Empty, -1, -1, ignoreDataTableColumnIndexList, null, null, ref errorMessage);
        }

        /// <summary>
        /// 将指定DataTable中的所有行的数据,导入到指定的Excel文件中,并可以设置Excel的列名。
        /// </summary>
        /// <param name="dt">要导出数据到Excel文件中的DataTable控件。</param>
        /// <param name="excelFileName">要导出DataTable数据的Excel全文件名。</param>
        /// <param name="columnNameList">导出到Excel中的列名集合,这将指定Excel中的表头行的列名,若为null,则所有列的列名默认为DataTable的列名,若无需设置某列列名,则可在列名集合中,指定对应的列名为string.Empty,则该列的列名默认为DataTable的对应列列名。</param>
        /// <param name="errorMessage">导出成功,返回string.Empty,若导出失败,则返回指定的错误信息。</param>
        /// <returns>导出成功,返回true,否则,返回false。</returns>
        /// <exception cref="System.ArgumentNullException">DataTable不能为 null！</exception>
        public static bool OutputDataTableToExcel(DataTable dt, string excelFileName, IEnumerable<string> columnNameList, ref string errorMessage)
        {
            return OutputDataTableToExcel(dt, excelFileName, string.Empty, -1, -1, null, columnNameList, null, ref errorMessage);
        }

        /// <summary>
        /// 将指定DataTable中的所有行的数据,导入到指定的Excel文件中,并可以设置Excel的列宽。
        /// </summary>
        /// <param name="dt">要导出数据到Excel文件中的DataTable控件。</param>
        /// <param name="excelFileName">要导出DataTable数据的Excel全文件名。</param>
        /// <param name="columnWidthList">导出到Excel中的各列列宽集合,这将指定Excel中的表头行的列宽,若为null,则所有列的列宽默认为20,若无需设置某列列宽,则可在列宽集合中,指定对应的列宽为一个小于等于0的值(如-1),则该列的列宽默认为20。</param>
        /// <param name="errorMessage">导出成功,返回string.Empty,若导出失败,则返回指定的错误信息。</param>
        /// <returns>导出成功,返回true,否则,返回false。</returns>
        /// <exception cref="System.ArgumentNullException">DataTable不能为 null！</exception>
        public static bool OutputDataTableToExcel(DataTable dt, string excelFileName, IEnumerable<double> columnWidthList, ref string errorMessage)
        {
            return OutputDataTableToExcel(dt, excelFileName, string.Empty, -1, -1, null, null, columnWidthList, ref errorMessage);
        }

        /// <summary>
        /// 将指定DataTable中的所有行的指定列的数据,导入到指定的Excel文件中,并可以设置Excel的列名及列宽。
        /// </summary>
        /// <param name="dt">要导出数据到Excel文件中的DataTable控件。</param>
        /// <param name="excelFileName">要导出DataTable数据的Excel全文件名。</param>
        /// <param name="ignoreDataTableColumnIndexList">忽略的DataTable的从0开始的列索引集合,这将指定DataTable中,哪些列将被忽略,不被导入到Excel中,若为null,则默认导出DataTable的所有列。</param>
        /// <param name="columnNameList">导出到Excel中的列名集合,这将指定Excel中的表头行的列名,若为null,则所有列的列名默认为DataTable的列名,若无需设置某列列名,则可在列名集合中,指定对应的列名为string.Empty,则该列的列名默认为DataTable的对应列列名。</param>
        /// <param name="columnWidthList">导出到Excel中的各列列宽集合,这将指定Excel中的表头行的列宽,若为null,则所有列的列宽默认为20,若无需设置某列列宽,则可在列宽集合中,指定对应的列宽为一个小于等于0的值(如-1),则该列的列宽默认为20。</param>
        /// <param name="errorMessage">导出成功,返回string.Empty,若导出失败,则返回指定的错误信息。</param>
        /// <returns>导出成功,返回true,否则,返回false。</returns>
        /// <exception cref="System.ArgumentNullException">DataTable不能为 null！</exception>
        public static bool OutputDataTableToExcel(DataTable dt, string excelFileName, IEnumerable<int> ignoreDataTableColumnIndexList, IEnumerable<string> columnNameList, IEnumerable<double> columnWidthList, ref string errorMessage)
        {
            return OutputDataTableToExcel(dt, excelFileName, string.Empty, -1, -1, ignoreDataTableColumnIndexList, columnNameList, columnWidthList, ref errorMessage);
        }

        /// <summary>
        /// 将指定DataTable中的所有行的指定列的数据,导入到指定的Excel文件中,并可以设置Excel的工作表名称、列名及列宽。
        /// </summary>
        /// <param name="dt">要导出数据到Excel文件中的DataTable控件。</param>
        /// <param name="excelFileName">要导出DataTable数据的Excel全文件名。</param>
        /// <param name="sheetName">Excel的工作表名称,若为null或string.Empty,则为默认的sheet1。</param>
        /// <param name="ignoreDataTableColumnIndexList">忽略的DataTable的从0开始的列索引集合,这将指定DataTable中,哪些列将被忽略,不被导入到Excel中,若为null,则默认导出DataTable的所有列。</param>
        /// <param name="columnNameList">导出到Excel中的列名集合,这将指定Excel中的表头行的列名,若为null,则所有列的列名默认为DataTable的列名,若无需设置某列列名,则可在列名集合中,指定对应的列名为string.Empty,则该列的列名默认为DataTable的对应列列名。</param>
        /// <param name="columnWidthList">导出到Excel中的各列列宽集合,这将指定Excel中的表头行的列宽,若为null,则所有列的列宽默认为20,若无需设置某列列宽,则可在列宽集合中,指定对应的列宽为一个小于等于0的值(如-1),则该列的列宽默认为20。</param>
        /// <param name="errorMessage">导出成功,返回string.Empty,若导出失败,则返回指定的错误信息。</param>
        /// <returns>导出成功,返回true,否则,返回false。</returns>
        /// <exception cref="System.ArgumentNullException">DataTable不能为 null！</exception>
        public static bool OutputDataTableToExcel(DataTable dt, string excelFileName, string sheetName, IEnumerable<int> ignoreDataTableColumnIndexList, IEnumerable<string> columnNameList, IEnumerable<double> columnWidthList, ref string errorMessage)
        {
            return OutputDataTableToExcel(dt, excelFileName, sheetName, -1, -1, ignoreDataTableColumnIndexList, columnNameList, columnWidthList, ref errorMessage);
        }

        /// <summary>
        /// 将指定DataTable中的所有行的指定列的数据,导入到指定的Excel文件中,并可以设置Excel的表头行行高、数据行行高、列名及列宽。
        /// </summary>
        /// <param name="dt">要导出数据到Excel文件中的DataTable控件。</param>
        /// <param name="excelFileName">要导出DataTable数据的Excel全文件名。</param>
        /// <param name="headRowHeight">Excel的表头行行高,若小于等于0,则为默认的18。</param>
        /// <param name="dataRowHeight">Excel的数据行行高,若小于等于0,则为默认的12.75。</param>
        /// <param name="ignoreDataTableColumnIndexList">忽略的DataTable的从0开始的列索引集合,这将指定DataTable中,哪些列将被忽略,不被导入到Excel中,若为null,则默认导出DataTable的所有列。</param>
        /// <param name="columnNameList">导出到Excel中的列名集合,这将指定Excel中的表头行的列名,若为null,则所有列的列名默认为DataTable的列名,若无需设置某列列名,则可在列名集合中,指定对应的列名为string.Empty,则该列的列名默认为DataTable的对应列列名。</param>
        /// <param name="columnWidthList">导出到Excel中的各列列宽集合,这将指定Excel中的表头行的列宽,若为null,则所有列的列宽默认为20,若无需设置某列列宽,则可在列宽集合中,指定对应的列宽为一个小于等于0的值(如-1),则该列的列宽默认为20。</param>
        /// <param name="errorMessage">导出成功,返回string.Empty,若导出失败,则返回指定的错误信息。</param>
        /// <returns>导出成功,返回true,否则,返回false。</returns>
        /// <exception cref="System.ArgumentNullException">DataTable不能为 null！</exception>
        public static bool OutputDataTableToExcel(DataTable dt, string excelFileName, double headRowHeight, double dataRowHeight, IEnumerable<int> ignoreDataTableColumnIndexList, IEnumerable<string> columnNameList, IEnumerable<double> columnWidthList, ref string errorMessage)
        {
            return OutputDataTableToExcel(dt, excelFileName, string.Empty, headRowHeight, dataRowHeight, ignoreDataTableColumnIndexList, columnNameList, columnWidthList, ref errorMessage);
        }

        /// <summary>
        /// 将指定DataTable中的所有行的数据,导入到指定的Excel文件中。
        /// </summary>
        /// <param name="dt">要导出数据到Excel文件中的DataTable控件。</param>
        /// <param name="excelFileName">要导出DataTable数据的Excel全文件名。</param>
        /// <returns>导出成功,返回true,否则,返回false。</returns>
        /// <exception cref="System.ArgumentNullException">DataTable不能为 null！</exception>
        public static bool OutputDataTableToExcel(Dictionary<DataTable, string> dtAndSheetName, string excelFileName, ref string errorMessage)
        {
            return OutputDataTableToExcel(dtAndSheetName, excelFileName, -1, -1, null, null, null, ref errorMessage);
        }

        /// <summary>
        /// 将指定DataTable中的所有行的数据,导入到指定的Excel文件中。
        /// </summary>
        /// <param name="dt">要导出数据到Excel文件中的DataTable控件。</param>
        /// <param name="excelFileName">要导出DataTable数据的Excel全文件名。</param>
        /// <returns>导出成功,返回true,否则,返回false。</returns>
        /// <exception cref="System.ArgumentNullException">DataTable不能为 null！</exception>
        public static bool OutputDataTableToExcel(DataTable dt, string excelFileName)
        {
            string errorMessage = string.Empty;
            return OutputDataTableToExcel(dt, excelFileName, string.Empty, -1, -1, null, null, null, ref errorMessage);
        }

        /// <summary>
        /// 将指定DataTable中的所有行的数据,导入到指定的Excel文件中,并可以设置Excel的工作表名称。
        /// </summary>
        /// <param name="dt">要导出数据到Excel文件中的DataTable控件。</param>
        /// <param name="excelFileName">要导出DataTable数据的Excel全文件名。</param>
        /// <param name="sheetName">Excel的工作表名称,若为null或string.Empty,则为默认的sheet1。</param>
        /// <returns>导出成功,返回true,否则,返回false。</returns>
        /// <exception cref="System.ArgumentNullException">DataTable不能为 null！</exception>
        public static bool OutputDataTableToExcel(DataTable dt, string excelFileName, string sheetName)
        {
            string errorMessage = string.Empty;
            return OutputDataTableToExcel(dt, excelFileName, sheetName, -1, -1, null, null, null, ref errorMessage);
        }

        /// <summary>
        /// 将指定DataTable中的所有行的数据,导入到指定的Excel文件中,并可以设置Excel的表头行行高及数据行行高。
        /// </summary>
        /// <param name="dt">要导出数据到Excel文件中的DataTable控件。</param>
        /// <param name="excelFileName">要导出DataTable数据的Excel全文件名。</param>
        /// <param name="headRowHeight">Excel的表头行行高,若小于等于0,则为默认的18。</param>
        /// <param name="dataRowHeight">Excel的数据行行高,若小于等于0,则为默认的12.75。</param>
        /// <returns>导出成功,返回true,否则,返回false。</returns>
        /// <exception cref="System.ArgumentNullException">DataTable不能为 null！</exception>
        public static bool OutputDataTableToExcel(DataTable dt, string excelFileName, double headRowHeight, double dataRowHeight)
        {
            string errorMessage = string.Empty;
            return OutputDataTableToExcel(dt, excelFileName, string.Empty, headRowHeight, dataRowHeight, null, null, null, ref errorMessage);
        }

        /// <summary>
        /// 将指定DataTable中的所有行的数据,导入到指定的Excel文件中,并可以设置Excel的工作表名称、表头行行高及数据行行高。
        /// </summary>
        /// <param name="dt">要导出数据到Excel文件中的DataTable控件。</param>
        /// <param name="excelFileName">要导出DataTable数据的Excel全文件名。</param>
        /// <param name="sheetName">Excel的工作表名称,若为null或string.Empty,则为默认的sheet1。</param>
        /// <param name="headRowHeight">Excel的表头行行高,若小于等于0,则为默认的18。</param>
        /// <param name="dataRowHeight">Excel的数据行行高,若小于等于0,则为默认的12.75。</param>
        /// <returns>导出成功,返回true,否则,返回false。</returns>
        /// <exception cref="System.ArgumentNullException">DataTable不能为 null！</exception>
        public static bool OutputDataTableToExcel(DataTable dt, string excelFileName, string sheetName, double headRowHeight, double dataRowHeight)
        {
            string errorMessage = string.Empty;
            return OutputDataTableToExcel(dt, excelFileName, sheetName, headRowHeight, dataRowHeight, null, null, null, ref errorMessage);
        }

        /// <summary>
        /// 将指定DataTable中的所有行的指定列的数据,导入到指定的Excel文件中。
        /// </summary>
        /// <param name="dt">要导出数据到Excel文件中的DataTable控件。</param>
        /// <param name="excelFileName">要导出DataTable数据的Excel全文件名。</param>
        /// <param name="ignoreDataTableColumnIndexList">忽略的DataTable的从0开始的列索引集合,这将指定DataTable中,哪些列将被忽略,不被导入到Excel中,若为null,则默认导出DataTable的所有列。</param>
        /// <returns>导出成功,返回true,否则,返回false。</returns>
        /// <exception cref="System.ArgumentNullException">DataTable不能为 null！</exception>
        public static bool OutputDataTableToExcel(DataTable dt, string excelFileName, IEnumerable<int> ignoreDataTableColumnIndexList)
        {
            string errorMessage = string.Empty;
            return OutputDataTableToExcel(dt, excelFileName, string.Empty, -1, -1, ignoreDataTableColumnIndexList, null, null, ref errorMessage);
        }

        /// <summary>
        /// 将指定DataTable中的所有行的数据,导入到指定的Excel文件中,并可以设置Excel的列名。
        /// </summary>
        /// <param name="dt">要导出数据到Excel文件中的DataTable控件。</param>
        /// <param name="excelFileName">要导出DataTable数据的Excel全文件名。</param>
        /// <param name="columnNameList">导出到Excel中的列名集合,这将指定Excel中的表头行的列名,若为null,则所有列的列名默认为DataTable的列名,若无需设置某列列名,则可在列名集合中,指定对应的列名为string.Empty,则该列的列名默认为DataTable的对应列列名。</param>
        /// <returns>导出成功,返回true,否则,返回false。</returns>
        /// <exception cref="System.ArgumentNullException">DataTable不能为 null！</exception>
        public static bool OutputDataTableToExcel(DataTable dt, string excelFileName, IEnumerable<string> columnNameList)
        {
            string errorMessage = string.Empty;
            return OutputDataTableToExcel(dt, excelFileName, string.Empty, -1, -1, null, columnNameList, null, ref errorMessage);
        }

        /// <summary>
        /// 将指定DataTable中的所有行的数据,导入到指定的Excel文件中,并可以设置Excel的列宽。
        /// </summary>
        /// <param name="dt">要导出数据到Excel文件中的DataTable控件。</param>
        /// <param name="excelFileName">要导出DataTable数据的Excel全文件名。</param>
        /// <param name="columnWidthList">导出到Excel中的各列列宽集合,这将指定Excel中的表头行的列宽,若为null,则所有列的列宽默认为20,若无需设置某列列宽,则可在列宽集合中,指定对应的列宽为一个小于等于0的值(如-1),则该列的列宽默认为20。</param>
        /// <returns>导出成功,返回true,否则,返回false。</returns>
        /// <exception cref="System.ArgumentNullException">DataTable不能为 null！</exception>
        public static bool OutputDataTableToExcel(DataTable dt, string excelFileName, IEnumerable<double> columnWidthList)
        {
            string errorMessage = string.Empty;
            return OutputDataTableToExcel(dt, excelFileName, string.Empty, -1, -1, null, null, columnWidthList, ref errorMessage);
        }

        /// <summary>
        /// 将指定DataTable中的所有行的指定列的数据,导入到指定的Excel文件中,并可以设置Excel的列名及列宽。
        /// </summary>
        /// <param name="dt">要导出数据到Excel文件中的DataTable控件。</param>
        /// <param name="excelFileName">要导出DataTable数据的Excel全文件名。</param>
        /// <param name="ignoreDataTableColumnIndexList">忽略的DataTable的从0开始的列索引集合,这将指定DataTable中,哪些列将被忽略,不被导入到Excel中,若为null,则默认导出DataTable的所有列。</param>
        /// <param name="columnNameList">导出到Excel中的列名集合,这将指定Excel中的表头行的列名,若为null,则所有列的列名默认为DataTable的列名,若无需设置某列列名,则可在列名集合中,指定对应的列名为string.Empty,则该列的列名默认为DataTable的对应列列名。</param>
        /// <param name="columnWidthList">导出到Excel中的各列列宽集合,这将指定Excel中的表头行的列宽,若为null,则所有列的列宽默认为20,若无需设置某列列宽,则可在列宽集合中,指定对应的列宽为一个小于等于0的值(如-1),则该列的列宽默认为20。</param>
        /// <returns>导出成功,返回true,否则,返回false。</returns>
        /// <exception cref="System.ArgumentNullException">DataTable不能为 null！</exception>
        public static bool OutputDataTableToExcel(DataTable dt, string excelFileName, IEnumerable<int> ignoreDataTableColumnIndexList, IEnumerable<string> columnNameList, IEnumerable<double> columnWidthList)
        {
            string errorMessage = string.Empty;
            return OutputDataTableToExcel(dt, excelFileName, string.Empty, -1, -1, ignoreDataTableColumnIndexList, columnNameList, columnWidthList, ref errorMessage);
        }

        /// <summary>
        /// 将指定DataTable中的所有行的指定列的数据,导入到指定的Excel文件中,并可以设置Excel的工作表名称、列名及列宽。
        /// </summary>
        /// <param name="dt">要导出数据到Excel文件中的DataTable控件。</param>
        /// <param name="excelFileName">要导出DataTable数据的Excel全文件名。</param>
        /// <param name="sheetName">Excel的工作表名称,若为null或string.Empty,则为默认的sheet1。</param>
        /// <param name="ignoreDataTableColumnIndexList">忽略的DataTable的从0开始的列索引集合,这将指定DataTable中,哪些列将被忽略,不被导入到Excel中,若为null,则默认导出DataTable的所有列。</param>
        /// <param name="columnNameList">导出到Excel中的列名集合,这将指定Excel中的表头行的列名,若为null,则所有列的列名默认为DataTable的列名,若无需设置某列列名,则可在列名集合中,指定对应的列名为string.Empty,则该列的列名默认为DataTable的对应列列名。</param>
        /// <param name="columnWidthList">导出到Excel中的各列列宽集合,这将指定Excel中的表头行的列宽,若为null,则所有列的列宽默认为20,若无需设置某列列宽,则可在列宽集合中,指定对应的列宽为一个小于等于0的值(如-1),则该列的列宽默认为20。</param>
        /// <returns>导出成功,返回true,否则,返回false。</returns>
        /// <exception cref="System.ArgumentNullException">DataTable不能为 null！</exception>
        public static bool OutputDataTableToExcel(DataTable dt, string excelFileName, string sheetName, IEnumerable<int> ignoreDataTableColumnIndexList, IEnumerable<string> columnNameList, IEnumerable<double> columnWidthList)
        {
            string errorMessage = string.Empty;
            return OutputDataTableToExcel(dt, excelFileName, sheetName, -1, -1, ignoreDataTableColumnIndexList, columnNameList, columnWidthList, ref errorMessage);
        }

        /// <summary>
        /// 将指定DataTable中的所有行的指定列的数据,导入到指定的Excel文件中,并可以设置Excel的表头行行高、数据行行高、列名及列宽。
        /// </summary>
        /// <param name="dt">要导出数据到Excel文件中的DataTable控件。</param>
        /// <param name="excelFileName">要导出DataTable数据的Excel全文件名。</param>
        /// <param name="headRowHeight">Excel的表头行行高,若小于等于0,则为默认的18。</param>
        /// <param name="dataRowHeight">Excel的数据行行高,若小于等于0,则为默认的12.75。</param>
        /// <param name="ignoreDataTableColumnIndexList">忽略的DataTable的从0开始的列索引集合,这将指定DataTable中,哪些列将被忽略,不被导入到Excel中,若为null,则默认导出DataTable的所有列。</param>
        /// <param name="columnNameList">导出到Excel中的列名集合,这将指定Excel中的表头行的列名,若为null,则所有列的列名默认为DataTable的列名,若无需设置某列列名,则可在列名集合中,指定对应的列名为string.Empty,则该列的列名默认为DataTable的对应列列名。</param>
        /// <param name="columnWidthList">导出到Excel中的各列列宽集合,这将指定Excel中的表头行的列宽,若为null,则所有列的列宽默认为20,若无需设置某列列宽,则可在列宽集合中,指定对应的列宽为一个小于等于0的值(如-1),则该列的列宽默认为20。</param>
        /// <returns>导出成功,返回true,否则,返回false。</returns>
        /// <exception cref="System.ArgumentNullException">DataTable不能为 null！</exception>
        public static bool OutputDataTableToExcel(DataTable dt, string excelFileName, double headRowHeight, double dataRowHeight, IEnumerable<int> ignoreDataTableColumnIndexList, IEnumerable<string> columnNameList, IEnumerable<double> columnWidthList)
        {
            string errorMessage = string.Empty;
            return OutputDataTableToExcel(dt, excelFileName, string.Empty, headRowHeight, dataRowHeight, ignoreDataTableColumnIndexList, columnNameList, columnWidthList, ref errorMessage);
        }

        /// <summary>
        /// 将指定DataTable中的所有行的指定列的数据,导入到指定的Excel文件中,并可以设置Excel的工作表名称、表头行行高、数据行行高、列名及列宽。
        /// </summary>
        /// <param name="dt">要导出数据到Excel文件中的DataTable控件。</param>
        /// <param name="excelFileName">要导出DataTable数据的Excel全文件名。</param>
        /// <param name="sheetName">Excel的工作表名称,若为null或string.Empty,则为默认的sheet1。</param>
        /// <param name="headRowHeight">Excel的表头行行高,若小于等于0,则为默认的18。</param>
        /// <param name="dataRowHeight">Excel的数据行行高,若小于等于0,则为默认的12.75。</param>
        /// <param name="ignoreDataTableColumnIndexList">忽略的DataTable的从0开始的列索引集合,这将指定DataTable中,哪些列将被忽略,不被导入到Excel中,若为null,则默认导出DataTable的所有列。</param>
        /// <param name="columnNameList">导出到Excel中的列名集合,这将指定Excel中的表头行的列名,若为null,则所有列的列名默认为DataTable的列名,若无需设置某列列名,则可在列名集合中,指定对应的列名为string.Empty,则该列的列名默认为DataTable的对应列列名。</param>
        /// <param name="columnWidthList">导出到Excel中的各列列宽集合,这将指定Excel中的表头行的列宽,若为null,则所有列的列宽默认为20,若无需设置某列列宽,则可在列宽集合中,指定对应的列宽为一个小于等于0的值(如-1),则该列的列宽默认为20。</param>
        /// <returns>导出成功,返回true,否则,返回false。</returns>
        /// <exception cref="System.ArgumentNullException">DataTable不能为 null！</exception>
        public static bool OutputDataTableToExcel(DataTable dt, string excelFileName, string sheetName, double headRowHeight, double dataRowHeight, IEnumerable<int> ignoreDataTableColumnIndexList, IEnumerable<string> columnNameList, IEnumerable<double> columnWidthList)
        {
            string errorMessage = string.Empty;
            return OutputDataTableToExcel(dt, excelFileName, sheetName, headRowHeight, dataRowHeight, ignoreDataTableColumnIndexList, columnNameList, columnWidthList, ref errorMessage);
        }

        #endregion

    }
}
