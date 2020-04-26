/* 
 * Copyright (c) 2014, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

using Hl7.Fhir.Utility;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace Hl7.Fhir.Introspection
{
    /// <summary>
    /// A cache of FHIR type mappings found on .NET classes.
    /// </summary>
    /// <remarks>Holds the mappings for a specific version of FHIR, even though some POCO's can 
    /// specify the definition of multiple versions of FHIR. 
    /// </remarks>
    public class ModelInspector
    {
        public const int R3_VERSION = 3;
        public const int R4_VERSION = 4;
        public const int R5_VERSION = 5;

        public ModelInspector(int fhirVersion)
        {
            _fhirVersion = fhirVersion;
        }

        private int _fhirVersion;

        // Primary index of classmappings, key is Type
        private readonly ConcurrentDictionary<Type, ClassMapping> _classMappingsByType =
            new ConcurrentDictionary<Type, ClassMapping>();
            
        // Index for easy lookup of datatypes, key is upper typenanme
        private readonly ConcurrentDictionary<string, ClassMapping> _classMappingsByName =
            new ConcurrentDictionary<string, ClassMapping>();

        public void Import(Assembly assembly)
        {
            if (assembly == null) throw Error.ArgumentNull(nameof(assembly));

#if NET40
            IEnumerable<Type> exportedTypes = assembly.GetExportedTypes();
#else
            IEnumerable<Type> exportedTypes = assembly.ExportedTypes;
#endif

            foreach (Type type in exportedTypes)
                ImportType(type);
        }


        public void ImportType(Type type)
        {
            if (_classMappingsByType.TryGetValue(type, out _))
                throw new ArgumentException($"There is already a mapping for type '{type.Name}'.", nameof(type));

            _ = GetOrAddClassMappingForType(type);
        }

        private ClassMapping createMapping(Type type, int version)
        {
            if (!ClassMapping.TryCreate(type, out var mapping, version))
            {
                Message.Info("Skipped type {0} while doing inspection: not recognized as representing a FHIR type", type.Name);
                return null;
            }
            else
            {
                Message.Info("Created Class mapping for newly encountered type {0} (FHIR type {1})", type.Name, mapping.Name);
                return mapping;
            }
        }


        public ClassMapping GetOrAddClassMappingForType(Type type)
        {
            var typeMapping = _classMappingsByType.GetOrAdd(type, tp => createMapping(tp, _fhirVersion));

            // Whether we are pre-empted here or not, resultMapping will always be "the" single instance
            // with a mapping for this type, shared across all threads. Now we are going to add an entry by
            // name for this mapping.
            
            var nameMapping = _classMappingsByName.GetOrAdd(typeMapping.Name.ToUpperInvariant(), typeMapping);
            if(!object.ReferenceEquals(typeMapping, nameMapping))
            {
                // ouch, there was already a mapping under this name, that does not correspond to the instance
                // for our type mapping -> there must be multiple types having a mapping for the same model type.
                throw new ArgumentException($"Type '{type.Name}' has a mapping for model type '{typeMapping.Name}', " +
                    $"but type '{nameMapping.NativeType.Name}' was already registered for that model type.");
            }

            return typeMapping;
        }

        public ClassMapping FindClassMappingByName(string name) =>
            _classMappingsByName.TryGetValue(name.ToUpperInvariant(), out var entry) ? entry : null;
        public ClassMapping FindClassMappingByType(Type t) =>
            _classMappingsByType.TryGetValue(t, out var entry) ? entry : null;

    }
}
