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
            {(int)ClientSendPackets.CreateFile, UserCreatedNewFile},
            {(int)ClientSendPackets.RequestFile, UserRequestedFile}
        };

        #region Main

        /* -PACKET LAYOUT-
         * FILE STRING (IS FULL LOCAL DIRECTORY, NAME AND EXTENSION) (STRING)
         */
        public static void UserCreatedNewFile(int SenderClientID, Packet Data)
        {
            CustomConsole.Log("SERVER INSTRUCTION: USER CREATED NEW FILE");

            string FileName = Data.ReadString();

            //Maybe rewrite using regex?
            //https://docs.microsoft.com/en-gb/windows/win32/fileio/naming-a-file?redirectedfrom=MSDN for seeing banned characters
            if (FileName.Contains('<') || FileName.Contains('>') || FileName.Contains(':') || FileName.Contains('"') || FileName.Contains('\\') || FileName.Contains('/') || FileName.Contains('|') || FileName.Contains('?') || FileName.Contains('*'))
            {
                ServerSendFunctions.SendErrorNotification(SenderClientID, "Failed to create file - File name uses invalid characters.");
                return;
            }

            string FileDirectoryString = ProjectInstance.LoadedProject.BasePath+FileName;

            if (File.Exists(FileDirectoryString))
            {
                ServerSendFunctions.SendErrorNotification(SenderClientID, "Failed to create file - File already exists.");
                return;
            }

            try
            {
                File.Create(FileDirectoryString);
            }
            catch (Exception E)
            {
                CustomConsole.Log("ServerRecieve Error: UserCreatedNewFile - " + E.ToString());
                ServerSendFunctions.SendErrorNotification(SenderClientID, "Failed to create file - Server failed to create it.");
            }
        }

        /* -PACKET LAYOUT-
         * FILE STRING (IS FULL LOCAL DIRECTORY, NAME AND EXTENSION) (STRING)
         */
        public static void UserRequestedFile(int SenderClientID, Packet Data)
        {
            CustomConsole.Log("SERVER INSTRUCTION: USER REQUESTED FILE");

            string FileName = Data.ReadString();

            if (ProjectInstance.LoadedProject.FileCacheLookup.ContainsKey(FileName))
            {
                ServerSendFunctions.SendFile(SenderClientID, FileName);
            }
            else
            {
                string FullFileDirectory = ProjectInstance.LoadedProject.BasePath + FileName;
                if (File.Exists(FullFileDirectory))
                {
                    try
                    {
                        ProjectInstance.LoadedProject.FileCacheLookup.Add(FileName, File.ReadAllBytes(FullFileDirectory));
                    }
                    catch (Exception E)
                    {
                        CustomConsole.Log("ServerRecieve Error: UserRequestedFile - " + E.ToString());
                        ServerSendFunctions.SendErrorNotification(SenderClientID, "Failed to retreive file - Server failed to load it.");
                    }

                    ServerSendFunctions.SendFile(SenderClientID, FileName);
                }
                else
                {
                    ServerSendFunctions.SendErrorNotification(SenderClientID, "Failed to retreive file - File doesn't exist.");
                }
            }
            
        }

        #endregion

    }

}
