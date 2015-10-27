using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Windows.Forms;

namespace ExtraQL
{
  internal class Servlets
  {
    private static readonly string[] DomainsAllowedForProxy = { "esreality.com", "quakelive.com", "github.com" };
    private const string AddScriptRoute = "/addScript";
    private const string CondumpFile = "extraql_condump.txt";
    private readonly HttpServer server;
    private readonly StringBuilder indexBuilder = new StringBuilder();
    private readonly string baseDir;
    private readonly ScriptRepository scriptRepository;
    private readonly Form form;
    private string joinServer, joinPass;

    public Action<string> Log;
    
    #region ctor()

    public Servlets(HttpServer server, ScriptRepository scriptRepository, Action<string> logger, Form form, string baseDir)
    {
      this.server = server;
      this.scriptRepository = scriptRepository;
      this.form = form;
      this.EnableScripts = true;
      this.EnablePrivateServlets = true;
      server.Log = logger;
      this.Log = server.Log;
      this.baseDir = baseDir;
      RegisterServlets();
    }

    #endregion

    public bool EnablePrivateServlets { get; set; }

    public string QuakeConfigFolder { get; set; }

    public string QuakeSteamFolder { get; set; }

    #region RegisterServlets()

    /// <summary>
    ///   Register all supported servlet URLs with the HTTP server
    /// </summary>
    private void RegisterServlets()
    {
      RegisterServlet("/", Index);
      RegisterServlet("/version", Version);
      RegisterServlet("/scripts", GetLocalScript);
      RegisterServlet("/images", GetImage);
      RegisterServlet("/data", DataStorage);
      RegisterServlet("/proxy", Proxy);
      RegisterServlet("/toggleFullscreen", ToggleFullscreen);
      RegisterServlet("/dockWindow", DockWindow);
      RegisterServlet("/log", ScriptLog);
      RegisterServlet(AddScriptRoute, AddScript);
      RegisterServlet("/bringToFront", BringToFront);
      RegisterServlet("/repository.json", RepositoryJson);
      RegisterServlet("/extraQL.exe", ExtraQlExe);
      RegisterServlet("/condump", GetCondump);
      RegisterServlet("/serverinfo", GetServerInfo);
      RegisterServlet("/join", JoinGame);
      RegisterServlet("/demos", ListDemos);
      RegisterServlet("/steamnick", SetSteamNick);
    }

    #endregion

    #region RegisterServlet()

    /// <summary>
    ///   Register a particular servlet with the HTTP Server and add it to the index page
    /// </summary>
    private void RegisterServlet(string path, HttpServer.Servlet servlet)
    {
      server.RegisterServlet(path, servlet);
      if (path != "/")
        indexBuilder.Append("<a href='").Append(path).Append("'>").Append(path).Append("</a><br>");
    }

    #endregion

    // service methods

    #region Index()

    /// <summary>
    ///   returns a HTML page with the registered servlet URLs, in case the user opens the extraQL HTTP URL in a browser
    /// </summary>
    private void Index(Stream stream, Uri uri, string request)
    {
      HttpOk(stream, "<html><body><h1>extraQL script server</h1>" + indexBuilder + "</body></html>");
    }
    #endregion

    #region Version()

    /// <summary>
    ///   Returns the extraQL version number.
    ///   Used by the extraQL user scripts to test wheter the local server is running.
    /// </summary>
    private void Version(Stream stream, Uri uri, string request)
    {
      HttpOk(stream, "{ \"version\": \"" + MainForm.Version + "\", \"enabled\": " + this.EnableScripts.ToString().ToLower() + " }");
    }

    #endregion

    #region GetLocalScript()

    /// <summary>
    ///   returns a script or list of script names from the "/scripts" folder
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="uri"></param>
    /// <param name="request"></param>
    private void GetLocalScript(Stream stream, Uri uri, string request)
    {
      DeliverFileOrDirectoryListing(stream, uri, "scripts", @".*\.usr\.js$", "text/javascript");
    }

    #endregion

    #region GetImage()

    /// <summary>
    ///   Returns a binary file from the "/images" folder
    /// </summary>
    private void GetImage(Stream stream, Uri uri, string request)
    {
      DeliverFileOrDirectoryListing(stream, uri, "images", "*", "image/png");
    }

    #endregion

    #region DataStorage()

