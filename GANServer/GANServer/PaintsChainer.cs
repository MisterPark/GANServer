using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GANServer
{
  class PaintsChainer : GAN
  {
    public PaintsChainer(int port) : base(port)
    {
    }

    public PaintsChainer(string ip, int port) : base(ip, port)
    {
    }

    public override void Start()
    {
      string appPath = Application.StartupPath + "\\PaintsChainer\\PaintsChainerServer.bat";
      Process.Start(appPath);

      base.Start();
    }

    public override void Close()
    {
      Packet packet = new Packet();
      packet.Write((int)MsgType.Shutdown);
      SendMessage(packet);
      
      base.Close();
    }
  }
}
