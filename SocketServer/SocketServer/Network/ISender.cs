using System.Net;
using System.Threading.Tasks;

namespace FTServer.Network
{
    public interface ISender
    {
        Task SendAsync(byte[] data, IPEndPoint endPoint);
        Task RudpSendAsync(byte[] data, IPEndPoint endPoint, int type);
    }
}
