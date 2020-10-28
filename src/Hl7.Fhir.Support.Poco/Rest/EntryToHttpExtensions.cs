/* 
 * Copyright (c) 2014, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-net-sdk/master/LICENSE
 */

using Hl7.Fhir.Serialization;
using Hl7.Fhir.Utility;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;

namespace Hl7.Fhir.Rest
{
    internal static class EntryToHttpExtensions
    {
        public static HttpRequestMessage ToHttpRequestMessage(this EntryRequest entry, Uri baseUrl, FhirClientSettings settings)
        {

            System.Diagnostics.Debug.WriteLine("{0}: {1}", entry.Method, entry.Url);

            if (entry.RequestBodyContent != null && !(entry.Method == HTTPVerb.POST || entry.Method == HTTPVerb.PUT || entry.Method == HTTPVerb.PATCH))
                throw Error.InvalidOperation("Cannot have a body on an Http " + entry.Method.ToString());

            // Create an absolute uri when the interaction.Url is relative.
            var uri = new Uri(entry.Url, UriKind.RelativeOrAbsolute);
            if (!uri.IsAbsoluteUri)
            {
                uri = HttpUtil.MakeAbsoluteToBase(uri, baseUrl);
            }

            var location = new RestUrl(uri);

            if (settings.UseFormatParameter)
                location.AddParam(HttpUtil.RESTPARAM_FORMAT, ContentType.BuildFormatParam(settings.PreferredFormat));

            var request = new HttpRequestMessage(getMethod(entry.Method), location.Uri);

            request.Headers.Add("User-Agent", ".NET FhirClient for FHIR " + entry.Agent);

            if (!settings.UseFormatParameter && !string.IsNullOrEmpty(entry.Headers.Accept))
                request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(entry.Headers.Accept));

            if (entry.Headers.IfMatch != null) request.Headers.Add("If-Match", entry.Headers.IfMatch);
            if (entry.Headers.IfNoneMatch != null) request.Headers.Add("If-None-Match", entry.Headers.IfNoneMatch);
            if (entry.Headers.IfModifiedSince != null) request.Headers.IfModifiedSince = entry.Headers.IfModifiedSince.Value.UtcDateTime;
            if (entry.Headers.IfNoneExist != null) request.Headers.Add("If-None-Exist", entry.Headers.IfNoneExist);

            var interactionType = entry.Type;

            bool canHaveReturnPreference() => entry.Type == InteractionType.Create ||
              entry.Type == InteractionType.Update ||
              entry.Type == InteractionType.Patch;

            if (canHaveReturnPreference() && settings.PreferredReturn != null)
            {
                if (settings.PreferredReturn == Prefer.RespondAsync)
                    request.Headers.Add("Prefer", PrimitiveTypeConverter.ConvertTo<string>(settings.PreferredReturn));
                else
                    request.Headers.Add("Prefer", "return=" + PrimitiveTypeConverter.ConvertTo<string>(settings.PreferredReturn));
            }

            else if (interactionType == InteractionType.Search && settings.PreferredParameterHandling != null)
            {
                List<string> preferHeader = new List<string>();
                if (settings.PreferredParameterHandling.HasValue)
                    preferHeader.Add("handling=" + settings.PreferredParameterHandling.GetLiteral());
                if (settings.PreferredReturn.HasValue && settings.PreferredReturn == Prefer.RespondAsync)
                    preferHeader.Add(settings.PreferredReturn.GetLiteral());
                if (preferHeader.Count > 0)
                    request.Headers.Add("Prefer", string.Join(", ", preferHeader));
            }


            if (entry.RequestBodyContent != null)
                setContentAndContentType(request, entry.RequestBodyContent, entry.ContentType);

            return request;
        }

        /// <summary>
        /// Converts bundle http verb to corresponding <see cref="HttpMethod"/>.
        /// </summary>
        /// <param name="verb"><see cref="HTTPVerb"/> specified by input bundle.</param>
        /// <returns><see cref="HttpMethod"/> corresponding to verb specified in input bundle.</returns>
        private static HttpMethod getMethod(HTTPVerb? verb)
        {
            switch (verb)
            {
                case HTTPVerb.GET:
                    return HttpMethod.Get;
                case HTTPVerb.POST:
                    return HttpMethod.Post;
                case HTTPVerb.PUT:
                    return HttpMethod.Put;
                case HTTPVerb.DELETE:
                    return HttpMethod.Delete;
                case HTTPVerb.HEAD:
                    return HttpMethod.Head;
                case HTTPVerb.PATCH:
                    return new HttpMethod("PATCH");
            }
            throw new HttpRequestException($"Valid HttpVerb could not be found for verb type: [{verb}]");
        }

        private static void setContentAndContentType(HttpRequestMessage request, byte[] data, string contentType)
        {
            if (data == null) throw Error.ArgumentNull(nameof(data));
            request.Content = new ByteArrayContent(data);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
        }


