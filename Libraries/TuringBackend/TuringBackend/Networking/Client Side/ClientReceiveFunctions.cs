﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TuringBackend.Debugging;

namespace TuringBackend.Networking
{
    static class ClientReceiveFunctions
    {
        public delegate void PacketFunctionPointer(Packet Data);
        public static Dictionary<int, PacketFunctionPointer> PacketToFunction = new Dictionary<int, PacketFunctionPointer>()
        {
            {(int)ServerSendPackets.ErrorNotification, ReceiveErrorNotification}
        };


        public static void ReceiveErrorNotification(Packet Data)
        {
            CustomConsole.Log("CLEINT: ERROR MESSAGE FROM SERVER: " + Data.ReadString());
        }

    }
}
