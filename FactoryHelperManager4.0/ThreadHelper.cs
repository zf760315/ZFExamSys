using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace FactoryHelperManager
{
    public delegate void VoidFunc();

    public class ThreadHelper
    {
        /// <summary>
        /// 把work放入线程池。错误记录日志并抛出。
        /// </summary>
        /// <param name="work"></param>
        public static void QueueWork(VoidFunc work)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(delegate
            {
                try
                {
                    work();
                }
                catch (Exception ex)
                {
                    LogHelper.WriteMessage(ex.Message);
                    #if DEV
                    throw;
                    #endif
                }
            }));
        }
    }
}
