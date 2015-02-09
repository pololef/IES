using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Connect.IES.Module
{
    /// <summary>
    /// 平行佇列
    /// </summary>
    public class ConcurrencyStructure<T> : IDisposable where T : class
    {
        /// <summary>
        /// 注意：此處的結構為thread safe的佇列
        /// </summary>
        public ConcurrencyStructure()
        {
            fIsDisposed = false;

            fQueue = new ConcurrentQueue<T>();
        }

        ~ConcurrencyStructure()
        {
            Dispose();
        }

        /// <summary>
        /// 釋放資源
        /// </summary>
        public void Dispose()
        {
            fIsDisposed = true;


            GC.SuppressFinalize(this);
        }

        #region 變數

        /// <summary>
        /// 是否dispose了
        /// </summary>
        private bool fIsDisposed;

        /// <summary>
        /// 過水結構
        /// </summary>
        private ConcurrentQueue<T> fQueue;

        #endregion

        #region 公開方法

        /// <summary>
        /// 丟入佇列
        /// </summary>
        /// <param name="data"></param>
        public void Enqueue(T data)
        {
            if (!fIsDisposed && data != null)
            {
                fQueue.Enqueue(data);
            }
        }

        /// <summary>
        /// 拿出全部
        /// </summary>
        /// <returns></returns>
        public List<T> DequeueAll()
        {
            List<T> result = new List<T>();
            T data;
            while (fQueue.TryDequeue(out data))
            {
                result.Add(data);
            }

            return result;
        }

        #endregion
    }
}