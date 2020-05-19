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
    internal class CollectionAdapter : IAdapter
    {
        public virtual bool TryAdd(
            object target,
            string name,
            IStructureDefinitionSummaryProvider contractResolver,
            object value,
            out string errorMessage)
        {
            errorMessage = $"The 'add' operation can only be applied on the single element.";
            return false;
        }

        public virtual bool TryInsert (
            object target,
            int index,
            IStructureDefinitionSummaryProvider contractResolver,
            object value,
            out string errorMessage)
        {
            var collection = (IEnumerable<ITypedElement>) target;

            int arraySize = collection.Count();
            if ( index > arraySize )
            {
                errorMessage = $"The index value provided for 'insert' operation is out of bounds of the array size.";
                return false;
            }

            var sampleElement = collection.First();
            if ( !TryConvertValue(value, sampleElement.Definition, out var convertedValue) )
            {
                errorMessage = $"The value '{value}' is invalid for target location.";
                return false;
            }

            try
            {
                var parent = ((ElementNode) sampleElement).Parent;
                // if inserting to the end of the array use a simpler Add method
                if ( index == arraySize )
                {
                    parent.Add(contractResolver, convertedValue, sampleElement.Name);
                }
                else
                {
                    parent.Insert(contractResolver, convertedValue, index, sampleElement.Name);
                }

                errorMessage = null;
                return true;
            }
            catch ( Exception ex )
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        public virtual bool TryDelete(
            object target,
            IStructureDefinitionSummaryProvider contractResolver,
            out string errorMessage)
        {
            var collection = (IEnumerable<ITypedElement>) target;
            if ( collection.Count() != 1 )
            {
                errorMessage = $"The 'delete' operation can only be applied on the single element.";
                return false;
            }

            var adapter = new ElementNodeAdapter();
            return adapter.TryDelete(collection.First(), contractResolver, out errorMessage);
        }

        public virtual bool TryReplace(
            object target,
            IStructureDefinitionSummaryProvider contractResolver,
            object value,
            out string errorMessage)
        {
            var collection = (IEnumerable<ITypedElement>) target;
            if ( collection.Count() != 1 )
            {
                errorMessage = $"The 'replace' operation can only be applied on the single element.";
                return false;
            }

            var adapter = new ElementNodeAdapter();
            return adapter.TryReplace(collection.First(), contractResolver, value, out errorMessage);
        }

        public virtual bool TryGet (
            object target,
            IStructureDefinitionSummaryProvider contractResolver,
            out object value,
            out string errorMessage)
        {
            var collection = (IEnumerable<ITypedElement>) target;
            if ( collection.Count() != 1 )
            {
                errorMessage = $"The 'get' operation can only be applied on the single element.";
                value = null;
                return false;
            }

            var adapter = new ElementNodeAdapter();
            return adapter.TryGet(collection.First(), contractResolver, out value, out errorMessage);
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
