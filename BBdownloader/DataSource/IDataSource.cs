using System;
using System.Collections.Generic;
using BBdownloader.Shares;

namespace BBdownloader.DataSource
{
    public interface IDataSource
    {
        bool Connect(string connectionString);

        IEnumerable<SortedList<DateTime,dynamic>> DownloadData(List<string> securityNames, List<IField> fields, DateTime? startDate = null, DateTime? endDate = null);

        void DownloadComponents(string index, string field, out List<string> members);
    }
}
