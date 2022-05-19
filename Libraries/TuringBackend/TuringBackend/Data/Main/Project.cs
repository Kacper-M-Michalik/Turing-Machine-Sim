using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

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

    public class UpdateFileData
    {
        public int VersionNumber;
        public HashSet<int> SubscriberIDs;

        public UpdateFileData()
        {
            VersionNumber = 1;
            SubscriberIDs = new HashSet<int>();
        }
    }

    public class Project
    {
        public List<Alphabet> ProjectAlphabets;

        //Cache System
        public Dictionary<string, CacheFileData> FileCacheLookup;
        public Dictionary<string, UpdateFileData> UpdateSubscribersLookup;

        //Settigns in future:
        //rules
        //cache all on startup

        public string BasePath;
        public string ProjectFilePath;


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
