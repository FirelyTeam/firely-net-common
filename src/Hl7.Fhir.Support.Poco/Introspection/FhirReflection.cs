using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Hl7.Fhir.Introspection
{
    public static class FhirReflection
    {
        private static readonly ConcurrentDictionary<string, IReadOnlyDictionary<string, IEnumerable<Type>>> _fhirTypes = new();
        private static readonly ConcurrentDictionary<Type, FhirTypeAttribute> _fhirTypeAttributes = new();
        private static readonly ConcurrentDictionary<Type, IReadOnlyDictionary<string, IEnumerable<PropertyInfo>>> _fhirTypeProperties = new();
        private static readonly ConcurrentDictionary<PropertyInfo, IEnumerable<FhirElementAttribute>> _fhirElementAttributes = new();

        /// <summary>
        /// List all POCO's representing FHIR types in the given assembly, including assemblies containing
        /// FHIR types that the given assembly refers to.
        /// </summary>
        public static IReadOnlyDictionary<string, IEnumerable<Type>> GetFhirTypes(Assembly modelAssembly)
        {
            return _fhirTypes.GetOrAdd(modelAssembly.FullName,
                getTypesFromAssembly(modelAssembly.GetName())
                    .ToLookup(p => p.fta.Name, p => (p.type, p.fta))
                    .ToDictionary(
                        lu => lu.Key,
                        lu => lu.OrderByDescending(lus => lus.fta.Since).Select(lus => lus.type),
                        StringComparer.OrdinalIgnoreCase));

            static IEnumerable<(FhirTypeAttribute fta, Type type)> getTypesFromAssembly(AssemblyName an)
            {
                Assembly a = Assembly.Load(an);
                if (a.GetCustomAttribute<FhirModelAssemblyAttribute>() is null) return Enumerable.Empty<(FhirTypeAttribute, Type)>();

                return
                    (from et in a.ExportedTypes
                     let fta = GetFhirTypeAttribute(et)
                     where fta is not null
                     select (fta, et))
                    .Concat(
                        a.GetReferencedAssemblies()
                        .SelectMany(ra => getTypesFromAssembly(ra)));
            }
        }

        public static FhirTypeAttribute GetFhirTypeAttribute(Type t) =>
            _fhirTypeAttributes.GetOrAdd(t, t.GetTypeInfo().GetCustomAttribute<FhirTypeAttribute>());

        public static IReadOnlyDictionary<string, IEnumerable<PropertyInfo>> GetFhirProperties(Type t) =>
            _fhirTypeProperties.GetOrAdd(t,
                t.GetTypeInfo().GetProperties()
                .SelectMany(pi =>
                    GetFhirElementAttributes(pi)
                    .Select(ea => (ea, pi)))
                .ToLookup(p => p.ea.Name, p => (p.pi, p.ea))
                .ToDictionary(
                    lu => lu.Key,
                    lu => lu.OrderByDescending(lus => lus.ea.Since).Select(lus => lus.pi)));

        public static IEnumerable<FhirElementAttribute> GetFhirElementAttributes(PropertyInfo pi) =>
            _fhirElementAttributes.GetOrAdd(pi,
                pi.GetCustomAttributes<FhirElementAttribute>().OrderByDescending(fea => fea.Since));
    }
}



