using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace ExtraQL
{
  public partial class MainForm : Form
  {
    public const string Version = "0.107";

    private int timerCount;
    private readonly Dictionary<string, string> passwordByEmail = new Dictionary<string, string>();
    private readonly HttpServer server;
    private readonly Servlets servlets;
    private readonly ScriptRepository scriptRepository;

    private Size windowDragOffset;

    #region ctor()
    public MainForm()
    {
      InitializeComponent();

      this.LoadSettings();
      base.Text = base.Text + " " + Version;
      this.lblVersion.Text = Version;
      this.lblExtra.Parent = this.picLogo;
      this.lblVersion.Parent = this.picLogo;

      this.server = new HttpServer();
      this.server.BindToAllInterfaces = this.cbBindToAll.Checked;

      this.scriptRepository = new ScriptRepository();
      this.scriptRepository.Log = this.Log;

      this.servlets = new Servlets(this.server, this.scriptRepository, this.Log);
    }
    #endregion


    #region OnShown()
    protected override void OnShown(EventArgs e)
    {
      base.OnShown(e);

      this.scriptRepository.UpdateAndRegisterScripts();
      this.RestartHttpServer();
      if (this.cbCheckUpdate.Checked)
        this.CheckForUpdate();
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

    #region picLogo_MouseDown
    private void picLogo_MouseDown(object sender, MouseEventArgs e)
    {
      var mouseCoord = MousePosition;
      this.windowDragOffset = new Size(mouseCoord.X - this.Left, mouseCoord.Y - this.Top);
    }
    #endregion

    #region picLogo_MouseMove
    private void picLogo_MouseMove(object sender, MouseEventArgs e)
    {
      if ((e.Button & MouseButtons.Left) == 0)
        return;
      this.Location = MousePosition - this.windowDragOffset;
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
      this.passwordByEmail.TryGetValue(this.comboEmail.Text, out pwd);
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
        this.passwordByEmail[this.comboEmail.Text] = this.txtPassword.Text;
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

    #region comboRealm_SelectedValueChanged
    private void comboRealm_SelectedValueChanged(object sender, EventArgs e)
    {
      this.btnStartSteam.Enabled = this.comboRealm.Text == "" && this.GetBaseq3Path(true) != null;
      this.miStartSteam.Enabled = this.btnStartSteam.Enabled;
    }
    #endregion

    #region cbDisableScripts_CheckedChanged
    private void cbDisableScripts_CheckedChanged(object sender, EventArgs e)
    {
      this.servlets.EnableScripts = !this.cbDisableScripts.Checked;
      this.btnInstallHook.Text = this.cbDisableScripts.Checked ? "Delete hook.js" : "Re-install hook.js";
    }
    #endregion

    #region cbSystemTray_CheckedChanged
    private void cbSystemTray_CheckedChanged(object sender, EventArgs e)
    {
      this.ShowInTaskbar = !this.cbSystemTray.Checked;
      this.trayIcon.Visible = cbSystemTray.Checked;
    }
    #endregion

    #region cbCheckUpdate_CheckedChanged
    private void cbCheckUpdate_CheckedChanged(object sender, EventArgs e)
    {
      if (this.cbCheckUpdate.Checked)
        this.CheckForUpdate();
    }
    #endregion

    #region btnLauncherExe_Click
    private void btnLauncherExe_Click(object sender, EventArgs e)
    {
      string path = "";
      string file = "Launcher.exe";
      try { path = Path.GetDirectoryName(this.txtLauncherExe.Text); } catch { }
      try { file = Path.GetFileName(this.txtLauncherExe.Text); } catch { }
      this.openFileDialog1.FileName = file;
      this.openFileDialog1.InitialDirectory = path;
      if (this.openFileDialog1.ShowDialog(this) == DialogResult.OK)
        this.txtLauncherExe.Text = this.openFileDialog1.FileName;
    }
    #endregion

    #region btnInstallHook_Click
    private void btnInstallHook_Click(object sender, EventArgs e)
    {
      this.InstallHookJs(false, force: true);
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

    #region linkAbout_LinkClicked
    private void linkAbout_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      Process.Start("https://sourceforge.net/projects/extraql");
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

    #region cbBindAll_CheckedChanged
    private void cbBindAll_CheckedChanged(object sender, EventArgs e)
    {
      this.RestartHttpServer();
    }
    #endregion

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



    #region ConfigFile
    private string ConfigFile
    {
      get
      {
        var path = Path.GetDirectoryName(Application.ExecutablePath);
        if (path.EndsWith(("Debug")))
          path = Path.GetDirectoryName(Path.GetDirectoryName(path));
        return Path.Combine(path, "extraQL.ini");
      }
    }
    #endregion

    #region LoadSettings()
    private void LoadSettings()
    {
      string email = "";
      string pwd = "";
      string lastEmail = "";
      string launcherExe = "";
      string realmHistory = "";
      string lastRealm = "";
      bool advanced = false;
      bool focus = false;
      bool bindToAll = false;
      bool systemTray = false;
      bool startMinimized = false;
      bool checkUpdates = true;

      var configFile = this.ConfigFile;
      if (File.Exists(configFile))
      {
        var lines = File.ReadAllLines(configFile);
        foreach (var line in lines)
        {
          var parts = line.Split(new[] {'='}, 2);
          if (parts.Length < 2) continue;
          var value = parts[1].Trim();
          switch (parts[0].Trim())
          {
            case "email": email = value; break;
            case "password": pwd = value; break;
            case "lastEmail": lastEmail = value; break;
            case "launcherExe": launcherExe = value; break;
            case "realmHistory": realmHistory = value; break;
            case "realm": lastRealm = value; break;
            case "advanced": advanced = value == "1"; break;
            case "focus": focus = value == "1"; break;
            case "bindToAll": bindToAll = value == "1"; break;
            case "systemTray": systemTray = value == "1"; break;
            case "startMinimized": startMinimized = value == "1"; break;
            case "checkUpdates": checkUpdates = value == "1"; break;
          }
        }
      }

      var pwds = string.IsNullOrEmpty(pwd) ? new string[0] : Cypher.DecryptString(pwd).Split('\t');
      if (!String.IsNullOrEmpty(email))
      {
        int i = 0;
        foreach (var mail in email.Split('\t'))
        {
          this.comboEmail.Items.Add(mail);
          this.passwordByEmail[mail] = i < pwds.Length ? pwds[i++] : "";
        }
        if (!String.IsNullOrEmpty(lastEmail))
          this.comboEmail.Text = lastEmail;
        else if (this.comboEmail.Items.Count > 0)
          this.comboEmail.SelectedIndex = 0;
      }

      this.txtLauncherExe.Text = launcherExe;
      if (String.IsNullOrEmpty(this.txtLauncherExe.Text))
        this.txtLauncherExe.Text = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + "\\Quake Live\\Launcher.exe";

      if (!String.IsNullOrEmpty(realmHistory))
      {
        this.comboRealm.Items.Add("");
        foreach (var realm in realmHistory.Trim().Split('\t'))
          this.comboRealm.Items.Add(realm);
      }
      this.comboRealm.Text = lastRealm;

      this.cbAdvanced.Checked = advanced;
      this.cbFocus.Checked = focus;
      this.cbBindToAll.Checked = bindToAll;
      this.cbSystemTray.Checked = systemTray;
      this.cbStartMinimized.Checked = startMinimized;
      if (startMinimized)
        this.WindowState = FormWindowState.Minimized;
      this.cbCheckUpdate.Checked = checkUpdates;
      this.ActiveControl = this.comboEmail;
    }

    #endregion

    #region SaveSettings()
    private void SaveSettings()
    {
      try
      {
        if (!this.comboEmail.Items.Contains(this.comboEmail.Text))
          this.comboEmail.Items.Add(this.comboEmail.Text);
        this.passwordByEmail[this.comboEmail.Text] = this.txtPassword.Text;

        string emails = "";
        string pwds = "";
        foreach (string email in this.comboEmail.Items)
        {
          emails += "\t" + email;
          pwds += "\t" + this.passwordByEmail[email];
        }

        string realmHistory = "";
        string realmUrl = this.comboRealm.Text.Trim();
        if (!this.comboRealm.Items.Contains(realmUrl))
          this.comboRealm.Items.Add(realmUrl);
        foreach (var realm in this.comboRealm.Items)
          realmHistory += realm + "\t";

        StringBuilder config = new StringBuilder();
        config.AppendLine("[extraQL]");
        config.AppendLine("email=" + (emails.Length == 0 ? "" : emails.Substring(1)));
        config.AppendLine("password=" + Cypher.EncryptString(pwds.Length == 0 ? "" : pwds.Substring(1)));
        config.AppendLine("lastEmail=" + this.comboEmail.Text);
        config.AppendLine("focus=" + (this.cbFocus.Checked ? 1 : 0));
        config.AppendLine("advanced=" + (this.cbAdvanced.Checked ? 1 : 0));
        config.AppendLine("realm=" + this.comboRealm.Text);
        config.AppendLine("realmHistory=" + realmHistory.Trim());
        config.AppendLine("launcherExe=" + this.txtLauncherExe.Text);
        config.AppendLine("bindToAll=" + (this.cbBindToAll.Checked ? 1 : 0));
        config.AppendLine("systemTray=" + (this.cbSystemTray.Checked ? 1 : 0));
        config.AppendLine("startMinimized=" + (this.cbStartMinimized.Checked ? 1 : 0));
        config.AppendLine("checkUpdates=" + (this.cbCheckUpdate.Checked ? 1 : 0));
        File.WriteAllText(this.ConfigFile, config.ToString(), Encoding.UTF8);
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
        this.txtLog.Text += "[" + DateTime.Now.ToString("T") + "] " + msg + "\r\n";
    }
    #endregion

    #region CheckForUpdate()
    private void CheckForUpdate()
    {
      Log("Checking for update...");
      try
      {
        WebClient client = new WebClient();
        client.OpenReadCompleted += CheckForUpdate_OpenReadCompleted;
        client.OpenReadAsync(new Uri("http://sourceforge.net/p/extraql/source/ci/master/tree/MainForm.cs?format=raw"));
      }
      catch (Exception ex)
      {
        Log("Failed to check for latest extraQL.exe version: " + ex.Message);
      }
    }

    private void CheckForUpdate_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
    {
      Exception error = e.Error;
      if (error == null)
      {
        try
        {
          var buffer = new byte[1024];
          int len = e.Result.Read(buffer, 0, buffer.Length);
          var code = Encoding.UTF8.GetString(buffer, 0, len);
          var match = System.Text.RegularExpressions.Regex.Match(code, @".*Version\s*=\s*" + "\"([0-9.a-z]*)\"");
          if (match.Success)
          {
            var remoteVersion = match.Groups[1].Value;
            if (ScriptRepository.IsNewer(remoteVersion, Version))
            {
              Log("Update for extraQL.exe version " + remoteVersion + " is available");
              if (MessageBox.Show(this, "Version " + remoteVersion + " of extraQL.exe is available!\n\nDo you want to open the download page?",
                "Update Check", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
              {
                Process.Start("http://sourceforge.net/projects/extraql/files/");
              }
            }
            else
              Log("Your extraQL.exe is up-to-date.");
          }
          ((WebClient) sender).Dispose();
        }
        catch (Exception ex)
        {
          error = ex;
        }
      }

      if (error != null)
        Log("Failed to check for latest extraQL.exe version: " + error.Message);
    }
    #endregion

    #region RestartHttpServer()
    private void RestartHttpServer()
    {
      if (this.server == null)
        return;
      this.server.Stop();
      this.server.BindToAllInterfaces = this.cbBindToAll.Checked;
      this.servlets.EnablePrivateServlets = !this.cbBindToAll.Checked;
      if (this.server.Start())
        this.Log("extraQL server listening on http://" + this.server.EndPoint);
      else
        this.Log("extraQL server failed to start on http://" + this.server.EndPoint +". Scripts are disabled!");
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

      var bundledHook = Path.GetDirectoryName(Application.ExecutablePath) ?? "";
      if (bundledHook.EndsWith("\\bin\\Debug"))
        bundledHook = Path.GetDirectoryName(Path.GetDirectoryName(bundledHook));
      bundledHook += "/scripts/hook.js";

      if (force || new FileInfo(targetHook).LastWriteTimeUtc < new FileInfo(bundledHook).LastWriteTimeUtc)
        File.Delete(targetHook);

      if (!File.Exists(targetHook))
      {
        File.Copy(bundledHook, targetHook);
        Log("Installed new hook.js");
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
          path += @"\SteamApps\Common\Quake Live\baseq3";
      }
      else
      {
        path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        if (path.EndsWith("Roaming"))
          path = Path.GetDirectoryName(path) + "\\LocalLow";

        string realmDir = this.comboRealm.Text.Contains("focus") ? "focus" : "quakelive";
        path += "\\id Software\\" + realmDir + "\\home\\baseq3\\";
      }

      return path != null && Directory.Exists(path) ? path : null;
    }

    #endregion

    #region StartLauncher()

    private void StartLauncher()
    {
      if (!File.Exists(this.txtLauncherExe.Text))
      {
        MessageBox.Show(this, "Can't find Launcher.exe at the specified location", "extraQL", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        return;
      }

      this.Log("Starting Quake Live Laucher...");
      string realmUrl = this.comboRealm.Text.Trim();

      ProcessStartInfo si = new ProcessStartInfo();
      si.FileName = this.txtLauncherExe.Text;
      if (!String.IsNullOrEmpty(realmUrl))
        si.Arguments = "--realm=\"" + realmUrl + "\"";
      Process.Start(si);
      this.WindowState = FormWindowState.Minimized;

      this.timerCount = 0;
      this.launcherDetectionTimer.Start();
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

    #region LoginToQlFocus()
    private void LoginToQlFocus()
    {
      using (var webRequest = new WebClient())
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
      }
    }
    #endregion

  }
}