using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.NetworkInformation;

namespace TuringBackend.Networking
{
    public static class NetworkingUtils
    {
        public static bool PortInUse(int Port)
        {
            bool InUse = false;

            IPGlobalProperties IpProperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] IpEndPoints = IpProperties.GetActiveTcpListeners();

            foreach (IPEndPoint EndPoint in IpEndPoints)
            {
                if (EndPoint.Port == Port)
                {
                    InUse = true;
                    break;
                }
            }

            return InUse;
        }
    }
}
