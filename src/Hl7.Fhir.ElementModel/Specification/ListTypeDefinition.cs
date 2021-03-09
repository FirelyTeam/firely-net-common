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
        public ListTypeDefinition(string name, TypeDefinition @base, TypeDefinition elementType, AnnotationList? annotations,
            bool isAbstract, bool isOrdered, string identifier, IDictionary<TypeDefinition, MethodInfo>? casts) :
            base(name, @base, annotations, isAbstract, isOrdered, identifier, casts)
        {
            ElementType = elementType ?? throw new ArgumentNullException(nameof(elementType));
        }

        public ListTypeDefinition(string name, TypeDefinition @base, TypeDefinition elementType)
            : base(name, @base)
        {
            ElementType = elementType ?? throw new ArgumentNullException(nameof(elementType));
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