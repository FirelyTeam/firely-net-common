/* 
 * Copyright (c) 2021, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-net-sdk/master/LICENSE
 */


#if NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER

#nullable enable

using Hl7.Fhir.Model;
using Hl7.Fhir.Validation;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Hl7.Fhir.Serialization
{
    /// <summary>
    /// Specify the optional features for Json deserialization.
    /// </summary>
    public record FhirJsonPocoDeserializerSettings
    {
        /// <summary>
        /// If the caller will not access base64 data in the deserialized resources, base64 decoding
        /// of <see cref="Base64Binary"/> values can be turned off to increase performance.
        /// </summary>
        /// <remarks>The <see cref="Base64Binary" /> element's <see cref="PrimitiveType.ObjectValue" /> will
        /// still contain the unparsed base64 data and will therefore be retained and round-tripped.</remarks>
        public bool DisableBase64Decoding { get; init; } = false;

        /// <summary>
        /// If set, this delegate is called when the deserializer fails to parse a primitive json value.
        /// </summary>
        public PrimitiveParseHandler? OnPrimitiveParseFailed { get; init; } = null;

        /// <summary>
        /// If set, this validator is invoked before the value is set in the object under construction to validate
        /// and possibly alter the value. Setting this property to <c>null</c> will disable validation completely.
        /// </summary>
        public IDeserializationValidator? Validator { get; init; } = DataAnnotationDeserialzationValidator.Default;
    }

    /// <summary>
    /// A callback that can handle parsing failures for primitive types.
    /// </summary>
    /// <param name="reader">A json reader positioned on the primitive value that failed to parse.</param>
    /// <param name="originalException">The exception that the deserializer would have raised if this handler was not installed.</param>
    /// <param name="targetType">The .NET type the deserializer needs this handler to return to be able to update the POCO under construction.</param>
    /// <remarks>Returns an object if this handler succeeded in parsing, otherwise the delegate must throw an exception (which might
    /// be the <c>originalException</c>).</remarks>
    public delegate object? PrimitiveParseHandler(ref Utf8JsonReader reader, Type targetType, FhirJsonException originalException);
}

#nullable restore
#endif