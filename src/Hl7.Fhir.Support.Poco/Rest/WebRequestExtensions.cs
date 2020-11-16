/* 
 * Copyright (c) 2014, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-net-sdk/master/LICENSE
 */

using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Hl7.Fhir.Rest
{
    public static class WebRequestExtensions
    {
        internal static void WriteBody(this HttpWebRequest request, bool CompressRequestBody, byte[] data)
        {
#if NETSTANDARD1_6
            Stream outs = null;
            //outs = request.GetRequestStreamAsync().Result;
            //outs.Write(data, 0, (int)data.Length);
            //outs.Flush();
            //outs.Dispose();

            ManualResetEvent requestReady = new ManualResetEvent(initialState: false);
            Exception caught = null;

            AsyncCallback callback = new AsyncCallback(ar =>
            {
                //var request = (WebRequest)ar.AsyncState;
                try
                {
                    outs = request.EndGetRequestStream(ar);
                }
                catch (Exception ex)
                {
                    caught = ex;
                }
                finally
                {
                    requestReady.Set();
                }
            });

            var async = request.BeginGetRequestStream(callback, null);

            if (!async.IsCompleted)
            {
                //async.AsyncWaitHandle.WaitOne();
                // Not having thread affinity seems to work better with ManualResetEvent
                // Using AsyncWaitHandle.WaitOne() gave unpredictable results (in the
                // unit tests), when EndGetResponse would return null without any error
                // thrown
                requestReady.WaitOne();
                //async.AsyncWaitHandle.WaitOne();
            }
            else
            {
                // If the async wasn't finished, then we need to wait anyway
                if (!async.CompletedSynchronously)
                    requestReady.WaitOne();
            }

            if (caught != null) throw caught;

            outs.Write(data, 0, (int)data.Length);
            outs.Flush();
            outs.Dispose();
#else
            Stream outs;
            Stream compressor = null;
            if (CompressRequestBody)
            {
                request.Headers.Add(HttpRequestHeader.ContentEncoding, "gzip");
                compressor = request.GetRequestStream();
                outs = new System.IO.Compression.GZipStream(compressor, System.IO.Compression.CompressionMode.Compress, true);
            }
            else
            {
                outs = request.GetRequestStream();
            }
            outs.Write(data, 0, (int)data.Length);
            outs.Flush();
            outs.Dispose();
            if (compressor != null)
                compressor.Dispose();
#endif
        }

        internal static Task<WebResponse> GetResponseAsync(this WebRequest request, TimeSpan timeout)
        {
            var t = Task.Factory.FromAsync<WebResponse>(
                request.BeginGetResponse,
                request.EndGetResponse,
                null);

            return t.ContinueWith(parent =>
            {
                if (parent.IsFaulted)
                {
                    if (parent.Exception.GetBaseException() is WebException wex)
                    {
                        if (!(wex.Response is HttpWebResponse resp))
                            throw t.Exception.GetBaseException();
                        return resp;
                    }
                    throw t.Exception.GetBaseException();
                }
                return parent.Result;
            });
        }
    }
}




