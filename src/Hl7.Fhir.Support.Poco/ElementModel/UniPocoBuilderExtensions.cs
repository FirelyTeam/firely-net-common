/* 
 * Copyright (c) 2018, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/fhir-net-api/blob/master/LICENSE
 */


using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Hl7.Fhir.Specification;
using System;

namespace Hl7.Fhir.ElementModel
{
    public static class UniPocoBuilderExtensions
    {
        public static Base ToPoco(this ISourceNode source, IStructureDefinitionSummaryProvider prov, Type pocoType = null, PocoBuilderSettings settings = null) =>
            new PocoBuilder(prov, settings).BuildFrom(source, pocoType);

        public static T ToPoco<T>(this ISourceNode source, IStructureDefinitionSummaryProvider prov, PocoBuilderSettings settings = null) where T : Base =>
               (T)source.ToPoco(prov, typeof(T), settings);

        public static Base ToPoco(this ITypedElement element, IStructureDefinitionSummaryProvider prov, PocoBuilderSettings settings = null) =>
            new PocoBuilder(prov, settings).BuildFrom(element);

        public static T ToPoco<T>(this ITypedElement element, IStructureDefinitionSummaryProvider prov, PocoBuilderSettings settings = null) where T : Base =>
               (T)element.ToPoco(prov,settings);
    }
}
