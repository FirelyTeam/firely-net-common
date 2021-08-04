/* 
 * Copyright (c) 2021, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-net-sdk/master/LICENSE
 */

#nullable enable

#if NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER

using Hl7.Fhir.Introspection;
using Hl7.Fhir.Model;
using System;
using System.Buffers;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hl7.Fhir.Serialization
{
    /// <summary>
    /// Common resource converter to support polymorphic deserialization.
    /// </summary>
    public class JsonFhirConverter : JsonConverter<Base>
    {
#pragma warning disable IDE0060 // Will become used when we add deserialization.
        public JsonFhirConverter(Assembly assembly, ElementFilter? filter = default)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            ModelInspector inspector = ModelInspector.ForAssembly(assembly);

            // _deserializer = new JsonDynamicDeserializer(assembly);
            _serializer = new JsonFhirDictionarySerializer(inspector.FhirRelease, filter);
        }

        /// <summary>
        /// Determines whether the specified type can be converted.
        /// </summary>
        public override bool CanConvert(Type objectType) => typeof(Base).IsAssignableFrom(objectType);

        //private readonly JsonDynamicDeserializer _deserializer;
        private readonly JsonFhirDictionarySerializer _serializer;

        /// <summary>
        /// Writes a specified value as JSON.
        /// </summary>
        public override void Write(Utf8JsonWriter writer, Base poco, JsonSerializerOptions options)
        {
            _serializer.Serialize(poco, writer);
        }

        /// <summary>
        /// Reads and converts the JSON to a typed object.
        /// </summary>
        public override Base Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
            //if (typeof(Resource).IsAssignableFrom(typeToConvert))
            //    return _deserializer.DeserializeResource(ref reader);
            //else
            //    return _deserializer.DeserializeObject(typeToConvert, ref reader);
        }
    }
}

#endif
#nullable restore