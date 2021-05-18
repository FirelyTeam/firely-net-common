using Hl7.Fhir.Introspection;
using Hl7.Fhir.Specification;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Hl7.Fhir.Serialization
{
    public static class FhirReflection
    {
        private static readonly ConcurrentDictionary<string, IReadOnlyDictionary<string, Type>> _fhirTypes = new();
        private static readonly ConcurrentDictionary<Type, FhirTypeAttribute> _fhirTypeAttributes = new();

        private static readonly Dictionary<string, Type> _empty = new();

        /// <summary>
        /// List all POCO's representing FHIR types in the given assembly, including assemblies containing
        /// FHIR types that the given assembly refers to.
        /// </summary>
        public static IReadOnlyDictionary<string, Type> GetFhirTypes(this Assembly modelAssembly)
        {
            return getTypesFromAssembly(modelAssembly.GetName());

            static IReadOnlyDictionary<string, Type> getTypesFromAssembly(AssemblyName an)
            {
                Assembly a = Assembly.Load(an);
                //if(a.GetCustomAttribute<FhirModelAssembly> is null) return _empty;

                return _fhirTypes.GetOrAdd(a.FullName,
                    a.ExportedTypes
                    .Where(et => et.RepresentsFhirType())
                    .Select(et => new KeyValuePair<string, Type>(et.Name, et))
                    .Concat(a.GetReferencedAssemblies()
                    .SelectMany(ra => getTypesFromAssembly(ra)))
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
            }
        }


        public static IReadOnlyDictionary<string, Type> GetFhirTypes(this Assembly modelAssembly, FhirRelease filter) => throw new NotImplementedException();

        /// <summary>
        /// List all POCO's that represent the given FHIR type. When assemblies for multiple FHIR
        /// releases are present, there might be more than one type returned.
        /// </summary>
        /// <remarks>Handle Any subclasses too</remarks>
        public static Type GetFhirTypeByName(this Assembly assemblies, string fhirTypeName,
            StringComparison comparison = StringComparison.Ordinal) => throw new NotImplementedException();

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>Will return concrete backbone type names using the A#b notation. And also
        /// know about Any types</remarks>
        public static string GetFhirTypeName(this Type t) => throw new NotImplementedException();

        public static IDictionary<string, PropertyInfo> GetFhirProperties(this Type t) => throw new NotImplementedException();


        private static FhirTypeAttribute getFhirTypeAttribute(Type t) =>
                    _fhirTypeAttributes.GetOrAdd(t, t.GetTypeInfo().GetCustomAttribute<FhirTypeAttribute>());

        /// <summary>
        /// Determines whether the type represents a FHIR datatype or resource.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static bool RepresentsFhirType(this Type t) => getFhirTypeAttribute(t) is not null;

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



