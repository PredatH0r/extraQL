#undef USERSCRIPT_ORG

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
    private readonly Dictionary<string, ScriptInfo> scriptById = new Dictionary<string, ScriptInfo>();

    public ScriptRepository()
    {
      this.Log =  txt => { };

      ScriptDir = Path.GetDirectoryName(Application.ExecutablePath) ?? "";
      if (ScriptDir.EndsWith("\\bin\\Debug"))
        ScriptDir = Path.GetDirectoryName(Path.GetDirectoryName(ScriptDir));
      ScriptDir += "\\scripts";
    }

    public Action<string> Log { get; set; }

    public readonly string ScriptDir;

    #region UpdateScripts()
    public void UpdateScripts()
    {
      foreach (var scriptfile in Directory.GetFiles(ScriptDir, "*.usr.js"))
      {
        var localCode = File.ReadAllText(scriptfile);
        var localMeta = GetMetadata(localCode);

        var id = localMeta.ContainsKey("id") ? localMeta["id"][0] : "";
        if (string.IsNullOrEmpty(id))
          id = StripAllExtenstions(scriptfile);
        this.scriptById[id] = new ScriptInfo(id, scriptfile, localMeta, localCode);

        if (!localMeta.ContainsKey("version"))
          continue;

        int nr;
        string url;
        if (localMeta.ContainsKey("downloadUrl"))
          url = localMeta["downloadUrl"][0];
#if USERSCRIPT_ORG
        else if (int.TryParse(id, out nr))
          url = "http://userscripts.org:8080/scripts/source/" + nr + ".user.js";
#endif
        else
          url = "https://raw.githubusercontent.com/PredatH0r/extraQL/master/scripts/" + Path.GetFileName(scriptfile);

        try
        {
          WebClient client = new WebClient();
          client.DownloadStringCompleted += Client_DownloadStringCompleted;
          client.DownloadStringAsync(new Uri(url), new object[] { scriptfile, localMeta });
        }
        catch (WebException) { }
      }
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
    private void Client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
    {
      ((WebClient)sender).Dispose();

      var scriptfile = (string) ((object[]) e.UserState)[0];
      var localMeta = (Dictionary<string, List<String>>) ((object[]) e.UserState)[1];
      if (e.Error != null)
      {
        Log("Failed to check version of " + Path.GetFileName(scriptfile));
        return;
      }

      var remoteCode = e.Result;
      var remoteMeta = GetMetadata(remoteCode);
      if (!remoteMeta.ContainsKey("version"))
        return;
      if (IsNewer(remoteMeta["version"][0], localMeta["version"][0]))
      {
        File.WriteAllText(scriptfile, remoteCode);
        Log("Downloaded new version of " + Path.GetFileName(scriptfile));
      }
    }

    #endregion

    #region IsNewer()
    private bool IsNewer(string version1, string version2)
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

    #region GetMetaData()
    private Dictionary<string, List<string>> GetMetadata(string script)
    {
      var metadata = new Dictionary<string, List<String>>();
      var start = script.IndexOf("// ==UserScript==");
      var end = script.IndexOf("// ==/UserScript==");
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
          metadata[key].Add(value);
        }
      }
      return metadata;
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

      using (var webRequest = new WebClient())
      {
        webRequest.Encoding = Encoding.UTF8;
        bool isUsoId = Regex.IsMatch(scriptIdOrUrl, "\\d+");
        string url = isUsoId ? "http://userscripts.org/scripts/source/" + scriptIdOrUrl + ".user.js" : scriptIdOrUrl;
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

    #region GetScripts()
    public List<ScriptInfo> GetScripts()
    {
      return new List<ScriptInfo>(scriptById.Values);
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
