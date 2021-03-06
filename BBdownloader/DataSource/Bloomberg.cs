﻿using System;
using System.Collections.Generic;
using BBdownloader.Shares;
using BBdownloader.Extension_Methods;
using System.Globalization;
using System.Linq;
using System.Diagnostics;
using System.Threading;

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
        public static Service refDataService;
        public static Session session;
        public static bool connected;
        public static int reconnectAttempts;
        public readonly int maxReconnectAttempts = 10;
        public readonly int reconnectionInterval = 60;

        public Bloomberg()
        {
            reconnectAttempts = 0;
        }
        
        public IEnumerable<string> GetFields(List<string> IDs, string fieldName)
        {
            var shareNames = new List<string>();
            if (!IDs.Any())
                return shareNames;

            var bbIDs = this.DownloadData(IDs, new List<IField> { new Field() { FieldName = fieldName, requestType = "ReferenceDataRequest" } });

            var listIDs = from ids in bbIDs.RemoveDates(IDs)
                          select (string)ids;
            
            foreach (var item in listIDs)
                shareNames.Add(item);

            return shareNames;
        }

        public bool Connect(string user = "", string dataType = "//blp/refdata")
        {
            if (connected)
                goto finish;

            while (!connected)
            {
                reconnectAttempts++;

                string serverHost = "localhost";
                int serverPort = 8194;

                SessionOptions sessionOptions = new SessionOptions();
                sessionOptions.ServerHost = serverHost;
                sessionOptions.ServerPort = serverPort;

                Trace.WriteLine("Connecting to Bloomberg " + dataType +  ". Attempt " + reconnectAttempts + "/" + maxReconnectAttempts +".");
                session = new Session(sessionOptions);

                bool sessionStarted = session.Start();

                try
                {                    
                    session.OpenService(dataType);
                    refDataService = session.GetService(dataType);
                    Trace.WriteLine("Connected to Bloomberg");
                    connected = true;
                    reconnectAttempts = 0;
                }
                catch
                {
                    Trace.WriteLine("Failed to connect. ");
                    connected = false;
                    if (reconnectAttempts >= maxReconnectAttempts)
                    {
                        Trace.WriteLine("Tried to connect to Bloomberg " + reconnectAttempts + " times.");
                        Trace.WriteLine("Exiting");
                        Environment.Exit(0);
                    }

                    Trace.WriteLine("Waiting for " + reconnectionInterval + "s before retrying to connect.");
                    Thread.Sleep(reconnectionInterval * 1000);
                    
                }
            }

            finish:
            return connected;
        }

        public List<string> DownloadMultipleComponents(List<string> indices, string bbgField)
        {
            var output = new List<string>();
            foreach (var idx in indices)
            {
                var outList = new List<string>();
                this.DownloadComponents(idx, bbgField, out outList);
                output.AddRange(outList);
            }
            return output;
        }

        public void DownloadComponents(string Index, string bbgField, out List<string> members)
        {
            members = new List<string>();
            Request request = refDataService.CreateRequest("ReferenceDataRequest");
            Element securities = request.GetElement("securities");
            securities.AppendValue(Index);
            Element fields = request.GetElement("fields");
            fields.AppendValue(bbgField);

            Trace.WriteLine("Sending Index Components Request: " + Index);

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

        private dynamic ParseBool(string value)
        {
            bool result;
            if (bool.TryParse(value, out result))
                return result;
            else
                return value;
        }

        private bool IsOverride(string element)
        {
            var Elements = new List<string>() {"periodicitySelection", "periodicityAdjustment", "currency","nonTradingFillOption","nonTradingFillMethod","adjustmentNormal",
            "adjustmentAbnormal", "adjustmentSplit"};

            var elements = (from e in Elements
                            select e.ToLower()).ToList();

            if (elements.Contains(element.ToLower()))
                return false;
            else
                return true;

        }

        public IEnumerable<Tuple<string,SortedList<DateTime, dynamic>>> DownloadData(List<string> securityNames, List<IField> fields, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                Request request = refDataService.CreateRequest(fields[0].requestType);

                foreach (var name in securityNames)
                    request.Append("securities", name);

                foreach (var f in fields)
                    request.Append("fields", f.FieldName);

                Element overrides = request["overrides"];

                foreach (var item in fields[0].Overrides)
                {
                    if (item.Key.Length > 0 && item.Value.Length > 0)
                    {
                        dynamic value = ParseBool(item.Value);

                        if (!IsOverride(item.Key))
                        {
                            request.Set(item.Key, value);
                        }
                        else
                        { 
                            Element override1 = overrides.AppendElement();
                            override1.SetElement("fieldId", item.Key);
                            override1.SetElement("value", value);
                        }
                    }
                }

                if (fields[0].requestType == "HistoricalDataRequest")
                {
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
            }
            catch
            {
                Trace.WriteLine("\nWrong bloomberg request for " + fields[0].FieldNickName);
                yield break;
            }

            bool done = false;

            while (!done)
            {

                Event eventObj = session.NextEvent();

                if (eventObj.Type == Event.EventType.RESPONSE || eventObj.Type == Event.EventType.PARTIAL_RESPONSE)
                {
                    foreach (var msg in eventObj)
                    {
                        if (msg.AsElement.HasElement("responseError"))
                            throw new Exception("Response error for fields ["+ fields.ToExtendedString() + "]: " + msg.GetElement("responseError").GetElement("message") + "field: ");

                        Element securityData = msg.GetElement("securityData");

                        foreach (var item in ParseUniversal(securityData, fields))
                            yield return item;
                        
                    }
                }
                if (eventObj.Type == Event.EventType.RESPONSE) done = true;

            }
        }

        private IEnumerable<Tuple<string,SortedList<DateTime, dynamic>>> ParseUniversal(Element securityDataArray, List<IField> fields)
        {
            Thread.Sleep(50);
            if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.P)
            {
                Console.WriteLine("\nDownload Paused, press c to continue");
                while (!Console.KeyAvailable || Console.ReadKey(true).Key  != ConsoleKey.C)
                {
                    Thread.Sleep(1000 * 1);
                }
            }

            var fieldNames = from f in fields
                             select f.FieldName.ToUpper();

            SortedList<DateTime, dynamic> output;

            if (securityDataArray.IsArray)
            {
                for (int i = 0; i < securityDataArray.NumValues; i++) //data for multiple securities
                {
                    Element securityData = securityDataArray.GetValueAsElement(i); //single security
                    string securityName = securityData.GetElementAsString("security");
                    Element fieldData = securityData.GetElement("fieldData"); //data for multiple fields.

                    int j = -1;
                    foreach (var fieldName in fieldNames)
                    {
                        j++;
                        Element field = fieldData.GetElement(fieldName);
                        
                        output = new SortedList<DateTime, dynamic>();

                        if (field.NumValues>0)
                        { 
                            var data = field.GetValue(); //check field.NumValues - sometimes NumValues>1 - then output into single field - because it is single field but with multiple values

                            string dataType;
                            if (fields[j].Type != null && fields[j].Type.Length > 0)
                                dataType = fields[j].Type;
                            else
                                dataType = field.Datatype.ToString();

                            var result = ConvertDataType(dataType, field);
                            
                            output.Add(ExtensionMethods.GetBusinessDay(DateTime.Today, -1), result.Item2);
                        }
                        yield return Tuple.Create(securityName,output);
                    }
                }
            }
            else
            {
                var Outputs = new SortedList<DateTime, dynamic>[fieldNames.Count()];

                for (int i = 0; i < Outputs.Length; i++)
                    Outputs[i] = new SortedList<DateTime, dynamic>();

                Element fieldDataArray = securityDataArray.GetElement("fieldData");  // data for multiple fields, multiple dates                                             
                string securityName = securityDataArray.GetElementAsString("security");

                for (int i = 0; i < fieldDataArray.NumValues; i++) //equals 0 if no fields or security wrong
                {
                    Element fieldData = fieldDataArray.GetValueAsElement(i);  // data for multiple fields, single date

                    int j = -1;

                    foreach (var fieldName in fieldNames)
                    {
                        j++;

                        Element date = fieldData.GetElement("date");

                        if (!fieldData.HasElement(fieldName))
                            continue;

                        Element field = fieldData.GetElement(fieldName);

                        dynamic elementValue = null;
                        bool ok = false;

                        string dataType;

                        if (fields[j].Type != null && fields[j].Type.Length > 0)
                            dataType = fields[j].Type;
                        else
                            dataType = field.Datatype.ToString();

                        var result = ConvertDataType(dataType, field);
                        ok = result.Item1;
                        elementValue = result.Item2;

                        if (!ok) { elementValue = null; }

                        Outputs[j].Add(date.GetValueAsDatetime().ToSystemDateTime(), elementValue);                        
                    }
                }

                for (int i = 0; i < Outputs.Length; i++)
                    yield return Tuple.Create(securityName, Outputs[i]);                
            }
        }

        private Tuple<bool,dynamic> ConvertDataType(string dataType, Element field)
        {
            bool ok = false;
            dynamic elementValue = null;

            switch (dataType)
            {
                case "DATE":
                    {
                        DateTime _elementValue;
                        if (DateTime.TryParse(field.GetValue().ToString(), out _elementValue) ||
                            DateTime.TryParseExact(field.GetValue().ToString(), "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _elementValue))
                        {
                            ok = true;
                            elementValue = _elementValue;
                        }
                        break;
                    }
                case "FLOAT64":
                case "FLOAT":
                    {
                        float _elementValue;
                        ok = float.TryParse(field.GetValue().ToString(), out _elementValue);
                        elementValue = _elementValue;
                        break;
                    }
                default:
                    elementValue = field.GetValue().ToString();
                    elementValue = ((string)elementValue).Replace(',', ' ');
                    ok = true;
                    break;
            }
            return Tuple.Create<bool, dynamic>(ok, elementValue);
        }

        public void Disconnect()
        {
            Trace.WriteLine("\nDisconnecting from Bloomberg service.");

            try
            {
                session.Stop();
            }
            catch
            {                
            }
            
            connected = false;
        }

        public Dictionary<string,string> DownloadFieldInfo(string securityName, IEnumerable<IField> fields)
        {
            var outDict = new Dictionary<string, string>(); 

            Request request = refDataService.CreateRequest("FieldInfoRequest");

            if (fields == null || fields.Count() == 0)
                return null;
            foreach (var item in fields)
            {
                request.Append("id", item.FieldName);
            }
            
            request.Set("returnFieldDocumentation", true);
            session.SendRequest(request,null);

            bool done = false;
            while (!done)
            {

                Event eventObj = session.NextEvent();

                if (eventObj.Type == Event.EventType.RESPONSE || eventObj.Type == Event.EventType.PARTIAL_RESPONSE)
                {
                    foreach (var msg in eventObj)
                    {
                        if (msg.AsElement.HasElement("responseError"))
                            throw new Exception("Response error for fields [" + fields.ToExtendedString() + "]: " + msg.GetElement("responseError").GetElement("message") + "field: ");

                        Element securityDataArray = msg.GetElement("fieldData");
                        for (int i = 0; i < securityDataArray.NumValues; i++)
                        {
                            Element fieldData = securityDataArray.GetValueAsElement(i);

                            try
                            { 
                                Element fieldInfo = fieldData.GetElement("fieldInfo");

                                string fieldName = fieldInfo.GetElementAsString("mnemonic");
                                string output = fieldInfo.GetElementAsString("documentation");

                                var fieldNickName = from f in fields
                                                    where f.FieldName == fieldName
                                                    select f.FieldNickName;

                                foreach (var f in fieldNickName)
                                {
                                    if (!outDict.ContainsKey(f))
                                        outDict.Add(f, output);
                                }
                            }
                            catch
                            {
                                Trace.WriteLine("Problem Downloading Field Definition for " + fieldData.GetElement("id"));
                            }
                        }
                    }
                }
                if (eventObj.Type == Event.EventType.RESPONSE) done = true;

            }
            return outDict;
        }
    }
}