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
    public class ModelInspector
    {
        public const int R3_VERSION = 3;
        public const int R4_VERSION = 4;
        public const int R5_VERSION = 5;

        public ModelInspector(int fhirVersion)
        {
            _classMappingsByType = new ConcurrentDictionary<Type, ClassMapping>()
        }

        // Primary index of classmappings, key is Type
        private readonly ConcurrentDictionary<Type, ClassMapping> _classMappingsByType =
            

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


        public ClassMapping ImportType(Type type)
        {
        }

        private void addClassMapping(ClassMapping mapping)
        {
            var type = mapping.NativeType;
            _classMappingsByType[type] = mapping;

            var key = mapping.Name.ToUpperInvariant();
            _classMappingsByName[key] = mapping;
        }

        public ClassMapping FindClassMappingByName(string name) =>
            _classMappingsByName.TryGetValue(name.ToUpperInvariant(), out var entry) ? entry : null;

        public ClassMapping FindClassMappingByType(Type type) =>
            _classMappingsByType.TryGetValue(type, out var entry) ? entry : null;
    }
}
