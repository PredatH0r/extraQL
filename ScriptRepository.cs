using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Timers;
using System.Windows.Forms;

namespace ExtraQL
{
  public class ScriptRepository
  {
    private const string SCRIPTINDEX_URL_FORMAT = "http://{0}/repository.json?all";
    private const string SCRIPTFILE_URL_FORMAT = "http://{0}/scripts/{1}";
    internal const string DEFAULT_DOWNLOADSOURCE_URL = "http://sourceforge.net/p/extraql/source/ci/master/tree/scripts/{0}?format=raw";
    private readonly Dictionary<string, ScriptInfo> scriptById = new Dictionary<string, ScriptInfo>();
    private readonly Dictionary<string, ScriptInfo> scriptByFileName = new Dictionary<string, ScriptInfo>();
    private readonly Encoding utf8withoutBom = new UTF8Encoding(false);
    private readonly System.Timers.Timer updateTimer = new System.Timers.Timer();
    private readonly List<UpdateQueueItem> updateQueue = new List<UpdateQueueItem>();
    private bool convertNewline;
    private string masterServer;

    #region ctor()
    public ScriptRepository()
    {
      this.Log = txt => { };

      this.ScriptDir = Path.GetDirectoryName(Application.ExecutablePath) ?? "";
      if (this.ScriptDir.Replace('\\','/').EndsWith("/bin/Debug"))
        this.ScriptDir = Path.GetDirectoryName(Path.GetDirectoryName(ScriptDir));
      this.ScriptDir += "/scripts";
      updateTimer.AutoReset = true;
      updateTimer.Elapsed += ProcessNextItemInUpdateQueue;
    }
    #endregion

    public Action<string> Log { get; set; }

    public string ScriptDir { get; private set; }

    #region RegisterScripts()

    public void RegisterScripts()
    {
      foreach (var scriptPath in Directory.GetFiles(ScriptDir, "*.js"))
      {
        var scriptFile = Path.GetFileName(scriptPath) ?? "";
        var localCode = File.ReadAllText(scriptPath);
        var localMeta = this.ParseHeaderFields(localCode);
        var scriptInfo = new ScriptInfo(scriptPath, localCode, localMeta);
        this.scriptById[scriptInfo.Id] = scriptInfo;
        this.scriptByFileName[scriptFile] = scriptInfo;
      }
    }

    #endregion

    #region UpdateScripts()

    public void UpdateScripts(bool fromDownloadsourceUrl, string extraqlServer)
    {
      Log("Checking for updates...");
      this.masterServer = extraqlServer;

      lock (this)
      {
        this.updateTimer.Stop();
        this.updateQueue.Clear();
      }

      if (fromDownloadsourceUrl || string.IsNullOrEmpty(extraqlServer))
        this.LoadUpdatesFromDownloadsource();
      else
        this.LoadUpdatesFromMasterServer();
    }
    #endregion

    #region LoadUpdatesFromDownloadsource()
    private void LoadUpdatesFromDownloadsource()
    {
      foreach (var script in this.scriptByFileName.Values)
      {
        if (script.Metadata.Get("version") == null)
          Log("Script has no @version: " + Path.GetFileName(script.Filepath));
        else
        {
          var queueItem = new UpdateQueueItem(script);
          if (script.Id == "hook" || script.Id == "extraQL")
            this.updateQueue.Insert(0, queueItem); // update system relevant scripts first so they're in place when QL starts
          else
            this.updateQueue.Add(queueItem);
        }
      }

      this.convertNewline = true;
      this.updateTimer.Interval = 5000; // sourceforge silently drops requests when hammered
      this.updateTimer.Start();
      // process hook.js and extraQL.js immediately (which are the first two)
      this.ProcessNextItemInUpdateQueue(this.updateTimer, null);
      this.ProcessNextItemInUpdateQueue(this.updateTimer, null);
    }
    #endregion

    #region LoadUpdatesFromMasterServer()
    private void LoadUpdatesFromMasterServer()
    {
      var client = new XWebClient(5000);
      client.DownloadStringCompleted += RepositoryJsonDownloadCompleted;
      var url = new Uri(string.Format(SCRIPTINDEX_URL_FORMAT, this.masterServer));
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
        this.AddUpdatedScriptsFromMasterServerToQueue(e.Result);
        this.convertNewline = false;
        this.updateTimer.Interval = 100;
        this.updateTimer.Start();
      }
      else
      {
        if (state.Attempt++ < 3)
        {
          client.DownloadStringAsync(state.Uri, state);
          return;
        }
        Log("Update server not responding, checking scripts one-by-one...");
        this.LoadUpdatesFromDownloadsource(); // fallback to script source
      }
      client.Dispose();

    }
    #endregion

    #region AddUpdatedScriptsFromMasterServerToQueue()
    private void AddUpdatedScriptsFromMasterServerToQueue(string indexFileContent)
    {
      var files = indexFileContent.Split('\n', '\r');
      Regex regexFilename = new Regex(".*\"filename\":\"(.*?)\".*");
      Regex regexVersion = new Regex(".*\"version\":\"(.*?)\".*");
      foreach (var jsonLine in files)
      {
        var matchVersion = regexVersion.Match(jsonLine);
        if (!matchVersion.Success)
          continue;
        var matchFilename = regexFilename.Match(jsonLine);
        var remoteVersion = matchVersion.Groups[1].Value;
        var filename = matchFilename.Groups[1].Value;

        ScriptInfo localScript;
        this.scriptByFileName.TryGetValue(filename, out localScript);

        if (localScript == null || IsNewer(remoteVersion, localScript.Metadata.Get("version")))
        {
          string url = string.Format(SCRIPTFILE_URL_FORMAT, this.masterServer, filename);
          this.updateQueue.Add(new UpdateQueueItem(url));
        }
        //else
        //  Log(filename + " is up-to-date");
      }
    }
    #endregion

