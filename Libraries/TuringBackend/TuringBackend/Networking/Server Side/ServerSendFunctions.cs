using System;
using System.Collections.Generic;

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

        //Unnecessary?
        public static void SendFileUpdate(int ClientID, string FileName)
        {
            Packet Data = new Packet();

            Data.Write((int)ServerSendPackets.UpdatedFile);
            Data.Write(ProjectInstance.LoadedProject.UpdateSubscribersLookup[FileName].VersionNumber);
            Data.Write(ProjectInstance.LoadedProject.FileCacheLookup[FileName].FileData);

            SendTCPData(ClientID, Data);
        }

        public static void SendFileRenamed(string OldFileName, string NewFileName)
        {
            Packet Data = new Packet();

            Data.Write((int)ServerSendPackets.RenamedFile);
            Data.Write(OldFileName);
            Data.Write(NewFileName);

            SendTCPToAllClients(Data);
        }

        public static void SendFileDeleted(string FileName)
        {
            Packet Data = new Packet();

            Data.Write((int)ServerSendPackets.DeletedFile);
            Data.Write(FileName);

            SendTCPToAllClients(Data);
        }
        #endregion
    }
}
