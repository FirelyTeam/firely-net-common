/* 
 * Copyright (c) 2021, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/fhir-net-api/blob/master/LICENSE
 */

using Hl7.Fhir.Utility;
using System;
using System.Collections.Generic;

namespace Hl7.Fhir.Specification
{
    public class UnionTypeDefinition : TypeDefinition
    {
        public UnionTypeDefinition(string name, Lazy<TypeDefinition> @base, Lazy<IReadOnlyCollection<TypeDefinition>> members, Lazy<AnnotationList> annotations,
            bool isAbstract, bool isOrdered, string binding, string identifier) : base(name, @base, annotations, isAbstract, isOrdered, binding, identifier)
        {
            _delayedMemberTypes = members ?? throw new ArgumentNullException(nameof(members));
        }

        public UnionTypeDefinition(string name, Lazy<TypeDefinition> @base, Lazy<IReadOnlyCollection<TypeDefinition>> members)
            : base(name, @base)
        {
            _delayedMemberTypes = members ?? throw new ArgumentNullException(nameof(members));
        }

        public IReadOnlyCollection<TypeDefinition> MemberTypes => _delayedMemberTypes.Value;
        private readonly Lazy<IReadOnlyCollection<TypeDefinition>> _delayedMemberTypes;
    }
}
