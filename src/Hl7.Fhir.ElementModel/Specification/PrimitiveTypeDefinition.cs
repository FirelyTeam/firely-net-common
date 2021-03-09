/* 
 * Copyright (c) 2021, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/fhir-net-api/blob/master/LICENSE
 */
#nullable enable

using Hl7.Fhir.Utility;
using System.Collections.Generic;
using System.Reflection;

namespace Hl7.Fhir.Specification
{
    public class PrimitiveTypeDefinition : NamedTypeDefinition
    {
        public PrimitiveTypeDefinition(string name, TypeDefinition @base) : base(name, @base)
        {
        }

        public PrimitiveTypeDefinition(string name, TypeDefinition? @base, AnnotationList? annotations,
            bool isAbstract, bool isOrdered, string? identifier, IDictionary<TypeDefinition, MethodInfo>? casts)
            : base(name, @base, annotations, isAbstract, isOrdered, identifier, casts)
        {
        }
    }
}

#nullable disable