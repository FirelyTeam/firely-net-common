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
    public class SystemModelDefinition : ModelDefinition
    {
        public static NamedTypeDefinition Any => throw new NotImplementedException();
        public static NamedTypeDefinition Boolean => throw new NotImplementedException();
        public static NamedTypeDefinition Code => throw new NotImplementedException();
        public static NamedTypeDefinition Concept => throw new NotImplementedException();
        public static NamedTypeDefinition Date => throw new NotImplementedException();
        public static NamedTypeDefinition DateTime => throw new NotImplementedException();
        public static NamedTypeDefinition Decimal => throw new NotImplementedException();
        public static NamedTypeDefinition Integer => throw new NotImplementedException();
        public static NamedTypeDefinition Integer64 => throw new NotImplementedException();
        public static NamedTypeDefinition Quantity => throw new NotImplementedException();
        public static NamedTypeDefinition String => throw new NotImplementedException();
        public static NamedTypeDefinition Time => throw new NotImplementedException();

 
        private SystemModelDefinition(string name, string version, IEnumerable<NamedTypeDefinition> types, DeferredModelInitializer<AnnotationList> annotationsInitializer) : base(name, version, types, annotationsInitializer)
        {
            throw new NotImplementedException();
        }
    }
}
