using Godot;
using System;
using TuringBackend;
using TuringBackend.Debugging;

public class TestingScript : Control
{
    public FileDialog FD;

    public override void _Ready()
    {
        FD = (FileDialog)GetNode("/root/Control/FileDialog");
        CustomConsole.LogPointer = delegate (string Message) { GD.Print(Message); }; ;
    }
        
    public void LoadProjectButtonPressed()
    {
        FD.Popup_();
    }

    public void OnProjectPathSelected(string Path)
    {
        ProjectInstance.StartProjectServer(Path, 2, 28104);
        ClientInstance.ConnectToLocalServer(28104);
    }

    public override void _Notification(int ID)
    {
        if (ID == MainLoop.NotificationWmQuitRequest)
        {
            ClientInstance.Disconnect();
            ProjectInstance.CloseProject();
            GetTree().Quit();
        }
    }
}