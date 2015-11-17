#http://dbahire.com/testing-the-fastest-way-to-import-a-table-into-mysql-and-some-interesting-5-7-performance-results/

import MySQLdb
db = MySQLdb.connect("localhost","root","sirius","bloomb")
cursor = db.cursor()
cursor.execute("TRUNCATE TABLE global_bbd")

sql = "INSERT INTO `global_bbd` (`bbd_unique`, `attribute_name`, `value_date`, `value`, `value_typ`) VALUES (%s,%s,%s,%s,%s);"
cursor execute(sql)


$insert_sql = "INSERT INTO `global_bbd` (`bbd_unique`, `attribute_name`, `value_date`, `value`, `value_typ`) "
        . "VALUES ('$bbd_unique', '$attribute_name', '$value_date', '$value', '$value_typ');";