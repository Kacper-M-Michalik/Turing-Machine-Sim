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
        }

        public static void RequestProjectFiles(string Folder)
        {

        }

        public static void CreateFile(string FileString)
        {
            Packet Data = new Packet();

            Data.Write((int)ClientSendPackets.CreateFile);
            Data.Write(FileString);

            SendTCPData(Data);
            Data.Dispose();
        }
    }
}
