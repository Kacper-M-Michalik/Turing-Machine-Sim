using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TuringBackend.Debugging;
using System.IO;

namespace TuringBackend.Networking
{
    static class ServerReceiveFunctions
    {
        public delegate void PacketFunctionPointer(int SenderClientID, Packet Data);
        public static Dictionary<int, PacketFunctionPointer> PacketToFunction = new Dictionary<int, PacketFunctionPointer>()
        {
            {(int)ClientSendPackets.CreateFile, UserCreatedNewFile}
        };

        #region Main

        /* -PACKET LAYOUT-
         * FILE STRING (CONTAINS NAME+FILE EXTENSION) (STRING)
         */
        public static void UserCreatedNewFile(int SenderClientID, Packet Data)
        {
            CustomConsole.Log("SERVER INSTRUCTION: USER CREATED NEW FILE");

            string FileString = ProjectInstance.LoadedProject.BasePath+"\\"+Data.ReadString();

            if (File.Exists(FileString))
            {
                ServerSendFunctions.SendErrorNotification(SenderClientID, "File already exists");
                return;
            }

            File.Create(FileString);
        }

        #endregion

    }

}
