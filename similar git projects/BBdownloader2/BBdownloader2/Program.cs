using System;
using System.Collections.Generic;
using System.Linq;
using BBCOMM = Bloomberglp.Blpapi;
using BloombergAPI;
//
// CLIENT PROGRAM
namespace BBdownloader2
{
    class Program
    {
        // data structures
        static List<string> bloombergSecurityNames;
        static List<string> bloombergFieldNames;
        static List<string> bloombergOverrideFields;
        static List<string> bloombergOverrideValues;
        static dynamic[,,] result;
        //
        static int Main()
        {
            try
            {
                Console.WriteLine("CASE 1 : create reference data request without overrides >");
                Console.WriteLine();
                //
                // create securities
                bloombergSecurityNames = new List<string>();
                bloombergSecurityNames.Add("GOOG US Equity");
                bloombergSecurityNames.Add("CSCO US Equity");
                bloombergSecurityNames.Add("CCE US Equity");
                //
                // create fields
                bloombergFieldNames = new List<string>();
                bloombergFieldNames.Add("ERN_ANN_DT_AND_PER");
                bloombergFieldNames.Add("PX_LAST");
                bloombergFieldNames.Add("BEST_EPS");
                bloombergFieldNames.Add("GICS_SUB_INDUSTRY_NAME");
                bloombergFieldNames.Add("EQY_OPT_AVAIL");
                bloombergFieldNames.Add("LATEST_ANN_DT_ANNUAL");
                bloombergFieldNames.Add("PX_ROUND_LOT_SIZE");
                bloombergFieldNames.Add("MIKE_JUNIPERHILL_SHOE_SIZE");
                //
                // create wrapper object, retrieve and print data
                BBCOMMDataRequest wrapper = new ReferenceDataRequest(bloombergSecurityNames, bloombergFieldNames);
                result = wrapper.ProcessData();
                Print(bloombergSecurityNames, result);
                //
                Console.WriteLine("CASE 2 : create reference data request with override >");
                Console.WriteLine();
                //
                // re-create fields
                bloombergFieldNames = new List<string>();
                bloombergFieldNames.Add("PX_LAST");
                bloombergFieldNames.Add("BEST_EPS");
                //
                // create override for best fiscal period
                bloombergOverrideFields = new List<string>();
                bloombergOverrideFields.Add("BEST_FPERIOD_OVERRIDE");
                bloombergOverrideValues = new List<string>();
                bloombergOverrideValues.Add("2FY");
                //
                // retrieve and print data
                wrapper = new ReferenceDataRequest(bloombergSecurityNames, bloombergFieldNames, bloombergOverrideFields, bloombergOverrideValues);
                result = wrapper.ProcessData();
                Print(bloombergSecurityNames, result);
                //
                Console.WriteLine("CASE 3 : create historical data request for one security >");
                Console.WriteLine();
                //
                bloombergSecurityNames = new List<string>();
                bloombergSecurityNames.Add("GOOG US Equity");
                //
                // re-create field
                bloombergFieldNames = new List<string>();
                bloombergFieldNames.Add("PX_LAST");
                //
                // create dates
                DateTime startDate = DateTime.Today.AddDays((double)-21);
                DateTime endDate = DateTime.Today.AddDays((double)-1);
                //
                // retrieve and print data
                // request time-series for Google : actual daily frequency, but only for weekdays and converted to Thai baht
                wrapper = new HistoricalDataRequest(bloombergSecurityNames, bloombergFieldNames, startDate, endDate,
                    bloombergNonTradingDayFillOption: E_NON_TRADING_DAY_FILL_OPTION.NON_TRADING_WEEKDAYS,
                    bloombergOverrideCurrency: "THB");
                //
                result = wrapper.ProcessData();
                Print(bloombergSecurityNames, result);
                //
                Console.WriteLine("CASE 4 : create historical data request for three securities >");
                Console.WriteLine();
                //
                // re-create securities
                bloombergSecurityNames = new List<string>();
                bloombergSecurityNames.Add("GOOG US Equity");
                bloombergSecurityNames.Add("CSCO US Equity");
                bloombergSecurityNames.Add("CCE US Equity");
                //
                // re-create field
                bloombergFieldNames = new List<string>();
                bloombergFieldNames.Add("PX_LAST");
                //
                // retrieve and print data
                // request three time-series : use default settings to retrieve date-consistent result data
                wrapper = new HistoricalDataRequest(bloombergSecurityNames, bloombergFieldNames, startDate, endDate);
                result = wrapper.ProcessData();
                Print(bloombergSecurityNames, result);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            //
            Console.ReadLine();
            return 0;
        }
        // result printer
        public static void Print(List<string> securities, dynamic[,,] data)
        {
            for (int i = 0; i < data.GetLength(0); i++)
            {
                Console.WriteLine(securities[i]);
                for (int j = 0; j < data.GetLength(1); j++)
                {
                    for (int k = 0; k < data.GetLength(2); k++)
                    {
                        Console.WriteLine(data[i, j, k]);
                    }
                }
                Console.WriteLine();
            }
        }
    }
}
//
// WRAPPER
namespace BloombergAPI
{
    // enumerators for historical data request settings
    public enum E_PRICING_OPTION { PRICING_OPTION_PRICE, PRICING_OPTION_YIELD };
    public enum E_PERIODICITY_ADJUSTMENT { ACTUAL, CALENDAR, FISCAL };
    public enum E_PERIODICITY_SELECTION { DAILY, WEEKLY, MONTHLY, QUARTERLY, SEMI_ANNUALLY, YEARLY };
    public enum E_NON_TRADING_DAY_FILL_OPTION { NON_TRADING_WEEKDAYS, ALL_CALENDAR_DAYS, ACTIVE_DAYS_ONLY };
    public enum E_NON_TRADING_DAY_FILL_METHOD { PREVIOUS_VALUE, NIL_VALUE };
    //
    // abstract base class for data request
    public abstract class BBCOMMDataRequest
    {
        // BBCOMM names
        protected readonly BBCOMM.Name SECURITY_DATA = new BBCOMM.Name("securityData");
        protected readonly BBCOMM.Name FIELD_DATA = new BBCOMM.Name("fieldData");
        protected readonly BBCOMM.Name FIELD_ID = new BBCOMM.Name("fieldId");
        protected readonly BBCOMM.Name VALUE = new BBCOMM.Name("value");
        protected readonly BBCOMM.Name OVERRIDES = new BBCOMM.Name("overrides");
        protected readonly BBCOMM.Name SECURITIES = new BBCOMM.Name("securities");
        protected readonly BBCOMM.Name FIELDS = new BBCOMM.Name("fields");
        protected readonly BBCOMM.Name SEQUENCE_NUMBER = new BBCOMM.Name("sequenceNumber");
        protected readonly BBCOMM.Name START_DATE = new BBCOMM.Name("startDate");
        protected readonly BBCOMM.Name END_DATE = new BBCOMM.Name("endDate");
        protected readonly BBCOMM.Name DATE = new BBCOMM.Name("date");
        protected readonly BBCOMM.Name PRICING_OPTION = new BBCOMM.Name("pricingOption");
        protected readonly BBCOMM.Name PERIODICITY_ADJUSTMENT = new BBCOMM.Name("periodicityAdjustment");
        protected readonly BBCOMM.Name PERIODICITY_SELECTION = new BBCOMM.Name("periodicitySelection");
        protected readonly BBCOMM.Name NON_TRADING_DAY_FILL_OPTION = new BBCOMM.Name("nonTradingDayFillOption");
        protected readonly BBCOMM.Name NON_TRADING_DAY_FILL_METHOD = new BBCOMM.Name("nonTradingDayFillMethod");
        protected readonly BBCOMM.Name OVERRIDE_CURRENCY = new BBCOMM.Name("currency");
        //
        // const strings, enumerators, etc.
        protected readonly string NOT_AVAILABLE = "#N/A";
        protected readonly string SESSION_EXCEPTION = "Session not started";
        protected readonly string SERVICE_EXCEPTION = "Service not opened";
        protected readonly string REQUEST_TYPE_REFERENCE = "ReferenceDataRequest";
        protected readonly string REQUEST_TYPE_HISTORICAL = "HistoricalDataRequest";
        protected readonly string REFERENCE_DATA_SERVICE = "//blp/refdata";
        protected readonly string BLOOMBERG_DATE_FORMAT = "yyyyMMdd";
        protected E_PRICING_OPTION pricingOption;
        protected E_PERIODICITY_ADJUSTMENT periodicityAdjustment;
        protected E_PERIODICITY_SELECTION periodicitySelection;
        protected E_NON_TRADING_DAY_FILL_OPTION nonTradingDayFillOption;
        protected E_NON_TRADING_DAY_FILL_METHOD nonTradingDayFillMethod;
        protected string requestType;
        protected string startDate;
        protected string endDate;
        protected string overrideCurrency;
        //
        // wrapped BBCOMM objects
        protected BBCOMM.Session session;
        protected BBCOMM.Service service;
        protected BBCOMM.Request request;
        //
        // input data structures
        protected List<string> securityNames = new List<string>();
        protected List<string> fieldNames = new List<string>();
        protected List<string> overrideFields = new List<string>();
        protected List<string> overrideValues = new List<string>();
        //
        // output result data structure
        protected dynamic[,,] result;
        //
        public dynamic[,,] ProcessData()
        {
            Open();
            CreateRequest();
            SendRequest();
            Close();
            return result;
        }
        private void Open()
        {
            // create and start bloomberg BBCOMM session
            BBCOMM.SessionOptions sessionOptions = new BBCOMM.SessionOptions();
            session = new BBCOMM.Session(sessionOptions);
            if (!session.Start()) throw new Exception(SESSION_EXCEPTION);
            //
            // get service from session object and create request by service object
            if (!session.OpenService(REFERENCE_DATA_SERVICE)) throw new Exception(SERVICE_EXCEPTION);
            service = session.GetService(REFERENCE_DATA_SERVICE);
            request = service.CreateRequest(requestType);
        }
        private void CreateRequest()
        {
            // append securities, fields
            foreach (string securityName in securityNames) request.Append(SECURITIES, securityName);
            foreach (string fieldName in fieldNames) request.Append(FIELDS, fieldName);
            //
            // conditionally, append overrides into request object
            if (overrideFields.Count > 0)
            {
                BBCOMM.Element requestOverrides = request.GetElement(OVERRIDES);
                for (int i = 0; i < overrideFields.Count; i++)
                {
                    BBCOMM.Element requestOverride = requestOverrides.AppendElement();
                    requestOverride.SetElement(FIELD_ID, overrideFields[i]);
                    requestOverride.SetElement(VALUE, overrideValues[i]);
                }
            }
            // set optional parameters for historical data request
            if (requestType == REQUEST_TYPE_HISTORICAL)
            {
                request.Set(START_DATE, startDate);
                request.Set(END_DATE, endDate);
                request.Set(PRICING_OPTION, pricingOption.ToString());
                request.Set(PERIODICITY_ADJUSTMENT, periodicityAdjustment.ToString());
                request.Set(PERIODICITY_SELECTION, periodicitySelection.ToString());
                request.Set(NON_TRADING_DAY_FILL_OPTION, nonTradingDayFillOption.ToString());
                request.Set(NON_TRADING_DAY_FILL_METHOD, nonTradingDayFillMethod.ToString());
                if (overrideCurrency != String.Empty) request.Set(OVERRIDE_CURRENCY, overrideCurrency);
            }
        }
        private void SendRequest()
        {
            // send constructed request to BBCOMM server
            long ID = Guid.NewGuid().GetHashCode();
            session.SendRequest(request, new BBCOMM.CorrelationID(ID));
            bool isProcessing = true;
            //
            while (isProcessing)
            {
                // receive data response from BBCOMM server, send 
                // response to be processed by sub-classed algorithm
                BBCOMM.Event response = session.NextEvent();
                switch (response.Type)
                {
                    case BBCOMM.Event.EventType.PARTIAL_RESPONSE:
                        ProcessDataResponse(ref response);
                        break;
                    case BBCOMM.Event.EventType.RESPONSE:
                        ProcessDataResponse(ref response);
                        isProcessing = false;
                        break;
                    default:
                        break;
                }
            }
        }
        private void Close()
        {
            // close BBCOMM session
            if (session != null) session.Stop();
        }
        //
        // sub-classes are providing specific algorithm implementations for 
        // processing and packing BBCOMM server response data into resulting data structure
        protected abstract void ProcessDataResponse(ref BBCOMM.Event response);
    }
    //
    // concrete class implementation for processing reference data request
    public class ReferenceDataRequest : BBCOMMDataRequest
    {
        public ReferenceDataRequest(List<string> bloombergSecurityNames,
            List<string> bloombergFieldNames)
        {
            // ctor : create reference data request without field overrides
            requestType = REQUEST_TYPE_REFERENCE;
            securityNames = bloombergSecurityNames;
            fieldNames = bloombergFieldNames;
            //
            // define result data structure dimensions for reference data request
            result = new dynamic[securityNames.Count, 1, fieldNames.Count];
        }
        public ReferenceDataRequest(List<string> bloombergSecurityNames,
            List<string> bloombergFieldNames, List<string> bloombergOverrideFields,
            List<string> bloombergOverrideValues)
        {
            // ctor : create reference data request with field overrides
            requestType = REQUEST_TYPE_REFERENCE;
            securityNames = bloombergSecurityNames;
            fieldNames = bloombergFieldNames;
            overrideFields = bloombergOverrideFields;
            overrideValues = bloombergOverrideValues;
            //
            // define result data structure dimensions for reference data request
            result = new dynamic[securityNames.Count, 1, fieldNames.Count];
        }
        protected override void ProcessDataResponse(ref BBCOMM.Event response)
        {
            // receive response, which contains N securities and M fields
            // event queue can send multiple responses for large requests
            foreach (BBCOMM.Message message in response.GetMessages())
            {
                // extract N securities
                BBCOMM.Element securities = message.GetElement(SECURITY_DATA);
                int nSecurities = securities.NumValues;
                //
                // loop through all securities
                for (int i = 0; i < nSecurities; i++)
                {
                    // extract one security and fields for this security
                    BBCOMM.Element security = securities.GetValueAsElement(i);
                    BBCOMM.Element fields = security.GetElement(FIELD_DATA);
                    int sequenceNumber = security.GetElementAsInt32(SEQUENCE_NUMBER);
                    int nFieldNames = fieldNames.Count;
                    //
                    // loop through all M fields for this security
                    for (int j = 0; j < nFieldNames; j++)
                    {
                        // if the requested field has been found, pack value into result data structure
                        if (fields.HasElement(fieldNames[j]))
                        {
                            result[sequenceNumber, 0, j] = fields.GetElement(fieldNames[j]).GetValue();
                        }
                        // otherwise, pack NOT_AVAILABLE string into data structure
                        else
                        {
                            result[sequenceNumber, 0, j] = NOT_AVAILABLE;
                        }
                    }
                }
            }
        }
    }
    //
    // concrete class implementation for processing historical data request
    public class HistoricalDataRequest : BBCOMMDataRequest
    {
        private bool hasDimensions = false;
        //
        // optional parameters are configured to retrieve time-series having actual daily observations, including all weekdays,
        // in the case of non-trading days the previous date value will be used.
        public HistoricalDataRequest(List<string> bloombergSecurityNames, List<string> bloombergFieldNames,
            DateTime bloombergStartDate, DateTime BloombergEndDate,
            E_PRICING_OPTION bloombergPricingOption = E_PRICING_OPTION.PRICING_OPTION_PRICE,
            E_PERIODICITY_SELECTION bloombergPeriodicitySelection = E_PERIODICITY_SELECTION.DAILY,
            E_PERIODICITY_ADJUSTMENT bloombergPeriodicityAdjustment = E_PERIODICITY_ADJUSTMENT.ACTUAL,
            E_NON_TRADING_DAY_FILL_OPTION bloombergNonTradingDayFillOption = E_NON_TRADING_DAY_FILL_OPTION.ALL_CALENDAR_DAYS,
            E_NON_TRADING_DAY_FILL_METHOD bloombergNonTradingDayFillMethod = E_NON_TRADING_DAY_FILL_METHOD.PREVIOUS_VALUE,
            string bloombergOverrideCurrency = "")
        {
            // ctor : create historical data request without field overrides
            requestType = REQUEST_TYPE_HISTORICAL;
            securityNames = bloombergSecurityNames;
            fieldNames = bloombergFieldNames;
            startDate = bloombergStartDate.ToString(BLOOMBERG_DATE_FORMAT);
            endDate = BloombergEndDate.ToString(BLOOMBERG_DATE_FORMAT);
            //
            pricingOption = bloombergPricingOption;
            periodicitySelection = bloombergPeriodicitySelection;
            periodicityAdjustment = bloombergPeriodicityAdjustment;
            nonTradingDayFillOption = bloombergNonTradingDayFillOption;
            nonTradingDayFillMethod = bloombergNonTradingDayFillMethod;
            overrideCurrency = bloombergOverrideCurrency;
        }
        public HistoricalDataRequest(List<string> bloombergSecurityNames, List<string> bloombergFieldNames,
            DateTime bloombergStartDate, DateTime BloombergEndDate, List<string> bloombergOverrideFields,
            List<string> bloombergOverrideValues,
            E_PRICING_OPTION bloombergPricingOption = E_PRICING_OPTION.PRICING_OPTION_PRICE,
            E_PERIODICITY_SELECTION bloombergPeriodicitySelection = E_PERIODICITY_SELECTION.DAILY,
            E_PERIODICITY_ADJUSTMENT bloombergPeriodicityAdjustment = E_PERIODICITY_ADJUSTMENT.ACTUAL,
            E_NON_TRADING_DAY_FILL_OPTION bloombergNonTradingDayFillOption = E_NON_TRADING_DAY_FILL_OPTION.ALL_CALENDAR_DAYS,
            E_NON_TRADING_DAY_FILL_METHOD bloombergNonTradingDayFillMethod = E_NON_TRADING_DAY_FILL_METHOD.PREVIOUS_VALUE,
            string bloombergOverrideCurrency = "")
        {
            // ctor : create historical data request with field overrides
            requestType = REQUEST_TYPE_HISTORICAL;
            securityNames = bloombergSecurityNames;
            fieldNames = bloombergFieldNames;
            overrideFields = bloombergOverrideFields;
            overrideValues = bloombergOverrideValues;
            startDate = bloombergStartDate.ToString(BLOOMBERG_DATE_FORMAT);
            endDate = BloombergEndDate.ToString(BLOOMBERG_DATE_FORMAT);
            //
            pricingOption = bloombergPricingOption;
            periodicitySelection = bloombergPeriodicitySelection;
            periodicityAdjustment = bloombergPeriodicityAdjustment;
            nonTradingDayFillOption = bloombergNonTradingDayFillOption;
            nonTradingDayFillMethod = bloombergNonTradingDayFillMethod;
            overrideCurrency = bloombergOverrideCurrency;
        }
        protected override void ProcessDataResponse(ref BBCOMM.Event response)
        {
            // unzip and pack messages received from BBCOMM server
            // receive one security per message and multiple messages per event
            foreach (BBCOMM.Message message in response.GetMessages())
            {
                // extract security and fields
                BBCOMM.Element security = message.GetElement(SECURITY_DATA);
                BBCOMM.Element fields = security.GetElement(FIELD_DATA);
                //
                int sequenceNumber = security.GetElementAsInt32(SEQUENCE_NUMBER);
                int nFieldNames = fieldNames.Count;
                int nObservationDates = fields.NumValues;
                //
                // the exact dimension will be known only, when the response has been received from BBCOMM server
                if (!hasDimensions)
                {
                    // define result data structure dimensions for historical data request
                    // observation date will be stored into first field for each observation date
                    result = new dynamic[securityNames.Count, nObservationDates, fieldNames.Count + 1];
                    hasDimensions = true;
                }
                //
                // loop through all observation dates
                for (int i = 0; i < nObservationDates; i++)
                {
                    // extract all field data for a single observation date
                    BBCOMM.Element observationDateFields = fields.GetValueAsElement(i);
                    //
                    // pack observation date into data structure
                    result[sequenceNumber, i, 0] = observationDateFields.GetElementAsDatetime(DATE);
                    //
                    // then, loop through all 'user-requested' fields for a given observation date
                    // and pack results data into data structure
                    for (int j = 0; j < nFieldNames; j++)
                    {
                        // pack field value into data structure if such value has been found
                        if (observationDateFields.HasElement(fieldNames[j]))
                        {
                            result[sequenceNumber, i, j + 1] = observationDateFields.GetElement(fieldNames[j]).GetValue();
                        }
                        // otherwise, pack NOT_AVAILABLE string into data structure
                        else
                        {
                            result[sequenceNumber, i, j + 1] = NOT_AVAILABLE;
                        }
                    }
                }
            }
        }
    }
}