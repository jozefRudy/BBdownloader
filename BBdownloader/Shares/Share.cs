using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BBdownloader.DataSource;
using BBdownloader.FileSystem;
using System.IO;


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

        public bool DownloadFields()
        {
            foreach (var field in this.fields)
	        {
                DownloadField(field);
	        }
            return true;
        }

        private bool DownloadField(IField field)
        {
            DateTime startDate = new DateTime(2000, 10, 1);
            DateTime endDate = DateTime.Today.AddDays(-1);

            if (loadedValues.ContainsKey(field.FieldName))
            {
                var kvp = loadedValues[field.FieldName].Last();
                startDate = kvp.Key;
            }
            
            var output = new SortedList<DateTime, dynamic>();
            dataSource.DownloadData(securityName: this.name, inputField: field.FieldName, startDate: startDate, endDate: endDate, outList: out output);

            if (!downloadedValues.ContainsKey(field.FieldNickName))
            {
                downloadedValues.Add(field.FieldNickName, output);
            }
            return true;
        }
        
        public bool LoadFields()
        {
            foreach (var field in this.fields)
            {
                LoadField(field);
            }
            return true;
        }

        public bool LoadField(IField field)
        {
            var content = fileAccess.ReadFile(Path.Combine(this.name,field.FieldNickName + ".csv"));
            FileParser parser = new FileParser();

            loadedValues[field.FieldNickName] = parser.Read(content);

            return true;
        }

        public bool WriteFields()
        {
            foreach (var field in this.fields)
            {
                WriteField(field);
            }
            return true;
        }

        public bool WriteField(IField field)
        {
            if (combinedValues.ContainsKey(field.FieldNickName))
            {
                FileParser parser = new FileParser();
                string content = parser.Write(combinedValues[field.FieldNickName],",");
                fileAccess.WriteFile(Path.Combine(this.name, field.FieldNickName + ".csv"), content);
                return true;
            }
            return false;
        }

        public bool CombineLoadedDownladedAll()
        {
            foreach (var field in this.fields)
            {
                CombineLoadedDownloaded(field);
            }
            return true;
        }

        public bool CombineLoadedDownloaded(IField field)
        {
            return true;
        }
    }
}
