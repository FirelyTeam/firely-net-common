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
    public delegate object Cast(object from);

    public abstract class TypeDefinition : IAnnotated
    {
        protected TypeDefinition(TypeDefinition? @base) : this(@base, annotations: null, isAbstract: false, isOrdered: false, casts: null)
        {
            // nothing
        }


        protected TypeDefinition(TypeDefinition? @base, AnnotationList? annotations,
            bool isAbstract, bool isOrdered, IDictionary<TypeDefinition, MethodInfo>? casts)
        {
            _annotations = annotations;
            Base = @base;
            IsAbstract = isAbstract;
            IsOrdered = isOrdered;
            Casts = casts;
        }

        public ModelDefinition? DeclaringModel { get; protected internal set; }

        public TypeDefinition? Base { get; private set; }

        public bool IsAbstract { get; }
        public bool IsOrdered { get; }

        public IDictionary<TypeDefinition, MethodInfo>? Casts { get; }

        private readonly AnnotationList? _annotations;

        /// <summary>
        /// Collection of additional model-specific information for this type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public IEnumerable<object> Annotations(Type type) => _annotations?.OfType(type) ?? Enumerable.Empty<object>();

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

        internal protected virtual void FixReferences(IDictionary<string, ModelDefinition> models)
        {
            if (Base is TypeDefinitionReference r) Base = r.Resolve(models);
        }
    }
}

#nullable disable