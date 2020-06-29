/* 
 * Copyright (c) 2020, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/fhir-net-api/blob/master/LICENSE
 */

using System;
using Hl7.Fhir.Patch.Adapters;
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

        public override void Apply(object objectToApplyTo, IObjectAdapter adapter)
        {
            if (objectToApplyTo == null)
            {
                throw new ArgumentNullException(nameof(objectToApplyTo));
            }

            if (adapter == null)
            {
                throw new ArgumentNullException(nameof(adapter));
            }

            adapter.Insert(this, objectToApplyTo);
        }
    }
}