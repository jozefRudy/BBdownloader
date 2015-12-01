using System;
using System.Collections.Generic;
using BBdownloader.Shares;

namespace BBdownloader.DataSource
{
    public class FakeData: IDataSource
    {
        static Random rnd = new Random();

        public string DefaultField { get; set; }
        public bool Connect(string connectionString)
        {
            return true;
        }

        public void DownloadData(string securityName, IField field, DateTime? startDate, DateTime? endDate, out SortedList<DateTime, dynamic> outList)
        {
            outList = new SortedList<DateTime, dynamic>();
            for (int i = 0; i < 20; i++)
            {
                outList.Add(DateTime.Today.AddDays(-20 + i), rnd.Next(-10000, 10000));
            }    
        }

        public void DownloadComponents(string index, string field, out List<string> members)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Tuple<string,SortedList<DateTime, dynamic>>> DownloadData(List<string> securityNames, List<IField> fields, DateTime? startDate, DateTime? endDate)
        {
            throw new NotImplementedException();
        }
    }
}
