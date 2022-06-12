using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using TuringBackend.Logging;

namespace TuringBackend.Networking
{
    public static class Server
    {
        public static bool IsServerOn;

        public static int MaxClients { get; private set; }
        public static int Port { get; private set; }
        public static long CacheExpiryTime { get; private set; } = TimeSpan.FromMinutes(5).Ticks;

        public static Thread ServerThread { get; private set; }
        static TcpListener ServerTcpListener;
        public static Dictionary<int, ServerClientSlot> Clients { get; private set; }

        static Queue<Packet> PacketProcessingQueue;
        static Queue<Packet> PacketsBeingProcessed;
        static bool MarkForClosing;
        static long LastTick;


        public static void StartServer(int SetMaxPlayers, int SetPort)
        {
            if (ProjectInstance.LoadedProject == null) throw new Exception("Cannot start server with unloaded project");

            MaxClients = SetMaxPlayers;
            Port = SetPort;

            ServerThread = new Thread(RunServer);
            ServerThread.Start();
        }

        public static void AddPacketToProcessOnServerThread(int SenderID, Packet PacketToAdd)
        {
            lock (PacketProcessingQueue)
            {
                PacketToAdd.InsertPacketSenderIDUnsafe(SenderID);
                PacketProcessingQueue.Enqueue(PacketToAdd);
            }
        }

        public static void CloseServer()
        {
            MarkForClosing = true;
        }

        private static void RunServer()
        {
            CustomConsole.Log("THREAD NOTIF SERVER: SERVER INIT ON THREAD " + Thread.CurrentThread.ManagedThreadId.ToString());

            IsServerOn = true;
            MarkForClosing = false;
            LastTick = DateTime.UtcNow.Ticks;

            Clients = new Dictionary<int, ServerClientSlot>();
            for (int i = 0; i < MaxClients; i++)
            {
                Clients.Add(i, new ServerClientSlot(i));
            }

            ServerTcpListener = new TcpListener(IPAddress.Any, Port);
            ServerTcpListener.Start();

            CustomConsole.Log("SERVER: Server Started on port: " + Port.ToString());

            ServerTcpListener.BeginAcceptTcpClient(new AsyncCallback(NewTCPClientConnectedCallback), null);

            PacketProcessingQueue = new Queue<Packet>();

            while (!MarkForClosing)
            {
                if (PacketProcessingQueue.Count > 0)
                {
                    //Copy queued packets
                    lock (PacketProcessingQueue)
                    {
                        PacketsBeingProcessed = new Queue<Packet>(PacketProcessingQueue);
                        PacketProcessingQueue.Clear();
                    }

                    int Length = PacketsBeingProcessed.Count;
                    for (int i = 0; i < Length; i++)
                    {
                        Packet Data = PacketsBeingProcessed.Dequeue();

                        //Packet Length has been replaced with sender ID
                        int SenderID = Data.ReadInt();
                        //Get Type
                        int PacketType = Data.ReadInt();
                        //Execute function

                        if (Enum.IsDefined(typeof(ClientSendPackets), PacketType))
                        {
                            ServerReceiveFunctions.PacketToFunction[PacketType](SenderID, Data);
                        }
                        else
                        {
                            CustomConsole.Log("SERVER: Invalid Packet recieved");
                        }
                        Data.Dispose();
                    }
                }

                long CurrentTime = DateTime.UtcNow.Ticks;
                foreach (KeyValuePair<int, CacheFileData> CachedFile in ProjectInstance.LoadedProject.CacheDataLookup)
                {
                    CachedFile.Value.ExpiryTimer += CurrentTime - LastTick;

                    if (CachedFile.Value.ExpiryTimer > CacheExpiryTime)
                    {
                        ProjectInstance.LoadedProject.CacheDataLookup.Remove(CachedFile.Key);
                    }
                }
                LastTick = CurrentTime;              
            }

            ShutDown();
        }

        private static void NewTCPClientConnectedCallback(IAsyncResult Result)
        {
            if (!IsServerOn) return;

            CustomConsole.Log("SERVER: Server dealing with new connection on thread " + Thread.CurrentThread.ManagedThreadId.ToString());

            TcpClient NewClient = ServerTcpListener.EndAcceptTcpClient(Result);

            ServerTcpListener.BeginAcceptTcpClient(new AsyncCallback(NewTCPClientConnectedCallback), null);

            for (int i = 0; i < MaxClients; i++)
            {
                if (Clients[i].TCP.ConnectionSocket == null)
                {
                    Clients[i].TCP.ConnectClientToServer(NewClient);
                    return;
                }
            }

            CustomConsole.Log("SERVER: " + NewClient.Client.RemoteEndPoint.ToString() + " failed to connect: Server Full!");

            NewClient.Close();
        }

