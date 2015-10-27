using System;
using System.Collections.Generic;

namespace BBdownloader.DataSource
{
    public interface IDataSource
    {
        string DefaultField { get; set; }
        bool Connect(string connectionString);
        void DownloadData(string securityName, string inputField, List<string[]> overrides, DateTime startDate, DateTime endDate, out SortedList<DateTime, dynamic> outList);
    }
}
