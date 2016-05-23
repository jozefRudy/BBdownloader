using System;
using System.Collections.Generic;
using System.Linq;
using BBdownloader.Shares;


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

            string url = String.Format($"https://docs.google.com/spreadsheets/d/{this.Id[0]}/export?format=csv&id=KEY&gid={this.Id[1]}");
           
            WebClientEx wc = new WebClientEx();
            var dt = wc.DownloadString(url);
            this.output = dt;
        }

        public HashSet<string> toShares()
        {
            var listShares = new List<string>();

            this.output = this.output.Replace("\r", "");

            listShares = this.output.Split(new Char[] { ',', '\n' }).ToList();
            listShares.RemoveAt(0); //remove heading
            listShares = listShares.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList(); //deduplicate and remove empty

            var uniqueShares = new HashSet<string>();
            foreach (var item in listShares)
                uniqueShares.Add(item);

            return uniqueShares;
        }

        public void toFields<T>(List<T> fields) where T: IField, new()
        {
            var rows = new List<string>();

            this.output = this.output.Replace("\r", "");
            rows = this.output.Split('\n' ).ToList();

            var headings = rows[0].Split(',').ToList();

            rows.RemoveAt(0);

            foreach (var r in rows)
            {

                SortedDictionary<string, string> overrides = new SortedDictionary<string,string>();

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
                        case "requesttype":
                            if (col.Length > 0)
                            {
                                field.requestType = col;
                            }
                            else
                                field.requestType = "HistoricalDataRequest";
                            break;
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
                            {
                                var cols = from c in col.Split(':')
                                           where c.Length > 0
                                           select c.Trim();

                                overrides.Add(cols.ElementAt(0),cols.ElementAt(1));                                
                            }
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
                field.Overrides = new SortedDictionary<string,string>(overrides);

                fields.Add(field);
            }            
        }

    }
}