    /// <summary>
    ///   Retrieve (GET) or write (POST) a file from/to the "/data" folder
    /// </summary>
    private void DataStorage(Stream stream, Uri uri, string request)
    {
      if (!this.EnablePrivateServlets)
      {
        HttpForbidden(stream);
        return;
      }

      string path = GetFilePath(uri, "data");
      if (path == null)
        return;

      string response = "";
      int idx = request.IndexOf("\r\n\r\n", StringComparison.Ordinal);
      if (request.StartsWith("POST"))
      {
        string dir = Path.GetDirectoryName(path) ?? "";
        Directory.CreateDirectory(dir);
        var utf8 = new UTF8Encoding(false); // UTF8 without a BOM
        File.WriteAllText(path, request.Substring(idx + 4), utf8);
        response = "Ok";
      }
      else if (request.StartsWith("GET"))
      {
        if (File.Exists(path))
          response = File.ReadAllText(path, Encoding.UTF8);
      }
      HttpOk(stream, response);
    }

    #endregion

    #region Proxy()

    /// <summary>
    ///   Relays the URL specified in the "url" parameter and returns the result.
    ///   This allows bypassing the "same-origin-policy" of browsers and QL and request
    ///   HTML data from external web sites like esreality.com
    /// </summary>
    private void Proxy(Stream stream, Uri uri, string request)
    {
      NameValueCollection args = HttpUtility.ParseQueryString(uri.Query);
      var url = args.Get("url");
      if (string.IsNullOrEmpty(url))
        HttpOk(stream, "missing 'url' parameter");
      else
      {
        string targetHost = "";
        try { targetHost = new Uri(url).Host; }
        catch { }
        if (Array.Find(DomainsAllowedForProxy, targetHost.EndsWith) == null)
        {
          this.HttpForbidden(stream, "requested domain is not supported");
          return;
        }
        string text = DownloadText(url);
        HttpOk(stream, text);
      }
    }

    #endregion

    #region ToggleFullscreen()

    /// <summary>
    ///   Send Alt+Enter key stroke to QL window
    /// </summary>
    private void ToggleFullscreen(Stream stream, Uri uri, string request)
    {
      if (!this.EnablePrivateServlets)
      {
        HttpForbidden(stream);
        return;
      }

      NameValueCollection args = HttpUtility.ParseQueryString(uri.Query);
      int mode;
      if (!int.TryParse(args.Get("mode"), out mode))
        return;

      var hWnd = QLWindowHandle;
      Win32.RECT rect;
      Win32.GetWindowRect(hWnd, out rect);
      var screen = Screen.FromHandle(hWnd);
      bool isFullscreen = rect.Width == screen.Bounds.Width && rect.Height == screen.Bounds.Height;

      bool wantFullscreen = mode == 1;
      if (isFullscreen != wantFullscreen)
        Win32.PostMessage(QLWindowHandle, Win32.WM_SYSKEYDOWN, (int)Keys.Enter, 43 << 16);
      HttpOk(stream, "Ok");
    }

    #endregion

    #region DockWindow()

