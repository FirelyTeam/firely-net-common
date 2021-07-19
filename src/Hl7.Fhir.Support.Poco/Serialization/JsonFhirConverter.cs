#if NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER

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
        public JsonFhirConverter(Assembly assembly)
        {
            _deserializer = new JsonDynamicDeserializer(assembly);
            _serializer = new JsonDictionarySerializer();
        }

        /// <summary>
        /// Determines whether the specified type can be converted.
        /// </summary>
        public override bool CanConvert(Type objectType) => typeof(Base).IsAssignableFrom(objectType);

        private readonly JsonDynamicDeserializer _deserializer;
        private readonly JsonDictionarySerializer _serializer;

        /// <summary>
        /// Writes a specified value as JSON.
        /// </summary>
        public override void Write(Utf8JsonWriter writer, Base poco, JsonSerializerOptions options)
        {
            _serializer.SerializeObject(poco, writer);
        }

        /// <summary>
        /// Reads and converts the JSON to a typed object.
        /// </summary>
        public override Base Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (typeof(Resource).IsAssignableFrom(typeToConvert))
                return _deserializer.DeserializeResource(ref reader);
            else
                return _deserializer.DeserializeObject(typeToConvert, ref reader);
        }
    }
}

#endif