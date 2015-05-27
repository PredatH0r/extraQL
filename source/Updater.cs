using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ExtraQL
{
  public class Updater
  {
    private readonly Config config;

    public readonly StringBuilder LogBuffer = new StringBuilder();
    public Action<string> Log { get; set; }

    /// <summary>
    /// Holds the version number of an available, but not installed update -or- null
    /// </summary>
    public string UpdateAvailable { get; set; }

    #region ctor()
    public Updater(Config config)
    {
      this.config = config;
      Log = msg => LogBuffer.AppendLine(msg);
    }
    #endregion  

    #region Run()

    public void Run()
    {
      try
      {
        using (var client = new XWebClient(1000))
        {
          client.Encoding = Encoding.UTF8;

          string url = config.GetString("masterServer") + "/version";
          string remoteVersionInfo = client.DownloadString(new Uri(url));
          MasterServerVersionInfoReceived(remoteVersionInfo);
        }
      }
      catch (Exception ex)
      {
        Log("Failed to check for latest extraQL.exe version: " + ex.Message);
      }
    }

    #endregion

    #region MasterServerVersionInfoReceived()

    private void MasterServerVersionInfoReceived(string versionJson)
    {
      try
      {
        Match match = Regex.Match(versionJson, ".*\"version\"\\s*:\\s*\"([0-9.]*)\".*");
        if (match.Success)
        {
          using (var form = new UpdateForm())
          {
            form.Show();
            form.Message = "Checking for extraQL.exe update..";

            string remoteVersion = match.Groups[1].Value;
            if (ScriptRepository.IsNewer(remoteVersion, MainForm.Version))
            {
              this.UpdateAvailable = remoteVersion;
              if (config.GetBool("checkUpdates"))
              {
                form.Message = "Downloading new extraQL.exe...";
                UpdateExe();
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        Log("Failed to check for latest extraQL.exe version: " + ex.Message);
      }
    }

    #endregion

    #region UpdateExe()

    private void UpdateExe()
    {
      try
      {
        using (var client = new XWebClient())
        {
          client.Timeout = 10000;
          client.Encoding = Encoding.UTF8;
          byte[] bin = client.DownloadData(new Uri(config.GetString("masterServer") + "/extraQL.exe"));
          ExeDownloadComplete(bin);
        }
      }
      catch (Exception ex)
      {
        Log("Failed to check for latest extraQL.exe version: " + ex.Message);
      }
    }

    #endregion

    #region ExeDownloadComplete()

    private void ExeDownloadComplete(byte[] bin)
    {
      try
      {
        string exe = Application.ExecutablePath.ToLower();
        if (exe.EndsWith(".old.exe")) // someone started the backup file
          exe = exe.Replace(".old", "");
        else
        {
          string oldExe = exe.Replace(".exe", ".old.exe");
          File.Delete(oldExe);
          File.Move(exe, oldExe);
        }
        File.WriteAllBytes(exe, bin);

        Process.Start(exe, Program.PostUpdateSwitch);
        Environment.Exit(1);
      }
      catch (Exception ex)
      {
        Log("Failed to download latest extraQL.exe version: " + ex.Message);
      }
    }

    #endregion
  }
}