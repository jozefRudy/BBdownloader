using System;
using System.Net;

namespace BBdownloader.GoogleDocs
{
    public class WebClientEx : WebClient
    {
        public WebClientEx()
        {
            this.container = new CookieContainer();
            this.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:22.0) Gecko/20100101 Firefox/22.0");
            this.Headers.Add("DNT", "1");
            this.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            this.Headers.Add("Accept-Encoding", "deflate");
            this.Headers.Add("Accept-Language", "en-US,en;q=0.5");
        }

        private readonly CookieContainer container = new CookieContainer();

        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest r = base.GetWebRequest(address);
            var request = r as HttpWebRequest;
            if (request != null)
            {
                request.CookieContainer = container;
            }
            return r;
        }

        protected override WebResponse GetWebResponse(WebRequest request, IAsyncResult result)
        {
            WebResponse response = base.GetWebResponse(request, result);
            ReadCookies(response);
            return response;
        }

        protected override WebResponse GetWebResponse(WebRequest request)        
        {
            try
            {
                WebResponse response = base.GetWebResponse(request);
                ReadCookies(response);
                return response;
            }
            catch
            {
                Console.WriteLine("Internet Connection Problem - Failed to download google docs");
                Environment.Exit(0);
            }
            return null;
        }

        private void ReadCookies(WebResponse r)
        {
            var response = r as HttpWebResponse;
            if (response != null)
            {
                CookieCollection cookies = response.Cookies;
                container.Add(cookies);
            }
        }
    }
}

