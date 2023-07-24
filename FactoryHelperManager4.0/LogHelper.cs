using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FactoryHelperManager
{
    public class LogHelper
    {

        //public static void Error(Type t, Exception ex)
        //{
        //    log4net.ILog log = log4net.LogManager.GetLogger(t);
        //    log.Error("Error", ex);
        //}

        //public static void Error(Type t, string msg)
        //{
        //    log4net.ILog log = log4net.LogManager.GetLogger(t);
        //    log.Error(msg);
        //}

        //public static void Debug(Type t, string msg)
        //{
        //    log4net.ILog log = log4net.LogManager.GetLogger(t);
        //    log.Debug(msg);
        //}

        //public static void Info(Type t, string msg)
        //{
        //    log4net.ILog log = log4net.LogManager.GetLogger(t);
        //    log.Info(msg);
        //}

        //public static void Warn(Type t, string msg)
        //{
        //    log4net.ILog log = log4net.LogManager.GetLogger(t);
        //    log.Warn(msg);
        //}

        public static void WriteMessage(string towrite, bool addTime = true)
        {
            try
            {
                string timeValue = addTime ? "\r\n" + DateTime.Now : "";
                File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.txt"), timeValue + "\r\n" + towrite, Encoding.UTF8);
            }
            catch (Exception ex)
            {
            }
        }

        public static void DeleteLogs()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.txt");
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch { }
        }
    }
}
