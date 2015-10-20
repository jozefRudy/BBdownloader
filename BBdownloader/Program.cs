using System;
using BBdownloader.GoogleDocs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BBdownloader
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Sheets sheet = new Sheets();
            sheet.Add(new string[] { "19hRk5zO3GeJSsgh3v2anYibAYpkEGIs7xIrY3aEJZqw", "0" }); //shares
            sheet.Add(new string[] { "19hRk5zO3GeJSsgh3v2anYibAYpkEGIs7xIrY3aEJZqw", "1607987342" }); //indices
            sheet.Add(new string[] { "19hRk5zO3GeJSsgh3v2anYibAYpkEGIs7xIrY3aEJZqw", "794076055" }); //fields
            sheet.Add(new string[] { "19hRk5zO3GeJSsgh3v2anYibAYpkEGIs7xIrY3aEJZqw", "485268174" }); //shares reload
            sheet.Add(new string[] { "19hRk5zO3GeJSsgh3v2anYibAYpkEGIs7xIrY3aEJZqw", "485268174" }); //shares reload
            sheet.Add(new string[] { "19hRk5zO3GeJSsgh3v2anYibAYpkEGIs7xIrY3aEJZqw", "1767144829" }); //shares delete
           
            sheet.Download();

            string url = @"https://spreadsheets.google.com/feeds/download/spreadsheets/Export?key=19hRk5zO3GeJSsgh3v2anYibAYpkEGIs7xIrY3aEJZqw&exportFormat=csv&gid=1607987342";

            WebClientEx wc = new WebClientEx();

            var outputCSVdata = wc.DownloadString(url);
            Console.Write(outputCSVdata);


            {
                Stock stock = new Stock("AAPL");
                stock.CreateDirectory();
                stock.WriteDates("PUBLICATION DATE", 10);
                stock.WriteFloats("EARN", 10);
                stock.WriteField("INDUSTRY", DateTime.Today.AddDays(-20), "Consumer discretionary");
                stock.WriteField("LONG NAME", DateTime.Today.AddDays(-20), "Apple corporation");
            }

            {
                Stock stock = new Stock("MSFT");
                stock.CreateDirectory();
                stock.WriteDates("PUBLICATION DATE", 10);
                stock.WriteFloats("EARN", 10);
                stock.WriteField("INDUSTRY", DateTime.Today.AddDays(-20), "IT");
                stock.WriteField("LONG NAME", DateTime.Today.AddDays(-20), "Microsoft");
            }

            {
                Stock stock = new Stock("TSL");
                stock.CreateDirectory();
                stock.WriteDates("PUBLICATION DATE", 10);
                stock.WriteFloats("EARN", 10);
                stock.WriteField("INDUSTRY", DateTime.Today.AddDays(-20), "Automotive");
                stock.WriteField("LONG NAME", DateTime.Today.AddDays(-20), "Tesla");
            }
        }
    }
}
