using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TuringBackend
{
    public class Project
    {
        public List<Alphabet> ProjectAlphabets;

        //For now we load in files from disk into memory as strings and send the string json contents to the client to rebuild the object from json,
        //Will replace system with one where it isnt the whole project stored in memory but only recent files, aka. moving to a cache based system.
        public Dictionary<string, byte[]> FileCacheLookup;
        public Dictionary<string, HashSet<int>> FileUpdateSubscriptionLookup;

        //Settigns in future:
        //rules
        //cache all on startup

        public string BasePath;
        public string ProjectFilePath;

        //windows
        //turing machine
        //data visualization
        //flow diagram viewer
        //alphabet manager
        //project settings
        //main windwo for server???
    }
}
