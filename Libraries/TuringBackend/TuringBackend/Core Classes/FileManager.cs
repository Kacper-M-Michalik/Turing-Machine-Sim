using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using TuringBackend.Debugging;
using TuringBackend.SaveFiles;
using System.Text.Json;

namespace TuringBackend
{
    public static class FileManager
    {
        public static bool IsValidFileName(string FileName)
        {
            //Maybe rewrite using regex?
            //https://docs.microsoft.com/en-gb/windows/win32/fileio/naming-a-file?redirectedfrom=MSDN for seeing banned characters
            if (FileName.Contains('<') || FileName.Contains('>') || FileName.Contains(':') || FileName.Contains('"') || FileName.Contains('/') || FileName.Contains('\\') || FileName.Contains('|') || FileName.Contains('?') || FileName.Contains('*'))
            {                
                return false;
            }

            return true;
        }

        /*
        public static string PathParentDirectory(string BasePath)
        {
            int LastDirectoryIndex = BasePath.LastIndexOf(Path.DirectorySeparatorChar);

            return BasePath.Substring(0, LastDirectoryIndex);
        }
        */

        //ID 0 reserved for BaseFolder
        static int NextID = 1;
        public static int GetNewFileID()
        {
            return NextID++;
        }

        public static Project LoadProjectFile(string FilePath)
        {
            string CorrectPath = "";
            if (FilePath.Substring(FilePath.Length-6) == ".tproj")
            {
                CorrectPath = FilePath;
            }
            else
            {
                //Search for tproj file
                string[] AllFiles = Directory.GetFiles(FilePath);
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

            string ProjectBasePath = Directory.GetParent(CorrectPath).ToString() + Path.DirectorySeparatorChar;

            string ProjectJson =  File.ReadAllText(CorrectPath);

            /*
            ProjectFile ProjectData = JsonSerializer.Deserialize<ProjectFile>(ProjectJson);

            Dictionary<int, string> NewFolderLocationLookup = new Dictionary<int, string>() { { 0, ProjectBasePath } };
            Dictionary<int, DirectoryFile> NewPersistentDataLookup = new Dictionary<int, DirectoryFile>();

            for (int i = 0; i < ProjectData.Folders.Count; i++)
            {
                NewFolderLocationLookup.Add(GetNewFileID(), ProjectBasePath + ProjectData.Folders[i].Replace('\\', Path.DirectorySeparatorChar));
            }
            for (int i = 0; i < ProjectData.Files.Count; i++)
            {
                NewPersistentDataLookup.Add(GetNewFileID(), new PersistentFileData(ProjectBasePath + ProjectData.Files[i].Replace('\\', Path.DirectorySeparatorChar)));
            }
            */

            return new Project()
            {
                BasePath = ProjectBasePath,
                ProjectFilePath = CorrectPath,
                CacheDataLookup = new Dictionary<int, CacheFileData>(),
                FileDataLookup = new Dictionary<int, DirectoryFile>() { },
                FolderDataLookup = new Dictionary<int, DirectoryFolder>() { { 0, new DirectoryFolder(0, "", null) } }
            };            

        }

        public static bool LoadFileIntoCache(int FileID)
        {
            if (ProjectInstance.LoadedProject.FileDataLookup.ContainsKey(FileID))
            {
                if (ProjectInstance.LoadedProject.CacheDataLookup.ContainsKey(FileID))
                {
                    ProjectInstance.LoadedProject.CacheDataLookup[FileID].ResetExpiryTimer();
                    return true;
                }
                else
                {
                    try
                    {
                        ProjectInstance.LoadedProject.CacheDataLookup.Add(FileID, new CacheFileData(
                                File.ReadAllBytes(ProjectInstance.LoadedProject.BasePath + ProjectInstance.LoadedProject.FileDataLookup[FileID].GetLocalPath())
                            ));
                        return true;
                    }
                    catch (Exception E)
                    {
                        CustomConsole.Log("File Manager Error: LoadFileIntoCache - " + E.ToString());
                    }
                }       
            }

            return false;
        }
        
        public static bool DeleteFileByPath(string FilePath)
        {
            try
            {
                File.Delete(FilePath);
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
