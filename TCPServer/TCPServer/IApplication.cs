using System.Collections.Generic;
using TCPServer.ClientInstance;

namespace TCPServer
{
    public interface IApplication
    {
        /// <summary>
        /// ClientNode 緩存區的容量
        /// </summary>
        uint InputBufferSize { get; }

        /// <summary>
        /// 存放連線進來的 TcpClient ，生成的 ClientNode
        /// </summary>
        List<ClientNode> MlClientSockets { get; }

        /// <summary>
        /// 介面 ListBox Client 
        /// </summary>
        System.Windows.Forms.ListBox LbClients { get; }
        /// <summary>
        /// 將文字在畫面上顯示
        /// </summary>
        /// <param name="_strPrint"></param>
        void PrintLine(string _strPrint);


    }
}
