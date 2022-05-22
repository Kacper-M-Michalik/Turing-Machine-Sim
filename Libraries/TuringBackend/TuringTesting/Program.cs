using System;
using TuringBackend;
using TuringBackend.Debugging;
using System.Threading;
using TuringBackend.Networking;

namespace TuringTesting
{
    class Program
    {

        static void Main(string[] args)
        {
            CustomConsole.Log("UI: APP RUNNING ON THREAD " + Thread.CurrentThread.ManagedThreadId.ToString());

            bool Continue = true;
            while (Continue)
            {
                Client.ProcessPackets();                
                string Option = Console.ReadLine();
                
                switch (Option.ToUpper())
                {
                    case ("SERVER"):
                        ProjectInstance.StartProjectServer("E:\\Professional Programming\\MAIN\\TestLocation", 2, 28104);
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
                    case ("CREATE FILE"):
                        string CreateName = Console.ReadLine() + ".tape";
                        ClientSendFunctions.CreateFile(CreateName);
                        break;
                    case ("REQUEST FILE"):
                        string RequestName = Console.ReadLine() + ".tape";
                        ClientSendFunctions.RequestFile(RequestName+".tape", true);
                        break;
                    case ("RENAME FILE"):
                        string OldName = Console.ReadLine() + ".tape";
                        string NewName = Console.ReadLine() + ".tape";
                        ClientSendFunctions.RenameOrMoveFile(OldName, NewName);
                        break;
                    case ("EDIT FILE"):
                        string EditName = Console.ReadLine() + ".tape";
                        int Version = Convert.ToInt32(Console.ReadLine());
                        string NewContents = Console.ReadLine();
                        ClientSendFunctions.UpdateFile(EditName, Version, NewContents);
                        break;
                    case ("DELETE FILE"):
                        string DeleteName = Console.ReadLine() + ".tape";
                        ClientSendFunctions.DeleteFile(DeleteName);
                        break;
                    case ("UNSUBSCRIBE"):
                        string UnsubName = Console.ReadLine() + ".tape";
                        ClientSendFunctions.UnsubscribeFromFileUpdates(UnsubName);
                        break;
                    case ("KILL CLIENT"):
                        Server.Clients[0].DisconnectClientFromServer();
                        break;
                    case ("BREAKPOINT"):
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
