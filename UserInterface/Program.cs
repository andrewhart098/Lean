using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            Console.WriteLine(args[0]);
            Console.WriteLine(args[1]);

            Console.WriteLine(args[2]);
            Console.WriteLine(args[3]);
            if (args.Length != 4)
            {
                throw new Exception(
                    "You must pass in the ports for the client and server as arguements to this executable");
            }

            var serverPort = args[0];
            var clientPort = args[1];
            var url = args[2];
            var holdUrl = args[3];

            var desktopApi = new DesktopMessageHandler(serverPort: serverPort, clientPort: clientPort);

            var form = new LeanWinForm(desktopApi, url, holdUrl);
            Application.Run(form);
        }
    }
}