    /// <summary>
    ///   Resize/Move QL window
    ///   Parameters:
    ///   sides (bitmask):
    ///     0 = 1024x768 @ current position
    ///     0x01 = left screen edge, 0x02 = right screen edge, 0x03 = full screen width
    ///     0x04 = top screen edge, 0x08 = bottom screen edge, 0x0C = full screen height
    ///   w: width for docking left/right
    ///   h: height for docking top/bottom
    /// 
    ///   If this function is called twice with the same argument, it will toggle the width
    ///   between the specified value and that value + 304 to make extra space for the chat area
    /// </summary>
    private void DockWindow(Stream stream, Uri uri, string request)
    {
      if (!this.EnablePrivateServlets)
      {
        HttpForbidden(stream);
        return;
      }

      NameValueCollection args = HttpUtility.ParseQueryString(uri.Query);
      int sides, w, h;
      if (!int.TryParse(args.Get("sides"), out sides))
      {
        HttpOk(stream, "'sides' parameter missing");
        return;
      }
      if (!int.TryParse(args.Get("w"), out w)) w = 1328;
      if (w < 1024) w = 1024;
      if (!int.TryParse(args.Get("h"), out h) || h < 768) h = 768;

      IntPtr handle = QLWindowHandle;

      // get current window position
      Win32.RECT oldRect;
      Win32.GetWindowRect(handle, out oldRect);
      int x = oldRect.Left;
      int y = oldRect.Top;
      int cx = oldRect.Width;
      int cy = oldRect.Height;

      // get size of window border
      uint style = Win32.GetWindowLong(handle, Win32.GWL_STYLE);
      var rect = new Win32.RECT();
      Win32.AdjustWindowRect(ref rect, style, false);

      if (sides == 0)
      {
        if (cx == 1328 + rect.Width)
          cx = 1024 + rect.Width;
        else
          cx = 1328 + rect.Width;
        cy = 768 + rect.Height;
      }
      else
      {
        Screen screen = Screen.FromHandle(handle);
        if ((sides & 0x01) != 0)
          x = screen.WorkingArea.Left;
        if ((sides & 0x03) == 0x03)
        {
          cx = screen.WorkingArea.Width;
          cy = (cy == h + rect.Height ? 768 : h) + rect.Height;
        }

        if ((sides & 0x04) != 0)
          y = screen.WorkingArea.Top;
        if ((sides & 0x0C) == 0x0C)
        {
          cy = screen.WorkingArea.Height;
          cx = (cx == w + rect.Width ? 1024 : w) + rect.Width;
        }

        if ((sides & 0x03) == 0x02)
          x = screen.WorkingArea.Right - cx;
        if ((sides & 0x0C) == 0x08)
          y = screen.WorkingArea.Bottom - cy;
      }

      // set new window size and notify QL that the size has changed
      const int flags = Win32.SWP_NOACTIVATE | Win32.SWP_NOOWNERZORDER | Win32.SWP_NOZORDER;
      Win32.SetWindowPos(handle, -1, x, y, cx, cy, flags);
      Win32.SendMessage(handle, Win32.WM_EXITSIZEMOVE, 0, 0);

      Point pos = Cursor.Position;
      pos.Offset(x - oldRect.Left, y - oldRect.Top);
      Cursor.Position = pos;

      HttpOk(stream, "ok");
    }

    #endregion

    #region ScriptLog()

    /// <summary>
    ///   Write a log message to the extraQL window.
    ///   Allows logging of large text messages independent of web- or in-game-console
    ///   and copy/paste the logged message
    /// </summary>
    private void ScriptLog(Stream stream, Uri uri, string request)
    {
      if (!this.EnablePrivateServlets)
      {
        HttpForbidden(stream);
        return;
      }

      int idx = request.IndexOf("\r\n\r\n", StringComparison.Ordinal);
      string msg = idx < 0 ? "" : request.Substring(idx + 4);
      if (!request.StartsWith("POST"))
        HttpOk(stream, "POST data with log message missing");
      else
      {
        Log("Script log:\r\n" + msg);
        HttpOk(stream, "");
      }
    }

    #endregion

    #region AddScript()
    /// <summary>
    /// Download a script from an external URL and return a JSON with the meta information
    /// </summary>
    private void AddScript(Stream stream, Uri uri, string request)
    {
#if false
      var args = HttpUtility.ParseQueryString(uri.Query);
      string text = "";
      string scriptId = uri.AbsolutePath.StartsWith(AddScriptRoute+"/") ? uri.AbsolutePath.Substring(AddScriptRoute.Length+1) : null;
      
      var scriptInfo = string.IsNullOrEmpty(scriptId) ? null : this.scriptRepository.GetScriptByIdOrUrl(scriptId);
      if (scriptInfo == null)
      {
        var writer = new StreamWriter(stream.GetStream());
        writer.WriteLine("HTTP/1.1 400 Not Found");
        writer.WriteLine("Access-Control-Allow-Origin: *");
        writer.WriteLine();
        writer.WriteLine("Script with ID " + scriptId + " not found");
        writer.Flush();
        return;
      }

      text += "{\"id\":\"" + scriptId + "\"";
      text += ",\"filename\":\"" + Path.GetFileName(scriptInfo.Filepath) + "\"";
      text += ",\"headers\":{";
      var sep1 = "";
      foreach (var entry in scriptInfo.Metadata)
      {
        text += sep1 + "\"" + entry.Key + "\":[";
        sep1 = ",";
        var sep2 = "";
        foreach (var value in entry.Value)
        {
          text += sep2 + "\"" + value.Replace("\"", "\\\"") + "\"";
          sep2 = ",";
        }
        text += "]";
      }
      text += "}";
      text += ",\"content\":\"" + scriptInfo.Code.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "").Replace("\t", "\\t") + "\"";
      text += "}";

