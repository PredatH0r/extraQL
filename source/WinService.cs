using System.ServiceProcess;
using System.Threading;
using System.Windows.Forms;

namespace ExtraQL
{
  class WinService : ServiceBase
  {
    private static Form mainForm;

    internal static void Start(Form form)
    {
      mainForm = form;
      var servicesToRun = new ServiceBase[] { new WinService() };
      ServiceBase.Run(servicesToRun);      
    }

    private WinService()
    {
      this.ServiceName = "extraQL";
    }

    protected override void OnStart(string[] args)
    {
      var uiThread = new Thread(() => Application.Run(mainForm));
      uiThread.Start();
    }

    protected override void OnStop()
    {
      mainForm.BeginInvoke((ThreadStart)(() => mainForm.Close()));
    }
  }
}
