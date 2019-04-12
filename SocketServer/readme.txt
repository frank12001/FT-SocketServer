the csproject websocketsharp is fork from https://github.com/sta/websocket-sharp
and change following code to support ipv6 connection


WebSocketSharp.Server.cs 

801  
802

   _listener = new TcpListener(port);

   §ï¦¨

   _listener = new TcpListener(System.Net.IPAddress.IPv6Any, port);
   _listener.Server.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);

935

   
   _listener.Server.SetSocketOption(
     SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true 
   );

   §ï¦¨ 

   _listener.Server.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);  
