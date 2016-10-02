using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using BBdownloader.DataSource;
using BBdownloader.FileSystem;
using BBdownloader.Extension_Methods;

namespace BBdownloader.Shares
{
    public class SharesBatch
    {
        private readonly int maxFields = 25;
        private readonly int maxShares = 50;


        private List<string> shareNames { get; set; }
        private List<string> sharesNew { get; set; }
        private List<string> sharesOld { get; set; }
        private IEnumerable<IField> fields { get; set; }

        private List<IField> newFields { get; set; }
        private List<IField> oldFields { get; set; }


        private DateTime startDate { get; set; }
        private DateTime endDate { get; set; }

        IDataSource dataSource { get; set; }
        IFileSystem fileAccess { get; set; }

        public SharesBatch(List<string> stringNames, IEnumerable<IField> fields, IDataSource dataSource, IFileSystem fileAccess, DateTime? startDate = null, DateTime? endDate = null)
        {
            this.shareNames = stringNames;            
            this.dataSource = dataSource;
            this.fileAccess = fileAccess;

            this.fields = fields;

            if (startDate != null)
                this.startDate = startDate.Value;

            if (endDate != null)
                this.endDate = endDate.Value;
        }

        private IEnumerable<IEnumerable<string>> ShareBlocks(IEnumerable<string> shares)
        {
            List<string> batch = new List<string>();
            for (int i = 0; i < shares.Count(); i++)
            {
                if (batch.Count() == 0 || batch.Count() < maxShares)
                    batch.Add(shares.ElementAt(i));
                
                if (batch.Count() >= maxShares || shares.ElementAtOrDefault(i+1) == null )
                {
                    if (batch.Count() > 0)
                        yield return batch;
                    batch = new List<string>();
                }
                
            }

        }

        private IEnumerable<IEnumerable<IField>> FieldBlocks(IEnumerable<IField> fields)
        {
            List<IField> batch = new List<IField>();
            for (int i = 0; i < fields.Count(); i++)
			{                
                if (batch.Count() == 0 || (batch.Last().CompareTo(fields.ElementAtOrDefault(i))==0 && batch.Count() < maxFields))                                       
                {
                    batch.Add(fields.ElementAt(i));
                }

                if (batch.Last().CompareTo(fields.ElementAtOrDefault(i+1)) != 0 || batch.Count() >= maxFields)
                {
                    if (batch.Count()>0)
                        yield return batch;
                    batch = new List<IField>();
                }
			}            
        }

        public void PerformOperations()
        {
            Trace.Write("Processing Batch");
            SharesNewOld(fields);
            FieldsNewOld();
            Console.ForegroundColor = ConsoleColor.White;
            Trace.Write("\nUpdating old fields for old shares: ");
            Console.ForegroundColor = ConsoleColor.Gray;
            DownloadOldWithSameLastUpdateDate();

            Console.ForegroundColor = ConsoleColor.White;
            Trace.Write("\nUpdating new fields for old shares: ");
            Console.ForegroundColor = ConsoleColor.Gray;
            DownloadNewFieldsForOldShares();

            Console.ForegroundColor = ConsoleColor.White;
            Trace.Write("\nUpdating new shares: ");
            Console.ForegroundColor = ConsoleColor.Gray;
            DownloadNewShares();

            Trace.Write("\n");
        }


        private void SharesNewOld(IEnumerable<IField> fields)
        {
            sharesNew = new List<string>();
            sharesOld = new List<string>();
            foreach (var shareName in shareNames)
            {
                var share = new Share(shareName, fields, dataSource, fileAccess);
                if (!share.ShareExists())
                    sharesNew.Add(shareName);
            }
            sharesOld = (from s in shareNames
                        where !this.sharesNew.Contains(s)
                        select s).ToList();
        }

        private void FieldsNewOld()
        {
            newFields = new List<IField>();
            oldFields = new List<IField>();

            if (sharesOld == null || sharesOld.Count() == 0)
                return;

            Share share = new Share(sharesOld.First(), fields, dataSource, fileAccess);
            foreach (var f in fields)
            {
                if (!share.FieldExists(f))
                    newFields.Add(f);
                else
                    oldFields.Add(f);
            }

            newFields.Sort();
            oldFields.Sort();

        }

        private void DownloadNew(List<string> shares, IEnumerable<IField> fields, DateTime? startDate)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Trace.Write("\nFields: ");
            Console.ForegroundColor = ConsoleColor.Gray;
            Trace.Write(fields.ToExtendedString());
            Console.ForegroundColor = ConsoleColor.White;
            Trace.Write(" Shares: ");
            Console.ForegroundColor = ConsoleColor.Gray;

            var equities = from s in shares
                           select s;

            foreach (var shareBlock in this.ShareBlocks(shares))
            {           
                var output = dataSource.DownloadData(shareBlock.ToList(), fields.ToList(), startDate: startDate.HasValue ? startDate.Value : this.startDate, endDate: endDate);

                var enumerator = output.GetEnumerator();

                foreach (var s in shareBlock)
                {
                    var share = new Share(" ", fields, dataSource, fileAccess, startDate: startDate.HasValue ? startDate.Value : this.startDate, endDate: endDate);

                    foreach (var f in fields)
	                {                    
                        enumerator.MoveNext();
                        var field = enumerator.Current;
                        share.name = field.Item1;
                        share.InjectDownloaded(f, field.Item2);                
	                }                
                    share.FieldsToKeep(this.fields);
                    share.DoWork();
                }
            }
        }

        private void DownloadNewShares()
        {
            {
                var flds = fields.ToList();
                flds.Sort();

                if (sharesNew.Count() == 0)
                    return;

                foreach (var f in FieldBlocks(flds))
                {
                    if (f != null && f.Count() > 0)
                        this.DownloadNew(sharesNew, f, null);
                }
            }
        }

        //check if field exists not present in random directory. If yes - get list of shares for which given fields are missing
        private void DownloadNewFieldsForOldShares()
        {
            foreach (var f in FieldBlocks(newFields))
            {
                if (f != null && f.Count() > 0)
                    this.DownloadNew(sharesOld.ToList(), f, null);
            }
        }


        // check specific share for last update - for all historical fields. Extend to all shares, where the same conditions are met. Download
        private void DownloadOldWithSameLastUpdateDate()
        {
            if (oldFields == null || oldFields.Count()==0)
                return;

            var oldFieldsReference = (from f in oldFields
                            where f.requestType == "ReferenceDataRequest"
                            select f).ToList();
            oldFieldsReference.Sort();

            foreach (var f in FieldBlocks(oldFieldsReference))
            {
                if (f != null && f.Count() > 0)
                    this.DownloadNew(sharesOld.ToList(), f, null);
            }

            var oldFieldsHistorical = (from f in oldFields
                                      where f.requestType == "HistoricalDataRequest"
                                      select f).ToList();
            oldFieldsHistorical.Sort();

            Share share = new Share(sharesOld.First(), fields, dataSource, fileAccess);

            DateTime oldestUpdate = this.endDate;
            foreach (var f in oldFieldsHistorical)
            {               
                var update = share.CheckLatest(f);
                if (update!=null)
                {
                    if (update < oldestUpdate)
                        oldestUpdate = update.Value;
                }              
            }

            if (oldestUpdate <= this.startDate)
                return;

            foreach (var f in FieldBlocks(oldFieldsHistorical))
            {
                if (f != null && f.Count() > 0)
                    this.DownloadNew(sharesOld.ToList(), f, oldestUpdate);
            }
        }
    }
}
