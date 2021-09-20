using System.Net.Http;

namespace Firely.Fhir.Packages
{


    public static class Testing
    {
        public static HttpClient GetInsecureClient()
        {
            // for testing without proper certificate
#if !NET452
            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            var client = new HttpClient(httpClientHandler, true);
#else
            // [WMR 20181102] HttpClientHandler and HttpClient are IDisposable ...

            // ServerCertificateCustomValidationCallback needs NET471
            var hander = new WebRequestHandler();
            hander.ServerCertificateValidationCallback = (message, cert, chain, errors) => true;
            var client = new HttpClient(hander, true);
#endif
            return client;
        }
    }


}
