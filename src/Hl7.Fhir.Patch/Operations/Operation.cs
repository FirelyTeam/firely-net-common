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
    public class Operation : OperationBase
    {
        public object value { get; set; }

        public Operation()
        {

        }

        /// <summary>
        /// Initializes <see cref="OperationType.Delete"/> operation
        /// </summary>
        /// <param name="op">name of the operation</param>
        /// <param name="path">target location</param>
        public Operation (string op, CompiledExpression path)
            : base(op, path)
        {

        }

        /// <summary>
        /// Initializes <see cref="OperationType.Replace"/> operation
        /// </summary>
        /// <param name="op">name of the operation</param>
        /// <param name="path">target location</param>
        /// <param name="value">new value for the target element</param>
        public Operation(string op, CompiledExpression path, object value)
            : base(op, path)
        {
            this.value = value;
        }

        /// <summary>
        /// Initializes <see cref="OperationType.Add"/> operation
        /// </summary>
        /// <param name="op">name of the operation</param>
        /// <param name="path">target location</param>
        /// <param name="name">name of the element to add</param>
        /// <param name="value">new value for the target element</param>
        public Operation (string op, CompiledExpression path, string name, object value)
            : base(op, path, name)
        {
            this.value = value;
        }

        /// <summary>
        /// Initializes <see cref="OperationType.Insert"/> operation
        /// </summary>
        /// <param name="op">name of the operation</param>
        /// <param name="path">target location</param>
        /// <param name="index">target index</param>
        /// <param name="value">element to insert</param>
        public Operation (string op, CompiledExpression path, int index, object value)
            : base(op, path, index)
        {
            this.value = value;
        }

        /// <summary>
        /// Initializes <see cref="OperationType.Move"/> operation
        /// </summary>
        /// <param name="op">name of the operation</param>
        /// <param name="path">target location</param>
        /// <param name="source">index to move element from</param>
        /// <param name="destination">index to move element to</param>
        public Operation (string op, CompiledExpression path, int source, int destination)
            : base(op, path, source, destination)
        {
        }

        public void Apply(object objectToApplyTo, IObjectAdapter adapter)
        {
            if (objectToApplyTo == null)
            {
                throw new ArgumentNullException(nameof(objectToApplyTo));
            }

            if (adapter == null)
            {
                throw new ArgumentNullException(nameof(adapter));
            }

            switch (OperationType)
            {
                case OperationType.Add:
                    adapter.Add(this, objectToApplyTo);
                    break;
                case OperationType.Insert:
                    adapter.Insert(this, objectToApplyTo);
                    break;
                case OperationType.Delete:
                    adapter.Delete(this, objectToApplyTo);
                    break;
                case OperationType.Replace:
                    adapter.Replace(this, objectToApplyTo);
                    break;
                case OperationType.Move:
                    adapter.Move(this, objectToApplyTo);
                    break;
                default:
                    break;
            }
        }

        public bool ShouldSerializevalue ()
        {
            return (OperationType == OperationType.Add
                || OperationType == OperationType.Insert
                || OperationType == OperationType.Replace);
        }
    }
}