        public static HttpWebRequest ToHttpWebRequest(this EntryRequest entry, Uri baseUrl, FhirClientSettings settings)
        {
            System.Diagnostics.Debug.WriteLine("{0}: {1}", (object)entry.Method, (object)entry.Url);

            if (entry.RequestBodyContent != null && !(entry.Method == HTTPVerb.POST || entry.Method == HTTPVerb.PUT || entry.Method == HTTPVerb.PATCH))
                throw Error.InvalidOperation((string)("Cannot have a body on an Http " + entry.Method.ToString()));

            // Create an absolute uri when the interaction.Url is relative.
            var uri = new Uri(entry.Url, UriKind.RelativeOrAbsolute);
            if (!uri.IsAbsoluteUri)
            {
                uri = HttpUtil.MakeAbsoluteToBase(uri, baseUrl);
            }

            var location = new RestUrl(uri);



            if (settings.UseFormatParameter)
                location.AddParam(HttpUtil.RESTPARAM_FORMAT, ContentType.BuildFormatParam(settings.PreferredFormat));

            var request = (HttpWebRequest)HttpWebRequest.Create(location.Uri);
            request.Method = entry.Method.ToString();
            setAgent(request, ".NET FhirClient for FHIR " + entry.Agent);

            if (!settings.UseFormatParameter)
                request.Accept = entry.Headers.Accept;

            request.ContentType = entry.ContentType;

            if (entry.Headers.IfMatch != null) request.Headers["If-Match"] = entry.Headers.IfMatch;
            if (entry.Headers.IfNoneMatch != null) request.Headers["If-None-Match"] = entry.Headers.IfNoneMatch;
#if NETSTANDARD1_6
            if (entry.Headers.IfModifiedSince != null) request.Headers["If-Modified-Since"] = entry.Headers.IfModifiedSince.Value.UtcDateTime.ToString();
#else
            if (entry.Headers.IfModifiedSince != null) request.IfModifiedSince = entry.Headers.IfModifiedSince.Value.UtcDateTime;

#endif
            if (entry.Headers.IfNoneExist != null) request.Headers["If-None-Exist"] = entry.Headers.IfNoneExist;

            if (canHaveReturnPreference() && settings.PreferredReturn.HasValue)
            {
                if (settings.PreferredReturn == Prefer.RespondAsync)
                    request.Headers["Prefer"] = PrimitiveTypeConverter.ConvertTo<string>(settings.PreferredReturn);
                else
                    request.Headers["Prefer"] = "return=" + PrimitiveTypeConverter.ConvertTo<string>(settings.PreferredReturn);
            }
            else if (entry.Type == InteractionType.Search)
            {
                List<string> preferHeader = new List<string>();
                if (settings.PreferredParameterHandling.HasValue)
                    preferHeader.Add("handling=" + settings.PreferredParameterHandling.GetLiteral());
                if (settings.PreferredReturn.HasValue && settings.PreferredReturn == Prefer.RespondAsync)
                    preferHeader.Add(settings.PreferredReturn.GetLiteral());
                if (preferHeader.Count > 0)
                    request.Headers["Prefer"] = string.Join(", ", preferHeader);
            }


            bool canHaveReturnPreference() => entry.Type == InteractionType.Create ||
                 entry.Type == InteractionType.Update ||
                 entry.Type == InteractionType.Patch;

            // PCL doesn't support setting the length (and in this case will be empty anyway)
#if !NETSTANDARD1_6
            if (entry.RequestBodyContent == null)
                request.ContentLength = 0;
#endif
            return request;
        }

        /// <summary>
        /// Flag to control the setting of the User Agent string (different platforms have different abilities)
        /// </summary>
        public static bool SetUserAgentUsingReflection = true;
        public static bool SetUserAgentUsingDirectHeaderManipulation = true;

        private static void setAgent(HttpWebRequest request, string agent)
        {
            bool userAgentSet = false;
            if (SetUserAgentUsingReflection)
            {
                try
                {
                    PropertyInfo prop = request.GetType().GetRuntimeProperty("UserAgent");

                    if (prop != null)
                        prop.SetValue(request, agent, null);
                    userAgentSet = true;
                }
                catch (Exception)
                {
                    // This approach doesn't work on this platform, so don't try it again.
                    SetUserAgentUsingReflection = false;
                }
            }
            if (!userAgentSet && SetUserAgentUsingDirectHeaderManipulation)
            {
                // platform does not support UserAgent property...too bad
                try
                {
#if NETSTANDARD1_6
                    request.Headers[HttpRequestHeader.UserAgent] = agent;
#else
                    request.UserAgent = agent;
#endif
                }
                catch (ArgumentException)
                {
                    SetUserAgentUsingDirectHeaderManipulation = false;
                    throw;
                }
            }
        }
    }
}
