/* 
 * Copyright (c) 2019, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

using Hl7.Fhir.Validation.Schema;
using Newtonsoft.Json.Linq;
using System;

namespace Hl7.Fhir.Validation.Impl
{
    public class TraceText : IAssertion
    {
        public readonly string Message;

        public TraceText(string message)
        {
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }

        public JToken ToJson()
        {
            var trace = new JObject(new JProperty("message", Message));
            return new JProperty("trace", trace);
        }

    }
}
