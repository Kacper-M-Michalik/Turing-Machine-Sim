﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TuringBackend.Networking
{
    static class ServerSendFunctions
    {
        #region Helper Functions
        private static void SendTCPData(int ClientID, Packet Data)
        {
            Data.InsertPacketLength();
            Server.Clients[ClientID].TCP.SendDataToClient(Data);
            Data.Dispose();
        }

        private static void SendTCPToAllClients(Packet Data)
        {
            for (int i = 1; i < Server.MaxClients; i++)
            {
                if (Server.Clients[i].TCP.ConnectionSocket != null)
                {
                    Data.InsertPacketLength();
                    Server.Clients[i].TCP.SendDataToClient(Data);
                }
            }
            Data.Dispose();
        }
        #endregion

        #region Main
        public static void SendErrorNotification(int ClientID, string ErrorString)
        {
            Packet Data = new Packet();

            Data.Write((int)ServerSendPackets.ErrorNotification);
            Data.Write(ErrorString);

            SendTCPData(ClientID, Data);
        }

        public static void SendFile(int ClientID, string FileName)
        {
            Packet Data = new Packet();

            Data.Write((int)ServerSendPackets.SentFile);
            Data.Write(ProjectInstance.LoadedProject.UpdateSubscribersLookup[FileName].VersionNumber);
            Data.Write(ProjectInstance.LoadedProject.FileCacheLookup[FileName].FileData);

            SendTCPData(ClientID, Data);
        }

        //unnecessary?
        public static void SendFileUpdate(int ClientID, string FileName)
        {
            Packet Data = new Packet();

            Data.Write((int)ServerSendPackets.UpdatedFile);

            //


            SendTCPData(ClientID, Data);
        }
        #endregion
    }
}
