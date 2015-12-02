using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.IO;

namespace BBdownloader.FileSystem
{
    class MySQL
    {
        MySqlConnection conn;
        string myConnectionString;
        string path;
        IFileSystem disk;
               
        public MySQL(string ip, string user, string password, string database, string path, IFileSystem disk)
        {
            myConnectionString = "server="+ip+";uid="+user +";pwd="+ password +";database="+database+";useCompression=true";
            
            this.path = disk.GetFullPath().Replace("\\","/");
            this.disk = disk;

            try
            {
                conn = new MySqlConnection(myConnectionString);
                conn.Open();
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void createTable()
        {           
            string text = @"CREATE TABLE IF NOT EXISTS `global_bbd` (
`globalbbdid` int(11) AUTO_INCREMENT PRIMARY KEY,
`bbd_unique` varchar(50) NOT NULL COLLATE latin1_bin DEFAULT '0',
`attribute_name` varchar(550) NOT NULL COLLATE latin1_bin DEFAULT '0',
`value_date` varchar(50) COLLATE latin1_bin DEFAULT NULL,
`value` varchar(550) NOT NULL COLLATE latin1_bin DEFAULT '0',
`value_typ` varchar(50) NOT NULL COLLATE latin1_bin DEFAULT '0'
) ENGINE=MyISAM DEFAULT CHARSET=latin1 COLLATE=latin1_bin;";
            
            MySqlCommand cmd = new MySqlCommand(text, conn);
            cmd.ExecuteNonQuery();

            text = "TRUNCATE TABLE global_bbd;";
            MySqlCommand cmd1 = new MySqlCommand(text, conn);
            cmd1.ExecuteNonQuery();
        }

        private void insertData(string id, string field)
        {
            string text = String.Format(@"LOAD DATA LOCAL INFILE '{0}/{1}/{2}.csv' INTO TABLE global_bbd FIELDS TERMINATED BY ',' (value_date,value,value_typ) SET attribute_name='{1}', bbd_unique='{2}';", this.path,id, field);
            var cmd = new MySqlCommand(text, conn);
            cmd.ExecuteNonQuery();
        }

        private void traverseDirs()
        {
            var ids = disk.ListDirectories("");

            Console.WriteLine("Uploading files via compressed SQL connection");

            int counter = -1;            
            foreach (var id in ids)
            {
                ProgressBar.DrawProgressBar(counter + 1, ids.Count());
                counter++;

                var fields = disk.ListFiles(id);
                foreach (var field in fields)
                {
                    insertData(id, field.Split('.')[0]);
                }
                
            }
            ProgressBar.DrawProgressBar(1, 1);
            Console.WriteLine("Upload successful");

            //insertData("BBG000B9XRY4", "BEST_CURRENT_EV_BEST_SALES_2BF");
        }

        public void DoWork()
        {
            createTable();
            traverseDirs();            
        }

    }
}
