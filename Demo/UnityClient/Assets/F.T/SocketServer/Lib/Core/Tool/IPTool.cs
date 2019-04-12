using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;

namespace FTServer
{
    public static class IPTool
    {
        //修正取 ip 方式，先查看 ip 中有無 "2001:2:0:aab1" mac 模擬特定 ip
        //有的話使用 ip 重組，梅的話使用 Dual Mode


        public static bool IOSCheck(IPAddress targetServerIP, out IPAddress address)
        {
            bool result = false;
            if (targetServerIP.AddressFamily != AddressFamily.InterNetwork)
                throw new Exception("targetServerIP need IPV4 Address");
            address = targetServerIP;
            result = isIosSim();
            if (result)
            {
                address = ChangeToIOSV6Address(targetServerIP);
            }
            return result;
        }

        private static bool isIosSim()
        {
            List<IPAddress> iPs = GetLocalAddressList();
            //ios sim environment
            string iosSimIP = "2001:2:0:aab1"; //"2001:2:0:1baa"
            bool result = false;
            foreach (IPAddress ip in iPs)
            {
                string ipString = ip.ToString();
                result = ipString.Contains(iosSimIP);
                if (result)
                    break;
            }
            return result;
        }

        private static List<IPAddress> GetLocalAddressList()
        {
            var localIps = new List<IPAddress>(); 
            foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                var ips = new List<IPAddress>();
                foreach (var uni in nic.GetIPProperties().UnicastAddresses)
                {
                    ips.Add(uni.Address);
                }                                   
                localIps.AddRange(ips);
            }
            return localIps;
        }

        private static IPAddress ChangeToIOSV6Address(IPAddress address)
        {
            address = address.MapToIPv6();
            string fullIpv6 = RevertIpv6(address);
            string[] ips = fullIpv6.ToString().Split(':');
            ips[0] = "2001";
            ips[1] = "2";//"2";
            ips[2] = "0";//"0";
            ips[3] = "1baa";//"1baa";
            string r = string.Format("{0}:{1}:{2}:{3}::{7}", ips[0], ips[1], ips[2], ips[3],
                ips[4], ips[5], ips[6], ips[7]);
            return IPAddress.Parse(r);
        }


        private static string RevertIpv6(IPAddress ipa)
        {
            string[] ips = ipa.ToString().Split(':');
            if (ips[0] == string.Empty || ips[ips.Length - 1] == string.Empty)
            {
                List<string> temp = new List<string>(ips);
                int removeIndex = 0;
                if (temp[0] == string.Empty)
                    removeIndex = 0;
                if (temp[temp.Count - 1] == string.Empty)
                    removeIndex = temp.Count - 1;
                temp.RemoveAt(removeIndex);
                ips = temp.ToArray();
            }
            int count = (8 - ips.Length) + 1;
            List<string> result = new List<string>();
            if (count != 0)
            {
                for (int i = 0; i < ips.Length; i++)
                {
                    if (ips[i] == string.Empty)
                    {
                        for (int j = 0; j < count; j++)
                        {
                            result.Add("0");
                        }
                    }
                    else
                        result.Add(ips[i]);
                }
            }
            else
                result = new List<string>(ips);

            string r = string.Format("{0}:{1}:{2}:{3}:{4}:{5}:{6}:{7}", result[0], result[1], result[2], result[3],
                result[4], result[5], result[6], result[7]);

            return r;
        }
    }
}