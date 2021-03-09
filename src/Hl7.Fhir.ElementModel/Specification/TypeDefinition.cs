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
    // TODO: Generate code in poco-modelinfo that references some of these using readonlies (e.g. ModelInfo.Patient, etc.)
    // TODO: Do the above for the system typespace too
    // TODO: Override equals and == to make sure it's not just reference equality
    public abstract class TypeDefinition : IAnnotated
    {
        public TypeDefinition(string name, TypeDefinition? @base) :
            this(name, @base, annotations: null, isAbstract: false, isOrdered: false,
                identifier: null, casts: null)
        {
            // nothing
        }

        public TypeDefinition(string name, TypeDefinition? @base, AnnotationList? annotations,
            bool isAbstract, bool isOrdered, string? identifier, IDictionary<TypeDefinition, MethodInfo>? casts)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Base = @base;
            _annotations = annotations;
            IsAbstract = isAbstract;
            IsOrdered = isOrdered;
            Identifier = identifier;
            Casts = casts;
        }

        /// <summary>
        /// The unique name for the type (within this model).
        /// </summary>
        public string Name { get; }

        public ModelDefinition? DeclaringModel { get; protected internal set; }

        public TypeDefinition? Base { get; private set; }

        public bool IsAbstract { get; }
        public bool IsOrdered { get; }

        /// <summary>
        /// A globally unique identifier, in FHIR this would be the canonical.
        /// </summary>
        public string? Identifier { get; }

        public IDictionary<TypeDefinition, MethodInfo>? Casts { get; }

        /// <summary>
        /// Collection of additional model-specific information for this type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public IEnumerable<object> Annotations(Type type) => _annotations?.OfType(type) ?? Enumerable.Empty<object>();
        private readonly AnnotationList? _annotations;

        public override string ToString() => Name;

        public string FullName => DeclaringModel?.Name + ":" + Name;

        internal protected virtual void FixReferences(IDictionary<string, ModelDefinition> models)
        {
            if (Base is TypeDefinitionReference r) Base = r.Resolve(models);
        }

        public bool TryGetCast(TypeDefinition to, out Cast? cast)
        {
            if (Casts is not null && Casts.TryGetValue(to, out MethodInfo mi))
            {
                cast = (Cast)mi.CreateDelegate(typeof(Cast));
                return true;
            }
            else
            {
                cast = default;
                return false;
            }
        }

    }

    public delegate object Cast(object from);
}

#nullable disable