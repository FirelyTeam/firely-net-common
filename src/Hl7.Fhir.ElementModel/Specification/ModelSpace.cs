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

        public bool TryGetType(string modelName, string modelVersion, string typeName, out NamedTypeDefinition definition)
        {

            if (TryGetModel(modelName, modelVersion, out var model))
                return model.TryGetType(typeName, out definition);

            definition = null;
            return false;
        }

        public bool TryGetType(ModelDefinition model, string typeName, out NamedTypeDefinition definition) =>
            model.TryGetType(typeName, out definition);

        public NamedTypeDefinition GetListTypeDefinition(NamedTypeDefinition elementType)
        {
            throw new NotImplementedException();
        }

        public NamedTypeDefinition GetUnionTypeDefinition(NamedTypeDefinition elementType)
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
}
