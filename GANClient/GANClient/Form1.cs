using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
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
    string imgPath = Application.StartupPath + "\\images";

    public Form1()
    {
      InitializeComponent();

      CreateFolder();

      logTimer = new Timer();
      logTimer.Interval = 100;
      logTimer.Tick += ProcessLog;
      logTimer.Start();

      client = new NetClient("192.168.0.7",14536);
      client.Received += ProcessPacket;
      client.Connect();
      
    }

    private void CreateFolder()
    {
      DirectoryInfo directoryInfo = new DirectoryInfo(imgPath);
      if(directoryInfo.Exists == false)
      {
        directoryInfo.Create();
      }
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
        case MsgType.CartoonGAN:
          ReceiveCartoonGAN(packet);
          break;
        case MsgType.Cartoonize:
          ReceiveCartoonize(packet);
          break;
        case MsgType.ArtLine:
          ReceiveArtline(packet);
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

    private void SendImage(MsgType type = MsgType.Image)
    {
      if (pictureBox1.Image == null)
      {
        Logger.Enqueue("[System] 이미지를 불러와 주세요.");
        return;
      }
      button2.Enabled = false;
      Packet packet = new Packet();
      packet.Write((int)type);
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
      int transactionID = 0;
      System.Drawing.Image img = null;
      packet.Read(ref transactionID);
      if(packet.Read(ref img))
      {
        CreateFolder();
        img.Save(imgPath + $"\\{transactionID}.png", System.Drawing.Imaging.ImageFormat.Png);
        this.Invoke(new Action(delegate ()
        {
          button2.Enabled = true;
          pictureBox2.Image = img;
        }));
        Logger.Enqueue($"[System] 이미지 변환 성공 transaction : {transactionID}");
      }
    }

    private void ReceiveCartoonGAN(Packet packet)
    {
      int transactionID = 0;
      System.Drawing.Image img = null;
      packet.Read(ref transactionID);
      if (packet.Read(ref img))
      {
        CreateFolder();
        img.Save(imgPath + $"\\{transactionID}.png", System.Drawing.Imaging.ImageFormat.Png);
        this.Invoke(new Action(delegate ()
        {
          button2.Enabled = true;
          pictureBox2.Image = img;
        }));
        Logger.Enqueue($"[System] CartoonGAN 이미지 변환 성공 transaction : {transactionID}");
      }
    }

    private void ReceiveCartoonize(Packet packet)
    {
      int transactionID = 0;
      System.Drawing.Image img = null;
      packet.Read(ref transactionID);
      if (packet.Read(ref img))
      {
        CreateFolder();
        img.Save(imgPath + $"\\{transactionID}.png", System.Drawing.Imaging.ImageFormat.Png);
        this.Invoke(new Action(delegate ()
        {
          button2.Enabled = true;
          pictureBox2.Image = img;
        }));
        Logger.Enqueue($"[System] Cartoonize 이미지 변환 성공 transaction : {transactionID}");
      }
    }

    private void ReceiveArtline(Packet packet)
    {
      int transactionID = 0;
      System.Drawing.Image img = null;
      packet.Read(ref transactionID);
      if (packet.Read(ref img))
      {
        CreateFolder();
        img.Save(imgPath + $"\\{transactionID}.png", System.Drawing.Imaging.ImageFormat.Png);
        this.Invoke(new Action(delegate ()
        {
          button2.Enabled = true;
          pictureBox2.Image = img;
        }));
        Logger.Enqueue($"[System] Cartoonize 이미지 변환 성공 transaction : {transactionID}");
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
      MsgType type = MsgType.Image;
      if(radioButton1.Checked)
      {
        type = MsgType.CartoonGAN;
      }
      else if(radioButton2.Checked)
      {
        type = MsgType.Cartoonize;
      }
      else if(radioButton3.Checked)
      {
        type = MsgType.ArtLine;
      }
      // 이미지 변환
      SendImage(type);
    }

    private void pictureBox1_DragEnter(object sender, DragEventArgs e)
    {
      e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
    }

    private void pictureBox1_DragDrop(object sender, DragEventArgs e)
    {
      try
      {
        string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
        pictureBox1.ImageLocation = files[0];
      }
      catch(System.Exception ex)
      {
        MessageBox.Show(ex.Message);
      }
      
    }

    private void Form1_DragDrop(object sender, DragEventArgs e)
    {
      try
      {
        string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
        pictureBox1.ImageLocation = files[0];
      }
      catch (System.Exception ex)
      {
        MessageBox.Show(ex.Message);
      }
    }

    private void Form1_DragEnter(object sender, DragEventArgs e)
    {
      e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
    }
  }
}
