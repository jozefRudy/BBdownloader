using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;

namespace BBdownloader
{
    public class Writer
    {

        public void Write(string path, DateTime fieldDate, dynamic fieldValueDynamic, string separator)
        {
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

            if (!File.Exists(path))
                using (StreamWriter sw = File.CreateText(path))
                {
                    sw.Write(fieldDate.ToString(format: "yyyy/MM/dd"));

                    sw.Write(separator);

                    sw.Write(fieldValue);

                    sw.WriteLine(type);
                }
            else
                using (StreamWriter sw = File.AppendText(path))
                {
                    sw.Write(fieldDate.ToString(format: "yyyy/MM/dd"));

                    sw.Write(separator);

                    sw.Write(fieldValue);

                    sw.WriteLine(type);
                }

        }

    }
}
