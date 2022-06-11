using System;
using TuringBackend;
using TuringBackend.Debugging;
using System.Threading;
using TuringBackend.Networking;
using System.Text.Json;
using System.Collections.Generic;
using System.Text;

namespace TuringTesting
{
    class Program
    {

        static void Main(string[] args)
        {
            CustomConsole.Log("UI: APP RUNNING ON THREAD " + Thread.CurrentThread.ManagedThreadId.ToString());

            /*
            ProjectFile PF = new ProjectFile
            {
                Folders = new List<string>() { "Tapes\\" },
                Files = new List<string>() { "Tapes\\newtape.tape", "Tapes\\TestTape.tape" }
            };

            JsonSerializerOptions Options = new JsonSerializerOptions { WriteIndented = true, IncludeFields = true };
            string jsonString = JsonSerializer.Serialize<ProjectFile>(PF, Options);
            System.IO.File.WriteAllBytes("E:\\Professional Programming\\MAIN\\TestLocation\\TestProject.tproj", Encoding.ASCII.GetBytes(jsonString));
            */

            bool Continue = true;
            while (Continue)
            {
                Client.ProcessPackets();                
                string Option = Console.ReadLine();

                switch (Option.ToUpper())
                {
                    case ("START"):
                        ProjectInstance.StartProjectServer("E:\\Professional Programming\\MAIN\\TestLocation", 2, 28104); 
                        ClientInstance.ConnectToLocalServer(28104);
                        break;
                    case ("SERVER"):
                        ProjectInstance.StartProjectServer("E:\\Professional Programming\\MAIN\\TestLocation", 2, 28104);
                        break;
                    case ("SAVE"):
                        FileManager.SaveProject();
                        break;
                    case ("CONNECT"):
                        ClientInstance.ConnectToLocalServer(28104);
                        break;
                    case ("DISCONNECT"):
                        ClientInstance.Disconnect();
                        break;
                    case ("SERVERID"):
                        CustomConsole.Log("SERVER THREAD: " + Server.ServerThread.ManagedThreadId.ToString());
                        break;
                    case ("CREATE"):
                        int BaseFolder = Convert.ToInt32(Console.ReadLine());
                        string CreateName = Console.ReadLine();
                        ClientSendFunctions.CreateFile(BaseFolder, CreateName);
                        break;
                    case ("REQUEST"):
                        int RequestID = Convert.ToInt32(Console.ReadLine());
                        ClientSendFunctions.RequestFile(RequestID, true);
                        break;
                    case ("RENAME"):
                        int RenameID = Convert.ToInt32(Console.ReadLine());
                        string NewName = Console.ReadLine();
                        ClientSendFunctions.RenameFile(RenameID, NewName);
                        break;
                    case ("MOVE"):
                        int MoveID = Convert.ToInt32(Console.ReadLine());
                        int MoveFolderID = Convert.ToInt32(Console.ReadLine());
                        ClientSendFunctions.MoveFile(MoveID, MoveFolderID);
                        break;
                    case ("EDIT"):
                        int EditID = Convert.ToInt32(Console.ReadLine());
                        int Version = Convert.ToInt32(Console.ReadLine());
                        string NewContents = Console.ReadLine();
                        ClientSendFunctions.UpdateFile(EditID, Version, NewContents);
                        break;
                    case ("DELETE"):
                        int DeleteID = Convert.ToInt32(Console.ReadLine());
                        ClientSendFunctions.DeleteFile(DeleteID);
                        break;
                    case ("UNSUB"):
                        int UnsubID = Convert.ToInt32(Console.ReadLine());
                        ClientSendFunctions.UnsubscribeFromFileUpdates(UnsubID);
                        break;
                    case ("CFOLDER"):
                        int NBaseFolder = Convert.ToInt32(Console.ReadLine());
                        string Name = Console.ReadLine();
                        ClientSendFunctions.CreateFolder(NBaseFolder, Name);
                        break;
                    case ("RFOLDER"):
                        int RFolder = Convert.ToInt32(Console.ReadLine());
                        string Rename = Console.ReadLine();
                        ClientSendFunctions.RenameFolder(RFolder, Rename);
                        break;
                    case ("MFOLDER"):
                        int MFolder = Convert.ToInt32(Console.ReadLine()); 
                        int MTFolder = Convert.ToInt32(Console.ReadLine());
                        ClientSendFunctions.MoveFolder(MFolder, MTFolder);
                        break;
                    case ("DFOLDER"):
                        int DFolder = Convert.ToInt32(Console.ReadLine());
                        ClientSendFunctions.DeleteFolder(DFolder);
                        break;
                    case ("KILL CLIENT"):
                        Server.Clients[0].DisconnectClientFromServer();
                        break;
                    case ("BREAKPOINT"):
                        //Add breakpoint below
                        ProjectInstance.LoadedProject.GetType();
                        break;
                    case ("STOP"):
                        ProjectInstance.CloseProject();
                        break;
                    default:
                        break;
                }
                
            }
        }

    }
}
