using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace ExtraQL
{
  internal class HttpServer : IDisposable
  {
    public delegate void Servlet(Stream stream, Uri uri, string request);

    private const int PortNumber = 27963;
    private const int MaxRequestSize = 16384; // 16 KB
    private bool bindToAllInterfaces;
    private bool useHttps;
    private IPEndPoint endPoint;
    public IDictionary<string, Servlet> servlets = new Dictionary<string, Servlet>();
    private readonly Encoding enc = Encoding.ASCII;
    private volatile bool started;
    private volatile bool shutdownInProgress;
    private volatile int clientCount;
    private readonly ManualResetEvent startupComplete = new ManualResetEvent(false);
    private readonly ManualResetEvent shutdownComplete = new ManualResetEvent(false);
    private Action<string> log = msg => { };
    private readonly string certPath;
    private X509Certificate cert;
    private readonly PropertyInfo hresultGetter;

    public HttpServer(string certFile)
    {
      this.certPath = certFile;
      hresultGetter = typeof (Exception).GetProperty("HResult", BindingFlags.Instance | BindingFlags.NonPublic);
    }

    #region BindToAllAddresses
    public bool BindToAllInterfaces
    {
      set
      {
        if (value == this.bindToAllInterfaces)
          return;
        if (this.IsRunning)
          throw new InvalidOperationException("Cannot change BindToAllInterfaces while server is running");
        this.bindToAllInterfaces = value;
        this.endPoint = null;
      }
    }
    #endregion

    #region UseHttps
    public bool UseHttps
    {
      set
      {
        if (value == this.useHttps)
          return;
        if (this.IsRunning)
          throw new InvalidOperationException("Cannot change UseHttps while server is running");
        this.useHttps = value;
        if (this.useHttps && cert == null)
          this.cert = new X509Certificate2(this.certPath, "extraQL", X509KeyStorageFlags.MachineKeySet); // windows XP fails if the .pxf file has no password
      }
    }
    #endregion

    #region LogAllRequests
    public bool LogAllRequests { get; set; }
    #endregion

    #region RegisterServlet()
    public void RegisterServlet(string relativUrl, Servlet servlet)
    {
      servlets[relativUrl] = servlet;
    }
    #endregion

    #region EndPoint
    private IPEndPoint EndPoint
    {
      get
      {
        if (this.endPoint == null)
          this.endPoint = new IPEndPoint(this.bindToAllInterfaces ? IPAddress.Any : IPAddress.Loopback, PortNumber);
        return this.endPoint;
      }
    }
    #endregion

    #region EndPointUrl
    public string EndPointUrl
    {
      get { return (this.useHttps ? "https://" : "http://") + this.EndPoint + "/"; }
    }
    #endregion

    #region Log
    public Action<string> Log
    {
      get { return log; }
      set { log = log == null ? msg => { } : value; }
    }
    #endregion

    #region IsRunning
    public bool IsRunning { get { return this.started; } }
    #endregion

    #region Start()
    public bool Start()
    {
      if (this.shutdownInProgress)
        return false;
      if (this.started)
        return true;
      this.startupComplete.Reset();
      Thread acceptLoop = new Thread(AcceptLoop);
      acceptLoop.IsBackground = true;
      acceptLoop.Name = "AcceptLoop";
      acceptLoop.Start();
      this.startupComplete.WaitOne();
      return this.started;
    }
    #endregion

    #region Stop()

    public void Stop()
    {
      if (!this.started || this.shutdownInProgress)
        return;
      this.shutdownInProgress = true;
      this.shutdownComplete.Reset();
      // connect to the service port so that the AcceptLoop thread unblocks and can terminate
      try
      {
        using (Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
        {
          IPEndPoint localServiceEndPoint = this.EndPoint.Address.ToString() == "0.0.0.0"
            ? new IPEndPoint(IPAddress.Loopback, this.EndPoint.Port)
            : this.EndPoint;
          client.Connect(localServiceEndPoint);
          client.Send(new byte[] {0});
        }
      }
      catch (SocketException)
      {
        // happens when Stop() is called through the finalizer
      }
      this.shutdownComplete.WaitOne();
      this.shutdownInProgress = false;
      this.started = false;
    }
    #endregion

    #region AcceptLoop()
    private void AcceptLoop()
    {
      TcpListener server;
      try
      {
        server = new TcpListener(this.EndPoint);
        server.Start(100);
        this.started = true;
      }
      catch (Exception ex)
      {
        this.startupComplete.Set();
        this.shutdownComplete.Set();
        Log("Unable to open TCP port " + this.EndPoint.Address + ":" + this.EndPoint.Port + "\n"
            + "Is there another extraQL running?\n\nError: " + ex.Message);
        return;
      }

      this.startupComplete.Set();
      while (true)
      {
        try
        {
          TcpClient client = server.AcceptTcpClient();
          if (this.shutdownInProgress)
            break;

          ++clientCount;
          Thread handler = new Thread(() => this.HandleClientConnectionSafely(client));
          handler.Name = "#" + clientCount;
          handler.Start();
        }
        catch(Exception ex)
        {
          Log(ex.Message);
        }
      }
      server.Stop();
      this.shutdownComplete.Set();
    }
    #endregion

    #region HandleClientConnectionSafely()
    private void HandleClientConnectionSafely(TcpClient client)
    {
      try
      {
        //Log("incoming connection from " + client.Client.RemoteEndPoint);
        Stream stream;
        if (this.useHttps)
        {
          SslStream ssl = new SslStream(client.GetStream(), false);
          ssl.AuthenticateAsServer(this.cert, false, SslProtocols.Default, false);
          stream = ssl;
        }
        else
          stream = client.GetStream();

        using (stream)
        {
          stream.ReadTimeout = 1000;
          stream.WriteTimeout = 1000;
          this.HandleClientConnection(stream);
        }
      }
      catch (IOException ex)
      {
        uint hresult = 0;
        if (hresultGetter != null)
          hresult = (uint)(int) hresultGetter.GetValue(ex, null);
        if (hresult != 2148734496) // don't log "client stream closed during authentication" errors
          Log(ex.Message);
      }
      catch (Exception ex)
      {
        Log(ex.Message);
      }
      finally
      {
        try { client.Close(); }
        catch (Exception ex)
        {
          Log(ex.Message);
        }
      }
    }
    #endregion

    #region HandleClientConnection()

    private void HandleClientConnection(Stream stream)
    {
      Uri url = null;
      try
      {
        bool keepAlive;
        do
        {
          Dictionary<string, string> header;
          string data = ReadAllBytes(stream, out header);
          if (string.IsNullOrEmpty(data))
            return;

          string conn;
          keepAlive = header.TryGetValue("Connection", out conn) && conn == "keep-alive";

          // Check that the user agent is Awesomium and not a regular browser
          // This should prevent abuse of extraQL URLs embedded in regular web pages
          //string userAgent;
          //if (!header.TryGetValue("User-Agent", out userAgent) || !userAgent.Contains("Awesomium"))
          //{
          //  var b = enc.GetBytes("HTTP/1.1 401 Unauthorized\r\n\r\nextraQL URLs may only be called from within QuakeLive scripts");
          //  stream.Write(b, 0, b.Length);
          //  continue;
          //}

          if (!data.StartsWith("POST ") && !data.StartsWith("GET "))
          {
            var b = enc.GetBytes("HTTP/1.1 405 Method Not Allowed\r\n\r\n");
            stream.Write(b, 0, b.Length);
            continue;
          }

          int idx = data.IndexOf(" HTTP", StringComparison.Ordinal);
          if (idx < 0)
            return;

          url = new Uri(new Uri(this.EndPointUrl), data.Substring(4, idx - 4));

          if (this.LogAllRequests)
            Log(url.ToString()); // ToString() displays the query string url-decoded, OriginalString doesn't

          string urlPath = url.AbsolutePath;
          if (!ExecuteServlet(stream, urlPath, url, data))
          {
            var buff = enc.GetBytes("HTTP/1.1 404 Not Found\r\n\r\n");
            stream.Write(buff, 0, buff.Length);
            Log("URL not found: " + urlPath);
          }

          stream.Flush();
        } while (keepAlive);
      }
      catch (Exception ex)
      {
        Log(url + ": " + ex.Message);
      }
      finally
      {
        try { stream.Close(); }
        catch { }
      }
    }
    #endregion

    #region ExecuteServlet()
    private bool ExecuteServlet(Stream stream, string urlPath, Uri url, string data)
    {
      foreach (var entry in servlets)
      {
        if (entry.Key == urlPath || urlPath.StartsWith(entry.Key + "/"))
        {
          Servlet servlet = entry.Value;
          servlet(stream, url, data);
          return true;
        }
      }
      return false;
    }

    #endregion

    #region ReadAllBytes()

    private string ReadAllBytes(Stream client, out Dictionary<string,string> header)
    {
      var sb = new StringBuilder();

      Decoder dec = new UTF8Encoding().GetDecoder();
      int contentLength = -1;
      int dataOffset = -1;
      int dataBytesRead = 0;
      header = new Dictionary<string, string>();
      try
      {
        int len;
        do
        {
          var bufferSeg = new byte[8192];
          var chars = new char[8192];
          len = client.Read(bufferSeg, 0, bufferSeg.Length);
          if (len == 0)
            return null;
          int clen = dec.GetChars(bufferSeg, 0, len, chars, 0, false);
          sb.Append(chars, 0, clen);

          if (dataOffset < 0) // try to parse HTTP header
          {
            header = TryExtractingHttpHeader(sb, bufferSeg, len, ref dataBytesRead, ref dataOffset);
            const string ContentLength = "Content-Length:";
            if (header.ContainsKey(ContentLength))
              int.TryParse(header[ContentLength], out contentLength);
          }
          else
            dataBytesRead += len;

          // enforce maximum allowed request size to prevent denial-of-service
          if (dataBytesRead >= MaxRequestSize)
          {
            var buff = Encoding.ASCII.GetBytes("HTTP/1.1 413 Request Entity Too Large\r\n\r\n");
            client.Write(buff, 0, buff.Length);
            Log("Request exceeded maximum allowed size");
            return null;
          }
        } while (dataOffset < 0 // header not finished
          || (contentLength >= 0 && dataBytesRead < contentLength) // data not finished
          || (dataOffset >= 0 && contentLength < 0 && len == 0)); // stalled out stream of unknown length
      }
      catch (IOException)
      {
        // clients may just close the stream when they're done sending requests on a keep-alive connection
      }

      return sb.ToString();
    }

    private Dictionary<string, string> TryExtractingHttpHeader(StringBuilder sb, byte[] buffer, int length, ref int dataBytesRead, ref int dataOffset)
    {

      var header = new Dictionary<string, string>();
      string str = sb.ToString();
      int idx = str.IndexOf("\r\n\r\n", StringComparison.Ordinal);
      if (idx >= 0)
      {
        int idx0 = str.IndexOf("\r\n") + 2;
        var headerLines = str.Substring(idx0, idx - idx0).Split('\n');
        foreach (var line in headerLines)
        {
          var keyVal = line.Split(new[] {':'}, 2);
          header[keyVal[0].Trim()] = keyVal[1].Trim();
        }

        dataOffset = idx + 4;
        dataBytesRead = length - dataOffset%buffer.Length;
      }
      return header;
    }

    #endregion

    #region IDisposable
    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize(this);
    }

    ~HttpServer()
    {
      this.Dispose(false);
    }

    protected virtual void Dispose(bool disposing)
    {
      this.Stop();
    }
    #endregion
  }
}