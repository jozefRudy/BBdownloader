using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BBdownloader.DataSource;
using BBdownloader.FileSystem;

namespace BBdownloader.Shares
{
    public class SharesBatch
    {
        private List<string> shareNames { get; set; }
        private List<string> sharesNew { get; set; }
        private IEnumerable<IField> fieldsHistorical { get; set; }
        private IEnumerable<IField> fieldsReference { get; set; }

        private DateTime startDate { get; set; }
        private DateTime endDate { get; set; }

        IDataSource dataSource { get; set; }
        IFileSystem fileAccess { get; set; }

        public SharesBatch(List<string> stringNames, IEnumerable<IField> fields, IDataSource dataSource, IFileSystem fileAccess, DateTime? startDate = null, DateTime? endDate = null)
        {
            this.shareNames = stringNames;            
            this.dataSource = dataSource;
            this.fileAccess = fileAccess;


            fieldsHistorical = from f in fields
                               where f.requestType == "HistoricalDataRequest"
                               select f;

            fieldsReference = from f in fields
                              where f.requestType == "ReferenceDataRequest"
                              select f;

            if (startDate != null)
                this.startDate = startDate.Value;

            if (endDate != null)
                this.endDate = endDate.Value;
        }

        /// <summary>
        /// find shares with no data - no directory exists. Download all historical fields for them. Upload data.
        /// </summary>
        public void DownloadNew()
        {
            sharesNew = new List<string>();

            foreach (var shareName in shareNames)
            {
                var share = new Share(shareName, fieldsHistorical, dataSource, fileAccess);
                if (!share.ShareExists())
                    sharesNew.Add(shareName);
            }

            var output = dataSource.DownloadData(sharesNew, fieldsHistorical.ToList(), startDate: startDate, endDate: endDate);

            var enumerator = output.GetEnumerator();

            foreach (var shareNew in sharesNew)
            {
                Share share = new Share(shareNew, fieldsHistorical, dataSource, fileAccess);

                foreach (var fieldHistorical in fieldsHistorical)
	            {
                    enumerator.MoveNext();
                    var field = enumerator.Current;

                    //share.
	            }
            }


            foreach (var share in output)
            {
                
            }

        }

        public void DownloadNewFields()
        {

        }

        public void DownloadWithSameLastUpdateDate()
        {

        }
    }
}
