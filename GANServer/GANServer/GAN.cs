using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public abstract class GAN 
{
  NetClient client;
  public string IPAddress { get; }
  public int Port { get; }

  public delegate void OnReceived(Packet packet);
  public event OnReceived Received;

  public GAN(int port)
  {
    IPAddress = "127.0.0.1";
    Port = port;
    client = new NetClient(IPAddress, Port);
    client.Received += ProcessPacket;
  }

  public GAN(string ip, int port)
  {
    IPAddress = ip;
    Port = port;
    client = new NetClient(IPAddress, Port);
    client.Received += ProcessPacket;
  }

  public virtual void Start()
  {
    client.ConnectAsync(true);
  }

  public virtual void Close()
  {
    client.Disconnect();
  }

  public void SendMessage(Packet packet)
  {
    client.Send(packet);
  }

  private void ProcessPacket(Packet packet)
  {
    Received?.Invoke(packet);
  }
}