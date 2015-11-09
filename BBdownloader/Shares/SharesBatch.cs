﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
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
        /// 

        public void PerformOperations()
        {
            Console.Write("Processing: ");
            SharesNew(fieldsHistorical);            
            DownloadShares();
            DownloadNewFields();
            DownloadWithSameLastUpdateDate();
            Console.Write("\n");
        }


        private void DownloadShares()
        {
            this.DownloadNew(sharesNew, fieldsHistorical);
            this.DownloadNew(sharesNew, fieldsReference);
        }

        private void SharesNew(IEnumerable<IField> fields)
        {
            sharesNew = new List<string>();

            foreach (var shareName in shareNames)
            {
                var share = new Share(shareName, fields, dataSource, fileAccess);
                if (!share.ShareExists())
                    sharesNew.Add(shareName);
            }
        }

        private void DownloadNew(List<string> sharesNew, IEnumerable<IField> fields)
        {
            var output = dataSource.DownloadData(sharesNew, fields.ToList(), startDate: startDate, endDate: endDate);

            var enumerator = output.GetEnumerator();

            foreach (var shareNew in sharesNew)
            {
                Share share = new Share(shareNew, fields, dataSource, fileAccess, startDate, endDate);

                foreach (var fieldHistorical in fields)
	            {
                    enumerator.MoveNext();
                    var field = enumerator.Current;

                    share.InjectDownloaded(fieldHistorical, field);                                       
	            }
                share.FieldsToKeep(fieldsHistorical.Concat(fieldsReference));
                share.PerformOperations();
            }
        }

        //check if field exists not present in random directory. If yes - get list of shares for which given fields are missing
        private void DownloadNewFields()
        {
            List<IField> newFieldsReference = new List<IField>();
            List<IField> newFieldsHistorical = new List<IField>();

            var sharesOld = from s in shareNames
                            where !this.sharesNew.Contains(s)
                            select s;

            Share share = new Share(sharesOld.First(), fieldsHistorical, dataSource, fileAccess);

            foreach (var f in fieldsHistorical.Concat(fieldsReference))
            {
                if (!share.FieldExists(f))
                {
                    if (f.requestType == "HistoricalDataRequest")
                        newFieldsHistorical.Add(f);
                    else
                        newFieldsReference.Add(f);
                }                    
            }

            if (sharesOld != null && sharesOld.Count() > 0)
            { 
                if (newFieldsHistorical != null && newFieldsHistorical.Count() > 0)
                    DownloadNew(sharesOld.ToList(), newFieldsHistorical);

                if (newFieldsReference != null && newFieldsReference.Count() > 0)
                    DownloadNew(sharesOld.ToList(), newFieldsReference);
            }
        }


        // check specific share for last update - for all historical fields. Extend to all shares, where the same conditions are met. Download
        private void DownloadWithSameLastUpdateDate()
        {

        }
    }
}