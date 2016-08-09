using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using BBdownloader.Shares;
using BBdownloader.GoogleDocs;
using BBdownloader.DataSource;
using BBdownloader.FileSystem;
using BBdownloader.Extension_Methods;
using BBdownloader.Settings;
using CommandLine;
using System.Globalization;
using System.Threading;

namespace BBdownloader
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            Stopwatch stopwatch = new Stopwatch();

            DateTime startDate;
            var endDate = DateTime.Today.GetBusinessDay(-1);

            var options = new CommandLineOptions();

            if (!Parser.Default.ParseArguments(args, options))
                Environment.Exit(Parser.DefaultExitCodeFail);

            DateTime.TryParse(String.Join(".", options.startDate), out startDate);

            var logging = new Logging(options.LogFile);
            Trace.WriteLine(options.ToString());

            var config = new ConfigBase();
            config.Load(options.Settings);

            if (!options.NoDownload)
            {
                IDataSource dataSource = new Bloomberg();
                dataSource.Connect();

                HashSet<string> shareNames = new HashSet<string>();

                // get specifications
                var sheet = new Sheet();
                sheet.Download(new string[] { config.GetValue("sheetCode"), config.GetValue("shareNames") });
                var shareIDs = sheet.toShares();
                shareNames.UnionWith(
                    dataSource.GetTickers(shareIDs.ToList()).StripOfIllegalCharacters()
                    );

                sheet.Download(new string[] { config.GetValue("sheetCode"), config.GetValue("indices") });
                var indexNames = sheet.toShares();

                sheet.Download(new string[] { config.GetValue("sheetCode"), config.GetValue("fields") });
                var fields = new List<Field>();
                sheet.toFields<Field>(fields);            

                //download index compositions  
                if (indexNames != null && indexNames.Count() > 0)
                {
                    //obtain components of indices
                    var names = dataSource.DownloadMultipleComponents(indexNames.ToList(), "INDX_MEMBERS");

                    //convert tickers -> BB IDs
                    shareNames.Union(dataSource.GetTickers(names));
                }

                LocalDisk disk = new LocalDisk();
                disk.SetPath(options.Dir);

                //delete data for shares-reload and shares-delete
                {
                    sheet.Download(new string[] { config.GetValue("sheetCode"), config.GetValue("shares-reload") });
                    var sharesReload = dataSource.GetTickers(sheet.toShares().ToList()).StripOfIllegalCharacters();


                    sheet.Download(new string[] { config.GetValue("sheetCode"), config.GetValue("shares-delete") });
                    var sharesDelete = dataSource.GetTickers(sheet.toShares().ToList()).StripOfIllegalCharacters();

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
                stopwatch.Start();
                {
                    var shares = new SharesBatch(shareNames.ToList(), fields, dataSource, disk, startDate, endDate);
                    shares.PerformOperations();

                    Trace.Write("Processing Individual: ");
                    foreach (var shareName in shareNames)
                    {
                        Share share = new Share(name: shareName, fields: fields, dataSource: dataSource, fileAccess: disk, startDate: startDate, endDate: endDate);
                        share.DoWork();
                    }
                }
                dataSource.Disconnect();

                //download fieldInfo
                {
                    if (shareNames.Count() > 0)
                    {
                        dataSource.Connect(dataType: "//blp/apiflds");
                        disk.SetPath(options.FieldInfoDir);
                        Share share = new Share(name: shareNames.First(), fields: fields, dataSource: dataSource, fileAccess: disk, startDate: startDate, endDate: endDate);
                        share.DoWorkFieldInfo();
                        dataSource.Disconnect();
                    }
                }
                stopwatch.Stop();
                Trace.WriteLine("Time spent downloading from BB: " + stopwatch.Elapsed.ToString());
            }

            //upload data via SQL connection
            if (!options.NoUpload)
            {
                stopwatch.Restart();
                {
                    LocalDisk disk = new LocalDisk();
                    disk.SetPath(options.Dir);

                    var database = new MySQL(config.GetValue("sqlIP"), config.GetValue("sqlUser"), config.GetValue("sqlPass"), config.GetValue("sqlDB"), disk);
                    database.DoWork();
                }

                {
                    LocalDisk disk = new LocalDisk();
                    disk.SetPath(options.FieldInfoDir);

                    var database = new MySQL(config.GetValue("sqlIP"), config.GetValue("sqlUser"), config.GetValue("sqlPass"), config.GetValue("sqlDB"), disk);
                    database.DoWorkFieldInfo();
                }
                stopwatch.Stop();
                Trace.WriteLine("Time spent uploading: " + stopwatch.Elapsed.ToString());
                logging.Close();

                {
                    Console.WriteLine("Executing long job, you can force exit program, it will continue executing on server");
                    LocalDisk disk = new LocalDisk();
                    disk.SetPath(options.Dir);
                    var database = new MySQL(config.GetValue("sqlIP"), config.GetValue("sqlUser"), config.GetValue("sqlPass"), config.GetValue("sqlDB"), disk);
                    database.executeScript();
                }
            }
        }
    }
}
