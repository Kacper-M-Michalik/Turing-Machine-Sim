using System;
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
        }
        #endregion

        #region Main
        public static void SendErrorNotification(int ClientID, string ErrorString)
        {
            Packet Data = new Packet();

            Data.Write((int)ServerSendPackets.ErrorNotification);
            Data.Write(ErrorString);

            SendTCPData(ClientID, Data);
            Data.Dispose();
        }

        /*
        //Temp
        public static void MachineStateChange(int Type)
        {
            Packet Data = new Packet();

            Data.Write((int)ServerSendPackets.MachineStateChange);
            Data.Write(Type);

            SendTCPToAllClients(Data);
            Data.Dispose();
        }
        */
        //redo connect welcome
        //change window/update UI
        //change project settings
        //change diagram
        //change tape status -> step so on
        //download project
        #endregion
    }
}
