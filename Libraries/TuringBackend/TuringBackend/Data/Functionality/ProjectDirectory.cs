using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TuringBackend.Data.Functionality
{
    class DirectoryObject { }

    class DirectoryFolder : DirectoryObject
    {
        string Name;
        int ID;

        List<DirectoryObject> SubFiles;
    }

    class DirectoryFile : DirectoryObject
    {
        string Name;
        int ID;
        int Version;
    }

}
