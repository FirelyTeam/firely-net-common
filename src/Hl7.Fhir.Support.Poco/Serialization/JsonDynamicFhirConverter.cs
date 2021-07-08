#if NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER

using System;
using System.Buffers;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Hl7.Fhir.Model;

namespace Hl7.Fhir.Serialization
{

    /// <summary>
    /// Common resource converter to support polymorphic deserialization.
    /// </summary>
    public class JsonDynamicFhirConverter : JsonConverter<Base>
    {
        public JsonDynamicFhirConverter(Assembly assembly)
        {
            _deserializer = new JsonDynamicDeserializer(assembly);
        }

        /// <summary>
        /// Determines whether the specified type can be converted.
        /// </summary>
        public override bool CanConvert(Type objectType) => typeof(Base).IsAssignableFrom(objectType);

        private JsonDynamicDeserializer _deserializer;


        /// <summary>
        /// Writes a specified value as JSON.
        /// </summary>
        public override void Write(Utf8JsonWriter writer, Base poco, JsonSerializerOptions options)
        {
            JsonSerializationExtensions.SerializeObject(poco, writer);
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