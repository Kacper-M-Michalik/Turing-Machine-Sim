using Godot;
using System;
using TuringBackend;
using TuringBackend.Debugging;

public class TestingScript : Control
{
    public CanvasItem LoadButton;
    public FileDialog FD;

    public override void _Ready()
    {
        LoadButton = (CanvasItem)GetNode("/root/Control/LoadButton");
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
        LoadButton.Hide();
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