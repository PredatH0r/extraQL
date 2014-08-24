using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace ExtraQL
{
  public partial class MainForm : Form
  {
    public const string Version = "1.7";

    private readonly Config config;
    private readonly Updater updater;
    private int timerCount;
    private readonly HttpServer server;
    private readonly Servlets servlets;
    private readonly ScriptRepository scriptRepository;
    private bool autoQuitQlIsRunning;

    #region ctor()
    public MainForm(Config config, Updater updater)
    {
      InitializeComponent();

      this.config = config;
      this.updater = updater;
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

      this.ActiveControl = this.comboEmail;
    }
    #endregion


    #region OnShown()
    protected override void OnShown(EventArgs e)
    {
      base.OnShown(e);

      this.scriptRepository.RegisterScripts();
      this.RestartHttpServer();
      this.CheckForUpdate();

      if (this.cbAutostartLauncher.Checked)
        this.Launch(false);
      else if (this.cbAutostartSteam.Checked)
        this.Launch(true);
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
      this.WindowState = FormWindowState.Minimized;
    }
    #endregion

    #region picClose_Click
    private void picClose_Click(object sender, EventArgs e)
    {
      this.Close();
    }
    #endregion

    #region comboEmail_SelectedIndexChanged
    private void comboEmail_SelectedIndexChanged(object sender, EventArgs e)
    {
      string pwd;
      this.config.Accounts.TryGetValue(this.comboEmail.Text, out pwd);
      this.txtPassword.Text = pwd ?? "";
    }
    #endregion

    #region comboEmail_KeyDown
    private void comboEmail_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.KeyData == Keys.Enter)
      {
        this.txtPassword.Select();
        e.Handled = true;
      }
    }
    #endregion

    #region txtPassword_KeyDown
    private void txtPassword_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.KeyData == Keys.Enter)
      {
        this.btnStartLauncher.Select();
        e.Handled = true;
      }
    }
    #endregion

    #region txtPassword_Validating
    private void txtPassword_Validating(object sender, CancelEventArgs e)
    {
      if (this.comboEmail.Text.Length > 0)
        this.config.Accounts[this.comboEmail.Text] = this.txtPassword.Text;
    }
    #endregion

    #region btnStartLauncher_Click
    private void btnStartLauncher_Click(object sender, EventArgs e)
    {
      this.Launch(false);
    }
    #endregion

    #region btnStartSteam_Click
    private void btnStartSteam_Click(object sender, EventArgs e)
    {
      this.Launch(true);
    }
    #endregion

    #region cbFocus_CheckedChanged
    private void cbFocus_CheckedChanged(object sender, EventArgs e)
    {
      this.panelFocus.Visible = this.cbFocus.Checked;
      if (this.cbFocus.Checked)
        this.Height += this.panelFocus.Height;
      else
        this.Height -= this.panelFocus.Height;        
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
      Process.Start("https://sourceforge.net/projects/extraql");
    }
    #endregion

    // controls in Focus view

    #region comboRealm_SelectedValueChanged
    private void comboRealm_SelectedValueChanged(object sender, EventArgs e)
    {
      this.btnStartSteam.Enabled = this.comboRealm.Text == "" && this.GetBaseq3Path(true) != null;
      this.miStartSteam.Enabled = this.btnStartSteam.Enabled;
    }
    #endregion

    #region linkFocusLogin_LinkClicked
    private void linkFocusLogin_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      LoginToQlFocus();
    }
    #endregion

    #region linkFocusForum_LinkClicked
    private void linkFocusForum_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      Process.Start("http://focus.quakelive.com/forum/index.php");
    }
    #endregion

    // controls in Options view

    #region btnLauncherExe_Click
    private void btnLauncherExe_Click(object sender, EventArgs e)
    {
      string path = "";
      string file = "Launcher.exe";
      try { path = Path.GetDirectoryName(this.txtLauncherExe.Text); }
      catch { }
      try { file = Path.GetFileName(this.txtLauncherExe.Text); }
      catch { }
      this.openFileDialog1.FileName = file;
      this.openFileDialog1.InitialDirectory = path;
      if (this.openFileDialog1.ShowDialog(this) == DialogResult.OK)
        this.txtLauncherExe.Text = this.openFileDialog1.FileName;
    }
    #endregion

    #region cbDisableScripts_CheckedChanged
    private void cbDisableScripts_CheckedChanged(object sender, EventArgs e)
    {
      this.servlets.EnableScripts = !this.cbDisableScripts.Checked;
      this.btnInstallHook.Text = this.cbDisableScripts.Checked ? "Delete hook.js" : "Re-install hook.js";
    }
    #endregion

    #region btnInstallHook_Click
    private void btnInstallHook_Click(object sender, EventArgs e)
    {
      this.InstallHookJs(false, force: true);
    }
    #endregion

    #region cbDownloadUpdates_CheckedChanged
    private void cbDownloadUpdates_CheckedChanged(object sender, EventArgs e)
    {
      if (this.cbDownloadUpdates.Checked)
      {
        this.CheckForUpdate();
        if (this.updater.UpdateAvailable != null)
        {
          this.SaveSettings();
          this.updater.Run();
          // if Run() succeeds, the .exe will terminate and start the new downloaded version
        }
      }

      this.updateCheckTimer.Enabled = this.cbBindToAll.Checked && this.cbDownloadUpdates.Checked;
    }
    #endregion
    
    #region cbSystemTray_CheckedChanged
    private void cbSystemTray_CheckedChanged(object sender, EventArgs e)
    {
      this.ShowInTaskbar = !this.cbSystemTray.Checked;
      this.trayIcon.Visible = cbSystemTray.Checked;
    }
    #endregion

    #region cbAutostart_CheckedChanged
    private void cbAutostart_CheckedChanged(object sender, EventArgs e)
    {
      if (!((CheckBox)sender).Checked)
        return;
      if (sender == this.cbAutostartLauncher)
        this.cbAutostartSteam.Checked = false;
      else
        this.cbAutostartLauncher.Checked = false;
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
      this.updateCheckTimer.Enabled = this.cbBindToAll.Checked && this.cbDownloadUpdates.Checked;
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
        this.WindowState = FormWindowState.Normal;
        this.BringToFront();
        this.Activate();
      }
    }
    #endregion

    #region miStartLauncher_Click
    private void miStartLauncher_Click(object sender, EventArgs e)
    {
      this.Launch(false);
    }
    #endregion

    #region miStartSteam_Click
    private void miStartSteam_Click(object sender, EventArgs e)
    {
      this.Launch(true);
    }
    #endregion

    #region miQuit_Click
    private void miQuit_Click(object sender, EventArgs e)
    {
      this.Close();
    }
    #endregion

    // timers

    #region updateCheckTimer_Tick
    private void updateCheckTimer_Tick(object sender, EventArgs e)
    {
      this.scriptRepository.UpdateScripts(true, true, this.config.GetString("masterServer"));
    }
    #endregion

    #region autoQuitTimer_Tick
    private void autoQuitTimer_Tick(object sender, EventArgs e)
    {
      int count = Process.GetProcessesByName("quakelive").Length;
      if (this.autoQuitQlIsRunning && count == 0)
        this.Close();
      else if (count > 0)
        this.autoQuitQlIsRunning = true;
    }
    #endregion



    #region LoadSettings()
    private void LoadSettings()
    {
      foreach (var account in this.config.Accounts)
        this.comboEmail.Items.Add(account.Key);

      string lastEmail = config.GetString("lastEmail");
      if (!String.IsNullOrEmpty(lastEmail))
        this.comboEmail.Text = lastEmail;
      else if (this.comboEmail.Items.Count > 0)
        this.comboEmail.SelectedIndex = 0;

      this.txtLauncherExe.Text = config.GetString("launcherExe");

      this.comboRealm.Items.Add("");
      foreach (var realm in config.RealmHistory)
        this.comboRealm.Items.Add(realm);
      this.comboRealm.Text = config.GetString("realm");

      this.cbAdvanced.Checked = config.GetBool("advanced");
      this.cbFocus.Checked = config.GetBool("focus");
      this.cbBindToAll.Checked = config.GetBool("bindToAll");
      this.cbSystemTray.Checked = config.GetBool("systemTray");
      this.cbStartMinimized.Checked = config.GetBool("startMinimized");
      if (this.cbStartMinimized.Checked)
        this.WindowState = FormWindowState.Minimized;
      this.cbDownloadUpdates.Checked = config.GetBool("checkUpdates");
      this.cbAutostartLauncher.Checked = config.GetString("autostart") == "1";
      this.cbAutostartSteam.Checked = config.GetString("autostart") == "2";
      this.cbRunAsCommandLine.Checked = config.GetBool("runAsCommandLine");
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
        if (!this.comboEmail.Items.Contains(this.comboEmail.Text))
          this.comboEmail.Items.Add(this.comboEmail.Text);
        this.config.Accounts[this.comboEmail.Text] = this.txtPassword.Text;

        string realmUrl = this.comboRealm.Text.Trim();
        if (!this.comboRealm.Items.Contains(realmUrl))
        {
          this.comboRealm.Items.Add(realmUrl);
          this.config.RealmHistory.Add(realmUrl);
        }
        config.Set("lastEmail", this.comboEmail.Text);
        config.Set("focus", this.cbFocus.Checked);
        config.Set("advanced", this.cbAdvanced.Checked);
        config.Set("realm", this.comboRealm.Text);
        config.Set("launcherExe", this.txtLauncherExe.Text);
        config.Set("bindToAll", this.cbBindToAll.Checked);
        config.Set("systemTray", this.cbSystemTray.Checked);
        config.Set("startMinimized", this.cbStartMinimized.Checked);
        config.Set("checkUpdates", this.cbDownloadUpdates.Checked);
        config.Set("autostart", this.cbAutostartLauncher.Checked ? "1" : this.cbAutostartSteam.Checked ? "2" : "0");
        config.Set("runAsCommandLine", this.cbRunAsCommandLine.Checked);        
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

    #region CheckForUpdate()
    private void CheckForUpdate()
    {
      try
      {
        this.Log(this.updater.LogBuffer.ToString());
        this.updater.LogBuffer.Remove(0, this.updater.LogBuffer.Length);

        string newerVersion = this.updater.UpdateAvailable;
        if (newerVersion != null)
          Log("An update for extraQL.exe version " + newerVersion + " is available. Enable  \"Download Updates\" to get the latest version");

        this.scriptRepository.UpdateScripts(this.cbDownloadUpdates.Checked, this.cbBindToAll.Checked, this.config.GetString("masterServer"));
      }
      catch (Exception ex)
      {
        Log("Failed to check for latest extraQL.exe version: " + ex.Message);
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

    #region Launch()

    private void Launch(bool steam)
    {
      SaveSettings();
      InstallHookJs(steam);
      if (steam)
        StartSteam();
      else
        StartLauncher();
    }
    #endregion

    #region InstallHookJs()

    private void InstallHookJs(bool steam, bool force = false)
    {
      string path = this.GetBaseq3Path(steam);
      if (path == null)
      {
        this.Log("Unable to detect Quake Live's baseq3 directory");
        return;
      }
      var targetHook = path + "hook.js";
      var backupHook = path + "hook_.js";

      // create backup of original hook.js
      if (!File.Exists(backupHook))
      {
        if (File.Exists(targetHook))
        {
          File.Move(targetHook, backupHook);
          Log("Renamed old hook.js to hook_.js");
        }
        else
          File.Create(backupHook).Close();
      }

      if (this.cbDisableScripts.Checked)
      {
        File.Delete(targetHook);
        Log("Deleted hook.js");
        return;
      }

      string bundledHook = this.config.AppBaseDir + "/scripts/hook.js";

      try
      {
        if (File.Exists(targetHook))
        {
          var attrib = File.GetAttributes(targetHook);
          if ((attrib & FileAttributes.ReadOnly) != 0)
            File.SetAttributes(targetHook, attrib & ~FileAttributes.ReadOnly);
          if (force || new FileInfo(targetHook).LastWriteTimeUtc < new FileInfo(bundledHook).LastWriteTimeUtc)
            File.Delete(targetHook);
        }

        if (!File.Exists(targetHook))
        {
          File.Copy(bundledHook, targetHook);
          Log("Installed new hook.js");
        }
      }
      catch (UnauthorizedAccessException ex)
      {
        MessageBox.Show(this, "Unable to write to hook.js.\nPlease make sure your QL config directory is not read-only.\n\nError: " + ex.Message,
          "extraQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }
    #endregion

    #region GetBaseq3Path()
    private string GetBaseq3Path(bool steam)
    {
      string path;
      if (steam)
      {
        path = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Valve\Steam", "SteamPath", null) as string;
        if (path != null)
          path += @"\SteamApps\Common\Quake Live\baseq3\";
      }
      else
      {
        path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        if (path.EndsWith("Roaming"))
          path = Path.GetDirectoryName(path) + @"\LocalLow";

        string realmDir = this.comboRealm.Text.Contains("focus") ? "focus" : "quakelive";
        path += @"\id Software\" + realmDir + @"\home\baseq3\";
      }

      return path != null && Directory.Exists(path) ? path : null;
    }

    #endregion

    #region StartSteam()
    private void StartSteam()
    {
      this.Log("Starting Quake Live Steam App...");
      Process.Start("steam://rungameid/282440");
      this.WindowState = FormWindowState.Minimized;
    }
    #endregion

    #region StartLauncher()

    private void StartLauncher()
    {
      string realmUrl = this.comboRealm.Text.Trim();

      ProcessStartInfo si = new ProcessStartInfo();
      if (this.cbRunAsCommandLine.Checked)
      {
        if (!this.RunAsCommandLine(si)) 
          return;
      }
      else
      {
        string cmd = this.txtLauncherExe.Text.Trim(new [] { ' ', '"' });
        if (!File.Exists(cmd))
        {
          MessageBox.Show(this, "Can't find Launcher.exe at the specified location", "extraQL", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
          return;
        }
        si.FileName = cmd;
      }

      if (this.cbFocus.Checked && !String.IsNullOrEmpty(realmUrl))
        si.Arguments += "--realm=\"" + realmUrl + "\"";

      try
      {
        this.Log("Starting Quake Live...");
        Process.Start(si);
      }
      catch(Exception ex)
      {
        MessageBox.Show(this, "Could not start \"" + si.FileName + "\":\n" + ex.Message, "extraQL", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        return;
      }
      this.WindowState = FormWindowState.Minimized;

      this.timerCount = 0;
      this.launcherDetectionTimer.Start();
    }
    #endregion

    #region RunAsCommandLine
    private bool RunAsCommandLine(ProcessStartInfo si)
    {
      string cmd = this.txtLauncherExe.Text;
      if (cmd.StartsWith("\""))
      {
        int end = cmd.IndexOf("\"", 1);
        if (end < 0)
        {
          MessageBox.Show(this, "Non-matching quotes in command line for Launcher.exe", "extraQL", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
          return false;
        }
        si.FileName = cmd.Substring(1, end - 1);
        if (end + 2 < cmd.Length)
          si.Arguments = cmd.Substring(end + 2, cmd.Length - (end + 2));
      }
      else
      {
        int end = cmd.IndexOf(" ");
        if (end > 0)
        {
          si.FileName = cmd.Substring(0, end);
          si.Arguments = cmd.Substring(end + 1, cmd.Length - (end + 1));
        }
        else
          si.FileName = cmd;
      }
      return true;
    }

    #endregion

    #region laucherDetectionTimer_Tick
    private void laucherDetectionTimer_Tick(object sender, EventArgs e)
    {
      // find Launcher main window handle
      var handle = LauncherWindowHandle;
      if (handle == IntPtr.Zero)
      {
        if (++this.timerCount == 20)
          this.launcherDetectionTimer.Stop();
        return;
      }

      this.launcherDetectionTimer.Stop();
      this.FillLaucherWithEmailAndPassword(handle);
    }
    #endregion

    #region launcherPlayTimer_Tick
    private void launcherPlayTimer_Tick(object sender, EventArgs e)
    {
      // this method is periodically called after starting the launcher to wait for the Play button to become enabled
      object[] context = (object[])((Timer)sender).Tag;
      IntPtr hwndPlay = (IntPtr)context[0];
      int attemptCount = (int)context[1];

      long style = Win32.GetWindowLong(hwndPlay, Win32.GWL_STYLE);
      if ((style & Win32.WS_DISABLED) == 0)
      {
        Win32.PostMessage(hwndPlay, Win32.WM_LBUTTONDOWN, 0, 0);
        Win32.PostMessage(hwndPlay, Win32.WM_LBUTTONUP, 0, 0);
        this.launcherPlayTimer.Stop();
      }
      else
      {
        context[1] = ++attemptCount;
        if (attemptCount == 25) // 25*200ms = 5sec
          this.launcherPlayTimer.Stop();
      }
    }
    #endregion

    #region LauncherWindowHandle
    public static IntPtr LauncherWindowHandle
    {
      get
      {
        foreach (var proc in Process.GetProcessesByName("launcher"))
          return proc.MainWindowHandle;
        return IntPtr.Zero;
      }
    }
    #endregion

    #region FillLaucherWithEmailAndPassword()
    private void FillLaucherWithEmailAndPassword(IntPtr handle)
    {
      // go through child windows and fill the text boxes
      var name = new StringBuilder(100);
      Win32.GetClassName(handle, name, 100);
      int i = 0;
      IntPtr hwndPlay = IntPtr.Zero;
      foreach (var hwndChild in Win32.GetChildWindows(handle))
      {
        Win32.GetClassName(hwndChild, name, 100);
        if (name.ToString() == "WindowsForms10.EDIT.app.0.33c0d9d")
        {
          if (i++ == 0)
            Win32.SendMessage(hwndChild, Win32.WM_SETTEXT, 0, this.comboEmail.Text);
          else
          {
            Win32.SendMessage(hwndChild, Win32.WM_SETTEXT, 0, this.txtPassword.Text);
            break;
          }
        }
        else if (name.ToString() == "WindowsForms10.Window.8.app.0.33c0d9d")
        {
          Win32.RECT rect;
          Win32.GetWindowRect(hwndChild, out rect);
          if (rect.Width == 87 && rect.Height == 88)
            hwndPlay = hwndChild;
        }
      }

      if (hwndPlay != IntPtr.Zero)
      {
        launcherPlayTimer.Tag = new object[] { hwndPlay, 0 };
        launcherPlayTimer.Start();
      }
    }
    #endregion

    #region LoginToQlFocus()
    private void LoginToQlFocus()
    {
      using (var webRequest = new XWebClient())
      {
        webRequest.Encoding = Encoding.UTF8;
        var html = webRequest.DownloadString("http://focus.quakelive.com/focusgate/");
        string js = @"
document.loginform.login.value = '" + this.comboEmail.Text + @"';
document.loginform.passwd.value = '" + this.txtPassword.Text + @"';
document.loginform.submit();";
        html = html.Replace("document.loginform.login.focus();", js);
        var file = Path.GetTempFileName();
        File.Move(file, file + ".html");
        file += ".html";
        File.WriteAllText(file, html);
        Process.Start(file);

        Timer timer = new Timer();
        timer.Interval = 5000;
        timer.Tick += (sender2, args) =>
        {
          timer.Stop();
          File.Delete(file);
          ((IDisposable)sender2).Dispose();
        };
        timer.Start();
      }
    }
    #endregion

  }
}