using System.Text.Json;
using System.Text.Json.Serialization;

namespace TuringBackend.SaveFiles
{
    public class ProjectSaveFile
    {
        [JsonInclude]
        public string ProjectName;
        [JsonInclude]
        public string BaseFolder;
        [JsonInclude]
        public TuringProjectType TypeRule;

        [JsonConstructor]
        public ProjectSaveFile(string ProjectName, string BaseFolder, TuringProjectType TypeRule)
        {
            this.ProjectName = ProjectName;
            this.BaseFolder = BaseFolder;
            this.TypeRule = TypeRule;
        }
    }
}
