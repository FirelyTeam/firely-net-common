using Hl7.Fhir.Specification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Hl7.Fhir.Introspection
{
    public static class FhirReflectionExtensions
    {
        /// <summary>
        /// List all POCO's that represent the given FHIR type. When assemblies for multiple FHIR
        /// releases are present, there might be more than one type returned.
        /// </summary>
        /// <remarks>Handle Any subclasses too</remarks>
        public static bool TryGetFhirType(this Assembly assembly, string fhirTypeName, out Type t)
        {
            if (!FhirReflection.GetFhirTypes(assembly).TryGetValue(fhirTypeName, out var typesFound))
            {
                t = default;
                return false;
            }
            else
            {
                if (typesFound.Count() > 1)
                {
                    var classes = string.Join(", ", typesFound.Select(t => $"{t.FullName} (from {t.Assembly.FullName})"));
                    throw new InvalidOperationException($"FHIR type '{fhirTypeName}' matches multiple classes" +
                        $"representing FHIR information: {classes}.");
                }

                t = typesFound.Single();
                return true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>Will return concrete backbone type names using the A#b notation. And also
        /// know about Any types</remarks>
        public static string GetFhirTypeName(this Type t) => FhirReflection.GetFhirTypeAttribute(t).Name;

        public static bool IsResource(this Type t) => FhirReflection.GetFhirTypeAttribute(t).IsResource;

        public static bool IsNestedType(this Type t) => FhirReflection.GetFhirTypeAttribute(t).IsNestedType;

        public static bool TryGetFhirProperty(this Type t, string elementName, out PropertyInfo pi, FhirRelease release = null)
        {
            if (!FhirReflection.GetFhirProperties(t).TryGetValue(elementName, out var propertiesFound))
            {
                pi = default;
                return false;
            }
            else
            {
                pi = propertiesFound
                    .Where(p => FhirReflection.GetFhirElementAttributes().Any(a => a.AppliesToVersion(release));
                return true;
            }
        }

        /// <summary>
        /// If the property is a choice element, returns the types allowed for the choice
        /// </summary>
        /// <param name="pi"></param>
        /// <returns></returns>
        public static IReadOnlyCollection<Type> GetTypeChoices(PropertyInfo pi) => throw new NotImplementedException();

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



