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
            {(int)ClientSendPackets.RequestFile, UserRequestedFile},
            {(int)ClientSendPackets.UpdatedFile, UserEditedFile},
            {(int)ClientSendPackets.UnsubscribeFromUpdatesForFile, UserUnsubscribedFromFileUpdates}
        };


        #region Main

        /* -PACKET LAYOUT-
         * string File Name (IS FULL LOCAL DIRECTORY, NAME AND EXTENSION)
         */
        public static void UserCreatedNewFile(int SenderClientID, Packet Data)
        {
            CustomConsole.Log("SERVER INSTRUCTION: User created file.");

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
         * string File Name (IS FULL LOCAL DIRECTORY, NAME AND EXTENSION)
         * bool Subscrive To Updates (Whether or not client wants to recieve new version of file when it is updated)
         */
        public static void UserRequestedFile(int SenderClientID, Packet Data)
        {
            CustomConsole.Log("SERVER INSTRUCTION: User requested file.");

            string FileName = Data.ReadString();
            bool SubscribeToUpdates = Data.ReadBool();

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

                    if (SubscribeToUpdates)
                    {
                        if (ProjectInstance.LoadedProject.FileUpdateSubscriptionLookup.ContainsKey(FileName))
                        {
                            ProjectInstance.LoadedProject.FileUpdateSubscriptionLookup[FileName].Add(SenderClientID);
                        }
                        else
                        {
                            ProjectInstance.LoadedProject.FileUpdateSubscriptionLookup.Add(FileName, new HashSet<int>() { SenderClientID });
                        }
                    }

                    ServerSendFunctions.SendFile(SenderClientID, FileName);
                }
                else
                {
                    ServerSendFunctions.SendErrorNotification(SenderClientID, "Failed to retreive file - File doesn't exist.");
                }
            }
            
        }

        /* -PACKET LAYOUT-
         * string FILE STRING (IS FULL LOCAL DIRECTORY, NAME AND EXTENSION)
         * byte[] NEW FILE DATA
         */
        public static void UserEditedFile(int SenderClientID, Packet Data)
        {
            CustomConsole.Log("SERVER INSTRUCTION: User edited file.");

            string FileName = Data.ReadString();

            ProjectInstance.LoadedProject.FileCacheLookup[FileName] = Data.ReadBytes();

            File.WriteAllBytes(ProjectInstance.LoadedProject.BasePath + FileName, ProjectInstance.LoadedProject.FileCacheLookup[FileName]);

            foreach (int Client in ProjectInstance.LoadedProject.FileUpdateSubscriptionLookup[FileName])
            {
                ServerSendFunctions.SendFileUpdate(Client, FileName);
            }
        }

        /* -PACKET LAYOUT-
         * string FILE STRING (IS FULL LOCAL DIRECTORY, NAME AND EXTENSION)
         */
        public static void UserUnsubscribedFromFileUpdates(int SenderClientID, Packet Data)
        {
            CustomConsole.Log("SERVER INSTRUCTION: User no longer viewing file.");

            string FileName = Data.ReadString();

            ProjectInstance.LoadedProject.FileUpdateSubscriptionLookup[FileName].Remove(SenderClientID);
        }

        #endregion

    }

}
