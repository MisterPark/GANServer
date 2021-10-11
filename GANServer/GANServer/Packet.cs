using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace GANServer
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
    byte[] buffer;
    int bufferSize;
    int front;
    int rear;
    public int Length { get { return rear - front; } }
    public int WritableLength { get { return bufferSize - rear; } }

    public Packet()
    {
      this.bufferSize = DEFAULT_SIZE;
      buffer = new byte[this.bufferSize];
      front = 0;
      rear = 0;
    }

    public Packet(int bufferSize)
    {
      this.bufferSize = bufferSize;
      buffer = new byte[this.bufferSize];
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

      byte[] temp = buffer;
      buffer = new byte[totalSize];
      front = 0;
      rear = 0;
      Write(header.Code);
      Write(header.Length);

      Buffer.BlockCopy(temp, originOffset, buffer, front, originLength);

    }

    public void Write(int value)
    {
      byte[] binary = BitConverter.GetBytes(value);
      if(WritableLength < binary.Length)
      {
        byte[] temp = new byte[bufferSize + binary.Length];
        Buffer.BlockCopy(buffer, front, temp, 0, Length);
        rear = Length;
        front = 0;
      }
      Buffer.BlockCopy(binary, 0, buffer, rear, binary.Length);
      rear += binary.Length;
    }

    public bool Read(ref int value)
    {
      int bytesLen = Marshal.SizeOf(value);
      if (this.Length < bytesLen)
      {
        return false;
      }

      value = BitConverter.ToInt32(buffer, front);
      front += bytesLen;
      return true;
    }

  }
}
