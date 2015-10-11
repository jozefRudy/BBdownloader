using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace BBdownloader
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            {
                Stock stock = new Stock("AAPL");
                stock.CreateDirectory();
                stock.WriteDates("PUBLICATION DATE", 10);
                stock.WriteFloats("EARN", 10);
                stock.WriteField("INDUSTRY", DateTime.Today.AddDays(-20), "Consumer discretionary");
                stock.WriteField("LONG NAME", DateTime.Today.AddDays(-20), "Apple corporation");
            }

            {
                Stock stock = new Stock("MSFT");
                stock.CreateDirectory();
                stock.WriteDates("PUBLICATION DATE", 10);
                stock.WriteFloats("EARN", 10);
                stock.WriteField("INDUSTRY", DateTime.Today.AddDays(-20), "IT");
                stock.WriteField("LONG NAME", DateTime.Today.AddDays(-20), "Microsoft");
            }

            {
                Stock stock = new Stock("TSL");
                stock.CreateDirectory();
                stock.WriteDates("PUBLICATION DATE", 10);
                stock.WriteFloats("EARN", 10);
                stock.WriteField("INDUSTRY", DateTime.Today.AddDays(-20), "Automotive");
                stock.WriteField("LONG NAME", DateTime.Today.AddDays(-20), "Tesla");
            }

        }
    }
}
