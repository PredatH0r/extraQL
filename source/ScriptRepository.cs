using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Timers;

namespace ExtraQL
{
  public class ScriptRepository
  {
    private const string SCRIPTINDEX_URL_FORMAT = "{0}/repository.json?all";
    private const string SCRIPTFILE_URL_FORMAT = "{0}/scripts/{1}";
    internal const string DEFAULT_DOWNLOADSOURCE_URL = "http://sourceforge.net/p/extraql/source/ci/master/tree/scripts/{0}?format=raw";
    private readonly Dictionary<string, ScriptInfo> scriptByFileName = new Dictionary<string, ScriptInfo>();
    private readonly Dictionary<string, ScriptInfo> scriptById = new Dictionary<string, ScriptInfo>();
    private readonly List<UpdateQueueItem> updateQueue = new List<UpdateQueueItem>();
    private readonly Timer updateTimer = new Timer();
    private readonly Encoding utf8withoutBom = new UTF8Encoding(false);
    private bool allowScriptDownload;
    private string masterServer;

    #region ctor()

    public ScriptRepository(string baseDir)
    {
      Log = txt => { };
      ScriptDir = baseDir + "/scripts";
      updateTimer.AutoReset = true;
      updateTimer.Elapsed += ProcessNextItemInUpdateQueue;
    }

    #endregion

    public Action<string> Log { get; set; }

    public string ScriptDir { get; private set; }

    #region RegisterScripts()

    public void RegisterScripts()
    {
      foreach (string scriptPath in Directory.GetFiles(ScriptDir, "*.js"))
      {
        string scriptFile = Path.GetFileName(scriptPath) ?? "";
        string localCode = File.ReadAllText(scriptPath);
        ScriptHeaderFields localMeta = ParseHeaderFields(localCode);
        var scriptInfo = new ScriptInfo(scriptPath, localCode, localMeta);
        scriptById[scriptInfo.Id] = scriptInfo;
        scriptByFileName[scriptFile] = scriptInfo;
      }
    }

    #endregion

    #region UpdateScripts()

    public void UpdateScripts(bool download, bool fromDownloadsourceUrl, string extraqlServer)
    {
      Log("Checking for updates...");
      masterServer = extraqlServer;
      allowScriptDownload = download;

      lock (this)
      {
        updateTimer.Stop();
        updateQueue.Clear();
      }

      if (fromDownloadsourceUrl || string.IsNullOrEmpty(extraqlServer))
        LoadUpdatesFromDownloadsource();
      else
        LoadUpdatesFromMasterServer();
    }

    #endregion

    #region LoadUpdatesFromDownloadsource()

    private void LoadUpdatesFromDownloadsource()
    {
      foreach (ScriptInfo script in scriptByFileName.Values)
      {
        if (script.Metadata.Get("version") == null)
          Log("Script has no @version: " + Path.GetFileName(script.Filepath));
        else
        {
          var queueItem = new UpdateQueueItem(script);
          if (script.IsUserscript)
            updateQueue.Add(queueItem);
          else
            updateQueue.Insert(0, queueItem); // update system relevant scripts first
        }
      }

      updateTimer.Interval = 500; // sourceforge silently drops requests when hammered
      updateTimer.Start();
    }

    #endregion

    #region LoadUpdatesFromMasterServer()

    private void LoadUpdatesFromMasterServer()
    {
      var client = new XWebClient(2000);
      client.DownloadStringCompleted += RepositoryJsonDownloadCompleted;
      var url = new Uri(string.Format(SCRIPTINDEX_URL_FORMAT, masterServer));
      client.DownloadStringAsync(url, new WebRequestState(url));
    }

    #endregion

    #region RepositoryJsonDownloadCompleted()

    private void RepositoryJsonDownloadCompleted(object sender, DownloadStringCompletedEventArgs e)
    {
      var client = (XWebClient) sender;
      var state = (WebRequestState) e.UserState;

      if (e.Error == null)
      {
        AddUpdatedScriptsFromMasterServerToQueue(e.Result);
        updateTimer.Interval = 100;
        updateTimer.Start();
      }
      else
      {
        if (state.Attempt++ < 3)
        {
          client.DownloadStringAsync(state.Uri, state);
          return;
        }
        Log("Update server on " + state.Uri + " is not responding, using sourceforge.com instead (slow)...");
        LoadUpdatesFromDownloadsource(); // fallback to script source
      }
    }

    #endregion

    #region AddUpdatedScriptsFromMasterServerToQueue()

