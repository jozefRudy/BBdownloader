﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BBdownloader.DataSource;
using BBdownloader.FileSystem;
using System.IO;
using BBdownloader.Extension_Methods;
using System.Diagnostics;

namespace BBdownloader.Shares
{
    public class Share
    {
        Dictionary<string, SortedList<DateTime, dynamic>> loadedValues { get; set; }
        Dictionary<string, SortedList<DateTime, dynamic>> downloadedValues { get; set; }
        Dictionary<string, SortedList<DateTime, dynamic>> combinedValues { get; set; }

        IEnumerable<IField> fields { get; set; }
        string name;
        IDataSource dataSource { get; set; }
        IFileSystem fileAccess { get; set; }

        private float lastPrice { get; set; }

        public Share(string name, IEnumerable<IField> fields, IDataSource dataSource, IFileSystem fileAccess)
        {
            this.name = name;
            this.fields = fields;
            this.dataSource = dataSource;
            this.fileAccess = fileAccess;
            loadedValues = new Dictionary<string, SortedList<DateTime, dynamic>>();
            downloadedValues = new Dictionary<string, SortedList<DateTime, dynamic>>();
            combinedValues = new Dictionary<string, SortedList<DateTime, dynamic>>();
        }

        private bool DownloadFields()
        {
            foreach (var field in this.fields)
	        {
                DownloadField(field);
	        }
            return true;
        }

        private bool DownloadField(IField field)
        {
            DateTime startDate = new DateTime(1990, 1, 1);
            DateTime endDate = DateTime.Today.AddDays(-1);

            if (loadedValues.ContainsKey(field.FieldNickName) && loadedValues[field.FieldNickName].Count>0)
            {
                var kvp = loadedValues[field.FieldNickName].Last();
                startDate = kvp.Key;
            }

            if (startDate >= endDate)
            {
                return false;                
            }
                
            var output = new SortedList<DateTime, dynamic>();

            var collection = dataSource.DownloadData(new List<string> { this.name }, new List<IField> { field }, startDate: startDate, endDate: endDate);
            foreach (SortedList<DateTime, dynamic> item in collection)
                output = item;
                            
            //dataSource.DownloadData(securityName: this.name, field: field, startDate: startDate, endDate: endDate, outList: out output);
            /*
            var securityNames = new List<string>() { "SPXJSS Index", "MSFT US Equity" };

            IField field1 = new Field();
            field1.FieldName = "PX_OPEN";
            field1.requestType = "HistoricalDataRequest";

            IField field2 = new Field();
            field2.FieldName = "PX_LAST";

            var fields = new List<IField>() { field1, field2 };


            collection = dataSource.DownloadData(securityNames, fields, startDate, endDate);

            var hovno = new SortedList<DateTime, dynamic>();
            foreach (SortedList<DateTime, dynamic> item in collection)
                hovno = item;

            //field2.FieldName = "INDX_MEMBERS";
            field1.requestType = "ReferenceDataRequest";
            collection = dataSource.DownloadData(securityNames, fields, startDate, endDate);
            foreach (SortedList<DateTime, dynamic> item in collection)
                hovno = item;
            */

            if (!downloadedValues.ContainsKey(field.FieldNickName))
            {
                downloadedValues.Add(field.FieldNickName, output);
            }
            return true;
        }


        private bool TransformFields()
        {
            foreach (var field in this.fields)
            {
                TransformField(field);
            }

            return true;
        }

        
        private bool LoadFields()
        {
            foreach (var field in this.fields)
            {
                LoadField(field);
            }
            return true;
        }

        private bool LoadField(IField field)
        {
            var content = fileAccess.ReadFile(Path.Combine(this.name,field.FieldNickName + ".csv"));
            FileParser parser = new FileParser();

            loadedValues[field.FieldNickName] = parser.Read(content);

            return true;
        }

        private bool WriteFields()
        {
            foreach (var field in this.fields)
            {
                WriteField(field);
            }
            return true;
        }

        private bool WriteField(IField field)
        {
            if (combinedValues.ContainsKey(field.FieldNickName))
            {
                FileParser parser = new FileParser();
                string content = parser.Write(combinedValues[field.FieldNickName],",");

                if (!downloadedValues.ContainsKey(field.FieldNickName) || downloadedValues[field.FieldNickName]==null || downloadedValues[field.FieldNickName].Count == 0)
                    return false;

                if (!fileAccess.DirectoryExists(this.name))
                    fileAccess.CreateDirectory(this.name);

                fileAccess.WriteFile(Path.Combine(this.name, field.FieldNickName + ".csv"), content);
                return true;
            }
            return false;
        }

