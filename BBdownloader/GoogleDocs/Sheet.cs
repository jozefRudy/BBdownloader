using System;
using System.Collections.Generic;
using System.Linq;
using BBdownloader.Shares;
using System.Text;
using System.Threading.Tasks;

namespace BBdownloader.GoogleDocs
{
    public class Sheet
    {

        private string[] Id { get; set; }
        private string output;

        public Sheet()
        {
            this.Id = new string[2];
        }

        public void Download(string [] Id)
        {
            this.Id = Id;
            //string url = @"https://spreadsheets.google.com/feeds/download/spreadsheets/Export?key=19hRk5zO3GeJSsgh3v2anYibAYpkEGIs7xIrY3aEJZqw&exportFormat=csv&gid=1607987342";

            string url = String.Format(@"https://spreadsheets.google.com/feeds/download/spreadsheets/Export?key={0}&exportFormat=csv&gid={1}",this.Id[0],this.Id[1]);

            WebClientEx wc = new WebClientEx();

            var outputCSVdata = wc.DownloadString(url);

            this.output = outputCSVdata;
        }

        public List<string> toShares()
        {
            var listShares = new List<string>();

            this.output = this.output.Replace("\r", "");

            listShares = this.output.Split(new Char[] { ',', '\n' }).ToList();
            listShares.RemoveAt(0); //remove heading
            listShares = listShares.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList(); //deduplicate and remove empty

            return listShares;
        }

        public IEnumerable<IField> toFields(IEnumerable<IField> fields)
        {
            var listFields = new List<string>();

            this.output = this.output.Replace("\r", "");
            listFields = this.output.Split('\n' ).ToList();



            /*
            foreach (var field in fields)
            {
                

            }*/

            return null;
        }

    }
}
