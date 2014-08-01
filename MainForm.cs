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
using ExtraQL.Properties;
using Microsoft.Win32;

namespace ExtraQL
{
  public partial class MainForm : Form
  {
    public const string Version = "0.101";

    private int timerCount;
    private readonly Dictionary<string, string> passwordByEmail = new Dictionary<string, string>();

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
    }
    #endregion


    #region OnShown()
    protected override void OnShown(EventArgs e)
    {
      base.OnShown(e);

      var updater = new ScriptRepository();
      updater.Log = this.Log;
      updater.UpdateScripts();

      if (Servlets.Startup(this.Log, updater))
        this.Log("extraQL server listening on http://127.0.0.1:27963/");
      else
        this.Log("extraQL server failed to start. Scripts are disabled.");
    }
    #endregion

    #region OnClosed()
    protected override void OnClosed(EventArgs e)
    {
      this.SaveSettings();
      Servlets.ShutDown();
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
    }
    #endregion

    #region cbDisableScripts_CheckedChanged
    private void cbDisableScripts_CheckedChanged(object sender, EventArgs e)
    {
      Servlets.Instance.EnableScripts = !this.cbDisableScripts.Checked;
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


 
    #region LoadSettings()
    private void LoadSettings()
    {
      var pwds = Cypher.DecryptString(Settings.Default.Password).Split('\t');
      int i;

      if (!String.IsNullOrEmpty(Settings.Default.Email))
      {
        i = 0;
        foreach (var email in Settings.Default.Email.Split('\t'))
        {
          this.comboEmail.Items.Add(email);
          this.passwordByEmail[email] = pwds[i++];
        }
        if (!String.IsNullOrEmpty(Settings.Default.LastEmail))
          this.comboEmail.Text = Settings.Default.LastEmail;
        else if (this.comboEmail.Items.Count > 0)
          this.comboEmail.SelectedIndex = 0;
      }

      this.txtLauncherExe.Text = Settings.Default.LauncherExe;
      if (String.IsNullOrEmpty(this.txtLauncherExe.Text))
        this.txtLauncherExe.Text = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + "\\Quake Live\\Launcher.exe";

      if (!String.IsNullOrEmpty(Settings.Default.RealmHistory))
      {
        this.comboRealm.Items.Add("");
        foreach (var realm in Settings.Default.RealmHistory.Trim().Split('\t'))
          this.comboRealm.Items.Add(realm);
      }
      this.comboRealm.Text = Settings.Default.Realm;

      this.cbAdvanced.Checked = Settings.Default.Advanced;
      this.cbFocus.Checked = Settings.Default.Focus;
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

        Settings.Default.Email = emails.Length == 0 ? "" : emails.Substring(1);
        Settings.Default.Password = Cypher.EncryptString(pwds.Length == 0 ? "" : pwds.Substring(1));
        Settings.Default.Focus = this.cbFocus.Checked;
        Settings.Default.Advanced = this.cbAdvanced.Checked;
        Settings.Default.Realm = this.comboRealm.Text;
        Settings.Default.RealmHistory = realmHistory.Trim();
        Settings.Default.LauncherExe = this.txtLauncherExe.Text;
        Settings.Default.Save();
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