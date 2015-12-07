using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace BBdownloader.Settings
{
    public class Logging
    {
        private StreamWriter logFile;

        public Logging(string _logFile)
        {
            Trace.Listeners.Add(new ConsoleTraceListener());

            try
            {
                //if (File.Exists(_logFile))
                    this.logFile = File.AppendText(_logFile);
                //else
                //    this.logFile = File.CreateText(_logFile);
                Trace.Listeners.Add(new TextWriterTraceListener(this.logFile));                
            }
            catch 
            { 
                Trace.WriteLine("No Logging possible - file locked"); 
            }        
            Trace.AutoFlush = true;
            Trace.WriteLine("\n"+DateTime.Now.ToString());
        }

        public void Finalize()
        {            
            if (this.logFile != null)
                this.logFile.Dispose();
            Trace.Close();
            Trace.Listeners.Clear();
        }
    }
}