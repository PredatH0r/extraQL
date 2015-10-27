﻿using System;
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
    public const string Version = "2.0.1";

    private readonly Config config;
    private readonly HttpServer server;
    private readonly Servlets servlets;
    private readonly ScriptRepository scriptRepository;
    private bool autoQuitQlIsRunning;

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

      this.server = new HttpServer(config.AppBaseDir + @"\https\localhost.pfx");
      this.server.BindToAllInterfaces = this.cbBindToAll.Checked;
      this.server.UseHttps = this.cbHttps.Checked;
      this.server.LogAllRequests = this.cbLogAllRequests.Checked;

      this.scriptRepository = new ScriptRepository(config.AppBaseDir);
      this.scriptRepository.Log = this.Log;

      this.servlets = new Servlets(this.server, this.scriptRepository, this.Log, this, config.AppBaseDir);
      this.UpdateServletSettings();

      this.ActiveControl = this.btnStartQL;
    }
    #endregion


    #region OnShown()
    protected override void OnShown(EventArgs e)
    {
      base.OnShown(e);

      this.scriptRepository.RegisterScripts();
      this.RestartHttpServer();
      this.UpdateServletSettings();
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

    #region cbAutoQuit_CheckedChanged()
    private void cbAutoQuit_CheckedChanged(object sender, EventArgs e)
    {
      if (this.cbAutoQuit.Checked)
        this.autoQuitQlIsRunning = false;
      this.autoQuitTimer.Enabled = this.cbAutoQuit.Checked;
    }
    #endregion

    #region cbBindAll_CheckedChanged
    private void cbBindAll_CheckedChanged(object sender, EventArgs e)
    {
      this.RestartHttpServer();
    }
    #endregion

    #region cbHttps_CheckedChanged
    private void cbHttps_CheckedChanged(object sender, EventArgs e)
    {
      this.RestartHttpServer();
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
      if (this.autoQuitQlIsRunning && !running)
        this.Close();
      else if (running)
        this.autoQuitQlIsRunning = true;
    }
    #endregion



    #region LoadSettings()
    private void LoadSettings()
    {
      this.txtSteamExe.Text = config.GetString("quakelive_steam.exe");

      this.cbAdvanced.Checked = config.GetBool("advanced");
      this.cbBindToAll.Checked = config.GetBool("bindToAll");
      this.cbSystemTray.Checked = config.GetBool("systemTray");
      this.cbStartMinimized.Checked = config.GetBool("startMinimized");
      if (this.cbStartMinimized.Checked)
        this.SetFormVisibility(false);
      this.cbAutostart.Checked = config.GetString("autostart") != "0";
      this.cbLog.Checked = config.GetBool("log");
      this.cbFollowLog.Checked = config.GetBool("followLog");
      this.cbHttps.Checked = config.GetBool("https");
      this.cbLogAllRequests.Checked = config.GetBool("logAllRequests");
      this.cbAutoQuit.Checked = config.GetBool("autoquit");
    }

    #endregion

    #region SaveSettings()
    private void SaveSettings()
    {
      try
      {
        config.Set("quakelive_steam.exe", this.txtSteamExe.Text);
        config.Set("advanced", this.cbAdvanced.Checked);
        config.Set("bindToAll", this.cbBindToAll.Checked);
        config.Set("systemTray", this.cbSystemTray.Checked);
        config.Set("startMinimized", this.cbStartMinimized.Checked);
        config.Set("autostart", this.cbAutostart.Checked ? "1" : "0");
        config.Set("log", this.cbLog.Checked);
        config.Set("followLog", this.cbFollowLog.Checked);
        config.Set("https", this.cbHttps.Checked);
        config.Set("logAllRequests", this.cbLogAllRequests.Checked);
        config.Set("autoquit", this.cbAutoQuit.Checked);
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
      var exeDir = Path.GetDirectoryName(Application.ExecutablePath).ToLower().Replace("/", "\\").TrimEnd('\\');
      var wsDir = this.GetSteamWorkshopPath().ToLower().TrimEnd('\\');
      if (exeDir != wsDir)
      {
        var answer = MessageBox.Show(this,
          "extraQL 2.0 is now a Steam Workshop item.\n" +
          "To keep extraQL updated, delete your old extraQL directory,\n" +
          "subscribe to the Workshop item and update your desktop shortcut.\n\n" +
          "Do you want to open the Steam Workshop page now?",
          "extraQL 2.0 Steam Workshop",
          MessageBoxButtons.YesNo, MessageBoxIcon.Question);
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
      }
    }
    #endregion

    #region RestartHttpServer()
    private void RestartHttpServer()
    {
      if (this.server == null)
        return;
      this.server.Stop();
      this.server.BindToAllInterfaces = this.cbBindToAll.Checked;
      this.server.UseHttps = this.cbHttps.Checked;
      this.servlets.EnablePrivateServlets = !this.cbBindToAll.Checked;
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
          return dir + "\\baseq3";
      }
      return null;
    }
    #endregion

    #region Launch()

    private void Launch()
    {
      SaveSettings();
      InstallScripts();
      StartQuakeLive();
    }
    #endregion

    #region InstallScripts()

    private void InstallScripts(bool force = false)
    {
      string path = this.GetQuakeLivePath();
      if (path == null)
      {
        this.Log("Unable to detect Quake Live's baseq3 directory");
        return;
      }

      // copy hook.js to all <steam-user-id>\baseq3 directories
      var dirs = Directory.GetDirectories(path);
      foreach (var dir in dirs)
      {
        if (Regex.IsMatch(Path.GetFileName(dir) ?? "", "\\d{5,}"))
          InstallScripts(dir + "\\baseq3\\", force);
      }
    }

    private void InstallScripts(string baseq3Path, bool force)
    {
      // delete obsolete extraQL 1.x stuff
      File.Delete(baseq3Path + "hook.js");
      File.Delete(baseq3Path + "hook_.js");
      File.Delete(scriptRepository.ScriptDir + "hook.js");
      File.Delete(scriptRepository.ScriptDir + "extraQL.js");
      foreach (var oldScript in Directory.GetFiles(scriptRepository.ScriptDir, "*.usr.js"))
        File.Delete(oldScript);

      // install new scripts
      var ws = this.GetSteamWorkshopPath();
      var wsScripts = Path.Combine(ws, @"baseq3\js");
      try
      {
        Directory.CreateDirectory(wsScripts);
        foreach(var script in scriptRepository.GetScripts())
          File.Copy(script.Filepath, Path.Combine(wsScripts, Path.GetFileName(script.Filepath) ?? ""), true);
      }
      catch (Exception ex)
      {
        MessageBox.Show(this, "Unable to copy userscripts\nfrom: " + scriptRepository.ScriptDir + "\nto: " + wsScripts + "\n\nError: " + ex.Message,
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
          path = Path.GetDirectoryName(this.txtSteamExe.Text) ?? "";
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
          path = path.Replace("/", "\\") + @"\SteamApps\Common\Quake Live\";
      }

      return path != null && Directory.Exists(path) ? path : null;
    }

    #endregion

    #region GetSteamWorkshopPath()
    private string GetSteamWorkshopPath()
    {
      return Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(this.GetQuakeLivePath()))) ?? "", @"workshop\content\282440\539252269\");
    }
    #endregion

    #region StartQuakeLive()
    private void StartQuakeLive()
    {
      this.Log("Starting Quake Live Steam App...");
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
        if (this.cbSystemTray.Checked)
          this.Hide();
        else
          this.WindowState = FormWindowState.Minimized;
      }
    }
    #endregion
  }
}