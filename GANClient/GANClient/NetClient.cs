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

    public NetClient(string serverIP, int port)
    {
      socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
      endpoint = new IPEndPoint(IPAddress.Parse(serverIP), port);
    }
  }
}
