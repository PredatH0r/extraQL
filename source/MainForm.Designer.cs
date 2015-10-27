using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace ExtraQL
{
  partial class MainForm
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
      this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
      this.panelAdvanced = new System.Windows.Forms.Panel();
      this.grpAdvanced = new System.Windows.Forms.GroupBox();
      this.txtSteamExe = new System.Windows.Forms.TextBox();
      this.lblSteamExe = new System.Windows.Forms.Label();
      this.btnSteamExe = new System.Windows.Forms.Button();
      this.cbAutoQuit = new System.Windows.Forms.CheckBox();
      this.cbHttps = new System.Windows.Forms.CheckBox();
      this.cbAutostart = new System.Windows.Forms.CheckBox();
      this.cbStartMinimized = new System.Windows.Forms.CheckBox();
      this.cbSystemTray = new System.Windows.Forms.CheckBox();
      this.cbBindToAll = new System.Windows.Forms.CheckBox();
      this.panelTop = new System.Windows.Forms.Panel();
      this.cbLog = new System.Windows.Forms.CheckBox();
      this.cbAdvanced = new System.Windows.Forms.CheckBox();
      this.linkAbout = new System.Windows.Forms.LinkLabel();
      this.btnStartQL = new System.Windows.Forms.Button();
      this.picClose = new System.Windows.Forms.PictureBox();
      this.picMinimize = new System.Windows.Forms.PictureBox();
      this.lblVersion = new System.Windows.Forms.Label();
      this.linkConfig = new System.Windows.Forms.LinkLabel();
      this.lblExtra = new System.Windows.Forms.Label();
      this.picLogo = new System.Windows.Forms.PictureBox();
      this.trayIcon = new System.Windows.Forms.NotifyIcon(this.components);
      this.mnuTrayIcon = new System.Windows.Forms.ContextMenuStrip(this.components);
      this.miStartQL = new System.Windows.Forms.ToolStripMenuItem();
      this.quitToolStripMenuItem = new System.Windows.Forms.ToolStripSeparator();
      this.miQuit = new System.Windows.Forms.ToolStripMenuItem();
      this.panelLog = new System.Windows.Forms.Panel();
      this.grpLog = new System.Windows.Forms.GroupBox();
      this.cbLogAllRequests = new System.Windows.Forms.CheckBox();
      this.btnClearLog = new System.Windows.Forms.Button();
      this.cbFollowLog = new System.Windows.Forms.CheckBox();
      this.txtLog = new System.Windows.Forms.TextBox();
      this.autoQuitTimer = new System.Windows.Forms.Timer(this.components);
      this.panelAdvanced.SuspendLayout();
      this.grpAdvanced.SuspendLayout();
      this.panelTop.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.picClose)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.picMinimize)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.picLogo)).BeginInit();
      this.mnuTrayIcon.SuspendLayout();
      this.panelLog.SuspendLayout();
      this.grpLog.SuspendLayout();
      this.SuspendLayout();
      // 
      // openFileDialog1
      // 
      this.openFileDialog1.DefaultExt = "exe";
      this.openFileDialog1.FileName = "openFileDialog1";
      this.openFileDialog1.Filter = "EXE Files|*.exe";
      this.openFileDialog1.RestoreDirectory = true;
      // 
      // panelAdvanced
      // 
      this.panelAdvanced.BackColor = System.Drawing.Color.Transparent;
      this.panelAdvanced.Controls.Add(this.grpAdvanced);
      this.panelAdvanced.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panelAdvanced.Location = new System.Drawing.Point(0, 143);
      this.panelAdvanced.Name = "panelAdvanced";
      this.panelAdvanced.Size = new System.Drawing.Size(429, 198);
      this.panelAdvanced.TabIndex = 4;
      // 
      // grpAdvanced
      // 
      this.grpAdvanced.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.grpAdvanced.Controls.Add(this.txtSteamExe);
      this.grpAdvanced.Controls.Add(this.lblSteamExe);
      this.grpAdvanced.Controls.Add(this.btnSteamExe);
      this.grpAdvanced.Controls.Add(this.cbAutoQuit);
      this.grpAdvanced.Controls.Add(this.cbHttps);
      this.grpAdvanced.Controls.Add(this.cbAutostart);
      this.grpAdvanced.Controls.Add(this.cbStartMinimized);
      this.grpAdvanced.Controls.Add(this.cbSystemTray);
      this.grpAdvanced.Controls.Add(this.cbBindToAll);
      this.grpAdvanced.ForeColor = System.Drawing.Color.White;
      this.grpAdvanced.Location = new System.Drawing.Point(12, 7);
      this.grpAdvanced.Name = "grpAdvanced";
      this.grpAdvanced.Size = new System.Drawing.Size(405, 179);
      this.grpAdvanced.TabIndex = 0;
      this.grpAdvanced.TabStop = false;
      this.grpAdvanced.Text = "Options";
      // 
      // txtSteamExe
      // 
      this.txtSteamExe.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.txtSteamExe.ForeColor = System.Drawing.Color.Black;
      this.txtSteamExe.Location = new System.Drawing.Point(10, 37);
      this.txtSteamExe.Name = "txtSteamExe";
      this.txtSteamExe.Size = new System.Drawing.Size(358, 21);
      this.txtSteamExe.TabIndex = 1;
      // 
      // lblSteamExe
      // 
      this.lblSteamExe.AutoSize = true;
      this.lblSteamExe.Location = new System.Drawing.Point(10, 19);
      this.lblSteamExe.Name = "lblSteamExe";
      this.lblSteamExe.Size = new System.Drawing.Size(317, 13);
      this.lblSteamExe.TabIndex = 0;
      this.lblSteamExe.Text = "File Path to quakelive_steam.exe (leave empty for auto-detect):";
      // 
      // btnSteamExe
      // 
      this.btnSteamExe.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnSteamExe.BackColor = System.Drawing.SystemColors.ButtonFace;
      this.btnSteamExe.ForeColor = System.Drawing.Color.Black;
      this.btnSteamExe.Location = new System.Drawing.Point(374, 37);
      this.btnSteamExe.Name = "btnSteamExe";
      this.btnSteamExe.Size = new System.Drawing.Size(23, 21);
      this.btnSteamExe.TabIndex = 2;
      this.btnSteamExe.Text = "…";
      this.btnSteamExe.UseVisualStyleBackColor = false;
      this.btnSteamExe.Click += new System.EventHandler(this.btnSteamExe_Click);
      // 
      // cbAutoQuit
      // 
      this.cbAutoQuit.AutoSize = true;
      this.cbAutoQuit.Location = new System.Drawing.Point(209, 99);
      this.cbAutoQuit.Name = "cbAutoQuit";
      this.cbAutoQuit.Size = new System.Drawing.Size(138, 17);
      this.cbAutoQuit.TabIndex = 13;
      this.cbAutoQuit.Text = "Autoquit when QL quits";
      this.cbAutoQuit.UseVisualStyleBackColor = true;
      this.cbAutoQuit.CheckedChanged += new System.EventHandler(this.cbAutoQuit_CheckedChanged);
      // 
      // cbHttps
      // 
      this.cbHttps.AutoSize = true;
      this.cbHttps.Location = new System.Drawing.Point(10, 145);
      this.cbHttps.Name = "cbHttps";
      this.cbHttps.Size = new System.Drawing.Size(365, 17);
      this.cbHttps.TabIndex = 15;
      this.cbHttps.Text = "Use HTTPS  (requires setup through https\\install.cmd as Administrator)";
      this.cbHttps.UseVisualStyleBackColor = true;
      this.cbHttps.CheckedChanged += new System.EventHandler(this.cbHttps_CheckedChanged);
      // 
      // cbAutostart
      // 
      this.cbAutostart.AutoSize = true;
      this.cbAutostart.Location = new System.Drawing.Point(10, 99);
      this.cbAutostart.Name = "cbAutostart";
      this.cbAutostart.Size = new System.Drawing.Size(128, 17);
      this.cbAutostart.TabIndex = 12;
      this.cbAutostart.Text = "Autostart Quake Live";
      this.cbAutostart.UseVisualStyleBackColor = true;
      // 
      // cbStartMinimized
      // 
      this.cbStartMinimized.AutoSize = true;
      this.cbStartMinimized.Location = new System.Drawing.Point(209, 76);
      this.cbStartMinimized.Name = "cbStartMinimized";
      this.cbStartMinimized.Size = new System.Drawing.Size(98, 17);
      this.cbStartMinimized.TabIndex = 11;
      this.cbStartMinimized.Text = "Start Minimized";
      this.cbStartMinimized.UseVisualStyleBackColor = true;
      // 
      // cbSystemTray
      // 
      this.cbSystemTray.AutoSize = true;
      this.cbSystemTray.Location = new System.Drawing.Point(10, 76);
      this.cbSystemTray.Name = "cbSystemTray";
      this.cbSystemTray.Size = new System.Drawing.Size(126, 17);
      this.cbSystemTray.TabIndex = 10;
      this.cbSystemTray.Text = "Show in System Tray";
      this.cbSystemTray.UseVisualStyleBackColor = true;
      this.cbSystemTray.CheckedChanged += new System.EventHandler(this.cbSystemTray_CheckedChanged);
      // 
      // cbBindToAll
      // 
      this.cbBindToAll.AutoSize = true;
      this.cbBindToAll.Location = new System.Drawing.Point(10, 122);
      this.cbBindToAll.Name = "cbBindToAll";
      this.cbBindToAll.Size = new System.Drawing.Size(310, 17);
      this.cbBindToAll.TabIndex = 14;
      this.cbBindToAll.Text = "Allow other computers to access your extraQL HTTP server";
      this.cbBindToAll.UseVisualStyleBackColor = true;
      this.cbBindToAll.CheckedChanged += new System.EventHandler(this.cbBindAll_CheckedChanged);
      // 
      // panelTop
      // 
      this.panelTop.BackColor = System.Drawing.Color.Transparent;
      this.panelTop.Controls.Add(this.cbLog);
      this.panelTop.Controls.Add(this.cbAdvanced);
      this.panelTop.Controls.Add(this.linkAbout);
      this.panelTop.Controls.Add(this.btnStartQL);
      this.panelTop.Controls.Add(this.picClose);
      this.panelTop.Controls.Add(this.picMinimize);
      this.panelTop.Controls.Add(this.lblVersion);
      this.panelTop.Controls.Add(this.linkConfig);
      this.panelTop.Controls.Add(this.lblExtra);
      this.panelTop.Controls.Add(this.picLogo);
      this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
      this.panelTop.Location = new System.Drawing.Point(0, 0);
      this.panelTop.Name = "panelTop";
      this.panelTop.Size = new System.Drawing.Size(429, 143);
      this.panelTop.TabIndex = 0;
      // 
      // cbLog
      // 
      this.cbLog.AutoSize = true;
      this.cbLog.Checked = true;
      this.cbLog.CheckState = System.Windows.Forms.CheckState.Checked;
      this.cbLog.Location = new System.Drawing.Point(341, 112);
      this.cbLog.Name = "cbLog";
      this.cbLog.Size = new System.Drawing.Size(43, 17);
      this.cbLog.TabIndex = 4;
      this.cbLog.Text = "Log";
      this.cbLog.UseVisualStyleBackColor = true;
      this.cbLog.CheckedChanged += new System.EventHandler(this.cbLog_CheckedChanged);
      // 
      // cbAdvanced
      // 
      this.cbAdvanced.AutoSize = true;
      this.cbAdvanced.Checked = true;
      this.cbAdvanced.CheckState = System.Windows.Forms.CheckState.Checked;
      this.cbAdvanced.Location = new System.Drawing.Point(341, 89);
      this.cbAdvanced.Name = "cbAdvanced";
      this.cbAdvanced.Size = new System.Drawing.Size(63, 17);
      this.cbAdvanced.TabIndex = 3;
      this.cbAdvanced.Text = "Options";
      this.cbAdvanced.UseVisualStyleBackColor = true;
      this.cbAdvanced.CheckedChanged += new System.EventHandler(this.cbAdvanced_CheckedChanged);
      // 
      // linkAbout
      // 
      this.linkAbout.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.linkAbout.AutoSize = true;
      this.linkAbout.LinkArea = new System.Windows.Forms.LinkArea(0, 22);
      this.linkAbout.LinkColor = System.Drawing.Color.Gold;
      this.linkAbout.Location = new System.Drawing.Point(196, 108);
      this.linkAbout.Name = "linkAbout";
      this.linkAbout.Size = new System.Drawing.Size(115, 18);
      this.linkAbout.TabIndex = 2;
      this.linkAbout.TabStop = true;
      this.linkAbout.Text = "Open extraQL website";
      this.linkAbout.UseCompatibleTextRendering = true;
      this.linkAbout.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkAbout_LinkClicked);
      // 
      // btnStartQL
      // 
      this.btnStartQL.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.btnStartQL.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
      this.btnStartQL.FlatAppearance.BorderSize = 2;
      this.btnStartQL.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnStartQL.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
      this.btnStartQL.ForeColor = System.Drawing.Color.Gold;
      this.btnStartQL.Location = new System.Drawing.Point(11, 89);
      this.btnStartQL.Name = "btnStartQL";
      this.btnStartQL.Size = new System.Drawing.Size(135, 37);
      this.btnStartQL.TabIndex = 0;
      this.btnStartQL.Text = "Start Quake Live";
      this.btnStartQL.UseVisualStyleBackColor = false;
      this.btnStartQL.Click += new System.EventHandler(this.btnStartQL_Click);
      // 
      // picClose
      // 
      this.picClose.Image = ((System.Drawing.Image)(resources.GetObject("picClose.Image")));
      this.picClose.Location = new System.Drawing.Point(410, 0);
      this.picClose.Name = "picClose";
      this.picClose.Size = new System.Drawing.Size(17, 15);
      this.picClose.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
      this.picClose.TabIndex = 28;
      this.picClose.TabStop = false;
      this.picClose.Click += new System.EventHandler(this.picClose_Click);
      // 
      // picMinimize
      // 
      this.picMinimize.Image = ((System.Drawing.Image)(resources.GetObject("picMinimize.Image")));
      this.picMinimize.Location = new System.Drawing.Point(387, 0);
      this.picMinimize.Name = "picMinimize";
      this.picMinimize.Size = new System.Drawing.Size(17, 15);
      this.picMinimize.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
      this.picMinimize.TabIndex = 27;
      this.picMinimize.TabStop = false;
      this.picMinimize.Click += new System.EventHandler(this.picMinimize_Click);
      // 
      // lblVersion
      // 
      this.lblVersion.Font = new System.Drawing.Font("Tahoma", 10F);
      this.lblVersion.Location = new System.Drawing.Point(353, 41);
      this.lblVersion.Name = "lblVersion";
      this.lblVersion.Size = new System.Drawing.Size(64, 17);
      this.lblVersion.TabIndex = 1;
      this.lblVersion.Text = "0.00";
      this.lblVersion.TextAlign = System.Drawing.ContentAlignment.TopRight;
      this.lblVersion.MouseMove += new System.Windows.Forms.MouseEventHandler(this.picLogo_MouseMove);
      // 
      // linkConfig
      // 
      this.linkConfig.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.linkConfig.LinkArea = new System.Windows.Forms.LinkArea(0, 40);
      this.linkConfig.LinkColor = System.Drawing.Color.Gold;
      this.linkConfig.Location = new System.Drawing.Point(196, 88);
      this.linkConfig.Name = "linkConfig";
      this.linkConfig.Size = new System.Drawing.Size(136, 20);
      this.linkConfig.TabIndex = 1;
      this.linkConfig.TabStop = true;
      this.linkConfig.Text = "Open QL Config Folder";
      this.linkConfig.UseCompatibleTextRendering = true;
      this.linkConfig.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkConfig_LinkClicked);
      // 
      // lblExtra
      // 
      this.lblExtra.AutoSize = true;
      this.lblExtra.Font = new System.Drawing.Font("Tahoma", 28F, System.Drawing.FontStyle.Bold);
      this.lblExtra.Location = new System.Drawing.Point(127, 17);
      this.lblExtra.Name = "lblExtra";
      this.lblExtra.Size = new System.Drawing.Size(172, 46);
      this.lblExtra.TabIndex = 0;
      this.lblExtra.Text = "extraQL";
      this.lblExtra.Visible = false;
      this.lblExtra.MouseMove += new System.Windows.Forms.MouseEventHandler(this.picLogo_MouseMove);
      // 
      // picLogo
      // 
      this.picLogo.Cursor = System.Windows.Forms.Cursors.SizeAll;
      this.picLogo.Image = ((System.Drawing.Image)(resources.GetObject("picLogo.Image")));
      this.picLogo.Location = new System.Drawing.Point(0, 0);
      this.picLogo.Name = "picLogo";
      this.picLogo.Size = new System.Drawing.Size(430, 78);
      this.picLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
      this.picLogo.TabIndex = 24;
      this.picLogo.TabStop = false;
      this.picLogo.Paint += new System.Windows.Forms.PaintEventHandler(this.picLogo_Paint);
      this.picLogo.MouseMove += new System.Windows.Forms.MouseEventHandler(this.picLogo_MouseMove);
      // 
      // trayIcon
      // 
      this.trayIcon.BalloonTipText = "Quake Live userscript server";
      this.trayIcon.BalloonTipTitle = "extraQL";
      this.trayIcon.ContextMenuStrip = this.mnuTrayIcon;
      this.trayIcon.Text = "extraQL";
      this.trayIcon.MouseUp += new System.Windows.Forms.MouseEventHandler(this.trayIcon_MouseUp);
      // 
      // mnuTrayIcon
      // 
      this.mnuTrayIcon.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miStartQL,
            this.quitToolStripMenuItem,
            this.miQuit});
      this.mnuTrayIcon.Name = "contextMenuStrip1";
      this.mnuTrayIcon.ShowImageMargin = false;
      this.mnuTrayIcon.Size = new System.Drawing.Size(135, 54);
      // 
      // miStartQL
      // 
      this.miStartQL.Name = "miStartQL";
      this.miStartQL.Size = new System.Drawing.Size(134, 22);
      this.miStartQL.Text = "Start Quake Live";
      this.miStartQL.Click += new System.EventHandler(this.miStartQL_Click);
      // 
      // quitToolStripMenuItem
      // 
      this.quitToolStripMenuItem.Name = "quitToolStripMenuItem";
      this.quitToolStripMenuItem.Size = new System.Drawing.Size(131, 6);
      // 
      // miQuit
      // 
      this.miQuit.Name = "miQuit";
      this.miQuit.Size = new System.Drawing.Size(134, 22);
      this.miQuit.Text = "Quit extraQL";
      this.miQuit.Click += new System.EventHandler(this.miQuit_Click);
      // 
      // panelLog
      // 
      this.panelLog.BackColor = System.Drawing.Color.Transparent;
      this.panelLog.Controls.Add(this.grpLog);
      this.panelLog.Dock = System.Windows.Forms.DockStyle.Right;
      this.panelLog.Location = new System.Drawing.Point(429, 0);
      this.panelLog.Name = "panelLog";
      this.panelLog.Size = new System.Drawing.Size(495, 341);
      this.panelLog.TabIndex = 5;
      // 
      // grpLog
      // 
      this.grpLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.grpLog.Controls.Add(this.cbLogAllRequests);
      this.grpLog.Controls.Add(this.btnClearLog);
      this.grpLog.Controls.Add(this.cbFollowLog);
      this.grpLog.Controls.Add(this.txtLog);
      this.grpLog.ForeColor = System.Drawing.Color.White;
      this.grpLog.Location = new System.Drawing.Point(12, 7);
      this.grpLog.Name = "grpLog";
      this.grpLog.Size = new System.Drawing.Size(471, 323);
      this.grpLog.TabIndex = 0;
      this.grpLog.TabStop = false;
      this.grpLog.Text = "Log";
      // 
      // cbLogAllRequests
      // 
      this.cbLogAllRequests.AutoSize = true;
      this.cbLogAllRequests.Location = new System.Drawing.Point(154, 24);
      this.cbLogAllRequests.Name = "cbLogAllRequests";
      this.cbLogAllRequests.Size = new System.Drawing.Size(104, 17);
      this.cbLogAllRequests.TabIndex = 1;
      this.cbLogAllRequests.Text = "Log all Requests";
      this.cbLogAllRequests.UseVisualStyleBackColor = true;
      this.cbLogAllRequests.CheckedChanged += new System.EventHandler(this.cbLogAllRequests_CheckedChanged);
      // 
      // btnClearLog
      // 
      this.btnClearLog.BackColor = System.Drawing.SystemColors.ButtonFace;
      this.btnClearLog.ForeColor = System.Drawing.Color.Black;
      this.btnClearLog.Location = new System.Drawing.Point(367, 20);
      this.btnClearLog.Name = "btnClearLog";
      this.btnClearLog.Size = new System.Drawing.Size(96, 23);
      this.btnClearLog.TabIndex = 2;
      this.btnClearLog.Text = "Clear";
      this.btnClearLog.UseVisualStyleBackColor = false;
      this.btnClearLog.Click += new System.EventHandler(this.btnClearLog_Click);
      // 
      // cbFollowLog
      // 
      this.cbFollowLog.AutoSize = true;
      this.cbFollowLog.Location = new System.Drawing.Point(10, 24);
      this.cbFollowLog.Name = "cbFollowLog";
      this.cbFollowLog.Size = new System.Drawing.Size(122, 17);
      this.cbFollowLog.TabIndex = 0;
      this.cbFollowLog.Text = "Always Scroll to End";
      this.cbFollowLog.UseVisualStyleBackColor = true;
      // 
      // txtLog
      // 
      this.txtLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.txtLog.ForeColor = System.Drawing.Color.Black;
      this.txtLog.Location = new System.Drawing.Point(10, 52);
      this.txtLog.Multiline = true;
      this.txtLog.Name = "txtLog";
      this.txtLog.ReadOnly = true;
      this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
      this.txtLog.Size = new System.Drawing.Size(453, 259);
      this.txtLog.TabIndex = 3;
      // 
      // autoQuitTimer
      // 
      this.autoQuitTimer.Interval = 2000;
      this.autoQuitTimer.Tick += new System.EventHandler(this.autoQuitTimer_Tick);
      // 
      // MainForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
      this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
      this.ClientSize = new System.Drawing.Size(924, 341);
      this.ControlBox = false;
      this.Controls.Add(this.panelAdvanced);
      this.Controls.Add(this.panelTop);
      this.Controls.Add(this.panelLog);
      this.DoubleBuffered = true;
      this.Font = new System.Drawing.Font("Tahoma", 8.25F);
      this.ForeColor = System.Drawing.Color.White;
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.MaximizeBox = false;
      this.MinimumSize = new System.Drawing.Size(430, 160);
      this.Name = "MainForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "extraQL";
      this.panelAdvanced.ResumeLayout(false);
      this.grpAdvanced.ResumeLayout(false);
      this.grpAdvanced.PerformLayout();
      this.panelTop.ResumeLayout(false);
      this.panelTop.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.picClose)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.picMinimize)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.picLogo)).EndInit();
      this.mnuTrayIcon.ResumeLayout(false);
      this.panelLog.ResumeLayout(false);
      this.grpLog.ResumeLayout(false);
      this.grpLog.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion
    private OpenFileDialog openFileDialog1;
    private Panel panelAdvanced;
    private Panel panelTop;
    private CheckBox cbAdvanced;
    private Button btnStartQL;
    private LinkLabel linkAbout;
    private GroupBox grpAdvanced;
    private PictureBox picLogo;
    private Label lblVersion;
    private Label lblExtra;
    private PictureBox picClose;
    private PictureBox picMinimize;
    private CheckBox cbBindToAll;
    private CheckBox cbSystemTray;
    private NotifyIcon trayIcon;
    private ContextMenuStrip mnuTrayIcon;
    private ToolStripMenuItem miStartQL;
    private CheckBox cbStartMinimized;
    private ToolStripSeparator quitToolStripMenuItem;
    private ToolStripMenuItem miQuit;
    private CheckBox cbAutostart;
    private CheckBox cbLog;
    private Panel panelLog;
    private GroupBox grpLog;
    private TextBox txtLog;
    private Button btnClearLog;
    private CheckBox cbFollowLog;
    private CheckBox cbHttps;
    private CheckBox cbLogAllRequests;
    private CheckBox cbAutoQuit;
    private Timer autoQuitTimer;
    private LinkLabel linkConfig;
    private TextBox txtSteamExe;
    private Label lblSteamExe;
    private Button btnSteamExe;
  }
}

