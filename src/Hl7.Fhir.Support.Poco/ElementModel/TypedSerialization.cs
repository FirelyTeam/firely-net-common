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
using System.Reflection;

namespace Hl7.Fhir.ElementModel
{
    public static class TypedSerialization
    {
        public static Base ToPoco(ISourceNode source, ModelInspector inspector, PocoBuilderSettings settings = null) =>
            new PocoBuilder(inspector, settings)
            .BuildFrom(source, (Type)null);

        public static Base ToPoco(ISourceNode source, Type pocoType, PocoBuilderSettings settings = null) =>
            new PocoBuilder(ModelInspector.ForAssembly(pocoType.GetTypeInfo().Assembly), settings)
            .BuildFrom(source, pocoType ?? throw new ArgumentNullException(nameof(pocoType)));

        public static T ToPoco<T>(ISourceNode source, PocoBuilderSettings settings = null) where T : Base =>
            (T)ToPoco(source, typeof(T), settings);

        public static Base ToPoco(ITypedElement element, Type pocoType, PocoBuilderSettings settings = null) =>
            new PocoBuilder(ModelInspector.ForAssembly(pocoType.GetTypeInfo().Assembly), settings)
            .BuildFrom(element);

        public static T ToPoco<T>(ITypedElement element, PocoBuilderSettings settings = null) where T : Base =>
           (T)ToPoco(element, typeof(T), settings);

        public static ITypedElement ToTypedElement(Base @base, string rootName = null) =>
            new PocoElementNode(ModelInspector.ForAssembly(@base.GetType().GetTypeInfo().Assembly), @base, rootName: rootName);

        public static ISourceNode ToSourceNode(Base @base, string rootName = null) =>
                ToTypedElement(@base, rootName).ToSourceNode();

    }
}
