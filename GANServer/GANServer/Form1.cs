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
    NetServer server;

    public Form1()
    {
      InitializeComponent();

      logTimer = new Timer();
      logTimer.Interval = 100;
      logTimer.Tick += ProcessLog;
      logTimer.Start();

      server = new NetServer(14536);
      server.Received += ProcessPacket;
      server.Start();

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

    private void ProcessPacket(int sessionID, Packet packet)
    {
      int type = (int)MsgType.None;
      packet.Read(ref type);
      MsgType msgType = (MsgType)type;
      switch (msgType)
      {
        case MsgType.Chat:
          ReceiveChat(sessionID, packet);
          break;
        case MsgType.Image:
          ReceiveImage(sessionID, packet);
          break;
        default:
          Logger.Enqueue($"[System] 알 수 없는 메세지 타입 : {msgType} / SessionID : {sessionID}");
          server.Disconnect(sessionID);
          break;
      }

      
    }

    private void ReceiveChat(int sessionID, Packet packet)
    {
      string text = string.Empty;
      packet.Read(ref text);
      //Logger.Enqueue($"[System] Received packet / SessionID : [{sessionID}] / Value : [{value}]");
      Logger.Enqueue($"{sessionID} : {text}");

      Packet pack = new Packet();
      pack.Write((int)MsgType.Chat);
      pack.Write(sessionID);
      pack.Write(text);
      server.SendBroadcast(pack);
    }

    private void ReceiveImage(int sessionID, Packet packet)
    {
      string imgLocation = string.Empty;
      System.Drawing.Image img = null;
      packet.Read(ref imgLocation);
      if (packet.Read(ref img))
      {
        Logger.Enqueue($"[System] 이미지 변환 요청 {imgLocation}");
        Packet pack = new Packet();
        pack.Write((int)MsgType.Image);
        pack.Write(img);
        server.SendUnicast(sessionID, pack);
      }
    }
  }
}
