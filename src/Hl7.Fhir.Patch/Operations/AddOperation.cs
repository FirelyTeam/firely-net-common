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

            adapter.Add(this, objectToApplyTo);
        }
    }
}