using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace GANClient
{
  class NetClient
  {
    Socket socket = null;
    IPEndPoint endpoint = null;
    bool tryConnect = false;
    NetBuffer recvBuffer;

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
      tryConnect = true;
      SocketAsyncEventArgs args = new SocketAsyncEventArgs();
      args.Completed += ConnectCompleted;
      args.RemoteEndPoint = endpoint;
      socket.ConnectAsync(args);
    }

    public void Disconnect()
    {
      if(socket.Connected)
      {
        socket.Disconnect(true);
        tryConnect = false;
      }
    }

    public void Send(Packet packet)
    {

    }

    private void ConnectCompleted(object sender, SocketAsyncEventArgs e)
    {
      if(e.SocketError == SocketError.Success)
      {
        IPEndPoint endpoint = e.RemoteEndPoint as IPEndPoint;
        Logger.Enqueue($"[System] Connected [{endpoint.Address.ToString()}]");

        SocketAsyncEventArgs args = new SocketAsyncEventArgs();
        args.Completed += new EventHandler<SocketAsyncEventArgs>(IOCompleted);
        args.UserToken = socket;
        args.SetBuffer(recvBuffer.Buffer, recvBuffer.Offset, recvBuffer.WritableLength);
        bool pending = e.ConnectSocket.ReceiveAsync(args);
      }
      else
      {
        tryConnect = false;
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

    }

    private void SendCompleted(SocketAsyncEventArgs e)
    {

    }

  }
}
