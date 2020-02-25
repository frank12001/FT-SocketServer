using System;
using System.Threading;
using FTServer.Network;

namespace GumihoDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            var myServer = new MyServer();
            myServer.Start(5000, Protocol.TCP);

            while(true)
                Thread.Sleep(1000);
        }
    }
}