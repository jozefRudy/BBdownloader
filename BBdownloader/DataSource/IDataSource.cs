using System;
using System.Collections.Generic;
using BBdownloader.Shares;

namespace BBdownloader.DataSource
{
    public interface IDataSource
    {
        bool Connect(string connectionString = "", string dataType = "//blp/refdata");

        IEnumerable<Tuple<string,SortedList<DateTime,dynamic>>> DownloadData(List<string> securityNames, List<IField> fields, DateTime? startDate = null, DateTime? endDate = null);

        //void DownloadData(string securityName, IField field, DateTime? startDate, DateTime? endDate, out SortedList<DateTime, dynamic> outList);

        void DownloadComponents(string index, string field, out List<string> members);

        void Disconnect();

        Dictionary<string,string> DownloadFieldInfo(string securityName, IEnumerable<IField> field);
    }
}
