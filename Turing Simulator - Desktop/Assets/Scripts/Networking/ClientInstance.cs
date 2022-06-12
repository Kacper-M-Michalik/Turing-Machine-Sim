using Godot;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using TuringBackend;
using TuringBackend.Logging;
using TuringFrontend.Networking;

public class ClientInstance : Node
{ 
    public static void ConnectToServer(string Address, int Port)
    {
        if (IPAddress.TryParse(Address, out IPAddress IP))
        {
            Client.ConnectToServer(IP, Port);
        }
        else
        {
            CustomConsole.Log("Invalid IP Address");
        }
    }

    public static void ConnectToLocalServer(int Port)
    {
        ConnectToServer("127.0.0.1", Port);
    }

    public static void Disconnect()
    {
        Client.Disconnect();
    }
}