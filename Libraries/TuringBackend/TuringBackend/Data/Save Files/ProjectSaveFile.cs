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
        public TuringProjectType TuringTypeRule;

        [JsonConstructor]
        public ProjectSaveFile(string ProjectName, string BaseFolder, TuringProjectType TuringTypeRule)
        {
            this.ProjectName = ProjectName;
            this.BaseFolder = BaseFolder;
            this.TuringTypeRule = TuringTypeRule;
        }
    }
}
