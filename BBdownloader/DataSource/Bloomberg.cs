﻿using System;
using System.Collections.Generic;


using System.Globalization;

using Event = Bloomberglp.Blpapi.Event;
using Message = Bloomberglp.Blpapi.Message;
using Name = Bloomberglp.Blpapi.Name;
using Request = Bloomberglp.Blpapi.Request;
using Service = Bloomberglp.Blpapi.Service;
using Session = Bloomberglp.Blpapi.Session;
using SessionOptions = Bloomberglp.Blpapi.SessionOptions;
using Element = Bloomberglp.Blpapi.Element;

//http://mikejuniperhill.blogspot.sk/2015/03/bloomberg-api-wrapper-in-c.html

namespace BBdownloader.DataSource { 
    public class Bloomberg: IDataSource
    {
        public string DefaultField {get;set; }

        public static Service refDataService;
        public static Session session;
        public static bool connected;

        public Bloomberg()
        {
            DefaultField = "PX_LAST";
        }

        public bool Connect(string user = "")
        {
            if (connected)
                goto finish;

            string serverHost = "localhost";
            int serverPort = 8194;

            SessionOptions sessionOptions = new SessionOptions();
            sessionOptions.ServerHost = serverHost;
            sessionOptions.ServerPort = serverPort;

            Console.WriteLine("Connecting to " + serverHost + ":" + serverPort);
            session = new Session(sessionOptions);

            bool sessionStarted = session.Start();

            if (!sessionStarted)
            {
                Console.Error.WriteLine("Failed to start session.");
                return false;
            }
            if (!session.OpenService("//blp/refdata"))
            {
                Console.Error.WriteLine("Failed to open //blp/refdata");
                return false;
            }

            refDataService = session.GetService("//blp/refdata");
            
            Console.WriteLine("Connected to Bloomberg");
            connected = true;

            finish:
            return connected;
        }

        public void DownloadData(string securityName, string inputField, List<string[]> overrides, DateTime startDate, DateTime endDate, out SortedList<DateTime, dynamic> outList)
        {
            Request request = refDataService.CreateRequest("HistoricalDataRequest");
            Element securities = request.GetElement("securities");
            securities.AppendValue(securityName);

            Element fields = request.GetElement("fields");

            fields.AppendValue(inputField);

            Element listOverrides = request.GetElement("overrides");

            foreach (var item in overrides)
            {
                listOverrides.SetElement(item[0],item[1]);
            }

            request.Set("periodicityAdjustment", "ACTUAL");
            request.Set("periodicitySelection", "DAILY");

            request.Set("startDate", startDate.ToString("yyyyMMdd"));

            var nextYear = new DateTime(DateTime.Today.Year + 1, 1, 1);
            var upperLimit = endDate > nextYear ? nextYear : endDate;

            request.Set("endDate", upperLimit.ToString("yyyyMMdd"));
            request.Set("maxDataPoints", 10000);
           
            //request.Set("returnEids", true);

            //Console.WriteLine("Sending Request: " + request);
            Console.WriteLine("Downloading: " + securityName + ", Field: " + inputField);
            session.SendRequest(request, null);

            bool done = false;

            outList = new SortedList<DateTime, dynamic>();

            while (!done)
            {
                Event eventObj = session.NextEvent();

                if (eventObj.Type == Event.EventType.RESPONSE || eventObj.Type == Event.EventType.PARTIAL_RESPONSE)
                {
                    foreach (var msg in eventObj)
                    {                    
                        if (msg.AsElement.HasElement("responseError"))
                            throw new Exception("Response error: " + msg.GetElement("responseError").GetElement("message"));
                    
                        var security = msg.GetElement("securityData");
                        //var securityName = security.GetElementAsString("security");
                        var fieldData = security.GetElement("fieldData");
                    
                        for (int i = 0; i < fieldData.NumValues; i++)
                        {
                            var data = fieldData.GetValueAsElement(i);

                            dynamic _dict = null;
                            DateTime date = new DateTime();

                            for (int j = 0; j < data.NumElements; j++)
                            {
                                var point = data.GetElement(j);
                                dynamic elementValue = null;                            
                                bool ok = false;

                                string dataType = point.Datatype.ToString();
                                if (point.Name.ToString() == "ECO_RELEASE_DT")
                                { dataType = "STRINGDATE"; }

                                switch (dataType)
	                            {
                                    case "DATE":
                                        {
                                        DateTime _elementValue;
                                        ok = DateTime.TryParse(point.GetValue().ToString(), out _elementValue);
                                        elementValue = _elementValue;        
                                        break;
                                        }
                                    case "FLOAT64":
                                        {
                                        float _elementValue;
                                        ok = float.TryParse(point.GetValue().ToString(), out _elementValue);
                                        elementValue = _elementValue;
                                        break;
                                        }
                                    case "STRINGDATE":
                                        { 
                                        DateTime _elementValue;
                                        ok = DateTime.TryParseExact(point.GetValue().ToString(), "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None,out _elementValue);
                                        elementValue = _elementValue;
                                        break;
                                        }
		                            default:
                                        break;
	                            }
                                if (!ok)
                                { elementValue = null; }
                                                        
                                if (point.Name.ToString() != "date")
                                { _dict = elementValue; }
                                else date = elementValue;
                            }

                            outList.Add(date, _dict);

                        }
                    }
                    //   Once we have a response we are done
                    if (eventObj.Type == Event.EventType.RESPONSE) done = true;
                }
            }
        }

    }
}