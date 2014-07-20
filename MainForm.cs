using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;
using ExtraQL.Properties;

namespace ExtraQL
{
  public partial class MainForm : Form
  {
    private int timerCount;
    private readonly Dictionary<string,string> passwordByEmail = new Dictionary<string, string>();

    private Size windowDragOffset;

    #region ctor()
    public MainForm()
    {
      InitializeComponent();

      this.LoadSettings();
      base.Text = base.Text + " " + Program.Version;
      this.lblVersion.Text = Program.Version;
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
        this.btnStartFocus.Select();
        this.Launch();
        e.Handled = true;
      }
    }
    #endregion

    #region txtPassword_Validating
    private void txtPassword_Validating(object sender, System.ComponentModel.CancelEventArgs e)
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
      this.InstallHookJs(true);
    }
    #endregion

    #region btnStartLauncher_Click
    private void btnStartLauncher_Click(object sender, EventArgs e)
    {
      Launch();
    }
    #endregion

    #region btnQuit_Click
    private void btnQuit_Click(object sender, EventArgs e)
    {
      this.Close();
    }
    #endregion

    #region linkAbout_LinkClicked
    private void linkAbout_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      Process.Start("https://github.com/PredatH0r/extraQL");
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

    #region timer1_Tick
    private void timer1_Tick(object sender, EventArgs e)
    {
      // find Launcher main window handle
      var handle = LauncherWindowHandle;
      if (handle == IntPtr.Zero)
      {
        if (++this.timerCount == 20)
          this.timer1.Stop();
        return;
      }

      this.timer1.Stop();
      this.FillLaucherWithEmailAndPassword(handle);
    }
    #endregion


    #region LoadSettings()
    private void LoadSettings()
    {
      var pwds = Cypher.DecryptString(Settings.Default.Password).Split('\t');
      int i;

      if (!string.IsNullOrEmpty(Settings.Default.Email))
      {
        i = 0;
        foreach (var email in Settings.Default.Email.Split('\t'))
        {
          this.comboEmail.Items.Add(email);
          this.passwordByEmail[email] = pwds[i++];
        }
        if (!string.IsNullOrEmpty(Settings.Default.LastEmail))
          this.comboEmail.Text = Settings.Default.LastEmail;
        else if (this.comboEmail.Items.Count > 0)
          this.comboEmail.SelectedIndex = 0;
      }

      this.txtLauncherExe.Text = Settings.Default.LauncherExe;
      if (string.IsNullOrEmpty(this.txtLauncherExe.Text))
        this.txtLauncherExe.Text = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + "\\Quake Live\\Launcher.exe";

      if (!string.IsNullOrEmpty(Settings.Default.RealmHistory))
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
    #endregion
    
    #region Log()

    private delegate void Action();

    private void Log(string msg)
    {
      if (string.IsNullOrEmpty(msg))
        return;

      if (this.InvokeRequired)
        this.BeginInvoke((Action) (() => this.Log(msg)));
      else
        this.txtLog.Text += "[" + DateTime.Now.ToString("T") + "] " + msg + "\r\n";
    }
    #endregion

    #region Launch()

    private void Launch()
    {
      SaveSettings();
      InstallHookJs();
      StartLauncher();
    }
    #endregion

    #region InstallHookJs()

    private void InstallHookJs(bool force = false)
    {
      var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
      if (path.EndsWith("Roaming"))
        path = Path.GetDirectoryName(path) + "\\LocalLow";

      string realmDir = this.comboRealm.Text.Contains("focus") ? "focus" : "quakelive";
      path += "\\id Software\\" + realmDir + "\\home\\baseq3\\";

      // create backup of original hook.js
      if (!File.Exists(path + "hook_.js"))
      {
        if (File.Exists(path + "hook.js"))
        {
          File.Move(path + "hook.js", path + "hook_.js");
          Log("renamed existing hook.js to hook_.js");
        }
        else
          File.Create(path + "hook_.js").Close();
      }

      if (force)
        File.Delete(path + "hook.js");
      if (!File.Exists(path + "hook.js"))
      {
        var exeDir = Path.GetDirectoryName(Application.ExecutablePath) ?? "";
        if (exeDir.EndsWith("\\bin\\Debug"))
          exeDir = Path.GetDirectoryName(Path.GetDirectoryName(exeDir));
        File.Copy(exeDir + "\\scripts\\hook.js", path + "hook.js");
        Log("installed new hook.js");
      }
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
      if (!string.IsNullOrEmpty(realmUrl))
        si.Arguments = "--realm=\"" + realmUrl + "\"";
      Process.Start(si);
      this.WindowState = FormWindowState.Minimized;

      this.timerCount = 0;
      this.timer1.Start();
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