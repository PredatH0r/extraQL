using System;
using System.Windows.Forms;

namespace ExtraQL
{
  static class Program
  {
    [STAThread]
    static void Main()
    {
      AppDomain.CurrentDomain.UnhandledException += (sender, args) => HandleException(args.ExceptionObject as Exception);
      Application.ThreadException += (sender, args) => HandleException(args.Exception);

      try
      {
        Application.EnableVisualStyles();
        Application.Run(new MainForm());
      }
      catch (Exception ex)
      {
        HandleException(ex);
      }
    }

    private static void HandleException(Exception ex)
    {
      MessageBox.Show(ex.ToString(), "Program failure", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
  }
}
