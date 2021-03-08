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
    // TODO: Generate code in poco-modelinfo that references some of these using readonlies (e.g. ModelInfo.Patient, etc.)
    // TODO: Do the above for the system typespace too
    // TODO: Override equals and == to make sure it's not just reference equality
    public abstract class TypeDefinition : IAnnotated
    {
        protected static readonly Lazy<AnnotationList> EMPTY_ANNOTATION_LIST = new Lazy<AnnotationList>(() => new AnnotationList());

        public TypeDefinition(string name, Lazy<TypeDefinition> @base) :
            this(name, @base, annotations: null, isAbstract: false, isOrdered: false, binding: null, identifier: null)
        {

        }

        public TypeDefinition(string name, Lazy<TypeDefinition> @base, Lazy<AnnotationList> annotations,
            bool isAbstract, bool isOrdered, string binding, string identifier)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            _delayedBase = @base;
            _delayedAnnotations = annotations ?? EMPTY_ANNOTATION_LIST;
            IsAbstract = isAbstract;
            IsOrdered = isOrdered;
            Binding = binding;
            Identifier = identifier;
        }

        /// <summary>
        /// The unique name for the type (within this model).
        /// </summary>
        public string Name { get; }

        public ModelDefinition DeclaringModel { get; protected internal set; }

        public TypeDefinition Base => _delayedBase?.Value;
        private readonly Lazy<TypeDefinition> _delayedBase;

        public bool IsAbstract { get; }
        public bool IsOrdered { get; }
        public string Binding { get; }

        /// <summary>
        /// A globally unique identifier, in FHIR this would be the canonical.
        /// </summary>
        public string Identifier { get; }

        /// <summary>
        /// Collection of additional model-specific information for this type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public IEnumerable<object> Annotations(Type type) => _delayedAnnotations.Value.OfType(type);
        private readonly Lazy<AnnotationList> _delayedAnnotations;

        // TODO: Add FullName() (which includes model name?)
        public override string ToString() => Name;

        public bool TryGetCast(TypeDefinition to, out Cast<TypeDefinition> cast) => throw new NotImplementedException();
        public bool TryGetCast<T>(out Cast<T, TypeDefinition> cast) => throw new NotImplementedException();
    }

    public delegate object Cast<F>(F from);
    public delegate T Cast<F, T>(F from);
}
