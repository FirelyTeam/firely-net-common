/* 
 * Copyright (c) 2021, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-net-sdk/master/LICENSE
 */

#nullable enable

#if NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER

using System.Collections.Generic;
using System.Text.Json;

namespace Hl7.Fhir.Serialization
{
    public static class JsonFhirDictionarySerializerExtensions
    {
        /// <summary>
        /// Serializes the given dictionary with FHIR data into Json.
        /// </summary>
        public static void SerializeToFhirJson(this IReadOnlyDictionary<string, object> members, Utf8JsonWriter writer) =>
            JsonFhirDictionarySerializer.Default.Serialize(members, writer);
    }
}

#endif
#nullable restore
