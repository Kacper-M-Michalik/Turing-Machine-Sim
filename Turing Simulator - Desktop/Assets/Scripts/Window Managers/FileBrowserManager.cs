using Godot;
using System;
using TuringBackend;

public class FileBrowserManager : Node
{
    //Temp?
    DirectoryFolder BaseFolder;
    DirectoryFolder CurrentFolder;

    public override void _Ready()
    {
        
    }
    
    public void ReloadUI()
    {
        for (int i = 0; i < CurrentFolder.SubFolders.Count; i++)
        {
            //create ui icon, place in
        }
    }
}
