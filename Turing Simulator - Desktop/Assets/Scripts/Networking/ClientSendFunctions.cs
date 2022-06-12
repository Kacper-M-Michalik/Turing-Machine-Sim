using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TuringBackend;

namespace TuringFrontend.Networking
{
    public static class ClientSendFunctions
    {
        private static void SendTCPData(Packet Data)
        {
            Data.InsertPacketLength();
            Client.TCP.SendDataToServer(Data);
            Data.Dispose();
        }

        public static void RequestFile(int FileID, bool RecieveUpdates)
        {
            Packet Data = new Packet();

            Data.Write((int)ClientSendPackets.RequestFile);
            Data.Write(FileID);
            Data.Write(RecieveUpdates);

            SendTCPData(Data);
        }

        public static void CreateFile(int Folder, string NewName)
        {
            Packet Data = new Packet();

            Data.Write((int)ClientSendPackets.CreateFile);
            Data.Write(Folder);
            Data.Write(NewName);

            SendTCPData(Data);
        }

        public static void UpdateFile(int FileID, int Version, string NewContents)
        {
            Packet Data = new Packet();

            Data.Write((int)ClientSendPackets.UpdateFile);
            Data.Write(FileID);
            Data.Write(Version);
            Data.Write(Encoding.ASCII.GetBytes(NewContents));

            SendTCPData(Data);
        }

        public static void RenameFile(int FileID, string NewFileName)
        {
            Packet Data = new Packet();

            Data.Write((int)ClientSendPackets.RenameFile);
            Data.Write(FileID);
            Data.Write(NewFileName);

            SendTCPData(Data);
        }

        public static void MoveFile(int FileID, int NewFolderID)
        {
            Packet Data = new Packet();

            Data.Write((int)ClientSendPackets.MoveFile);
            Data.Write(FileID);
            Data.Write(NewFolderID);

            SendTCPData(Data);
        }

        public static void DeleteFile(int FileID)
        {
            Packet Data = new Packet();

            Data.Write((int)ClientSendPackets.DeleteFile);
            Data.Write(FileID);

            SendTCPData(Data);
        }

        public static void UnsubscribeFromFileUpdates(int FileID)
        {
            Packet Data = new Packet();

            Data.Write((int)ClientSendPackets.UnsubscribeFromUpdatesForFile);
            Data.Write(FileID);

            SendTCPData(Data);
        }


        public static void CreateFolder(int BaseFolder, string NewName)
        {
            Packet Data = new Packet();

            Data.Write((int)ClientSendPackets.CreateFolder);
            Data.Write(BaseFolder);
            Data.Write(NewName);

            SendTCPData(Data);
        }

        public static void RenameFolder(int Folder, string NewName)
        {
            Packet Data = new Packet();

            Data.Write((int)ClientSendPackets.RenameFolder);
            Data.Write(Folder);
            Data.Write(NewName);

            SendTCPData(Data);
        }

        public static void MoveFolder(int Folder, int TargetFolder)
        {
            Packet Data = new Packet();

            Data.Write((int)ClientSendPackets.MoveFolder);
            Data.Write(Folder);
            Data.Write(TargetFolder);

            SendTCPData(Data);
        }

        public static void DeleteFolder(int Folder)
        {
            Packet Data = new Packet();

            Data.Write((int)ClientSendPackets.DeleteFolder);
            Data.Write(Folder);

            SendTCPData(Data);
        }
    }
}