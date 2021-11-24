/* 
 * Copyright (c) 2021, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-net-sdk/master/LICENSE
 */


using Hl7.Fhir.Introspection;
using System;

#nullable enable

namespace Hl7.Fhir.Serialization
{
    /// <summary>
    /// Contains information of the location in the POCO that is currently being deserialized and is passed 
    /// to delegate methods implementing parts of user-definable deserialization and validation logic.
    /// </summary>
    public readonly struct DeserializationContext
    {
        internal DeserializationContext(
            PathStack ps,
            string propertyName,
            ClassMapping targetMapping,
            PropertyMapping propMapping,
            Type valueType
            )
        {
            PathStack = ps;
            PropertyName = propertyName;
            TargetObjectMapping = targetMapping;
            ElementMapping = propMapping;
            ValueType = valueType;
        }

        internal PathStack PathStack { get; }

        /// <inheritdoc cref="PathStack.GetPath"/>
        public string GetPath() => PathStack.GetPath();

        /// <summary>
        /// The metadata for the type of which the current property is part of.
        /// </summary>
        public ClassMapping TargetObjectMapping { get; }

        /// <summary>
        /// The property name for which an instance is currently being deserialized.
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        /// The metadata for the element that is currently being deserialized.
        /// </summary>
        public PropertyMapping ElementMapping { get; }

        /// <summary>
        /// The type of the instance currently being deserialized.
        /// </summary>
        public Type ValueType { get; }
    }
}

#nullable restore
