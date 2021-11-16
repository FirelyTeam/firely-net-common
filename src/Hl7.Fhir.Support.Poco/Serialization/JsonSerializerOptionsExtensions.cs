/* 
 * Copyright (c) 2021, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-net-sdk/master/LICENSE
 */

#nullable enable

#if NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER

using System.Reflection;
using System.Text.Json;

namespace Hl7.Fhir.Serialization
{
    /// <summary>
    /// Utility extension method to initialize the <see cref="JsonSerializerOptions"/> to use the System.Text.Json
    /// based (de)serializers.
    /// </summary>
    public static class JsonSerializerOptionsExtensions
    {
        /// <summary>
        /// Initialize the options to serialize using the JsonFhirConverter, producing compact output without whitespace.
        /// </summary>
        public static JsonSerializerOptions ForFhir(this JsonSerializerOptions options, Assembly modelAssembly, SerializationFilter? filter = default)
        {
            var result = new JsonSerializerOptions(options)
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            result.Converters.Add(new FhirJsonConverter(modelAssembly, filter));
            return result;
        }

        /// <summary>
        /// Initialize the options to serialize using the JsonFhirConverter, producing compact output without whitespace.
        /// </summary>
        public static JsonSerializerOptions ForFhir(this JsonSerializerOptions options, FhirJsonConverter converter)
        {
            var result = new JsonSerializerOptions(options)
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            };

            result.Converters.Add(converter);
            return result;
        }

        /// <summary>
        /// Initialize the options to serialize using the JsonFhirConverter, producing compact output without whitespace.
        /// </summary>
        public static JsonSerializerOptions Compact(this JsonSerializerOptions options)
        {
            return new JsonSerializerOptions(options)
            {
                WriteIndented = false
            };
        }


        /// <summary>
        /// Initialize the options to serialize using the JsonFhirConverter, producing pretty output.
        /// </summary>
        public static JsonSerializerOptions Pretty(this JsonSerializerOptions options)
        {
            return new JsonSerializerOptions(options)
            {
                WriteIndented = true
            };
        }
    }
}

#endif
#nullable restore