      if (args.Get("callback") != null)
        text = args.Get("callback") + "(" + text + ");";

      this.HttpOk(stream, text);
#else
      HttpNotImplemented(stream);
#endif
    }
    #endregion

    #region BringToFront()
    private void BringToFront(Stream stream, Uri uri, string request)
    {
      if (!this.EnablePrivateServlets)
      {
        HttpForbidden(stream);
        return;
      }

      form.BeginInvoke((ThreadStart)(() =>
      {
        this.form.WindowState = FormWindowState.Normal;
        this.form.BringToFront();
        this.form.Activate();
      }));
      this.HttpOk(stream, "ok");
    }
    #endregion

    #region RepositoryJson()
    /// <summary>
    ///   Locally serve QLHM's JSONP calls, which are normally handled via http://qlhm.phob.net/uso/:id
    ///   The returned object contains the script ID in ._meta.id, the metadata in .headers[fieldname][]
    ///   and the actual script code in .content
    /// </summary>
    private void RepositoryJson(Stream stream, Uri uri, string request)
    {
      string text = "[";
      string sep = "";
      bool all = uri.Query == "?all";
      foreach (var info in this.scriptRepository.GetScripts())
      {
        if (!all && !info.IsUserscript)
          continue;
        text += sep + "\n{\"id\":\"" + info.Id + "\"";
        text += ",\"filename\":\"" + Path.GetFileName(info.Filepath) + "\"";
        foreach (var key in info.Metadata.Keys)
        {
          var value = info.Metadata.Get(key); 
          switch (key)
          {
            case "id": continue;
            case "name": value = info.Name; break;
          }        
          text += ",\"" + key + "\":\"" + value.Replace("\\","\\\\").Replace("\"", "\\\"") + "\"";
        }
        text += "}";
        sep = ",";
      }
      text += "\n]";
      this.HttpOk(stream, text);
    }
    #endregion

    #region ExtraQlExe()
    /// <summary>
    /// Deliver the .exe binary (for auto-update of downstream extraQL.exe clients)
    /// </summary>
    private void ExtraQlExe(Stream stream, Uri uri, string request)
    {
      var data = File.ReadAllBytes(Application.ExecutablePath);

      var writer = new StreamWriter(stream);
      writer.WriteLine("HTTP/1.1 200 OK");
      writer.WriteLine("Access-Control-Allow-Origin: *"); // allow QL scripts to request URLs from this server
      writer.WriteLine("Content-Length: " + data.Length);
      writer.WriteLine("Content-Type: application/octet-stream");
      writer.WriteLine();
      writer.Flush();
      stream.Write(data, 0, data.Length);
    }
    #endregion

    #region GetCondump()
    private void GetCondump(Stream stream, Uri uri, string request)
    {
      var file = this.QuakeConfigFolder + "\\" + CondumpFile;
      string content = File.Exists(file) ? File.ReadAllText(file) : "";
      HttpOk(stream, content);
    }
    #endregion

    #region GetServerInfo()
    private void GetServerInfo(Stream stream, Uri uri, string request)
    {
      var file = this.QuakeConfigFolder + "\\" + CondumpFile;
      var lines = ReadUpdatedFile(file);
      if (lines == null)
      {
        HttpOk(stream, "{\"error\":\"" + CondumpFile + " is old or doesn't exist.\"}");
        return;       
      }

      // find line with last /configstrings command
      int i;
      for (i = lines.Length - 1; i >= 0; i--)
        if (lines[i].EndsWith("]\\configstrings"))
          break;

      if (i < 0)
      {
        HttpOk(stream, "{\"error\":\"Can't find configstrings in condump.\"}");
        return;
      }

      // search for lines with index 529 - 553
      var info = ExtractConfigstrings(i, lines);
      var json = GenerateClientinfoJson(info);
      HttpOk(stream, json);
    }

    private string[] ReadUpdatedFile(string file)
    {
      for (int attempt = 0; attempt < 30; attempt++)
      {
        if (File.Exists(file) && (DateTime.Now - File.GetLastWriteTime(file)).TotalSeconds < 5)
        {
          try
          {
            var lines = File.ReadAllLines(file);
            return lines;
          }
          catch
          {
            // file may still be locked by QL for writing
          }
        }
        Thread.Sleep(100);
      }
      return null;
    }

