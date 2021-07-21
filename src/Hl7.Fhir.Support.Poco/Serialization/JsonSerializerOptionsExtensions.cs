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
    public static class JsonSerializerOptionsExtensions
    {
        public static JsonSerializerOptions ForFhirCompact(this JsonSerializerOptions options, Assembly modelAssembly)
        {
            options.Converters.Add(new JsonFhirConverter(modelAssembly));
            options.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
            options.WriteIndented = false;

            return options;
        }

        public static JsonSerializerOptions ForFhirPretty(this JsonSerializerOptions options, Assembly modelAssembly)
        {
            options.Converters.Add(new JsonFhirConverter(modelAssembly));
            options.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
            options.WriteIndented = true;

            return options;
        }

        public static JsonWriterOptions ForFhirCompact(this JsonWriterOptions options)
        {
            options.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
            options.Indented = false;

            return options;
        }

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