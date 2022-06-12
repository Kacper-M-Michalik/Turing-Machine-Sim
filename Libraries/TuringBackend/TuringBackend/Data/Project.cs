using System;
using System.Collections.Generic;

namespace TuringBackend
{
    public class CacheFileData
    {
        public byte[] FileData;
        public long ExpiryTimer;

        public CacheFileData(byte[] SetFileData)
        {
            FileData = SetFileData;
            ExpiryTimer = 0;
        }

        public void ResetExpiryTimer()
        {
            ExpiryTimer = 0;
        }
    }

    public class Project
    {
        //Cache System
        public Dictionary<int, CacheFileData> CacheDataLookup;
        public Dictionary<int, DirectoryFile> FileDataLookup;
        public Dictionary<int, DirectoryFolder> FolderDataLookup;

        //Data
        public string ProjectName;
        public string BasePath;
        public string ProjectFilePath;
        public TuringProjectType TuringTypeRule;

        public DirectoryFolder BaseDirectoryFolder;

        //TBD
        public List<Alphabet> ProjectAlphabets;


        //Rules get sent to client -> client Ui responsible for telling uiser cant run turing with these rules, unless is server simulated turing
        //Allow server to simualte hese windwos as to have mutiple users have access to same window not only file:
        //windows
        //turing machine
        //data visualization
        //flow diagram viewer
        //alphabet manager
        //project settings
    }
}
