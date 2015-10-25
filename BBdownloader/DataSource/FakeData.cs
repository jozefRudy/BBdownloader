﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BBdownloader.DataSource
{
    public class FakeData: IDataSource
    {
        static Random rnd = new Random();

        public string DefaultField { get; set; }
        public bool Connect(string connectionString)
        {
            return true;
        }

        public void DownloadData(string securityName, string inputField, DateTime startDate, DateTime endDate, out SortedList<DateTime, dynamic> outList)
        {
            outList = new SortedList<DateTime, dynamic>();
            for (int i = 0; i < 20; i++)
            {
                outList.Add(DateTime.Today.AddDays(-20 + i), rnd.Next(-10000, 10000));
            }    
        }


    }
}