    private void AddUpdatedScriptsFromMasterServerToQueue(string indexFileContent)
    {
      string[] files = indexFileContent.Split('\n', '\r');
      var regexFilename = new Regex(".*\"filename\":\"(.*?)\".*");
      var regexVersion = new Regex(".*\"version\":\"(.*?)\".*");
      foreach (string jsonLine in files)
      {
        Match matchVersion = regexVersion.Match(jsonLine);
        if (!matchVersion.Success)
          continue;
        Match matchFilename = regexFilename.Match(jsonLine);
        string remoteVersion = matchVersion.Groups[1].Value;
        string filename = matchFilename.Groups[1].Value;

        ScriptInfo localScript;
        scriptByFileName.TryGetValue(filename, out localScript);

        if (localScript == null || IsNewer(remoteVersion, localScript.Metadata.Get("version")))
        {
          string url = string.Format(SCRIPTFILE_URL_FORMAT, masterServer, filename);
          if (allowScriptDownload)
            updateQueue.Add(new UpdateQueueItem(url));
          else
            Log("New version " + remoteVersion + " of " + filename + " is available");
        }
      }
    }

    #endregion

    #region ProcessNextItemInUpdateQueue()

    private void ProcessNextItemInUpdateQueue(object sender, ElapsedEventArgs e)
    {
      lock (this)
      {
        if (updateQueue.Count == 0)
        {
          updateTimer.Stop();
          Log("Script update check completed");
          return;
        }

        UpdateQueueItem queueItem = updateQueue[0];
        updateQueue.Remove(queueItem);

        try
        {
          var client = new XWebClient(2500);
          client.DownloadDataCompleted += ScriptFile_DownloadDataCompleted;
          client.DownloadDataAsync(queueItem.Uri, queueItem);
        }
        catch (Exception ex)
        {
          Log("Failed to get script source from " + queueItem.Uri + ": " + ex.Message);
        }
      }
    }

    #endregion

    #region ScriptFile_DownloadDataCompleted

    private void ScriptFile_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
    {
      try
      {
        var queueItem = (UpdateQueueItem) e.UserState;
        if (e.Error != null)
        {
          Log("Failed to check version of " + queueItem.Uri + ": " + e.Error.Message);
          return;
        }

        string remoteCode = utf8withoutBom.GetString(e.Result);
        ScriptHeaderFields remoteMeta = ParseHeaderFields(remoteCode);
        string remoteVersion = remoteMeta.Get("version");
        if (remoteVersion == null)
        {
          Log("Missing version info in script header of " + queueItem.Uri);
          return;
        }
        if (queueItem.ScritpInfo == null || IsNewer(remoteVersion, queueItem.ScritpInfo.Metadata.Get("version")))
        {
          string scriptPath = queueItem.ScritpInfo != null ? queueItem.ScritpInfo.Filepath : Path.Combine(ScriptDir, Path.GetFileName(queueItem.Uri.LocalPath));
          string scriptFile = Path.GetFileName(scriptPath) ?? "";
          if (!allowScriptDownload)
          {
            Log("New version " + remoteVersion + " of " + scriptFile + " is available");
            return;
          }

          if (!remoteCode.Contains(Environment.NewLine))
            remoteCode = remoteCode.Replace("\n", Environment.NewLine);
          File.WriteAllText(scriptPath, remoteCode, utf8withoutBom);
          lock (this)
          {
            var newScriptInfo = new ScriptInfo(scriptPath, remoteCode, remoteMeta);
            scriptById[newScriptInfo.Id] = newScriptInfo;
            scriptByFileName[scriptFile] = newScriptInfo;
          }
          Log("Downloaded version " + remoteVersion + " of " + scriptFile);
        }
        else
          Log(Path.GetFileName(queueItem.ScritpInfo.Filepath) + " is up-to-date");
      }
      catch (Exception ex)
      {
        Log("Program error: " + ex);
      }
    }

    #endregion

    #region ParseHeaderFields()

    private ScriptHeaderFields ParseHeaderFields(string script)
    {
      var metadata = new ScriptHeaderFields();
      int start = script.IndexOf("// ==UserScript==");
      int end = script.IndexOf("// ==/UserScript==");
      if (start < 0 || end < 0)
      {
        start = script.IndexOf("/*");
        end = script.IndexOf("*/", start + 1);
      }
      if (start >= 0 && end >= 0)
      {
        var regex = new Regex("^\\s*//\\s*@(\\w+)\\s+(.*?)\\s*$");
        string[] lines = script.Substring(start, end - start + 1).Split('\n');
        foreach (string line in lines)
        {
          Match match = regex.Match(line);
          if (!match.Success)
            continue;
          string key = match.Groups[1].Value;
          string value = match.Groups[2].Value;

          metadata.Add(key, value);
        }
      }
      return metadata;
    }

