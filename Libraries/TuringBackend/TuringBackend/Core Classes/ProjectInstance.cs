using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TuringBackend.Networking;
using TuringBackend.Debugging;

namespace TuringBackend
{
    public static class ProjectInstance
    {
        public static Project LoadedProject;
        public static int MaxClients;
        public static int Port;

        public static void StartProjectServer(string Location, int SetMaxClients, int SetPort)
        {
            LoadedProject = FileManager.LoadProjectFile(Location);
            MaxClients = SetMaxClients;
            Port = SetPort;

            if (LoadedProject != null)
            {
                CustomConsole.Log("Loader Successful");
                Server.StartServer(MaxClients, Port);                
            }
        }

        public static void CloseProject()
        {           
            Server.CloseServer();
            //Do saving here
        }
    }
}
