﻿using System.Net;
using System.Threading.Tasks;

namespace FTServer.Network
{
    public interface ISender
    {
        Task SendAsync(byte[] datagram, IPEndPoint endPoint);
    }
}