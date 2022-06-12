using Godot;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using TuringBackend;
using TuringBackend.Logging;
using TuringFrontend.Networking;

public class MenuScreenManager : Node
{
    PackedScene CreateProjectWindow = (PackedScene)GD.Load("res://Assets/UI Prefabs/CreateProjectPrefab.tscn");
    PackedScene LoadProjectWindow = (PackedScene)GD.Load("res://Assets/UI Prefabs/LoadProjectPrefab.tscn");

    Node CreateProjectWindowInstance;
    Node LoadProjectWindowInstance;
    FileDialog ProjectFileDialog;

    public override void _Ready()
    {
        CustomConsole.LogPointer = delegate (string Message) { GD.Print(Message); };
        ProjectFileDialog = (FileDialog)GetNode("/root/Control/FileDialog");
    }

    public void ClearScreen()
    {
        if (CreateProjectWindowInstance != null)
        {
            CreateProjectWindowInstance.Free();
            CreateProjectWindowInstance = null;
        }
        if (LoadProjectWindowInstance != null)
        {
            LoadProjectWindowInstance.Free();
            LoadProjectWindowInstance = null;
        }
        ProjectFileDialog.Hide();
    }

    public void CreateProject()
    {
        ClearScreen();

        CreateProjectWindowInstance = CreateProjectWindow.Instance();
        GetParent().AddChild(CreateProjectWindowInstance);
    }

    public void LoadProject()
    {
        ClearScreen();

        LoadProjectWindowInstance = LoadProjectWindow.Instance();
        GetParent().AddChild(LoadProjectWindowInstance);
    }   

}

