﻿using System;
using BBdownloader.GoogleDocs;
using System.Collections.Generic;
using BBdownloader.Shares;
using System.Linq;
using System.Threading.Tasks;
using BBdownloader.DataSource;
using BBdownloader.FileSystem;

namespace BBdownloader
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            List<string> shareNames = new List<string>();
            var fields = new List<Field>();

            Sheet sheet = new Sheet();
            sheet.Download(new string[] { "19hRk5zO3GeJSsgh3v2anYibAYpkEGIs7xIrY3aEJZqw", "0" });
            shareNames = sheet.toShares();

            sheet.Download(new string[] { "19hRk5zO3GeJSsgh3v2anYibAYpkEGIs7xIrY3aEJZqw", "794076055" });
            sheet.toFields<Field>(fields);

            IDataSource dataSource = new FakeData();
            if (dataSource.Connect(""))
                Console.WriteLine("Connection to Bloomberg Established Succesfully");
            else
            { 
                Console.WriteLine("Connection Failed");
                Console.ReadKey();
                Environment.Exit(0);
            }

            LocalDisk disk = new LocalDisk();
            disk.SetPath("data");

            foreach (var shareName in shareNames)
            {
                Share share = new Share(name: shareName, fields: fields, dataSource: dataSource, fileAccess: disk);
                share.PerformOperations();                
            }

            /*
            sheet.Add(new string[] { "19hRk5zO3GeJSsgh3v2anYibAYpkEGIs7xIrY3aEJZqw", "1607987342" }); //indices
            sheet.Add(new string[] { "19hRk5zO3GeJSsgh3v2anYibAYpkEGIs7xIrY3aEJZqw", "794076055" }); //fields
            sheet.Add(new string[] { "19hRk5zO3GeJSsgh3v2anYibAYpkEGIs7xIrY3aEJZqw", "485268174" }); //shares reload
            sheet.Add(new string[] { "19hRk5zO3GeJSsgh3v2anYibAYpkEGIs7xIrY3aEJZqw", "485268174" }); //shares reload
            sheet.Add(new string[] { "19hRk5zO3GeJSsgh3v2anYibAYpkEGIs7xIrY3aEJZqw", "1767144829" }); //shares delete*/
          
        }
    }
}
