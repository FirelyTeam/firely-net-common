/* 
 * Copyright (c) 2020, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/fhir-net-api/blob/master/LICENSE
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Specification;

namespace Hl7.Fhir.Patch.Internal
{
    /// <summary>
    /// This API supports infrastructure and is not intended to be used
    /// directly from your code. This API may change or be removed in future releases.
    /// </summary>
    internal class ElementNodeAdapter : IAdapter
    {
        public virtual bool TryAdd(
            object target,
            string name,
            IStructureDefinitionSummaryProvider contractResolver,
            object value,
            out string errorMessage)
        {
            var typedElement = (ElementNode) target;

            if (!TryGetProperty(typedElement, name, contractResolver, out var PropertyDefinition, out var PropertyValue))
            {
                errorMessage = $"The target location specified by path segment '{name}' was not found.";
                return false;
            }

            if ( PropertyValue.FirstOrDefault() != null && !PropertyDefinition.IsCollection )
            {
                errorMessage = $"The property at path '{name}' is already set. Use 'replace' operation instead.";
                return false;
            }

            if (!TryConvertValue(value, PropertyDefinition, out var convertedValue))
            {
                errorMessage = $"The value '{value}' is invalid for target location.";
                return false;
            }

            try
            {
                typedElement.Add(contractResolver, convertedValue, name);
                errorMessage = null;
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        public virtual bool TryInsert (
            object target,
            int index,
            IStructureDefinitionSummaryProvider contractResolver,
            object value,
            out string errorMessage)
        {
            errorMessage = $"The 'insert' operation can only be applied on the collection.";
            return false;
        }

        public virtual bool TryDelete (
            object target,
            IStructureDefinitionSummaryProvider contractResolver,
            out string errorMessage)
        {
            try
            {
                var typedElement = (ElementNode) target;
                typedElement.Parent.Remove(typedElement);
            }
            catch ( Exception ex )
            {
                errorMessage = ex.Message;
                return false;
            }

            errorMessage = null;
            return true;
        }

        public virtual bool TryReplace(
            object target,
            IStructureDefinitionSummaryProvider contractResolver,
            object value,
            out string errorMessage)
        {
            var typedElement = (ElementNode) target;

            if ( !TryConvertValue(value, typedElement.Definition, out var convertedValue) )
            {
                errorMessage = $"The value '{value}' is invalid for target location.";
                return false;
            }

            try
            {
                var newElement = ElementNode.FromElement(convertedValue);
                typedElement.Parent.Replace(contractResolver, typedElement, newElement);
            }
            catch ( Exception ex )
            {
                errorMessage = ex.Message;
                return false;
            }

            errorMessage = null;
            return true;
        }

        public virtual bool TryGet (
            object target,
            IStructureDefinitionSummaryProvider contractResolver,
            out object value,
            out string errorMessage)
        {
            value = target;
            errorMessage = null;
            return true;
        }

        protected virtual bool TryGetProperty(
            ITypedElement typedElement,
            string segment,
            IStructureDefinitionSummaryProvider contractResolver,
            out IElementDefinitionSummary PropertyDefinition, out IEnumerable<ITypedElement> PropertyValue)
        {
            if ( typedElement != null )
            {
                PropertyDefinition = typedElement.ChildDefinitions(contractResolver).Where(x => x.ElementName == segment).FirstOrDefault();

                if ( PropertyDefinition != null )
                {
                    PropertyValue = typedElement.Children(segment);
                    return true;
                }
            }

            PropertyValue = Enumerable.Empty<ITypedElement>();
            PropertyDefinition = null;
            return false;
        }

        protected virtual bool TryConvertValue (object value, IElementDefinitionSummary propertyDefinition, out ElementNode convertedValue)
        {
            if ( !(value is ITypedElement typedValue) )
            {
                convertedValue = null;
                return false;
            }

            if ( propertyDefinition.Type.Any(t => t.GetTypeName() == typedValue.InstanceType) )
            {
                convertedValue = ElementNode.FromElement(typedValue);
                return true;
            }

            convertedValue = null;
            return false;
        }
    }
}
