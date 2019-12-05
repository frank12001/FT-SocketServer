using FTServer.Network;
using FTServer.ClientInstance;

namespace FTServer.ExtensionMethod
{
    public static class RudpSend
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientnode"></param>
        /// <param name="data"></param>
        /// <param name="type">(Unreliable,ReliableUnordered,Sequenced,ReliableOrdered,ReliableSequenced),(0,1,2,3,4,5)</param>
        public static async void RudpSendAsync(this ClientNode clientnode,byte[] data,int type)
        {
            if (clientnode.Sender is RUdp rUdp)
            {
                await rUdp.RudpSendAsync(data, clientnode.IpEndPoint, type);
            }
        }
    }
}
