using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ExtraQL
{
  public class ScriptRepository
  {
    private const string DEFAULT_UPDATE_BASE_URL = "http://sourceforge.net/p/extraql/source/ci/master/tree/scripts/{0}?format=raw";
    private const string INDEX_FILE = "!ndex.txt";
    private readonly Dictionary<string, ScriptInfo> scriptById = new Dictionary<string, ScriptInfo>();
    private readonly Encoding utf8withoutBom = new UTF8Encoding(false);

    public ScriptRepository()
    {
      this.Log = txt => { };

      this.ScriptDir = Path.GetDirectoryName(Application.ExecutablePath) ?? "";
      if (this.ScriptDir.Replace('\\','/').EndsWith("/bin/Debug"))
        this.ScriptDir = Path.GetDirectoryName(Path.GetDirectoryName(ScriptDir));
      this.ScriptDir += "/scripts";
    }

    public Action<string> Log { get; set; }

    public string ScriptDir { get; private set; }

    #region UpdateScripts()

    public void UpdateScripts()
    {
      var client = new WebClient();
      client.DownloadStringCompleted += UpdateScripts_IndexDownloadCompleted;
      client.DownloadStringAsync(new Uri(string.Format(DEFAULT_UPDATE_BASE_URL, INDEX_FILE)));
    }

    private void UpdateScripts_IndexDownloadCompleted(object sender, DownloadStringCompletedEventArgs e)
    {
      if (e.Error != null)
      {
        Log("Failed to retrieve list of available userscripts: " + e.Error.Message);
        return;
      }

      var files = e.Result.Split('\n', '\r');
      foreach (var scriptfile in files)
      {
        if (string.IsNullOrEmpty(scriptfile))
          continue;

        string localCode = "";
        Dictionary<string, List<string>> localMeta;

        if (File.Exists(ScriptDir + "/" + scriptfile))
        {
          localCode = File.ReadAllText(scriptfile);
          localMeta = GetMetadata(localCode);
        }
        else
        {
          localMeta = new Dictionary<string, List<string>>();
          localMeta["version"] = new List<string> { "0" };
        }

        if (scriptfile.EndsWith(".usr.js"))
        {
          var id = localMeta.ContainsKey("id") ? localMeta["id"][0] : "";
          if (string.IsNullOrEmpty(id))
            id = StripAllExtenstions(scriptfile);
          this.scriptById[id] = new ScriptInfo(id, scriptfile, localMeta, localCode);
        }

        if (!localMeta.ContainsKey("version"))
          continue;

        string url;
        if (localMeta.ContainsKey("downloadUrl"))
          url = localMeta["downloadUrl"][0];
        else
          url = string.Format(DEFAULT_UPDATE_BASE_URL, Path.GetFileName(scriptfile));

        try
        {
          WebClient client = new WebClient();
          client.DownloadDataCompleted += Client_DownloadDataCompleted;
          client.DownloadDataAsync(new Uri(url), new object[] { scriptfile, localMeta });
        }
        catch (WebException) { }
      }
    }
    #endregion

    #region GetMetaData()
    private Dictionary<string, List<string>> GetMetadata(string script)
    {
      var metadata = new Dictionary<string, List<String>>();
      var start = script.IndexOf("// ==UserScript==");
      var end = script.IndexOf("// ==/UserScript==");
      if (start < 0 || end < 0)
      {
        start = script.IndexOf("/*");
        end = script.IndexOf("*/", start + 1);
      }
      if (start >= 0 && end >= 0)
      {
        var regex = new Regex("^\\s*//\\s*@(\\w+)\\s+(.*?)\\s*$");
        var lines = script.Substring(start, end - start + 1).Split('\n');
        foreach (var line in lines)
        {
          var match = regex.Match(line);
          if (!match.Success)
            continue;
          var key = match.Groups[1].Value;
          var value = match.Groups[2].Value;
          if (!metadata.ContainsKey(key))
            metadata[key] = new List<string>();

          if (key == "name")
            value = this.CleanupName(value);
          metadata[key].Add(value);
        }
      }
      return metadata;
    }

    private string CleanupName(string value)
    {
      var prefixes = new [] {"ql ", "quakelive ", "quake live "};
      var lower = value.ToLower();
      foreach (var prefix in prefixes)
      {
        if (lower.StartsWith(prefix))
          return value.Substring(prefix.Length);
      }
      return value;
    }

    #endregion

    #region StripAllExtenstions()
    private string StripAllExtenstions(string scriptfile)
    {
      var filename = Path.GetFileName(scriptfile) ?? "";
      var idx = filename.IndexOf('.');
      return idx < 0 ? filename : filename.Substring(0, idx);
    }
    #endregion

    #region Client_DownloadStringCompleted
    private void Client_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
    {
      ((WebClient)sender).Dispose();

      var scriptfile = (string) ((object[]) e.UserState)[0];
      var localMeta = (Dictionary<string, List<String>>) ((object[]) e.UserState)[1];
      if (e.Error != null)
      {
        Log("Failed to check version of " + Path.GetFileName(scriptfile) + ": " + e.Error.Message);
        return;
      }

      var remoteCode = utf8withoutBom.GetString(e.Result);
      var remoteMeta = GetMetadata(remoteCode);
      if (!remoteMeta.ContainsKey("version"))
        return;
      if (IsNewer(remoteMeta["version"][0], localMeta["version"][0]))
      {
        remoteCode = remoteCode.Replace("\n", Environment.NewLine);
        File.WriteAllText(scriptfile, remoteCode, utf8withoutBom);
        Log("Downloaded version " + remoteMeta["version"][0] + " of " + Path.GetFileName(scriptfile));
      }
    }

    #endregion

    #region IsNewer()
    internal static bool IsNewer(string version1, string version2)
    {
      var parts1 = version1.Split('.');
      var parts2 = version2.Split('.');
      for (int i = 0; i < parts1.Length; i++)
      {
        if (i >= parts2.Length)
          return true;
        int n1, n2;
        if (int.TryParse(parts1[i], out n1) && int.TryParse(parts2[i], out n2))
        {
          if (n1 > n2)
            return true;
          if (n1 < n2)
            return false;
        }
        else
        {
          var c = String.Compare(parts1[i], parts2[i], StringComparison.Ordinal);
          if (c < 0) return false;
          if (c > 0) return true;
        }
      }
      return false;
    }
    #endregion

    #region GetScripts()
    public List<ScriptInfo> GetScripts()
    {
      return new List<ScriptInfo>(scriptById.Values);
    }
    #endregion

    #region GetScriptByIdOrUrl()
    public ScriptInfo GetScriptByIdOrUrl(string scriptIdOrUrl)
    {
      ScriptInfo script;
      if (this.scriptById.TryGetValue(scriptIdOrUrl, out script))
      {
        // refresh script info it file was modified locally (e.g. while developing)
        if (script.Timestamp < new FileInfo(script.Filepath).LastWriteTimeUtc.Ticks)
        {
          var content = File.ReadAllText(script.Filepath);
          var meta = this.GetMetadata(content);
          script = new ScriptInfo(script.Id, script.Filepath, meta, content);
          this.scriptById[scriptIdOrUrl] = script;
        }
        return script;
      }

      // download script from external URL
      using (var webRequest = new WebClient())
      {
        webRequest.Encoding = Encoding.UTF8;
        string url = scriptIdOrUrl;
        try
        {
          string filename = Path.GetFileName(new Uri(url).AbsolutePath);
          var code = webRequest.DownloadString(url);
          var meta = this.GetMetadata(code);
          var id = meta.ContainsKey("id") ? meta["id"][0] : "";
          if (string.IsNullOrEmpty(id))
            id = this.StripAllExtenstions(filename);
          var filePath = ScriptDir + "\\" + StripAllExtenstions(filename) + ".usr.js";
          File.WriteAllText(filePath, code);
          script = new ScriptInfo(id, filePath, meta, code);
          this.Log("Downloaded script from " + url);
          this.scriptById[scriptIdOrUrl] = script;
        }
        catch (Exception)
        {
          this.Log("Failed to download script from " + url);
        }
        return script;
      }
    }
    #endregion
  }

  #region class ScriptInfo
  public class ScriptInfo
  {
    public readonly string Id;
    public readonly string Filepath;
    public readonly long Timestamp;
    public readonly Dictionary<string,List<string>> Metadata;
    public readonly string Code;

    public ScriptInfo(string id, string filepath, Dictionary<string, List<string>> metadata, string code)
    {
      this.Id = id;
      this.Filepath = filepath;
      this.Timestamp = new FileInfo(filepath).LastWriteTimeUtc.Ticks;
      this.Metadata = metadata;
      this.Code = code;
    }
  }
  #endregion
}
