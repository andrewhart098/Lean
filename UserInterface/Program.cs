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

            var desktopMessageHandler = new DesktopMessageHandler(port);
            var form = new LeanWinForm(desktopMessageHandler);

            desktopMessageHandler.ReceivedJobEvent += (packet) =>
            {
                form.Initialize(packet);
            };


            Application.Run(form);
        }
    }
}
