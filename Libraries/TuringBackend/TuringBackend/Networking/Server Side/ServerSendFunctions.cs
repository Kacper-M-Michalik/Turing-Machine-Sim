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

        public static void SendProjectFiles()
        {
            //send rules
            //send directory
        }

        public static void SendFile(int ClientID, int FileID)
        {
            Packet Data = new Packet();

            Data.Write((int)ServerSendPackets.SentFile);
            Data.Write(ProjectInstance.LoadedProject.FileDataLookup[FileID].Version);
            Data.Write(ProjectInstance.LoadedProject.CacheDataLookup[FileID].FileData);

            SendTCPData(ClientID, Data);
        }

        public static void SendFileRenamed(int FileID)
        {
            Packet Data = new Packet();

            Data.Write((int)ServerSendPackets.RenamedFile);
            ///

            SendTCPToAllClients(Data);
        }

        public static void SendFileMoved(int FileID)
        {
            Packet Data = new Packet();

            Data.Write((int)ServerSendPackets.MovedFile);
            ///

            SendTCPToAllClients(Data);
        }

        public static void SendFileDeleted(int FileID)
        {
            Packet Data = new Packet();

            Data.Write((int)ServerSendPackets.DeletedFile);
            Data.Write(FileID);

            SendTCPToAllClients(Data);
        }

        public static void SendFileUnsubscribed(int FileID)
        {
            Packet Data = new Packet();

            Data.Write((int)ServerSendPackets.DeletedFile);
            Data.Write(FileID);

            SendTCPToAllClients(Data);
        }

        public static void SendFolderCreated(int FolderID)
        {
            Packet Data = new Packet();

            Data.Write((int)ServerSendPackets.CreatedFolder);
            Data.Write(FolderID);

            SendTCPToAllClients(Data);
        }

        public static void SendFolderRenamed(int FolderID)
        {
            Packet Data = new Packet();

            Data.Write((int)ServerSendPackets.RenamedFolder);
            Data.Write(FolderID);

            SendTCPToAllClients(Data);
        }

        public static void SendFolderMoved(int FolderID)
        {
            Packet Data = new Packet();

            Data.Write((int)ServerSendPackets.MovedFolder);
            Data.Write(FolderID);

            SendTCPToAllClients(Data);
        }

        public static void SendFolderDeleted(int FolderID)
        {
            Packet Data = new Packet();

            Data.Write((int)ServerSendPackets.DeletedFolder);
            Data.Write(FolderID);

            SendTCPToAllClients(Data);
        }
        #endregion
    }
}
