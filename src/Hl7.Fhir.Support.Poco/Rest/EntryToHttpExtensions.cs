/* 
 * Copyright (c) 2014, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

using Hl7.Fhir.Serialization;
using Hl7.Fhir.Utility;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Hl7.Fhir.Rest
{
    internal static class EntryToHttpExtensions
    {
        public static HttpWebRequest ToHttpRequest(this EntryRequest entry, Uri baseUrl, FhirClientSettings settings)
        {
            System.Diagnostics.Debug.WriteLine("{0}: {1}", entry.Method, entry.Url);

            var interaction = entry;

            if (entry.RequestBodyContent != null && !(interaction.Method == HTTPVerb.POST || interaction.Method == HTTPVerb.PUT))
                throw Error.InvalidOperation("Cannot have a body on an Http " + interaction.Method.ToString());

            // Create an absolute uri when the interaction.Url is relative.
            var uri = new Uri(interaction.Url, UriKind.RelativeOrAbsolute);
            if (!uri.IsAbsoluteUri)
            {
                uri = HttpUtil.MakeAbsoluteToBase(uri, baseUrl);
            }

            var location = new RestUrl(uri);

            if (settings.UseFormatParameter)
                location.AddParam(HttpUtil.RESTPARAM_FORMAT, Hl7.Fhir.Rest.ContentType.BuildFormatParam(settings.PreferredFormat));

            var request = (HttpWebRequest)HttpWebRequest.Create(location.Uri);
            request.Method = interaction.Method.ToString();
            setAgent(request, ".NET FhirClient for FHIR " + entry.Agent);

            if (!settings.UseFormatParameter)
                request.Accept = Hl7.Fhir.Rest.ContentType.BuildContentType(settings.PreferredFormat, forBundle: false);

            if (interaction.Headers.IfMatch != null) request.Headers["If-Match"] = interaction.Headers.IfMatch;
            if (interaction.Headers.IfNoneMatch != null) request.Headers["If-None-Match"] = interaction.Headers.IfNoneMatch;
#if NETSTANDARD1_1
            if (interaction.Headers.IfModifiedSince != null) request.Headers["If-Modified-Since"] = interaction.Headers.IfModifiedSince.Value.UtcDateTime.ToString();
#else
            if (interaction.Headers.IfModifiedSince != null) request.IfModifiedSince = interaction.Headers.IfModifiedSince.Value.UtcDateTime;

#endif
            if (interaction.Headers.IfNoneExist != null) request.Headers["If-None-Exist"] = interaction.Headers.IfNoneExist;

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
                    request.Headers["Prefer"] = String.Join(", ", preferHeader);
            }

            bool canHaveReturnPreference() => entry.Type == InteractionType.Create ||
                 entry.Type == InteractionType.Update ||
                 entry.Type == InteractionType.Patch;
            
            // PCL doesn't support setting the length (and in this case will be empty anyway)
#if !NETSTANDARD1_1
            if(entry.RequestBodyContent == null)
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
					System.Reflection.PropertyInfo prop = request.GetType().GetRuntimeProperty("UserAgent");

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
                    request.Headers[HttpRequestHeader.UserAgent] = agent;
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
