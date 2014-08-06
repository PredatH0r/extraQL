using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ExtraQL
{
  public abstract class TcpServer : IDisposable
  {
    protected IPEndPoint servicePort;
    protected readonly Encoding enc = Encoding.ASCII;

    private volatile bool started;
    private volatile bool shutdownInProgress;
    private volatile int clientCount;
    readonly ManualResetEvent startupComplete = new ManualResetEvent(false);
    readonly ManualResetEvent shutdownComplete = new ManualResetEvent(false);
    private Action<string> log = DefaultLog;

    #region Log
    public Action<string> Log
    {
      get { return log; }
      set { log = log == null ? DefaultLog : value; }
    }

    private static void DefaultLog(string msg)
    {
      if (System.Diagnostics.Debugger.IsAttached)
        System.Diagnostics.Debugger.Log(1, "TcpServer", msg);
      else
        Console.Error.WriteLine(msg);
    }
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
      using (Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
      {
        IPEndPoint localServiceEndPoint = this.servicePort.Address.ToString()  == "0.0.0.0" ? 
          new IPEndPoint(IPAddress.Loopback, this.servicePort.Port) : 
          this.servicePort;
        client.Connect(localServiceEndPoint);
      }
      this.shutdownComplete.WaitOne();
      this.shutdownInProgress = false;
      this.started = false;
    }
    #endregion

    #region Started
    public bool IsRunning { get { return this.started; } }
    #endregion

    #region AcceptLoop()
    private void AcceptLoop()
    {
      TcpListener server;
      try
      {
        server = new TcpListener(this.servicePort);
        server.Start(100);
        this.started = true;
      }
      catch (Exception ex)
      {
        this.startupComplete.Set();
        this.shutdownComplete.Set();
        Log("Unable to open TCP port " + this.servicePort.Address + ":" + this.servicePort.Port + "\n"
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
        this.HandleClientConnection(client);
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

    protected abstract void HandleClientConnection(TcpClient client);

    #region Dispose()

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize(this);
    }

    ~TcpServer()
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
