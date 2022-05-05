using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TuringBackend.Networking
{
    public enum ClientSendPackets
    {
        CreateFile = 1
    }

    public enum ServerSendPackets
    {
        ErrorNotification = 1
    }

    public class Packet : IDisposable
    {
        byte[] ReadBuffer;
        public List<byte> TemporaryWriteBuffer;
        public int ReadPointerPosition;

        public Packet()
        {
            TemporaryWriteBuffer = new List<byte>();
            ReadPointerPosition = 0;
        }

        public Packet(byte[] Data)
        {
            TemporaryWriteBuffer = new List<byte>();
            ReadPointerPosition = 0;
            ReadBuffer = Data;
        }


        #region Writes

        public void Write(byte[] Data)
        {
            TemporaryWriteBuffer.AddRange(Data);
        }

        public void Write(float Data)
        {
            TemporaryWriteBuffer.AddRange(BitConverter.GetBytes(Data));
        }

        public void Write(int Data)
        {
            TemporaryWriteBuffer.AddRange(BitConverter.GetBytes(Data));
        }

        public void Write(short Data)
        {
            TemporaryWriteBuffer.AddRange(BitConverter.GetBytes(Data));
        }

        public void Write(bool Data)
        {
            TemporaryWriteBuffer.AddRange(BitConverter.GetBytes(Data));
        }

        public void Write(string Data)
        {
            //Reading string will be reading an int to get length, then looping through length.
            Write(Data.Length);
            TemporaryWriteBuffer.AddRange(Encoding.ASCII.GetBytes(Data));
        }

        #endregion

        #region Reads

        public byte[] ReadBytes(int Length, bool MovePointer = true)
        {
            if (ReadPointerPosition + Length > ReadBuffer.Length) throw new Exception("ReadBytes Length out of bounds!");
            byte[] Result = new byte[Length];
            Array.Copy(ReadBuffer, Result, Length);
            if (MovePointer) ReadPointerPosition += Length;
            return Result;
        }

        public int ReadInt(bool MovePointer = true)
        {
            if (ReadPointerPosition + 4 > ReadBuffer.Length) throw new Exception("ReadInt Length out of bounds!");
            int Result = BitConverter.ToInt32(ReadBuffer, ReadPointerPosition);
            if (MovePointer) ReadPointerPosition += 4;
            return Result;
        }

        public short ReadShort(bool MovePointer = true)
        {
            if (ReadPointerPosition + 2 > ReadBuffer.Length) throw new Exception("ReadShort Length out of bounds!");
            short Result = BitConverter.ToInt16(ReadBuffer, ReadPointerPosition);
            if (MovePointer) ReadPointerPosition += 2;
            return Result;
        }

        public bool ReadBool(bool MovePointer = true)
        {
            if (ReadPointerPosition + 1 > ReadBuffer.Length) throw new Exception("ReadBool Length out of bounds!");
            bool Result = BitConverter.ToBoolean(ReadBuffer, ReadPointerPosition);
            if (MovePointer) ReadPointerPosition += 1;
            return Result;
        }

        public string ReadString(bool MovePointer = true)
        {
            int Length = ReadInt();
            if (ReadPointerPosition + Length > ReadBuffer.Length) throw new Exception("ReadString Length out of bounds!");
            string Result = Encoding.ASCII.GetString(ReadBuffer, ReadPointerPosition, Length);
            if (MovePointer) ReadPointerPosition += Length;
            return Result;
        }

        #endregion

        #region Helper Functions

        public int Length()
        {
            return ReadBuffer.Length;
        }

        public int UnreadLength()
        {
            return ReadBuffer.Length - ReadPointerPosition;
        }

        public void InsertPacketLength()
        {
            TemporaryWriteBuffer.InsertRange(0, BitConverter.GetBytes(TemporaryWriteBuffer.Count + 4));
        }

        public void InsertPacketSenderIDUnsafe(int SenderID)
        {
            byte[] IDToBytes = BitConverter.GetBytes(SenderID);
            ReadBuffer[0] = IDToBytes[0];
            ReadBuffer[1] = IDToBytes[1];
            ReadBuffer[2] = IDToBytes[2];
            ReadBuffer[3] = IDToBytes[3];
        }

        public byte[] SaveTemporaryBufferToPernamentReadBuffer()
        {
            ReadBuffer = TemporaryWriteBuffer.ToArray();
            return ReadBuffer;
        }

        public void Reset()
        {
            ReadBuffer = null;
            TemporaryWriteBuffer = new List<byte>();
            ReadPointerPosition = 0;
        }
        #endregion

        #region Disposing

        private bool disposed = false;

        protected virtual void Dispose(bool _disposing)
        {
            if (!disposed)
            {
                if (_disposing)
                {
                    ReadBuffer = null;
                    TemporaryWriteBuffer = null;
                    ReadPointerPosition = 0;
                }

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }

}
