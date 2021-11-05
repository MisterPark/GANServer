using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.IO;

namespace GANServer
{
  public partial class Form1 : Form
  {
    System.Windows.Forms.Timer logTimer;
    NetServer server;
    int transaction = 0;
    CartoonGAN cartoonGAN;
    Cartoonize cartoonize;
    Artline artline;
    PaintsChainer paintsChainer;

    string currentDirectory = string.Empty;

    public Form1()
    {
      InitializeComponent();
      currentDirectory = Directory.GetCurrentDirectory();

      logTimer = new System.Windows.Forms.Timer();
      logTimer.Interval = 100;
      logTimer.Tick += ProcessLog;
      logTimer.Start();

      cartoonGAN = new CartoonGAN(9999);
      cartoonGAN.GANType = CartoonGANType.Hayao;
      cartoonGAN.Received += ProcessPacket;
      cartoonGAN.Start();

      cartoonize = new Cartoonize(8888);
      cartoonize.Received += ProcessPacket;
      cartoonize.Start();

      artline = new Artline(6666);
      artline.Received += ProcessPacket;
      artline.Start();

      paintsChainer = new PaintsChainer(11111);
      paintsChainer.Received += ProcessPacket;
      paintsChainer.Start();

      server = new NetServer(14536);
      server.Received += ProcessPacket;
      server.Start();

      
    }

    private void Form1_FormClosing(object sender, FormClosingEventArgs e)
    {
      cartoonGAN.Close();
      cartoonize.Close();
      artline.Close();
      paintsChainer.Close();
    }

    private void ProcessLog(object sender, EventArgs e)
    {
      List<string> logs = Logger.Dequeue();
      string folder = DateTime.Now.ToString("yyMMdd");
      string file = DateTime.Now.ToString("HH");
      if (Directory.Exists($"{currentDirectory}\\Log\\{folder}") == false)
      {
        Directory.CreateDirectory($"{currentDirectory}\\Log\\{folder}");
      }
      // TODO : 여기서 메모리 릭남
      using (FileStream stream = File.Open($"{currentDirectory}\\Log\\{folder}\\{file}.txt", FileMode.Append))
      {
        using (StreamWriter writer = new StreamWriter(stream))
        {
          foreach (var item in logs)
          {
            listBox1.Items.Add(item);
            writer.WriteLine(item);
          }
          listBox1.SelectedIndex = listBox1.Items.Count - 1;
        }
      }
        
      
    }

    private void ProcessPacket(int sessionID, Packet packet) // Client To Server
    {
      int type = (int)MsgType.None;
      packet.Read(ref type);
      MsgType msgType = (MsgType)type;
      switch (msgType)
      {
        case MsgType.Chat:
          RequestChat(sessionID, packet);
          break;
        case MsgType.Image:
          RequestConvertImage(sessionID, packet);
          break;
        case MsgType.CartoonGAN:
          RequestConvertCartoonGAN(sessionID, packet);
          break;
        case MsgType.Cartoonize:
          RequestConvertCartoonize(sessionID, packet);
          break;
        case MsgType.Artline:
          RequestConvertArtLine(sessionID, packet);
          break;
        case MsgType.PaintsChainer:
          RequestConvertPaintsChainer(sessionID, packet);
          break;
        default:
          Logger.Enqueue($"[System] 알 수 없는 메세지 타입 : {msgType} / SessionID : {sessionID}");
          server.Disconnect(sessionID);
          break;
      }

      
    }

    private void ProcessPacket(Packet packet) // GAN to Server
    {
      int type = (int)MsgType.None;
      packet.Read(ref type);
      MsgType msgType = (MsgType)type;
      switch (msgType)
      {
        case MsgType.CartoonGAN:
          ResponseConvertCartoonGAN(packet);
          break;
        case MsgType.Cartoonize:
          ResponseConvertCartoonize(packet);
          break;
        case MsgType.Artline:
          ResponseConvertArtLine(packet);
          break;
        case MsgType.PaintsChainer:
          ResponseConvertPaintsChainer(packet);
          break;
        default:
          Logger.Enqueue($"[System] 알 수 없는 메세지 타입 : {msgType} / GAN Server로부터 받은 메세지");
          break;
      }
      
    }
    private void ResponseConvertCartoonGAN(Packet packet)
    {
      // CartoonGAN 에서 응답
      int sessionID = -1;
      int transactionId = 0;
      Image image = null;
      packet.Read(ref sessionID);
      packet.Read(ref transactionId);
      packet.Read(ref image);

      Packet pack = new Packet();
      pack.Write((int)MsgType.CartoonGAN);
      pack.Write(transactionId);
      pack.Write(image);
      server.SendUnicast(sessionID, pack);
    }

