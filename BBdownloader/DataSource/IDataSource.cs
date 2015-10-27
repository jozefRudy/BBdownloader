using System;
using System.Collections.Generic;
using BBdownloader.Shares;

namespace BBdownloader.DataSource
{
    public interface IDataSource
    {
        string DefaultField { get; set; }
        bool Connect(string connectionString);
        void DownloadData(string securityName, IField field, DateTime startDate, DateTime endDate, out SortedList<DateTime, dynamic> outList);
    }
}
