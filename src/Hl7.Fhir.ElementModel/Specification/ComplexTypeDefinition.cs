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
    public class ComplexTypeDefinition : TypeDefinition
    {
        public ComplexTypeDefinition(string name, Lazy<TypeDefinition> @base, Lazy<IReadOnlyList<TypeMemberDefinition>> members) : base(name, @base)
        {
            _delayedMembers = members ?? throw new ArgumentNullException(nameof(members));
        }

        public ComplexTypeDefinition(string name, Lazy<TypeDefinition> @base, Lazy<IReadOnlyList<TypeMemberDefinition>> members,
            Lazy<AnnotationList> annotations,
            bool isAbstract, bool isOrdered, string binding, string identifier) : base(name, @base, annotations, isAbstract, isOrdered, binding, identifier)
        {
            _delayedMembers = members ?? throw new ArgumentNullException(nameof(members));
        }

        public IReadOnlyList<TypeMemberDefinition> Members => _delayedMembers.Value;

        private readonly Lazy<IReadOnlyList<TypeMemberDefinition>> _delayedMembers;
    }
}
