using System;
using System.Collections.Generic;
using BBdownloader.Shares;

namespace BBdownloader.DataSource
{
    public interface IDataSource
    {
        string DefaultField { get; set; }
        bool Connect(string connectionString);
        void DownloadData(string securityName, IField field, DateTime? startDate, DateTime? endDate, out SortedList<DateTime, dynamic> outList);

        IEnumerable<SortedList<DateTime,dynamic>> DownloadData(List<string> securityNames, List<IField> fields, DateTime? startDate, DateTime? endDate);

        void DownloadComponents(string index, string field, out List<string> members);
    }
}
