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

            //For now we strucutre tproj fiel like this:
            //Settings, each is a setting name, =, and a value
            //Those are hard coded in order for now
            //Eventually we hit a line that contains:
            //FILES
            //This signifies everythign below is a included file directory

            //Considering Rewriting the whole thing to json later
            //VS does xml for csproj but does something very similiar to what i have above for sln files.

            string[] ProjectFileData =  File.ReadAllLines(CorrectPath);

            //get first line
            //check if files header exists
            //loop, if contaisn extension is a file, add to lookup, else is folder, will be used later for when construcitng directory object

            //maybe instead of dir object just read full contents of folders through os each time - allwos for drag and drop trhoug fiel explorer?         

            return new Project()
            {
                BasePath = Directory.GetParent(CorrectPath).ToString()+"\\",
                ProjectFilePath = CorrectPath,
                FileCacheLookup = new Dictionary<string, byte[]>()
            };

        }

    }
}
