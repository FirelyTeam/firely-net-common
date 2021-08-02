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
        public static JsonSerializerOptions ForFhirCompact(this JsonSerializerOptions options, Assembly modelAssembly)
        {
            options.Converters.Add(new JsonFhirConverter(modelAssembly));
            options.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
            options.WriteIndented = false;

            return options;
        }

        /// <summary>
        /// Initialize the options to serialize using the JsonFhirConverter, producing pretty output.
        /// </summary>
        public static JsonSerializerOptions ForFhirPretty(this JsonSerializerOptions options, Assembly modelAssembly)
        {
            options.Converters.Add(new JsonFhirConverter(modelAssembly));
            options.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
            options.WriteIndented = true;

            return options;
        }

        /// <summary>
        /// Initialize JsonWriterOptions to produce compact output without whitespace.
        /// </summary>
        public static JsonWriterOptions ForFhirCompact(this JsonWriterOptions options)
        {
            options.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
            options.Indented = false;

            return options;
        }

        /// <summary>
        /// Initialize JsonWriterOptions to produce pretty output.
        /// </summary>
        public static JsonWriterOptions ForFhirPretty(this JsonWriterOptions options)
        {
            options.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
            options.Indented = true;

            return options;
        }
    }
}

#endif
#nullable restore