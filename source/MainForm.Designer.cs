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
      this.panelOptions = new System.Windows.Forms.Panel();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.lblSteamNameStart = new System.Windows.Forms.Label();
      this.btnNickEnd = new System.Windows.Forms.Button();
      this.txtNickStart = new System.Windows.Forms.TextBox();
      this.lblSteamNameEnd = new System.Windows.Forms.Label();
      this.txtNickEnd = new System.Windows.Forms.TextBox();
      this.btnNickStart = new System.Windows.Forms.Button();
      this.grpWebPak = new System.Windows.Forms.GroupBox();
      this.linkChinese = new System.Windows.Forms.LinkLabel();
      this.linkGerman = new System.Windows.Forms.LinkLabel();
      this.comboWebPak = new System.Windows.Forms.ComboBox();
      this.lblTranslations = new System.Windows.Forms.Label();
      this.linkCroatian = new System.Windows.Forms.LinkLabel();
      this.linkRussian = new System.Windows.Forms.LinkLabel();
      this.lblWebPak = new System.Windows.Forms.Label();
      this.lblCustomizedVersions = new System.Windows.Forms.Label();
      this.grpOptions = new System.Windows.Forms.GroupBox();
      this.cbCloseServerBrowser = new System.Windows.Forms.CheckBox();
      this.cbStartServerBrowser = new System.Windows.Forms.CheckBox();
      this.txtSteamExe = new System.Windows.Forms.TextBox();
      this.lblSteamExe = new System.Windows.Forms.Label();
      this.btnSteamExe = new System.Windows.Forms.Button();
      this.cbAutoQuit = new System.Windows.Forms.CheckBox();
      this.cbAutostart = new System.Windows.Forms.CheckBox();
      this.cbStartMinimized = new System.Windows.Forms.CheckBox();
      this.cbSystemTray = new System.Windows.Forms.CheckBox();
      this.panelTop = new System.Windows.Forms.Panel();
      this.cbLog = new System.Windows.Forms.CheckBox();
      this.cbOptions = new System.Windows.Forms.CheckBox();
      this.linkOpenExtraQLWebsite = new System.Windows.Forms.LinkLabel();
      this.btnStartQL = new System.Windows.Forms.Button();
      this.picClose = new System.Windows.Forms.PictureBox();
      this.picMinimize = new System.Windows.Forms.PictureBox();
      this.lblVersion = new System.Windows.Forms.Label();
      this.linkOpenConfigFolder = new System.Windows.Forms.LinkLabel();
      this.lblExtra = new System.Windows.Forms.Label();
      this.picLogo = new System.Windows.Forms.PictureBox();
      this.trayIcon = new System.Windows.Forms.NotifyIcon(this.components);
      this.mnuTrayIcon = new System.Windows.Forms.ContextMenuStrip(this.components);
      this.miOpenExtraQl = new System.Windows.Forms.ToolStripMenuItem();
      this.miStartQL = new System.Windows.Forms.ToolStripMenuItem();
      this.miStartServerBrowser = new System.Windows.Forms.ToolStripMenuItem();
      this.quitToolStripMenuItem = new System.Windows.Forms.ToolStripSeparator();
      this.miQuit = new System.Windows.Forms.ToolStripMenuItem();
      this.panelLog = new System.Windows.Forms.Panel();
      this.grpLog = new System.Windows.Forms.GroupBox();
      this.cbLogAllRequests = new System.Windows.Forms.CheckBox();
      this.btnClearLog = new System.Windows.Forms.Button();
      this.cbFollowLog = new System.Windows.Forms.CheckBox();
      this.txtLog = new System.Windows.Forms.TextBox();
      this.autoQuitTimer = new System.Windows.Forms.Timer(this.components);
      this.panelOptions.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.grpWebPak.SuspendLayout();
      this.grpOptions.SuspendLayout();
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
      resources.ApplyResources(this.openFileDialog1, "openFileDialog1");
      this.openFileDialog1.RestoreDirectory = true;
      // 
      // panelOptions
      // 
      this.panelOptions.BackColor = System.Drawing.Color.Transparent;
      this.panelOptions.Controls.Add(this.groupBox1);
      this.panelOptions.Controls.Add(this.grpWebPak);
      this.panelOptions.Controls.Add(this.grpOptions);
      resources.ApplyResources(this.panelOptions, "panelOptions");
      this.panelOptions.Name = "panelOptions";
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.lblSteamNameStart);
      this.groupBox1.Controls.Add(this.btnNickEnd);
      this.groupBox1.Controls.Add(this.txtNickStart);
      this.groupBox1.Controls.Add(this.lblSteamNameEnd);
      this.groupBox1.Controls.Add(this.txtNickEnd);
      this.groupBox1.Controls.Add(this.btnNickStart);
      this.groupBox1.ForeColor = System.Drawing.Color.White;
      resources.ApplyResources(this.groupBox1, "groupBox1");
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.TabStop = false;
      // 
      // lblSteamNameStart
      // 
      resources.ApplyResources(this.lblSteamNameStart, "lblSteamNameStart");
      this.lblSteamNameStart.Name = "lblSteamNameStart";
      // 
      // btnNickEnd
      // 
      resources.ApplyResources(this.btnNickEnd, "btnNickEnd");
      this.btnNickEnd.BackColor = System.Drawing.SystemColors.ButtonFace;
      this.btnNickEnd.ForeColor = System.Drawing.Color.Black;
      this.btnNickEnd.Name = "btnNickEnd";
      this.btnNickEnd.TabStop = false;
      this.btnNickEnd.UseVisualStyleBackColor = false;
      this.btnNickEnd.Click += new System.EventHandler(this.btnNickEnd_Click);
      // 
      // txtNickStart
      // 
      resources.ApplyResources(this.txtNickStart, "txtNickStart");
      this.txtNickStart.ForeColor = System.Drawing.Color.Black;
      this.txtNickStart.Name = "txtNickStart";
      // 
      // lblSteamNameEnd
      // 
      resources.ApplyResources(this.lblSteamNameEnd, "lblSteamNameEnd");
      this.lblSteamNameEnd.Name = "lblSteamNameEnd";
      // 
      // txtNickEnd
      // 
      resources.ApplyResources(this.txtNickEnd, "txtNickEnd");
      this.txtNickEnd.ForeColor = System.Drawing.Color.Black;
      this.txtNickEnd.Name = "txtNickEnd";
      // 
      // btnNickStart
      // 
      resources.ApplyResources(this.btnNickStart, "btnNickStart");
      this.btnNickStart.BackColor = System.Drawing.SystemColors.ButtonFace;
      this.btnNickStart.ForeColor = System.Drawing.Color.Black;
      this.btnNickStart.Name = "btnNickStart";
      this.btnNickStart.TabStop = false;
      this.btnNickStart.UseVisualStyleBackColor = false;
      this.btnNickStart.Click += new System.EventHandler(this.btnNickStart_Click);
      // 
      // grpWebPak
      // 
      resources.ApplyResources(this.grpWebPak, "grpWebPak");
      this.grpWebPak.Controls.Add(this.linkChinese);
      this.grpWebPak.Controls.Add(this.linkGerman);
      this.grpWebPak.Controls.Add(this.comboWebPak);
      this.grpWebPak.Controls.Add(this.lblTranslations);
      this.grpWebPak.Controls.Add(this.linkCroatian);
      this.grpWebPak.Controls.Add(this.linkRussian);
      this.grpWebPak.Controls.Add(this.lblWebPak);
      this.grpWebPak.Controls.Add(this.lblCustomizedVersions);
      this.grpWebPak.ForeColor = System.Drawing.Color.White;
      this.grpWebPak.Name = "grpWebPak";
      this.grpWebPak.TabStop = false;
      // 
      // linkChinese
      // 
      resources.ApplyResources(this.linkChinese, "linkChinese");
      this.linkChinese.LinkColor = System.Drawing.Color.Gold;
      this.linkChinese.Name = "linkChinese";
      this.linkChinese.TabStop = true;
      this.linkChinese.UseCompatibleTextRendering = true;
      this.linkChinese.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkChinese_LinkClicked);
      // 
      // linkGerman
      // 
      resources.ApplyResources(this.linkGerman, "linkGerman");
      this.linkGerman.LinkColor = System.Drawing.Color.Gold;
      this.linkGerman.Name = "linkGerman";
      this.linkGerman.TabStop = true;
      this.linkGerman.UseCompatibleTextRendering = true;
      this.linkGerman.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkGerman_LinkClicked);
      // 
      // comboWebPak
      // 
      this.comboWebPak.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboWebPak.FormattingEnabled = true;
      resources.ApplyResources(this.comboWebPak, "comboWebPak");
      this.comboWebPak.Name = "comboWebPak";
      // 
      // lblTranslations
      // 
      resources.ApplyResources(this.lblTranslations, "lblTranslations");
      this.lblTranslations.Name = "lblTranslations";
      // 
      // linkCroatian
      // 
      resources.ApplyResources(this.linkCroatian, "linkCroatian");
      this.linkCroatian.LinkColor = System.Drawing.Color.Gold;
      this.linkCroatian.Name = "linkCroatian";
      this.linkCroatian.TabStop = true;
      this.linkCroatian.UseCompatibleTextRendering = true;
      this.linkCroatian.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkCroatian_LinkClicked);
      // 
      // linkRussian
      // 
      resources.ApplyResources(this.linkRussian, "linkRussian");
      this.linkRussian.LinkColor = System.Drawing.Color.Gold;
      this.linkRussian.Name = "linkRussian";
      this.linkRussian.TabStop = true;
      this.linkRussian.UseCompatibleTextRendering = true;
      this.linkRussian.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkRussian_LinkClicked);
      // 
      // lblWebPak
      // 
      resources.ApplyResources(this.lblWebPak, "lblWebPak");
      this.lblWebPak.Name = "lblWebPak";
      // 
      // lblCustomizedVersions
      // 
      resources.ApplyResources(this.lblCustomizedVersions, "lblCustomizedVersions");
      this.lblCustomizedVersions.Name = "lblCustomizedVersions";
      // 
      // grpOptions
      // 
      resources.ApplyResources(this.grpOptions, "grpOptions");
      this.grpOptions.Controls.Add(this.cbCloseServerBrowser);
      this.grpOptions.Controls.Add(this.cbStartServerBrowser);
      this.grpOptions.Controls.Add(this.txtSteamExe);
      this.grpOptions.Controls.Add(this.lblSteamExe);
      this.grpOptions.Controls.Add(this.btnSteamExe);
      this.grpOptions.Controls.Add(this.cbAutoQuit);
      this.grpOptions.Controls.Add(this.cbAutostart);
      this.grpOptions.Controls.Add(this.cbStartMinimized);
      this.grpOptions.Controls.Add(this.cbSystemTray);
      this.grpOptions.ForeColor = System.Drawing.Color.White;
      this.grpOptions.Name = "grpOptions";
      this.grpOptions.TabStop = false;
      // 
      // cbCloseServerBrowser
      // 
      resources.ApplyResources(this.cbCloseServerBrowser, "cbCloseServerBrowser");
      this.cbCloseServerBrowser.Name = "cbCloseServerBrowser";
      this.cbCloseServerBrowser.UseVisualStyleBackColor = true;
      // 
      // cbStartServerBrowser
      // 
      resources.ApplyResources(this.cbStartServerBrowser, "cbStartServerBrowser");
      this.cbStartServerBrowser.Name = "cbStartServerBrowser";
      this.cbStartServerBrowser.UseVisualStyleBackColor = true;
      this.cbStartServerBrowser.CheckedChanged += new System.EventHandler(this.cbStartServerBrowser_CheckedChanged);
      // 
      // txtSteamExe
      // 
      resources.ApplyResources(this.txtSteamExe, "txtSteamExe");
      this.txtSteamExe.ForeColor = System.Drawing.Color.Black;
      this.txtSteamExe.Name = "txtSteamExe";
      // 
      // lblSteamExe
      // 
      resources.ApplyResources(this.lblSteamExe, "lblSteamExe");
      this.lblSteamExe.Name = "lblSteamExe";
      // 
      // btnSteamExe
      // 
      resources.ApplyResources(this.btnSteamExe, "btnSteamExe");
      this.btnSteamExe.BackColor = System.Drawing.SystemColors.ButtonFace;
      this.btnSteamExe.ForeColor = System.Drawing.Color.Black;
      this.btnSteamExe.Name = "btnSteamExe";
      this.btnSteamExe.TabStop = false;
      this.btnSteamExe.UseVisualStyleBackColor = false;
      this.btnSteamExe.Click += new System.EventHandler(this.btnSteamExe_Click);
      // 
      // cbAutoQuit
      // 
      resources.ApplyResources(this.cbAutoQuit, "cbAutoQuit");
      this.cbAutoQuit.Name = "cbAutoQuit";
      this.cbAutoQuit.UseVisualStyleBackColor = true;
      // 
      // cbAutostart
      // 
      resources.ApplyResources(this.cbAutostart, "cbAutostart");
      this.cbAutostart.Name = "cbAutostart";
      this.cbAutostart.UseVisualStyleBackColor = true;
      // 
      // cbStartMinimized
      // 
      resources.ApplyResources(this.cbStartMinimized, "cbStartMinimized");
      this.cbStartMinimized.Name = "cbStartMinimized";
      this.cbStartMinimized.UseVisualStyleBackColor = true;
      // 
      // cbSystemTray
      // 
      resources.ApplyResources(this.cbSystemTray, "cbSystemTray");
      this.cbSystemTray.Name = "cbSystemTray";
      this.cbSystemTray.UseVisualStyleBackColor = true;
      this.cbSystemTray.CheckedChanged += new System.EventHandler(this.cbSystemTray_CheckedChanged);
      // 
      // panelTop
      // 
      this.panelTop.BackColor = System.Drawing.Color.Transparent;
      this.panelTop.Controls.Add(this.cbLog);
      this.panelTop.Controls.Add(this.cbOptions);
      this.panelTop.Controls.Add(this.linkOpenExtraQLWebsite);
      this.panelTop.Controls.Add(this.btnStartQL);
      this.panelTop.Controls.Add(this.picClose);
      this.panelTop.Controls.Add(this.picMinimize);
      this.panelTop.Controls.Add(this.lblVersion);
      this.panelTop.Controls.Add(this.linkOpenConfigFolder);
      this.panelTop.Controls.Add(this.lblExtra);
      this.panelTop.Controls.Add(this.picLogo);
      resources.ApplyResources(this.panelTop, "panelTop");
      this.panelTop.Name = "panelTop";
      // 
      // cbLog
      // 
      resources.ApplyResources(this.cbLog, "cbLog");
      this.cbLog.Checked = true;
      this.cbLog.CheckState = System.Windows.Forms.CheckState.Checked;
      this.cbLog.Name = "cbLog";
      this.cbLog.UseVisualStyleBackColor = true;
      this.cbLog.CheckedChanged += new System.EventHandler(this.cbLog_CheckedChanged);
      // 
      // cbOptions
      // 
      resources.ApplyResources(this.cbOptions, "cbOptions");
      this.cbOptions.Checked = true;
      this.cbOptions.CheckState = System.Windows.Forms.CheckState.Checked;
      this.cbOptions.Name = "cbOptions";
      this.cbOptions.UseVisualStyleBackColor = true;
      this.cbOptions.CheckedChanged += new System.EventHandler(this.cbAdvanced_CheckedChanged);
      // 
      // linkOpenExtraQLWebsite
      // 
      resources.ApplyResources(this.linkOpenExtraQLWebsite, "linkOpenExtraQLWebsite");
      this.linkOpenExtraQLWebsite.LinkColor = System.Drawing.Color.Gold;
      this.linkOpenExtraQLWebsite.Name = "linkOpenExtraQLWebsite";
      this.linkOpenExtraQLWebsite.TabStop = true;
      this.linkOpenExtraQLWebsite.UseCompatibleTextRendering = true;
      this.linkOpenExtraQLWebsite.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkAbout_LinkClicked);
      // 
      // btnStartQL
      // 
      resources.ApplyResources(this.btnStartQL, "btnStartQL");
      this.btnStartQL.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
      this.btnStartQL.FlatAppearance.BorderSize = 2;
      this.btnStartQL.ForeColor = System.Drawing.Color.Gold;
      this.btnStartQL.Name = "btnStartQL";
      this.btnStartQL.UseVisualStyleBackColor = false;
      this.btnStartQL.Click += new System.EventHandler(this.btnStartQL_Click);
      // 
      // picClose
      // 
      resources.ApplyResources(this.picClose, "picClose");
      this.picClose.Name = "picClose";
      this.picClose.TabStop = false;
      this.picClose.Click += new System.EventHandler(this.picClose_Click);
      // 
      // picMinimize
      // 
      resources.ApplyResources(this.picMinimize, "picMinimize");
      this.picMinimize.Name = "picMinimize";
      this.picMinimize.TabStop = false;
      this.picMinimize.Click += new System.EventHandler(this.picMinimize_Click);
      // 
      // lblVersion
      // 
      resources.ApplyResources(this.lblVersion, "lblVersion");
      this.lblVersion.Name = "lblVersion";
      this.lblVersion.MouseMove += new System.Windows.Forms.MouseEventHandler(this.picLogo_MouseMove);
      // 
      // linkOpenConfigFolder
      // 
      resources.ApplyResources(this.linkOpenConfigFolder, "linkOpenConfigFolder");
      this.linkOpenConfigFolder.LinkColor = System.Drawing.Color.Gold;
      this.linkOpenConfigFolder.Name = "linkOpenConfigFolder";
      this.linkOpenConfigFolder.TabStop = true;
      this.linkOpenConfigFolder.UseCompatibleTextRendering = true;
      this.linkOpenConfigFolder.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkConfig_LinkClicked);
      // 
      // lblExtra
      // 
      resources.ApplyResources(this.lblExtra, "lblExtra");
      this.lblExtra.Name = "lblExtra";
      this.lblExtra.MouseMove += new System.Windows.Forms.MouseEventHandler(this.picLogo_MouseMove);
      // 
      // picLogo
      // 
      this.picLogo.Cursor = System.Windows.Forms.Cursors.SizeAll;
      resources.ApplyResources(this.picLogo, "picLogo");
      this.picLogo.Name = "picLogo";
      this.picLogo.TabStop = false;
      this.picLogo.Paint += new System.Windows.Forms.PaintEventHandler(this.picLogo_Paint);
      this.picLogo.MouseMove += new System.Windows.Forms.MouseEventHandler(this.picLogo_MouseMove);
      // 
      // trayIcon
      // 
      resources.ApplyResources(this.trayIcon, "trayIcon");
      this.trayIcon.ContextMenuStrip = this.mnuTrayIcon;
      this.trayIcon.MouseUp += new System.Windows.Forms.MouseEventHandler(this.trayIcon_MouseUp);
      // 
      // mnuTrayIcon
      // 
      this.mnuTrayIcon.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miOpenExtraQl,
            this.miStartQL,
            this.miStartServerBrowser,
            this.quitToolStripMenuItem,
            this.miQuit});
      this.mnuTrayIcon.Name = "contextMenuStrip1";
      this.mnuTrayIcon.ShowImageMargin = false;
      resources.ApplyResources(this.mnuTrayIcon, "mnuTrayIcon");
      // 
      // miOpenExtraQl
      // 
      this.miOpenExtraQl.Name = "miOpenExtraQl";
      resources.ApplyResources(this.miOpenExtraQl, "miOpenExtraQl");
      this.miOpenExtraQl.Click += new System.EventHandler(this.miOpenExtraQl_Click);
      // 
      // miStartQL
      // 
      this.miStartQL.Name = "miStartQL";
      resources.ApplyResources(this.miStartQL, "miStartQL");
      this.miStartQL.Click += new System.EventHandler(this.miStartQL_Click);
      // 
      // miStartServerBrowser
      // 
      this.miStartServerBrowser.Name = "miStartServerBrowser";
      resources.ApplyResources(this.miStartServerBrowser, "miStartServerBrowser");
      this.miStartServerBrowser.Click += new System.EventHandler(this.miStartServerBrowser_Click);
      // 
      // quitToolStripMenuItem
      // 
      this.quitToolStripMenuItem.Name = "quitToolStripMenuItem";
      resources.ApplyResources(this.quitToolStripMenuItem, "quitToolStripMenuItem");
      // 
      // miQuit
      // 
      this.miQuit.Name = "miQuit";
      resources.ApplyResources(this.miQuit, "miQuit");
      this.miQuit.Click += new System.EventHandler(this.miQuit_Click);
      // 
      // panelLog
      // 
      this.panelLog.BackColor = System.Drawing.Color.Transparent;
      this.panelLog.Controls.Add(this.grpLog);
      resources.ApplyResources(this.panelLog, "panelLog");
      this.panelLog.Name = "panelLog";
      // 
      // grpLog
      // 
      resources.ApplyResources(this.grpLog, "grpLog");
      this.grpLog.Controls.Add(this.cbLogAllRequests);
      this.grpLog.Controls.Add(this.btnClearLog);
      this.grpLog.Controls.Add(this.cbFollowLog);
      this.grpLog.Controls.Add(this.txtLog);
      this.grpLog.ForeColor = System.Drawing.Color.White;
      this.grpLog.Name = "grpLog";
      this.grpLog.TabStop = false;
      // 
      // cbLogAllRequests
      // 
      resources.ApplyResources(this.cbLogAllRequests, "cbLogAllRequests");
      this.cbLogAllRequests.Name = "cbLogAllRequests";
      this.cbLogAllRequests.UseVisualStyleBackColor = true;
      this.cbLogAllRequests.CheckedChanged += new System.EventHandler(this.cbLogAllRequests_CheckedChanged);
      // 
      // btnClearLog
      // 
      this.btnClearLog.BackColor = System.Drawing.SystemColors.ButtonFace;
      this.btnClearLog.ForeColor = System.Drawing.Color.Black;
      resources.ApplyResources(this.btnClearLog, "btnClearLog");
      this.btnClearLog.Name = "btnClearLog";
      this.btnClearLog.UseVisualStyleBackColor = false;
      this.btnClearLog.Click += new System.EventHandler(this.btnClearLog_Click);
      // 
      // cbFollowLog
      // 
      resources.ApplyResources(this.cbFollowLog, "cbFollowLog");
      this.cbFollowLog.Name = "cbFollowLog";
      this.cbFollowLog.UseVisualStyleBackColor = true;
      // 
      // txtLog
      // 
      resources.ApplyResources(this.txtLog, "txtLog");
      this.txtLog.ForeColor = System.Drawing.Color.Black;
      this.txtLog.Name = "txtLog";
      this.txtLog.ReadOnly = true;
      // 
      // autoQuitTimer
      // 
      this.autoQuitTimer.Enabled = true;
      this.autoQuitTimer.Interval = 2000;
      this.autoQuitTimer.Tick += new System.EventHandler(this.autoQuitTimer_Tick);
      // 
      // MainForm
      // 
      resources.ApplyResources(this, "$this");
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
      this.ControlBox = false;
      this.Controls.Add(this.panelOptions);
      this.Controls.Add(this.panelTop);
      this.Controls.Add(this.panelLog);
      this.DoubleBuffered = true;
      this.ForeColor = System.Drawing.Color.White;
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
      this.MaximizeBox = false;
      this.Name = "MainForm";
      this.panelOptions.ResumeLayout(false);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.grpWebPak.ResumeLayout(false);
      this.grpWebPak.PerformLayout();
      this.grpOptions.ResumeLayout(false);
      this.grpOptions.PerformLayout();
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
    private Panel panelOptions;
    private Panel panelTop;
    private CheckBox cbOptions;
    private Button btnStartQL;
    private LinkLabel linkOpenExtraQLWebsite;
    private GroupBox grpOptions;
    private PictureBox picLogo;
    private Label lblVersion;
    private Label lblExtra;
    private PictureBox picClose;
    private PictureBox picMinimize;
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
    private CheckBox cbLogAllRequests;
    private CheckBox cbAutoQuit;
    private Timer autoQuitTimer;
    private LinkLabel linkOpenConfigFolder;
    private TextBox txtSteamExe;
    private Label lblSteamExe;
    private Button btnSteamExe;
    private TextBox txtNickEnd;
    private Label lblSteamNameEnd;
    private TextBox txtNickStart;
    private Button btnNickEnd;
    private Button btnNickStart;
    private CheckBox cbCloseServerBrowser;
    private CheckBox cbStartServerBrowser;
    private ToolStripMenuItem miOpenExtraQl;
    private ToolStripMenuItem miStartServerBrowser;
    private GroupBox grpWebPak;
    private Label lblCustomizedVersions;
    private Label lblTranslations;
    private LinkLabel linkCroatian;
    private LinkLabel linkRussian;
    private Label lblWebPak;
    private ComboBox comboWebPak;
    private GroupBox groupBox1;
    private Label lblSteamNameStart;
    private LinkLabel linkGerman;
    private LinkLabel linkChinese;
  }
}

