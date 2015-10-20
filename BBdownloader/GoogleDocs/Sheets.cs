using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BBdownloader.GoogleDocs
{
    public class Sheets
    {

        private List<string[]> Ids { get; set; }

        public Sheets()
        {
            Ids = new List<string[]>();
        }


        public void Add(string[] add)
        {
            this.Ids.Add(add);
        }

        public void Download()
        {
            foreach (var id in this.Ids)
            {                
                //string url = @"https://spreadsheets.google.com/feeds/download/spreadsheets/Export?key=19hRk5zO3GeJSsgh3v2anYibAYpkEGIs7xIrY3aEJZqw&exportFormat=csv&gid=1607987342";


                string url = String.Format(@"https://spreadsheets.google.com/feeds/download/spreadsheets/Export?key={0}&exportFormat=csv&gid={1}",id[0],id[1]);

                WebClientEx wc = new WebClientEx();

                var outputCSVdata = wc.DownloadString(url);

                Console.Write(outputCSVdata);
            }            
        }

    }
}