    private void ResponseConvertCartoonize(Packet packet)
    {
      // Cartoonize 에서 응답
      int sessionID = -1;
      int transactionId = 0;
      Image image = null;
      packet.Read(ref sessionID);
      packet.Read(ref transactionId);
      packet.Read(ref image);

      Packet pack = new Packet();
      pack.Write((int)MsgType.Cartoonize);
      pack.Write(transactionId);
      pack.Write(image);
      server.SendUnicast(sessionID, pack);
    }

    private void ResponseConvertArtLine(Packet packet)
    {
      // Cartoonize 에서 응답
      int sessionID = -1;
      int transactionId = 0;
      Image image = null;
      packet.Read(ref sessionID);
      packet.Read(ref transactionId);
      packet.Read(ref image);

      Packet pack = new Packet();
      pack.Write((int)MsgType.Artline);
      pack.Write(transactionId);
      pack.Write(image);
      server.SendUnicast(sessionID, pack);
    }

    private void ResponseConvertPaintsChainer(Packet packet)
    {
      // PaintsChainer 에서 응답
      int sessionID = -1;
      int transactionId = 0;
      Image image = null;
      packet.Read(ref sessionID);
      packet.Read(ref transactionId);
      packet.Read(ref image);

      Packet pack = new Packet();
      pack.Write((int)MsgType.PaintsChainer);
      pack.Write(transactionId);
      pack.Write(image);
      server.SendUnicast(sessionID, pack);
    }

    private void RequestChat(int sessionID, Packet packet)
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

    private void RequestConvertImage(int sessionID, Packet packet)
    {
      string imgLocation = string.Empty;
      System.Drawing.Image img = null;
      packet.Read(ref imgLocation);
      if (packet.Read(ref img))
      {
        Logger.Enqueue($"[System] 이미지 변환 요청 {imgLocation}");
        // 에코
        //Packet pack = new Packet();
        //pack.Write((int)MsgType.Image);
        //pack.Write(img);
        //server.SendUnicast(sessionID, pack);

        // CartoonGAN
        int transactionId = Interlocked.Increment(ref transaction);
        
        Packet pack = new Packet();
        pack.Write((int)MsgType.CartoonGAN);
        pack.Write(sessionID);
        pack.Write(transactionId);
        pack.Write(img);
        cartoonGAN.SendMessage(pack);
      }
    }

    private void RequestConvertCartoonGAN(int sessionID, Packet packet)
    {
      string imgLocation = string.Empty;
      System.Drawing.Image img = null;
      packet.Read(ref imgLocation);
      if (packet.Read(ref img))
      {
        Logger.Enqueue($"[System] 이미지 CartoonGAN 변환 요청 {imgLocation}");
        int transactionId = Interlocked.Increment(ref transaction);

        Packet pack = new Packet();
        pack.Write((int)MsgType.CartoonGAN);
        pack.Write(sessionID);
        pack.Write(transactionId);
        pack.Write(img);
        cartoonGAN.SendMessage(pack);
      }
    }

    private void RequestConvertCartoonize(int sessionID, Packet packet)
    {
      string imgLocation = string.Empty;
      System.Drawing.Image img = null;
      packet.Read(ref imgLocation);
      if (packet.Read(ref img))
      {
        Logger.Enqueue($"[System] 이미지 Cartoonize 변환 요청 {imgLocation}");
        int transactionId = Interlocked.Increment(ref transaction);

        Packet pack = new Packet();
        pack.Write((int)MsgType.Cartoonize);
        pack.Write(sessionID);
        pack.Write(transactionId);
        pack.Write(img);
        cartoonize.SendMessage(pack);
      }
    }

    private void RequestConvertArtLine(int sessionID, Packet packet)
    {
      string imgLocation = string.Empty;
      System.Drawing.Image img = null;
      packet.Read(ref imgLocation);
      if (packet.Read(ref img))
      {
        Logger.Enqueue($"[System] 이미지 Cartoonize 변환 요청 {imgLocation}");
        int transactionId = Interlocked.Increment(ref transaction);

        Packet pack = new Packet();
        pack.Write((int)MsgType.Artline);
        pack.Write(sessionID);
        pack.Write(transactionId);
        pack.Write(img);
        artline.SendMessage(pack);
      }
    }

    private void RequestConvertPaintsChainer(int sessionID, Packet packet)
    {
      System.Drawing.Image img = null;
      System.Drawing.Image img_ref = null;
      packet.Read(ref img);
      packet.Read(ref img_ref);

      Logger.Enqueue($"[System] 이미지 PaintsChainer 변환 요청 SessionID :{sessionID}");
      int transactionId = Interlocked.Increment(ref transaction);

      Packet pack = new Packet();
      pack.Write((int)MsgType.PaintsChainer);
      pack.Write(sessionID);
      pack.Write(transactionId);
      pack.Write(img);
      pack.Write(img_ref);
      paintsChainer.SendMessage(pack);
      Logger.Enqueue($"[System] 이미지 PaintsChainer 변환 요청 Transaction :{transactionId}");
    }

    
  }
}
