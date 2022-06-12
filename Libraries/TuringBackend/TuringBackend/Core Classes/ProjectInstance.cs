using System;
using System.Collections.Generic;
using TuringBackend.Networking;
using TuringBackend.Logging;

namespace TuringBackend
{
    public static class ProjectInstance
    {
        public static Project LoadedProject;

        public static bool StartProjectServer(string Location, int SetMaxClients, int SetPort)
        {
            LoadedProject = FileManager.LoadProjectFile(Location);

            if (LoadedProject != null)
            {
                CustomConsole.Log("Loader Successful");
                Server.StartServer(SetMaxClients, SetPort);
                return true;
            }
            else
            {
                CustomConsole.Log("Loader Unsuccessful");
            }

            return false;
        }

        public static void CloseProject()
        {
            Server.CloseServer();
        }
    }
}
