/* 
 * Copyright (c) 2014, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

using Hl7.Fhir.Utility;
using System;
using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;

namespace Hl7.Fhir.Rest
{    
    public class Requester : IRequester
    {
        public Uri BaseUrl { get; private set; }

        public FhirClientSettings Settings { get; set; }

        public Action<HttpWebRequest, byte[]> BeforeRequest { get; set; }
        public Action<HttpWebResponse, byte[]> AfterResponse { get; set; }

        public Requester(Uri baseUrl, FhirClientSettings settings)
        {
            BaseUrl = baseUrl;
            Settings = settings;
        }

        public EntryResponse Execute(EntryRequest interaction)
        {
            return ExecuteAsync(interaction).WaitResult();
        }
        public async Task<EntryResponse> ExecuteAsync(EntryRequest interaction)
        {
            if (interaction == null) throw Error.ArgumentNull(nameof(interaction));

            bool compressRequestBody = false;

            var request = interaction.ToHttpRequest(BaseUrl, Settings);

            //Handled in ToHttpRequest
            //request.ContentType = interaction.ContentType;

#if !NETSTANDARD1_1
            request.Timeout = Settings.Timeout;
#endif

            if (Settings.PreferCompressedResponses)
            {
                request.Headers["Accept-Encoding"] = "gzip, deflate";
            }

            var result = new EntryResponse();
            result.LastRequest = request;

            BeforeRequest?.Invoke(request, interaction.RequestBodyContent);

            // Write the body to the output
            if (interaction.RequestBodyContent != null)
                request.WriteBody(compressRequestBody, interaction.RequestBodyContent);

            // Make sure the HttpResponse gets disposed!
            using (HttpWebResponse webResponse = (HttpWebResponse)await request.GetResponseAsync(new TimeSpan(0, 0, 0, 0, Settings.Timeout)).ConfigureAwait(false))
            {
                try
                {
                    //Read body before we call the hook, so the hook cannot read the body before we do
                    var inBody = readBody(webResponse);

                    result.LastResponse = webResponse;
                    AfterResponse?.Invoke(webResponse, inBody);

                    webResponse.ToEntryResponse(inBody, ref result);
                    return result;
                }
                catch (AggregateException ae)
                {
                    throw ae.GetBaseException();
                }
            }
        }

        private static byte[] readBody(HttpWebResponse response)
        {
            if (response.ContentLength != 0)
            {
                byte[] body = null;
                var respStream = response.GetResponseStream();
#if !DOTNETFW
                var contentEncoding = response.Headers["Content-Encoding"];
#else
                    var contentEncoding = response.ContentEncoding;
#endif
                if (contentEncoding == "gzip")
                {
                    using (var decompressed = new GZipStream(respStream, CompressionMode.Decompress, true))
                    {
                        body = HttpUtil.ReadAllFromStream(decompressed);
                    }
                }
                else if (contentEncoding == "deflate")
                {
                    using (var decompressed = new DeflateStream(respStream, CompressionMode.Decompress, true))
                    {
                        body = HttpUtil.ReadAllFromStream(decompressed);
                    }
                }
                else
                {
                    body = HttpUtil.ReadAllFromStream(respStream);
                }
                respStream.Dispose();

                if (body.Length > 0)
                    return body;
                else
                    return null;
            }
            else
                return null;
        }
    }
}
