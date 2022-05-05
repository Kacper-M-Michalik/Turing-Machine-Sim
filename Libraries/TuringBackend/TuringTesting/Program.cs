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
                        ProjectInstance.StartProjectServer("E:\\Professional Programming\\MAIN\\Turing-Machine-Sim\\TestLocation", 2, 28104);
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
                        ClientSendFunctions.CreateFile("test.tape");
                        break;
                    case ("REQUEST FILE"):
                        ClientSendFunctions.RequestProjectFiles("a.tape");
                        ClientSendFunctions.RequestProjectFiles("testtape2.tape");
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
