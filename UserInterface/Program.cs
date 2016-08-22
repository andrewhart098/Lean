using System;
using System.Threading;
using System.Windows.Forms;
using QuantConnect.Util;
using QuantConnect.Views.WinForms;

namespace QuantConnect.Views
{
    class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            if (args.Length == 1)
            {
                var thread = new Thread(() => LaunchUX(args[0]));
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();

                var server = new SimpleHTTPServer(1238);
            }
            else
            {
                throw new Exception("No URL");
            }

        }

        static void LaunchUX(string url)
        {
            //Launch the UX
            var form = new LeanWinForm(new MessageHandler(), url);
            Application.Run(form);
        }
    }
}
