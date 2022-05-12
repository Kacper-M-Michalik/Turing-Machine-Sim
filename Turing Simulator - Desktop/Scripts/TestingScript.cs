using Godot;
using System;
using TuringBackend;
using TuringBackend.Debugging;

public class TestingScript : Button
{
    public override void _Ready()
    {
        CustomConsole.LogPointer = delegate (string Message) { GD.Print(Message); }; ;
    }
        
    public void On_Button_Pressed()
    {
        string FilePath;
        using (System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog())
        {
            openFileDialog.InitialDirectory = "c:\\";
            openFileDialog.Filter = "tproj files (*.tproj)|*.tproj|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                FilePath = openFileDialog.FileName;                
            }
        }
        CustomConsole.Log("TEst");
        //ProjectInstance.StartProjectServer()
    }
    
}