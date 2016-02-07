using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ExtraQL.Properties;
using Microsoft.Win32;

namespace ExtraQL
{
  public partial class MainForm : Form
  {
    public const string Version = "2.25";

    private readonly Config config;
    private readonly HttpServer server;
    private readonly Servlets servlets;
    private readonly ScriptRepository scriptRepository;
    private readonly Steamworks steam = new Steamworks();
    private bool qlStarted;
    private bool skipWorkshopNotice;
    private ulong steamAppId;
    private bool suppressInitialShow;
    private bool startupCompleted;
    private ulong steamClientId;

    private const int QuakeLiveAppId = 282440;
    private const int WorkshopExtraQL = 539252269;
    private const int WorkshopWebpakRussian = 550194516;
    private const int WorkshopWebpakCroatian = 553206606;
    private const int WorkshopWebpakGerman = 555806367;
    private const int WorkshopWebpakChinese = 555763644;

    #region ctor()
    public MainForm(Config config)
    {
      InitializeComponent();

      this.config = config;
      this.LoadSettings();
      base.Text = base.Text + " " + Version;
      this.lblVersion.Text = Version;
      this.lblExtra.Parent = this.picLogo;
      this.lblVersion.Parent = this.picLogo;
      this.trayIcon.Icon = this.Icon;

      this.server = new HttpServer(null);
      this.server.BindToAllInterfaces = false;
      this.server.LogAllRequests = this.cbLogAllRequests.Checked;

      this.scriptRepository = new ScriptRepository(config.AppBaseDir);
      this.scriptRepository.Log = this.Log;

      this.servlets = new Servlets(this.server, this.scriptRepository, this.Log, this, config.AppBaseDir, this.steam);
      this.UpdateServletSettings();

      this.miStartServerBrowser.Visible = this.GetServerBrowserExe() != null;
      this.FillAlternativeUis();

      this.ActiveControl = this.btnStartQL;

      // set current dir to .exe directory. Maybe that helps that steam_api.dll finds the steam_appid.txt file
      Environment.CurrentDirectory = config.AppBaseDir;

      this.suppressInitialShow = Environment.CommandLine.Contains(Program.BackgroundSwitch) || this.cbStartMinimized.Checked;
      if (this.suppressInitialShow)
      {
        this.WindowState = FormWindowState.Minimized;
        this.Startup(); // OnShown will never be executed when started minimized
      }
    }
    #endregion

    #region WndProc()
    protected override void WndProc(ref Message m)
    {
      // This hack is needed because none of the documented ways to start an application window in minimized state works.
      // this.ShowWindowWithoutActivation gets never called, this.WindowState get overwritten somehow after setting this.Visible=true, ...
      if (m.Msg == Win32.WM_SHOWWINDOW && this.suppressInitialShow && (int)m.WParam == 1)
      {
        m.Result = IntPtr.Zero;
        this.suppressInitialShow = false;
        return;
      }

      base.WndProc(ref m);
    }
    #endregion

    #region OnShown()
    protected override void OnShown(EventArgs e)
    {
      base.OnShown(e);
      this.Startup();
    }
    #endregion

    #region OnClosed()
    protected override void OnClosed(EventArgs e)
    {
      // make sure the window can be closed even if there are exceptions
      try { this.SaveSettings(); } catch { }
      try { this.server.Stop(); } catch { }
      try
      {
        if (this.cbCloseServerBrowser.Checked)
          this.CloseServerBrowser();
      }
      catch { }
      base.OnClosed(e);
    }
    #endregion

    // controls in basic view

    #region picLogo_Paint
    private void picLogo_Paint(object sender, PaintEventArgs e)
    {
      using (Font font = new Font("Tahoma", 28, FontStyle.Bold, GraphicsUnit.Point))
      using (SolidBrush textBrush = new SolidBrush(Color.White))
      using (SolidBrush shadowBrush = new SolidBrush(Color.FromArgb(128, 64, 64, 64)))
      {
        e.Graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
        Point p = this.lblExtra.Location;
        p.Offset(2, 2);
        e.Graphics.DrawString("extraQL", font, shadowBrush, p);
        e.Graphics.DrawString("extraQL", font, textBrush, this.lblExtra.Location);
      }
    }
    #endregion

    #region picLogo_MouseMove
    private void picLogo_MouseMove(object sender, MouseEventArgs e)
    {
      if ((e.Button & MouseButtons.Left) == 0)
        return;
      Win32.ReleaseCapture();
      Win32.SendMessage(this.Handle, Win32.WM_NCLBUTTONDOWN, Win32.HT_CAPTION, 0);
    }
    #endregion

