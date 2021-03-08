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
using System.Linq;

namespace Hl7.Fhir.Specification
{


    public class TypeMemberDefinition : IAnnotated, IElementDefinitionSummary
    {
        private class ReferredTypeSummary : ITypeSerializationInfo
        {
            public ReferredTypeSummary(string referredType) => ReferredType = referredType;

            public string ReferredType { get; }
        }

        protected static readonly AnnotationList EMPTY_ANNOTATION_LIST = new AnnotationList();

        public TypeMemberDefinition(string name, TypeDefinition type, ComplexTypeDefinition declaringType, AnnotationList annotations)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Type = type ?? throw new ArgumentNullException(nameof(type));
            DeclaringType = declaringType ?? throw new ArgumentNullException(nameof(type));
            _annotations = annotations ?? EMPTY_ANNOTATION_LIST;
        }

        public string Name { get; }

        public ComplexTypeDefinition DeclaringType { get; }

        public TypeDefinition Type { get; }

        string IElementDefinitionSummary.ElementName => Name;

        bool IElementDefinitionSummary.IsCollection => Type is ListTypeDefinition;

        bool IElementDefinitionSummary.IsRequired =>
            this.TryGetAnnotation<FhirElementDefinitionAnnotation>(out var ann) ? ann.IsRequired : false;

        bool IElementDefinitionSummary.InSummary =>
            this.TryGetAnnotation<FhirElementDefinitionAnnotation>(out var ann) ? ann.InSummary : false;

        bool IElementDefinitionSummary.IsChoiceElement => Type is UnionTypeDefinition;

        bool IElementDefinitionSummary.IsResource => ((IStructureDefinitionSummary)Type).IsResource;

        ITypeSerializationInfo[] IElementDefinitionSummary.Type =>
             isBackboneElement() ? new[] { (ITypeSerializationInfo)Type } :
                Type is UnionTypeDefinition union ? union.MemberTypes.Cast<ITypeSerializationInfo>().ToArray() :
                    new[] { new ReferredTypeSummary(Type.Name) };

        private bool isBackboneElement() =>
            Type is ComplexTypeDefinition && Type.TryGetAnnotation<FhirStructureDefinitionAnnotation>(out var ann) && ann.IsBackboneElement;

        string IElementDefinitionSummary.DefaultTypeName =>
            this.TryGetAnnotation<Hl7v3XmlMemberAnnotation>(out var ann) ? ann.DefaultTypeName : null;

        string IElementDefinitionSummary.NonDefaultNamespace =>
            this.TryGetAnnotation<FhirXmlMemberAnnotation>(out var ann) ? ann.NonDefaultNamespace : null;

        XmlRepresentation IElementDefinitionSummary.Representation =>
              this.TryGetAnnotation<FhirXmlMemberAnnotation>(out var ann) ? ann.Representation : XmlRepresentation.XmlElement;

        int IElementDefinitionSummary.Order => DeclaringType.Members
            .Select((def, ix) => ((TypeMemberDefinition def, int ix)?)(def, ix))
            .Where(d => d.Value.def == this).SingleOrDefault().Value.ix;

        public IEnumerable<object> Annotations(Type type) => _annotations.OfType(type);
        private readonly AnnotationList _annotations;
    }

    public class FhirXmlMemberAnnotation
    {
        /// <inheritdoc cref="IElementDefinitionSummary.NonDefaultNamespace" />
        public string NonDefaultNamespace { get; set; }

        /// <inheritdoc cref="IElementDefinitionSummary.Representation" />
        public XmlRepresentation Representation { get; set; }
    }

    public class Hl7v3XmlMemberAnnotation
    {
        public string DefaultTypeName { get; set; }
    }

    public class FhirElementDefinitionAnnotation
    {
        public bool IsRequired { get; set; }

        public bool InSummary { get; set; }
    }
}
