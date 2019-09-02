/* 
 * Copyright (c) 2014, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

using System;
using System.IO;
using System.Net;
using System.Text;

namespace Hl7.Fhir.Rest
{
    public static class HttpToEntryExtensions
    {
        //private const string USERDATA_BODY = "$body";

        internal static EntryResponse ToEntryResponse(this HttpWebResponse response, byte[] body, ref EntryResponse result)
        {
            result = result ?? new EntryResponse();

            result.Status = ((int)response.StatusCode).ToString();
            foreach (var key in response.Headers.AllKeys)
            {
                result.Headers.Add(key, response.Headers[key]);
            }
            result.ResponseUri = response.ResponseUri;
            result.Location = response.Headers[HttpUtil.LOCATION] ?? response.Headers[HttpUtil.CONTENTLOCATION];

#if NETSTANDARD1_1
            if (!String.IsNullOrEmpty(response.Headers[HttpUtil.LASTMODIFIED]))
            {
                DateTimeOffset dateTimeOffset = new DateTimeOffset();
                bool success = DateTimeOffset.TryParse(response.Headers[HttpUtil.LASTMODIFIED], out dateTimeOffset);
                if (!success)
                    throw new FormatException($"Last-Modified header has value '{response.Headers[HttpUtil.LASTMODIFIED]}', which is not recognized as a valid DateTime");
                result.LastModified = dateTimeOffset;
            }
#else
            result.LastModified = response.LastModified;
#endif
            result.Etag = getETag(response);
            result.ContentType = getContentType(response);
            result.Body = body;

            return result;
        }

        private static string getETag(HttpWebResponse response)
        {
            var result = response.Headers[HttpUtil.ETAG];

            if (result != null)
            {
                if (result.StartsWith(@"W/")) result = result.Substring(2);
                result = result.Trim('\"');
            }

            return result;
        }

        private static string getContentType(HttpWebResponse response)
        {
            if (!String.IsNullOrEmpty(response.ContentType))
            {
                return ContentType.GetMediaTypeFromHeaderValue(response.ContentType);
            }
            else
                return null;
        }

        public static bool IsSuccessful(this EntryResponse response)
        {
            int.TryParse(response.Status, out int code);
            return code >= 200 && code < 300;
        }
        
        public static string GetBodyAsText(this EntryResponse interaction)
        {
            var body = interaction.Body;

            if (body != null)
                return HttpUtil.DecodeBody(body, Encoding.UTF8);
            else
                return null;
        }
    }
}
