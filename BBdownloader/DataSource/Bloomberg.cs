using System;
using System.Collections.Generic;
using BBdownloader.Shares;
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

        public void DownloadComponents(string Index, string bbgField, out List<string> members)
        {
            members = new List<string>();
            Request request = refDataService.CreateRequest("ReferenceDataRequest");
            Element securities = request.GetElement("securities");
            securities.AppendValue(Index);
            Element fields = request.GetElement("fields");
            fields.AppendValue(bbgField);

            System.Console.WriteLine("Sending Index Components Request: " + Index);
            session.SendRequest(request, null);

            bool done = false;

            while (!done)
            {
                Event eventObj = session.NextEvent();

                if (eventObj.Type == Event.EventType.RESPONSE || eventObj.Type == Event.EventType.PARTIAL_RESPONSE)
                {
                    foreach (var msg in eventObj)
                    {
                        if (msg.AsElement.HasElement("responseError"))
                            throw new Exception("Response error: " + msg.GetElement("responseError").GetElement("message"));

                        var security = msg.GetElement("securityData").GetValueAsElement();
                        var field = security.GetElement("fieldData").GetElement(bbgField);
                        
                        for (int i = 0; i < field.NumValues; i++)
                        {
                            string data = "";
                            if (field.NumValues > 1)
                                data = field.GetValueAsElement(i).GetElement(0).GetValueAsString();
                            else
                                data = field.GetValueAsString();
                            members.Add(data + " Equity");
                        }
                    }
                    if (eventObj.Type == Event.EventType.RESPONSE) done = true;
                }
            }
        }


        public bool DownloadData(List<string> securityNames, List<IField> fields, DateTime? startDate, DateTime? endDate)
        {
            Request request = refDataService.CreateRequest(fields[0].requestType);
            
            foreach (var name in securityNames)
                request.Append("securities", name);

            foreach (var f in fields)
                request.Append("fields", f.FieldName);

            foreach (var item in fields[0].Overrides)
            {
                if (item[0].Length > 0 && item[1].Length > 0)
                    request.Set(item[0], item[1]);
            }

            if (fields[0].requestType == "HistoricalDataRequest")
            {
                request.Set("periodicityAdjustment", "ACTUAL");
                request.Set("periodicitySelection", "DAILY");

                var d = startDate.Value;

                if (startDate != null)
                    request.Set("startDate", startDate.Value.ToString("yyyyMMdd"));

                if (endDate != null)
                {
                    var nextYear = new DateTime(DateTime.Today.Year + 1, 1, 1);
                    DateTime upperLimit = endDate.Value > nextYear ? nextYear : endDate.Value;

                    request.Set("endDate", upperLimit.ToString("yyyyMMdd"));
                }

                request.Set("maxDataPoints", 10000);
            }
            session.SendRequest(request, null);

            ParseData(securityNames, fields);

            return true;
        }

        private bool ParseData(List<string> securityNames, List<IField> fields)
        {
            bool done = false;

            while (!done)
            {

                Event eventObj = session.NextEvent();

                if (eventObj.Type == Event.EventType.RESPONSE || eventObj.Type == Event.EventType.PARTIAL_RESPONSE)
                {
                    foreach (var msg in eventObj)
                    {
                        if (msg.AsElement.HasElement("responseError"))
                            throw new Exception("Response error: " + msg.GetElement("responseError").GetElement("message"));

                        Element securityData = msg.GetElement("securityData");

                        ParseUniversal(securityData);
                    }
                }

                if (eventObj.Type == Event.EventType.RESPONSE) done = true;

            }

            return true;
        }


        private SortedList<DateTime, dynamic> ParseUniversal(Element securityDataArray)
        {
            SortedList<DateTime, dynamic> output;

            if (securityDataArray.IsArray)
            {
                for (int i = 0; i < securityDataArray.NumValues; i++)
                {
                    Element securityData = securityDataArray.GetValueAsElement(i);
                    Element fieldData = securityData.GetElement("fieldData");

                    for (int j = 0; j < fieldData.NumElements; j++)
                    {
                        Element field = fieldData.GetElement(j);
                        
                        var data = field.GetValue();
                        var dataType = field.Datatype.ToString();
                        output = new SortedList<DateTime, dynamic>();
                        output.Add(DateTime.Now, data);
                                                           
                        return output;
                    }

                }
            }
            else
            {
                Element fieldDataArray = securityDataArray.GetElement("fieldData");

                for (int i = 0; i < fieldDataArray.NumValues; i++)
                {
                    Element fieldData = fieldDataArray.GetValueAsElement(i);

                    for (int j = 0; j < fieldData.NumElements; j++)
                    {
                        Element field = fieldData.GetElement(j);                       
                    }
                    return null;
                }
            }
            return null;
        }




        public void DownloadData(string securityName, IField field, DateTime? startDate, DateTime? endDate, out SortedList<DateTime, dynamic> outList)
        {
            Request request = refDataService.CreateRequest(field.requestType);
            Element securities = request.GetElement("securities");
            securities.AppendValue(securityName);

            Element fields = request.GetElement("fields");

            fields.AppendValue(field.FieldName);

            
            Element listOverrides = request.GetElement("overrides");

            foreach (var item in field.Overrides)
            {
                if (item[0].Length>0 && item[1].Length>0)
                    listOverrides.SetElement(item[0],item[1]);
            }

            if (field.requestType != "ReferenceDataRequest")
            {
                request.Set("periodicityAdjustment", "ACTUAL");
                request.Set("periodicitySelection", "DAILY");

                var d = startDate.Value;

                if (startDate != null)
                    request.Set("startDate", startDate.Value.ToString("yyyyMMdd"));

                if (endDate != null)
                {
                    var nextYear = new DateTime(DateTime.Today.Year + 1, 1, 1);
                    DateTime upperLimit = endDate.Value > nextYear ? nextYear : endDate.Value;

                    request.Set("endDate", upperLimit.ToString("yyyyMMdd"));
                }

                request.Set("maxDataPoints", 10000);
            }
           
            //request.Set("returnEids", true);

            //Console.WriteLine("Sending Request: " + request);
            Console.WriteLine("Downloading: " + securityName + ", Field: " + field.FieldName);
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

                        //msg.GetElement("securityData").GetValueAsElement().GetElement("fieldData").GetElement(field.FieldName).Name;
                    
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

                                string dataType = "";

                                if (field.Type!= null && field.Type.Length > 0)
                                    dataType = field.Type;
                                else
                                    dataType = point.Datatype.ToString();                                

                                switch (dataType)
	                            {
                                    case "DATE":
                                        {
                                            DateTime _elementValue;
                                            if (DateTime.TryParse(point.GetValue().ToString(), out _elementValue) || 
                                                DateTime.TryParseExact(point.GetValue().ToString(), "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None,out _elementValue))
                                            {
                                                ok = true;
                                                elementValue = _elementValue;
                                            }                                            
                                        break;
                                        }
                                    case "FLOAT64":
                                        {
                                        float _elementValue;
                                        ok = float.TryParse(point.GetValue().ToString(), out _elementValue);
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