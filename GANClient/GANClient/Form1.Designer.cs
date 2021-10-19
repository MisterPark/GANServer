
namespace GANClient
{
  partial class Form1
  {
    /// <summary>
    /// 필수 디자이너 변수입니다.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// 사용 중인 모든 리소스를 정리합니다.
    /// </summary>
    /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form 디자이너에서 생성한 코드

    /// <summary>
    /// 디자이너 지원에 필요한 메서드입니다. 
    /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
    /// </summary>
    private void InitializeComponent()
    {
      this.listBox1 = new System.Windows.Forms.ListBox();
      this.button1 = new System.Windows.Forms.Button();
      this.textBox1 = new System.Windows.Forms.TextBox();
      this.button2 = new System.Windows.Forms.Button();
      this.button3 = new System.Windows.Forms.Button();
      this.pictureBox1 = new System.Windows.Forms.PictureBox();
      this.panel1 = new System.Windows.Forms.Panel();
      this.panel2 = new System.Windows.Forms.Panel();
      this.pictureBox2 = new System.Windows.Forms.PictureBox();
      this.label1 = new System.Windows.Forms.Label();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.radioButton1 = new System.Windows.Forms.RadioButton();
      this.radioButton2 = new System.Windows.Forms.RadioButton();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
      this.panel1.SuspendLayout();
      this.panel2.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // listBox1
      // 
      this.listBox1.FormattingEnabled = true;
      this.listBox1.ItemHeight = 12;
      this.listBox1.Location = new System.Drawing.Point(12, 314);
      this.listBox1.Name = "listBox1";
      this.listBox1.Size = new System.Drawing.Size(806, 124);
      this.listBox1.TabIndex = 0;
      // 
      // button1
      // 
      this.button1.Location = new System.Drawing.Point(596, 285);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(114, 23);
      this.button1.TabIndex = 1;
      this.button1.Text = "텍스트 전송";
      this.button1.UseVisualStyleBackColor = true;
      this.button1.Click += new System.EventHandler(this.button1_Click);
      // 
      // textBox1
      // 
      this.textBox1.Location = new System.Drawing.Point(12, 285);
      this.textBox1.Name = "textBox1";
      this.textBox1.Size = new System.Drawing.Size(578, 21);
      this.textBox1.TabIndex = 2;
      this.textBox1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox1_KeyDown);
      // 
      // button2
      // 
      this.button2.Location = new System.Drawing.Point(596, 256);
      this.button2.Name = "button2";
      this.button2.Size = new System.Drawing.Size(114, 23);
      this.button2.TabIndex = 3;
      this.button2.Text = "이미지 변환";
      this.button2.UseVisualStyleBackColor = true;
      this.button2.Click += new System.EventHandler(this.button2_Click);
      // 
      // button3
      // 
      this.button3.Location = new System.Drawing.Point(596, 227);
      this.button3.Name = "button3";
      this.button3.Size = new System.Drawing.Size(114, 23);
      this.button3.TabIndex = 4;
      this.button3.Text = "이미지 불러오기";
      this.button3.UseVisualStyleBackColor = true;
      this.button3.Click += new System.EventHandler(this.button3_Click);
      // 
      // pictureBox1
      // 
      this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.pictureBox1.Location = new System.Drawing.Point(0, 0);
      this.pictureBox1.Name = "pictureBox1";
      this.pictureBox1.Size = new System.Drawing.Size(261, 256);
      this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
      this.pictureBox1.TabIndex = 5;
      this.pictureBox1.TabStop = false;
      this.pictureBox1.DragDrop += new System.Windows.Forms.DragEventHandler(this.pictureBox1_DragDrop);
      this.pictureBox1.DragEnter += new System.Windows.Forms.DragEventHandler(this.pictureBox1_DragEnter);
      // 
      // panel1
      // 
      this.panel1.BackColor = System.Drawing.Color.White;
      this.panel1.Controls.Add(this.pictureBox1);
      this.panel1.Location = new System.Drawing.Point(12, 12);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(261, 256);
      this.panel1.TabIndex = 6;
      // 
      // panel2
      // 
      this.panel2.BackColor = System.Drawing.Color.White;
      this.panel2.Controls.Add(this.pictureBox2);
      this.panel2.Location = new System.Drawing.Point(329, 12);
      this.panel2.Name = "panel2";
      this.panel2.Size = new System.Drawing.Size(261, 256);
      this.panel2.TabIndex = 7;
      // 
      // pictureBox2
      // 
      this.pictureBox2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.pictureBox2.Location = new System.Drawing.Point(0, 0);
      this.pictureBox2.Name = "pictureBox2";
      this.pictureBox2.Size = new System.Drawing.Size(261, 256);
      this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
      this.pictureBox2.TabIndex = 5;
      this.pictureBox2.TabStop = false;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(292, 146);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(17, 12);
      this.label1.TabIndex = 8;
      this.label1.Text = "▶";
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.radioButton2);
      this.groupBox1.Controls.Add(this.radioButton1);
      this.groupBox1.Location = new System.Drawing.Point(596, 117);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(222, 104);
      this.groupBox1.TabIndex = 10;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "GAN 타입";
      // 
      // radioButton1
      // 
      this.radioButton1.AutoSize = true;
      this.radioButton1.Checked = true;
      this.radioButton1.Location = new System.Drawing.Point(25, 41);
      this.radioButton1.Name = "radioButton1";
      this.radioButton1.Size = new System.Drawing.Size(135, 16);
      this.radioButton1.TabIndex = 0;
      this.radioButton1.TabStop = true;
      this.radioButton1.Text = "CartoonGAN-Hayao";
      this.radioButton1.UseVisualStyleBackColor = true;
      // 
      // radioButton2
      // 
      this.radioButton2.AutoSize = true;
      this.radioButton2.Location = new System.Drawing.Point(25, 63);
      this.radioButton2.Name = "radioButton2";
      this.radioButton2.Size = new System.Drawing.Size(84, 16);
      this.radioButton2.TabIndex = 1;
      this.radioButton2.Text = "Cartoonize";
      this.radioButton2.UseVisualStyleBackColor = true;
      // 
      // Form1
      // 
      this.AllowDrop = true;
      this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(830, 450);
      this.Controls.Add(this.groupBox1);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.panel2);
      this.Controls.Add(this.panel1);
      this.Controls.Add(this.button3);
      this.Controls.Add(this.button2);
      this.Controls.Add(this.textBox1);
      this.Controls.Add(this.button1);
      this.Controls.Add(this.listBox1);
      this.Name = "Form1";
      this.Text = "GAN Client";
      this.DragDrop += new System.Windows.Forms.DragEventHandler(this.Form1_DragDrop);
      this.DragEnter += new System.Windows.Forms.DragEventHandler(this.Form1_DragEnter);
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
      this.panel1.ResumeLayout(false);
      this.panel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.ListBox listBox1;
    private System.Windows.Forms.Button button1;
    private System.Windows.Forms.TextBox textBox1;
    private System.Windows.Forms.Button button2;
    private System.Windows.Forms.Button button3;
    private System.Windows.Forms.PictureBox pictureBox1;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Panel panel2;
    private System.Windows.Forms.PictureBox pictureBox2;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.RadioButton radioButton2;
    private System.Windows.Forms.RadioButton radioButton1;
  }
}

