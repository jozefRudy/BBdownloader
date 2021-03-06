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
        Dictionary<string, string> fieldDefinitions { get; set; }

        Dictionary<string, SortedList<DateTime, dynamic>> loadedValues { get; set; }
        Dictionary<string, SortedList<DateTime, dynamic>> downloadedValues { get; set; }
        Dictionary<string, SortedList<DateTime, dynamic>> combinedValues { get; set; }

        IEnumerable<IField> fields { get; set; }
        IEnumerable<IField> fieldsToKeep { get; set; }

        public string name;
        IDataSource dataSource { get; set; }
        IFileSystem fileAccess { get; set; }

        private DateTime startDate { get; set; }
        private DateTime endDate { get; set; }

        private Dictionary<string, float> lastPrice { get; set; }

        public Share(string name, IEnumerable<IField> fields, IDataSource dataSource, IFileSystem fileAccess, DateTime? startDate = null, DateTime? endDate = null)
        {
            this.name = name;
            this.fields = fields;
            this.dataSource = dataSource;
            this.fileAccess = fileAccess;

            fieldDefinitions = new Dictionary<string, string>();
            loadedValues = new Dictionary<string, SortedList<DateTime, dynamic>>();
            downloadedValues = new Dictionary<string, SortedList<DateTime, dynamic>>();
            combinedValues = new Dictionary<string, SortedList<DateTime, dynamic>>();

            lastPrice = new Dictionary<string, float>();

            if (startDate != null)
                this.startDate = startDate.Value;

            if (endDate != null)
                this.endDate = endDate.Value;
        }

        private bool DownloadFields()
        {
            var anyDownloaded = from f in this.fields
                                select DownloadField(f);

            bool wasAnyDownloaded = false;

            foreach (var any in anyDownloaded)
            {
                if (any == true)
                    wasAnyDownloaded = true;
            }

            if (wasAnyDownloaded)
                return true;
            else
                return false;
        }

        private bool DownloadField(IField field)
        {
            var startDate = this.startDate;

            if (loadedValues.ContainsKey(field.FieldNickName) && loadedValues[field.FieldNickName].Count > 0)
            {
                var kvp = loadedValues[field.FieldNickName].Last();
                startDate = kvp.Key;
            }

            if (startDate >= endDate)
                return false;

            string file = Path.Combine(this.name.StripOfIllegalCharacters(), field.FieldNickName + ".csv");
            if (fileAccess.LastModifiedDate(file) != null && fileAccess.LastModifiedDate(file).Value == DateTime.Now.Date)
                return false;

            if (!downloadedValues.ContainsKey(field.FieldNickName))
            {
                var output = dataSource.DownloadData(new List<string> { this.name }, new List<IField> { field }, startDate: startDate, endDate: endDate).First().Item2;

                if (output == null || output.Count() == 0)
                    return false;

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
            string file = Path.Combine(this.name.StripOfIllegalCharacters(), field.FieldNickName + ".csv");

            if (!fileAccess.FileExists(file))
                return false;

            var content = fileAccess.ReadFile(file);
            FileParser parser = new FileParser();

            loadedValues[field.FieldNickName] = parser.Read(content);

            return true;
        }

        public DateTime? CheckLatest(IField field)
        {
            if (!this.FieldExists(field))
                return null;

            this.LoadField(field);


            var outList = new SortedList<DateTime, dynamic>();
            this.loadedValues.TryGetValue(field.FieldNickName, out outList);

            return outList.Last().Key;
        }

        private bool WriteFields()
        {
            foreach (var field in this.fields)
            {
                WriteField(field);
            }
            return true;
        }

        public bool FieldExists(IField field)
        {
            return fileAccess.FileExists(Path.Combine(this.name.StripOfIllegalCharacters(), field.FieldNickName + ".csv"));
        }

        public bool ShareExists()
        {
            return fileAccess.DirectoryExists(this.name.StripOfIllegalCharacters());
        }

        private bool WriteField(IField field)
        {
            if (combinedValues.ContainsKey(field.FieldNickName) && combinedValues[field.FieldNickName] != null)
            {
                FileParser parser = new FileParser();
                string content = parser.Write(combinedValues[field.FieldNickName], ",");

                if (!downloadedValues.ContainsKey(field.FieldNickName) || downloadedValues[field.FieldNickName] == null || downloadedValues[field.FieldNickName].Count == 0)
                    return false;

                if (!fileAccess.DirectoryExists(this.name.StripOfIllegalCharacters()))
                    fileAccess.CreateDirectory(this.name.StripOfIllegalCharacters());

                fileAccess.WriteFile(Path.Combine(this.name.StripOfIllegalCharacters(), field.FieldNickName + ".csv"), content);
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

        public void FieldsToKeep(IEnumerable<IField> fieldsToKeep)
        {
            this.fieldsToKeep = fieldsToKeep;
        }

        private bool DeleteFields()
        {
            if (!fileAccess.DirectoryExists(this.name.StripOfIllegalCharacters()))
                fileAccess.CreateDirectory(this.name.StripOfIllegalCharacters());

            var files = fileAccess.ListFiles(name.StripOfIllegalCharacters());
            List<string> fields = new List<string>();

            IEnumerable<string> fieldNickNames;

            if (this.fieldsToKeep == null)
            {
                fieldNickNames = from f in this.fields
                                 select f.FieldNickName;
            }
            else
            {
                fieldNickNames = from f in fieldsToKeep
                                 select f.FieldNickName;
            }


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
            fileAccess.DeleteFile(Path.Combine(name.StripOfIllegalCharacters(), field + ".csv"));
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

        public void InjectDownloaded(IField field, SortedList<DateTime, dynamic> data)
        {
            downloadedValues[field.FieldNickName] = new SortedList<DateTime, dynamic>(data);
        }

        public bool DoWork()
        {
            try
            {
                LoadFields();
                if (DownloadFields())
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Trace.Write(this.name);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Trace.Write(", ");
                }
                else
                {
                    Trace.Write("~" + this.name);
                    Trace.Write(", ");
                }
                TransformFields();
                CombineLoadedDownladedAll();
                UnTransformFields();
                WriteFields();
                DeleteFields();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception caught for share {this.name}: {e}");

                var loaded_values = loadedValues.Select(kvp => kvp.Key + "- " + kvp.Value.Keys.LastOrDefault().ToString() + ": " + kvp.Value.Values.LastOrDefault());
                var downloaded_values = downloadedValues.Select(kvp => kvp.Key + "- " + kvp.Value.Keys.LastOrDefault().ToString() + ": " + kvp.Value.Values.LastOrDefault());
                Console.WriteLine("loaded values: +\n");
                Console.WriteLine(string.Join(Environment.NewLine, loaded_values));
                Console.WriteLine("downloaded values: +\n");
                Console.WriteLine(string.Join(Environment.NewLine, downloaded_values));
            }
            return true;

        }

        public bool DoWorkFieldInfo()
        {
            var toDownload = new List<IField>();
            foreach (var f in this.fields)
            {
                string file = Path.Combine(f.FieldNickName + ".csv");
                if (fileAccess.FileExists(file))
                {
                    var content = fileAccess.ReadFile(file);
                    fieldDefinitions[f.FieldNickName] = content.ToString();
                }
                else
                    toDownload.Add(f);
            }

            var outDict = dataSource.DownloadFieldInfo(this.name, toDownload);

            if (outDict == null || outDict.Count() == 0)
                return false;

            foreach (var f in outDict)
            {
                if (!fieldDefinitions.ContainsKey(f.Key))
                {
                    string file = Path.Combine(f.Key + ".csv");
                    fileAccess.WriteFile(file, f.Value.ToString());
                    fieldDefinitions.Add(f.Key, f.Value.ToString());
                }
            }

            //delete non present fields in gdocs
            var files = fileAccess.ListFiles("");
            foreach (var file in files)
            {
                var field = file.Split('.')[0];
                if (!fieldDefinitions.ContainsKey(field))
                    fileAccess.DeleteFile(file);
            }

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

                if (field.requestType == "HistoricalDataRequest")
                {
                    combinedValues[field.FieldNickName] =
                        loadedValues[field.FieldNickName].merge(downloadedValues[field.FieldNickName], 0);
                }
                else
                {
                    combinedValues[field.FieldNickName] =
                        loadedValues[field.FieldNickName].mergeUniqueValues(downloadedValues[field.FieldNickName]);
                }

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
                        {
                            loadedValues[field.FieldNickName] = loadedValues[field.FieldNickName].price2ret();

                        }
                        if (downloadedValues.ContainsKey(field.FieldNickName) && downloadedValues[field.FieldNickName].Count() > 0)
                        {
                            lastPrice[field.FieldNickName] = downloadedValues[field.FieldNickName].Last().Value;
                            downloadedValues[field.FieldNickName] = downloadedValues[field.FieldNickName].price2ret();
                        }
                        if (!lastPrice.ContainsKey(field.FieldNickName) && loadedValues.ContainsKey(field.FieldNickName) && loadedValues[field.FieldNickName].Count() > 0)
                            lastPrice[field.FieldNickName] = loadedValues[field.FieldNickName].Last().Value;
                        break;
                    case "ONLYRIGHT":

                        if (downloadedValues.ContainsKey(field.FieldNickName))
                        {
                            if (downloadedValues[field.FieldNickName] != null && downloadedValues[field.FieldNickName].Count > 0)
                            {
                                SortedList<DateTime, dynamic> newOutput = new SortedList<DateTime, dynamic>();

                                foreach (var item in downloadedValues[field.FieldNickName])
                                {
                                    if (item.Value != null)
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
                        if (combinedValues.ContainsKey(field.FieldNickName) && combinedValues[field.FieldNickName] != null && combinedValues[field.FieldNickName].Count > 1)
                            combinedValues[field.FieldNickName] = combinedValues[field.FieldNickName].ret2price(lastPrice[field.FieldNickName]);
                        break;
                    default:
                        break;
                }
            }


            return true;
        }
    }
}
