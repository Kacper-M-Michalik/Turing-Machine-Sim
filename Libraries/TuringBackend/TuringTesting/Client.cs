using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using TuringBackend.Logging;

namespace TuringBackend.Networking
{
    public static class Client
    {
        public static bool IsConnected 
        { 
            get
            {
                return TCP.ConnectionSocket != null ? TCP.ConnectionSocket.Connected : false;
            }
        }
        public static TCPInterface TCP { get; private set; } = new TCPInterface();
        static int DataBufferSize = 4096;

        static IPAddress TargetIP;
        static int TargetPort;
        static int Timeout = 1000;   

        public class TCPInterface
        {
            public TcpClient ConnectionSocket;
            private NetworkStream DataStream;
            private byte[] ReceiveDataBuffer;
            private Packet PacketCurrentlyBeingRebuilt;

            public void Connect()
            {
                try
                {
                    ConnectionSocket = new TcpClient();
                    ConnectionSocket.ReceiveBufferSize = DataBufferSize;
                    ConnectionSocket.SendBufferSize = DataBufferSize;

                    ReceiveDataBuffer = new byte[DataBufferSize];

                    ConnectionSocket.BeginConnect(TargetIP, TargetPort, OnConnectCallBack, ConnectionSocket);
                    Thread.Sleep(Timeout);                  
                    
                    if (!IsConnected)
                    {
                        CustomConsole.Log("CLIENT: Connection timed out.");
                        TCPInternalDisconnect();
                        UIEventBindings.ClientFailedConnecting?.Invoke(this, new EventArgs());
                    }                                    

                }
                catch (Exception E)
                {
                    CustomConsole.Log("CLIENT: Connection attempt failure! " + E.ToString());
                    TCPInternalDisconnect();
                }
            }

            public void OnConnectCallBack(IAsyncResult Result)            
            {
                try
                {
                    if (ConnectionSocket == null) return;

                    CustomConsole.Log("CLIENT: Connect callback called.");

                    ConnectionSocket.EndConnect(Result);
                    DataStream = ConnectionSocket.GetStream();
                    PacketCurrentlyBeingRebuilt = new Packet();

                    UIEventBindings.ClientSuccessConnecting?.Invoke(this, new EventArgs());

                    DataStream.BeginRead(ReceiveDataBuffer, 0, DataBufferSize, OnReceiveDataFromServer, null);
                }
                catch (Exception E)
                {
                    CustomConsole.Log(E.ToString());
                    TCPInternalDisconnect();
                }
            }

            public void SendDataToServer(Packet Data)
            {
                try
                {
                    CustomConsole.Log("CLIENT: Client is writing data to server");
                    DataStream.BeginWrite(Data.SaveTemporaryBufferToPernamentReadBuffer(), 0, Data.Length(), null, null);
                }
                catch (Exception E)
                {
                    CustomConsole.Log("CLIENT: Error Sending Data To Server! " + E.ToString());
                }
            }

            private void OnReceiveDataFromServer(IAsyncResult Result)
            {
                try
                {
                    if (ConnectionSocket == null) return;

                    int IncomingDataLength = DataStream.EndRead(Result);

                    if (IncomingDataLength == 0)
                    {
                        CustomConsole.Log("CLIENT: SERVER DISCONNECTED ME");
                        TCPInternalDisconnect();
                        return;
                    }

                    //ReceiveBuffer is always 4096 or whatever , we need to only pack data that is useful into our rebuilt packet -> as such we only copy IncomingDataLength of recievebuffer.
                    byte[] UsefuldataBuffer = new byte[IncomingDataLength];
                    Array.Copy(ReceiveDataBuffer, UsefuldataBuffer, IncomingDataLength);

                    PacketCurrentlyBeingRebuilt.Write(UsefuldataBuffer, false);
                    PacketCurrentlyBeingRebuilt.SaveTemporaryBufferToPernamentReadBuffer();

                    if (PacketCurrentlyBeingRebuilt.UnreadLength() >= 4)
                    {
                        int PacketLength = PacketCurrentlyBeingRebuilt.ReadInt(false);

                        while (PacketCurrentlyBeingRebuilt.UnreadLength() >= PacketLength && PacketCurrentlyBeingRebuilt.UnreadLength() >= 4)
                        {
                            Packet ProcessedPacket = new Packet(PacketCurrentlyBeingRebuilt.ReadBytes(PacketLength));
                            AddPacketToProcessOnMainThread(ProcessedPacket);

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

                    if (ConnectionSocket != null) DataStream.BeginRead(ReceiveDataBuffer, 0, DataBufferSize, OnReceiveDataFromServer, null);
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

        private static Queue<Packet> PacketProcessingQueue = new Queue<Packet>();
        private static Queue<Packet> PacketsBeingProcessed;
        
        public static void AddPacketToProcessOnMainThread(Packet PacketToAdd)
        {
            lock (PacketProcessingQueue)
            {
                PacketProcessingQueue.Enqueue(PacketToAdd);
            }
        }

        public static void ProcessPackets()
        {     
            if (PacketProcessingQueue.Count > 0)
            {
                lock (PacketProcessingQueue)
                {
                    PacketsBeingProcessed = new Queue<Packet>(PacketProcessingQueue);
                    PacketProcessingQueue.Clear();
                }

                int Length = PacketsBeingProcessed.Count;
                for (int i = 0; i < Length; i++)
                {
                    Packet Data = PacketsBeingProcessed.Dequeue();

                    //Get rid of packet size
                    Data.ReadInt();
                    //Get Type
                    int PacketType = Data.ReadInt();
                    //Execute function
                    ClientReceiveFunctions.PacketToFunction[PacketType](Data);
                    Data.Dispose();
                }

            }
            
        }

        public static void ConnectToServer(IPAddress IP, int Port)
        {
            TargetIP = IP;
            TargetPort = Port;
            Thread ConnectThread = new Thread(TCP.Connect);
            ConnectThread.Start();
        }

        public static void Disconnect()
        {
            CustomConsole.Log("CLIENT: DISCONNECTING FROM SERVER!");
            TCP.TCPInternalDisconnect();
        }
    }
}
