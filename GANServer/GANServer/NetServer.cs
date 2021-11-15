using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;
using System.Threading;
using System.Runtime.InteropServices;

namespace GANServer
{
  class NetServer
  {
    Socket listenSocket = null;
    IPEndPoint endPoint;
    int port;
    ConcurrentDictionary<int,Session> Sessions;
    bool isStart = false;
    int uniqueID = 0;

    public delegate void OnReceived(int sessionID, Packet packet);
    public event OnReceived Received;

    public NetServer(int port)
    {
      this.port = port;
      endPoint = new IPEndPoint(IPAddress.Any, port);
      Sessions = new ConcurrentDictionary<int, Session>();

    }

    ~NetServer()
    {
      Stop();
    }

    /// <summary>
    /// 서버를 시작합니다. 리슨 소켓을 생성하고 리슨합니다.
    /// </summary>
    public void Start()
    {
      if (isStart) return;
      isStart = true;
      
      listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
      listenSocket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
      listenSocket.Bind(endPoint);
      listenSocket.Listen(ushort.MaxValue);
      Logger.Enqueue("[System] Listening...");

      SocketAsyncEventArgs args = new SocketAsyncEventArgs();
      args.Completed += new EventHandler<SocketAsyncEventArgs>(AcceptCompleted);
      StartAccept(null);
    }

    /// <summary>
    /// 서버를 중지합니다. 모든 세션을 종료합니다.
    /// </summary>
    public void Stop()
    {
      if (isStart == false) return;
      isStart = false;
      DisconnectAll();
      listenSocket.Close();
      listenSocket = null;
    }

    /// <summary>
    /// 클라이언트의 연결을 종료합니다.
    /// </summary>
    /// <param name="sessionID">세션 ID 입니다.</param>
    public void Disconnect(int sessionID)
    {
      Session session;
      if(Sessions.TryRemove(sessionID, out session))
      {
        if (session.Socket.Connected)
        {
          try
          {
            session.Socket.Shutdown(SocketShutdown.Both);
          }
          finally
          {
            Logger.Enqueue($"[System] Disconnect SeesionID : {session.ID} [{session.IPAddress}]  Number of Users : {Sessions.Count}");
            session.Socket.Close();
            session.Socket.Dispose();
            session.Socket = null;
          }
        }
      }
    }

    /// <summary>
    /// 클라이언트의 연결을 종료합니다.
    /// </summary>
    /// <param name="session">세션 입니다.</param>
    public void Disconnect(Session session)
    {
      Session remome;
      Sessions.TryRemove(session.ID, out remome);

      if (session.Socket.Connected)
      {
        try
        {
          session.Socket.Shutdown(SocketShutdown.Both);
        }
        finally
        {
          Logger.Enqueue($"[System] Disconnect SeesionID : {session.ID} [{session.IPAddress}]  Number of Users : {Sessions.Count}");
          session.Socket.Close();
          session.Socket.Dispose();
          session.Socket = null;
        }
        
      }
    }

    /// <summary>
    /// 모든 클라이언트의 연결을 종료합니다.
    /// </summary>
    public void DisconnectAll()
    {
      foreach(var session in Sessions)
      {
        Disconnect(session.Value);
      }
      
    }

    public void SendUnicast(int sessionID, Packet packet)
    {
      Session session;
      if(Sessions.TryGetValue(sessionID,out session))
      {
        packet.SetHeader();
        session.SendBuffer.Enqueue(packet);
        ProcessSend(sessionID);
      }
    }

    public void SendBroadcast(Packet packet)
    {
      packet.SetHeader();
      foreach (var item in Sessions)
      {
        item.Value.SendBuffer.Enqueue(packet);
        ProcessSend(item.Key);
      }
    }

    private void StartAccept(SocketAsyncEventArgs e)
    {
      if (e == null)
      {
        e = new SocketAsyncEventArgs();
        e.Completed += new EventHandler<SocketAsyncEventArgs>(AcceptCompleted);
      }
      else
      {
        e.AcceptSocket = null;
      }
      
      bool willRaiseEvent = listenSocket.AcceptAsync(e);
      if (!willRaiseEvent)
      {
        ProcessAccept(e);
      }
    }

