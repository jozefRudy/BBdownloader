#http://dbahire.com/testing-the-fastest-way-to-import-a-table-into-mysql-and-some-interesting-5-7-performance-results/

import MySQLdb
import os

path = '/var/www/screener/BBdownloader1'
#path = 'g:/projects/BBdownloader/BBdownloader/bin/Debug/data/'

db = MySQLdb.connect("localhost","root","sirius","bloomb")
cursor = db.cursor()

sql = """CREATE TABLE IF NOT EXISTS `global_bbd` (
  `globalbbdid` int(11) AUTO_INCREMENT PRIMARY KEY,
  `bbd_unique` varchar(50) NOT NULL COLLATE latin1_bin DEFAULT '0',
  `attribute_name` varchar(550) NOT NULL COLLATE latin1_bin DEFAULT '0',
  `value_date` varchar(50) COLLATE latin1_bin DEFAULT NULL,
  `value` varchar(550) NOT NULL COLLATE latin1_bin DEFAULT '0',
  `value_typ` varchar(50) NOT NULL COLLATE latin1_bin DEFAULT '0'
) ENGINE=MyISAM DEFAULT CHARSET=latin1 COLLATE=latin1_bin;"""

cursor.execute(sql)

cursor.execute("TRUNCATE TABLE global_bbd")

dirs = os.listdir(path)

for d in dirs:
    if os.path.isfile(os.path.join(path,d)):
        continue
    files = os.listdir(os.path.join(path,d))

    id = d
    for f in files:
        field = f.split('.')[0]

        sql = """LOAD DATA INFILE '/var/www/screener/BBdownloader1/{id}/{field}.csv' INTO TABLE global_bbd
FIELDS TERMINATED BY ','
(value_date,value,value_typ)
SET attribute_name='{id}', bbd_unique='{field}';""".format(id=id,field=field)
        cursor.execute(sql)