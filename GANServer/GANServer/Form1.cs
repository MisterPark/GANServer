using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GANServer
{
  public partial class Form1 : Form
  {
    Timer logTimer;

    public Form1()
    {
      InitializeComponent();

      logTimer = new Timer();
      logTimer.Interval = 100;
      logTimer.Tick += ProcessLog;
      logTimer.Start();


      //CartoonGAN gan = new CartoonGAN();
      //gan.GANType = CartoonGANType.Hayao;
      //gan.Test();


    }

    private void ProcessLog(object sender, EventArgs e)
    {
      List<string> logs = Logger.Dequeue();
      foreach (var item in logs)
      {
        listBox1.Items.Add(item);
      }
      
    }
  }
}
