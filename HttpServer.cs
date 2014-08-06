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

    public IDictionary<string, Servlet> servlets = new Dictionary<string, Servlet>();

    public HttpServer()
    {
      this.BindToAllInterfaces = false;
    }

    #region BindToAllAddresses
    public bool BindToAllInterfaces
    {
      set
      {
        if (this.IsRunning)
          throw new InvalidOperationException("Cannot change BindToAllIpAddresses while server is running");
        this.servicePort = new IPEndPoint(value ? IPAddress.Any : IPAddress.Loopback, PortNumber);
      }
    }
    #endregion

    #region RegisterServlet()
    public void RegisterServlet(string relativUrl, Servlet servlet)
    {
      servlets[relativUrl] = servlet;
    }
    #endregion

    #region HandleClientConnection()

    protected override void HandleClientConnection(TcpClient client)
    {
      try
      {
        client.ReceiveTimeout = 1000;
        string data = ReadAllBytes(client);
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

    private static string ReadAllBytes(TcpClient client)
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
      } while (len == 8192 || (contentLength >= 0 && dataOffset >= 0 && dataBytesRead < contentLength));
      return sb.ToString();
    }

    #endregion

    public string Endpoint { get { return this.servicePort.ToString(); } }
  }
}