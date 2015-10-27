using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace ExtraQL
{
  public class ScriptRepository
  {
    private readonly Dictionary<string, ScriptInfo> scriptById = new Dictionary<string, ScriptInfo>();

    #region ctor()

    public ScriptRepository(string baseDir)
    {
      Log = txt => { };
      ScriptDir = baseDir + "/scripts";
    }

    #endregion

    public Action<string> Log { get; set; }

    public string ScriptDir { get; }

    #region RegisterScripts()

    public void RegisterScripts()
    {
      foreach (string scriptPath in Directory.GetFiles(ScriptDir, "*.js"))
      {
        string localCode = File.ReadAllText(scriptPath);
        ScriptHeaderFields localMeta = ParseHeaderFields(localCode);
        var scriptInfo = new ScriptInfo(scriptPath, localCode, localMeta);
        scriptById[scriptInfo.Id] = scriptInfo;
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

      return null;
    }

    #endregion
  }

  #region class ScriptInfo

  public class ScriptInfo
  {
    public readonly string Code;
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
}