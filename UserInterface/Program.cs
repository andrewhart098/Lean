using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using QuantConnect.Views.WinForms;

namespace QuantConnect.Views
{
    public class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                throw new Exception(
                    "Error: You must specify the port on which the application will open a TCP socket.");
            }

            var port = args[0];

            var form = new LeanWinForm();

            var desktopMessageHandler = new DesktopMessageHandler();

            var thread = new Thread(() => desktopMessageHandler.StartMessageHandler(port, form));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            Application.Run(form);
        }
    }
}
