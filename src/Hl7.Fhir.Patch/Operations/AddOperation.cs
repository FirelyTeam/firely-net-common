/* 
 * Copyright (c) 2020, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/fhir-net-api/blob/master/LICENSE
 */

using System;
using Hl7.Fhir.Patch.Internal;
using Hl7.FhirPath;

namespace Hl7.Fhir.Patch.Operations
{
    public class AddOperation : OperationWithValue
    {
        public string Name { get; }

        /// <summary>
        /// Initializes <see cref="OperationType.Add"/> operation
        /// </summary>
        /// <param name="path">target location</param>
        /// <param name="name">name of the element to add</param>
        /// <param name="value">new value for the target element</param>
        public AddOperation (CompiledExpression path, string name, object value)
            : base(OperationType.Add, path, value)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public override void Apply(object objectToApplyTo, PatchHelper patchHelper)
        {
            if (patchHelper == null)
            {
                throw new ArgumentNullException(nameof(patchHelper));
            }

            if (objectToApplyTo == null)
            {
                throw new ArgumentNullException(nameof(objectToApplyTo));
            }

            var visitor = new ObjectVisitor(Path, patchHelper.AdapterFactory);

            object target = objectToApplyTo;
            if ( !visitor.TryVisit(patchHelper.Provider, ref target, out var adapter, out var errorMessage) )
            {
                var error = patchHelper.CreatePathNotFoundError(objectToApplyTo, this, errorMessage);
                patchHelper.ErrorReporter(error);
                return;
            }

            if ( !adapter.TryAdd(target, Name, Value, out errorMessage) )
            {
                var error = patchHelper.CreateOperationFailedError(objectToApplyTo, this, errorMessage);
                patchHelper.ErrorReporter(error);
                return;
            }
        }
    }
}