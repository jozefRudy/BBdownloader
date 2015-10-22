using System;
using BBdownloader.GoogleDocs;
using System.Collections.Generic;
using BBdownloader.Shares;
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
            List<string> shareNames = new List<string>();
            IEnumerable<Field> fields = new List<Field>();

            Sheet sheet = new Sheet();
            sheet.Download(new string[] { "19hRk5zO3GeJSsgh3v2anYibAYpkEGIs7xIrY3aEJZqw", "0" });
            shareNames = sheet.toShares();

            sheet.Download(new string[] { "19hRk5zO3GeJSsgh3v2anYibAYpkEGIs7xIrY3aEJZqw", "794076055" });
            var daco = sheet.toFields(fields);

            foreach (var shareName in shareNames)
            {
                //Share share = new Share(name: shareName);
            }



            /*
            sheet.Add(new string[] { "19hRk5zO3GeJSsgh3v2anYibAYpkEGIs7xIrY3aEJZqw", "1607987342" }); //indices
            sheet.Add(new string[] { "19hRk5zO3GeJSsgh3v2anYibAYpkEGIs7xIrY3aEJZqw", "794076055" }); //fields
            sheet.Add(new string[] { "19hRk5zO3GeJSsgh3v2anYibAYpkEGIs7xIrY3aEJZqw", "485268174" }); //shares reload
            sheet.Add(new string[] { "19hRk5zO3GeJSsgh3v2anYibAYpkEGIs7xIrY3aEJZqw", "485268174" }); //shares reload
            sheet.Add(new string[] { "19hRk5zO3GeJSsgh3v2anYibAYpkEGIs7xIrY3aEJZqw", "1767144829" }); //shares delete*/
          


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
