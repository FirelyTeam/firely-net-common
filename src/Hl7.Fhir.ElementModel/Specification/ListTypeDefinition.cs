/* 
 * Copyright (c) 2021, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/fhir-net-api/blob/master/LICENSE
 */
#nullable enable

using Hl7.Fhir.Utility;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Hl7.Fhir.Specification
{
    public class ListTypeDefinition : TypeDefinition
    {
        public ListTypeDefinition(TypeDefinition @base, TypeDefinition elementType, AnnotationList? annotations,
            IDictionary<TypeDefinition, MethodInfo>? casts) :
            base(@base, annotations, isAbstract: false, isOrdered: false, casts)
        {
            ElementType = elementType ?? throw new ArgumentNullException(nameof(elementType));
        }

        public ListTypeDefinition(TypeDefinition @base, TypeDefinition elementType)
            : this(@base, elementType, annotations: null, casts: null)
        {
            // nothing
        }

        public TypeDefinition ElementType { get; private set; }

        protected internal override void FixReferences(IDictionary<string, ModelDefinition> models)
        {
            base.FixReferences(models);

            if (ElementType is TypeDefinitionReference r) ElementType = r.Resolve(models);
        }
    }
}

#nullable disable