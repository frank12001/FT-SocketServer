WebSocketSharp.Server.cs 

801  
802

   _listener = new TcpListener(port);

   改成

   _listener = new TcpListener(System.Net.IPAddress.IPv6Any, port);
   _listener.Server.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);

935

   
   _listener.Server.SetSocketOption(
     SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true 
   );

   改成 

   _listener.Server.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);  