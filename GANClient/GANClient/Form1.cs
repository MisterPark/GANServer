using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GANClient
{
  public partial class Form1 : Form
  {
    Timer logTimer;
    NetClient client;

    public Form1()
    {
      InitializeComponent();

      logTimer = new Timer();
      logTimer.Interval = 100;
      logTimer.Tick += ProcessLog;
      logTimer.Start();

      client = new NetClient("192.168.0.7",14536);
      client.Connect();
      
    }

    private void ProcessLog(object sender, EventArgs e)
    {
      List<string> logs = Logger.Dequeue();
      foreach (var item in logs)
      {
        listBox1.Items.Add(item);
      }

    }

    private void button1_Click(object sender, EventArgs e)
    {
      
    }
  }
}
