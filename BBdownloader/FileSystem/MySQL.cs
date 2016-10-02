using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using MySql.Data.MySqlClient;
using System.Threading.Tasks;


namespace BBdownloader.FileSystem
{
    class MySQL
    {
        string myConnectionString;
        string path;
        IFileSystem disk;
               
        public MySQL(string ip, string user, string password, string database, IFileSystem disk)
        {
            myConnectionString = "server="+ip+";uid="+user +";pwd="+ password +";database="+database+ ";useCompression=true;ConnectionTimeout=28880;DefaultCommandTimeout=28880;";
            
            this.path = disk.GetFullPath().Replace("\\","/");
            this.disk = disk;
        }

        private void createTable()
        {           
            string text = @"CREATE TABLE IF NOT EXISTS `global_bbd` (
`globalbbdid` int(11) AUTO_INCREMENT PRIMARY KEY,
`bbd_unique` varchar(50) NOT NULL COLLATE latin1_bin DEFAULT '0',
`attribute_name` varchar(550) NOT NULL COLLATE latin1_bin DEFAULT '0',
`value_date` varchar(50) COLLATE latin1_bin DEFAULT NULL,
`value` varchar(550) NOT NULL COLLATE latin1_bin DEFAULT '0',
`value_typ` varchar(50) NOT NULL COLLATE latin1_bin DEFAULT '0',
`titul_id` int(11) DEFAULT NULL
) ENGINE=MyISAM DEFAULT CHARSET=latin1 COLLATE=latin1_bin;";

            ExecuteQuery(text);

            text = "TRUNCATE TABLE global_bbd;";
            ExecuteQuery(text);
        }

        private void createTableFieldInfo()
        {
            string text = @"CREATE TABLE IF NOT EXISTS `field_info` (
`attribute_name` varchar(550) NOT NULL COLLATE latin1_bin PRIMARY KEY,
`value` varchar(5000) NOT NULL COLLATE latin1_bin DEFAULT '0'
) ENGINE=InnoDB DEFAULT CHARSET=latin1 COLLATE=latin1_bin;";

            ExecuteQuery(text);

            text = "TRUNCATE TABLE field_info;";
            ExecuteQuery(text);
        }

        public void executeScript()
        {
            string command = @"TRUNCATE TABLE titul_bbd;
TRUNCATE TABLE attributes_bbd; 
INSERT INTO titul_bbd(Titul)
SELECT DISTINCT bbd_unique FROM global_bbd;INSERT INTO attributes_bbd(attributename)
SELECT DISTINCT attribute_name FROM global_bbd;
UPDATE global_bbd b
JOIN
(
SELECT  a.globalbbdid, b.titulID
FROM global_bbd a JOIN titul_bbd b ON bbd_unique = Titul
) a ON a.globalbbdid = b.globalbbdid
SET b.titul_id = a.titulID;
UPDATE global_bbd b
JOIN
(
SELECT a.globalbbdid, b.attributeid
FROM global_bbd a JOIN attributes_bbd b ON b.attributename = a.attribute_name
) a ON a.globalbbdid = b.globalbbdid
SET b.attributeid = a.titulID;";

            ExecuteQuery(command);
        }

        private void uploadFields()
        {
            Trace.WriteLine("Uploading field definitions via compressed SQL connection ...");
            var ids = disk.ListFiles("");
            
            foreach (var field in ids)
            {
                string text = String.Format($"LOAD DATA LOCAL INFILE '{this.path}/{field.Split('.')[0]}.csv' INTO TABLE field_info LINES TERMINATED BY 'IMPOSSIBLE' (value) SET attribute_name='{field.Split('.')[0]}' ");
                ExecuteQuery(text);
            }
            Trace.WriteLine("\nUpload successful");
        }

        private void insertData(string id, string field, string ticker)
        {
            string text = String.Format($"LOAD DATA LOCAL INFILE '{this.path}/{id}/{field}.csv' INTO TABLE global_bbd FIELDS TERMINATED BY ',' (value_date,value,value_typ) SET attribute_name='{field}', bbd_unique='{ticker}';");

            ExecuteQuery(text);
        }

        private void traverseDirs()
        {
            var ids = disk.ListDirectories("");

            Trace.WriteLine("Uploading files via compressed SQL connection");

            int counter = -1;            
            foreach (var id in ids)
            {
                ProgressBar.DrawProgressBar(counter + 1, ids.Count());
                counter++;

                var fields = disk.ListFiles(id);
                
                try
                {
                    var text = File.ReadAllText(Path.Combine(this.path, id, "TICKER.csv"));
                    var ticker = text.Split(',')[1];
                    foreach (var field in fields)
                    {
                        insertData(id, field.Split('.')[0], ticker);
                    }
                }
                catch
                {
                    Trace.WriteLine("Cannot find TICKER.csv for share ", id);
                }


            }
            ProgressBar.DrawProgressBar(1, 1);
            Trace.WriteLine("\nUpload successful");
        }

        public void DoWork()
        {
            createTable();            
            traverseDirs();            
        }        

        public void DoWorkFieldInfo()
        {
            createTableFieldInfo();
            uploadFields();
        }

        public void ExecuteQuery(string query)
        {
            try
            {
                using (var conn = new MySqlConnection(myConnectionString))
                {
                    conn.Open();
                    var cmd = new MySqlCommand(query, conn);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (MySqlException ex)
            {
                Trace.WriteLine(ex.Message);
            }

        }

    }
}
