using System;
using System.Net;
using System.Timers;
using FTServer.Log;
using FTServer.Network;
using FTServer.ClientInstance;

namespace FTServer
{
    public abstract class SocketServer
    {
        private const float TickConsoleClear = 60 * 1000;//5 * 60 * 1000; // 30 minute

        protected Core NetworkCore;
        private int _port;
        private Timer _consoleClear;

        protected void StartListen(int port, Protocol protocol)
        {
            // CompositeResolver is singleton helper for use custom resolver.
            // Of course you can also make custom resolver.
            MessagePack.Resolvers.CompositeResolver.RegisterAndSetAsDefault(
                // use generated resolver first, and combine many other generated/custom resolvers
                MessagePack.Resolvers.GeneratedResolver.Instance,

                // finally, use builtin/primitive resolver(don't use StandardResolver, it includes dynamic generation)
                MessagePack.Resolvers.BuiltinResolver.Instance,
                MessagePack.Resolvers.AttributeFormatterResolver.Instance,
                MessagePack.Resolvers.PrimitiveObjectResolver.Instance
            );

            _port = port;
            Listen(protocol);

            string serverInfo =
                $"Socket Server Start. Base Info => \n     Listen Port :  {port}\n     Network Protocol : {protocol.ToString()}";
            Printer.WriteLine(serverInfo);

            //定時把 console clear 掉，嘗試解決GamingServer 過一段時間後會死在 Printer.WriteLine 的問題
            //BeginConsoleClearAsync();
        }

        /// <summary>
        /// 開始等待封包傳入
        /// </summary>
        private void BeginConsoleClearAsync()
        {
            _consoleClear = new Timer(TickConsoleClear);
            _consoleClear.Elapsed += (sender, eventArg) =>
            {
                Console.Clear();
            };
            _consoleClear.Start();
        }

        private async void Listen(Protocol protocol)
        {
            switch (protocol)
            {
                case Protocol.TCP:
                    NetworkCore = new Tcp(this, new IPEndPoint(IPAddress.Any, _port));
                    break;
                case Protocol.UDP:
                    NetworkCore = new Udp(this, new IPEndPoint(IPAddress.Any, _port));
                    break;
                case Protocol.WebSocket:
                    NetworkCore = new WebSocket(this, _port);
                    break;
                case Protocol.RUdp:
                    NetworkCore = new RUdp(this, new IPEndPoint(IPAddress.Any, _port));
                    break;
                default:
                    Printer.WriteLine("Not Support this Protocol.");
                    break;
            }

            await NetworkCore.StartListen();
        }

        public void CloseClient(IPEndPoint iPEndPoint)
        {
            NetworkCore.DisConnect(iPEndPoint);
        }

        public abstract ClientNode GetPeer(Core core, IPEndPoint iPEndPoint, SocketServer application);
    }
}
