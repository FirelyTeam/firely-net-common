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
    public class MoveOperation : Operation
    {
        public int Source { get; }

        public int Destination { get; }

        /// <summary>
        /// Initializes <see cref="OperationType.Move"/> operation
        /// </summary>
        /// <param name="path">target location</param>
        /// <param name="source">index to move element from</param>
        /// <param name="destination">index to move element to</param>
        public MoveOperation (CompiledExpression path, int source, int destination)
            : base(OperationType.Move, path)
        {
            Source = source;
            Destination = destination;
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

            adapter.Move(this, objectToApplyTo);
        }
    }
}