    #region ProcessNextItemInUpdateQueue()
    private void ProcessNextItemInUpdateQueue(object sender, ElapsedEventArgs e)
    {
      lock (this)
      {
        if (this.updateQueue.Count == 0)
        {
          this.updateTimer.Stop();
          this.Log("Scripts are up-to-date");
          return;
        }

        var queueItem = this.updateQueue[0];
        this.updateQueue.Remove(queueItem);

        try
        {
          WebClient client = new XWebClient(1000);
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
        ((WebClient)sender).Dispose();
        var queueItem = (UpdateQueueItem) e.UserState;
        if (e.Error != null)
        {
          Log("Failed to check version of " + queueItem.Uri + ": " + e.Error.Message);
          return;
        }

        var remoteCode = utf8withoutBom.GetString(e.Result);
        var remoteMeta = this.ParseHeaderFields(remoteCode);
        var remoteVersion = remoteMeta.Get("version");
        if (remoteVersion == null)
          return;
        if (queueItem.ScritpInfo == null || IsNewer(remoteVersion, queueItem.ScritpInfo.Metadata.Get("version")))
        {
          string scriptPath = queueItem.ScritpInfo != null ? queueItem.ScritpInfo.Filepath : Path.Combine(this.ScriptDir, Path.GetFileName(queueItem.Uri.LocalPath));
          string scriptFile = Path.GetFileName(scriptPath) ?? "";
          if (convertNewline)
            remoteCode = remoteCode.Replace("\n", Environment.NewLine);
          File.WriteAllText(scriptPath, remoteCode, utf8withoutBom);
          lock (this)
          {
            var newScriptInfo = new ScriptInfo(scriptPath, remoteCode, remoteMeta);
            this.scriptById[newScriptInfo.Id] = newScriptInfo;
            this.scriptByFileName[scriptFile] = newScriptInfo;
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

          metadata.Add(key, value);
        }
      }
      return metadata;
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
      lock (this)
      {
        ScriptInfo script;
        if (this.scriptById.TryGetValue(scriptIdOrUrl, out script))
        {
          // refresh script info it file was modified locally (e.g. while developing)
          if (script.Timestamp < new FileInfo(script.Filepath).LastWriteTimeUtc.Ticks)
          {
            var content = File.ReadAllText(script.Filepath);
            var meta = this.ParseHeaderFields(content);
            script = new ScriptInfo(script.Filepath, content, meta);
            this.scriptById[scriptIdOrUrl] = script;
          }
          return script;
        }
      }

      return null;
#if false
      // download script from external URL
      using (var webRequest = new WebClient())
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
    public readonly string Id;
    public readonly string Name;
    public readonly string Filepath;
    public readonly long Timestamp;
    public readonly ScriptHeaderFields Metadata;
    public readonly string Code;
    public readonly string DownloadUrl;
    public readonly bool IsUserscript;

    public ScriptInfo(string filepath, string code, ScriptHeaderFields metadata)
    {
      var fileName = Path.GetFileName(filepath) ?? "";
      var baseName = this.StripAllExtenstions(fileName);
      this.Id = metadata.Get("id") ?? baseName;
      this.Name = CleanupName(metadata.Get("name") ?? baseName);
      this.Filepath = filepath;
      this.Timestamp = new FileInfo(filepath).LastWriteTimeUtc.Ticks;
      this.Metadata = metadata;
      this.Code = code;
      this.DownloadUrl = metadata.Get("downloadUrl") ?? string.Format(ScriptRepository.DEFAULT_DOWNLOADSOURCE_URL, fileName);
      this.IsUserscript = filepath.EndsWith(".usr.js");
    }

    private string StripAllExtenstions(string scriptfile)
    {
      var filename = Path.GetFileName(scriptfile) ?? "";
      var idx = filename.IndexOf('.');
      return idx < 0 ? filename : filename.Substring(0, idx);
    }

    private string CleanupName(string value)
    {
      var prefixes = new[] { "ql ", "quakelive ", "quake live " };
      var lower = value.ToLower();
      foreach (var prefix in prefixes)
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

    public void Add(string key, string value)
    {
      if (!data.ContainsKey(key))
        data.Add(key, new List<string>());
      var values = data[key];
      values.Add(value);
    }

    public string Get(string key, int index = 0)
    {
      if (!data.ContainsKey(key))
        return null;
      var values = data[key];
      return index < values.Count ? values[index] : null;
    }

    public IEnumerable<string> Keys { get { return data.Keys; }}
  }
  #endregion

  #region class WebRequestState
  class WebRequestState
  {
    public readonly Uri Uri;
    public int Attempt;

    public WebRequestState(Uri uri)
    {
      this.Uri = uri;
    }
  }
  #endregion

  #region class UpdateQueueItem
  class UpdateQueueItem
  {
    public readonly Uri Uri;
    public readonly ScriptInfo ScritpInfo;

    public UpdateQueueItem(ScriptInfo scriptInfo)
    {
      this.Uri = new Uri(scriptInfo.DownloadUrl);
      this.ScritpInfo = scriptInfo;
    }

    public UpdateQueueItem(string downloadUrl)
    {
      this.Uri = new Uri(downloadUrl);
    }
  }
  #endregion
}
