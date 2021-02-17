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
    public delegate T DeferredModelInitializer<T>(ModelSpace space);

    public class ModelSpace
    {
        public IReadOnlyCollection<ModelDefinition> Models => _models;
        private readonly List<ModelDefinition> _models = new List<ModelDefinition>();

        // todo private list of JITted list definitions

        public bool TryGetModel(string modelName, string modelVersion, out ModelDefinition model)
        {
            model = Models.SingleOrDefault(m => m.Name == modelName && m.Version == modelVersion);
            return model != null;
        }

        public bool TryGetType(string modelName, string modelVersion, string typeName, out TypeDefinition definition)
        {

            if (TryGetModel(modelName, modelVersion, out var model))
                return model.TryGetType(typeName, out definition);

            definition = null;
            return false;
        }

        public bool TryGetType(ModelDefinition model, string typeName, out TypeDefinition definition) =>
            model.TryGetType(typeName, out definition);

        public TypeDefinition GetListTypeDefinition(TypeDefinition elementType)
        {
            throw new NotImplementedException();
        }

        public TypeDefinition GetUnionTypeDefinition(TypeDefinition elementType)
        {
            throw new NotImplementedException();
        }

        public void Add(ModelDefinition definition)
        {
            if (TryGetModel(definition.Name, definition.Version, out _))
                throw new ArgumentException($"ModelSpace already contains a model with name '{definition.Name}' and version '{definition.Version}'.");

            definition.DeclaringSpace = this;
            _models.Add(definition);
        }
    }


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

    public class ListTypeDefinition : TypeDefinition
    {
        public ListTypeDefinition(string name, Lazy<TypeDefinition> @base, Lazy<TypeDefinition> elementType, Lazy<AnnotationList> annotations,
            bool isAbstract, bool isOrdered, string binding, string identifier) : base(name, @base, annotations, isAbstract, isOrdered, binding, identifier)
        {
            _delayedElementType = elementType ?? throw new ArgumentNullException(nameof(elementType));
        }

        public ListTypeDefinition(string name, Lazy<TypeDefinition> @base, Lazy<TypeDefinition> elementType)
            : base(name, @base)
        {
            _delayedElementType = elementType ?? throw new ArgumentNullException(nameof(elementType));
        }

        public TypeDefinition ElementType => _delayedElementType.Value;
        private readonly Lazy<TypeDefinition> _delayedElementType;
    }
}
