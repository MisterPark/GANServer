using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GANServer
{
  class Cartoonize : IGAN
  {
    private Process process;
    public Process Process => process;

    public void Start()
    {
      string appPath = Application.StartupPath + "\\Cartoonization\\test_code\\cartoonizeServer.bat";
      process = Process.Start(appPath);
    }

    public void Close()
    {
      if (process == null) return;
      process.Kill();
    }
  }
}
