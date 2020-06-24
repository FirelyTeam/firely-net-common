/* 
 * Copyright (c) 2015, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/ewoutkramer/fhir-net-api/master/LICENSE
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Specification;
using Hl7.Fhir.Utility;

namespace Hl7.FhirPath.Functions
{
    internal static class TypeOperators
    {
        public static bool Is(this ITypedElement focus, string type)
        {
            if (focus.InstanceTypeD != null)
            {
                return Is(focus.InstanceTypeD, type);     // I have no information about classes/subclasses
            }
            else
                throw Error.InvalidOperation("Is operator is called on untyped data");
        }

        public static bool Is(TypeDefinition instanceType, string declaredType)
        {
            // Bit of a hack since I don't have model information here (yet)
            var fullInstanceTypeName = @"{instanceType.DeclaringModel.Name}.{instanceType.Name}";
            if (declaredType.Contains("."))
                return fullInstanceTypeName == declaredType;
            else
            {
                return fullInstanceTypeName == "System." + declaredType ||
                        fullInstanceTypeName == "FHIR." + declaredType;
            }
        }

        public static IEnumerable<ITypedElement> FilterType(this IEnumerable<ITypedElement> focus, string typeName)
            => focus.Where(item => item.Is(typeName));

        public static ITypedElement CastAs(this ITypedElement focus, string typeName)
            => focus.Is(typeName) ? focus : null;
    }
}