        private bool CombineLoadedDownladedAll()
        {
            foreach (var field in this.fields)
            {
                CombineLoadedDownloaded(field);
            }
            return true;
        }

        private bool DeleteFields()
        {
            var files = fileAccess.ListFiles(name);
            List<string> fields = new List<string>();

            var fieldNickNames = from f in this.fields
                                 select f.FieldNickName;
                                 
            foreach (var file in files)
            {
                var field = file.Split('.')[0];
                if (!fieldNickNames.Contains(field))
                    DeleteField(field);                
            }

            return true;
        }

        private bool DeleteField(string field)
        {
            fileAccess.DeleteFile(Path.Combine(name, field + ".csv"));
            return true;
        }

        private bool UnTransformFields()
        {
            foreach (var field in this.fields)
            {
                UnTransformField(field);
            }
            return true;
        }

        public bool PerformOperations()
        {
            LoadFields();
            DownloadFields();
            TransformFields();
            CombineLoadedDownladedAll();
            UnTransformFields();
            WriteFields();
            DeleteFields();
            return true;
        }

        private bool CombineLoadedDownloaded(IField field)
        {
            if (field.Transform.Contains("MERGE") || (!field.Transform.Contains("ONLYNEW") && !field.Transform.Contains("ONLYOLD")))
            {
                if (!loadedValues.ContainsKey(field.FieldNickName) ||
                    !downloadedValues.ContainsKey(field.FieldNickName))
                {
                    SortedList<DateTime, dynamic> outValue;
                    if (!loadedValues.TryGetValue(field.FieldNickName, out outValue))
                        downloadedValues.TryGetValue(field.FieldNickName, out outValue);
                    combinedValues[field.FieldNickName] = outValue;
                    return true;
                }

                combinedValues[field.FieldNickName] =
                                loadedValues[field.FieldNickName].merge(downloadedValues[field.FieldNickName], 1);
            }
            else if (field.Transform.Contains("ONLYNEW"))
            {
                SortedList<DateTime, dynamic> outValue = new SortedList<DateTime, dynamic>();
                downloadedValues.TryGetValue(field.FieldNickName, out outValue);
                combinedValues[field.FieldNickName] = outValue;
            }
            else if (field.Transform.Contains("ONLYOLD"))
            {
                SortedList<DateTime, dynamic> outValue = new SortedList<DateTime, dynamic>();
                loadedValues.TryGetValue(field.FieldNickName, out outValue);
                combinedValues[field.FieldNickName] = outValue;
            }

            return true;
        }

        private bool TransformField(IField field)
        {
            foreach (var f in field.Transform)
            {
                switch (f)
                {
                    case "TORETURNS":
                        if (loadedValues.ContainsKey(field.FieldNickName))
                            loadedValues[field.FieldNickName] = loadedValues[field.FieldNickName].price2ret();

                        if (downloadedValues.ContainsKey(field.FieldNickName))
                        {
                            lastPrice = downloadedValues[field.FieldNickName].Last().Value;
                            downloadedValues[field.FieldNickName] = downloadedValues[field.FieldNickName].price2ret();
                        }
                        break;
                    case "ONLYRIGHT":
                        
                        if (downloadedValues.ContainsKey(field.FieldNickName))
                        {                            
                            if (downloadedValues[field.FieldNickName] != null && downloadedValues[field.FieldNickName].Count > 0)
                            {
                                SortedList<DateTime, dynamic> newOutput = new SortedList<DateTime, dynamic>();

                                foreach (var item in downloadedValues[field.FieldNickName])
                                {
                                    if (!newOutput.ContainsKey(item.Value))
                                        newOutput.Add(item.Value, null);
                                }

                                downloadedValues[field.FieldNickName] = new SortedList<DateTime, dynamic>(newOutput);
                            }
                        }
                        break;
                    default:
                        break;
                }
            }

            return true;
        }

        private bool UnTransformField(IField field)
        {
            foreach (var f in field.Transform)
            {
                switch (f)
                {
                    case "TORETURNS":
                        if (combinedValues.ContainsKey(field.FieldNickName) && combinedValues[field.FieldNickName].Count > 1)
                            combinedValues[field.FieldNickName] = combinedValues[field.FieldNickName].ret2price(lastPrice);
                        break;
                    default:
                        break;
                }
            }


            return true;
        }
    }
}