    #endregion

    #region IsNewer()

    internal static bool IsNewer(string version1, string version2)
    {
      string[] parts1 = version1.Split('.');
      string[] parts2 = version2.Split('.');
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
          int c = String.Compare(parts1[i], parts2[i], StringComparison.Ordinal);
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
      lock (this)
      {
        ScriptInfo script;
        if (scriptById.TryGetValue(scriptIdOrUrl, out script))
        {
          // refresh script info it file was modified locally (e.g. while developing)
          if (script.Timestamp < new FileInfo(script.Filepath).LastWriteTimeUtc.Ticks)
          {
            string content = File.ReadAllText(script.Filepath);
            ScriptHeaderFields meta = ParseHeaderFields(content);
            script = new ScriptInfo(script.Filepath, content, meta);
            scriptById[scriptIdOrUrl] = script;
          }
          return script;
        }
      }

      return null;
#if false
  // download script from external URL
      using (var webRequest = new XWebClient())
      {
        webRequest.Encoding = Encoding.UTF8;
        string url = scriptIdOrUrl;
        try
        {
          string filename = Path.GetFileName(new Uri(url).AbsolutePath);
          var code = webRequest.DownloadString(url);
          var meta = this.ParseHeaderFields(code);
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
#endif
    }

    #endregion
  }

  #region class ScriptInfo

  public class ScriptInfo
  {
    public readonly string Code;
    public readonly string DownloadUrl;
    public readonly string Filepath;
    public readonly string Id;
    public readonly bool IsUserscript;
    public readonly ScriptHeaderFields Metadata;
    public readonly string Name;
    public readonly long Timestamp;

    public ScriptInfo(string filepath, string code, ScriptHeaderFields metadata)
    {
      string fileName = Path.GetFileName(filepath) ?? "";
      string baseName = StripAllExtenstions(fileName);
      Id = metadata.Get("id") ?? baseName;
      Name = CleanupName(metadata.Get("name") ?? baseName);
      Filepath = filepath;
      Timestamp = new FileInfo(filepath).LastWriteTimeUtc.Ticks;
      Metadata = metadata;
      Code = code;
      DownloadUrl = metadata.Get("downloadURL") ?? string.Format(ScriptRepository.DEFAULT_DOWNLOADSOURCE_URL, fileName);
      IsUserscript = filepath.EndsWith(".usr.js");
    }

    private string StripAllExtenstions(string scriptfile)
    {
      string filename = Path.GetFileName(scriptfile) ?? "";
      int idx = filename.IndexOf('.');
      return idx < 0 ? filename : filename.Substring(0, idx);
    }

    private string CleanupName(string value)
    {
      var prefixes = new[] {"ql ", "quakelive ", "quake live "};
      string lower = value.ToLower();
      foreach (string prefix in prefixes)
      {
        if (lower.StartsWith(prefix))
          return value.Substring(prefix.Length);
      }
      return value;
    }
  }

  #endregion

  #region class ScriptHeaderFields

  public class ScriptHeaderFields
  {
    private readonly Dictionary<string, List<string>> data = new Dictionary<string, List<string>>();

    public IEnumerable<string> Keys
    {
      get { return data.Keys; }
    }

    public void Add(string key, string value)
    {
      if (!data.ContainsKey(key))
        data.Add(key, new List<string>());
      List<string> values = data[key];
      values.Add(value);
    }

    public string Get(string key, int index = 0)
    {
      if (!data.ContainsKey(key))
        return null;
      List<string> values = data[key];
      return index < values.Count ? values[index] : null;
    }
  }

  #endregion

  #region class WebRequestState

  internal class WebRequestState
  {
    public readonly Uri Uri;
    public int Attempt;

    public WebRequestState(Uri uri)
    {
      Uri = uri;
    }
  }

  #endregion

  #region class UpdateQueueItem

  internal class UpdateQueueItem
  {
    public readonly ScriptInfo ScritpInfo;
    public readonly Uri Uri;

    public UpdateQueueItem(ScriptInfo scriptInfo)
    {
      Uri = new Uri(scriptInfo.DownloadUrl);
      ScritpInfo = scriptInfo;
    }

    public UpdateQueueItem(string downloadUrl)
    {
      Uri = new Uri(downloadUrl);
    }
  }

  #endregion
}