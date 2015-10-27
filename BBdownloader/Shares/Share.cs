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
        Dictionary<string, SortedList<DateTime,dynamic>> loadedValues { get; set; }
        Dictionary<string, SortedList<DateTime, dynamic>> downloadedValues { get; set; }
        Dictionary<string, SortedList<DateTime, dynamic>> combinedValues { get; set; }

        IEnumerable<IField> fields { get; set; }
        string name;
        IDataSource dataSource { get; set; }
        IFileSystem fileAccess { get; set; }
        private SortedList<DateTime, dynamic> _currentField;

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

            if (loadedValues.ContainsKey(field.FieldName))
            {
                var kvp = loadedValues[field.FieldName].Last();
                startDate = kvp.Key;
            }

            if (startDate >= endDate)
            {
                return false;                
            }
                
            var output = new SortedList<DateTime, dynamic>();
            dataSource.DownloadData(securityName: this.name, inputField: field.FieldName, startDate: startDate, endDate: endDate, outList: out output);

            if (!downloadedValues.ContainsKey(field.FieldNickName))
            {
                downloadedValues.Add(field.FieldNickName, output);
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

                if (!downloadedValues.ContainsKey(field.FieldNickName) || downloadedValues[field.FieldNickName].Count == 0)
                    return false;

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

        private bool CombineLoadedDownloaded(IField field)
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

            switch (field.FieldNickName)
            {
                case "adjusted_price":
                    { 
                        var loaded = loadedValues[field.FieldNickName].price2ret();
                        var downloaded = downloadedValues[field.FieldNickName].price2ret();
                        float lastPrice = downloadedValues[field.FieldNickName].Last().Value;

                        if (downloadedValues[field.FieldNickName].Count > 1)
                            combinedValues[field.FieldNickName] = loaded.merge(downloaded, 1).ret2price(lastPrice);
                        else
                            combinedValues[field.FieldNickName] = downloadedValues[field.FieldNickName];
                    }
                    break;                    
                default:
                    {
                        combinedValues[field.FieldNickName] =
                            loadedValues[field.FieldNickName].merge(downloadedValues[field.FieldNickName], 1);                        
                    }
                    break;
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

        public bool PerformOperations()
        {
            LoadFields();
            DownloadFields();
            CombineLoadedDownladedAll();
            WriteFields();
            DeleteFields();
            return true;
        }
    }
}
