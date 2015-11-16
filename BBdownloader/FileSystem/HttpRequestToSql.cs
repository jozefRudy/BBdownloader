using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace BBdownloader.FileSystem
{ 
    class HttpRequestToSql
    {
        private string request = @"http://www.metatronlse.eu/BBdownloader/import_to_db.php?auto=1";
        private string id = @"&bbd_unique=";
        private string field = @"&attribute_name=";
        private string date = @"&value_date=";
        private string value = @"&value=";
        private string type = @"&value_typ=";

        public void DeleteTable()
        {
            string url = @"http://www.metatronlse.eu/BBdownloader/import_to_db.php?auto=1&truncate=1";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream resStream = response.GetResponseStream();
        }

        public void IssueRequest(string id, string field, string date, string value, string type)
        {
            string url = this.request + this.id + id + this.field + field + this.date + date + this.value + value + this.type + type;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream resStream = response.GetResponseStream();
        }

        public void UploadFolder(IFileSystem file, string folder)
        {
            if (!file.DirectoryExists(folder))
                return;

            var files = file.ListFiles(folder);

            string id = folder;

            foreach (var f in files)
            {
                var contents = file.ReadFile(Path.Combine(id,f));
                string field = Path.GetFileName(f).Split('.')[0];

                foreach (var line in contents)
                {
                    var lineSplit = line.Split(',');
                    string date = lineSplit[0];
                    date = date.Replace("/", "%2F");

                    string valueType = lineSplit[1];

                    string value = valueType.Split('~')[0];
                    string type = valueType.Split('~')[1].ToLower();

                    IssueRequest(id, field, date, value, type);
                }

            }

        }

    }
}
