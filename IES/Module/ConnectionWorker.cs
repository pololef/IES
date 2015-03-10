using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Connect.IES.Module
{
    /// <summary>
    /// 連線工作模組
    /// <para> 因為TcpListenter 在等待連線進入的時候 會卡住Thread 所以才會另外用Thread將動作包起來 以免影響其他行為</para>
    /// </summary>
    public class ConnectionWorker:IDisposable
    {
        /// <summary>
        /// 注意：此處會直接生成並直接執行一個Thread
        /// </summary>
        public ConnectionWorker(int systemPort)
        {
            //客端連線接收器
            fClientListener = new TcpListener(IPAddress.Any, systemPort);
            fClientQueue = new ConcurrencyStructure<TcpClient>();

            fWorker = new Thread(RcvConnection);
            fWorker.IsBackground = true;
            fWorker.Start();
        }

        #region 解構子

        /// <summary>
        /// 
        /// </summary>
        ~ConnectionWorker()
        {
            Dispose();
        }

        #endregion

        #region 變數

        /// <summary>
        /// 內部Thread 
        /// </summary>
        private Thread fWorker;

        /// <summary>
        /// 監聽socket
        /// </summary>
        private TcpListener fClientListener;

        /// <summary>
        /// 儲存客戶端的佇列 僅為過水用
        /// </summary>
        private ConcurrencyStructure<TcpClient> fClientQueue;

        /// <summary>
        /// 輸出訊息
        /// </summary>
        private Action<string> fOutputMessage;

        #endregion

        #region 私有方法

        /// <summary>
        /// 接收連線的Thread方法
        /// </summary>
        private void RcvConnection()
        {
            //開始接收
            fClientListener.Start();
            while (true)
            {
                try
                {
                    TcpClient clientInfo = fClientListener.AcceptTcpClient();
                    fClientQueue.Enqueue(clientInfo);
                }
                catch (SocketException connectEx)
                {
                    //處理連線錯誤
                }
                catch (Exception ex)
                {
                    //未預期的錯誤
                }
            }
        }
        
        #endregion

        #region 公開方法

        /// <summary>
        /// 此處需實作停止Thread的方法
        /// </summary>
        public void Dispose()
        {
            // 停止listener
            fClientListener.Stop();

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 取出所有連上的Client
        /// </summary>
        /// <returns></returns>
        public List<TcpClient> TakeOutClient()
        {
            return fClientQueue.DequeueAll();
        }

        #endregion
    }
}
