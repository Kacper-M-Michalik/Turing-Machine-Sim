using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            LocalPath = ParentFolder == null ? "" : ParentFolder.LocalPath + Name + Path.DirectorySeparatorChar;

            SubFolders = new List<DirectoryFolder>();
            SubFiles = new List<DirectoryFile>();
        }
    }

    public class DirectoryFile
    {
        public int ID;
        public string Name;
        public int Version;
        public DirectoryFolder ParentFolder;
        public HashSet<int> SubscriberIDs;

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
