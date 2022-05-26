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
            {(int)ClientSendPackets.RenameFile, UserRenamedFile},
            {(int)ClientSendPackets.MoveFile, UserMovedFile},
            {(int)ClientSendPackets.DeleteFile, UserDeletedFile},
            {(int)ClientSendPackets.UnsubscribeFromUpdatesForFile, UserUnsubscribedFromFileUpdates},
            {(int)ClientSendPackets.CreateFolder, UserCreatedFolder},
            {(int)ClientSendPackets.RenameFolder, UserRenamedFolder},
            {(int)ClientSendPackets.MoveFolder, UserMovedFolder},
            {(int)ClientSendPackets.DeleteFolder, UserDeletedFolder}
        };


        #region Main

        /* -PACKET LAYOUT-
         * int File ID (IS FULL LOCAL DIRECTORY, NAME AND EXTENSION)
         * bool Subscribe To Updates (Whether or not client wants to recieve new version of file when it is updated)
         */
        public static void UserRequestedFile(int SenderClientID, Packet Data)
        {
            CustomConsole.Log("SERVER INSTRUCTION: User requested file.");

            int FileID;
            bool SubscribeToUpdates;

            try
            {
                FileID = Data.ReadInt();
                SubscribeToUpdates = Data.ReadBool();
            }
            catch
            {
                CustomConsole.Log("ServerReceive Error: Invalid request file packet recieved from client: " + SenderClientID.ToString());
                return;
            }

            if (!ProjectInstance.LoadedProject.CacheDataLookup.ContainsKey(FileID))
            {
                if (!FileManager.LoadFileIntoCache(FileID))
                {
                    ServerSendFunctions.SendErrorNotification(SenderClientID, "Failed to retreive file - Server failed to load it.");
                    return;
                }
            }    
            
            if (SubscribeToUpdates) ProjectInstance.LoadedProject.PersistentDataLookup[FileID].SubscriberIDs.Add(SenderClientID);

            ProjectInstance.LoadedProject.CacheDataLookup[FileID].ResetExpiryTimer();
            ServerSendFunctions.SendFile(SenderClientID, FileID);
        }


        /* -PACKET LAYOUT-
         * int Folder ID
         * string File Name (Is Name + Extension)
         */
        public static void UserCreatedNewFile(int SenderClientID, Packet Data)
        {
            CustomConsole.Log("SERVER INSTRUCTION: User created file.");

            int FolderID;
            string FileName;
            try
            {
                FolderID = Data.ReadInt();
                FileName = Data.ReadString();
            }
            catch
            {
                CustomConsole.Log("ServerReceive Error: Invalid create file packet recieved from client: " + SenderClientID.ToString());
                return;
            }

            if (!FileManager.IsValidFileName(FileName))
            {
                ServerSendFunctions.SendErrorNotification(SenderClientID, "Failed to create file - File name uses invalid characters.");
                return;
            }
            if (!ProjectInstance.LoadedProject.FolderLocationLookup.ContainsKey(FolderID))
            {
                ServerSendFunctions.SendErrorNotification(SenderClientID, "Failed to create file - Folder doesnt exist.");
                return;
            }

            string NewFileLocation = ProjectInstance.LoadedProject.FolderLocationLookup[FolderID] + FileName;

            if (File.Exists(NewFileLocation))
            {
                ServerSendFunctions.SendErrorNotification(SenderClientID, "Failed to create file - File already exists.");
                return;
            }

            try
            {
                FileStream Fs = File.Create(NewFileLocation);
                Fs.Close();
            }
            catch (Exception E)
            {
                CustomConsole.Log("ServerRecieve Error: UserCreatedNewFile - " + E.ToString());
                ServerSendFunctions.SendErrorNotification(SenderClientID, "Failed to create file - Server failed to create it.");
            }

            int NewID = FileManager.GetNewFileID();
            ProjectInstance.LoadedProject.PersistentDataLookup.Add(NewID, new PersistentFileData(NewFileLocation));
            FileManager.LoadFileIntoCache(NewID);
        }
                     

        /* -PACKET LAYOUT-
         * int File ID
         * int File Version
         * byte[] New File Data
         */
        public static void UserUpdatedFile(int SenderClientID, Packet Data)
        {
            CustomConsole.Log("SERVER INSTRUCTION: User edited file.");

            int FileID;
            int FileVersion;
            byte[] NewData;

            try
            {
                FileID = Data.ReadInt(); 
                FileVersion = Data.ReadInt();
                NewData = Data.ReadByteArray();
            }
            catch
            {
                CustomConsole.Log("ServerReceive Error: Invalid update file packet recieved from client: " + SenderClientID.ToString());
                return;
            }

            if (!ProjectInstance.LoadedProject.CacheDataLookup.ContainsKey(FileID))
            {
                if (!FileManager.LoadFileIntoCache(FileID))
                {
                    ServerSendFunctions.SendErrorNotification(SenderClientID, "Failed to update file - Server failed to load it.");
                    return;
                }
            }

            //Possibly implement parsing bytes into actual object to see if it succeeds -> prevent users from sending corrupt files

            PersistentFileData FileData = ProjectInstance.LoadedProject.PersistentDataLookup[FileID];
            if (FileData.VersionNumber != FileVersion)
            {
                ServerSendFunctions.SendErrorNotification(SenderClientID, "Failed to update file - You updated an older version of the file.");
                return;
            }

            try
            {                
                //Replace with async here later?
                File.WriteAllBytes(FileData.FileLocation, NewData);
            }
            catch (Exception E)
            {
                CustomConsole.Log("ServerRecieve Error: UserEditedFile - " + E.ToString());
                ServerSendFunctions.SendErrorNotification(SenderClientID, "Failed to update file - Server failed to write to file.");
                return;
            }

            FileData.VersionNumber++;

            ProjectInstance.LoadedProject.CacheDataLookup[FileID].FileData = NewData;
            ProjectInstance.LoadedProject.CacheDataLookup[FileID].ResetExpiryTimer();

            foreach (int Client in FileData.SubscriberIDs)
            {
                ServerSendFunctions.SendFileUpdate(Client, FileID);
            }
            
        }


        /* -PACKET LAYOUT-
         * int File ID
         * string New File Name
         */
        public static void UserRenamedFile(int SenderClientID, Packet Data)
        {
            int FileID;
            string NewFileName;

            try
            {
                FileID = Data.ReadInt();
                NewFileName = Data.ReadString();
            }
            catch
            {
                CustomConsole.Log("ServerReceive Error: Invalid rename file packet recieved from client: " + SenderClientID.ToString());
                return;
            }

            if (!ProjectInstance.LoadedProject.CacheDataLookup.ContainsKey(FileID))
            {
                if (!FileManager.LoadFileIntoCache(FileID))
                {
                    ServerSendFunctions.SendErrorNotification(SenderClientID, "Failed to rename file - Server failed to load it.");
                    return;
                }
            }

            if (!FileManager.IsValidFileName(NewFileName))
            {
                ServerSendFunctions.SendErrorNotification(SenderClientID, "Failed to rename file - File name uses invalid characters.");
                return;
            }

            PersistentFileData FileData = ProjectInstance.LoadedProject.PersistentDataLookup[FileID];
            string NewFileLocation = FileManager.PathParentDirectory(FileData.FileLocation) + NewFileName;

            if (File.Exists(NewFileLocation))
            {
                ServerSendFunctions.SendErrorNotification(SenderClientID, "Failed to rename file - File with this name already exists.");
                return;
            }

            try
            {
                //Replace with async here later?
                File.WriteAllBytes(NewFileLocation, ProjectInstance.LoadedProject.CacheDataLookup[FileID].FileData);               
            }
            catch (Exception E)
            {
                CustomConsole.Log("ServerRecieveError: UserRenameFile - " + E.ToString());
                ServerSendFunctions.SendErrorNotification(SenderClientID, "Failed to rename/move file - Server failed to save the renamed/moved file.");
                return;
            }

            if (!FileManager.DeleteFileByPath(FileData.FileLocation)) 
            {
                ServerSendFunctions.SendErrorNotification(SenderClientID, "Failed to rename/move file - Server failed to clean old file.");
                FileManager.DeleteFileByPath(NewFileLocation);
                return;
            }

            FileData.FileLocation = NewFileLocation;

            ServerSendFunctions.SendFileRenamed(FileID);
        }

        /* -PACKET LAYOUT-
         * int File ID
         * int New Folder ID
         */
        public static void UserMovedFile(int SenderClientID, Packet Data)
        {
            int FileID;
            int NewFolderID;

            try
            {
                FileID = Data.ReadInt();
                NewFolderID = Data.ReadInt();
            }
            catch
            {
                CustomConsole.Log("ServerReceive Error: Invalid move file packet recieved from client: " + SenderClientID.ToString());
                return;
            }

            if (!ProjectInstance.LoadedProject.CacheDataLookup.ContainsKey(FileID))
            {
                if (!FileManager.LoadFileIntoCache(FileID))
                {
                    ServerSendFunctions.SendErrorNotification(SenderClientID, "Failed to move file - Server failed to load it.");
                    return;
                }
            }

            if (!ProjectInstance.LoadedProject.FolderLocationLookup.ContainsKey(NewFolderID))
            {
                ServerSendFunctions.SendErrorNotification(SenderClientID, "Failed to move file - Target folder doesn't exist.");
                return;
            }

            PersistentFileData FileData = ProjectInstance.LoadedProject.PersistentDataLookup[FileID];
            string NewFileLocation = ProjectInstance.LoadedProject.FolderLocationLookup[NewFolderID] + FileData.FileLocation.Substring(FileData.FileLocation.LastIndexOf(Path.DirectorySeparatorChar)+1);

            if (File.Exists(NewFileLocation))
            {
                ServerSendFunctions.SendErrorNotification(SenderClientID, "Failed to move file - File with this name already exists.");
                return;
            }

            try
            {
                //Replace with async here later?
                File.WriteAllBytes(NewFileLocation, ProjectInstance.LoadedProject.CacheDataLookup[FileID].FileData);
            }
            catch (Exception E)
            {
                CustomConsole.Log("ServerRecieveError: UserRenameFile - " + E.ToString());
                ServerSendFunctions.SendErrorNotification(SenderClientID, "Failed to rename/move file - Server failed to save the renamed/moved file.");
                return;
            }

            if (!FileManager.DeleteFileByPath(FileData.FileLocation))
            {
                ServerSendFunctions.SendErrorNotification(SenderClientID, "Failed to rename/move file - Server failed to clean old file.");
                FileManager.DeleteFileByPath(NewFileLocation);
                return;
            }

            FileData.FileLocation = NewFileLocation;

            ServerSendFunctions.SendFileMoved(FileID);
        }

        /* -PACKET LAYOUT-
         * int FileID
         */
        public static void UserDeletedFile(int SenderClientID, Packet Data)
        {
            int FileID;

            try
            {
                FileID = Data.ReadInt();
            }
            catch
            {
                CustomConsole.Log("ServerReceive Error: Invalid delete file packet recieved from client: " + SenderClientID.ToString());
                return;
            }

            if (!ProjectInstance.LoadedProject.PersistentDataLookup.ContainsKey(FileID))
            {
                ServerSendFunctions.SendErrorNotification(SenderClientID, "Failed to delete file - File doesn't exist.");
                return;
            }

            if (!FileManager.DeleteFileByPath(ProjectInstance.LoadedProject.PersistentDataLookup[FileID].FileLocation))
            {
                ServerSendFunctions.SendErrorNotification(SenderClientID, "Failed to delete file - Server failed to delete it.");
                return;
            }

            if (ProjectInstance.LoadedProject.CacheDataLookup.ContainsKey(FileID)) ProjectInstance.LoadedProject.CacheDataLookup.Remove(FileID);
            ProjectInstance.LoadedProject.PersistentDataLookup.Remove(FileID);

            ServerSendFunctions.SendFileDeleted(FileID);
        }

        /* -PACKET LAYOUT-
         * int FileID
         */
        public static void UserUnsubscribedFromFileUpdates(int SenderClientID, Packet Data)
        {
            int FileID;

            try
            {
                FileID = Data.ReadInt();
            }
            catch
            {
                CustomConsole.Log("ServerReceive Error: Invalid delete file packet recieved from client: " + SenderClientID.ToString());
                return;
            }

            CustomConsole.Log("SERVER INSTRUCTION: User "+SenderClientID.ToString()+" no longer recieiving updates to file "+ FileID.ToString()+".");

            ProjectInstance.LoadedProject.PersistentDataLookup[FileID].SubscriberIDs.Remove(SenderClientID);
        }

                


        /* -PACKET LAYOUT-
         * int Parent Folder ID
         * string Folder Name
         */
        public static void UserCreatedFolder(int SenderClientID, Packet Data)
        {
            int ParentFolderID;
            string NewFolderName;

            try
            {
                ParentFolderID = Data.ReadInt();
                NewFolderName = Data.ReadString();
            }
            catch
            {
                CustomConsole.Log("ServerReceive Error: Invalid delete file packet recieved from client: " + SenderClientID.ToString());
                return;
            }

            if (!FileManager.IsValidFileName(NewFolderName))
            {
                ServerSendFunctions.SendErrorNotification(SenderClientID, "Failed to create folder - New folder name invalid.");
                return;
            }

            string NewFolderDirectory = ProjectInstance.LoadedProject.FolderLocationLookup[ParentFolderID] + NewFolderName + Path.DirectorySeparatorChar;

            if (Directory.Exists(NewFolderDirectory))
            {                
                ServerSendFunctions.SendErrorNotification(SenderClientID, "Failed to create folder - Root folder doesn't exist.");
                return;     
            }

            try
            {
                Directory.CreateDirectory(NewFolderDirectory);
            }
            catch (Exception E)
            {
                CustomConsole.Log("ServerRecieve Error: UserCreatedFolder - " + E.ToString());
                ServerSendFunctions.SendErrorNotification(SenderClientID, "Failed to create folder - Server failed to create the folder.");
                return;
            }

            ProjectInstance.LoadedProject.FolderLocationLookup.Add(FileManager.GetNewFileID(), NewFolderDirectory);
        }

        /* -PACKET LAYOUT-
         * string OLD FOLDER STRING (IS FULL LOCAL DIRECTORY)
         * string NEW FOLDER STRING (IS FULL LOCAL DIRECTORY)
         */
        public static void UserRenamedFolder(int SenderClientID, Packet Data)
        {

        }

        /* -PACKET LAYOUT-
         * string OLD FOLDER STRING (IS FULL LOCAL DIRECTORY)
         * string NEW FOLDER STRING (IS FULL LOCAL DIRECTORY)
         */
        public static void UserMovedFolder(int SenderClientID, Packet Data)
        {

        }

        /* -PACKET LAYOUT-
         * string FOLDER STRING (IS FULL LOCAL DIRECTORY)
         */
        public static void UserDeletedFolder(int SenderClientID, Packet Data)
        {

        }

        #endregion

    }

}
