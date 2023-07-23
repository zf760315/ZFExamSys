using ZFExamSys.Views;
using FactoryHelperManager;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ZFExamSys.ViewModels
{
    public class MainViewModel:BindableBase
    {
        private string _filePath = string.Empty;

        /// <summary>
        /// 文件路径
        /// </summary>
        public string FilePath
        {
            get { return _filePath; }
            set { SetProperty(ref _filePath, value); }
        }

        private DelegateCommand _countCommand;

        public DelegateCommand CountCommand =>
            _countCommand ?? (_countCommand = new DelegateCommand(ExecuteCountData));

        private DelegateCommand _openPicCommand;

        public DelegateCommand OpenPicCommand =>
            _openPicCommand ?? (_openPicCommand = new DelegateCommand(OpenPicWindow));

        /// <summary>
        /// 统计数据，并将数据写入excel文件中
        /// </summary>
        public void ExecuteCountData()
        {
            if (!string.IsNullOrEmpty(FilePath))
            {
                if (File.Exists(FilePath))
                {
                    DataTable dt = ExcelDataHelper.ReadExcel(FilePath, "场次汇总表", 1, 0, 1);

                    var query1 = from t in dt.AsEnumerable()
                                 group t by new
                                 {
                                     t1 = t.Field<string>("column2"),
                                     t2 = t.Field<string>("column4").Substring(5, 5),
                                     t3 = int.Parse(t.Field<string>("column4").Substring(11, 2)) <= 12 ? "上午" : "下午",
                                 } into m
                                 select new
                                 {
                                     column2 = m.Key.t1,
                                     ks = from t in dt.AsEnumerable()
                                          where t["column2"].ToString() == m.Key.t1
                                          group t by new
                                          {
                                              t1 = t.Field<string>("column2"),
                                          } into n
                                          select new
                                          {
                                              count = n.ToList().GroupBy(dr => dr["column3"]).Count()
                                          },
                                     temp = string.Format("{0}月{1}日 {2} {3}~{4} {5}场",
                                     m.Key.t2.Substring(0, 2),
                                     m.Key.t2.Substring(3, 2),
                                     m.Key.t3,
                                     m.Min(n => DateTime.Parse(n.Field<string>("column4").Substring(11, 5))).ToString().Substring(11, 5),
                                     m.Max(n => DateTime.Parse(n.Field<string>("column4").Substring(11, 5))).ToString().Substring(11, 5),
                                     m.Count(n => n.Field<string>("column4") != "")),
                                     kdrs = from t in dt.AsEnumerable()
                                            where t["column2"].ToString() == m.Key.t1
                                            group t by new
                                            {
                                                t1 = t.Field<string>("column2"),
                                            } into n
                                            select new
                                            {
                                                count = n.Sum(s => s.Field<double>("column5"))
                                            }
                                 };

                    DataTable dtExcel = new DataTable();
                    dtExcel.Columns.Add(new DataColumn("考点名称"));
                    dtExcel.Columns.Add(new DataColumn("考场数"));
                    dtExcel.Columns.Add(new DataColumn("时间安排"));
                    dtExcel.Columns.Add(new DataColumn("考点人数"));

                    foreach (var de in query1)
                    {
                        DataRow dr = dtExcel.NewRow();
                        dr["考点名称"] = de.column2;
                        dr["考场数"] = de.ks.ToList()[0].count;
                        dr["时间安排"] = de.temp;
                        dr["考点人数"] = de.kdrs.ToList()[0].count;
                        dtExcel.Rows.Add(dr);
                    }

                    string createDirectoryPath = Path.Combine(Path.GetDirectoryName(FilePath), string.Format("淮南考点统计表{0}-{1}-{2} {3}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Ticks));
                    DirectoryHelper.CreateDirecory(createDirectoryPath);
                    string createFilePath = Path.Combine(createDirectoryPath, "淮南考点.xlsx");

                    string errorMessage = string.Empty;
                    ExcelDataHelper.OutputDataTableToExcel(
                        dtExcel,
                        createFilePath,
                        string.Empty,
                        -1,
                        -1,
                        null,
                        null,
                        null,
                        ref errorMessage,
                        "考点名称"
                        );
                }
                else
                {
                    MessageBoxX.Info("该文件不存在，请重新选择！");
                }
            }
            else
            {
                MessageBoxX.Info("请选择淮南各考点考试时间表！");
            }
        }

        /// <summary>
        /// 打开模板样片
        /// </summary>
        public void OpenPicWindow()
        {
            ImageView imageView = new ImageView();
            imageView.ShowDialog();
        }
    }
}
