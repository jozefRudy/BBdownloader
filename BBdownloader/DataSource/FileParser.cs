using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;

namespace BBdownloader.DataSource
{
    public class FileParser
    {
        public SortedList<DateTime, dynamic> Read(string[] content)
        {
            var outList = new SortedList<DateTime, dynamic>();

            foreach (string row in content)
            {
                string[] items = row.Split(',');
                
                if (items.Length > 2)
                {
                    DateTime date = DateTime.Parse(items[0]);
                    dynamic parsedValue = "";

                    switch (items[2])
                    {
                        case "float":
                            parsedValue = float.Parse(items[1]);
                            break;
                        case "date":
                            parsedValue = DateTime.Parse(items[1]);
                            break;                          
                        case "string":
                            parsedValue = items[1];
                            break;
                        default:
                            break;
                    }
                    if (!outList.ContainsKey(date))
                        outList.Add(date, parsedValue);
                    
                }
            }
            return outList;
        }

        public string Write(SortedList<DateTime, dynamic> inList, string separator)
        {
            StringBuilder outputString = new StringBuilder();
            float value;
            string type = "";
            DateTime date;

            if (inList!=null && inList.Count>0)
            {
                var kvp = inList.ElementAt(0);

                string fieldValue = "";
                if (kvp.Value!=null)
                    fieldValue = kvp.Value.ToString();

                if (float.TryParse(fieldValue, out value))
                    type = ",float";
                else if (DateTime.TryParse(fieldValue, out date))
                {
                    type = ",date";
                }
                else
                    type = ",string";
            }

            foreach (var kvp in inList)
            {
                outputString.Append(kvp.Key.ToString(format: "yyyy/MM/dd"));
                outputString.Append(separator);

                string fieldValue = "";
                if (kvp.Value != null)
                    if (typeof(DateTime) == kvp.Value.GetType())
                    {
                        fieldValue = kvp.Value.ToString(format: "yyyy/MM/dd");
                    }
                    else
                        fieldValue = kvp.Value.ToString();
                outputString.Append(fieldValue);
                outputString.AppendLine(type);                
            }

            return outputString.ToString();
        }

        public string Write(string[] inList)
        {
            StringBuilder outputString = new StringBuilder();

            foreach (var item in inList)
            {
                outputString.AppendLine(item);
            }
            return outputString.ToString();
        }



        

    }
}
