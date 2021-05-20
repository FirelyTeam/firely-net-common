using Hl7.Fhir.Introspection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Hl7.Fhir.Serialization
{
    public class FhirReflectionContext
    {
        public static FhirReflectionContext FromTypeName(Assembly a, string typeName)
        {
            throw new NotImplementedException();
        }

        public FhirReflectionContext(Type pocoType)
        {
            throw new NotImplementedException();
        }
    }

    public static class FhirReflection
    {
        private static readonly ConcurrentDictionary<string, IReadOnlyDictionary<string, IEnumerable<Type>>> _fhirTypes = new();
        private static readonly ConcurrentDictionary<Type, IReadOnlyDictionary<string, IEnumerable<PropertyInfo>>> _fhirTypeProperties = new();
        private static readonly ConcurrentDictionary<Type, IEnumerable<FhirTypeAttribute>> _fhirTypeAttributes = new();
        private static readonly ConcurrentDictionary<PropertyInfo, IEnumerable<FhirElementAttribute>> _fhirElementAttributes = new();

        /// <summary>
        /// List all POCO's representing FHIR types in the given assembly, including assemblies containing
        /// FHIR types that the given assembly refers to.
        /// </summary>
        public static IReadOnlyDictionary<string, IEnumerable<Type>> GetFhirTypes(this Assembly modelAssembly)
        {
            return _fhirTypes.GetOrAdd(modelAssembly.FullName,
                getTypesFromAssembly(modelAssembly.GetName())
                    .ToLookup(p => p.typeName, p => p.type)
                    .ToDictionary(lu => lu.Key, lu => lu.AsEnumerable(), StringComparer.OrdinalIgnoreCase));

            static IEnumerable<(string typeName, Type type)> getTypesFromAssembly(AssemblyName an)
            {
                Assembly a = Assembly.Load(an);
                if (a.GetCustomAttribute<FhirModelAssemblyAttribute>() is null) return Enumerable.Empty<(string, Type)>();

                return
                    a.ExportedTypes
                    .SelectMany(et => et.GetFhirTypeAttributes().Select(ta => (ta.Name, et)))
                    .Concat(
                        a.GetReferencedAssemblies()
                        .SelectMany(ra => getTypesFromAssembly(ra)));
            }
        }

        public static IEnumerable<FhirTypeAttribute> GetFhirTypeAttributes(this Type t) =>
            _fhirTypeAttributes.GetOrAdd(t, t.GetTypeInfo().GetCustomAttributes<FhirTypeAttribute>());

        public static IReadOnlyDictionary<string, IEnumerable<PropertyInfo>> GetFhirProperties(this Type t) =>
            _fhirTypeProperties.GetOrAdd(t,
                t.GetTypeInfo().GetProperties()
                .SelectMany(pi => pi.GetFhirElementAttributes()
                    .Select(ea => new KeyValuePair<string, PropertyInfo>(ea.Name, pi)))
                .ToLookup(p => p.Key, p => p.Value)
                .ToDictionary(lu => lu.Key, lu => lu.AsEnumerable()));

        public static IEnumerable<FhirElementAttribute> GetFhirElementAttributes(this PropertyInfo pi) =>
            _fhirElementAttributes.GetOrAdd(pi, pi.GetCustomAttributes<FhirElementAttribute>());

        ///// <summary>
        ///// List all POCO's that represent the given FHIR type. When assemblies for multiple FHIR
        ///// releases are present, there might be more than one type returned.
        ///// </summary>
        ///// <remarks>Handle Any subclasses too</remarks>
        //public static IEnumerable<Type> GetFhirTypeByName(this Assembly assemblies, string fhirTypeName) =>
        //            assemblies.GetFhirTypes().TryGetValue(fhirTypeName, out var types) ?
        //                types
        //                : Enumerable.Empty<Type>();

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <remarks>Will return concrete backbone type names using the A#b notation. And also
        ///// know about Any types</remarks>
        //public static string GetFhirTypeName(this Type t) => throw new NotImplementedException();


        /// <summary>
        /// If the property is a choice element, returns the types allowed for the choicse
        /// </summary>
        /// <param name="pi"></param>
        /// <returns></returns>
        public static IReadOnlyCollection<Type> GetTypeChoices(this PropertyInfo pi) => throw new NotImplementedException();

        public static bool IsContainedResource(this PropertyInfo pi) => throw new NotImplementedException();

        public static bool IsChoiceElement(this PropertyInfo pi) => throw new NotImplementedException();

        /// <summary>
        /// Returns the type that represents the FHIR type of the property.
        /// </summary>
        /// <param name="pi"></param>
        /// <returns></returns>
        public static Type GetPropertyTypeForElement(this PropertyInfo pi) => throw new NotImplementedException();

        public static string GetElementName(this PropertyInfo pi) => throw new NotImplementedException();
    }
}



