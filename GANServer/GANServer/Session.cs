﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace GANServer
{
  class Session
  {
    public Socket Socket { get; set; } = null;
    public int ID { get; set; }
    public byte[] recvBytes = new byte[1024];
    public StringBuilder RecvBuffer { get; private set; } = new StringBuilder();
    public StringBuilder SendBuffer { get; private set; } = new StringBuilder();
  }
}
