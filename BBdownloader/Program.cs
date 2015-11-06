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
            DateTime endDate = DateTime.Today.AddDays(-1);


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
            if (dataSource.Connect(""))
                Console.WriteLine("Connection to Bloomberg Established Succesfully");
            else
            { 
                Console.WriteLine("Connection Failed");
                Console.ReadKey();
                Environment.Exit(0);
            }

            
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
            }

            //download data
            {
                LocalDisk disk = new LocalDisk();
                disk.SetPath("data");

                //Ftp disk = new Ftp(config.GetValue("ftpIP"), config.GetValue("ftpLogin"), config.GetValue("ftpPass"));
                //disk.SetPath("BBdownloader");
                            
                var shares = new SharesBatch(shareNames, fields, dataSource, disk, startDate, endDate);
                shares.PerformOperations();
            
                foreach (var shareName in shareNames)
                {
                    Share share = new Share(name: shareName, fields: fields, dataSource: dataSource, fileAccess: disk);
                    share.PerformOperations();
                }
            }

            //implement bloomberg handling - if fails to connect - retry 3x and exit - readline

            //zip directories and files
            //delete old zip file
            //upload zipped file
            //show percentage counter of uploading big file

            /*
            sheet.Add(new string[] { "19hRk5zO3GeJSsgh3v2anYibAYpkEGIs7xIrY3aEJZqw", "1607987342" }); //indices
            sheet.Add(new string[] { "19hRk5zO3GeJSsgh3v2anYibAYpkEGIs7xIrY3aEJZqw", "794076055" }); //fields
            sheet.Add(new string[] { "19hRk5zO3GeJSsgh3v2anYibAYpkEGIs7xIrY3aEJZqw", "485268174" }); //shares reload
            sheet.Add(new string[] { "19hRk5zO3GeJSsgh3v2anYibAYpkEGIs7xIrY3aEJZqw", "485268174" }); //shares reload
            sheet.Add(new string[] { "19hRk5zO3GeJSsgh3v2anYibAYpkEGIs7xIrY3aEJZqw", "1767144829" }); //shares delete*/

        }
    }
}
