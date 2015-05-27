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
      this.lblLauncherExe = new System.Windows.Forms.Label();
      this.txtLauncherExe = new System.Windows.Forms.TextBox();
      this.btnLauncherExe = new System.Windows.Forms.Button();
      this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
      this.launcherDetectionTimer = new System.Windows.Forms.Timer(this.components);
      this.lblRealm = new System.Windows.Forms.Label();
      this.btnInstallHook = new System.Windows.Forms.Button();
      this.panelAdvanced = new System.Windows.Forms.Panel();
      this.grpAdvanced = new System.Windows.Forms.GroupBox();
      this.txtSteamExe = new System.Windows.Forms.TextBox();
      this.lblSteamExe = new System.Windows.Forms.Label();
      this.btnSteamExe = new System.Windows.Forms.Button();
      this.cbAutoQuit = new System.Windows.Forms.CheckBox();
      this.cbHttps = new System.Windows.Forms.CheckBox();
      this.cbRunAsCommandLine = new System.Windows.Forms.CheckBox();
      this.cbAutostart = new System.Windows.Forms.CheckBox();
      this.cbDownloadUpdates = new System.Windows.Forms.CheckBox();
      this.cbStartMinimized = new System.Windows.Forms.CheckBox();
      this.cbSystemTray = new System.Windows.Forms.CheckBox();
      this.cbBindToAll = new System.Windows.Forms.CheckBox();
      this.cbDisableScripts = new System.Windows.Forms.CheckBox();
      this.linkFocusForum = new System.Windows.Forms.LinkLabel();
      this.linkFocusLogin = new System.Windows.Forms.LinkLabel();
      this.comboRealm = new System.Windows.Forms.ComboBox();
      this.panelTop = new System.Windows.Forms.Panel();
      this.rbUseLauncher = new System.Windows.Forms.RadioButton();
      this.btnStartQL = new System.Windows.Forms.Button();
      this.rbUseSteam = new System.Windows.Forms.RadioButton();
      this.lblUseSteam = new System.Windows.Forms.Label();
      this.picClose = new System.Windows.Forms.PictureBox();
      this.picMinimize = new System.Windows.Forms.PictureBox();
      this.lblVersion = new System.Windows.Forms.Label();
      this.lblExtra = new System.Windows.Forms.Label();
      this.picLogo = new System.Windows.Forms.PictureBox();
      this.linkConfig = new System.Windows.Forms.LinkLabel();
      this.cbLog = new System.Windows.Forms.CheckBox();
      this.comboEmail = new System.Windows.Forms.ComboBox();
      this.cbFocus = new System.Windows.Forms.CheckBox();
      this.linkAbout = new System.Windows.Forms.LinkLabel();
      this.cbAdvanced = new System.Windows.Forms.CheckBox();
      this.txtPassword = new System.Windows.Forms.TextBox();
      this.lblPassword = new System.Windows.Forms.Label();
      this.lblEmail = new System.Windows.Forms.Label();
      this.panelFocus = new System.Windows.Forms.Panel();
      this.grpFocus = new System.Windows.Forms.GroupBox();
      this.trayIcon = new System.Windows.Forms.NotifyIcon(this.components);
      this.mnuTrayIcon = new System.Windows.Forms.ContextMenuStrip(this.components);
      this.miStartQL = new System.Windows.Forms.ToolStripMenuItem();
      this.quitToolStripMenuItem = new System.Windows.Forms.ToolStripSeparator();
      this.miQuit = new System.Windows.Forms.ToolStripMenuItem();
      this.launcherPlayTimer = new System.Windows.Forms.Timer(this.components);
      this.updateCheckTimer = new System.Windows.Forms.Timer(this.components);
      this.panelLog = new System.Windows.Forms.Panel();
      this.grpLog = new System.Windows.Forms.GroupBox();
      this.cbLogAllRequests = new System.Windows.Forms.CheckBox();
      this.btnClearLog = new System.Windows.Forms.Button();
      this.cbFollowLog = new System.Windows.Forms.CheckBox();
      this.txtLog = new System.Windows.Forms.TextBox();
      this.autoQuitTimer = new System.Windows.Forms.Timer(this.components);
      this.panelAccount = new System.Windows.Forms.Panel();
      this.grpAccount = new System.Windows.Forms.GroupBox();
      this.panelStart = new System.Windows.Forms.Panel();
      this.panelAdvanced.SuspendLayout();
      this.grpAdvanced.SuspendLayout();
      this.panelTop.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.picClose)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.picMinimize)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.picLogo)).BeginInit();
      this.panelFocus.SuspendLayout();
      this.grpFocus.SuspendLayout();
      this.mnuTrayIcon.SuspendLayout();
      this.panelLog.SuspendLayout();
      this.grpLog.SuspendLayout();
      this.panelAccount.SuspendLayout();
      this.grpAccount.SuspendLayout();
      this.panelStart.SuspendLayout();
      this.SuspendLayout();
      // 
      // lblLauncherExe
      // 
      this.lblLauncherExe.AutoSize = true;
      this.lblLauncherExe.Location = new System.Drawing.Point(10, 68);
      this.lblLauncherExe.Name = "lblLauncherExe";
      this.lblLauncherExe.Size = new System.Drawing.Size(134, 13);
      this.lblLauncherExe.TabIndex = 3;
      this.lblLauncherExe.Text = "File Path to Launcher.exe:";
      // 
      // txtLauncherExe
      // 
      this.txtLauncherExe.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.txtLauncherExe.ForeColor = System.Drawing.Color.Black;
      this.txtLauncherExe.Location = new System.Drawing.Point(10, 86);
      this.txtLauncherExe.Name = "txtLauncherExe";
      this.txtLauncherExe.Size = new System.Drawing.Size(358, 21);
      this.txtLauncherExe.TabIndex = 4;
      // 
      // btnLauncherExe
      // 
      this.btnLauncherExe.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnLauncherExe.BackColor = System.Drawing.SystemColors.ButtonFace;
      this.btnLauncherExe.ForeColor = System.Drawing.Color.Black;
      this.btnLauncherExe.Location = new System.Drawing.Point(374, 86);
      this.btnLauncherExe.Name = "btnLauncherExe";
      this.btnLauncherExe.Size = new System.Drawing.Size(23, 21);
      this.btnLauncherExe.TabIndex = 6;
      this.btnLauncherExe.Text = "…";
      this.btnLauncherExe.UseVisualStyleBackColor = false;
      this.btnLauncherExe.Click += new System.EventHandler(this.btnLauncherExe_Click);
      // 
      // openFileDialog1
      // 
      this.openFileDialog1.DefaultExt = "exe";
      this.openFileDialog1.FileName = "openFileDialog1";
      this.openFileDialog1.Filter = "EXE Files|*.exe";
      this.openFileDialog1.RestoreDirectory = true;
      // 
      // launcherDetectionTimer
      // 
      this.launcherDetectionTimer.Interval = 500;
      this.launcherDetectionTimer.Tick += new System.EventHandler(this.laucherDetectionTimer_Tick);
      // 
      // lblRealm
      // 
      this.lblRealm.AutoSize = true;
      this.lblRealm.Location = new System.Drawing.Point(7, 27);
      this.lblRealm.Name = "lblRealm";
      this.lblRealm.Size = new System.Drawing.Size(40, 13);
      this.lblRealm.TabIndex = 0;
      this.lblRealm.Text = "Realm:";
      // 
      // btnInstallHook
      // 
      this.btnInstallHook.BackColor = System.Drawing.SystemColors.ButtonFace;
      this.btnInstallHook.FlatAppearance.BorderSize = 2;
      this.btnInstallHook.ForeColor = System.Drawing.Color.Black;
      this.btnInstallHook.Location = new System.Drawing.Point(209, 114);
      this.btnInstallHook.Name = "btnInstallHook";
      this.btnInstallHook.Size = new System.Drawing.Size(188, 29);
      this.btnInstallHook.TabIndex = 8;
      this.btnInstallHook.Text = "Re-install hook.js";
      this.btnInstallHook.UseVisualStyleBackColor = false;
      this.btnInstallHook.Click += new System.EventHandler(this.btnInstallHook_Click);
      // 
      // panelAdvanced
      // 
      this.panelAdvanced.BackColor = System.Drawing.Color.Transparent;
      this.panelAdvanced.Controls.Add(this.grpAdvanced);
      this.panelAdvanced.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panelAdvanced.Location = new System.Drawing.Point(0, 395);
      this.panelAdvanced.Name = "panelAdvanced";
      this.panelAdvanced.Size = new System.Drawing.Size(429, 275);
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
      this.grpAdvanced.Controls.Add(this.txtLauncherExe);
      this.grpAdvanced.Controls.Add(this.cbRunAsCommandLine);
      this.grpAdvanced.Controls.Add(this.cbAutostart);
      this.grpAdvanced.Controls.Add(this.cbDownloadUpdates);
      this.grpAdvanced.Controls.Add(this.cbStartMinimized);
      this.grpAdvanced.Controls.Add(this.cbSystemTray);
      this.grpAdvanced.Controls.Add(this.cbBindToAll);
      this.grpAdvanced.Controls.Add(this.lblLauncherExe);
      this.grpAdvanced.Controls.Add(this.cbDisableScripts);
      this.grpAdvanced.Controls.Add(this.btnLauncherExe);
      this.grpAdvanced.Controls.Add(this.btnInstallHook);
      this.grpAdvanced.ForeColor = System.Drawing.Color.White;
      this.grpAdvanced.Location = new System.Drawing.Point(12, 7);
      this.grpAdvanced.Name = "grpAdvanced";
      this.grpAdvanced.Size = new System.Drawing.Size(405, 256);
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
      this.txtSteamExe.TextChanged += new System.EventHandler(this.txtSteamExe_TextChanged);
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
      this.cbAutoQuit.Location = new System.Drawing.Point(209, 186);
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
      this.cbHttps.Location = new System.Drawing.Point(10, 232);
      this.cbHttps.Name = "cbHttps";
      this.cbHttps.Size = new System.Drawing.Size(365, 17);
      this.cbHttps.TabIndex = 15;
      this.cbHttps.Text = "Use HTTPS  (requires setup through https\\install.cmd as Administrator)";
      this.cbHttps.UseVisualStyleBackColor = true;
      this.cbHttps.CheckedChanged += new System.EventHandler(this.cbHttps_CheckedChanged);
      // 
      // cbRunAsCommandLine
      // 
      this.cbRunAsCommandLine.AutoSize = true;
      this.cbRunAsCommandLine.Location = new System.Drawing.Point(209, 67);
      this.cbRunAsCommandLine.Name = "cbRunAsCommandLine";
      this.cbRunAsCommandLine.Size = new System.Drawing.Size(131, 17);
      this.cbRunAsCommandLine.TabIndex = 5;
      this.cbRunAsCommandLine.Text = "Run as Command Line";
      this.cbRunAsCommandLine.UseVisualStyleBackColor = true;
      // 
      // cbAutostart
      // 
      this.cbAutostart.AutoSize = true;
      this.cbAutostart.Location = new System.Drawing.Point(10, 186);
      this.cbAutostart.Name = "cbAutostart";
      this.cbAutostart.Size = new System.Drawing.Size(128, 17);
      this.cbAutostart.TabIndex = 12;
      this.cbAutostart.Text = "Autostart Quake Live";
      this.cbAutostart.UseVisualStyleBackColor = true;
      // 
      // cbDownloadUpdates
      // 
      this.cbDownloadUpdates.AutoSize = true;
      this.cbDownloadUpdates.Checked = true;
      this.cbDownloadUpdates.CheckState = System.Windows.Forms.CheckState.Checked;
      this.cbDownloadUpdates.Location = new System.Drawing.Point(10, 140);
      this.cbDownloadUpdates.Name = "cbDownloadUpdates";
      this.cbDownloadUpdates.Size = new System.Drawing.Size(116, 17);
      this.cbDownloadUpdates.TabIndex = 9;
      this.cbDownloadUpdates.Text = "Download Updates";
      this.cbDownloadUpdates.UseVisualStyleBackColor = true;
      this.cbDownloadUpdates.CheckedChanged += new System.EventHandler(this.cbDownloadUpdates_CheckedChanged);
      // 
      // cbStartMinimized
      // 
      this.cbStartMinimized.AutoSize = true;
      this.cbStartMinimized.Location = new System.Drawing.Point(209, 163);
      this.cbStartMinimized.Name = "cbStartMinimized";
      this.cbStartMinimized.Size = new System.Drawing.Size(98, 17);
      this.cbStartMinimized.TabIndex = 11;
      this.cbStartMinimized.Text = "Start Minimized";
      this.cbStartMinimized.UseVisualStyleBackColor = true;
      // 
      // cbSystemTray
      // 
      this.cbSystemTray.AutoSize = true;
      this.cbSystemTray.Location = new System.Drawing.Point(10, 163);
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
      this.cbBindToAll.Location = new System.Drawing.Point(10, 209);
      this.cbBindToAll.Name = "cbBindToAll";
      this.cbBindToAll.Size = new System.Drawing.Size(310, 17);
      this.cbBindToAll.TabIndex = 14;
      this.cbBindToAll.Text = "Allow other computers to access your extraQL HTTP server";
      this.cbBindToAll.UseVisualStyleBackColor = true;
      this.cbBindToAll.CheckedChanged += new System.EventHandler(this.cbBindAll_CheckedChanged);
      // 
      // cbDisableScripts
      // 
      this.cbDisableScripts.AutoSize = true;
      this.cbDisableScripts.Location = new System.Drawing.Point(10, 117);
      this.cbDisableScripts.Name = "cbDisableScripts";
      this.cbDisableScripts.Size = new System.Drawing.Size(116, 17);
      this.cbDisableScripts.TabIndex = 7;
      this.cbDisableScripts.Text = "Disable Userscripts";
      this.cbDisableScripts.UseVisualStyleBackColor = true;
      this.cbDisableScripts.CheckedChanged += new System.EventHandler(this.cbDisableScripts_CheckedChanged);
      // 
      // linkFocusForum
      // 
      this.linkFocusForum.LinkArea = new System.Windows.Forms.LinkArea(0, 37);
      this.linkFocusForum.LinkColor = System.Drawing.Color.Gold;
      this.linkFocusForum.Location = new System.Drawing.Point(209, 57);
      this.linkFocusForum.Name = "linkFocusForum";
      this.linkFocusForum.Size = new System.Drawing.Size(111, 18);
      this.linkFocusForum.TabIndex = 3;
      this.linkFocusForum.TabStop = true;
      this.linkFocusForum.Text = "Open Focus Forum ";
      this.linkFocusForum.UseCompatibleTextRendering = true;
      this.linkFocusForum.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkFocusForum_LinkClicked);
      // 
      // linkFocusLogin
      // 
      this.linkFocusLogin.LinkArea = new System.Windows.Forms.LinkArea(0, 40);
      this.linkFocusLogin.LinkColor = System.Drawing.Color.Gold;
      this.linkFocusLogin.Location = new System.Drawing.Point(10, 57);
      this.linkFocusLogin.Name = "linkFocusLogin";
      this.linkFocusLogin.Size = new System.Drawing.Size(136, 13);
      this.linkFocusLogin.TabIndex = 2;
      this.linkFocusLogin.TabStop = true;
      this.linkFocusLogin.Text = "Login to Focus Gate";
      this.linkFocusLogin.UseCompatibleTextRendering = true;
      this.linkFocusLogin.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkFocusLogin_LinkClicked);
      // 
      // comboRealm
      // 
      this.comboRealm.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.comboRealm.ForeColor = System.Drawing.Color.Black;
      this.comboRealm.FormattingEnabled = true;
      this.comboRealm.Location = new System.Drawing.Point(53, 24);
      this.comboRealm.Name = "comboRealm";
      this.comboRealm.Size = new System.Drawing.Size(344, 21);
      this.comboRealm.TabIndex = 1;
      // 
      // panelTop
      // 
      this.panelTop.BackColor = System.Drawing.Color.Transparent;
      this.panelTop.Controls.Add(this.rbUseLauncher);
      this.panelTop.Controls.Add(this.btnStartQL);
      this.panelTop.Controls.Add(this.rbUseSteam);
      this.panelTop.Controls.Add(this.lblUseSteam);
      this.panelTop.Controls.Add(this.picClose);
      this.panelTop.Controls.Add(this.picMinimize);
      this.panelTop.Controls.Add(this.lblVersion);
      this.panelTop.Controls.Add(this.lblExtra);
      this.panelTop.Controls.Add(this.picLogo);
      this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
      this.panelTop.Location = new System.Drawing.Point(0, 0);
      this.panelTop.Name = "panelTop";
      this.panelTop.Size = new System.Drawing.Size(429, 143);
      this.panelTop.TabIndex = 0;
      // 
      // rbUseLauncher
      // 
      this.rbUseLauncher.AutoSize = true;
      this.rbUseLauncher.Checked = true;
      this.rbUseLauncher.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.rbUseLauncher.Location = new System.Drawing.Point(96, 108);
      this.rbUseLauncher.Name = "rbUseLauncher";
      this.rbUseLauncher.Size = new System.Drawing.Size(146, 20);
      this.rbUseLauncher.TabIndex = 4;
      this.rbUseLauncher.TabStop = true;
      this.rbUseLauncher.Text = "Standalone Launcher";
      this.rbUseLauncher.UseVisualStyleBackColor = true;
      this.rbUseLauncher.CheckedChanged += new System.EventHandler(this.rbUseSteam_CheckedChanged);
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
      this.btnStartQL.Location = new System.Drawing.Point(274, 91);
      this.btnStartQL.Name = "btnStartQL";
      this.btnStartQL.Size = new System.Drawing.Size(135, 37);
      this.btnStartQL.TabIndex = 0;
      this.btnStartQL.Text = "Start Quake Live";
      this.btnStartQL.UseVisualStyleBackColor = false;
      this.btnStartQL.Click += new System.EventHandler(this.btnStartQL_Click);
      // 
      // rbUseSteam
      // 
      this.rbUseSteam.AutoSize = true;
      this.rbUseSteam.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.rbUseSteam.Location = new System.Drawing.Point(12, 108);
      this.rbUseSteam.Name = "rbUseSteam";
      this.rbUseSteam.Size = new System.Drawing.Size(63, 20);
      this.rbUseSteam.TabIndex = 3;
      this.rbUseSteam.TabStop = true;
      this.rbUseSteam.Text = "Steam";
      this.rbUseSteam.UseVisualStyleBackColor = true;
      this.rbUseSteam.CheckedChanged += new System.EventHandler(this.rbUseSteam_CheckedChanged);
      // 
      // lblUseSteam
      // 
      this.lblUseSteam.AutoSize = true;
      this.lblUseSteam.Location = new System.Drawing.Point(9, 90);
      this.lblUseSteam.Name = "lblUseSteam";
      this.lblUseSteam.Size = new System.Drawing.Size(191, 13);
      this.lblUseSteam.TabIndex = 2;
      this.lblUseSteam.Text = "How do you want to start Quake Live?";
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
      // linkConfig
      // 
      this.linkConfig.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.linkConfig.LinkArea = new System.Windows.Forms.LinkArea(0, 40);
      this.linkConfig.LinkColor = System.Drawing.Color.Gold;
      this.linkConfig.Location = new System.Drawing.Point(274, 3);
      this.linkConfig.Name = "linkConfig";
      this.linkConfig.Size = new System.Drawing.Size(136, 20);
      this.linkConfig.TabIndex = 1;
      this.linkConfig.TabStop = true;
      this.linkConfig.Text = "Open QL Config Folder";
      this.linkConfig.UseCompatibleTextRendering = true;
      this.linkConfig.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkConfig_LinkClicked);
      // 
      // cbLog
      // 
      this.cbLog.AutoSize = true;
      this.cbLog.Checked = true;
      this.cbLog.CheckState = System.Windows.Forms.CheckState.Checked;
      this.cbLog.Location = new System.Drawing.Point(95, 24);
      this.cbLog.Name = "cbLog";
      this.cbLog.Size = new System.Drawing.Size(43, 17);
      this.cbLog.TabIndex = 4;
      this.cbLog.Text = "Log";
      this.cbLog.UseVisualStyleBackColor = true;
      this.cbLog.CheckedChanged += new System.EventHandler(this.cbLog_CheckedChanged);
      // 
      // comboEmail
      // 
      this.comboEmail.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.comboEmail.ForeColor = System.Drawing.Color.Black;
      this.comboEmail.FormattingEnabled = true;
      this.comboEmail.Location = new System.Drawing.Point(9, 42);
      this.comboEmail.Name = "comboEmail";
      this.comboEmail.Size = new System.Drawing.Size(196, 21);
      this.comboEmail.TabIndex = 1;
      this.comboEmail.SelectedIndexChanged += new System.EventHandler(this.comboEmail_SelectedIndexChanged);
      this.comboEmail.KeyDown += new System.Windows.Forms.KeyEventHandler(this.comboEmail_KeyDown);
      // 
      // cbFocus
      // 
      this.cbFocus.AutoSize = true;
      this.cbFocus.Checked = true;
      this.cbFocus.CheckState = System.Windows.Forms.CheckState.Checked;
      this.cbFocus.Location = new System.Drawing.Point(12, 3);
      this.cbFocus.Name = "cbFocus";
      this.cbFocus.Size = new System.Drawing.Size(70, 17);
      this.cbFocus.TabIndex = 5;
      this.cbFocus.Text = "QL Focus";
      this.cbFocus.UseVisualStyleBackColor = false;
      this.cbFocus.CheckedChanged += new System.EventHandler(this.cbFocus_CheckedChanged);
      // 
      // linkAbout
      // 
      this.linkAbout.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.linkAbout.AutoSize = true;
      this.linkAbout.LinkArea = new System.Windows.Forms.LinkArea(0, 22);
      this.linkAbout.LinkColor = System.Drawing.Color.Gold;
      this.linkAbout.Location = new System.Drawing.Point(274, 23);
      this.linkAbout.Name = "linkAbout";
      this.linkAbout.Size = new System.Drawing.Size(115, 18);
      this.linkAbout.TabIndex = 2;
      this.linkAbout.TabStop = true;
      this.linkAbout.Text = "Open extraQL website";
      this.linkAbout.UseCompatibleTextRendering = true;
      this.linkAbout.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkAbout_LinkClicked);
      // 
      // cbAdvanced
      // 
      this.cbAdvanced.AutoSize = true;
      this.cbAdvanced.Checked = true;
      this.cbAdvanced.CheckState = System.Windows.Forms.CheckState.Checked;
      this.cbAdvanced.Location = new System.Drawing.Point(12, 24);
      this.cbAdvanced.Name = "cbAdvanced";
      this.cbAdvanced.Size = new System.Drawing.Size(63, 17);
      this.cbAdvanced.TabIndex = 3;
      this.cbAdvanced.Text = "Options";
      this.cbAdvanced.UseVisualStyleBackColor = true;
      this.cbAdvanced.CheckedChanged += new System.EventHandler(this.cbAdvanced_CheckedChanged);
      // 
      // txtPassword
      // 
      this.txtPassword.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.txtPassword.ForeColor = System.Drawing.Color.Black;
      this.txtPassword.Location = new System.Drawing.Point(217, 42);
      this.txtPassword.Name = "txtPassword";
      this.txtPassword.PasswordChar = '☺';
      this.txtPassword.Size = new System.Drawing.Size(179, 21);
      this.txtPassword.TabIndex = 3;
      this.txtPassword.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtPassword_KeyDown);
      this.txtPassword.Validating += new System.ComponentModel.CancelEventHandler(this.txtPassword_Validating);
      // 
      // lblPassword
      // 
      this.lblPassword.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.lblPassword.AutoSize = true;
      this.lblPassword.Location = new System.Drawing.Point(214, 24);
      this.lblPassword.Name = "lblPassword";
      this.lblPassword.Size = new System.Drawing.Size(106, 13);
      this.lblPassword.TabIndex = 2;
      this.lblPassword.Text = "Password (optional):";
      // 
      // lblEmail
      // 
      this.lblEmail.AutoSize = true;
      this.lblEmail.Location = new System.Drawing.Point(6, 24);
      this.lblEmail.Name = "lblEmail";
      this.lblEmail.Size = new System.Drawing.Size(88, 13);
      this.lblEmail.TabIndex = 0;
      this.lblEmail.Text = "E-Mail (optional):";
      // 
      // panelFocus
      // 
      this.panelFocus.BackColor = System.Drawing.Color.Transparent;
      this.panelFocus.Controls.Add(this.grpFocus);
      this.panelFocus.Dock = System.Windows.Forms.DockStyle.Top;
      this.panelFocus.Location = new System.Drawing.Point(0, 293);
      this.panelFocus.Name = "panelFocus";
      this.panelFocus.Size = new System.Drawing.Size(429, 102);
      this.panelFocus.TabIndex = 3;
      // 
      // grpFocus
      // 
      this.grpFocus.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.grpFocus.Controls.Add(this.comboRealm);
      this.grpFocus.Controls.Add(this.lblRealm);
      this.grpFocus.Controls.Add(this.linkFocusForum);
      this.grpFocus.Controls.Add(this.linkFocusLogin);
      this.grpFocus.ForeColor = System.Drawing.Color.White;
      this.grpFocus.Location = new System.Drawing.Point(12, 7);
      this.grpFocus.Name = "grpFocus";
      this.grpFocus.Size = new System.Drawing.Size(405, 84);
      this.grpFocus.TabIndex = 0;
      this.grpFocus.TabStop = false;
      this.grpFocus.Text = "QL Focus";
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
      // launcherPlayTimer
      // 
      this.launcherPlayTimer.Interval = 200;
      this.launcherPlayTimer.Tick += new System.EventHandler(this.launcherPlayTimer_Tick);
      // 
      // updateCheckTimer
      // 
      this.updateCheckTimer.Interval = 600000;
      this.updateCheckTimer.Tick += new System.EventHandler(this.updateCheckTimer_Tick);
      // 
      // panelLog
      // 
      this.panelLog.BackColor = System.Drawing.Color.Transparent;
      this.panelLog.Controls.Add(this.grpLog);
      this.panelLog.Dock = System.Windows.Forms.DockStyle.Right;
      this.panelLog.Location = new System.Drawing.Point(429, 0);
      this.panelLog.Name = "panelLog";
      this.panelLog.Size = new System.Drawing.Size(495, 670);
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
      this.grpLog.Size = new System.Drawing.Size(471, 652);
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
      this.txtLog.Size = new System.Drawing.Size(453, 588);
      this.txtLog.TabIndex = 3;
      // 
      // autoQuitTimer
      // 
      this.autoQuitTimer.Interval = 2000;
      this.autoQuitTimer.Tick += new System.EventHandler(this.autoQuitTimer_Tick);
      // 
      // panelAccount
      // 
      this.panelAccount.BackColor = System.Drawing.Color.Transparent;
      this.panelAccount.Controls.Add(this.grpAccount);
      this.panelAccount.Dock = System.Windows.Forms.DockStyle.Top;
      this.panelAccount.Location = new System.Drawing.Point(0, 199);
      this.panelAccount.Name = "panelAccount";
      this.panelAccount.Size = new System.Drawing.Size(429, 94);
      this.panelAccount.TabIndex = 2;
      // 
      // grpAccount
      // 
      this.grpAccount.Controls.Add(this.txtPassword);
      this.grpAccount.Controls.Add(this.lblEmail);
      this.grpAccount.Controls.Add(this.comboEmail);
      this.grpAccount.Controls.Add(this.lblPassword);
      this.grpAccount.ForeColor = System.Drawing.Color.White;
      this.grpAccount.Location = new System.Drawing.Point(12, 7);
      this.grpAccount.Name = "grpAccount";
      this.grpAccount.Size = new System.Drawing.Size(404, 75);
      this.grpAccount.TabIndex = 0;
      this.grpAccount.TabStop = false;
      this.grpAccount.Text = "Quake Live Account";
      // 
      // panelStart
      // 
      this.panelStart.BackColor = System.Drawing.Color.Transparent;
      this.panelStart.Controls.Add(this.linkAbout);
      this.panelStart.Controls.Add(this.cbAdvanced);
      this.panelStart.Controls.Add(this.cbFocus);
      this.panelStart.Controls.Add(this.cbLog);
      this.panelStart.Controls.Add(this.linkConfig);
      this.panelStart.Dock = System.Windows.Forms.DockStyle.Top;
      this.panelStart.Location = new System.Drawing.Point(0, 143);
      this.panelStart.Name = "panelStart";
      this.panelStart.Size = new System.Drawing.Size(429, 56);
      this.panelStart.TabIndex = 1;
      // 
      // MainForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
      this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
      this.ClientSize = new System.Drawing.Size(924, 670);
      this.ControlBox = false;
      this.Controls.Add(this.panelAdvanced);
      this.Controls.Add(this.panelFocus);
      this.Controls.Add(this.panelAccount);
      this.Controls.Add(this.panelStart);
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
      this.panelFocus.ResumeLayout(false);
      this.grpFocus.ResumeLayout(false);
      this.grpFocus.PerformLayout();
      this.mnuTrayIcon.ResumeLayout(false);
      this.panelLog.ResumeLayout(false);
      this.grpLog.ResumeLayout(false);
      this.grpLog.PerformLayout();
      this.panelAccount.ResumeLayout(false);
      this.grpAccount.ResumeLayout(false);
      this.grpAccount.PerformLayout();
      this.panelStart.ResumeLayout(false);
      this.panelStart.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private Label lblLauncherExe;
    private TextBox txtLauncherExe;
    private Button btnLauncherExe;
    private OpenFileDialog openFileDialog1;
    private Timer launcherDetectionTimer;
    private Label lblRealm;
    private Button btnInstallHook;
    private Panel panelAdvanced;
    private Panel panelTop;
    private CheckBox cbAdvanced;
    private TextBox txtPassword;
    private Label lblPassword;
    private Label lblEmail;
    private Button btnStartQL;
    private ComboBox comboRealm;
    private LinkLabel linkAbout;
    private LinkLabel linkFocusLogin;
    private LinkLabel linkFocusForum;
    private CheckBox cbDisableScripts;
    private GroupBox grpAdvanced;
    private CheckBox cbFocus;
    private Panel panelFocus;
    private GroupBox grpFocus;
    private ComboBox comboEmail;
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
    private CheckBox cbDownloadUpdates;
    private ToolStripSeparator quitToolStripMenuItem;
    private ToolStripMenuItem miQuit;
    private CheckBox cbAutostart;
    private CheckBox cbRunAsCommandLine;
    private Timer launcherPlayTimer;
    private Timer updateCheckTimer;
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
    private RadioButton rbUseLauncher;
    private RadioButton rbUseSteam;
    private Label lblUseSteam;
    private Panel panelAccount;
    private Panel panelStart;
    private GroupBox grpAccount;
    private TextBox txtSteamExe;
    private Label lblSteamExe;
    private Button btnSteamExe;
  }
}

