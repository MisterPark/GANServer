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
    struct Line
    {
      public Color color;
      public int width;
      public Point start;
      public Point end;
    }
    Timer logTimer;
    NetClient client;
    string imgPath = Application.StartupPath + "\\images";
    Bitmap canvas = null;
    bool isMouseDown = false;
    List<Line> lines = new List<Line>();
    Point startPoint;
    Color currentColor = Color.Black;

    public Form1()
    {
      InitializeComponent();

      CreateFolder();


      pictureBox4.BackColor = Color.Transparent;
      pictureBox4.Parent = pictureBox3;
      pictureBox4.Location = new Point(0, 0);

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
        case MsgType.PaintsChainer:
          ReceivePaintsChainer(packet);
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

    private void ReceivePaintsChainer(Packet packet)
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
          pictureBox5.Image = img;
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

    private void Form1_DragDrop(object sender, DragEventArgs e)
    {
      try
      {
        string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
        if(tabControl1.SelectedIndex == 0)
        {
          pictureBox1.ImageLocation = files[0];
        }
        else
        {
          pictureBox3.Image = new Bitmap(files[0]);
          int w = pictureBox3.Image.Width;
          int h = pictureBox3.Image.Height;
          lines.Clear();
          Bitmap bitmap = new Bitmap(w, h);
          canvas = bitmap;
          pictureBox4.Image = bitmap;


        }
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

    private void pictureBox4_MouseDown(object sender, MouseEventArgs e)
    {
      isMouseDown = true;
      double wRatio = (double)pictureBox4.Image.Width / pictureBox4.Width;
      double hRatio = (double)pictureBox4.Image.Height / pictureBox4.Height;
      startPoint = new Point((int)(e.X * wRatio), (int)(e.Y * hRatio));
    }

    private void pictureBox4_MouseMove(object sender, MouseEventArgs e)
    {
      if (isMouseDown == false) return;
      if (pictureBox4.Image == null) return;

      double wRatio = (double)pictureBox4.Image.Width / pictureBox4.Width;
      double hRatio = (double)pictureBox4.Image.Height / pictureBox4.Height;

      Line line = new Line();
      line.width = (int)numericUpDown1.Value;
      line.color = currentColor;
      line.start = startPoint;
      line.end = new Point((int)(e.X * wRatio), (int)(e.Y * hRatio));
      lines.Add(line);
      startPoint = line.end;
      pictureBox4.Refresh();
    }

    private void pictureBox4_MouseUp(object sender, MouseEventArgs e)
    {
      isMouseDown = false;
    }

    private void pictureBox4_MouseLeave(object sender, EventArgs e)
    {
      isMouseDown = false;
    }

    private void pictureBox4_Paint(object sender, PaintEventArgs e)
    {
      if (canvas == null) return;
      if (pictureBox4.Image == null) return;
      
      

      //Bitmap bitmap = (Bitmap)pictureBox4.Image;
      using(Graphics g = Graphics.FromImage(canvas))
      {
        foreach (var line in lines)
        {
          Pen pen = new Pen(line.color);
          pen.Width = line.width;
          g.DrawLine(pen, line.start, line.end);
          pen.Dispose();
        }
      }
    }

    private void tabPage2_SizeChanged(object sender, EventArgs e)
    {
      int h = tabPage2.Size.Height;
      pictureBox3.Size = new Size(h - 10, h - 10);
      pictureBox4.Size = new Size(h - 10, h - 10);
      pictureBox5.Size = new Size(h - 10, h - 10);

      pictureBox5.Location = new Point(h, 5);
    }

    private void button4_Click(object sender, EventArgs e)
    {
      if (pictureBox3.Image == null) return;
      if (pictureBox4.Image == null) return;

      Packet packet = new Packet();
      packet.Write((int)MsgType.PaintsChainer);
      packet.Write(pictureBox3.Image);
      packet.Write(pictureBox4.Image);
      client.Send(packet);
    }

    private void button5_Click(object sender, EventArgs e)
    {
      if(colorDialog1.ShowDialog() == DialogResult.OK)
      {
        currentColor = colorDialog1.Color;
        panel5.BackColor = colorDialog1.Color;
      }
    }

    private void button6_Click(object sender, EventArgs e)
    {
      // 이미지 불러오기
      OpenFileDialog dia = new OpenFileDialog();
      dia.Multiselect = false;
      dia.Filter = "jpg files|*.jpg";

      if (dia.ShowDialog() == DialogResult.OK)
      {
        pictureBox3.Image = new Bitmap(dia.FileName);
        int w = pictureBox3.Image.Width;
        int h = pictureBox3.Image.Height;
        lines.Clear();
        Bitmap bitmap = new Bitmap(w, h);
        canvas = bitmap;
        pictureBox4.Image = bitmap;
      }
    }
  }
}
