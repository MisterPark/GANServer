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

  class CartoonGAN : IGAN
  {
    private Process process;
    public Process Process => process;
    public CartoonGANType GANType { get; set; }

    public void Start()
    {
      string appPath = Application.StartupPath + "\\CartoonGAN.bat";
      process = Process.Start(appPath);
    }

    public void Close()
    {
      if (process == null) return;
      process.Kill();
    }
  }
}