    private static Dictionary<int, string> ExtractConfigstrings(int i, string[] lines)
    {
      int index = -1;
      string value = "";
      var info = new Dictionary<int, string>();
      for (; i < lines.Length; i++)
      {
        string line = lines[i];
        if (line.Length >= 4 && line[0] == ' ' && line[4] == ':')
        {
          if (index == 0 || index >= 529 && index < 529 + 32)
            info.Add(index, value);
          index = int.Parse(line.Substring(1, 3));
          if (index >= 529 + 32)
            break;
          value = line.Substring(5).Trim();
        }
        else if (index >= 0)
          value += line.TrimEnd();
      }
      return info;
    }

    private string GenerateClientinfoJson(Dictionary<int, string> info)
    {
      StringBuilder sb = new StringBuilder();
      sb.Append("{\"error\":false");
      sb.Append(",\"gameinfo\":{").Append(FieldListToJson(info[0].Substring(1))).Append('}');
      sb.Append(",\"players\":[");
      var sep = "";
      foreach (var entry in info)
      {
        if (entry.Key >= 529 && entry.Key < 529 + 32)
        {
          sb.Append(sep);
          sb.Append("{\"clientid\":\"").Append(entry.Key - 529).Append("\",");
          sb.Append(FieldListToJson(entry.Value));
          sb.Append("}");
          sep = ",";
        }
      }
      sb.Append("]}");
      return sb.ToString();
    }

    private string FieldListToJson(string fieldList)
    {
      StringBuilder sb = new StringBuilder();
      var fields = fieldList.Split('\\');
      for (int i = 0; i+1 < fields.Length; i += 2)
      {
        if (i > 0)
          sb.Append(',');
        sb.Append('"').Append(fields[i]).Append("\":\"").Append(fields[i + 1]).Append('"');
      }
      return sb.ToString();
    }
    #endregion

    #region JoinGame()

    /// <summary>
    /// Stores/returns server and password for joining a game
    /// External apps can show links like http://127.0.0.1:27963/join/91.198.152.211:27003/tdm
    /// and when the user clicks on it, the browser calls the servlet which stores server and password.
    /// The joinGame.usr.js userscript inside QL polls this information and connects you to the server.
    /// </summary>
    private void JoinGame(Stream stream, Uri uri, string request)
    {
      if (!this.EnablePrivateServlets)
      {
        HttpForbidden(stream);
        return;
      }

      int idx = uri.LocalPath.IndexOf("/", 1);
      if (idx >= 0) // store server/pass
      {
        string path = uri.LocalPath.Substring(idx + 1);
        idx = path.IndexOf("/");
        this.joinServer = idx < 0 ? path : path.Substring(0, idx);
        this.joinPass = idx < 0 ? null : path.Substring(idx + 1);
        HttpOk(stream, 
          "<html><body>\n" +
          "<p>extraQL will connect you in a second...</p>\n" +
          "<p>(this window closes in 5 secs)</p>\n" +
          "<script type='text/javascript'>\n" +
          "window.setTimeout(function(){ window.close(); }, 5000);\n"+
          "</script></body></html>");
      }
      else // poll server/pass
      {
        var json = "{ \"server\":\"" + this.joinServer + "\", \"pass\":\"" + this.joinPass + "\" }";
        this.joinServer = null;
        this.joinPass = null;
        HttpOk(stream, json);
      }
    }

    #endregion

    #region ListDemos()

    private void ListDemos(Stream stream, Uri uri, string request)
    {
      if (!this.EnablePrivateServlets)
      {
        HttpForbidden(stream);
        return;
      }

      var json = new StringBuilder();
      json.Append("[");
      foreach (var file in Directory.GetFiles(this.QuakeConfigFolder + "\\demos", "*.dm_*"))
      {
        if (json.Length > 1)
          json.Append(",");
        json.Append("{\"file\":\"").Append(Path.GetFileName(file)).Append("\"");
        json.Append(",\"date\":\"").Append(File.GetCreationTime(file).ToString("s")).Append("\"");
        json.Append("}");
      }
      json.Append("]");
      HttpOk(stream, json.ToString());
    }

    #endregion

    #region SetSteamNick()

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate bool Init();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr SteamFriends();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void SetPersonaName(IntPtr handle, byte[] utf8name);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate bool Shutdown();

