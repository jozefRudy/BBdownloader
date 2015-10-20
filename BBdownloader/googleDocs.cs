using System;
using System.Net;
using BBdownloader.DownloadData;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace BBDownloader
{
    class Program
    {


        static void Main(string[] args)
        {
            string url = @"https://spreadsheets.google.com/feeds/download/spreadsheets/Export?key=19hRk5zO3GeJSsgh3v2anYibAYpkEGIs7xIrY3aEJZqw&exportFormat=csv&gid=1607987342";


            WebClientEx wc = new WebClientEx(new CookieContainer());
            wc.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:22.0) Gecko/20100101 Firefox/22.0");
            wc.Headers.Add("DNT", "1");
            wc.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            wc.Headers.Add("Accept-Encoding", "deflate");
            wc.Headers.Add("Accept-Language", "en-US,en;q=0.5");

            var outputCSVdata = wc.DownloadString(url);
            Console.Write(outputCSVdata);
        }
    }
}