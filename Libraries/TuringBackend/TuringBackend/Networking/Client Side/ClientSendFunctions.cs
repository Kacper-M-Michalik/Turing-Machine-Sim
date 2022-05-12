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

        public static void RequestProjectFiles(string FileName, bool RecieveUpdates)
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
    }
}
