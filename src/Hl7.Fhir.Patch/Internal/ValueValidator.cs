/* 
 * Copyright (c) 2020, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/fhir-net-api/blob/master/LICENSE
 */

using System.Linq;
using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Specification;

namespace Hl7.Fhir.Patch.Internal
{
    internal static class ValueValidator
    {
        internal static bool IsValueValidElement (object value, IElementDefinitionSummary propertyDefinition, out ElementNode valueNode)
        {
            if ( !(value is ITypedElement typedValue) )
            {
                valueNode = null;
                return false;
            }

            if ( propertyDefinition.Type.Any(t => t.GetTypeName() == typedValue.InstanceType) )
            {
                valueNode = ElementNode.FromElement(typedValue);
                return true;
            }

            valueNode = null;
            return false;
        }
    }
}
