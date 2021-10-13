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
      client.Received += ProcessPacket;
      client.Connect();
      
    }

    private void ProcessLog(object sender, EventArgs e)
    {
      List<string> logs = Logger.Dequeue();
      foreach (var item in logs)
      {
        listBox1.Items.Add(item);
        listBox1.SelectedIndex = listBox1.Items.Count - 1;
      }

    }

    private void ProcessPacket(Packet packet)
    {
      int type = (int)MsgType.None;
      packet.Read(ref type);
      MsgType msgType = (MsgType)type;
      switch (msgType)
      {
        case MsgType.Chat:
          ReceiveChat(packet);
          break;
        case MsgType.Image:
          ReceiveImage(packet);
          break;
        default:
          Logger.Enqueue($"[System] 알 수 없는 메세지 타입 : {msgType}");
          break;
      }

    }

    private void SendChat()
    {
      string text = textBox1.Text;
      if (text == string.Empty) return;
      textBox1.Text = string.Empty;
      Packet packet = new Packet();
      packet.Write((int)MsgType.Chat);
      packet.Write(text);
      client.Send(packet);

    }

    private void SendImage()
    {
      if (pictureBox1.Image == null)
      {
        Logger.Enqueue("[System] 이미지를 불러와 주세요.");
        return;
      }
      button2.Enabled = false;
      Packet packet = new Packet();
      packet.Write((int)MsgType.Image);
      packet.Write(pictureBox1.ImageLocation);
      packet.Write(pictureBox1.Image);
      client.Send(packet);
    }

    private void ReceiveChat(Packet packet)
    {
      int sessionID = 0;
      string text = string.Empty;
      packet.Read(ref sessionID);
      packet.Read(ref text);
      Logger.Enqueue($"{sessionID} : {text}");
    }

    private void ReceiveImage(Packet packet)
    {
      System.Drawing.Image img = null;
      if(packet.Read(ref img))
      {
        this.Invoke(new Action(delegate ()
        {
          button2.Enabled = true;
          pictureBox2.Image = img;
        }));
        Logger.Enqueue("[System] 이미지 변환 성공");
      }
    }

    private void button1_Click(object sender, EventArgs e)
    {
      SendChat();
    }

    private void textBox1_KeyDown(object sender, KeyEventArgs e)
    {
      if(e.KeyCode == Keys.Enter)
      {
        SendChat();
      }
    }

    private void button3_Click(object sender, EventArgs e)
    {
      // 이미지 불러오기
      OpenFileDialog dia = new OpenFileDialog();
      dia.Multiselect = false;
      dia.Filter = "jpg files|*.jpg";

      if (dia.ShowDialog() == DialogResult.OK)
      {
        this.pictureBox1.ImageLocation = dia.FileName;
      }
    }

    private void button2_Click(object sender, EventArgs e)
    {
      // 이미지 변환
      SendImage();
    }
  }
}
