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
                string Option = Console.ReadLine();

                switch (Option.ToUpper())
                {
                    case ("SERVER"):
                        ProjectInstance.StartProjectServer("E:\\Professional Programming\\MAIN\\TestLocation", 2, 28104);
                        break;
                    case ("CONNECT"):
                        ClientInstance.ConnectToLocalServer(28104);
                        break;
                    case ("SERVERID"):
                        CustomConsole.Log("SERVER THREAD: " + Server.ServerThread.ManagedThreadId.ToString());
                        break;
                    case ("CREATEFILE"):
                        ClientSendFunctions.CreateFile("testastd.tape");
                        break;
                    case ("KILL"):
                        Server.Clients[0].DisconnectClientFromServer();
                        break;
                    case ("CLIENTID"):
                        break;
                    case ("STOP"):
                        ProjectInstance.CloseProject();
                        //Continue = false;
                        break;
                    default:
                        break;
                }
            }

        }

    }
}
