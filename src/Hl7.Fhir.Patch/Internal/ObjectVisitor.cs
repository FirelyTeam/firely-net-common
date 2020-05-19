/* 
 * Copyright (c) 2020, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/fhir-net-api/blob/master/LICENSE
 */

using System;
using System.Linq;
using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Patch.Adapters;
using Hl7.FhirPath;

namespace Hl7.Fhir.Patch.Internal
{
    /// <summary>
    /// This API supports infrastructure and is not intended to be used
    /// directly from your code. This API may change or be removed in future releases.
    /// </summary>
    internal class ObjectVisitor
    {
        private readonly IAdapterFactory _adapterFactory;
        private readonly CompiledExpression _path;

        /// <summary>
        /// Initializes a new instance of <see cref="ObjectVisitor"/>.
        /// </summary>
        /// <param name="path">The path of the Patch operation</param>
        public ObjectVisitor(CompiledExpression path)
            :this(path, new AdapterFactory())
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ObjectVisitor"/>.
        /// </summary>
        /// <param name="path">The path of the Patch operation</param>
        /// <param name="adapterFactory">The <see cref="IAdapterFactory"/> to use when creating adaptors.</param>
        public ObjectVisitor(CompiledExpression path, IAdapterFactory adapterFactory)
        {
            _path = path;
            _adapterFactory = adapterFactory ?? throw new ArgumentNullException(nameof(adapterFactory));
        }

        public bool TryVisit(ref object target, out IAdapter adapter, out string errorMessage)
        {
            
            if (!(target is ITypedElement typedTarget))
            {
                adapter = null;
                errorMessage = null;
                return false;
            }

            try
            {
                var targetElements = _path(typedTarget, EvaluationContext.CreateDefault());
                var arraySize = targetElements.Count();
                if ( arraySize == 0 )
                {
                    adapter = null;
                    errorMessage = $"The target location specified by path was not found";
                    return false;
                }

                target = arraySize == 1 && !targetElements.First().Definition.IsCollection ? (object)targetElements.First() : targetElements;
            }
            catch ( Exception ex )
            {
                adapter = null;
                errorMessage = ex.Message;
                return false;
            }

            adapter = SelectAdapter(target);
            errorMessage = null;
            return true;
        }

        private IAdapter SelectAdapter(object targetObject)
        {
            return _adapterFactory.Create(targetObject);
        }
    }
}
