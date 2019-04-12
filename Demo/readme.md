the rudp is fork from https://github.com/RevenantX/LiteNetLib
and change following code to fix the Burn-In problem on ios or android device

NetSocket.cs
public bool Bind(IPAddress addressIPv4, IPAddress addressIPv6, int port, bool reuseAddress)

use BeginReceiveFrom replace two threads waiting for network income.
