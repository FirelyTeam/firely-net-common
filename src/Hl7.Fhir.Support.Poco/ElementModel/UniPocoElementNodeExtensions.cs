/* 
 * Copyright (c) 2016, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

using Hl7.Fhir.Model;
using Hl7.Fhir.Specification;

namespace Hl7.Fhir.ElementModel
{
    public static class UniPocoElementNodeExtensions
    {
        public static ITypedElement ToTypedElement(this Base @base, IStructureDefinitionSummaryProvider prov, string rootName = null) =>
            new PocoElementNode(prov, @base, rootName: rootName);
     

    }
}