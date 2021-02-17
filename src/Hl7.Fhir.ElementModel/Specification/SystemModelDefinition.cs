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
        public static TypeDefinition Any => throw new NotImplementedException();
        public static TypeDefinition Boolean => throw new NotImplementedException();
        public static TypeDefinition Code => throw new NotImplementedException();
        public static TypeDefinition Concept => throw new NotImplementedException();
        public static TypeDefinition Date => throw new NotImplementedException();
        public static TypeDefinition DateTime => throw new NotImplementedException();
        public static TypeDefinition Decimal => throw new NotImplementedException();
        public static TypeDefinition Integer => throw new NotImplementedException();
        public static TypeDefinition Integer64 => throw new NotImplementedException();
        public static TypeDefinition Quantity => throw new NotImplementedException();
        public static TypeDefinition String => throw new NotImplementedException();
        public static TypeDefinition Time => throw new NotImplementedException();

 
        private SystemModelDefinition(string name, string version, IEnumerable<TypeDefinition> types, DeferredModelInitializer<AnnotationList> annotationsInitializer) : base(name, version, types, annotationsInitializer)
        {
            throw new NotImplementedException();
        }
    }
}
