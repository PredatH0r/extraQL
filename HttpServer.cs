using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ExtraQL
{
  internal class HttpServer : TcpServer
  {
    public delegate void Servlet(TcpClient client, Uri uri, string request);

    private const int PortNumber = 27963;
    private const int MaxRequestSize = 16384; // 16 KB
    private bool bindToAllInterfaces;
    private IPEndPoint endPoint;

    public IDictionary<string, Servlet> servlets = new Dictionary<string, Servlet>();

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

    #region RegisterServlet()
    public void RegisterServlet(string relativUrl, Servlet servlet)
    {
      servlets[relativUrl] = servlet;
    }
    #endregion

    #region EndPoint
    public override IPEndPoint EndPoint
    {
      get
      {
        if (this.endPoint == null)
          this.endPoint = new IPEndPoint(this.bindToAllInterfaces ? IPAddress.Any : IPAddress.Loopback, PortNumber);
        return this.endPoint;
      }
    }
    #endregion

    #region HandleClientConnection()

    protected override void HandleClientConnection(TcpClient client)
    {
      try
      {
        client.ReceiveTimeout = 1000;
        string data = ReadAllBytes(client);
        if (data == null)
          return;
        if (!data.StartsWith("POST ") && !data.StartsWith("GET "))
          return;
        int idx = data.IndexOf(" HTTP", StringComparison.Ordinal);
        if (idx < 0)
          return;

        var url = new Uri(new Uri("http://localhost:" + PortNumber + "/"), data.Substring(4, idx - 4));
        string urlPath = url.AbsolutePath;
        foreach (var entry in servlets)
        {
          if (entry.Key == urlPath || urlPath.StartsWith(entry.Key + "/"))
          {
            Servlet servlet = entry.Value;
            servlet(client, url, data);
            return;
          }
        }
        client.Client.Send(enc.GetBytes("HTTP/0.9 404 Not Found\r\n\r\n"));
        Log("URL not found: " + urlPath);
      }
      catch (Exception ex)
      {
        Log(ex.Message);
      }
      finally
      {
        try { client.Close(); }
        catch
        {
        }
      }
    }

    #endregion

    #region ReadAllBytes()

    private string ReadAllBytes(TcpClient client)
    {
      const string ContentLength = "Content-Length:";
      var sb = new StringBuilder();

      Decoder dec = new UTF8Encoding().GetDecoder();
      int len;
      int contentLength = -1;
      int dataOffset = -1;
      int dataBytesRead = 0;
      do
      {
        var bufferSeg = new byte[8192];
        var chars = new char[8192];
        len = client.Client.Receive(bufferSeg);
        int clen = dec.GetChars(bufferSeg, 0, len, chars, 0, false);
        sb.Append(chars, 0, clen);

        if (dataOffset < 0) // try to parse HTTP header
        {
          string str = sb.ToString();
          int idx = str.IndexOf("\r\n\r\n", StringComparison.Ordinal);
          if (idx >= 0)
          {
            dataOffset = idx + 4;
            dataBytesRead = len - dataOffset%bufferSeg.Length;

            idx = str.IndexOf(ContentLength, StringComparison.Ordinal);
            if (idx > 0)
            {
              int idx2 = str.IndexOf("\r\n", idx, StringComparison.Ordinal);
              if (idx2 > 0)
                int.TryParse(str.Substring(idx + ContentLength.Length), out contentLength);
            }
          }
        }
        else
          dataBytesRead += len;

        // enforce maximum allowed request size to prevent denial-of-service
        if (dataBytesRead >= MaxRequestSize)
        {
          client.Client.Send(Encoding.ASCII.GetBytes("HTTP/0.9 413 Request Entity Too Large\r\n\r\n"));
          Log("Request exceeded maximum allowed size: " + client.Client.RemoteEndPoint);
          return null;
        }
      } while (len == 8192 || (contentLength >= 0 && dataOffset >= 0 && dataBytesRead < contentLength));
      return sb.ToString();
    }

    #endregion
  }
}