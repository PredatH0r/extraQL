using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Windows.Forms;

namespace ExtraQL
{
  internal class Servlets
  {
    private const string AddScriptRoute = "/addScript";
    private readonly HttpServer server;
    private readonly StringBuilder indexBuilder = new StringBuilder();
    private string _fileBaseDir;
    private readonly ScriptRepository scriptRepository;

    public Action<string> Log;
    
    #region ctor()

    public Servlets(HttpServer server, ScriptRepository scriptRepository, Action<string> logger)
    {
      this.server = server;
      this.scriptRepository = scriptRepository;
      this.EnableScripts = true;
      server.Log = logger;
      this.Log = server.Log;

      RegisterServlets();
    }

    #endregion

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
      RegisterServlet("/repository.json", RepositoryJson);
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
    private void Index(TcpClient client, Uri uri, string request)
    {
      HttpOk(client, "<html><body><h1>extraQL script server</h1>" + indexBuilder + "</body></html>");
    }

    #endregion

    #region Version()

    /// <summary>
    ///   Returns the extraQL version number.
    ///   Used by the extraQL user scripts to test wheter the local server is running.
    /// </summary>
    private void Version(TcpClient client, Uri uri, string request)
    {
      HttpOk(client, "{ \"version\": \"" + MainForm.Version + "\", \"enabled\": " + this.EnableScripts.ToString().ToLower() + " }");
    }

    #endregion

    #region GetLocalScript()

    /// <summary>
    ///   returns a script or list of script names from the "/scripts" folder
    /// </summary>
    /// <param name="client"></param>
    /// <param name="uri"></param>
    /// <param name="request"></param>
    private void GetLocalScript(TcpClient client, Uri uri, string request)
    {
      DeliverFileOrDirectoryListing(client, uri, "scripts", @".*\.usr\.js$");
    }

    #endregion

    #region GetImage()

    /// <summary>
    ///   Returns a binary file from the "/images" folder
    /// </summary>
    private void GetImage(TcpClient client, Uri uri, string request)
    {
      DeliverFileOrDirectoryListing(client, uri, "images");
    }

    #endregion

    #region DataStorage()

    /// <summary>
    ///   Retrieve (GET) or write (POST) a file from/to the "/data" folder
    /// </summary>
    private void DataStorage(TcpClient client, Uri uri, string request)
    {
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
      HttpOk(client, response);
    }

    #endregion

    #region Proxy()

    /// <summary>
    ///   Relays the URL specified in the "url" parameter and returns the result.
    ///   This allows bypassing the "same-origin-policy" of browsers and QL and request
    ///   HTML data from external web sites like esreality.com
    /// </summary>
    private void Proxy(TcpClient client, Uri uri, string request)
    {
      NameValueCollection args = HttpUtility.ParseQueryString(uri.Query);
      var url = args.Get("url");
      if (string.IsNullOrEmpty(url))
        HttpOk(client, "missing 'url' parameter");
      else
      {
        string text = DownloadText(url);
        HttpOk(client, text);
      }
    }

    #endregion

    #region ToggleFullscreen()

