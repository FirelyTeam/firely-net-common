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
using Hl7.Fhir.Patch.Adapters;
using Hl7.Fhir.Specification;

namespace Hl7.Fhir.Patch.Internal
{
    /// <summary>
    /// This API supports infrastructure and is not intended to be used
    /// directly from your code. This API may change or be removed in future releases.
    /// </summary>
    internal class CollectionAdapter : IAdapter
    {
        private readonly IStructureDefinitionSummaryProvider _provider;
        private readonly IAdapterFactory _adapterFactory;

        public CollectionAdapter (IStructureDefinitionSummaryProvider provider, IAdapterFactory adapterFactory)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _adapterFactory = adapterFactory ?? throw new ArgumentNullException(nameof(adapterFactory));
        }

        public virtual bool TryAdd(
            object target,
            string name,
            object value,
            out string errorMessage)
        {
            errorMessage = $"The 'add' operation can only be applied on the single element.";
            return false;
        }

        public virtual bool TryInsert (
            object target,
            int index,
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
            if ( !ValueValidator.IsValueValidElement(value, sampleElement.Definition, out var valueNode) )
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
                    parent.Add(_provider, valueNode, sampleElement.Name);
                }
                else
                {
                    parent.Insert(_provider, valueNode, index, sampleElement.Name);
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
            out string errorMessage)
        {
            var collection = (IEnumerable<ITypedElement>) target;
            if ( collection.Count() != 1 )
            {
                errorMessage = $"The 'delete' operation can only be applied on the single element.";
                return false;
            }

            object newTarget = collection.First();
            IAdapter adapter = _adapterFactory.Create(newTarget, _provider);
            return adapter.TryDelete(newTarget, out errorMessage);
        }

        public virtual bool TryReplace(
            object target,
            object value,
            out string errorMessage)
        {
            var collection = (IEnumerable<ITypedElement>) target;
            if ( collection.Count() != 1 )
            {
                errorMessage = $"The 'replace' operation can only be applied on the single element.";
                return false;
            }

            object newTarget = collection.First();
            IAdapter adapter = _adapterFactory.Create(newTarget, _provider);
            return adapter.TryReplace(newTarget, value, out errorMessage);
        }

        public virtual bool TryGet (
            object target,
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

            object newTarget = collection.First();
            IAdapter adapter = _adapterFactory.Create(newTarget, _provider);
            return adapter.TryGet(newTarget, out value, out errorMessage);
        }
    }
}
