using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace ExtraQL
{
  public class Config
  {
    private readonly Dictionary<string,string> settings = new Dictionary<string, string>();

    public readonly string AppBaseDir;

    #region ctor()
    public Config()
    {
      AppBaseDir = Path.GetDirectoryName(Application.ExecutablePath) ?? "";
      if (AppBaseDir.ToLower().EndsWith("\\bin\\debug"))
        AppBaseDir = Path.GetDirectoryName(Path.GetDirectoryName(AppBaseDir));      
    }
    #endregion

    #region LoadSettings()
    public void LoadSettings()
    {
      this.settings.Clear();
      this.settings["advanced"] = "0";
      this.settings["launcherExe"] = GetDefaultLauncherPath();
      this.settings["bindToAll"] = "0";
      this.settings["systemTray"] = "0";
      this.settings["startMinimized"] = "0";
      this.settings["checkUpdates"] = "1";
      this.settings["autostart"] = "0";
      this.settings["log"] = "0";
      this.settings["followLog"] = "0";
      this.settings["https"] = "0";
      this.settings["logAllRequests"] = "0";
      this.settings["autoquit"] = "0";
      this.settings["quakelive_steam.exe"] = "";
      this.settings["nickQuake"] = "";
      this.settings["nickSteam"] = "";

      var configFile = this.ConfigFile;
      if (File.Exists(configFile))
      {
        var lines = File.ReadAllLines(configFile);
        foreach (var line in lines)
        {
          var parts = line.Split(new[] { '=' }, 2);
          if (parts.Length < 2) continue;
          var value = parts[1].Trim();
          settings[parts[0].Trim()] = value;
        }
      }
    }
    #endregion

    #region GetString(), GetBool()

    public string GetString(string setting)
    {
      return this.settings[setting];
    }

    public bool GetBool(string setting)
    {
      return this.settings[setting] == "1";
    }
    #endregion

    #region Set()
    public void Set(string setting, string value)
    {
      if (!settings.ContainsKey(setting))
        throw new ArgumentOutOfRangeException("setting", value);
      this.settings[setting] = value;
    }

    public void Set(string setting, bool value)
    {
      this.Set(setting, value ? "1" : "0");
    }
    #endregion

    #region SaveSettings()
    public void SaveSettings()
    {
      StringBuilder config = new StringBuilder();
      config.AppendLine("[extraQL]");
      foreach (var entry in this.settings)
        config.AppendLine(entry.Key + "=" + entry.Value);
      File.WriteAllText(this.ConfigFile, config.ToString(), Encoding.UTF8);
    }
    #endregion

    #region ConfigFile
    private string ConfigFile => Path.Combine(this.AppBaseDir, "extraQL.ini");

    #endregion

    #region GetDefaultLauncherPath()
    private string GetDefaultLauncherPath()
    {
      try
      {
        string path = Registry.GetValue(@"HKEY_CURRENT_USER\Software\id Software\Quake Live", null, null) as string;
        if (path != null && File.Exists(path += "\\Launcher.exe"))
          return path;
      }
      catch { } // fails on Mono
      
      try
      {
        string path = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Quake Live", "UninstallString", null) as string;
        if (path != null && File.Exists(path = path.Replace("uninstall.exe", "Launcher.exe")))
          return path;
      }
      catch { }
      return Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + "\\Quake Live\\Launcher.exe";
    }
    #endregion

  }
}
