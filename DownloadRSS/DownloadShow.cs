using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace DownloadShow
{
    class DownloadShow
    {
        StringBuilder sb = new StringBuilder();
        private AutoResetEvent notifier = new AutoResetEvent(false);
        private string fullurl = "";
        private int shownr = 0;
        private string @scorpiopath;
        private string @logfile = "";
        private string show = "";
        private string url = "";
        private string extensie = "";

        public DownloadShow(string myshow, string myurl, string myshownr, string myextensie, string @myscorpiopath, string @mylogfile)
        {
            sb.Append(DateTime.Now + Environment.NewLine);
            sb.Append("Preparing for download - " + myshow);
            
            @scorpiopath = @myscorpiopath.TrimStart().TrimEnd();
            @logfile = @mylogfile.TrimStart().TrimEnd();
            show = myshow;
            extensie = myextensie;
            url = myurl;
            try
            {
                shownr = int.Parse(myshownr.Trim());
            }
            catch
            {
                sb.Append("De waarde voor parameter 'IntoTranceShowNumberToDownload' is geen geldig getal: " + shownr + Environment.NewLine);
                appendToLog(sb);
                return;
            }
            fullurl = myurl.TrimStart().TrimEnd() + shownr + myextensie;

            if (!System.IO.Directory.Exists(Path.GetDirectoryName(scorpiopath)))
            {
                sb.Append("Wegschrijven audiobestand: directory bestaat niet -> " + scorpiopath + Environment.NewLine);
                appendToLog(sb);
                return;
            }

            Uri uri;
            if (Uri.TryCreate(fullurl, UriKind.Absolute, out uri))
            {
                try
                {
                    // create the request
                    HttpWebRequest request = WebRequest.Create(uri) as HttpWebRequest;
                    // instruct the server to return headers only
                    request.Method = "HEAD";
                    // make the connection
                    HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                    // get the status code
                    HttpStatusCode status = response.StatusCode;
                    if (status != HttpStatusCode.OK)
                    {
                        sb.Append("Ongeldige URL: " + fullurl + Environment.NewLine);
                        appendToLog(sb);
                        return;
                    }
                }
                catch
                {
                    sb.Append("Ongeldige URL: " + fullurl + Environment.NewLine);
                    appendToLog(sb);
                    return;
                }
            }
            else
            {
                sb.Append("Ongeldige URL: " + fullurl + Environment.NewLine);
                appendToLog(sb);
                return;
            }
            WebClient mWebClient = new WebClient();

            Console.WriteLine("Start downloading: " + fullurl + " to path: " + scorpiopath);
            sb.Append("Start downloading: " + fullurl + " to path: " + scorpiopath + Environment.NewLine);

            mWebClient.DownloadProgressChanged += (sender, e) => progressPercentageChanged(e.ProgressPercentage);
            mWebClient.DownloadFileCompleted += (sender, e) => yourMethodToProcessTheFile(ref sb);
            mWebClient.DownloadFileAsync(uri, scorpiopath);

            notifier.WaitOne();
        }
        private object progressPercentageChanged(int p)
        {
            Console.Write("\r{0}%", p);
            Thread.Sleep(50);
            return null;
        }

        private string yourMethodToProcessTheFile(ref StringBuilder sb)
        {
            Console.WriteLine("Download complete" );
            Console.WriteLine();
            sb.Append("Download successful: " + DateTime.Now + Environment.NewLine);

            try
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                shownr = shownr+1;
                string newValue = url + "," + shownr.ToString() + "," + extensie + "," + @scorpiopath + "," + logfile ;//change shownummer... //    config.AppSettings.Settings["IntoTranceShowNumberToDownload"].Value = "" + ++shownr;
                config.AppSettings.Settings[show].Value = newValue;
                sb.Append("Nieuwe waarde voor nummering van volgende te downloaden show: " + shownr + Environment.NewLine);
                config.Save(ConfigurationSaveMode.Modified);
            }
            catch (Exception e)
            {
                sb.Append("Fout bij wegschrijven nieuwe waarden in App.config" + Environment.NewLine);
                sb.Append(e.Message + Environment.NewLine);
                if (e.InnerException != null)
                    sb.Append(e.InnerException.Message + Environment.NewLine);
                appendToLog(sb);
                Environment.Exit(0);
            }

            appendToLog(sb);
            notifier.Set();
            return "success";
        }

        private void appendToLog(StringBuilder sb)
        {
            sb.Append("-----------------------------------------------------------------------");
            sb.Append(Environment.NewLine);
            try
            {
                File.AppendAllText(@logfile, sb.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("Error saving log: ");
                Console.WriteLine(e.Message);
                Console.WriteLine();
                Console.WriteLine("Log:");
                Console.WriteLine(sb.ToString());
                Console.WriteLine();
            }
        }
    }
}