        private static void ShutDown()
        {
            IsServerOn = false;
            ServerTcpListener.Stop();
            ServerTcpListener = null;
            Clients = null;
            PacketProcessingQueue = null;
            PacketsBeingProcessed = null;

            CustomConsole.Log("SERVER: Server Closed");
        }
    }

    public class ServerClientSlot
    {
        private int ClientId;
        public TCPInterface TCP;

        public static int DefaultDataBufferSize = 4096;

        public ServerClientSlot(int SetClientID)
        {
            ClientId = SetClientID;
            TCP = new TCPInterface(ClientId);
        }

        public class TCPInterface
        {
            public TcpClient ConnectionSocket;
            NetworkStream DataStream;
            readonly int ID;

            byte[] ReceiveDataBuffer;
            int DataBufferSize = DefaultDataBufferSize;

            Packet PacketCurrentlyBeingRebuilt;

            public TCPInterface(int SetID)
            {
                ID = SetID;
            }

            public void ConnectClientToServer(TcpClient SetConnectionSocket)
            {
                ConnectionSocket = SetConnectionSocket;
                ConnectionSocket.ReceiveBufferSize = DataBufferSize;
                ConnectionSocket.SendBufferSize = DataBufferSize;

                DataStream = ConnectionSocket.GetStream();
                ReceiveDataBuffer = new byte[DataBufferSize];
                PacketCurrentlyBeingRebuilt = new Packet();

                DataStream.BeginRead(ReceiveDataBuffer, 0, DataBufferSize, OnReceiveDataFromClient, null);

                CustomConsole.Log("SERVER: Client at " + ConnectionSocket.Client.RemoteEndPoint.ToString() + " has been connected to server!");
            }

            public void SendDataToClient(Packet Data)
            {
                try
                {
                    DataStream.BeginWrite(Data.SaveTemporaryBufferToPernamentReadBuffer(), 0, Data.Length(), null, null);
                }
                catch (Exception E)
                {
                    CustomConsole.Log("SERVER: Error Sending Data To Client " + ID.ToString() + ":  " + E.ToString());
                }
            }

            private void OnReceiveDataFromClient(IAsyncResult Result)
            {
                try
                {
                    if (ConnectionSocket == null) return;

                    CustomConsole.Log("THREAD NOTIF SERVER: Server dealing with incoming data on thread " + Thread.CurrentThread.ManagedThreadId.ToString());
                    int IncomingDataLength = DataStream.EndRead(Result);

                    if (IncomingDataLength == 0)
                    {
                        TCPInternalDisconnect();
                        CustomConsole.Log("SERVER: Client " + ID.ToString() + " has disconnected!");
                        return;
                    }

                    byte[] UsefuldataBuffer = new byte[IncomingDataLength];
                    Array.Copy(ReceiveDataBuffer, UsefuldataBuffer, IncomingDataLength);

                    CustomConsole.Log("SERVER: Server is receiving data from client " + ID.ToString() + "!");

                    PacketCurrentlyBeingRebuilt.Write(UsefuldataBuffer, false);
                    PacketCurrentlyBeingRebuilt.SaveTemporaryBufferToPernamentReadBuffer();

                    if (PacketCurrentlyBeingRebuilt.UnreadLength() >= 4)
                    {
                        int PacketLength = PacketCurrentlyBeingRebuilt.ReadInt(false);

                        while (PacketCurrentlyBeingRebuilt.UnreadLength() >= PacketLength && PacketCurrentlyBeingRebuilt.UnreadLength() >= 4)
                        {
                            Packet ProcessedPacket = new Packet(PacketCurrentlyBeingRebuilt.ReadBytes(PacketLength));
                            Server.AddPacketToProcessOnServerThread(ID, ProcessedPacket);

                            if (PacketCurrentlyBeingRebuilt.UnreadLength() >= 4)
                            {
                                PacketLength = PacketCurrentlyBeingRebuilt.ReadInt(false);
                            }

                        }

                        if (PacketCurrentlyBeingRebuilt.UnreadLength() == 0)
                        {
                            PacketCurrentlyBeingRebuilt.Reset();
                        }

                    }

                    if (ConnectionSocket != null) DataStream.BeginRead(ReceiveDataBuffer, 0, DataBufferSize, OnReceiveDataFromClient, null);
                }
                catch (Exception E)
                {
                    CustomConsole.Log(E.ToString());
                }
            }

            public void TCPInternalDisconnect()
            {
                ConnectionSocket?.Close();
                ConnectionSocket = null;
                DataStream = null;
                ReceiveDataBuffer = null;
                PacketCurrentlyBeingRebuilt = null;
            }

        }

        public void DisconnectClientFromServer()
        {
            CustomConsole.Log("SERVER: " + TCP.ConnectionSocket.Client.RemoteEndPoint.ToString() + " aka. Client N:" + ClientId.ToString() + " has been disconnected from the server!");

            TCP.TCPInternalDisconnect();
        }
    }
}
