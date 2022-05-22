using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using TuringBackend.Debugging;

namespace TuringBackend
{
    public static class FileManager
    {
        public static bool IsValidFileName(string FileName)
        {
            //Maybe rewrite using regex?
            //https://docs.microsoft.com/en-gb/windows/win32/fileio/naming-a-file?redirectedfrom=MSDN for seeing banned characters
            if (FileName.Contains('<') || FileName.Contains('>') || FileName.Contains(':') || FileName.Contains('"') || FileName.Contains('/') || FileName.Contains('|') || FileName.Contains('?') || FileName.Contains('*'))
            {                
                return false;
            }

            return true;
        }

        public static string PathParentDirectory(string Path)
        {
            int LastDirectoryIndex = Path.LastIndexOf("\\");

            return Path.Substring(0, LastDirectoryIndex);
        }

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
                BasePath = Directory.GetParent(CorrectPath).ToString() + "\\",
                ProjectFilePath = CorrectPath,
                FileCacheLookup = new Dictionary<string, CacheFileData>(),
                UpdateSubscribersLookup = new Dictionary<string, UpdateFileData>()
            };

        }

        public static bool LoadFileIntoCache(string FileName)
        {
            string FullFileDirectory = ProjectInstance.LoadedProject.BasePath + FileName;
            if (File.Exists(FullFileDirectory))
            {
                try
                {
                    ProjectInstance.LoadedProject.FileCacheLookup.Add(FileName, new CacheFileData(File.ReadAllBytes(FullFileDirectory)));
                    if (!ProjectInstance.LoadedProject.UpdateSubscribersLookup.ContainsKey(FileName)) ProjectInstance.LoadedProject.UpdateSubscribersLookup.Add(FileName, new UpdateFileData());
                    return true;
                }
                catch (Exception E)
                {
                    CustomConsole.Log("File Manager Error: LoadFileIntoCache - " + E.ToString());
                }                
            }

            return false;
        }
        
        public static bool DeleteFile(string FileName)
        {
            try
            {
                File.Delete(ProjectInstance.LoadedProject.BasePath + FileName);
            }
            catch (Exception E)
            {
                CustomConsole.Log("File Manager Error: DeleteFile - " + E.ToString());
                return false;
            }

            return true;
        }

    }
}
