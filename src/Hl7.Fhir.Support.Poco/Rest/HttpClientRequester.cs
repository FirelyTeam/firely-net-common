/* 
 * Copyright (c) 2014, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-net-sdk/master/LICENSE
 */

using Hl7.Fhir.Utility;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Hl7.Fhir.Rest
{
    public class HttpClientRequester : IClientRequester, IDisposable
    {
        public FhirClientSettings Settings { get; set; }
        public Uri BaseUrl { get; private set; }
        public HttpClient Client { get; private set; }

        public HttpClientRequester(Uri baseUrl, FhirClientSettings settings, HttpMessageHandler messageHandler)
        {
            Settings = settings;
            BaseUrl = baseUrl;

            Client = new HttpClient(messageHandler);
            Client.DefaultRequestHeaders.Add("User-Agent", $".NET FhirClient for FHIR");
            Client.Timeout = new TimeSpan(0, 0, 0, Settings.Timeout);
        }


        public EntryResponse LastResult { get; private set; }

        public EntryResponse Execute(EntryRequest interaction)
        {
            return ExecuteAsync(interaction).WaitResult();
        }

        public async Task<EntryResponse> ExecuteAsync(EntryRequest interaction)
        {
            if (interaction == null) throw Error.ArgumentNull(nameof(interaction));
            bool compressRequestBody = Settings.CompressRequestBody;

            using var requestMessage = interaction.ToHttpRequestMessage(BaseUrl, Settings);
            if (Settings.PreferCompressedResponses)
            {
                requestMessage.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
                requestMessage.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
            }

            byte[] outgoingBody = null;
            if (requestMessage.Method == HttpMethod.Post || requestMessage.Method == HttpMethod.Put)
            {
                outgoingBody = await requestMessage.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            }

            using var response = await Client.SendAsync(requestMessage).ConfigureAwait(false);
            try
            {
                var body = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);

                LastResult = response.ToEntryResponse(body);
                return LastResult;
            }
            catch (AggregateException ae)
            {
                throw ae.GetBaseException();
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.Client.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }

}
