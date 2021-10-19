using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO;

namespace GANServer
{
  public struct NetHeader
  {
    public int Code;
    public int Length;
  }

  class Packet
  {
    public const int DEFAULT_SIZE = 1024;
    public const int CODE = 0x77;
    public byte[] Buffer;
    int bufferSize;
    int front;
    int rear;
    public int Front { get { return front; } }
    public int Rear { get { return rear; } }
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
      int totalSize = Marshal.SizeOf(header) + originLength;

      byte[] temp = Buffer;
      Buffer = new byte[totalSize];
      front = 0;
      rear = 0;
      Write(header.Code);
      Write(header.Length);
      //Logger.Enqueue(header.Length.ToString());

      System.Buffer.BlockCopy(temp, originOffset, Buffer, rear, originLength);
      rear += originLength;
    }

    public void Resize(int newSize)
    {
      if (newSize <= 0)
      {
        throw new Exception("newSize 는 0보다 작을 수 없습니다.");
      }
      int copySize = (newSize < Length) ? newSize : Length;
      byte[] newBuffer = new byte[newSize];
      System.Buffer.BlockCopy(Buffer, front, newBuffer, 0, copySize);
      Buffer = newBuffer;
      bufferSize = newSize;
      front = 0;
      rear = copySize;
    }

    public void Write(byte value)
    {
      if (WritableLength < 1)
      {
        Resize(bufferSize + 1);
      }
      Buffer[rear] = value;
      rear += 1;
    }

    public void Write(int value)
    {
      byte[] binary = BitConverter.GetBytes(value);
      if (WritableLength < binary.Length)
      {
        Resize(bufferSize + binary.Length);
      }
      System.Buffer.BlockCopy(binary, 0, Buffer, rear, binary.Length);
      rear += binary.Length;
    }

    public void Write(string value)
    {
      byte[] binary = Encoding.UTF8.GetBytes(value);
      Write(binary.Length);
      if (WritableLength < binary.Length)
      {
        Resize(bufferSize + binary.Length);
      }

      System.Buffer.BlockCopy(binary, 0, Buffer, rear, binary.Length);
      rear += binary.Length;
    }

    public void Write(System.Drawing.Image value)
    {
      MemoryStream ms = new MemoryStream();
      value.Save(ms, value.RawFormat);
      byte[] binary = ms.ToArray();
      Write(binary.Length);
      if (WritableLength < binary.Length)
      {
        Resize(bufferSize + binary.Length);
      }
      System.Buffer.BlockCopy(binary, 0, Buffer, rear, binary.Length);
      rear += binary.Length;
    }

    public bool Read(ref byte value)
    {
      if (this.Length < 1)
      {
        return false;
      }

      value = Buffer[front];
      front += 1;
      return true;
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

    public bool Read(ref string value)
    {
      int len = 0;
      if (Read(ref len) == false)
      {
        return false;
      }
      if (this.Length < len)
      {
        return false;
      }

      value = Encoding.UTF8.GetString(Buffer, front, len);
      front += len;
      return true;
    }

    public bool Read(ref System.Drawing.Image value)
    {
      int len = 0;
      if (Read(ref len) == false)
      {
        return false;
      }
      if (this.Length < len)
      {
        return false;
      }

      MemoryStream ms = new MemoryStream(Buffer, front, len);
      value = System.Drawing.Image.FromStream(ms);
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
