using System;
using System.Collections.Generic;
using System.IO;
using TuringBackend.Debugging;

namespace TuringBackend.Networking
{
    static class ServerReceiveFunctions
    {
        public delegate void PacketFunctionPointer(int SenderClientID, Packet Data);
        public static Dictionary<int, PacketFunctionPointer> PacketToFunction = new Dictionary<int, PacketFunctionPointer>()
        {
            {(int)ClientSendPackets.CreateFile, UserCreatedNewFile},
            {(int)ClientSendPackets.RequestFile, UserRequestedFile},
            {(int)ClientSendPackets.UpdateFile, UserUpdatedFile},
            {(int)ClientSendPackets.RenameMoveFile, UserRenamedFile},
            {(int)ClientSendPackets.DeleteFile, UserDeletedFile},
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

            if (!FileManager.IsValidFileName(FileName))
            {
                ServerSendFunctions.SendErrorNotification(SenderClientID, "Failed to create file - File name uses invalid characters.");
                return;
            }

            string FileDirectoryString = ProjectInstance.LoadedProject.BasePath + FileName;

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
                if (!FileManager.LoadFileIntoCache(FileName))
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
        public static void UserUpdatedFile(int SenderClientID, Packet Data)
        {
            CustomConsole.Log("SERVER INSTRUCTION: User edited file.");

            string FileName = Data.ReadString();
            int FileVersion = Data.ReadInt();

            if (!ProjectInstance.LoadedProject.FileCacheLookup.ContainsKey(FileName))
            {
                if (!FileManager.LoadFileIntoCache(FileName))
                {
                    ServerSendFunctions.SendErrorNotification(SenderClientID, "Failed to update file - Server failed to load it.");
                    return;
                }
            }

            //Possibly implement parsing bytes into actual object to see if it succeeds -> prevent users from sending corrupt files

            UpdateFileData UpdateFile = ProjectInstance.LoadedProject.UpdateSubscribersLookup[FileName];
            if (UpdateFile.VersionNumber != FileVersion)
            {
                ServerSendFunctions.SendErrorNotification(SenderClientID, "Failed to update file - You updated an older version of the file.");
                return;
            }

            try
            {
                
                //Replace with async here later?
                File.WriteAllBytes(ProjectInstance.LoadedProject.BasePath + FileName, Data.ReadByteArray(false));
                byte[] Debug = Data.ReadByteArray(false);
            }
            catch (Exception E)
            {
                CustomConsole.Log("ServerRecieve Error: UserEditedFile - " + E.ToString());
                ServerSendFunctions.SendErrorNotification(SenderClientID, "Failed to update file - Server failed to write to file.");
                return;
            }

            UpdateFile.VersionNumber++;

            CacheFileData CacheFile = ProjectInstance.LoadedProject.FileCacheLookup[FileName];
            CacheFile.FileData = Data.ReadByteArray();
            CacheFile.ResetExpiryTimer();

            foreach (int Client in UpdateFile.SubscriberIDs)
            {
                ServerSendFunctions.SendFileUpdate(Client, FileName);
            }
            
        }

        /* -PACKET LAYOUT-
         * string OLD FILE STRING (IS FULL LOCAL DIRECTORY, NAME AND EXTENSION)
         * string NEW FILE STRING
         */
        public static void UserRenamedFile(int SenderClientID, Packet Data)
        {
            string OldFileName = Data.ReadString();
            string NewFileName = Data.ReadString();

            string OldFileDirectory = ProjectInstance.LoadedProject.BasePath + OldFileName;
            string NewFileDirectory = ProjectInstance.LoadedProject.BasePath + NewFileName;

            if (!FileManager.IsValidFileName(NewFileName))
            {
                ServerSendFunctions.SendErrorNotification(SenderClientID, "Failed to rename file - File name uses invalid characters.");
                return;
            }

            if (File.Exists(NewFileDirectory))
            {
                ServerSendFunctions.SendErrorNotification(SenderClientID, "Failed to rename file - File with this name already exists.");
                return;
            }

            if (!ProjectInstance.LoadedProject.FileCacheLookup.ContainsKey(OldFileName))
            {
                if (!FileManager.LoadFileIntoCache(OldFileName))
                {
                    ServerSendFunctions.SendErrorNotification(SenderClientID, "Failed to retreive file - Server failed to load it.");
                    return;
                }
            }

            try
            {
                //Replace with async here later?
                File.WriteAllBytes(NewFileDirectory, ProjectInstance.LoadedProject.FileCacheLookup[OldFileName].FileData);               
            }
            catch (Exception E)
            {
                CustomConsole.Log("ServerRecieveError: UserRenameFile - " + E.ToString());
                ServerSendFunctions.SendErrorNotification(SenderClientID, "Failed to rename/move file - Server failed to save the renamed/moved file.");
                return;
            }

            if (!FileManager.DeleteFile(OldFileName)) 
            {
                ServerSendFunctions.SendErrorNotification(SenderClientID, "Failed to rename file - Server failed to clean old file.");

                try
                {
                    File.Delete(NewFileDirectory);
                }
                catch (Exception E)
                {
                    CustomConsole.Log("Failed to clean up renamed file: " + E.ToString());
                }

                return;
            }

            ProjectInstance.LoadedProject.FileCacheLookup.Add(NewFileName, new CacheFileData(ProjectInstance.LoadedProject.FileCacheLookup[OldFileName].FileData));
            ProjectInstance.LoadedProject.FileCacheLookup.Remove(OldFileName);
            ProjectInstance.LoadedProject.UpdateSubscribersLookup.Add(NewFileName, new UpdateFileData(ProjectInstance.LoadedProject.UpdateSubscribersLookup[OldFileName]));
            ProjectInstance.LoadedProject.UpdateSubscribersLookup.Remove(OldFileName);

            ServerSendFunctions.SendFileRenamed(OldFileName, NewFileName);
        }

        /* -PACKET LAYOUT-
         * string FILE STRING (IS FULL LOCAL DIRECTORY, NAME AND EXTENSION)
         */
        public static void UserDeletedFile(int SenderClientID, Packet Data)
        {
            string FileName = Data.ReadString();

            if (!FileManager.DeleteFile(FileName))
            {
                ServerSendFunctions.SendErrorNotification(SenderClientID, "Failed to delete file - Server failed to delete it.");
                return;
            }

            ServerSendFunctions.SendFileDeleted(FileName);

            if (ProjectInstance.LoadedProject.FileCacheLookup.ContainsKey(FileName)) ProjectInstance.LoadedProject.FileCacheLookup.Remove(FileName);
            if (ProjectInstance.LoadedProject.UpdateSubscribersLookup.ContainsKey(FileName)) ProjectInstance.LoadedProject.UpdateSubscribersLookup.Remove(FileName);
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
