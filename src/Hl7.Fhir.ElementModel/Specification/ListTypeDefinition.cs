/* 
 * Copyright (c) 2021, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/fhir-net-api/blob/master/LICENSE
 */

using Hl7.Fhir.Utility;
using System;

namespace Hl7.Fhir.Specification
{
    public class ListTypeDefinition : TypeDefinition
    {
        public ListTypeDefinition(string name, Lazy<TypeDefinition> @base, Lazy<TypeDefinition> elementType, Lazy<AnnotationList> annotations,
            bool isAbstract, bool isOrdered, string binding, string identifier) : base(name, @base, annotations, isAbstract, isOrdered, binding, identifier)
        {
            _delayedElementType = elementType ?? throw new ArgumentNullException(nameof(elementType));
        }

        public ListTypeDefinition(string name, Lazy<TypeDefinition> @base, Lazy<TypeDefinition> elementType)
            : base(name, @base)
        {
            _delayedElementType = elementType ?? throw new ArgumentNullException(nameof(elementType));
        }

        public TypeDefinition ElementType => _delayedElementType.Value;
        private readonly Lazy<TypeDefinition> _delayedElementType;
    }
}
