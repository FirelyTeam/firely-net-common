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
using System.Linq;
using System.Reflection;

namespace Hl7.Fhir.Specification
{
    public class ComplexTypeDefinition : TypeDefinition, IStructureDefinitionSummary
    {
        public ComplexTypeDefinition(string name, TypeDefinition? @base, IReadOnlyList<TypeMemberDefinition> members)
            : base(name, @base)
        {
            Members = members ?? throw new ArgumentNullException(nameof(members));
        }

        public ComplexTypeDefinition(string name, TypeDefinition? @base, IReadOnlyList<TypeMemberDefinition>? members,
            AnnotationList? annotations,
            bool isAbstract, bool isOrdered, string? identifier, IDictionary<TypeDefinition, MethodInfo>? casts) : base(name, @base, annotations, isAbstract, isOrdered, identifier, casts)
        {
            Members = members ?? throw new ArgumentNullException(nameof(members));
        }
        public IReadOnlyList<TypeMemberDefinition> Members { get; private set; }

        string IStructureDefinitionSummary.TypeName => Name;

        bool IStructureDefinitionSummary.IsAbstract => IsAbstract;

        bool IStructureDefinitionSummary.IsResource =>
            this.TryGetAnnotation<FhirStructureDefinitionAnnotation>(out var ann) && ann.IsResource;

        IReadOnlyCollection<IElementDefinitionSummary> IStructureDefinitionSummary.GetElements() =>
            Members.Where(m => !isFhirPrimitiveValueMember(m)).ToList();

        static bool isFhirPrimitiveValueMember(TypeMemberDefinition td) =>
            td.Type is PrimitiveTypeDefinition && td.Name == "value";

        protected internal override void FixReferences(IDictionary<string, ModelDefinition> models)
        {
            base.FixReferences(models);

            foreach (var member in Members) member.FixReferences(models);
        }
    }

    public class FhirStructureDefinitionAnnotation
    {
        public bool IsResource { get; set; }
        public bool IsBackboneElement { get; set; }
    }
}

#nullable disable