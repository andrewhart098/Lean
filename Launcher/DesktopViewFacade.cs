using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using QuantConnect.Interfaces;
using QuantConnect.Messaging;
using QuantConnect.Packets;

namespace QuantConnect.Lean.Launcher
{
    public class DesktopViewFacade
    {
        private AlgorithmNodePacket _job;
        private EventMessagingHandler _messaging;

        public delegate void SendMessagesHandler(Object sender, EventArgs e);
        public DesktopViewFacade(IMessagingHandler messaging, AlgorithmNodePacket job)
        {
            _messaging = (EventMessagingHandler) messaging;
            _job = job;

            

            Run();
        }

        private void Run()
        {
            var url = GetUrl(_job, false);
            Process.Start(@".\..\..\..\UserInterface\bin\Debug\QuantConnect.Views.exe", url);

        }

        /// <summary>
        /// Get the URL for the embedded charting
        /// </summary>
        /// <param name="job">Job packet for the URL</param>
        /// <param name="liveMode">Is this a live mode chart?</param>
        /// <param name="holdReady">Hold the ready signal to inject data</param>
        private static string GetUrl(AlgorithmNodePacket job, bool liveMode = false, bool holdReady = false)
        {
            var url = "";
            var hold = holdReady == false ? "0" : "1";
            var embedPage = liveMode ? "embeddedLive" : "embedded";

            url = string.Format(
                "https://www.quantconnect.com/terminal/{0}?user={1}&token={2}&pid={3}&version={4}&holdReady={5}&bid={6}",
                embedPage, job.UserId, job.Channel, job.ProjectId, Globals.Version, hold, job.AlgorithmId);

            // Turn these into exceptions
            //Show warnings if the API token and UID aren't set.
            //if (_job.UserId == 0)
            //{
            //    MessageBox.Show("Your user id is not set. Please check your config.json file 'job-user-id' property.", "LEAN Algorithmic Trading", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //}
            //if (_job.Channel == "")
            //{
            //    MessageBox.Show("Your API token is not set. Please check your config.json file 'api-access-token' property.", "LEAN Algorithmic Trading", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //}

            return url;
        }
    }
}
