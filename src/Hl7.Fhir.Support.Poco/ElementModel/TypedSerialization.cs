/* 
 * Copyright (c) 2021, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/firely-net-sdk/blob/master/LICENSE
 */

using Hl7.Fhir.Introspection;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace Hl7.Fhir.ElementModel
{
    public static class TypedSerialization
    {
        private static readonly ConcurrentDictionary<string, ModelInspector> _inspectedAssemblies = new();

        public static ModelInspector GetInspectorForAssembly(Assembly a)
        {
            return _inspectedAssemblies.GetOrAdd(a.FullName, _ => configureInspector(a));

            static ModelInspector configureInspector(Assembly a)
            {
                var commonAssembly = typeof(Resource).GetTypeInfo().Assembly;

                if (a.FullName == commonAssembly.FullName)
                {
                    // If all we need is the common POCOs, assume we are using
                    // the latest version of FHIR.
                    var newInspector = new ModelInspector(Specification.FhirRelease.R5);
                    newInspector.Import(commonAssembly);
                    return newInspector;
                }
                else
                {
                    // TODO: We can obtain the FhirRelease from the assembly passed in
                    var newInspector = new ModelInspector(Specification.FhirRelease.STU3);
                    newInspector.Import(a);
                    newInspector.Import(commonAssembly);
                    return newInspector;
                }
            }
        }

        public static Base ToPoco(ISourceNode source, Type pocoType, PocoBuilderSettings settings = null) =>
            new PocoBuilder(GetInspectorForAssembly(pocoType.GetTypeInfo().Assembly), settings)
            .BuildFrom(source, pocoType ?? throw new ArgumentNullException(nameof(pocoType)));

        public static T ToPoco<T>(ISourceNode source, PocoBuilderSettings settings = null) where T : Base =>
            (T)ToPoco(source, typeof(T), settings);

        public static Base ToPoco(ITypedElement element, Type pocoType, PocoBuilderSettings settings = null) =>
            new PocoBuilder(GetInspectorForAssembly(pocoType.GetTypeInfo().Assembly), settings)
            .BuildFrom(element);

        public static T ToPoco<T>(ITypedElement element, PocoBuilderSettings settings = null) where T : Base =>
           (T)ToPoco(element, typeof(T), settings);

        public static ITypedElement ToTypedElement(Base @base, string rootName = null) =>
            new PocoElementNode(GetInspectorForAssembly(@base.GetType().GetTypeInfo().Assembly), @base, rootName: rootName);
    }
}
