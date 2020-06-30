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
    public class InsertOperation : OperationWithValue
    {
        public int Index { get; }

        /// <summary>
        /// Initializes <see cref="OperationType.Insert"/> operation
        /// </summary>
        /// <param name="path">target location</param>
        /// <param name="index">target index</param>
        /// <param name="value">element to insert</param>
        public InsertOperation (CompiledExpression path, int index, object value)
            : base(OperationType.Insert, path, value)
        {
            Index = index;
        }

        public override void Apply (object objectToApplyTo, PatchHelper patchHelper)
        {
            if ( patchHelper == null )
            {
                throw new ArgumentNullException(nameof(patchHelper));
            }

            if ( objectToApplyTo == null )
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

            if ( !adapter.TryInsert(target, Index, Value, out errorMessage) )
            {
                var error = patchHelper.CreateOperationFailedError(objectToApplyTo, this, errorMessage);
                patchHelper.ErrorReporter(error);
                return;
            }
        }
    }
}