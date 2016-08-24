using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using QuantConnect.Packets;
using QuantConnect.Views.WinForms;

namespace QuantConnect.Views
{
    public class Program
    {
        
        [STAThread]
        static void Main(string[] args)
        {
            //if (args.Length != 1)
            //{
            //    throw new Exception(
            //        "Error: You must specify the port on which the application will open a TCP socket.");
            //}

            var port = "1234";//args[0];

            var desktopApi = new DesktopMessageHandler(port);
            var form = new LeanWinForm(desktopApi);

            desktopApi.ReceivedJobEvent += (packet) =>
            {
                form.Initialize(packet);
            };

            
            Application.Run(form);
        }
    }
}