    #region picMinimize_Click
    private void picMinimize_Click(object sender, EventArgs e)
    {
      this.SetFormVisibility(false);
    }
    #endregion

    #region picClose_Click
    private void picClose_Click(object sender, EventArgs e)
    {
      if (this.cbSystemTray.Checked)
        this.SetFormVisibility(false);
      else
        this.Close();
    }
    #endregion

    #region btnStartQL_Click
    private void btnStartQL_Click(object sender, EventArgs e)
    {
      this.Launch();
    }
    #endregion

    #region cbAdvanced_CheckedChanged
    private void cbAdvanced_CheckedChanged(object sender, EventArgs e)
    {
      if (this.cbOptions.Checked)
      {
        this.Height += panelOptions.Height;
        this.panelOptions.BringToFront();
        this.panelOptions.Visible = true;
      }
      else
      {
        this.panelOptions.Visible = false;
        this.Height -= panelOptions.Height;
      }
      this.panelScripts.Visible = this.cbOptions.Checked;
    }
    #endregion

    #region cbLog_CheckedChanged
    private void cbLog_CheckedChanged(object sender, EventArgs e)
    {
      this.SuspendLayout();
      this.panelRight.Visible = this.cbLog.Checked;
      if (!cbLog.Checked)
        this.Width -= this.panelRight.Width;
      else
        this.Width += this.panelRight.Width;
      this.ResumeLayout();
    }
    #endregion

    #region linkConfig_LinkClicked
    private void linkConfig_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      this.OpenConfigFolder(false);
    }
    #endregion

