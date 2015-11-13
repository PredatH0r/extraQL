using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.Win32;

namespace ExtraQL
{
  public partial class MainForm : Form
  {
    public const string Version = "2.7";

    private readonly Config config;
    private readonly HttpServer server;
    private readonly Servlets servlets;
    private readonly ScriptRepository scriptRepository;
    private bool qlStarted;
    private bool skipWorkshopNotice;
    private int steamAppId;
    private bool suppressInitialShow;

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
      this.suppressInitialShow = Environment.CommandLine.Contains(Program.BackgroundSwitch) || this.cbStartMinimized.Checked;
      if (this.suppressInitialShow)
        this.WindowState = FormWindowState.Minimized;

      this.server = new HttpServer(null);
      this.server.BindToAllInterfaces = false;
      this.server.LogAllRequests = this.cbLogAllRequests.Checked;

      this.scriptRepository = new ScriptRepository(config.AppBaseDir);
      this.scriptRepository.Log = this.Log;

      this.servlets = new Servlets(this.server, this.scriptRepository, this.Log, this, config.AppBaseDir);
      this.UpdateServletSettings();

      this.ActiveControl = this.btnStartQL;
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

      this.scriptRepository.RegisterScripts();
      this.RestartHttpServer();
      this.CheckIfStartedFromWorkshopFolder();
      if (this.cbAutostart.Checked)
        this.Launch();
    }
    #endregion

    #region OnClosed()
    protected override void OnClosed(EventArgs e)
    {
      // make sure the window can be closed even if there are exceptions
      try { this.SaveSettings(); } catch { }
      try { this.server.Stop(); } catch { }
      try { this.servlets.Dispose(); } catch { }
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

    #region linkConfig_LinkClicked
    private void linkConfig_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      this.OpenConfigFolder();
    }
    #endregion

    #region cbAdvanced_CheckedChanged
    private void cbAdvanced_CheckedChanged(object sender, EventArgs e)
    {
      if (this.cbAdvanced.Checked)
      {
        this.Height += panelAdvanced.Height;
        this.panelAdvanced.BringToFront();
        this.panelAdvanced.Visible = true;
      }
      else
      {
        this.panelAdvanced.Visible = false;
        this.Height -= panelAdvanced.Height;
      }
    }
    #endregion

    #region cbLog_CheckedChanged
    private void cbLog_CheckedChanged(object sender, EventArgs e)
    {
      this.SuspendLayout();
      this.panelLog.Visible = this.cbLog.Checked;
      if (!cbLog.Checked)
        this.Width -= this.panelLog.Width;
      else
        this.Width += this.panelLog.Width;
      this.ResumeLayout();
    }
    #endregion

    #region linkAbout_LinkClicked
    private void linkAbout_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      Process.Start("https://github.com/PredatH0r/extraQL/wiki");
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

    #region miStartQL_Click
    private void miStartQL_Click(object sender, EventArgs e)
    {
      this.Launch();
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
      this.cbAdvanced.Checked = config.GetBool("advanced");
      this.cbSystemTray.Checked = config.GetBool("systemTray");
      this.cbStartMinimized.Checked = config.GetBool("startMinimized");
      this.cbAutostart.Checked = config.GetString("autostart") != "0";
      this.cbLog.Checked = config.GetBool("log");
      this.cbFollowLog.Checked = config.GetBool("followLog");
      this.cbLogAllRequests.Checked = config.GetBool("logAllRequests");
      this.cbAutoQuit.Checked = config.GetBool("autoquit");
      this.skipWorkshopNotice = config.GetBool("skipWorkshopNotice");
      int.TryParse(config.GetString("steamAppId"), out this.steamAppId);
      this.cbStartServerBrowser.Checked = config.GetBool("startServerBrowser");
      this.cbCloseServerBrowser.Checked = config.GetBool("closeServerBrowser");
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
        config.Set("advanced", this.cbAdvanced.Checked);
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
        config.SaveSettings();
      }
      catch (Exception ex)
      {
        MessageBox.Show(this, "An error occured while saving your settings:\n" + ex, "Saving settings", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
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
          Process.Start("steam://url/CommunityFilePage/539252269");
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
    private void OpenConfigFolder()
    {
      var dir = this.GetConfigFolder();
      if (dir != null)
        Process.Start("explorer.exe", "/e," + dir);
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
          var targetFile = Path.Combine(jsFolder, Path.GetFileName(script.Filepath) ?? "");
          if (install)
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
    private string GetQuakeLivePath()
    {
      string path;
      if (this.txtSteamExe.Text != "")
      {
        try
        {
          path = this.txtSteamExe.Text;
          if (Directory.Exists(path))
          {
            if (!path.EndsWith("/") && !path.EndsWith("\\"))
              path += "quakelive_steam.exe";
          }
        }
        catch
        {
          return null;
        }
      }
      else
      {
        path = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Valve\Steam", "SteamPath", null) as string;
        if (path != null)
          path = path.Replace("/", "\\") + @"\SteamApps\Common\Quake Live\quakelive_steam.exe";
      }

      if (!File.Exists(path))
        return null;
      path = Path.GetDirectoryName(path);
      //Log("Quake Live folder: " + path);
      return path;
    }

    #endregion

    #region GetConfigFolder()
    private string GetConfigFolder()
    {
      var baseq3 = this.GetQuakeLivePath();
      if (baseq3 == null)
        return null;

      var dirs = Directory.GetDirectories(baseq3);
      foreach (var dir in dirs)
      {
        if (Regex.IsMatch(Path.GetFileName(dir) ?? "", "\\d{5,}"))
        {
          var path = Path.Combine(dir, "baseq3");
          //Log("Config folder: " + path);
          return path;
        }
      }
      return null;
    }
    #endregion

    #region GetSteamWorkshopPath()
    private string GetSteamWorkshopPath()
    {
      var path = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(this.GetQuakeLivePath())) ?? "", @"workshop\content\282440\539252269");
      //Log("Workshop folder: " + path);
      return path;
    }
    #endregion

    #region StartQuakeLive()
    private void StartQuakeLive()
    {
      this.Log("Starting Quake Live...");
      Process.Start("steam://rungameid/282440");
      this.SetFormVisibility(false);
    }
    #endregion

    #region SetFormVisiblity()
    private void SetFormVisibility(bool visible)
    {
      if (visible)
      {
        this.WindowState = FormWindowState.Normal;
        this.Show();
      }
      else
      {
        this.WindowState = FormWindowState.Minimized;
        if (this.cbSystemTray.Checked)
          this.Hide();
      }
    }
    #endregion

    #region StartServerBrowser()
    private void StartServerBrowser()
    {
      var proc = Process.GetProcessesByName("ServerBrowser");
      if (proc.Length == 0)
      {
        var wsPath = this.GetSteamWorkshopPath();
        wsPath = Path.Combine(Path.GetDirectoryName(wsPath) ?? ".", @"543312745\ServerBrowser.exe");
        if (File.Exists(wsPath))
          Process.Start(wsPath);
        else
          Log("Could not find " + wsPath + ".\nMake sure you have steam workshop item 543312745 installed.");
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
}