    /// <summary>
    /// </summary>
    private void SetSteamNick(Stream stream, Uri uri, string request)
    {
      if (!this.EnablePrivateServlets)
      {
        HttpForbidden(stream);
        return;
      }

      NameValueCollection args = HttpUtility.ParseQueryString(uri.Query);
      string name = args.Get("name");
      string dllDir = Application.StartupPath + "\\";

      bool ok = false;
      if (!string.IsNullOrEmpty(name))
      {
        // any app-id that is freely available will do (282440=Quake Live)
        if (!File.Exists(dllDir + "steam_appid.txt"))
          File.WriteAllText(dllDir + "steam_appid.txt", "282440");

        var hModule = Win32.LoadLibrary(dllDir + "steam_api.dll");
        if (hModule != IntPtr.Zero)
        {

          IntPtr pInit = Win32.GetProcAddress(hModule, "SteamAPI_Init");
          Init init = (Init)Marshal.GetDelegateForFunctionPointer(pInit, typeof(Init));

          if (init())
          {
            IntPtr pSteamFriends = Win32.GetProcAddress(hModule, "SteamFriends");
            SteamFriends steamFriends = (SteamFriends)Marshal.GetDelegateForFunctionPointer(pSteamFriends, typeof(SteamFriends));

            IntPtr pSetPersonaName = Win32.GetProcAddress(hModule, "SteamAPI_ISteamFriends_SetPersonaName");
            SetPersonaName setPersonaName = (SetPersonaName)Marshal.GetDelegateForFunctionPointer(pSetPersonaName, typeof(SetPersonaName));

            IntPtr pShutdown = Win32.GetProcAddress(hModule, "SteamAPI_Shutdown");
            Shutdown shutdown = (Shutdown)Marshal.GetDelegateForFunctionPointer(pShutdown, typeof(Shutdown));

            var handle = steamFriends();
            var cName = Encoding.UTF8.GetBytes(name + "\0");
            setPersonaName(handle, cName);
            ok = true;

            shutdown();
          }
          Win32.FreeLibrary(hModule);
        }
      }

      if (ok)
        HttpOk(stream);
      else
        HttpUnavailable(stream, "steam_api.dll could not be initialized. Make sure your Steam client is running.");
    }

    #endregion

    // internal methods

    #region EnableScripts
    public bool EnableScripts { get; set; }
    #endregion

    #region QLWindowHandle

    /// <summary>
    ///   Win32 window handle of the QL window
    /// </summary>
    public static IntPtr QLWindowHandle
    {
      get
      {
        foreach (Process proc in Process.GetProcessesByName("quakelive")) // standalone
          return proc.MainWindowHandle;
        foreach (Process proc in Process.GetProcessesByName("quakelive_steam")) // steam
          return proc.MainWindowHandle;
        return IntPtr.Zero;
      }
    }

    #endregion

    #region DownloadText()

    private string DownloadText(string url)
    {
      try
      {
        using (var webRequest = new XWebClient())
        {
          webRequest.Encoding = Encoding.UTF8;
          return webRequest.DownloadString(url);
        }
      }
      catch (Exception ex)
      {
        Log(ex.Message);
        return null;
      }
    }

    #endregion

    #region DeliverFileOrDirectoryListing()

    /// <summary>
    ///   Delivers a directory listing or the contents of a local file (relative to the server root and an additional basePath)
    /// </summary>
    private void DeliverFileOrDirectoryListing(Stream stream, Uri uri, string basePath, string pattern, string contentType)
    {
      string absPath = this.GetFilePath(uri, basePath);
      var writer = new StreamWriter(stream);
      byte[] data;
      if (absPath != null && Directory.Exists(absPath))
        data = this.GetDirectoryListing(uri, absPath, pattern);
      else if (absPath != null && File.Exists(absPath))
        data = File.ReadAllBytes(absPath);
      else
      {
        writer.WriteLine("HTTP/1.1 400 Not Found\r\n\r\n" + absPath + " not found");
        writer.Flush();
        return;
      }

      writer.WriteLine("HTTP/1.1 200 OK");
      writer.WriteLine("Access-Control-Allow-Origin: *"); // allow QL scripts to request URLs from this server
      writer.WriteLine("Content-Length: " + data.Length);
      if (contentType != null)
        writer.WriteLine("Content-Type: " + contentType);
      writer.WriteLine();
      writer.Flush();
      stream.Write(data, 0, data.Length);
    }

