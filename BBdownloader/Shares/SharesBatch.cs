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


        private List<string> shareNames { get; set; }
        private List<string> sharesNew { get; set; }
        private List<string> sharesOld { get; set; }
        private IEnumerable<IField> fieldsHistorical { get; set; }
        private IEnumerable<IField> fieldsReference { get; set; }
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
        /// 

        private IEnumerable<IEnumerable<IField>> FieldBlocks(IEnumerable<IField> fields)
        {
            List<IField> batch = new List<IField>();
            for (int i = 0; i < fields.Count(); i++)
			{                
                if (fields.ElementAt(i).CompareTo(fields.ElementAtOrDefault(i-1)) == 0 && batch.Count() < maxFields)
                {
                    batch.Add(fields.ElementAt(i));
                }
			    else
                {
                    if (batch.Count()>0)
                        yield return batch;
                    batch = new List<IField>();
                    batch.Add(fields.ElementAt(i));
                }
			}            
        }

        public void PerformOperations()
        {
            Console.Write("Processing Batch");
            SharesNewOld(fields);
            FieldsNewOld();
            Console.Write("\nUpdating old: ");
            DownloadOldWithSameLastUpdateDate();
            Console.Write("\nUpdating new fields for old: ");
            DownloadNewFieldsForOldShares();
            Console.Write("\nUpdating new shares: ");
            DownloadNewShares();            
            Console.Write("\n");
        }


        private static IEnumerable<IEnumerable<IField>> SplitFields(IEnumerable<IField> fields)
        {
            var output = new List<List<IField>>();



            return null;
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

        }

        private void DownloadNew(List<string> shares, IEnumerable<IField> fields, DateTime? startDate)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("\nFields: ");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(fields.ToExtendedString());
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" Shares: ");
            Console.ForegroundColor = ConsoleColor.Gray;

            var output = dataSource.DownloadData(shares, fields.ToList(), startDate: startDate.HasValue ? startDate.Value : this.startDate, endDate: endDate);

            var enumerator = output.GetEnumerator();

            foreach (var s in shares)
            {
                Share share = new Share(s, fields, dataSource, fileAccess, startDate: startDate.HasValue ? startDate.Value : this.startDate, endDate: endDate);

                foreach (var f in fields)
	            {
                    enumerator.MoveNext();
                    var field = enumerator.Current;

                    share.InjectDownloaded(f, field);                                       
	            }
                share.FieldsToKeep(this.fields);
                share.PerformOperations();
            }
        }

        private void DownloadNewShares()
        {
            {
                var flds = fields.ToList();
                flds.Sort();

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
            newFields.Sort();
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
