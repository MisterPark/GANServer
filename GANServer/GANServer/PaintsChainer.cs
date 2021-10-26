using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GANServer
{
  class PaintsChainer : IGAN
  {
    private Process process;
    public Process Process => process;

    public void Close()
    {
      if (process == null) return;
      process.Kill();
    }

    public void Start()
    {
      string appPath = Application.StartupPath + "\\PaintsChainer\\PaintsChainerServer.bat";
      process = Process.Start(appPath);
    }
  }
}
