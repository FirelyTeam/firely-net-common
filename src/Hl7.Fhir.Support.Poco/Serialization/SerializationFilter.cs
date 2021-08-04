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
using System;

namespace Hl7.Fhir.Serialization
{
    /// <summary>
    /// A filter that instructs the serializers which parts of a tree to serialize.
    /// </summary>
    /// <remarks>An instance of a filter is passed to the <see cref="JsonFhirDictionarySerializer.Serialize(System.Collections.Generic.IReadOnlyDictionary{string, object}, System.Text.Json.Utf8JsonWriter, SerializationFilter?)"/>
    /// functions.
    /// </remarks>
    public abstract class SerializationFilter
    {
        /// <summary>
        /// The serializer calls this function when it starts serializing a complex object.
        /// </summary>
        public abstract void EnterObject(object value, ClassMapping? mapping);

        /// <summary>
        /// The serializer calls this function when it needs to serialize the subtree contained in an element.
        /// When this function return false, the subtree will not be serialized.
        /// </summary>
        public abstract bool TryEnterMember(string name, object value, PropertyMapping? mapping);

        /// <summary>
        /// The serializer calls this function when it is done serializing the subtree for an element.
        /// </summary>
        public abstract void LeaveMember(string name, object value, PropertyMapping? mapping);

        /// <summary>
        /// The serializer calls this function when it is done serializing a complex object.
        /// </summary>
        public abstract void LeaveObject(object value, ClassMapping? mapping);

        public static SerializationFilter ForSummary => new BundleFilter(new ElementMetadataFilter() { IncludeInSummary = true });

        public static SerializationFilter ForText() => new BundleFilter(new TopLevelFilter(
            new ElementMetadataFilter()
            {
                IncludeNames = new[] { "text", "id", "meta" },
                IncludeMandatory = true
            }));

        public static SerializationFilter ForElements(string[] elements) => new BundleFilter(new TopLevelFilter(
          new ElementMetadataFilter()
          {
              IncludeNames = elements,
              IncludeMandatory = true
          }));
    }
}

#endif
#nullable restore
