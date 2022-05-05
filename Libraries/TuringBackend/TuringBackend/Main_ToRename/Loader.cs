using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace TuringBackend
{
    public static class Loader
    {
        public static Project LoadProjectFile(string Path)
        {
            string CorrectPath = "";
            if (Path.Substring(Path.Length-6) == ".tproj")
            {
                CorrectPath = Path;
            }
            else
            {
                //Search for tproj file
                string[] AllFiles = Directory.GetFiles(Path);
                for (int i = 0; i < AllFiles.Length; i++)
                {
                    if (AllFiles[i].Substring(AllFiles[i].Length - 6) == ".tproj")
                    {
                        CorrectPath = AllFiles[i];
                        i = AllFiles.Length;
                    }
                }
            }

            if (CorrectPath == "") return null;            

            return new Project() {BasePath = Directory.GetParent(CorrectPath).ToString(), ProjectFilePath = CorrectPath};

        }

    }
}
