using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GANServer
{
  interface IGAN
  {
    Process Process { get; }
    void Start();
    void Close();
  }
}
