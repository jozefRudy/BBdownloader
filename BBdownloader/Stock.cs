using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace BBdownloader
{
    public class Stock
    {
        public string _ticker { get; set; }

        public string separator { get; set; }


        static Random rnd = new Random();

        public Stock(string name)
        {
            _ticker = name;

            separator = ",";
        }


        public void CreateDirectory()
        {
            Directory.CreateDirectory(this._ticker);
        }      

        public void WriteField(string field, DateTime date, dynamic fieldValue)
        {
            string path = this._ticker + "\\" + field + ".csv";
                     
            Writer write = new Writer();

            write.Write(path, date, fieldValue, this.separator);
        }

        public void WriteDates(string field, int length)
        {
            List<DateTime> dates = new List<DateTime>();

            for (int i = 0; i < length; i++)
            {
                WriteField(field, DateTime.Today.Date.AddDays(-20 + i), DateTime.Today.AddDays(-20 + i));
            }
        }

        public void WriteFloats(string field, int length)
        {
            for (int i = 0; i < length; i++)
            {
                WriteField(field, DateTime.Today.AddDays(-20 + i), rnd.Next(-10000, 10000));
            }            
        }

        public void WriteStrings(string field, int length)
        {
            for (int i = 0; i < length; i++)
            {
                WriteField(field, DateTime.Today.AddDays(-20 + i), "CONSUMER DISCRETIONARY");
            }            
        }


    }
}
