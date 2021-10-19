using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GANServer
{
  class Artline : IGAN
  {
    private Process process;
    public Process Process => process;

    public void Start()
    {
      string appPath = Application.StartupPath + "\\Artline\\ArtLineServer.bat";
      process = Process.Start(appPath);
    }

    public void Close()
    {
      if (process == null) return;
      process.Kill();
    }
  }
}
