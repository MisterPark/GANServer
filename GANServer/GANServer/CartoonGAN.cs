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
    public CartoonGANType GANType { get; set; }

    public void Test()
    {
      string ganExe = string.Empty;
      switch (GANType)
      {
        case CartoonGANType.Hosoda:
          ganExe = "\\test_Hosoda.bat";
          break;
        case CartoonGANType.Hayao:
          ganExe = "\\test_Hayao.bat";
          break;
        case CartoonGANType.Paprika:
          ganExe = "\\test_Paprika.bat";
          break;
        case CartoonGANType.Shinkai:
          ganExe = "\\test_Shinkai.bat";
          break;
        default:
          ganExe = "\\test_Hosoda.bat";
          break;
      }

      string appPath = Application.StartupPath + ganExe;
      Process.Start(appPath);

    }
  }
}
