using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace GANServer
{
  public enum CartoonGANType
  {
    Hosoda,
    Hayao,
    Paprika,
    Shinkai
  }

  class CartoonGAN : GAN
  {
    public CartoonGANType GANType { get; set; }


    public CartoonGAN(int port) : base(port)
    {
    }

    public CartoonGAN(string ip, int port) : base(ip, port)
    {
    }

    public override void Start()
    {
      string appPath = Application.StartupPath + "\\CartoonGAN\\CartoonGAN.bat";
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
