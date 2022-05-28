using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace TuringBackend.SaveFiles
{
    public class ProjectFile
    {
        [JsonInclude]
        public List<string> Folders;
        [JsonInclude]
        public List<string> Files;
    }
}
