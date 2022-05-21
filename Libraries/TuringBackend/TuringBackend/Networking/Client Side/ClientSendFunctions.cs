using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TuringBackend.Networking
{
    public static class ClientSendFunctions
    {
        private static void SendTCPData(Packet Data)
        {
            Data.InsertPacketLength();
            Client.TCP.SendDataToServer(Data);
            Data.Dispose();
        }

        public static void RequestFile(string FileName, bool RecieveUpdates)
        {
            Packet Data = new Packet();

            Data.Write((int)ClientSendPackets.RequestFile);
            Data.Write(FileName);
            Data.Write(RecieveUpdates);

            SendTCPData(Data);
        }

        public static void CreateFile(string FileName)
        {
            Packet Data = new Packet();

            Data.Write((int)ClientSendPackets.CreateFile);
            Data.Write(FileName);

            SendTCPData(Data);
        }

        public static void UpdateFile(string FileName, int Version, string NewContents)
        {
            Packet Data = new Packet();

            Data.Write((int)ClientSendPackets.UpdateFile);
            Data.Write(FileName);
            Data.Write(Version);
            Data.Write(Encoding.ASCII.GetBytes(NewContents));

            SendTCPData(Data);
        }

        public static void RenameOrMoveFile(string OldFileName, string NewFileName)
        {
            Packet Data = new Packet();

            Data.Write((int)ClientSendPackets.RenameMoveFile);
            Data.Write(OldFileName);
            Data.Write(NewFileName);

            SendTCPData(Data);
        }

        public static void DeleteFile(string FileName)
        {
            Packet Data = new Packet();

            Data.Write((int)ClientSendPackets.DeleteFile);
            Data.Write(FileName);

            SendTCPData(Data);
        }

        public static void UnsubscribeFromFileUpdates(string FileName)
        {
            Packet Data = new Packet();

            Data.Write((int)ClientSendPackets.UnsubscribeFromUpdatesForFile);
            Data.Write(FileName);

            SendTCPData(Data);
        }
    }
}
