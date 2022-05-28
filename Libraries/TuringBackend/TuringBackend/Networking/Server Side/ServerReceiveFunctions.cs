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
           
            if (!FileManager.LoadFileIntoCache(FileID))
            {
                ServerSendFunctions.SendErrorNotification(SenderClientID, "Failed to retreive file - Server failed to load it.");
                return;
            }             
            
            if (SubscribeToUpdates) ProjectInstance.LoadedProject.FileDataLookup[FileID].SubscriberIDs.Add(SenderClientID);

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
            if (!ProjectInstance.LoadedProject.FolderDataLookup.ContainsKey(FolderID))
            {
                ServerSendFunctions.SendErrorNotification(SenderClientID, "Failed to create file - Folder doesnt exist.");
                return;
            }

            DirectoryFolder DirFolder = ProjectInstance.LoadedProject.FolderDataLookup[FolderID];
            string NewFileLocation = ProjectInstance.LoadedProject.BasePath + DirFolder.LocalPath + FileName;

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
            DirectoryFile NewFileData = new DirectoryFile(NewID, FileName, ProjectInstance.LoadedProject.FolderDataLookup[FolderID]);
            ProjectInstance.LoadedProject.FileDataLookup.Add(NewID, NewFileData);
            DirFolder.SubFiles.Add(NewFileData);
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

            //Possibly implement parsing bytes into actual object to see if it succeeds -> prevent users from sending corrupt files

            if (!ProjectInstance.LoadedProject.FileDataLookup.ContainsKey(FileID))
            {
                ServerSendFunctions.SendErrorNotification(SenderClientID, "Failed to update file - File doesn't exist.");
                return;
            }

            DirectoryFile FileData = ProjectInstance.LoadedProject.FileDataLookup[FileID];
            if (FileData.Version != FileVersion)
            {
                ServerSendFunctions.SendErrorNotification(SenderClientID, "Failed to update file - You updated an older version of the file.");
                return;
            }

            try
            {                
                //Replace with async here later?
                File.WriteAllBytes(ProjectInstance.LoadedProject.BasePath + FileData.GetLocalPath(), NewData);
            }
            catch (Exception E)
            {
                CustomConsole.Log("ServerRecieve Error: UserEditedFile - " + E.ToString());
                ServerSendFunctions.SendErrorNotification(SenderClientID, "Failed to update file - Server failed to write to file.");
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
            else
            {
                ProjectInstance.LoadedProject.CacheDataLookup[FileID].FileData = NewData;
            }

            FileData.Version++;

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
            CustomConsole.Log("SERVER INSTRUCTION: User renamed file.");

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

            if (!FileManager.IsValidFileName(NewFileName))
            {
                ServerSendFunctions.SendErrorNotification(SenderClientID, "Failed to rename file - File name uses invalid characters.");
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

            DirectoryFile FileData = ProjectInstance.LoadedProject.FileDataLookup[FileID];

            string NewFileLocation = ProjectInstance.LoadedProject.BasePath + FileData.ParentFolder.LocalPath + Path.DirectorySeparatorChar + NewFileName;

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

            if (!FileManager.DeleteFileByPath(ProjectInstance.LoadedProject.BasePath + FileData.GetLocalPath())) 
            {
                ServerSendFunctions.SendErrorNotification(SenderClientID, "Failed to rename/move file - Server failed to clean old file.");
                FileManager.DeleteFileByPath(NewFileLocation);
                return;
            }

            FileData.Name = NewFileName;

            ServerSendFunctions.SendFileRenamed(FileID);
        }

        /* -PACKET LAYOUT-
         * int File ID
         * int New Folder ID
         */
        public static void UserMovedFile(int SenderClientID, Packet Data)
        {
            CustomConsole.Log("SERVER INSTRUCTION: User moved file.");

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

            if (!ProjectInstance.LoadedProject.FolderDataLookup.ContainsKey(NewFolderID))
            {
                ServerSendFunctions.SendErrorNotification(SenderClientID, "Failed to move file - Target folder doesn't exist.");
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

            DirectoryFile FileData = ProjectInstance.LoadedProject.FileDataLookup[FileID];
            DirectoryFolder FolderData = ProjectInstance.LoadedProject.FolderDataLookup[NewFolderID];
            string NewFileLocation = ProjectInstance.LoadedProject.BasePath + FolderData.LocalPath + FileData.Name;

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

            if (!FileManager.DeleteFileByPath(ProjectInstance.LoadedProject.BasePath + FileData.GetLocalPath()))
            {
                ServerSendFunctions.SendErrorNotification(SenderClientID, "Failed to rename/move file - Server failed to clean old file.");
                FileManager.DeleteFileByPath(NewFileLocation);
                return;
            }

            FileData.ParentFolder.SubFiles.Remove(FileData);
            FileData.ParentFolder = FolderData;

            ServerSendFunctions.SendFileMoved(FileID);
        }

        /* -PACKET LAYOUT-
         * int FileID
         */
        public static void UserDeletedFile(int SenderClientID, Packet Data)
        {
            CustomConsole.Log("SERVER INSTRUCTION: User deleted file.");

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

            if (!ProjectInstance.LoadedProject.FileDataLookup.ContainsKey(FileID))
            {
                ServerSendFunctions.SendErrorNotification(SenderClientID, "Failed to delete file - File doesn't exist.");
                return;
            }

            DirectoryFile FileData = ProjectInstance.LoadedProject.FileDataLookup[FileID];

            if (!FileManager.DeleteFileByPath(ProjectInstance.LoadedProject.BasePath + FileData.GetLocalPath()))
            {
                ServerSendFunctions.SendErrorNotification(SenderClientID, "Failed to delete file - Server failed to delete it.");
                return;
            }

            FileData.ParentFolder.SubFiles.Remove(FileData);

            ProjectInstance.LoadedProject.CacheDataLookup.Remove(FileID);
            ProjectInstance.LoadedProject.FileDataLookup.Remove(FileID);

            ServerSendFunctions.SendFileDeleted(FileID);
        }

        /* -PACKET LAYOUT-
         * int FileID
         */
        public static void UserUnsubscribedFromFileUpdates(int SenderClientID, Packet Data)
        {
            CustomConsole.Log("SERVER INSTRUCTION: User unsubed from file.");

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

            if (!ProjectInstance.LoadedProject.FileDataLookup.ContainsKey(FileID))
            {
                ServerSendFunctions.SendErrorNotification(SenderClientID, "Failed to unsubscribe from file - File doesn't exist.");
                return;
            }

            CustomConsole.Log("SERVER INSTRUCTION: User "+SenderClientID.ToString()+" no longer recieiving updates to file "+ FileID.ToString()+".");

            ProjectInstance.LoadedProject.FileDataLookup[FileID].SubscriberIDs.Remove(SenderClientID);
            ServerSendFunctions.SendFileUnsubscribed(FileID);
        }

                


        /* -PACKET LAYOUT-
         * int Parent Folder ID
         * string Folder Name
         */
        public static void UserCreatedFolder(int SenderClientID, Packet Data)
        {
            CustomConsole.Log("SERVER INSTRUCTION: User created folder.");

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

            if (!ProjectInstance.LoadedProject.FolderDataLookup.ContainsKey(ParentFolderID))
            {
                ServerSendFunctions.SendErrorNotification(SenderClientID, "Failed to create folder - Root folder doesn't exist.");
                return;
            }

            DirectoryFolder ParentFolderData = ProjectInstance.LoadedProject.FolderDataLookup[ParentFolderID];

            string NewFolderDirectory = ProjectInstance.LoadedProject.BasePath + ParentFolderData.LocalPath + NewFolderName + Path.DirectorySeparatorChar;

            if (Directory.Exists(NewFolderDirectory))
            {                
                ServerSendFunctions.SendErrorNotification(SenderClientID, "Failed to create folder - Folder with this name already exists.");
                return;     
            }

            try
            {
                Directory.CreateDirectory(NewFolderDirectory);
            }
            catch (Exception E)
            {
                CustomConsole.Log("ServerRecieve Error: UserCreatedFolder - " + E.ToString());
                ServerSendFunctions.SendErrorNotification(SenderClientID, "Failed to create folder - Server failed to create the folder locally.");
                return;
            }

            int NewID = FileManager.GetNewFileID();
            DirectoryFolder NewFolderData = new DirectoryFolder(NewID, NewFolderName, ParentFolderData);
            ParentFolderData.SubFolders.Add(NewFolderData);
            ProjectInstance.LoadedProject.FolderDataLookup.Add(NewID, NewFolderData);
        }

        /* -PACKET LAYOUT-
         * int Folder ID
         * string New Folder Name
         */
        public static void UserRenamedFolder(int SenderClientID, Packet Data)
        {
            int FolderID;
            string NewFolderName;

            try
            {
                FolderID = Data.ReadInt();
                NewFolderName = Data.ReadString();
            }
            catch
            {
                CustomConsole.Log("ServerReceive Error: Invalid delete file packet recieved from client: " + SenderClientID.ToString());
                return;
            }

            if (!FileManager.IsValidFileName(NewFolderName))
            {
                ServerSendFunctions.SendErrorNotification(SenderClientID, "Failed to rename folder - New folder name invalid.");
                return;
            }


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