    #endregion

    #region GetDirectoryListing()

    /// <summary>
    ///   Returns a listing of the directory in either HTML or JSON format, if the URL parameter "json" is present
    /// </summary>
    private byte[] GetDirectoryListing(Uri uri, string absPath, string pattern = null)
    {
      NameValueCollection args = HttpUtility.ParseQueryString(uri.Query);
      bool json = args[null] == "json";
      var sb = new StringBuilder();
      sb.Append(json ? "[" : "<html><body>");
      int i = 0;
      string[] entries = Directory.GetFiles(absPath);
      Array.Sort(entries, (a, b) => String.Compare((Path.GetFileName(a) ?? ""), Path.GetFileName(b) ?? "", StringComparison.Ordinal));
      var regex = new Regex(pattern ?? ".*");
      foreach (string file in entries)
      {
        string filename = Path.GetFileName(file) ?? "";
        if (!regex.IsMatch(filename))
          continue;
        if (++i > 1)
          sb.Append(json ? "," : "<br>");
        sb.AppendFormat(json ? "\"{1}\"" : "<a href='{0}/{1}'>{1}</a>", uri.AbsolutePath, filename);
      }
      sb.Append(json ? "]" : "</body></html>");
      return Encoding.UTF8.GetBytes(sb.ToString());
    }

    #endregion

    #region GetFilePath()

    /// <summary>
    ///   Ensures the path specified in the URL doesn't contain ":", ".." or "\\" or any other invalid characters.
    ///   Returns an absolute file path or null, if invalid
    /// </summary>
    private string GetFilePath(Uri uri, string basePath)
    {
      int pos = uri.AbsolutePath.IndexOf("/", 1, StringComparison.Ordinal);
      string relPath = pos < 0 ? "" : uri.AbsolutePath.Substring(pos + 1);
      if (relPath.Contains(":") || relPath.Contains("..") || relPath.Replace('/', '\\').Contains("\\\\") || relPath.IndexOfAny(Path.GetInvalidPathChars()) > 0)
        return null;

      return this.baseDir + "\\" + basePath + "\\" + relPath;
    }

    #endregion


    #region HttpOk()

    /// <summary>
    ///   Write standard HTTP response
    /// </summary>
    private void HttpOk(Stream stream, string content = null, string headers = null)
    {
      var writer = new StreamWriter(stream);
      writer.WriteLine("HTTP/1.1 200 OK");
      writer.WriteLine("Access-Control-Allow-Origin: *"); // allow QL scripts to request URLs from this server
      if (!string.IsNullOrEmpty(headers))
        writer.Write(headers);
      if (content == null)
      {
        writer.WriteLine();
        writer.Flush();
      }
      else
      {
        byte[] data = Encoding.UTF8.GetBytes(content);
        writer.WriteLine("Content-Type: text; charset=utf-8");
        writer.WriteLine("Content-Length: " + data.Length);
        writer.WriteLine();
        writer.Flush();
        stream.Write(data, 0, data.Length);
      }
    }

    #endregion

    #region HttpForbidden()
    private void HttpForbidden(Stream stream, string msg = null)
    {
      var writer = new StreamWriter(stream);
      writer.WriteLine("HTTP/1.1 403 Forbidden");
      writer.WriteLine("Access-Control-Allow-Origin: *");
      writer.WriteLine();

      writer.WriteLine(msg ?? "This function is not available from a remote extraQL.exe server");
      writer.Flush();
    }
    #endregion

    #region HttpUnavailable()
    private void HttpUnavailable(Stream stream, string msg = null)
    {
      var writer = new StreamWriter(stream);
      writer.WriteLine("HTTP/1.1 503 Unavailable");
      writer.WriteLine("Access-Control-Allow-Origin: *");
      writer.WriteLine();

      writer.WriteLine(msg ?? "The requested operation is not supported");
      writer.Flush();
    }
    #endregion

    #region HttpNotImplemented()
    private static void HttpNotImplemented(Stream stream)
    {
      var writer = new StreamWriter(stream);
      writer.WriteLine("HTTP/1.1 501 Not Implemented");
      writer.WriteLine("Access-Control-Allow-Origin: *");
      writer.WriteLine();
      writer.WriteLine("This function is not implemented yet");
      writer.Flush();
    }
    #endregion
  }
}