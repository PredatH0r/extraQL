using System.Windows.Forms;

namespace ExtraQL
{
  public partial class UpdateForm : Form
  {
    public UpdateForm()
    {
      InitializeComponent();
    }

    public string Message
    {
      get { return this.lblText.Text; }
      set 
      { 
        this.lblText.Text = value;
        Application.DoEvents();
      }
    }
  }
}
