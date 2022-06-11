using System;
using System.Collections.Generic;
using TuringBackend.Networking;
using TuringBackend.Debugging;

namespace TuringBackend
{
    public static class ProjectInstance
    {
        public static Project LoadedProject;

        public static void StartProjectServer(string Location, int SetMaxClients, int SetPort)
        {
            LoadedProject = FileManager.LoadProjectFile(Location);

            if (LoadedProject != null)
            {
                CustomConsole.Log("Loader Successful");
                Server.StartServer(SetMaxClients, SetPort);                
            }
        }

        public static void CloseProject()
        {           
            Server.CloseServer();
        }
    }
}
