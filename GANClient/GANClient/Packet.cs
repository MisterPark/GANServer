using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace GANClient
{
  public struct NetHeader
  {
    public byte Code;
    public int Length;
  }

  class Packet
  {
    public const int DEFAULT_SIZE = 1024;
    public const byte CODE = 0x77;
    public byte[] Buffer;
    int bufferSize;
    int front;
    int rear;
    public int Offset { get { return rear; } }
    public int Length { get { return rear - front; } }
    public int WritableLength { get { return bufferSize - rear; } }

    public Packet()
    {
      this.bufferSize = DEFAULT_SIZE;
      Buffer = new byte[this.bufferSize];
      front = 0;
      rear = 0;
    }

    public Packet(int bufferSize)
    {
      this.bufferSize = bufferSize;
      Buffer = new byte[this.bufferSize];
      front = 0;
      rear = 0;
    }

    public void SetHeader()
    {
      NetHeader header;
      header.Code = CODE;
      header.Length = Length;

      // HACK : 야매코드 (Header가 바뀌면 바뀌어야하는 코드
      int originOffset = front;
      int originLength = Length;
      int totalSize = Marshal.SizeOf(header) + Length;

      byte[] temp = Buffer;
      Buffer = new byte[totalSize];
      front = 0;
      rear = 0;
      Write(header.Code);
      Write(header.Length);

      System.Buffer.BlockCopy(temp, originOffset, Buffer, front, originLength);

    }

    public void Write(int value)
    {
      byte[] binary = BitConverter.GetBytes(value);
      if(WritableLength < binary.Length)
      {
        byte[] temp = new byte[bufferSize + binary.Length];
        System.Buffer.BlockCopy(Buffer, front, temp, 0, Length);
        rear = Length;
        front = 0;
      }
      System.Buffer.BlockCopy(binary, 0, Buffer, rear, binary.Length);
      rear += binary.Length;
    }

    public bool Read(ref int value)
    {
      int bytesLen = Marshal.SizeOf(value);
      if (this.Length < bytesLen)
      {
        return false;
      }

      value = BitConverter.ToInt32(Buffer, front);
      front += bytesLen;
      return true;
    }

    public void MoveFront(int bytes)
    {
      front += bytes;
    }

    public void MoveRear(int bytes)
    {
      rear += bytes;
    }
  }
}
