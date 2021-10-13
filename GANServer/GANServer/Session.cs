using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;

namespace GANServer
{
  class Session
  {
    public Socket Socket { get; set; } = null;
    public int ID { get; set; }
    public NetBuffer RecvBuffer { get; set; } = new NetBuffer();
    public ConcurrentQueue<Packet> SendBuffer { get; private set; } = new ConcurrentQueue<Packet>();
  }
}
