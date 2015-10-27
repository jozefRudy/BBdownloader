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
                
                if (items.Length > 1)
                {
                    DateTime date = DateTime.Parse(items[0]);
                    dynamic parsedValue = "";

                    var value = items[1].Split('~');

                    if (value.Length>1)
                    {
                        switch (value[1])
                        {
                            case "FLOAT":
                                parsedValue = float.Parse(value[0]);
                                break;
                            case "DATE":
                                parsedValue = DateTime.Parse(value[0]);
                                break;                          
                            case "STRING":
                                parsedValue = value[0];
                                break;
                            default:
                                break;
                        }
                        if (!outList.ContainsKey(date))
                            outList.Add(date, parsedValue);
                    }
                }
            }
            return outList;
        }


        public string Write(string path, DateTime fieldDate, dynamic fieldValueDynamic, string separator)
        {
            StringBuilder outputString = new StringBuilder();
            float value;
            string type;
            DateTime date;

            string fieldValue = fieldValueDynamic.ToString();

            if (float.TryParse(fieldValue, out value))
                type = "~FLOAT";
            else if (DateTime.TryParse(fieldValue, out date))
            {
                type = "~DATE";
                fieldValue = date.ToString(format: "yyyy/MM/dd");
            }
            else
                type = "~STRING";

            outputString.Append(fieldDate.ToString(format: "yyyy/MM/dd"));
            outputString.Append(separator);
            outputString.Append(fieldValue);
            outputString.Append(type);

            return outputString.ToString();
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

                string fieldValue = kvp.Value.ToString();

                if (float.TryParse(fieldValue, out value))
                    type = "~FLOAT";
                else if (DateTime.TryParse(fieldValue, out date))
                {
                    type = "~DATE";
                    fieldValue = date.ToString(format: "yyyy/MM/dd");
                }
                else
                    type = "~STRING";
            }

            foreach (var kvp in inList)
            {
                outputString.Append(kvp.Key.ToString(format: "yyyy/MM/dd"));
                outputString.Append(separator);
                outputString.Append(kvp.Value);
                outputString.AppendLine(type);                
            }

            return outputString.ToString();
        }





        

    }
}
