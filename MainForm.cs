using System;
using System.Collections.Generic;
using System.Diagnostics;
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

    private static readonly List<RealmInfo> realms = new List<RealmInfo>
    {
      new RealmInfo("Prod", "https://secure.quakelive.com", "quakelive"),
      new RealmInfo("Pre-Prod NG0", "https://ng0.quakelive.com", "quakelive"),
      new RealmInfo("Focus", "https://focus.quakelive.com", "focus")
    };

    #region ctor()
    public MainForm()
    {
      InitializeComponent();

      this.LoadSettings();
      base.Text = base.Text + " " + Program.Version;
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

    #region txtEmail_KeyDown
    private void txtEmail_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.KeyData == Keys.Enter)
      {
        this.lblPassword.Select();
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

    #region cbFocus_CheckedChanged
    private void cbFocus_CheckedChanged(object sender, EventArgs e)
    {
      this.panelFocus.Visible = this.cbFocus.Checked;
      if (!this.cbFocus.Checked)
        this.Height -= this.panelFocus.Height;
      else
        this.Height += this.panelFocus.Height;
    }
    #endregion

    #region cbAdvanced_CheckedChanged
    private void cbAdvanced_CheckedChanged(object sender, EventArgs e)
    {
      if (this.cbAdvanced.Checked)
      {
        this.FormBorderStyle = FormBorderStyle.Sizable;
        this.Height += panelAdvanced.Height;
        this.panelAdvanced.Visible = true;
        this.panelAdvanced.Dock = DockStyle.Fill;
      }
      else
      {
        this.panelAdvanced.Visible = false;
        this.panelAdvanced.Dock = DockStyle.None;
        this.Height -= panelAdvanced.Height;
        this.FormBorderStyle = FormBorderStyle.FixedSingle;
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
      this.InstallHookJs(realms[this.comboRealm.SelectedIndex], true);
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
      Process.Start("http://www.quakelive.com/#!/profile/summary/PredatH0r/");
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
      this.txtLauncherExe.Text = Settings.Default.LauncherExe;
      if (Settings.Default.Password != "")
        this.txtPassword.Text = Cypher.DecryptString(Settings.Default.Password);
      this.txtEmail.Text = Settings.Default.Email;
      if (string.IsNullOrEmpty(this.txtLauncherExe.Text))
        this.txtLauncherExe.Text = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + "\\Quake Live\\Launcher.exe";

      int i = 0;
      int selIndex = 0;
      foreach (var realmInfo in realms)
      {
        this.comboRealm.Items.Add(realmInfo.Label);
        if (Settings.Default.Realm == realmInfo.Url)
          selIndex = i;
        ++i;
      }
      this.comboRealm.SelectedIndex = selIndex;

      this.cbAdvanced.Checked = Settings.Default.Advanced;
      this.cbFocus.Checked = Settings.Default.Focus;
      this.ActiveControl = this.txtEmail;
    }

    #endregion

    #region SaveSettings()
    private void SaveSettings()
    {
      Settings.Default.Email = this.txtEmail.Text;
      Settings.Default.Password = Cypher.EncryptString(this.txtPassword.Text);
      Settings.Default.Focus = this.cbFocus.Checked;
      Settings.Default.Advanced = this.cbAdvanced.Checked;
      Settings.Default.Realm = realms[this.comboRealm.SelectedIndex].Url;
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
      var realm = realms[this.comboRealm.SelectedIndex];
      InstallHookJs(realm);
      StartLauncher(realm);
    }
    #endregion

    #region InstallHookJs()

    private void InstallHookJs(RealmInfo realm, bool force = false)
    {
      var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
      if (path.EndsWith("Roaming"))
        path = Path.GetDirectoryName(path) + "\\LocalLow";

      path += "\\id Software\\" + realm.Directory + "\\home\\baseq3\\";

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

    private void StartLauncher(RealmInfo realm)
    {
      if (!File.Exists(this.txtLauncherExe.Text))
      {
        MessageBox.Show(this, "Can't find Launcher.exe at the specified location", "extraQL", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        return;
      }

      this.Log("Starting Quake Live Laucher...");

      ProcessStartInfo si = new ProcessStartInfo();
      si.FileName = this.txtLauncherExe.Text;
      si.Arguments = "--realm=" + realm.Url;
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
document.loginform.login.value = '" + this.txtEmail.Text + @"';
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
            Win32.SendMessage(hwndChild, Win32.WM_SETTEXT, 0, this.txtEmail.Text);
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

  #region class RealmInfo
  class RealmInfo
  {
    public readonly string Label;
    public readonly string Url;
    public readonly string Directory;

    public RealmInfo(string label, string url, string directory)
    {
      this.Label = label;
      this.Url = url;
      this.Directory = directory;
    }
  }
  #endregion
}