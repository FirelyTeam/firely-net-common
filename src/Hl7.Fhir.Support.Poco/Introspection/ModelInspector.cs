/* 
 * Copyright (c) 2014, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

using Hl7.Fhir.Specification;
using Hl7.Fhir.Utility;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Hl7.Fhir.Introspection
{

    public class ModelInspector
    {

        public ModelInspector(FhirRelease fhirVersion)
        {
            _fhirVersion = fhirVersion;
        }

        private readonly FhirRelease _fhirVersion;
        // Index for easy lookup of datatypes, key is upper typenanme
        private readonly Dictionary<string, ClassMapping> _classMappingsByName = new Dictionary<string,ClassMapping>();

        // Index for easy lookup of classmappings, key is Type
        private readonly Dictionary<Type, ClassMapping> _classMappingsByType = new Dictionary<Type, ClassMapping>();

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


        private readonly object importLockObject = new object();

        public ClassMapping ImportType(Type type)
        {
            lock (importLockObject)
            {
                var mapping = FindClassMappingByType(type);
                if (mapping != null) return mapping;

                if (!ClassMapping.TryCreate(type, out mapping, _fhirVersion))
                {
                    Message.Info("Skipped type {0} while doing inspection: not recognized as representing a FHIR type", type.Name);
                    return null;
                }
                else
                {
                    addClassMapping(mapping);
                    Message.Info("Created Class mapping for newly encountered type {0} (FHIR type {1})", type.Name, mapping.Name);
                    return mapping;
                }
            }
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
