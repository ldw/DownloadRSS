using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Configuration;
using System.Threading;
using System.IO;

namespace DownloadShow
{
    class Program
    {
        static void Main(string[] args)
        {
            //new DownloadShow("into trance", "http://djpitch.shiftingpeaks.com/intotrance", "322", ".mp3", @"c:\Temp\newshow.mp3", @"c:\Temp\newlog.txt");

            foreach (string key in ConfigurationManager.AppSettings)
            {
                try
                {
                    string value = ConfigurationManager.AppSettings[key];
                    string[] splitString = value.Split(',');
                    ////string myshow, string myurl, string myshownr, string myextensie, string @myscorpiopath, string @mylogfile
                    new DownloadShow(key, splitString[0], splitString[1], splitString[2], splitString[3], splitString[4]);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error reading values for: " + key + ". Missing a value?");
                    Console.WriteLine(e.Message);
                    Console.ReadLine();
                }
            }
        }
    }
}