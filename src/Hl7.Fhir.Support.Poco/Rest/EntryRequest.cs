/* 
 * Copyright (c) 2014, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

using Hl7.Fhir.Utility;
using System;

namespace Hl7.Fhir.Rest
{
    public class EntryRequest
    {
        public HTTPVerb? Method { get; set; }
        public string Url { get; set; }
        public string ContentType { get; set; }
        public byte[] RequestBodyContent { get; set; }
        public EntryRequestHeaders Headers { get; set; }
        public InteractionType Type { get; set; }
        public string Agent { get; set; }
    }
    
    public class EntryRequestHeaders
    {
        public string IfMatch { get; set; }
        public string IfNoneMatch { get; set; }
        public string IfNoneExist { get; set; }
        public DateTimeOffset? IfModifiedSince { get; set; }
    }

    //Needs to be in sync with Bundle.HTTPVerbs
    public enum HTTPVerb
    {
        [EnumLiteral("GET", "http://hl7.org/fhir/http-verb"), Description("GET")]
        GET,
        [EnumLiteral("HEAD", "http://hl7.org/fhir/http-verb"), Description("HEAD")]
        HEAD,
        [EnumLiteral("POST", "http://hl7.org/fhir/http-verb"), Description("POST")]
        POST,
        [EnumLiteral("PUT", "http://hl7.org/fhir/http-verb"), Description("PUT")]
        PUT,
        [EnumLiteral("DELETE", "http://hl7.org/fhir/http-verb"), Description("DELETE")]
        DELETE,
        [EnumLiteral("PATH", "http://hl7.org/fhir/http-verb"), Description("PATH")]
        PATCH,
    }
    
    public enum InteractionType
    {
        Search,
        Unspecified,
        Read,
        VRead,
        Update,
        Delete,
        Create,
        Capabilities,
        History,
        Operation,
        Transaction,
        Patch
    }
}