    #region linkExtraQlFolder_LinkClicked
    private void linkExtraQlFolder_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      this.OpenConfigFolder(true);
    }
    #endregion

    #region linkAbout_LinkClicked
    private void linkAbout_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      Process.Start("https://github.com/PredatH0r/extraQL/wiki");
    }
    #endregion

    // controls in the UI selection screen

    #region linkRussian_LinkClicked
    private void linkRussian_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      Process.Start("steam://url/CommunityFilePage/" + WorkshopWebpakRussian);
    }
    #endregion

    #region linkCroatian_LinkClicked
    private void linkCroatian_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      Process.Start("steam://url/CommunityFilePage/" + WorkshopWebpakCroatian);
    }
    #endregion

    #region linkGerman_LinkClicked
    private void linkGerman_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      Process.Start("steam://url/CommunityFilePage/" + WorkshopWebpakGerman);
    }
    #endregion

    #region linkChinese_LinkClicked
    private void linkChinese_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      Process.Start("steam://url/CommunityFilePage/" + WorkshopWebpakChinese);
    }
    #endregion

    // controls in Options view

    #region btnNickStart_Click
    private void btnNickStart_Click(object sender, EventArgs e)
    {
      servlets.SetSteamNick(this.txtNickStart.Text);
    }
    #endregion

    #region  btnNickEnd_Click
    private void btnNickEnd_Click(object sender, EventArgs e)
    {
      servlets.SetSteamNick(this.txtNickEnd.Text);
    }
    #endregion

    #region btnSteamExe_Click
    private void btnSteamExe_Click(object sender, EventArgs e)
    {
      string path = "";
      string file = "quakelive_steam.exe";
      try { path = Path.GetDirectoryName(this.txtSteamExe.Text); }
      catch { }
      try { file = Path.GetFileName(this.txtSteamExe.Text); }
      catch { }
      this.openFileDialog1.FileName = file;
      this.openFileDialog1.InitialDirectory = path;
      if (this.openFileDialog1.ShowDialog(this) == DialogResult.OK)
        this.txtSteamExe.Text = this.openFileDialog1.FileName;
    }
    #endregion
    
    #region cbSystemTray_CheckedChanged
    private void cbSystemTray_CheckedChanged(object sender, EventArgs e)
    {
      this.ShowInTaskbar = !this.cbSystemTray.Checked;
      this.trayIcon.Visible = cbSystemTray.Checked;
      this.picMinimize.Visible = !this.cbSystemTray.Checked;
      if (this.picMinimize.Visible)
        this.picMinimize.BringToFront();
    }
    #endregion

    #region cbStartServerBrowser_CheckedChanged
    private void cbStartServerBrowser_CheckedChanged(object sender, EventArgs e)
    {
      if (this.cbStartServerBrowser.Checked)
        this.StartServerBrowser();
    }
    #endregion

    // controls in Log view

    #region cbLogAllRequests_CheckedChanged
    private void cbLogAllRequests_CheckedChanged(object sender, EventArgs e)
    {
      if (this.server != null)
        this.server.LogAllRequests = this.cbLogAllRequests.Checked;
    }
    #endregion

    #region btnClearLog_Click
    private void btnClearLog_Click(object sender, EventArgs e)
    {
      this.txtLog.Clear();
    }
    #endregion

    #region lbScripts_SelectedIndexChanged
    private void lbScripts_SelectedIndexChanged(object sender, EventArgs e)
    {
      var script = (ScriptInfo) this.lbScripts.SelectedItem;
      this.txtScriptDescription.Text = script?.Metadata.Get("description", "\r\n") ?? "";
      this.txtScriptAuthor.Text = script?.Metadata.Get("author", ", ") ?? "";
      this.txtScriptVersion.Text = script?.Metadata.Get("version") ?? "";
    }
    #endregion

    // controls in System Tray

    #region trayIcon_MouseUp
    private void trayIcon_MouseUp(object sender, MouseEventArgs e)
    {
      if (e.Button == MouseButtons.Left)
      {
        this.SetFormVisibility(true);
        this.BringToFront();
        this.Activate();
      }
    }
    #endregion

    #region miOpenExtraQl_Click
    private void miOpenExtraQl_Click(object sender, EventArgs e)
    {
      this.SetFormVisibility(true);
    }
    #endregion

    #region miStartQL_Click
    private void miStartQL_Click(object sender, EventArgs e)
    {
      this.Launch();
    }
    #endregion

    #region miStartServerBrowser_Click()
    private void miStartServerBrowser_Click(object sender, EventArgs e)
    {
      this.StartServerBrowser();
    }
    #endregion

    #region miQuit_Click
    private void miQuit_Click(object sender, EventArgs e)
    {
      this.Close();
    }
    #endregion

    // timers

    #region autoQuitTimer_Tick
    private void autoQuitTimer_Tick(object sender, EventArgs e)
    {
      bool running = Servlets.QLWindowHandle != IntPtr.Zero;

      if (running)
        qlStarted = true;
      else if (qlStarted)
        servlets.SetSteamNick(this.txtNickEnd.Text);

      if (qlStarted && !running)
      {
        if (this.cbAutoQuit.Checked)
          this.Close();
        if (this.cbCloseServerBrowser.Checked)
          this.CloseServerBrowser();
      }
      this.qlStarted = running;
    }
    #endregion



    #region LoadSettings()
    private void LoadSettings()
    {
      this.txtSteamExe.Text = config.GetString("quakelive_steam.exe");
      this.txtNickStart.Text = config.GetString("nickQuake");
      this.txtNickEnd.Text = config.GetString("nickSteam");
      this.cbOptions.Checked = config.GetBool("advanced");
      this.cbSystemTray.Checked = config.GetBool("systemTray");
      this.cbStartMinimized.Checked = config.GetBool("startMinimized");
      this.cbAutostart.Checked = config.GetString("autostart") != "0";
      this.cbLog.Checked = config.GetBool("log");
      this.cbFollowLog.Checked = config.GetBool("followLog");
      this.cbLogAllRequests.Checked = config.GetBool("logAllRequests");
      this.cbAutoQuit.Checked = config.GetBool("autoquit");
      this.skipWorkshopNotice = config.GetBool("skipWorkshopNotice");
      this.cbStartServerBrowser.Checked = config.GetBool("startServerBrowser");
      this.cbCloseServerBrowser.Checked = config.GetBool("closeServerBrowser");
      
      // these settings are not written back to the .ini
      ulong.TryParse(config.GetString("steamAppId"), out this.steamAppId);
      var regex = new Regex(@"(?:^|\s|/|-)steamappid=(\d+)(?:\s|$)", RegexOptions.IgnoreCase);
      var match = regex.Match(Environment.CommandLine);
      if (match.Success)
        ulong.TryParse(match.Groups[1].Value, out this.steamAppId);

      ulong.TryParse(config.GetString("steamId"), out this.steamClientId);
      regex = new Regex(@"(?:^|\s|/|-)steamid=(\d+)(?:\s|$)", RegexOptions.IgnoreCase);
      match = regex.Match(Environment.CommandLine);
      if (match.Success)
        ulong.TryParse(match.Groups[1].Value, out this.steamClientId);
    }

    #endregion

    #region SaveSettings()
    private void SaveSettings()
    {
      try
      {
        config.Set("quakelive_steam.exe", this.txtSteamExe.Text);
        config.Set("nickQuake", this.txtNickStart.Text);
        config.Set("nickSteam", this.txtNickEnd.Text);
        config.Set("advanced", this.cbOptions.Checked);
        config.Set("systemTray", this.cbSystemTray.Checked);
        config.Set("startMinimized", this.cbStartMinimized.Checked);
        config.Set("autostart", this.cbAutostart.Checked ? "1" : "0");
        config.Set("log", this.cbLog.Checked);
        config.Set("followLog", this.cbFollowLog.Checked);
        config.Set("logAllRequests", this.cbLogAllRequests.Checked);
        config.Set("autoquit", this.cbAutoQuit.Checked);
        config.Set("skipWorkshopNotice", this.skipWorkshopNotice);
        config.Set("startServerBrowser", this.cbStartServerBrowser.Checked ? "1" : "0");
        config.Set("closeServerBrowser", this.cbCloseServerBrowser.Checked ? "1" : "0");
        config.Set("webpakWorkshopItem", ((QuakeLiveWebPak) this.comboWebPak.SelectedItem)?.WorkshopId.ToString() ?? "0");

        SaveScriptConfig();

        config.SaveSettings();
      }
      catch (Exception ex)
      {
        MessageBox.Show(this, "An error occured while saving your settings:\n" + ex, "Saving settings", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private void SaveScriptConfig()
    {
      for (int i = 0; i < this.lbScripts.Items.Count; i++)
      {
        var script = (ScriptInfo) this.lbScripts.Items[i];
        config.SetScriptState(script.Id, this.lbScripts.GetItemChecked(i));
      }
    }

    #endregion

    #region Startup()
    private void Startup()
    {
      if (this.startupCompleted)
        return;

      this.scriptRepository.RegisterScripts();
      this.RestartHttpServer();
      this.CheckIfStartedFromWorkshopFolder();
      this.FillScriptList();

      this.StartSteamClient();
      if (this.cbAutostart.Checked)
        this.Launch();

      this.startupCompleted = true;
    }
    #endregion

    #region OnSecondInstanceStarted()
    private void OnSecondInstanceStarted()
    {
      if (this.cbAutostart.Checked)
        this.Launch();
      else
        this.SetFormVisibility(true);
    }
    #endregion

    #region CheckIfStartedFromWorkshopFolder()
    private void CheckIfStartedFromWorkshopFolder()
    {
      if (this.skipWorkshopNotice)
        return;
      var exeDir = (Path.GetDirectoryName(Application.ExecutablePath) ?? "").ToLower().Replace("/", "\\").TrimEnd('\\');
      var wsDir = this.GetSteamWorkshopPath().ToLower().TrimEnd('\\');
      if (exeDir != wsDir)
      {
        var answer = MessageBox.Show(this,
          "extraQL 2.x is now a Steam Workshop item.\n" +
          "To keep extraQL updated, delete your old extraQL directory,\n" +
          "subscribe to the Workshop item and update your desktop shortcut.\n\n" +
          "Do you want to open the Steam Workshop page now?",
          "extraQL 2.x Steam Workshop",
          MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        this.skipWorkshopNotice = ModifierKeys == Keys.Control;
        if (answer == DialogResult.Yes)
          Process.Start("steam://url/CommunityFilePage/" + WorkshopExtraQL);
      }
    }
    #endregion

    #region Log()

    private delegate void Action();

    private void Log(string msg)
    {
      if (String.IsNullOrEmpty(msg))
        return;

      if (this.InvokeRequired)
        this.BeginInvoke((Action) (() => this.Log(msg)));
      else
      {
        var text = this.txtLog.Text;
        if (text.Length > 10000) // truncate log after 10k chars
          text = ""; 
        this.txtLog.Text = text + "[" + DateTime.Now.ToString("T") + "] " + msg + "\r\n";
        if (this.cbFollowLog.Checked)
        {
          this.txtLog.SelectionStart = this.txtLog.TextLength;
          this.txtLog.SelectionLength = 0;
          this.txtLog.ScrollToCaret();
        }
      }
    }
    #endregion

    #region UpdateServletSettings()
    private void UpdateServletSettings()
    {
      if (this.servlets != null)
      {
        this.servlets.QuakeConfigFolder = this.GetConfigFolder();
        this.servlets.QuakeSteamFolder = this.GetQuakeLivePath();
        if (this.steamAppId != 0)
          this.servlets.SteamAppId = this.steamAppId;
        this.servlets.BringToFrontHandler = this.OnSecondInstanceStarted;
      }
    }
    #endregion

    #region RestartHttpServer()
    private void RestartHttpServer()
    {
      if (this.server == null)
        return;
      this.server.Stop();
      this.server.BindToAllInterfaces = false;
      this.servlets.EnablePrivateServlets = true;
      if (this.server.Start())
        this.Log("extraQL server listening on " + this.server.EndPointUrl);
      else
        this.Log("extraQL server failed to start on " + this.server.EndPointUrl +". Scripts are disabled!");
    }
    #endregion

    #region OpenConfigFolder()
    private void OpenConfigFolder(bool extraQlConfig)
    {
      var dir = extraQlConfig ? config.AppBaseDir : this.GetConfigFolder();
      if (dir != null)
        Process.Start("explorer.exe", "/e," + dir);
    }
    #endregion

    #region FillAlternativeUis()

    private void FillAlternativeUis()
    {
      this.comboWebPak.Items.Clear();
      var ws = this.GetSteamWorkshopPath();
      if (ws == null) return;
      ws = Path.GetDirectoryName(ws) ?? Environment.CurrentDirectory;

      long curUi;
      long.TryParse(this.config.GetString("webpakWorkshopItem"), out curUi);

      var options = new List<QuakeLiveWebPak>();
      foreach (var wsdir in Directory.GetDirectories(ws))
      {
        var dir = wsdir;
        long workshopId;
        if (!long.TryParse(Path.GetFileName(dir), out workshopId))
          continue;

        if (File.Exists(dir + @"\baseq3\web\bundle.js"))
          dir += @"\baseq3";

        if (!File.Exists(dir + @"\web\bundle.js"))
          continue;

        var descr = dir + @"\description.txt";
        descr = File.Exists(descr) ? File.ReadAllText(descr).Trim() 
          : workshopId == WorkshopWebpakRussian ? "русский (Russian)" 
          : workshopId == WorkshopWebpakCroatian ? "Hrvatski (Croation)"
          : workshopId == WorkshopWebpakGerman ? "Deutsch (German)"
          : workshopId == WorkshopWebpakChinese ? "Traditional Chinese"
          : workshopId.ToString();
        options.Add(new QuakeLiveWebPak(workshopId, descr, dir + @"\web"));
      }
      options.Sort();
      options.Insert(0, new QuakeLiveWebPak(0, "default web.pak (English)", ""));

      var web = this.GetQuakeLivePath() + @"\web";
      if (File.Exists(web + "\\bundle.js"))
        options.Insert(1, new QuakeLiveWebPak(1, "extracted web.pak folder", web));

      foreach (var option in options)
      {
        this.comboWebPak.Items.Add(option);
        if (option.WorkshopId == curUi)
          this.comboWebPak.SelectedIndex = this.comboWebPak.Items.Count - 1;
      }
    }

    #endregion

    #region FillScriptList()
    private void FillScriptList()
    {
      this.lbScripts.Items.Clear();
      var list = new List<ScriptInfo>(this.scriptRepository.GetScripts());
      list.Sort((a,b) => a.Name.CompareTo(b.Name));
      foreach (var script in list)
      {
        bool enabled = config.GetScriptState(script.Id) ?? script.Metadata.Get("enabled") != "0";
        this.lbScripts.Items.Add(script, enabled);
      }
    }
    #endregion

    #region StartSteamClient()
    private void StartSteamClient()
    {
      if (!steam.IsSteamRunning())
      {
        this.btnStartQL.Enabled = false;
        Log("starting Steam Client...");
        Process.Start("steam://preload/" + QuakeLiveAppId);
        for (int i = 0; i < 600; i++)
        {
          if (steam.IsSteamRunning())
            break;
          System.Threading.Thread.Sleep(100);
          Application.DoEvents();
        }

        // wait 7.5sec to the Steam Client GUI to be fully loaded. Otherwise trying to start QL would fail.
        for (int i = 0; i < 75; i++)
        {
          System.Threading.Thread.Sleep(100);
          Application.DoEvents();
        }          
      }

      if (this.steamClientId == 0)
        this.steamClientId = steam.GetUserID();

      if (this.steamClientId != 0)
      {
        this.Log("Using QL config folder for steam ID " + this.steamClientId);
        this.btnStartQL.Enabled = true;
      }
      else
      {
        if (Environment.GetEnvironmentVariable("SteamAppId") != null)
          this.Log("WARNING: extraQL was started as a Steam Library item.\r\nThis prevents it from using the Steam API, which is required to detect your Steam-ID or change your nickname. Please use a regular Windows shortcut to start extraQL.exe.");
        this.Log("Unable to auto-detect your steam ID (needed for the QL config folder). You can manually set steamId=... in extraQL.ini or as a command line argument.");
      }
    }
    #endregion

    #region Launch()

    private void Launch()
    {
      SaveSettings();
      if (!InstallScripts())
        return;
      UpdateServletSettings();
      servlets.SetSteamNick(this.txtNickStart.Text);
      if ((ModifierKeys & Keys.Control) == 0) // use ctrl+Start button just re-installs the scripts (during development)
      {
        if (cbStartServerBrowser.Checked)
          StartServerBrowser();
        StartQuakeLive();
      }
    }
    #endregion

    #region InstallScripts()

    private bool InstallScripts(bool force = false)
    {
      const bool installScriptsInQlFolder = false;

      // make sure we know quake's home directory
      string path = this.GetQuakeLivePath();
      if (path == null)
      {
        this.Log("Unable to locate quakelive_steam.exe and the Quake Live baseq3 directory");
        MessageBox.Show(this, "extraQL was not able to find your quakelive_steam.exe file.\nPlease provide the correct location in the Options pane.",
          "extraQL", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        return false;
      }

      this.SaveScriptConfig();

      // modify files in <steam-user-id>\baseq3 directories
      var dirs = Directory.GetDirectories(path);
      foreach (var dir in dirs)
      {
        if (Regex.IsMatch(Path.GetFileName(dir) ?? "", "\\d{5,}"))
        {
          InstallConfigFiles(dir + @"\baseq3\");
          InstallJavaScripts(dir, installScriptsInQlFolder);
        }
      }

      // modify files in the steam workshop folder
      var workshopFolder = this.GetSteamWorkshopPath();
      InstallJavaScripts(workshopFolder, !installScriptsInQlFolder);
      return true;
    }
    #endregion

    #region InstallConfigFiles()
    private void InstallConfigFiles(string baseq3Path)
    {
      try
      {
        // delete obsolete extraQL 1.x stuff
        File.Delete(baseq3Path + "hook.js");
        File.Delete(baseq3Path + "hook_.js");
        File.Delete(baseq3Path + "gameendcfg"); // typo in 2.3
        File.Delete(scriptRepository.ScriptDir + "hook.js");
        File.Delete(scriptRepository.ScriptDir + "extraQL.js");
        foreach (var oldScript in Directory.GetFiles(scriptRepository.ScriptDir, "*.usr.js"))
          File.Delete(oldScript);

        // create informational gamestart.cfg and gameend.cfg files for autoExec.js
        var file = baseq3Path + "gamestart.cfg";
        if (!File.Exists(file))
          File.WriteAllText(file, "// this file will be executed by extraQL/autoExec.js every time a map is loaded\n// you can use commands like /steamnick <nickname> to change your steam nickname when you enter a game.");
        file = baseq3Path + "gameend.cfg";
        if (!File.Exists(file))
          File.WriteAllText(file, "// this file will be executed by extraQL/autoExec.js every time a map is unloaded\n// you can use commands like /steamnick <nickname> to change your steam nickname when you enter a game.");

        // create ibounce_on.cfg for InstaBounce gametype
        file = baseq3Path + "ibounce_on.cfg";
        if (!File.Exists(file))
          File.WriteAllText(file, @"// extraQL InstaBounce game type config
alias +hook 'weapon 10; wait; wait; +attack'
alias -hook '-attack; weapon 7'
alias +rock 'weapon 5; wait; wait; +attack'
alias -rock '-attack; weapon 7'
seta cl_preferredStartingWeapons '7'

// uncomment/edit the lines below

//seta cg_disableInstaBounceBindMsg 1
bind mouse2 +rock
bind mouse4 +hook
bind mouse5 +hook
".Replace('\'', '"'));
      }
      catch (Exception ex)
      {
        // some like to make their config directory read-only. bad luck.
        this.Log("Unable to update files in Quake Live's <steam-id>/baseq3: " + ex.Message);
      }
    }
    #endregion

    #region InstallJavaScripts()
    private void InstallJavaScripts(string folder, bool install)
    {
      var jsFolder = Path.Combine(folder, @"baseq3\js");
      try
      {
        Directory.CreateDirectory(jsFolder);
        foreach (var script in scriptRepository.GetScripts())
        {
          bool inst = install && (this.config.GetScriptState(script.Id) ?? true);
          var targetFile = Path.Combine(jsFolder, Path.GetFileName(script.Filepath) ?? "");
          if (inst)
            File.Copy(script.Filepath, targetFile, true);
          else
            File.Delete(targetFile);
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show(this, "Unable to update userscripts\nfrom: " + scriptRepository.ScriptDir + "\nto: " + jsFolder + "\n\nError: " + ex.Message,
          "extraQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }
    #endregion

    #region GetQuakeLivePath()

    private delegate string StrFunc();

    private string GetQuakeLivePath()
    {
      var methods = new StrFunc[] {GetQuakeLivePathFromTextField, GetQuakeLivePathFromExtraQlPath, GetQuakeLivePathFromRegisty };

      foreach (var method in methods)
      {
        try
        {
          var path = method();
          if (File.Exists(path))
          {
            path = Path.GetDirectoryName(path);
            return path;
          }
        }
        catch
        {
        }
      }
      return null;
    }

    private string GetQuakeLivePathFromTextField()
    {
      if (this.txtSteamExe.Text != "")
      {
        var path = this.txtSteamExe.Text;
        if (Directory.Exists(path))
        {
          if (!path.EndsWith("/") && !path.EndsWith("\\"))
            path += "quakelive_steam.exe";
        }
        return path;
      }
      return null;
    }

    private string GetQuakeLivePathFromExtraQlPath()
    {
      var path = this.GetType().Assembly.Location;
      for (int i = 0; i < 5; i++)
        path = Path.GetDirectoryName(path) ?? "";
      return Path.Combine(path, @"common\Quake Live\quakelive_steam.exe");
    }

    private string GetQuakeLivePathFromRegisty()
    {
      var path = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Valve\Steam", "SteamPath", null) as string;
      if (path != null)
        path = path.Replace("/", "\\") + @"\SteamApps\Common\Quake Live\quakelive_steam.exe";
      return path;
    }

    #endregion

    #region GetConfigFolder()
    private string GetConfigFolder()
    {
      var baseq3 = this.GetQuakeLivePath();
      if (baseq3 == null)
        return null;

      // use SteamID from steamworks API when possible
      if (this.steamClientId != 0 && Directory.Exists(Path.Combine(baseq3, this.steamClientId.ToString())))
        return Path.Combine(Path.Combine(baseq3, this.steamClientId.ToString()), "baseq3");

      // pick the first directory that looks like a SteamID
      var dirs = Directory.GetDirectories(baseq3);
      foreach (var dir in dirs)
      {
        if (Regex.IsMatch(Path.GetFileName(dir) ?? "", "\\d{5,}"))
        {
          var path = Path.Combine(dir, "baseq3");
          return path;
        }
      }
      return null;
    }
    #endregion

    #region GetSteamWorkshopPath()
    private string GetSteamWorkshopPath()
    {
      var qlPath = this.GetQuakeLivePath();
      if (qlPath == null)
        return null;
      var path = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(qlPath)) ?? "", @"workshop\content\" + QuakeLiveAppId + "\\" + WorkshopExtraQL);
      return path;
    }
    #endregion

    #region StartQuakeLive()
    private void StartQuakeLive()
    {
      var procList = Process.GetProcessesByName("quakelive_steam");
      if (procList.Length > 0)
      {
        // bring existing QL window to front and activate it
        var hWnd = procList[0].MainWindowHandle;
        Win32.ShowWindow(hWnd, Win32.SW_SHOWNORMAL);
        Win32.SetForegroundWindow(hWnd);
        Win32.SetCapture(hWnd);
        Win32.SetFocus(hWnd);
        Win32.SetActiveWindow(hWnd);
      }
      else
      {
        var args = "";
        if (!PrepareAlternativeQuakeLiveUI(ref args))
          return;
        this.Log("Starting Quake Live...");
        Process.Start("steam://rungameid/" + QuakeLiveAppId + args);
      }
      this.SetFormVisibility(false);
    }
    #endregion

    #region PrepareAlternativeQuakeLiveUI()
    private bool PrepareAlternativeQuakeLiveUI(ref string args)
    {
      try
      {
        var webpak = (QuakeLiveWebPak) this.comboWebPak.SelectedItem;
        var js = this.GetConfigFolder() + @"\js\fs_webpath.js";
        File.Delete(js);
        if (webpak.WorkshopId != 0)
        {
          if (!this.config.GetBool("ignoreStaleWebPak"))
          {
            var fiDir = new FileInfo(Path.Combine(webpak.Path, "bundle.js"));
            var fiPak = new FileInfo(Path.Combine(this.GetQuakeLivePath(), "web.pak"));
            if (fiPak.LastWriteTimeUtc > fiDir.LastWriteTimeUtc)
            {
              var choice = MessageBox.Show(this,
                Resources.MainForm_StaleWebPak, 
                "extraQL", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
              if (choice == DialogResult.Cancel)
                return false;
              if (choice == DialogResult.Yes)
              {
                this.comboWebPak.SelectedIndex = 0;
                return true;
              }
              if ((ModifierKeys & (Keys.Shift | Keys.Control | Keys.Alt)) != 0)
                this.config.Set("ignoreStaleWebPak", true);
            }
          }
          // adding command line args works, but steam shows a popup in that case. So instead we use a JS file to change fs_webpath
          //args = System.Web.HttpUtility.UrlPathEncode("//+set fs_webpath \"" + webpak.Path + "\"");

          File.WriteAllText(js, @"
// extraQL script to activate an alternative Quake Live UI (if fs_webpath was not set on the command line)
(function() {
var path = '" + webpak.Path.Replace("\\", "\\\\") + @"';
if (qz_instance.GetCvar('fs_webpath') != path) {
  qz_instance.SetCvar('fs_webpath', path);
  qz_instance.SendGameCommand('web_reload');
  setTimeout(function() {
    qz_instance.SendGameCommand('echo ^3fs_webpath.js:^7 activating customized UI...^7');
  }, 0);
}
})();
");
        }
      }
      catch (Exception ex)
      {
        Log("Failed to set alternative Quake Live UI: " + ex.Message);
      }
      return true;
    }

    #endregion

    #region SetFormVisiblity()
    private void SetFormVisibility(bool visible)
    {
      if (visible)
      {
        this.Show();
        this.WindowState = FormWindowState.Normal;
        this.BringToFront();
        this.Activate();
      }
      else
      {
        if (this.cbSystemTray.Checked)
          this.Hide();
        else
          this.WindowState = FormWindowState.Minimized;
      }
    }
    #endregion

    #region GetServerBrowserExe()
    private string GetServerBrowserExe()
    {
      var wsPath = this.GetSteamWorkshopPath();
      wsPath = Path.Combine(Path.GetDirectoryName(wsPath) ?? ".", @"543312745\ServerBrowser.exe");
      return File.Exists(wsPath) ? wsPath : null;
    }
    #endregion

    #region StartServerBrowser()
    private void StartServerBrowser()
    {
      var proc = Process.GetProcessesByName("ServerBrowser");
      if (proc.Length == 0)
      {
        var exe = this.GetServerBrowserExe();
        if (exe != null)
          Process.Start(exe);
        else
          Log("Could not find ServerBrowser.exe.\nMake sure you have steam workshop item 543312745 installed.");
      }
      else
        Win32.ShowWindow(proc[0].MainWindowHandle, 1);
    }
    #endregion

    #region CloseServerBrowser()
    private void CloseServerBrowser()
    {
      var proc = Process.GetProcessesByName("ServerBrowser");
      foreach (var p in proc)
        Win32.SendMessage(p.MainWindowHandle, Win32.WM_CLOSE, 0, 0);
    }
    #endregion
  }

  #region class QuakeLiveWebPak
  class QuakeLiveWebPak : IComparable<QuakeLiveWebPak>
  {
    public readonly long WorkshopId;
    public readonly string Description;
    public readonly string Path;

    public QuakeLiveWebPak(long workshopId, string description, string path)
    {
      this.WorkshopId = workshopId;
      this.Description = description;
      this.Path = path;
    }

    public int CompareTo(QuakeLiveWebPak other)
    {
      var c = this.Description.CompareTo(other.Description);
      if (c != 0) return c;
      return this.WorkshopId.CompareTo(other.WorkshopId);
    }

    public override string ToString()
    {
      return this.Description;
    }
  }
  #endregion
}