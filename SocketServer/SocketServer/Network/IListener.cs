using System.Net;
using System.Threading.Tasks;

namespace FTServer.Network
{
    public interface IListener
    {
        Task<ReceiveResult> ReceiveAsync();
    }

    public struct ReceiveResult
    {
        public bool isOk;
        public IPEndPoint RemoteEndPoint;
        public ReceiveResult(IPEndPoint iPEndPoint)
        {
            isOk = true;
            this.RemoteEndPoint = iPEndPoint;
        }
    }
}
