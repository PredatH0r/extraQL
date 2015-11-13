using System;
using System.Windows.Forms;

namespace ExtraQL
{
  static class Program
  {
    public const string WinServiceSwitch = "-service";
    public const string BackgroundSwitch = "-background";

    #region Main()
    [STAThread]
    static void Main()
    {
      AppDomain.CurrentDomain.UnhandledException += (sender, args) => HandleException(args.ExceptionObject as Exception);
      Application.ThreadException += (sender, args) => HandleException(args.Exception);

      try
      {
        Config config = new Config();
        config.LoadSettings();
        if (ActivateRunningInstance()) 
          return;

        Application.EnableVisualStyles();

        var mainForm = new MainForm(config);
        if (Environment.CommandLine.Contains(WinServiceSwitch))
          WinService.Start(mainForm);
        else
          Application.Run(mainForm);
      }
      catch (Exception ex)
      {
        HandleException(ex);
      }
    }
    #endregion

    #region ActivateRunningInstance()
    private static bool ActivateRunningInstance()
    {
      using (var client = new XWebClient(500))
      {
        try
        {
          var servlet = Environment.CommandLine.Contains(BackgroundSwitch) ? "version" : "bringToFront";
          var result = client.DownloadString("http://127.0.0.1:27963/" + servlet);
          if (result != null)
            return true;
        }
        catch
        {
        }
      }
      return false;
    }
    #endregion

    #region HandleException()
    private static void HandleException(Exception ex)
    {
      MessageBox.Show(ex.ToString(), "Program failure", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
    #endregion
  }
}
