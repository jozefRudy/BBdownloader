#http://dbahire.com/testing-the-fastest-way-to-import-a-table-into-mysql-and-some-interesting-5-7-performance-results/

import MySQLdb
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

sql = """LOAD DATA INFILE '/var/www/screener/BBdownloader1/{id}/{field}.csv' INTO TABLE global_bbd
FIELDS TERMINATED BY ','
(value_date,value,value_typ)
SET attribute_name='{id}', bbd_unique='{field}';""".format(id='BBG000B9XRY4',field='BEST_CURRENT_EV_BEST_SALES_1BF')

cursor.execute(sql)

sql = """LOAD DATA INFILE '/var/www/screener/BBdownloader1/BBG000B9XRY4/BEST_CURRENT_EV_BEST_SALES_2BF.csv' INTO TABLE global_bbd
FIELDS TERMINATED BY ','
(value_date,value,value_typ)
SET attribute_name='BEST_CURRENT_EV_BEST_SALES_2BF', bbd_unique='BBG000B9XRY4';"""

cursor.execute(sql)