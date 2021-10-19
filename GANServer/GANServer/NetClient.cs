using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.InteropServices;

namespace GANServer
{
  class NetClient
  {
    Socket socket = null;
    IPEndPoint endpoint = null;
    bool tryConnect = false;
    NetBuffer recvBuffer;

    public delegate void OnReceived(Packet packet);
    public event OnReceived Received;

    public NetClient(string serverIP, int port)
    {
      socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
      endpoint = new IPEndPoint(IPAddress.Parse(serverIP), port);
      recvBuffer = new NetBuffer();
    }

    ~NetClient()
    {
      Disconnect();
    }

    public void Connect()
    {
      if (socket.Connected) return;
      if (tryConnect) return;
      socket.Connect(endpoint);
      Logger.Enqueue($"[System] Connected [{endpoint.Address}]");

      SocketAsyncEventArgs args = new SocketAsyncEventArgs();
      args.Completed += new EventHandler<SocketAsyncEventArgs>(IOCompleted);
      args.UserToken = socket;
      args.SetBuffer(recvBuffer.Buffer, recvBuffer.Rear, recvBuffer.WritableLength);
      bool pending = socket.ReceiveAsync(args);
    }

    public void ConnectAsync(bool again = false)
    {
      if (socket.Connected) return;
      if (tryConnect) return;
      tryConnect = true;
      SocketAsyncEventArgs args = new SocketAsyncEventArgs();
      if(again)
      {
        args.Completed += ConnectCompleted_Repeat;
      }
      else
      {
        args.Completed += ConnectCompleted;
      }
      args.RemoteEndPoint = endpoint;
      socket.ConnectAsync(args);
    }

    public void Disconnect()
    {
      Logger.Enqueue($"[System] Disconnect. [{endpoint.Address}:{endpoint.Port}]");
      if (socket.Connected)
      {
        socket.Disconnect(true);
        tryConnect = false;
      }
    }

    public void Send(Packet packet)
    {
      packet.SetHeader();
      SocketAsyncEventArgs args = new SocketAsyncEventArgs();
      args.Completed += IOCompleted;
      args.SetBuffer(packet.Buffer, packet.Front, packet.Length);
      bool pending = socket.SendAsync(args);
      if(!pending)
      {
        SendCompleted(args);
      }
    }


    private void ConnectCompleted(object sender, SocketAsyncEventArgs e)
    {
      if(e.SocketError == SocketError.Success)
      {
        IPEndPoint endpoint = e.RemoteEndPoint as IPEndPoint;
        Logger.Enqueue($"[System] Connected [{endpoint.Address}:{endpoint.Port}]");

        SocketAsyncEventArgs args = new SocketAsyncEventArgs();
        args.Completed += new EventHandler<SocketAsyncEventArgs>(IOCompleted);
        args.UserToken = socket;
        args.SetBuffer(recvBuffer.Buffer, recvBuffer.Rear, recvBuffer.WritableLength);
        bool pending = e.ConnectSocket.ReceiveAsync(args);
      }
      else
      {
        tryConnect = false;
        Logger.Enqueue($"[System] Connect Failed!!");
      }
    }

    private void ConnectCompleted_Repeat(object sender, SocketAsyncEventArgs e)
    {
      if (e.SocketError == SocketError.Success)
      {
        IPEndPoint endpoint = e.RemoteEndPoint as IPEndPoint;
        Logger.Enqueue($"[System] Connected [{endpoint.Address}:{endpoint.Port}]");

        SocketAsyncEventArgs args = new SocketAsyncEventArgs();
        args.Completed += new EventHandler<SocketAsyncEventArgs>(IOCompleted);
        args.UserToken = socket;
        args.SetBuffer(recvBuffer.Buffer, recvBuffer.Rear, recvBuffer.WritableLength);
        bool pending = e.ConnectSocket.ReceiveAsync(args);
      }
      else
      {
        tryConnect = false;
        Logger.Enqueue($"[System] Connect to {endpoint.Address}:{endpoint.Port} again...");
        Thread.Sleep(1000);
        ConnectAsync(true);
      }
    }

    private void IOCompleted(object sender, SocketAsyncEventArgs e)
    {
      switch (e.LastOperation)
      {
        case SocketAsyncOperation.Receive:
          ReceiveCompleted(e);
          break;
        case SocketAsyncOperation.Send:
          SendCompleted(e);
          break;
        default:
          throw new ArgumentException("The last operation completed on the socket was not a receive or send");
      }

    }

    private void ReceiveCompleted(SocketAsyncEventArgs e)
    {
      if (e.SocketError == SocketError.Success && e.BytesTransferred > 0)
      {
        recvBuffer.MoveRear(e.BytesTransferred);

        NetHeader header;
        header.Code = 0;
        header.Length = 0;
        int headerSize = Marshal.SizeOf(header);
        int size;

        while (true)
        {
          size = recvBuffer.Length;
          if (size < headerSize) break;
          recvBuffer.Peek<NetHeader>(ref header);
          if (header.Code != Packet.CODE)
          {
            Logger.Enqueue($"[Error] 패킷의 코드가 일치하지 않습니다. Code : {header.Code}");
            break;
          }
          if (size < headerSize + header.Length) break;

          Packet packet = new Packet(header.Length);
          recvBuffer.MoveFront(headerSize);
          recvBuffer.Read(ref packet.Buffer, packet.Rear, header.Length);
          packet.MoveRear(header.Length);

          Received.Invoke(packet);
        }

        if (recvBuffer.WritableLength == 0)
        {
          recvBuffer.Resize(recvBuffer.BufferSize * 2);
        }
        e.SetBuffer(recvBuffer.Buffer, recvBuffer.Rear, recvBuffer.WritableLength);
        bool pending = socket.ReceiveAsync(e);
        if (!pending)
        {
          ReceiveCompleted(e);
        }
      }
      else
      {
        //Logger.Enqueue($"[Error] Receicve Failed!");
        Disconnect();
      }
    }

    private void SendCompleted(SocketAsyncEventArgs e)
    {
      if(e.SocketError != SocketError.Success)
      {
        Logger.Enqueue($"[Error] Send Failed!");
        Disconnect();
      }
    }

  }
}
