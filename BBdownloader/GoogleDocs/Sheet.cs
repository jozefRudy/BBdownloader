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

        public void toFields<T>(List<Field> fields) where T: Field, new()
        {
            var rows = new List<string>();

            this.output = this.output.Replace("\r", "");
            rows = this.output.Split('\n' ).ToList();

            var headings = rows[0].Split(',').ToList();

            rows.RemoveAt(0);

            foreach (var r in rows)
            {
                var columns = r.Split(',');

                int i = -1;
                T field = new T();
                foreach (var col in columns)
                {
                    i++;
                    string heading = headings.ElementAtOrDefault(i);
                    if (heading != null && heading.Length > 0) 
                        heading = heading.ToLower();

                    switch (heading)
                    {
                        case "transform":
                            if (col.Length > 0)
                            {
                                var cols = col.Split(';');
                                foreach (var c in cols)
                                {
                                    var transform = c.Trim();
                                    if (transform.Length > 0)
                                        field.Transform.Add(transform.ToUpper());
                                }
                            }
                            else
                            { field.Transform.Add("MERGE"); }
                            break;
                        case "type":
                            if (col.Length > 0)
                                field.Type = col;
                            break;
                        case "override":
                            if (col.Length > 0)
                                field.Overrides.Add(col.Split(':'));
                            break;
                        case "fieldnickname":
                            field.FieldNickName = col;
                            break;
                        case "field":
                            field.FieldName = col;
                            break;
                        default:
                            break;
                    }
                    
                }
                fields.Add(field);
            }            
        }

    }
}
