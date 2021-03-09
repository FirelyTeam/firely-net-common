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
    // TODO: Generate code in poco-modelinfo that references some of these using readonlies (e.g. ModelInfo.Patient, etc.)
    // TODO: Do the above for the system typespace too
    // TODO: Override equals and == to make sure it's not just reference equality
    public abstract class NamedTypeDefinition : TypeDefinition
    {
        public NamedTypeDefinition(string name, TypeDefinition? @base) :
            this(name, @base, annotations: null, isAbstract: false, isOrdered: false,
                identifier: null, casts: null)
        {
            // nothing
        }

        public NamedTypeDefinition(string name, TypeDefinition? @base, AnnotationList? annotations,
            bool isAbstract, bool isOrdered, string? identifier, IDictionary<TypeDefinition, MethodInfo>? casts)
            : base(@base, annotations, isAbstract, isOrdered, casts)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Identifier = identifier;
        }

        /// <summary>
        /// The unique name for the type (within this model).
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// A globally unique identifier, in FHIR this would be the canonical.
        /// </summary>
        public string? Identifier { get; }

        public override string ToString() => Name;

        public string FullName => DeclaringModel?.Name + ":" + Name;
    }
}

#nullable disable