    private void AcceptCompleted(object sender, SocketAsyncEventArgs e)
    {
      ProcessAccept(e);
    }

    private void ProcessAccept(SocketAsyncEventArgs e)
    {
      if (e.SocketError == SocketError.Success)
      {
        Session session = new Session();
        session.Socket = e.AcceptSocket;
        while(true)
        {
          int id = Interlocked.Increment(ref uniqueID);
          if(Sessions.TryAdd(id, session))
          {
            session.ID = id;
            break;
          }
        }

        IPEndPoint endpoint = session.Socket.RemoteEndPoint as IPEndPoint;
        session.IPAddress = endpoint.Address.ToString();

        SocketAsyncEventArgs args = new SocketAsyncEventArgs();
        args.Completed += new EventHandler<SocketAsyncEventArgs>(IOCompleted);
        args.UserToken = session;
        args.SetBuffer(session.RecvBuffer.Buffer, session.RecvBuffer.Rear, session.RecvBuffer.WritableLength);
        bool pending = e.AcceptSocket.ReceiveAsync(args);
        if(!pending)
        {
          ReceiveCompleted(e);
        }

        Logger.Enqueue($"[System] Accept SeesionID : {session.ID} [{session.IPAddress}] Number of Users : {Sessions.Count}");
      }

      StartAccept(e);
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
      Session session = (Session)e.UserToken;
      if (e.SocketError == SocketError.Success && e.BytesTransferred > 0)
      {
        session.RecvBuffer.MoveRear(e.BytesTransferred);

        NetHeader header;
        header.Code = 0;
        header.Length = 0;
        int headerSize = Marshal.SizeOf(header);
        int size;
        
        while(true)
        {
          size = session.RecvBuffer.Length;
          if (size < headerSize) break;
          session.RecvBuffer.Peek<NetHeader>(ref header);
          if(header.Code != Packet.CODE)
          {
            Logger.Enqueue($"[Error] 패킷의 코드가 일치하지 않습니다. Code : {header.Code}");
            Disconnect(session.ID);
            break;
          }
          if (size < headerSize + header.Length) break;

          Packet packet = new Packet(header.Length);
          session.RecvBuffer.MoveFront(headerSize);
          session.RecvBuffer.Read(ref packet.Buffer, packet.Rear, header.Length);
          packet.MoveRear(header.Length);

          Received.Invoke(session.ID, packet);
        }

        if(session.RecvBuffer.WritableLength == 0)
        {
          session.RecvBuffer.Resize(session.RecvBuffer.BufferSize * 2);
        }
        e.SetBuffer(session.RecvBuffer.Buffer, session.RecvBuffer.Rear, session.RecvBuffer.WritableLength);
        bool pending = session.Socket.ReceiveAsync(e);
        if (!pending)
        {
          ReceiveCompleted(e);
        }
      }
      else
      {
        Disconnect(session);
      }
    }

    private void SendCompleted(SocketAsyncEventArgs e)
    {
      Session session = (Session)e.UserToken;
      if (e.SocketError == SocketError.Success)
      {
        while (session.SendBuffer.Count > 0)
        {
          Packet packet;
          if (session.SendBuffer.TryDequeue(out packet))
          {
            e.SetBuffer(packet.Buffer, packet.Front, packet.Length);
            bool pending = session.Socket.SendAsync(e);
            if (!pending)
            {
              SendCompleted(e);
            }
          }
          else
          {
            break;
          }
        }
        
      }
      else
      {
        Disconnect(session);
      }
    }

    private void ProcessSend(int sessionID)
    {
      Session session;
      if(Sessions.TryGetValue(sessionID,out session))
      {
        Packet packet;
        if(session.SendBuffer.TryDequeue(out packet))
        {
          SocketAsyncEventArgs args = new SocketAsyncEventArgs();
          args.Completed += IOCompleted;
          args.UserToken = session;
          args.SetBuffer(packet.Buffer, packet.Front, packet.Length);
          bool pending = session.Socket.SendAsync(args);
          if (!pending)
          {
            SendCompleted(args);
          }
        }
      }
    }

    

  }
}
