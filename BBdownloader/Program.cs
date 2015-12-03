using System;
using System.Collections.Generic;
using System.Linq;
using BBdownloader.Shares;
using BBdownloader.GoogleDocs;
using BBdownloader.DataSource;
using BBdownloader.FileSystem;
using BBdownloader.Extension_Methods;
using CommandLine;

namespace BBdownloader
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var options = new CommandLineOptions();


            if (!Parser.Default.ParseArguments(args, options))
            {
                Environment.Exit(Parser.DefaultExitCodeFail);
            }                      

            var startDate = new DateTime(1990, 1, 1);
            var endDate = DateTime.Today.GetBusinessDay(-1);
            
            var config = new ConfigBase();
            config.Load(options.Settings);
                                           
            if (!options.NoDownload)
            {
                // get specifications
                var gdocsSheet = config.GetValue("sheetCode");
                var sheet = new Sheet();
                sheet.Download(new string[] { gdocsSheet, config.GetValue("shareNames") });
                var shareNames = sheet.toShares();

                sheet.Download(new string[] { gdocsSheet, config.GetValue("fields") });
                var fields = new List<Field>();
                sheet.toFields<Field>(fields);

                sheet.Download(new string[] { gdocsSheet, config.GetValue("indices") });
                var indexNames = sheet.toShares();

                IDataSource dataSource = new Bloomberg();
                dataSource.Connect("");

                //download index compositions  
                if (indexNames != null && indexNames.Count() > 0)
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
                    var bbIDs = dataSource.DownloadData(names, new List<IField> { new Field() { FieldName = "ID_BB_GLOBAL", requestType = "ReferenceDataRequest" } });
                    var listIDs = from ids in bbIDs.RemoveDates()
                                  select (string)ids;
                    shareNames.AddRange(listIDs.ToList());
                }


                LocalDisk disk = new LocalDisk();
                disk.SetPath(options.Dir);

                //delete data for shares-reload and shares-delete
                {
                    sheet.Download(new string[] { gdocsSheet, config.GetValue("shares-reload") });
                    var sharesReload = sheet.toShares();

                    sheet.Download(new string[] { gdocsSheet, config.GetValue("shares-delete") });
                    var sharesDelete = sheet.toShares();

                    foreach (var item in sharesDelete.Concat(sharesReload))
                        disk.DeleteDirectory(item);

                    //delete shares-delete names from list of downloadable shares
                    foreach (var item in sharesDelete)
                    {
                        if (shareNames.Contains(item))
                            shareNames.Remove(item);
                    }

                }

                //download and save data
                {
                    var shares = new SharesBatch(shareNames, fields, dataSource, disk, startDate, endDate);
                    shares.PerformOperations();

                    Console.Write("Processing Individual: ");
                    foreach (var shareName in shareNames)
                    {
                        Share share = new Share(name: shareName, fields: fields, dataSource: dataSource, fileAccess: disk, startDate: startDate, endDate: endDate);
                        share.DoWork();
                    }
                }
            }



            //upload data via SQL connection
            if (!options.NoUpload)
            {
                LocalDisk disk = new LocalDisk();
                disk.SetPath(options.Dir);

                var database = new MySQL(config.GetValue("sqlIP"), config.GetValue("sqlUser"), config.GetValue("sqlPass"), config.GetValue("sqlDB"), options.Dir, disk);
                database.DoWork();
            }

            /*
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