    /// <summary>
    ///   Send Alt+Enter key stroke to QL window
    /// </summary>
    private void ToggleFullscreen(TcpClient client, Uri uri, string request)
    {
      Win32.PostMessage(QLWindowHandle, Win32.WM_SYSKEYDOWN, (int) Keys.Enter, 0);
      HttpOk(client, "Ok");
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
    private void DockWindow(TcpClient client, Uri uri, string request)
    {
      NameValueCollection args = HttpUtility.ParseQueryString(uri.Query);
      int sides, w, h;
      if (!int.TryParse(args.Get("sides"), out sides))
      {
        HttpOk(client, "'sides' parameter missing");
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

      HttpOk(client, "ok");
    }

    #endregion

    #region ScriptLog()

    /// <summary>
    ///   Write a log message to the extraQL window.
    ///   Allows logging of large text messages independent of web- or in-game-console
    ///   and copy/paste the logged message
    /// </summary>
    private void ScriptLog(TcpClient client, Uri uri, string request)
    {
      int idx = request.IndexOf("\r\n\r\n", StringComparison.Ordinal);
      string msg = idx < 0 ? "" : request.Substring(idx + 4);
      if (!request.StartsWith("POST"))
        HttpOk(client, "POST data with log message missing");
      else
      {
        Log("Script log:\r\n" + msg);
        HttpOk(client, "");
      }
    }

    #endregion

    #region AddScript()
    /// <summary>
    /// Download a script from an external URL and return a JSON with the meta information
    /// </summary>
    private void AddScript(TcpClient client, Uri uri, string request)
    {
#if false
      var args = HttpUtility.ParseQueryString(uri.Query);
      string text = "";
      string scriptId = uri.AbsolutePath.StartsWith(AddScriptRoute+"/") ? uri.AbsolutePath.Substring(AddScriptRoute.Length+1) : null;
      
      var scriptInfo = string.IsNullOrEmpty(scriptId) ? null : this.scriptRepository.GetScriptByIdOrUrl(scriptId);
      if (scriptInfo == null)
      {
        var writer = new StreamWriter(client.GetStream());
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

      this.HttpOk(client, text);
#else
      var writer = new StreamWriter(client.GetStream());
      writer.WriteLine("HTTP/1.1 501 Not Implemented");
      writer.WriteLine("Access-Control-Allow-Origin: *");
      writer.WriteLine();
      writer.WriteLine("This function is not implemented yet");
      writer.Flush();
#endif
    }
    #endregion

    #region RepositoryJson()
    /// <summary>
    ///   Locally serve QLHM's JSONP calls, which are normally handled via http://qlhm.phob.net/uso/:id
    ///   The returned object contains the script ID in ._meta.id, the metadata in .headers[fieldname][]
    ///   and the actual script code in .content
    /// </summary>
    private void RepositoryJson(TcpClient client, Uri uri, string request)
    {
      string text = "[";
      string sep = "";
      foreach (var info in this.scriptRepository.GetScriptIds())
      {
        text += sep + "\n{\"id\":\"" + info.Id + "\"";
        text += ",\"filename\":\"" + Path.GetFileName(info.Filepath) + "\"";
        foreach (var entry in info.Metadata)
        {
          if (entry.Key == "id")
            continue;
          text += ",\"" + entry.Key + "\":\"" + entry.Value[0].Replace("\\","\\\\").Replace("\"", "\\\"") + "\"";
        }
        text += "}";
        sep = ",";
      }
      text += "\n]";
      this.HttpOk(client, text);
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
        foreach (Process proc in Process.GetProcessesByName("quakelive"))
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
        using (var webRequest = new WebClient())
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
    private void DeliverFileOrDirectoryListing(TcpClient client, Uri uri, string basePath, string pattern = null)
    {
      string absPath = GetFilePath(uri, basePath);
      var writer = new StreamWriter(client.GetStream());
      byte[] data;
      if (absPath != null && Directory.Exists(absPath))
        data = GetDirectoryListing(uri, absPath, pattern);
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
      writer.WriteLine();
      writer.Flush();
      client.GetStream().Write(data, 0, data.Length);
    }

    #endregion

    #region GetDirectoryListing()

    /// <summary>
    ///   Returns a listing of the directory in either HTML or JSON format, if the URL parameter "json" is present
    /// </summary>
    private static byte[] GetDirectoryListing(Uri uri, string absPath, string pattern = null)
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

      if (_fileBaseDir == null)
      {
        _fileBaseDir = Path.GetDirectoryName(Application.ExecutablePath) ?? "";
        if (_fileBaseDir.EndsWith("\\bin\\Debug")) // when started from VisualStudio
          _fileBaseDir = Path.GetDirectoryName(Path.GetDirectoryName(_fileBaseDir));
      }

      return _fileBaseDir + "\\" + basePath + "\\" + relPath;
    }

    #endregion

    #region HttpOk()

    /// <summary>
    ///   Write standard HTTP response
    /// </summary>
    private void HttpOk(TcpClient client, string content = null, string headers = null)
    {
      var writer = new StreamWriter(client.GetStream());
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
        client.GetStream().Write(data, 0, data.Length);
      }
    }

    #endregion
  }
}