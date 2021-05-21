/* 
 * Copyright (c) 2014, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-net-sdk/master/LICENSE
 */

#nullable enable

using Hl7.Fhir.Model;
using Hl7.Fhir.Specification;
using Hl7.Fhir.Utility;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Hl7.Fhir.Introspection
{
    /// <summary>
    /// A cache of FHIR type mappings found on .NET classes.
    /// </summary>
    /// <remarks>POCO's in the "common" assemblies
    /// can reflect the definition of multiple releases of FHIR using <see cref="IFhirVersionDependent"/>
    /// attributes. A <see cref="ModelInspector"/> will always capture the metadata for one such
    /// <see cref="Specification.FhirRelease" /> which is passed to it in the constructor.
    /// </remarks>
    public class ModelInspector : IStructureDefinitionSummaryProvider
    {
        private static readonly ConcurrentDictionary<string, ModelInspector> _inspectedAssemblies = new();

        /// <summary>
        /// Returns a fully configured <see cref="ModelInspector"/> with the
        /// FHIR metadata contents of the given assembly. Calling this function repeatedly for
        /// the same assembly will return the same inspector.
        /// </summary>
        /// <remarks>If the assembly given is FHIR Release specific, the returned inspector will contain
        /// metadata for that release only. If the assembly is the common assembly, it will contain
        /// metadata for the most recent release for those common classes.</remarks>
        public static ModelInspector ForAssembly(Assembly a)
        {
            return _inspectedAssemblies.GetOrAdd(a.FullName, _ => configureInspector(a));

            static ModelInspector configureInspector(Assembly a)
            {
                if (a.GetCustomAttribute<FhirModelAssemblyAttribute>() is not FhirModelAssemblyAttribute modelAssemblyAttr)
                    throw new InvalidOperationException($"Assembly {a.FullName} cannot be used to supply FHIR metadata," +
                        $" as it is not marked with a {nameof(FhirModelAssemblyAttribute)} attribute.");

                var newInspector = new ModelInspector(modelAssemblyAttr.Since);
                newInspector.Import(a);

                // Make sure we always include the common types too. 
                var commonAssembly = typeof(Resource).GetTypeInfo().Assembly;
                if (a.FullName != commonAssembly.FullName)
                    newInspector.Import(commonAssembly);

                return newInspector;
            }
        }


        /// <summary>
        /// Constructs a ModelInspector that will reflect the FHIR metadata for the given FHIR release
        /// </summary>
        public ModelInspector(FhirRelease fhirRelease)
        {
            FhirRelease = fhirRelease;
        }

        public readonly FhirRelease FhirRelease;

        // Index for easy lookup of datatypes.
        private readonly ConcurrentDictionary<string, ClassMapping> _classMappingsByName =
            new(StringComparer.OrdinalIgnoreCase);

        // Primary index of classmappings, key is Type
        private readonly ConcurrentDictionary<Type, ClassMapping?> _classMappingsByType = new();

        /// <summary>
        /// Locates all types in the assembly representing FHIR metadata and extracts
        /// the data as <see cref="ClassMapping"/>s.
        /// </summary>
        public IReadOnlyList<ClassMapping> Import(Assembly assembly)
        {
            if (assembly == null) throw Error.ArgumentNull(nameof(assembly));

#if NET40
            IEnumerable<Type> exportedTypes = assembly.GetExportedTypes();
#else
            IEnumerable<Type> exportedTypes = assembly.ExportedTypes;
#endif

            return exportedTypes.Select(t => ImportType(t))
                .Where(cm => cm is not null)
                .ToList()!;
        }

        /// <inheritdoc cref="IStructureDefinitionSummaryProvider.Provide(string)"/>
        public IStructureDefinitionSummary? Provide(string canonical)
        {
            var isLocalType = !canonical.Contains("/");

            if (!isLocalType)
            {
                // So, we have received a canonical url, not being a relative path
                // (know resource/datatype), we -for now- only know how to get a ClassMapping
                // for this, if it's a built-in T4 generated POCO, so there's no way
                // to find a mapping for this.
                return null;
            }

            return FindClassMapping(canonical);
        }


        /// <summary>
        /// Extracts the FHIR metadata from a <see cref="Type"/> into a <see cref="ClassMapping"/>.
        /// </summary>
        public ClassMapping? ImportType(Type type)
        {
            if (_classMappingsByType.TryGetValue(type, out var mapping))
                return mapping;     // no need to import the same type twice

            // Don't import types that aren't marked with [FhirType]
            if (ClassMapping.GetAttribute<FhirTypeAttribute>(type.GetTypeInfo(), FhirRelease) == null) return null;

            // When explicitly importing a (newer?) class mapping for the same
            // model type name, overwrite the old entry.
            return getOrAddClassMappingForTypeInternal(type, overwrite: true);
        }

        private ClassMapping? createMapping(Type type, FhirRelease version)
        {
            if (!ClassMapping.TryCreate(type, out var mapping, version))
            {
                Message.Info("Skipped type {0} while doing inspection: not recognized as representing a FHIR type", type.Name);
                return null;
            }
            else
            {
                Message.Info("Created Class mapping for newly encountered type {0} (FHIR type {1})", type.Name, mapping!.Name);
                return mapping;
            }
        }

        private ClassMapping? getOrAddClassMappingForTypeInternal(Type type, bool overwrite)
        {
            var typeMapping = _classMappingsByType.GetOrAdd(type, tp => createMapping(tp, FhirRelease));

            if (typeMapping == null) return null;

            // Whether we are pre-empted here or not, resultMapping will always be "the" single instance
            // with a mapping for this type, shared across all threads. Now we are going to add an entry by
            // name for this mapping.

            var key = typeMapping.Name;
            // Add this mapping by name of the mapping, overriding any entry already present
            if (overwrite)
            {
                _ = _classMappingsByName
                            .AddOrUpdate(key, typeMapping, (_, __) => typeMapping);
            }
            else
            {
                var nameMapping = _classMappingsByName.GetOrAdd(key, typeMapping);

                if (!object.ReferenceEquals(typeMapping, nameMapping))
                {
                    // ouch, there was already a mapping under this name, that does not correspond to the instance
                    // for our type mapping -> there must be multiple types having a mapping for the same model type.
                    throw new ArgumentException($"Type '{type.Name}' has a mapping for model type '{typeMapping.Name}', " +
                        $"but type '{nameMapping.NativeType.Name}' was already registered for that model type.");
                }
            }

            return typeMapping;
        }

        /// <summary>
        /// Retrieves an already imported <see cref="ClassMapping" /> given a FHIR type name.
        /// </summary>
        public ClassMapping? FindClassMapping(string fhirTypeName) =>
            _classMappingsByName.TryGetValue(fhirTypeName, out var entry) ? entry : null;

        /// <summary>
        /// Retrieves an already imported <see cref="ClassMapping" /> given a Type.
        /// </summary>
        public ClassMapping? FindClassMapping(Type t) =>
            _classMappingsByType.TryGetValue(t, out var entry) ? entry : null;
    }
}

#nullable restore