using Godot;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

public class ProjectManager : Node
{
    List<Node> Windows = new List<Node>();

    public void OpenProject()
    {

    }

    public override void _Notification(int ID)
    {
        if (ID == MainLoop.NotificationWmQuitRequest)
        {
            //ClientInstance.Disconnect();
            //ProjectInstance.CloseProject();
            GetTree().Quit();
        }
    }
}

