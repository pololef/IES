using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Connect.IES.Module;

namespace Connect.IES
{
    /// <summary>
    /// 資料交換系統(Information Exchanged Server)
    /// </summary>
    public class IES : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="systemHost"></param>
        /// <param name="systemPort"></param>
        /// <param name="systemCheckSum"></param>
        public IES(int systemPort)
        {
            //客端連線接收器
            fClientListener = new TcpListener(IPAddress.Any, systemPort);
            fToken = new CancellationTokenSource();
            fClientQueue = new ConcurrencyStructure<TcpClient>();
            fConnectionWorker = Task.Factory.StartNew(RcvConnection, fToken.Token);
        }

        ~IES()
        {
            Dispose();
        }

        /// <summary>
        /// 釋放資源，放開執行緒
        /// </summary>
        public void Dispose()
        {
            // 停止listener
            fClientListener.Stop();

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 監聽socket
        /// </summary>
        private TcpListener fClientListener;

        /// <summary>
        /// 取消Thread運作的物件
        /// </summary>
        private CancellationTokenSource fToken;

        /// <summary>
        /// 儲存客戶端的佇列 僅為過水用
        /// </summary>
        private ConcurrencyStructure<TcpClient> fClientQueue;

        /// <summary>
        /// 處理連線資訊的Thread
        /// </summary>
        private Task fConnectionWorker;

        /// <summary>
        /// 接收連線的Thread方法
        /// </summary>
        private void RcvConnection()
        {
            //開始接收
            fClientListener.Start();
            while (!fToken.IsCancellationRequested)
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

            //如果走到這步 代表此thread是我們手動關閉的 或者是出現不知名的原因 導致他跳出迴圈
            //手動停止就算了 如果是不知名的原因 則 其餘的工作也該要停下來
            //所以用這個方法去停止所有正在運行的 Thread
            fToken.Token.ThrowIfCancellationRequested();
        }
    }
}
