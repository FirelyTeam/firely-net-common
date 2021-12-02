/* 
 * Copyright (c) 2021, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-net-sdk/master/LICENSE
 */


using Hl7.Fhir.Introspection;

#nullable enable

namespace Hl7.Fhir.Serialization
{
    /// <summary>
    /// Contains contextual information for the property that is currently being deserialized and is passed 
    /// to delegate methods implementing parts of user-definable deserialization and validation logic.
    /// </summary>
    public readonly struct PropertyDeserializationContext
    {
        internal PropertyDeserializationContext(
            PathStack ps,
            string propertyName,
            PropertyMapping propMapping)
        {
            PathStack = ps;
            PropertyName = propertyName;
            ElementMapping = propMapping;
        }

        internal PathStack PathStack { get; }

        /// <inheritdoc cref="PathStack.GetPath"/>
        public string GetPath() => PathStack.GetPath();

        /// <summary>
        /// The property name for which an instance is currently being deserialized.
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        /// The metadata for the element that is currently being deserialized.
        /// </summary>
        public PropertyMapping ElementMapping { get; }
    }



    /// <summary>
    /// Contains contextual information for the instance that is currently being deserialized and is passed 
    /// to delegate methods implementing parts of user-definable deserialization and validation logic.
    /// </summary>
    public readonly struct InstanceDeserializationContext
    {
        internal InstanceDeserializationContext(
            PathStack ps,
            ClassMapping instanceMapping)
        {
            PathStack = ps;
            InstanceMapping = instanceMapping;
        }

        internal PathStack PathStack { get; }

        /// <inheritdoc cref="PathStack.GetPath"/>
        public string GetPath() => PathStack.GetPath();

        /// <summary>
        /// The metadata for the type of which the current property is part of.
        /// </summary>
        public ClassMapping InstanceMapping { get; }
    }
}

#nullable restore
