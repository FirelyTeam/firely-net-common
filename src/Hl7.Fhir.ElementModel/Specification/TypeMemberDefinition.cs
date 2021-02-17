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
    public class TypeMemberDefinition : IAnnotated
    {
        protected static readonly AnnotationList EMPTY_ANNOTATION_LIST = new AnnotationList();

        public TypeMemberDefinition(string name, TypeDefinition type, AnnotationList annotations)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Type = type ?? throw new ArgumentNullException(nameof(type));
            _annotations = annotations ?? EMPTY_ANNOTATION_LIST;
        }

        public string Name { get; }

        public TypeDefinition Type { get; }

        public IEnumerable<object> Annotations(Type type) => _annotations.OfType(type);
        private readonly AnnotationList _annotations;
    }
}
