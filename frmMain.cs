using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace fds
{
  public class frmMain : Form
  {
    private readonly Dictionary<int, string> MfNames = new Dictionary<int, string>()
    {
      { 0, "Unknown (Unlicensed?)" },
      { 1, "Nintendo" },
      { 8, "Capcom" },
      { 10, "Jaleco" },
      { 24, "Hudson Soft" },
      { 73, "Irem" },
      { 74, "Gakken" },
      { 139, "BPS" },
      { 153, "Pack-In-Video" },
      { 155, "Tecmo" },
      { 156, "Imagineer" },
      { 162, "Scorpion Soft" },
      { 164, "Konami" },
      { 166, "Kawada" },
      { 167, "Takara" },
      { 168, "Royal Industries" },
      { 172, "Toei Animation" },
      { 175, "Namco" },
      { 177, "ASCII Corporation" },
      { 178, "Bandai" },
      { 179, "Soft Pro" },
      { 182, "HAL Laboratory" },
      { 187, "Sunsoft" },
      { 188, "Toshiba EMI" },
      { 192, "Taito" },
      { 193, "Sunsoft / Ask Koudansha" },
      { 194, "Kemco" },
      { 195, "Square" },
      { 196, "Tokuma Shoten" },
      { 197, "Data East" },
      { 198, "Tonkin House" },
      { 199, "East Cube" },
      { 202, "Konami" },
      { 203, "VAP" },
      { 204, "Use" },
      { 206, "Pony Canyon" },
      { 209, "Sofel" },
      { 210, "Bothtec" },
      { 219, "Hiro" },
      { 231, "Athena" },
      { 235, "Atlus" }
    };

    private readonly Dictionary<int, string> DiskTypes = new Dictionary<int, string>()
    {
      { 32, "Normal" },
      { 69, "Event" },
      { 82, "Reduced price" }
    };

    private FDSFile[] FDSFiles;
    private IContainer components;
    private TextBox textBox1;
    private Button btnOpen;
    private OpenFileDialog FdsDlgOpen;
    private Label label1;
    private Label label2;
    private TextBox txtGameName;
    private Label label3;
    private TextBox txtDiskType;
    private Label label4;
    private TextBox txtRev;
    private Label label5;
    private TextBox txtSide;
    private Label label6;
    private TextBox txtDiskNumber;
    private Label label7;
    private TextBox txtShutter;
    private Label label8;
    private TextBox txtBootFile;
    private Label label9;
    private TextBox txtMfDate;
    private Label label10;
    private TextBox txtRwDate;
    private Label label11;
    private TextBox txtRwSerial;
    private Label label12;
    private TextBox txtRwCount;
    private Label label13;
    private TextBox txtPrice;
    private ListView lvwFileList;
    private ColumnHeader clmNumber;
    private ColumnHeader clmID;
    private ColumnHeader clmName;
    private ColumnHeader clmAddress;
    private ColumnHeader clmSize;
    private ColumnHeader clmType;
    private Button btnQuit;
    private Button btnExtract;
    private SaveFileDialog FdsDlgSave;
    private ContextMenuStrip ctxmExtract;
    private ToolStripMenuItem extractToolStripMenuItem;
    private TextBox txtFileName;
    private ComboBox cmbSide;

    private int BCDBlackMagic(int BCDInput)
    {
      // Here be dragons
      int result;
      int.TryParse(BCDInput.ToString("x"), out result);
      return result;
    }

    private void LoadSide(string FilePath, int SelectedSide)
    {
      byte[] GameName = new byte[3];
      byte[] MfDate = new byte[3];
      byte[] RwDate = new byte[3];
      byte[] RwSerial = new byte[2];
      this.lvwFileList.Items.Clear();
      FileStream FdsImage = File.OpenRead(this.FdsDlgOpen.FileName);
      FdsImage.Position += FdsImage.Length % 65500 + SelectedSide * 65500;
      FdsImage.Position += 15;
      int MfCode = FdsImage.ReadByte();
      FdsImage.Read(GameName, 0, 3);
      int DiskType = FdsImage.ReadByte();
      int GameRevision = FdsImage.ReadByte();
      int DiskSide = FdsImage.ReadByte();
      int DiskNumber = FdsImage.ReadByte();
      int Shutter = FdsImage.ReadByte();
      ++FdsImage.Position;
      int BootFile = FdsImage.ReadByte();
      FdsImage.Position += 5;
      FdsImage.Read(MfDate, 0, 3);
      FdsImage.Position += 10;
      FdsImage.Read(RwDate, 0, 3);
      FdsImage.Position += 2;
      FdsImage.Read(RwSerial, 0, 2);
      ++FdsImage.Position;
      int RwCount = FdsImage.ReadByte();
      FdsImage.Position += 2;
      int DiskPrice = FdsImage.ReadByte();
      ++FdsImage.Position;
      FdsImage.ReadByte();

      // The file index is a single byte, so theoretical maximum is 256 files
      this.FDSFiles = new FDSFile[256];
      int index = 0;
      while (FdsImage.ReadByte() == 3)
      {
        this.FDSFiles[index] = new FDSFile();
        this.FDSFiles[index].FileNumber = FdsImage.ReadByte();
        this.FDSFiles[index].FileID = FdsImage.ReadByte();
        byte[] numArray2 = new byte[8];
        FdsImage.Read(numArray2, 0, 8);
        this.FDSFiles[index].FileName = Encoding.Default.GetString(numArray2);
        byte[] buffer4 = new byte[2];
        FdsImage.Read(buffer4, 0, 2);
        this.FDSFiles[index].FileLoadAddress = (int) BitConverter.ToUInt16(buffer4, 0);
        byte[] buffer5 = new byte[2];
        FdsImage.Read(buffer5, 0, 2);
        this.FDSFiles[index].FileSize = (int) BitConverter.ToUInt16(buffer5, 0);
        this.FDSFiles[index].FileType = FdsImage.ReadByte();
        this.FDSFiles[index].FileDataStart = FdsImage.Position + 1L;
        this.lvwFileList.Items.Add(new ListViewItem(this.FDSFiles[index].GetInfo()));
        FdsImage.Position += (long) (this.FDSFiles[index].FileSize + 1);
        ++index;
      }
      string MfName;
      this.MfNames.TryGetValue(MfCode, out MfName);
      this.textBox1.Text = MfCode.ToString("x2") + " / " + MfName;
      this.txtGameName.Text = Encoding.Default.GetString(GameName);
      string strDiskType;
      this.DiskTypes.TryGetValue(DiskType, out strDiskType);
      this.txtDiskType.Text = strDiskType;
      this.txtRev.Text = GameRevision.ToString();
      if (DiskSide != 0)
      {
        if (DiskSide == 1)
          this.txtSide.Text = "B";
        else
          this.txtSide.Text = "Unknown";
      }
      else
        this.txtSide.Text = "A";
      this.txtDiskNumber.Text = "Disk " + (DiskNumber + 1).ToString();
      if (Shutter != 0)
      {
        if (Shutter == 1)
          this.txtShutter.Text = "Yes";
        else
          this.txtShutter.Text = "Unknown value";
      }
      else
        this.txtShutter.Text = "No";
      this.txtBootFile.Text = BootFile.ToString();
      int MfDateYear = this.BCDBlackMagic((int) MfDate[0]);
      int MfDateMonth = this.BCDBlackMagic((int) MfDate[1]);
      int MfDateDay = this.BCDBlackMagic((int) MfDate[2]);
      this.txtMfDate.Text = (MfDateYear < 61 ? MfDateYear + 1988 : MfDateYear + 1925).ToString() + "-" + MfDateMonth.ToString("D2") + "-" + MfDateDay.ToString("D2");
      int RwDateYear = this.BCDBlackMagic((int) RwDate[0]);
      int RwDateMonth = this.BCDBlackMagic((int) RwDate[1]);
      int RwDateDay = this.BCDBlackMagic((int) RwDate[2]);
      this.txtRwDate.Text = (RwDateYear < 61 ? RwDateYear + 1988 : RwDateYear + 1925).ToString() + "-" + RwDateMonth.ToString("D2") + "-" + RwDateDay.ToString("D2");
      this.txtRwSerial.Text = RwSerial[0].ToString("x") + RwSerial[1].ToString("x");
      if (RwCount == 0)
      {
        this.txtRwCount.Text = "0 (retail disk)";
        if (DiskPrice != 0)
        {
          if (DiskPrice == 1)
            this.txtPrice.Text = "3400 yen (game + accessories)";
          else
            this.txtPrice.Text = "Unknown";
        }
        else
          this.txtPrice.Text = "3400 yen (game only)";
      }
      else
      {
        this.txtRwCount.Text = this.BCDBlackMagic(RwCount).ToString();
        if (DiskPrice != 0)
        {
          if (DiskPrice == 1)
            this.txtPrice.Text = "600 yen (per rewrite)";
          else
            this.txtPrice.Text = "Unknown";
        }
        else
          this.txtPrice.Text = "500 yen (per rewrite)";
      }
    }

    public frmMain()
    {
      this.InitializeComponent();
    }

    private void btnOpen_Click(object sender, EventArgs e)
    {
      int num = (int) this.FdsDlgOpen.ShowDialog();
    }

    private void FdsDlgOpen_FileOk(object sender, CancelEventArgs e)
    {
      this.cmbSide.Items.Clear();
      this.txtFileName.Text = this.FdsDlgOpen.FileName;
      int NumSides;
      switch (new FileInfo(this.FdsDlgOpen.FileName).Length)
      {
        case 131016:
          NumSides = 2;
          break;
        case 262000:
          NumSides = 4;
          break;
        case 262016:
          NumSides = 4;
          break;
        case 65500:
          NumSides = 1;
          break;
        case 65516:
          NumSides = 1;
          break;
        case 131000:
          NumSides = 2;
          break;
        default:
          MessageBox.Show("Image not valid. Valid images are 1, 2 or 4-sided, with or without a 16-byte header.");
          return;
      }
      for (int index = 0; index < NumSides; ++index)
        cmbSide.Items.Add("Side " + (index + 1).ToString());
      cmbSide.SelectedIndex = 0;
    }

    private void btnQuit_Click(object sender, EventArgs e)
    {
      Application.Exit();
    }

    private void btnExtract_Click(object sender, EventArgs e)
    {
      if (this.lvwFileList.SelectedItems.Count <= 0)
        return;
      this.FdsDlgSave.FileName = this.lvwFileList.SelectedItems[0].SubItems[2].Text;
      int num = (int) this.FdsDlgSave.ShowDialog();
    }

    private void FdsDlgSave_FileOk(object sender, CancelEventArgs e)
    {
      FileStream OutputFile = File.OpenWrite(this.FdsDlgSave.FileName);
      FileStream InputFile = File.OpenRead(this.FdsDlgOpen.FileName);
      int index = this.lvwFileList.SelectedItems[0].Index;

      InputFile.Seek(this.FDSFiles[index].FileDataStart, SeekOrigin.Begin);
      byte[] ReadBuffer = new byte[this.FDSFiles[index].FileSize];
      InputFile.Read(ReadBuffer, 0, this.FDSFiles[index].FileSize);
      byte[] WriteBuffer = ReadBuffer;
      int offset = 0;
      int fileSize = this.FDSFiles[index].FileSize;
      OutputFile.Write(WriteBuffer, offset, fileSize);

      InputFile.Close();
      OutputFile.Close();
    }

    private void extractToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (this.lvwFileList.SelectedItems.Count <= 0)
        return;
      this.FdsDlgSave.FileName = this.lvwFileList.SelectedItems[0].SubItems[2].Text;
      int num = (int) this.FdsDlgSave.ShowDialog();
    }

    private void frmMain_Load(object sender, EventArgs e)
    {
    }

    private void cmbSide_SelectedIndexChanged(object sender, EventArgs e)
    {
      this.LoadSide(this.FdsDlgOpen.FileName, this.cmbSide.SelectedIndex);
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      this.components = (IContainer) new Container();
      this.textBox1 = new TextBox();
      this.btnOpen = new Button();
      this.FdsDlgOpen = new OpenFileDialog();
      this.label1 = new Label();
      this.label2 = new Label();
      this.txtGameName = new TextBox();
      this.label3 = new Label();
      this.txtDiskType = new TextBox();
      this.label4 = new Label();
      this.txtRev = new TextBox();
      this.label5 = new Label();
      this.txtSide = new TextBox();
      this.label6 = new Label();
      this.txtDiskNumber = new TextBox();
      this.label7 = new Label();
      this.txtShutter = new TextBox();
      this.label8 = new Label();
      this.txtBootFile = new TextBox();
      this.label9 = new Label();
      this.txtMfDate = new TextBox();
      this.label10 = new Label();
      this.txtRwDate = new TextBox();
      this.label11 = new Label();
      this.txtRwSerial = new TextBox();
      this.label12 = new Label();
      this.txtRwCount = new TextBox();
      this.label13 = new Label();
      this.txtPrice = new TextBox();
      this.lvwFileList = new ListView();
      this.clmNumber = new ColumnHeader();
      this.clmID = new ColumnHeader();
      this.clmName = new ColumnHeader();
      this.clmAddress = new ColumnHeader();
      this.clmSize = new ColumnHeader();
      this.clmType = new ColumnHeader();
      this.ctxmExtract = new ContextMenuStrip(this.components);
      this.extractToolStripMenuItem = new ToolStripMenuItem();
      this.btnQuit = new Button();
      this.btnExtract = new Button();
      this.FdsDlgSave = new SaveFileDialog();
      this.txtFileName = new TextBox();
      this.cmbSide = new ComboBox();
      this.ctxmExtract.SuspendLayout();
      this.SuspendLayout();
      this.textBox1.Location = new Point(156, 51);
      this.textBox1.Name = "textBox1";
      this.textBox1.ReadOnly = true;
      this.textBox1.Size = new Size(194, 20);
      this.textBox1.TabIndex = 0;
      this.btnOpen.Location = new Point(35, 402);
      this.btnOpen.Name = "btnOpen";
      this.btnOpen.Size = new Size(119, 25);
      this.btnOpen.TabIndex = 1;
      this.btnOpen.Text = "Open";
      this.btnOpen.UseVisualStyleBackColor = true;
      this.btnOpen.Click += new EventHandler(this.btnOpen_Click);
      this.FdsDlgOpen.DefaultExt = "fds";
      this.FdsDlgOpen.Filter = "FDS images|*.fds";
      this.FdsDlgOpen.FileOk += new CancelEventHandler(this.FdsDlgOpen_FileOk);
      this.label1.AutoSize = true;
      this.label1.Location = new Point(32, 54);
      this.label1.Name = "label1";
      this.label1.Size = new Size(70, 13);
      this.label1.TabIndex = 2;
      this.label1.Text = "Manufacturer";
      this.label2.AutoSize = true;
      this.label2.Location = new Point(32, 80);
      this.label2.Name = "label2";
      this.label2.Size = new Size(62, 13);
      this.label2.TabIndex = 4;
      this.label2.Text = "Game code";
      this.txtGameName.Location = new Point(156, 77);
      this.txtGameName.Name = "txtGameName";
      this.txtGameName.ReadOnly = true;
      this.txtGameName.Size = new Size(194, 20);
      this.txtGameName.TabIndex = 3;
      this.label3.AutoSize = true;
      this.label3.Location = new Point(32, 106);
      this.label3.Name = "label3";
      this.label3.Size = new Size(51, 13);
      this.label3.TabIndex = 6;
      this.label3.Text = "Disk type";
      this.txtDiskType.Location = new Point(156, 103);
      this.txtDiskType.Name = "txtDiskType";
      this.txtDiskType.ReadOnly = true;
      this.txtDiskType.Size = new Size(194, 20);
      this.txtDiskType.TabIndex = 5;
      this.label4.AutoSize = true;
      this.label4.Location = new Point(32, 132);
      this.label4.Name = "label4";
      this.label4.Size = new Size(74, 13);
      this.label4.TabIndex = 8;
      this.label4.Text = "Game revision";
      this.txtRev.Location = new Point(156, 129);
      this.txtRev.Name = "txtRev";
      this.txtRev.ReadOnly = true;
      this.txtRev.Size = new Size(194, 20);
      this.txtRev.TabIndex = 7;
      this.label5.AutoSize = true;
      this.label5.Location = new Point(32, 158);
      this.label5.Name = "label5";
      this.label5.Size = new Size(28, 13);
      this.label5.TabIndex = 10;
      this.label5.Text = "Side";
      this.txtSide.Location = new Point(156, 155);
      this.txtSide.Name = "txtSide";
      this.txtSide.ReadOnly = true;
      this.txtSide.Size = new Size(194, 20);
      this.txtSide.TabIndex = 9;
      this.label6.AutoSize = true;
      this.label6.Location = new Point(32, 184);
      this.label6.Name = "label6";
      this.label6.Size = new Size(66, 13);
      this.label6.TabIndex = 12;
      this.label6.Text = "Disk number";
      this.txtDiskNumber.Location = new Point(156, 181);
      this.txtDiskNumber.Name = "txtDiskNumber";
      this.txtDiskNumber.ReadOnly = true;
      this.txtDiskNumber.Size = new Size(194, 20);
      this.txtDiskNumber.TabIndex = 11;
      this.label7.AutoSize = true;
      this.label7.Location = new Point(32, 210);
      this.label7.Name = "label7";
      this.label7.Size = new Size(79, 13);
      this.label7.TabIndex = 14;
      this.label7.Text = "Shutter present";
      this.txtShutter.Location = new Point(156, 207);
      this.txtShutter.Name = "txtShutter";
      this.txtShutter.ReadOnly = true;
      this.txtShutter.Size = new Size(194, 20);
      this.txtShutter.TabIndex = 13;
      this.label8.AutoSize = true;
      this.label8.Location = new Point(32, 236);
      this.label8.Name = "label8";
      this.label8.Size = new Size(73, 13);
      this.label8.TabIndex = 16;
      this.label8.Text = "Boot file index";
      this.txtBootFile.Location = new Point(156, 233);
      this.txtBootFile.Name = "txtBootFile";
      this.txtBootFile.ReadOnly = true;
      this.txtBootFile.Size = new Size(194, 20);
      this.txtBootFile.TabIndex = 15;
      this.label9.AutoSize = true;
      this.label9.Location = new Point(32, 262);
      this.label9.Name = "label9";
      this.label9.Size = new Size(99, 13);
      this.label9.TabIndex = 18;
      this.label9.Text = "Manufacturing date";
      this.txtMfDate.Location = new Point(156, 259);
      this.txtMfDate.Name = "txtMfDate";
      this.txtMfDate.ReadOnly = true;
      this.txtMfDate.Size = new Size(194, 20);
      this.txtMfDate.TabIndex = 17;
      this.label10.AutoSize = true;
      this.label10.Location = new Point(32, 288);
      this.label10.Name = "label10";
      this.label10.Size = new Size(76, 13);
      this.label10.TabIndex = 20;
      this.label10.Text = "Rewritten date";
      this.txtRwDate.Location = new Point(156, 285);
      this.txtRwDate.Name = "txtRwDate";
      this.txtRwDate.ReadOnly = true;
      this.txtRwDate.Size = new Size(194, 20);
      this.txtRwDate.TabIndex = 19;
      this.label11.AutoSize = true;
      this.label11.Location = new Point(32, 314);
      this.label11.Name = "label11";
      this.label11.Size = new Size(96, 13);
      this.label11.TabIndex = 22;
      this.label11.Text = "Disk Writer serial #";
      this.txtRwSerial.Location = new Point(156, 311);
      this.txtRwSerial.Name = "txtRwSerial";
      this.txtRwSerial.ReadOnly = true;
      this.txtRwSerial.Size = new Size(194, 20);
      this.txtRwSerial.TabIndex = 21;
      this.label12.AutoSize = true;
      this.label12.Location = new Point(32, 340);
      this.label12.Name = "label12";
      this.label12.Size = new Size(73, 13);
      this.label12.TabIndex = 24;
      this.label12.Text = "Rewrite count";
      this.txtRwCount.Location = new Point(156, 337);
      this.txtRwCount.Name = "txtRwCount";
      this.txtRwCount.ReadOnly = true;
      this.txtRwCount.Size = new Size(194, 20);
      this.txtRwCount.TabIndex = 23;
      this.label13.AutoSize = true;
      this.label13.Location = new Point(32, 366);
      this.label13.Name = "label13";
      this.label13.Size = new Size(31, 13);
      this.label13.TabIndex = 26;
      this.label13.Text = "Price";
      this.txtPrice.Location = new Point(156, 363);
      this.txtPrice.Name = "txtPrice";
      this.txtPrice.ReadOnly = true;
      this.txtPrice.Size = new Size(194, 20);
      this.txtPrice.TabIndex = 25;
      this.lvwFileList.Columns.AddRange(new ColumnHeader[6]
      {
        this.clmNumber,
        this.clmID,
        this.clmName,
        this.clmAddress,
        this.clmSize,
        this.clmType
      });
      this.lvwFileList.ContextMenuStrip = this.ctxmExtract;
      this.lvwFileList.FullRowSelect = true;
      this.lvwFileList.Location = new Point(378, 51);
      this.lvwFileList.MultiSelect = false;
      this.lvwFileList.Name = "lvwFileList";
      this.lvwFileList.Size = new Size(425, 332);
      this.lvwFileList.TabIndex = 28;
      this.lvwFileList.UseCompatibleStateImageBehavior = false;
      this.lvwFileList.View = View.Details;
      this.clmNumber.Text = "Number";
      this.clmNumber.Width = 52;
      this.clmID.Text = "ID";
      this.clmID.Width = 30;
      this.clmName.Text = "Name";
      this.clmName.Width = 88;
      this.clmAddress.Text = "Load Address";
      this.clmAddress.Width = 84;
      this.clmSize.Text = "Size";
      this.clmType.Text = "Type";
      this.clmType.Width = 80;
      this.ctxmExtract.Items.AddRange(new ToolStripItem[1]
      {
        (ToolStripItem) this.extractToolStripMenuItem
      });
      this.ctxmExtract.Name = "ctxmExtract";
      this.ctxmExtract.Size = new Size(110, 26);
      this.extractToolStripMenuItem.Name = "extractToolStripMenuItem";
      this.extractToolStripMenuItem.Size = new Size(109, 22);
      this.extractToolStripMenuItem.Text = "Extract";
      this.extractToolStripMenuItem.Click += new EventHandler(extractToolStripMenuItem_Click);
      this.btnQuit.Location = new Point(231, 402);
      this.btnQuit.Name = "btnQuit";
      this.btnQuit.Size = new Size(119, 25);
      this.btnQuit.TabIndex = 29;
      this.btnQuit.Text = "Quit";
      this.btnQuit.UseVisualStyleBackColor = true;
      this.btnQuit.Click += new EventHandler(btnQuit_Click);
      this.btnExtract.Location = new Point(684, 402);
      this.btnExtract.Name = "btnExtract";
      this.btnExtract.Size = new Size(119, 25);
      this.btnExtract.TabIndex = 30;
      this.btnExtract.Text = "Extract";
      this.btnExtract.UseVisualStyleBackColor = true;
      this.btnExtract.Click += new EventHandler(this.btnExtract_Click);
      this.FdsDlgSave.FileOk += new CancelEventHandler(this.FdsDlgSave_FileOk);
      this.txtFileName.Location = new Point(35, 13);
      this.txtFileName.Name = "txtFileName";
      this.txtFileName.ReadOnly = true;
      this.txtFileName.Size = new Size(648, 20);
      this.txtFileName.TabIndex = 31;
      this.cmbSide.DropDownStyle = ComboBoxStyle.DropDownList;
      this.cmbSide.FormattingEnabled = true;
      this.cmbSide.Location = new Point(702, 13);
      this.cmbSide.Name = "cmbSide";
      this.cmbSide.Size = new Size(101, 21);
      this.cmbSide.TabIndex = 32;
      this.cmbSide.SelectedIndexChanged += new EventHandler(this.cmbSide_SelectedIndexChanged);
      this.AutoScaleDimensions = new SizeF(6f, 13f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.ClientSize = new Size(830, 445);
      this.Controls.Add((Control) this.cmbSide);
      this.Controls.Add((Control) this.txtFileName);
      this.Controls.Add((Control) this.btnExtract);
      this.Controls.Add((Control) this.btnQuit);
      this.Controls.Add((Control) this.lvwFileList);
      this.Controls.Add((Control) this.label13);
      this.Controls.Add((Control) this.txtPrice);
      this.Controls.Add((Control) this.label12);
      this.Controls.Add((Control) this.txtRwCount);
      this.Controls.Add((Control) this.label11);
      this.Controls.Add((Control) this.txtRwSerial);
      this.Controls.Add((Control) this.label10);
      this.Controls.Add((Control) this.txtRwDate);
      this.Controls.Add((Control) this.label9);
      this.Controls.Add((Control) this.txtMfDate);
      this.Controls.Add((Control) this.label8);
      this.Controls.Add((Control) this.txtBootFile);
      this.Controls.Add((Control) this.label7);
      this.Controls.Add((Control) this.txtShutter);
      this.Controls.Add((Control) this.label6);
      this.Controls.Add((Control) this.txtDiskNumber);
      this.Controls.Add((Control) this.label5);
      this.Controls.Add((Control) this.txtSide);
      this.Controls.Add((Control) this.label4);
      this.Controls.Add((Control) this.txtRev);
      this.Controls.Add((Control) this.label3);
      this.Controls.Add((Control) this.txtDiskType);
      this.Controls.Add((Control) this.label2);
      this.Controls.Add((Control) this.txtGameName);
      this.Controls.Add((Control) this.label1);
      this.Controls.Add((Control) this.btnOpen);
      this.Controls.Add((Control) this.textBox1);
      this.FormBorderStyle = FormBorderStyle.FixedSingle;
      this.MaximizeBox = false;
      this.Text = "FDS Info";
      this.Load += new EventHandler(this.frmMain_Load);
      this.ctxmExtract.ResumeLayout(false);
      this.ResumeLayout(false);
      this.PerformLayout();
    }
  }
}
