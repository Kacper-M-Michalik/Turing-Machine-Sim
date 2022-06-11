using System;
using System.Collections.Generic;
using System.IO;

namespace TuringBackend
{
    public class DirectoryFolder
    {
        public int ID;

        public string Name;
        public DirectoryFolder ParentFolder;
        public string LocalPath;

        public List<DirectoryFolder> SubFolders;
        public List<DirectoryFile> SubFiles;

        public DirectoryFolder(int SetID, string SetName, DirectoryFolder SetParentFolder)
        {
            ID = SetID;
            Name = SetName;
            ParentFolder = SetParentFolder;
            UpdatePath();

            SubFolders = new List<DirectoryFolder>();
            SubFiles = new List<DirectoryFile>();
        }

        public void UpdatePath()
        {
            LocalPath = ParentFolder == null ? Name + Path.DirectorySeparatorChar : ParentFolder.LocalPath + Name + Path.DirectorySeparatorChar;
        }
    }

    public class DirectoryFile
    {
        public int ID;
        public int Version;
        public HashSet<int> SubscriberIDs;

        public string Name;
        public DirectoryFolder ParentFolder;

        public DirectoryFile(int SetID, string SetName, DirectoryFolder SetParentFolder)
        {
            ID = SetID;
            Name = SetName;
            Version = 1;
            ParentFolder = SetParentFolder;
            SubscriberIDs = new HashSet<int>();
        }

        public string GetLocalPath()
        {
            return ParentFolder.LocalPath + Name;
        }
    }

}
