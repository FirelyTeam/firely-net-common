using Hl7.Fhir.Introspection;
using Hl7.Fhir.Validation;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Hl7.Fhir.Serialization
{
    public static class FhirReflection
    {
        /// <summary>
        /// List all POCO's representing FHIR types in the given assemblies.
        /// </summary>
        public static IReadOnlyDictionary<string, Type> GetFhirTypes(this Assembly assemblies, params Assembly[] otherAssemblies) => throw new NotImplementedException();

        /// <summary>
        /// List all POCO's representing FHIR types in the given assemblies.
        /// </summary>
        /// <remarks>Handle Any subclasses too</remarks>
        public static Type? GetFhirTypeByName(this Assembly assemblies, string fhirTypeName,
            StringComparison comparison = StringComparison.Ordinal) => throw new NotImplementedException();

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>Will return concrete backbone type names using the A#b notation. And also
        /// know about Any types</remarks>
        public static string GetFhirTypeName(this Type t) => throw new NotImplementedException();

        public static IDictionary<string, PropertyInfo> GetFhirProperties(this Type t) => throw new NotImplementedException();

        public static FhirElementAttribute GetFhirElementAttribute(this PropertyInfo pi) => throw new NotImplementedException();

        public static AllowedTypesAttribute GetAllowedTypesAttribute(this PropertyInfo pi) => throw new NotImplementedException();

        public static Type GetPropertyTypeForElement(this PropertyInfo pi) => throw new NotImplementedException();
    }
}



