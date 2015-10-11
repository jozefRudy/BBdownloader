using Bloomberglp.Blpapi;
using log4net;
using log4net.Config;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace bloomberg_downloader
{
    public class BloombergDownloader
    {
        private static readonly Name SECURITY_DATA = new Name("securityData");
        private static readonly Name SECURITY = new Name("security");
        private static readonly Name FIELD_DATA = new Name("fieldData");
        private static readonly Name RESPONSE_ERROR = new Name("responseError");
        private static readonly Name SECURITY_ERROR = new Name("securityError");
        private static readonly Name FIELD_EXCEPTIONS = new Name("fieldExceptions");
        private static readonly Name FIELD_ID = new Name("fieldId");
        private static readonly Name ERROR_INFO = new Name("errorInfo");
        private static readonly Name CATEGORY = new Name("category");
        private static readonly Name MESSAGE = new Name("message");

        private const String PX_LAST = "PX_LAST";
        private const String VWAP_VOLUME = "VWAP_VOLUME";
        private const String VWAP_LIT_VOLUME = "VWAP_LIT_VOLUME";
        private const String DATE = "date";

        private string _host;
        private int _port;
        private readonly ArrayList _securities;
        private readonly ArrayList _fields;

        private int? _dateId;

        public static readonly ILog Logger = LogManager.GetLogger(typeof(BloombergDownloader));

        public static void Main(string[] args)
        {
            // initialize log4net
            XmlConfigurator.Configure();

            Logger.Info("Starting Bloomberg downloader...");
            var downloader = new BloombergDownloader();
            downloader.Run(args);

            if (Logger.IsDebugEnabled)
            {
                Console.WriteLine("Press ENTER to quit");
                Console.Read();
            }
        }

        public BloombergDownloader()
        {
            _securities = new ArrayList();
            _fields = new ArrayList();
        }

        private void Run(string[] args)
        {
            var commandLineOptions = new CommandLineOptions();
            if (!CommandLine.Parser.Default.ParseArguments(args, commandLineOptions))
            {
                return;
            }

            Initialize(commandLineOptions);

            var sessionOptions = GetSessionOptions();

            Logger.InfoFormat("Connecting to [{0}]:[{1}]", _host, _port);
            var session = new Session(sessionOptions);
            bool sessionStarted = session.Start();
            if (!sessionStarted)
            {
                Logger.Error("Failed to start session.");
                return;
            }
            if (!session.OpenService("//blp/refdata"))
            {
                Logger.Error("Failed to open //blp/refdata");
                return;
            }

            try
            {
                SendRefDataRequest(session);
            }
            catch (InvalidRequestException e)
            {
                Logger.WarnFormat("Exception occurred: [{0}]", e);
            }

            // wait for events from session.
            EventLoop(session);

            session.Stop();
        }

        private void Initialize(CommandLineOptions commandLineOptions)
        {
            //TODO: check last parser state

            _host = commandLineOptions.Host;
            _port = commandLineOptions.Port;
            _dateId = commandLineOptions.DateId ?? DateTime.Now.PreviousDateId();

            // add fields
            _fields.AddRange(getFields());

            // add tickers
            _securities.AddRange(GetTickers().ToList());

            //TODO: is this needed
            registerCallback(3);
        }

        private SessionOptions GetSessionOptions()
        {
            var sessionOptions = new SessionOptions { ServerHost = _host, ServerPort = _port };
            return sessionOptions;
        }

        private void EventLoop(Session session)
        {
            var done = false;
            while (!done)
            {
                var eventObj = session.NextEvent();
                if (eventObj.Type == Event.EventType.PARTIAL_RESPONSE)
                {
                    Logger.Info("Processing Partial Response");
                    ProcessResponseEvent(eventObj);
                }
                else if (eventObj.Type == Event.EventType.RESPONSE)
                {
                    Logger.Info("Processing Response");
                    ProcessResponseEvent(eventObj);
                    done = true;
                }
                else
                {
                    foreach (var msg in eventObj)
                    {
                        Logger.DebugFormat("[{0}]", msg.AsElement);
                        if (eventObj.Type == Event.EventType.SESSION_STATUS)
                        {
                            if (msg.MessageType.Equals("SessionTerminated"))
                            {
                                done = true;
                            }
                        }
                    }
                }
            }
        }

        // return true if processing is completed, false otherwise
        private void ProcessResponseEvent(IEnumerable<Message> eventObj)
        {
            foreach (var msg in eventObj)
            {
                // one message per security
                if (msg.HasElement(RESPONSE_ERROR))
                {
                    printErrorInfo("REQUEST FAILED: ", msg.GetElement(RESPONSE_ERROR));
                    continue;
                }

                var securityData = msg.GetElement(SECURITY_DATA);
                var fieldDataArray = securityData.GetElement(FIELD_DATA);
                var ticker = securityData.GetElementAsString(SECURITY);
                Logger.InfoFormat("Processing [{0}]", ticker);
                Element fieldData;
                try
                {
                    fieldData = fieldDataArray.GetValueAsElement();
                }
                catch (ArgumentOutOfRangeException exception)
                {
                    Logger.WarnFormat("Exception occurred for [{0}]. Exception: [{1}]", ticker, exception.Message);
                    continue;
                }
                if (fieldData.HasElement("securityError"))
                {
                    printErrorInfo("\tSECURITY FAILED: ", fieldData.GetElement(SECURITY_ERROR));
                    continue;
                }

                if (fieldData.NumElements > 0)
                {
                    Logger.Info("FIELD\t\tVALUE");
                    Logger.Info("-----\t\t-----");
                    var numElements = fieldData.NumElements;
                    for (var j = 0; j < numElements; ++j)
                    {
                        var field = fieldData.GetElement(j);
                        Logger.Info(field.Name + "\t\t" + field.GetValueAsString());
                    }
                    InsertData(fieldData, ticker);
                }
                var fieldExceptions = securityData.GetElement(FIELD_EXCEPTIONS);
                if (fieldExceptions.NumValues > 0)
                {
                    Logger.Info("FIELD\t\tEXCEPTION");
                    Logger.Info("-----\t\t---------");
                    for (var k = 0; k < fieldExceptions.NumValues; ++k)
                    {
                        var fieldException = fieldExceptions.GetValueAsElement(k);
                        printErrorInfo(fieldException.GetElementAsString(FIELD_ID) +
                            "\t\t", fieldException.GetElement(ERROR_INFO));
                    }
                }
            }
        }

        private static void InsertData(Element fieldData, String ticker)
        {
            if (IsValid(fieldData))
            {
                using (var context = new EFMMDataClassesDataContext())
                {
                    context.insertBloombergDownload(fieldData.GetElementAsDate(DATE).ToDateId()
                        , (decimal)fieldData.GetElementAsFloat64(PX_LAST)
                        , fieldData.GetElementAsInt64(VWAP_VOLUME)
                        , fieldData.GetElementAsInt64(VWAP_LIT_VOLUME)
                        , ticker);
                    context.SubmitChanges();
                }
            }
            else
            {
                Logger.WarnFormat("At least one field missing for [{0}]", ticker);
            }
        }

        private static bool IsValid(Element fieldData)
        {
            return fieldData.HasElement(DATE)
                            && fieldData.HasElement(PX_LAST)
                            && fieldData.HasElement(VWAP_VOLUME)
                            && fieldData.HasElement(VWAP_LIT_VOLUME);
        }

        private static IEnumerable<string> GetTickers()
        {
            var tickers = new List<string>();
            using (var context = new EFMMDataClassesDataContext())
            {
                var result = from ticker in context.getBloombergTickers(null)
                             select ticker.value;
                tickers.AddRange(result);
            }
            return tickers;
        }

        private void SendRefDataRequest(Session session)
        {
            var request = CreateRequest(session);

            Logger.Info("Sending Request: " + request);
            session.SendRequest(request, null);
        }

        private Request CreateRequest(Session session)
        {
            var request = session.GetService("//blp/refdata").CreateRequest("HistoricalDataRequest");

            AddSecurities(request);
            AddFields(request);

            request.Set("startDate", _dateId.ToString());
            request.Set("endDate", _dateId.ToString());
            request.Set("periodicityAdjustment", "ACTUAL");
            request.Set("periodicitySelection", "DAILY"); // 1 data point per day
            request.Set("currency", "EUR"); // request px_last converted to euro

            return request;
        }

        private void AddFields(Request request)
        {
            foreach (var field in _fields)
            {
                request.Append("fields", (string)field);
            }
        }

        private void AddSecurities(Request request)
        {
            foreach (var ticker in _securities)
            {
                request.Append("securities", (string)ticker);
            }
        }

        internal class LoggingCallback : Logging.Callback
        {
            public void OnMessage(long threadId,
                TraceLevel level,
                Datetime dateTime,
                String
                loggerName,
                String message)
            {
                Logger.Info(dateTime + "  " + loggerName
                    + " [" + level + "] Thread ID = "
                    + threadId + " " + message);
            }
        }

        private void registerCallback(int verbosityCount)
        {
            TraceLevel level;
            switch (verbosityCount)
            {
                case 1:
                    {
                        level = TraceLevel.Warning;
                    } break;
                case 2:
                    {
                        level = TraceLevel.Info;
                    } break;
                default:
                    {
                        level = TraceLevel.Verbose;
                    } break;
            }
            Logging.RegisterCallback(new LoggingCallback(), level);
        }

        private List<string> getFields()
        {
            return new List<string> { PX_LAST, VWAP_LIT_VOLUME, VWAP_VOLUME };
        }

        private void printErrorInfo(string leadingStr, Element errorInfo)
        {
            Logger.Error(leadingStr + errorInfo.GetElementAsString(CATEGORY) +
                " (" + errorInfo.GetElementAsString(MESSAGE) + ")");
        }
    }
}