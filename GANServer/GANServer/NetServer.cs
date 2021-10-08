using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace GANServer
{
  class NetServer
  {
    Socket listenSocket;
    IPEndPoint endPoint;
    int port;
    List<Session> Clients;

    public NetServer(int port)
    {
      Clients = new List<Session>();
      this.port = port;
      listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
      endPoint = new IPEndPoint(IPAddress.Any, port);
      listenSocket.Bind(endPoint);
    }

    ~NetServer()
    {
      
    }

    public void Start()
    {
      listenSocket.Listen(ushort.MaxValue);

      SocketAsyncEventArgs args = new SocketAsyncEventArgs();
      args.Completed += new EventHandler<SocketAsyncEventArgs>(AcceptProc);
      listenSocket.AcceptAsync(args);
    }

    public void DisconnectAll()
    {
      foreach(Session client in Clients)
      {
        if(client.Socket.Connected)
        {
          client.Socket.Shutdown(SocketShutdown.Both);
        }
      }
      Clients.Clear();
    }

    private void AcceptProc(object sender, SocketAsyncEventArgs e)
    {
      if(e.SocketError == SocketError.Success)
      {
        Session session = new Session();
        session.Socket = e.AcceptSocket;
        Clients.Add(session);
      }

      SocketAsyncEventArgs args = new SocketAsyncEventArgs();
      args.Completed += new EventHandler<SocketAsyncEventArgs>(AcceptProc);
      listenSocket.AcceptAsync(args);
    }

    private void ReceiveProc(object sender, SocketAsyncEventArgs e)
    {

    }

  }
}
