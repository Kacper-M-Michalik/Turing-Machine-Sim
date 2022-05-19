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
            {(int)ClientSendPackets.UpdateFile, UserEditedFile},
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

            if (!ProjectInstance.LoadedProject.FileCacheLookup.ContainsKey(FileName))
            {
                if (!Loader.LoadFileIntoCache(FileName))
                {
                    ServerSendFunctions.SendErrorNotification(SenderClientID, "Failed to retreive file - Server failed to load it.");
                    return;
                }
            }    
            
            if (SubscribeToUpdates) ProjectInstance.LoadedProject.UpdateSubscribersLookup[FileName].SubscriberIDs.Add(SenderClientID);

            ProjectInstance.LoadedProject.FileCacheLookup[FileName].ResetExpiryTimer();
            ServerSendFunctions.SendFile(SenderClientID, FileName);
        }

        /* -PACKET LAYOUT-
         * string FILE STRING (IS FULL LOCAL DIRECTORY, NAME AND EXTENSION)
         * int FILE VERSION
         * byte[] NEW FILE DATA
         */
        public static void UserEditedFile(int SenderClientID, Packet Data)
        {
            CustomConsole.Log("SERVER INSTRUCTION: User edited file.");

            string FileName = Data.ReadString();
            int FileVersion = Data.ReadInt();

            if (!ProjectInstance.LoadedProject.FileCacheLookup.ContainsKey(FileName))
            {
                if (!Loader.LoadFileIntoCache(FileName))
                {
                    ServerSendFunctions.SendErrorNotification(SenderClientID, "Failed to update file - Server failed to load it.");
                    return;
                }
            }

            UpdateFileData UpdateFile = ProjectInstance.LoadedProject.UpdateSubscribersLookup[FileName];
            if (UpdateFile.VersionNumber == FileVersion)
            {
                CacheFileData CacheFile = ProjectInstance.LoadedProject.FileCacheLookup[FileName];
                CacheFile.FileData = Data.ReadBytes();
                CacheFile.ResetExpiryTimer();

                //Replace with async here later
                File.WriteAllBytes(ProjectInstance.LoadedProject.BasePath + FileName, CacheFile.FileData);

                foreach (int Client in UpdateFile.SubscriberIDs)
                {
                    ServerSendFunctions.SendFileUpdate(Client, FileName);
                }
            }
            else
            {
                ServerSendFunctions.SendErrorNotification(SenderClientID, "Failed to update file - You updated an older version of the file.");
                return;
            }
        }

        /* -PACKET LAYOUT-
         * string FILE STRING (IS FULL LOCAL DIRECTORY, NAME AND EXTENSION)
         */
        public static void UserUnsubscribedFromFileUpdates(int SenderClientID, Packet Data)
        {
            CustomConsole.Log("SERVER INSTRUCTION: User no longer viewing file.");

            string FileName = Data.ReadString();

            ProjectInstance.LoadedProject.UpdateSubscribersLookup[FileName].SubscriberIDs.Remove(SenderClientID);
        }

        #endregion

    }

}
