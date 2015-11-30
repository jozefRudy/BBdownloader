using System;
using BBdownloader.GoogleDocs;
using System.Collections.Generic;
using BBdownloader.Shares;
using System.Linq;
using BBdownloader.DataSource;
using BBdownloader.FileSystem;
using BBdownloader.Extension_Methods;

namespace BBdownloader
{
    static class Program
    {
        [STAThread]
        static void Main()
        {           
            DateTime startDate = new DateTime(1990, 1, 1);
            DateTime endDate = DateTime.Today.GetBusinessDay(-1);
            
            ConfigBase config = new ConfigBase();
            config.Load("settings.cfg");

                       
            string gdocsSheet = config.GetValue("sheetCode");

            List<string> shareNames = new List<string>();
            List<string> indexNames = new List<string>();
            var fields = new List<Field>();

            Sheet sheet = new Sheet();
            sheet.Download(new string[] { gdocsSheet, config.GetValue("shareNames") });
            shareNames = sheet.toShares();

            sheet.Download(new string[] { gdocsSheet, config.GetValue("fields") });
            sheet.toFields<Field>(fields);

            sheet.Download(new string[] { gdocsSheet, config.GetValue("indices") });
            indexNames = sheet.toShares();

            IDataSource dataSource = new Bloomberg();

            /*
            dataSource.Connect("");
                        
            {
                //obtain components of indices
                var names = new List<string>();
                foreach (var index in indexNames)
                {
                    var outList = new List<string>();
                    dataSource.DownloadComponents(index, "INDX_MEMBERS", out outList);
                    names.AddRange(outList);
                }

                //convert tickers -> BB IDs
                var bbIDs = dataSource.DownloadData(names, new List<IField> { new Field() { FieldName = "ID_BB_GLOBAL", requestType ="ReferenceDataRequest" } });
                var listIDs = from ids in bbIDs.RemoveDates()
                              select (string)ids;
                shareNames.AddRange(listIDs.ToList());
            }*/

            //download data
            
            LocalDisk disk = new LocalDisk();
            disk.SetPath("data");
            /*
            {                           
                var shares = new SharesBatch(shareNames, fields, dataSource, disk, startDate, endDate);
                shares.PerformOperations();

                Console.Write("Processing Individual: ");
                foreach (var shareName in shareNames)
                {
                    Share share = new Share(name: shareName, fields: fields, dataSource: dataSource, fileAccess: disk, startDate: startDate, endDate: endDate);
                    share.PerformOperations();
                }
            }*/

            //upload data via SQL connection
            var database = new MySQL(config.GetValue("sqlIP"), config.GetValue("sqlUser"), config.GetValue("sqlPass"), config.GetValue("sqlDB"), "data", disk);
            database.DoWork();
            Console.ReadKey();

            
            //upload data to FTP server
            /*
            {
                Console.Write("\n");
                Console.WriteLine("Uploading Files to FTP server:");

                var diskDirectories = disk.ListDirectories("");

                Ftp ftp = new Ftp(config.GetValue("ftpIP"), config.GetValue("ftpLogin"), config.GetValue("ftpPass"));
                ftp.SetPath("BBdownloader1");

                var directoryList = ftp.ListDirectories("");
               
                Mirror mirror = new Mirror(disk, ftp);
                mirror.PerformOperations();
                
                //test with mato
            }            

            //upload data via http get requests
            
            LocalDisk disk = new LocalDisk();
            disk.SetPath("data");

            {
                Console.Write("\n");
                Console.WriteLine("Uploading Files via HTTP requests");

                var HttpRequest = new HttpRequestToSql();
                HttpRequest.DeleteTable();

                var diskDirectories = disk.ListDirectories("");

                int counter = -1;
                foreach (var folder in diskDirectories)
                {
                    counter++;                    
                    HttpRequest.UploadFolder(disk, folder);
                    ProgressBar.DrawProgressBar(counter + 1, diskDirectories.Count());
                }
            }*/

        }
    }
}
