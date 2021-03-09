/* 
 * Copyright (c) 2021, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/fhir-net-api/blob/master/LICENSE
 */
#nullable enable

using System;
using System.Collections.Generic;

namespace Hl7.Fhir.Specification
{
    public class TypeDefinitionReference : TypeDefinition
    {
        public TypeDefinitionReference(string modelName, string typeName) : base(null) =>
            (ModelName, TypeName) = (modelName, typeName);

        public string ModelName { get; }
        public string TypeName { get; }

        public NamedTypeDefinition Resolve(IDictionary<string, ModelDefinition> models) =>
            models.TryGetValue(ModelName, out var model) && model.TryGetValue(TypeName, out var type)
                ? type
                : throw new InvalidOperationException($"Cannot resolve type {TypeName} from model {ModelName}.");
    }
}

#nullable disable