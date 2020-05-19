/* 
 * Copyright (c) 2020, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/fhir-net-api/blob/master/LICENSE
 */

using System;
using System.Collections.Generic;
using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Patch.Adapters;
using Hl7.Fhir.Patch.Exceptions;
using Hl7.Fhir.Patch.Internal;
using Hl7.Fhir.Patch.Operations;
using Hl7.Fhir.Specification;
using Hl7.FhirPath;

namespace Hl7.Fhir.Patch
{
    // Implementation details: the purpose of this type of patch document is to allow creation of such
    // documents for cases where there's no class/DTO to work on. Typical use case: backend not built in
    // .NET or architecture doesn't contain a shared DTO layer.
    public class PatchDocument : IPatchDocument
    {
        public List<Operation> Operations { get; private set; }

        public IStructureDefinitionSummaryProvider ContractResolver { get; set; }

        public PatchDocument()
        {
            Operations = new List<Operation>();
            ContractResolver = null;
        }

        public PatchDocument(List<Operation> operations, IStructureDefinitionSummaryProvider contractResolver)
        {
            if (operations == null)
            {
                throw new ArgumentNullException(nameof(operations));
            }

            if ( contractResolver == null )
            {
                throw new ArgumentNullException(nameof(contractResolver));
            }

            Operations = operations;
            ContractResolver = contractResolver;
        }

        /// <summary>
        /// Add operation.  Will result in, for example,
        /// { "op": "add", "path": "/a/b", "name": "c", "value": [ "foo", "bar" ] }
        /// </summary>
        /// <param name="path">target location</param>
        /// <param name="name">name of the element to add</param>
        /// <param name="value">value</param>
        /// <returns>The <see cref="PatchDocument"/> for chaining.</returns>
        public PatchDocument Add(CompiledExpression path, string name, object value)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if ( name == null )
            {
                throw new ArgumentNullException(nameof(name));
            }

            Operations.Add(new Operation(OperationType.Add.ToString("G"), path, name, value));
            return this;
        }

        /// <summary>
        /// Insert operation.  Will result in, for example,
        /// { "op": "insert", "path": "/a/b/c", "index": 2, "value": { "foo": "bar" } }
        /// </summary>
        /// <param name="path">target location</param>
        /// <param name="index">target index</param>
        /// <param name="value">value</param>
        /// <returns>The <see cref="PatchDocument"/> for chaining.</returns>
        public PatchDocument Insert (CompiledExpression path, int index, object value)
        {
            if ( path == null )
            {
                throw new ArgumentNullException(nameof(path));
            }

            Operations.Add(new Operation(OperationType.Insert.ToString("G"), path, index, value));
            return this;
        }

        /// <summary>
        /// Delete value at target location.  Will result in, for example,
        /// { "op": "delete", "path": "/a/b/c" }
        /// </summary>
        /// <param name="path">target location</param>
        /// <returns>The <see cref="PatchDocument"/> for chaining.</returns>
        public PatchDocument Delete(CompiledExpression path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            Operations.Add(new Operation(OperationType.Delete.ToString("G"), path));
            return this;
        }

        /// <summary>
        /// Replace value.  Will result in, for example,
        /// { "op": "replace", "path": "/a/b/c", "value": 42 }
        /// </summary>
        /// <param name="path">target location</param>
        /// <param name="value">value</param>
        /// <returns>The <see cref="PatchDocument"/> for chaining.</returns>
        public PatchDocument Replace(CompiledExpression path, object value)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            Operations.Add(new Operation(OperationType.Replace.ToString("G"), path, value));
            return this;
        }

        /// <summary>
        /// Removes value at specified location and add it to the target location.  Will result in, for example:
        /// { "op": "move", "source": 3, "destination": 5, "path": "/a/b/d" }
        /// </summary>
        /// <param name="path">target location</param>
        /// <param name="source">index to move element from</param>
        /// <param name="destination">index to move element to</param>
        /// <returns>The <see cref="PatchDocument"/> for chaining.</returns>
        public PatchDocument Move(CompiledExpression path, int source, int destination)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            Operations.Add(new Operation(OperationType.Move.ToString("G"), path, source, destination));
            return this;
        }

        /// <summary>
        /// Apply this PatchDocument
        /// </summary>
        /// <param name="objectToApplyTo">Object to apply the PatchDocument to</param>
        public void ApplyTo(ElementNode objectToApplyTo)
        {
            if (objectToApplyTo == null)
            {
                throw new ArgumentNullException(nameof(objectToApplyTo));
            }

            ApplyTo(objectToApplyTo, new ObjectAdapter(ContractResolver, null, new AdapterFactory()));
        }

        /// <summary>
        /// Apply this PatchDocument
        /// </summary>
        /// <param name="objectToApplyTo">Object to apply the PatchDocument to</param>
        /// <param name="logErrorAction">Action to log errors</param>
        public void ApplyTo(ElementNode objectToApplyTo, Action<PatchError> logErrorAction)
        {
            ApplyTo(objectToApplyTo, new ObjectAdapter(ContractResolver, logErrorAction, new AdapterFactory()), logErrorAction);
        }

        /// <summary>
        /// Apply this PatchDocument
        /// </summary>
        /// <param name="objectToApplyTo">Object to apply the PatchDocument to</param>
        /// <param name="adapter">IObjectAdapter instance to use when applying</param>
        /// <param name="logErrorAction">Action to log errors</param>
        public void ApplyTo(ElementNode objectToApplyTo, IObjectAdapter adapter, Action<PatchError> logErrorAction)
        {
            if (objectToApplyTo == null)
            {
                throw new ArgumentNullException(nameof(objectToApplyTo));
            }

            if (adapter == null)
            {
                throw new ArgumentNullException(nameof(adapter));
            }

            foreach (var op in Operations)
            {
                try
                {
                    op.Apply(objectToApplyTo, adapter);
                }
                catch (PatchException PatchException)
                {
                    var errorReporter = logErrorAction ?? ErrorReporter.Default;
                    errorReporter(new PatchError(objectToApplyTo, op, PatchException.Message));

                    // As per  Patch spec if an operation results in error, further operations should not be executed.
                    break;
                }
            }
        }

        /// <summary>
        /// Apply this PatchDocument
        /// </summary>
        /// <param name="objectToApplyTo">Object to apply the PatchDocument to</param>
        /// <param name="adapter">IObjectAdapter instance to use when applying</param>
        public void ApplyTo(ElementNode objectToApplyTo, IObjectAdapter adapter)
        {
            if (objectToApplyTo == null)
            {
                throw new ArgumentNullException(nameof(objectToApplyTo));
            }

            if (adapter == null)
            {
                throw new ArgumentNullException(nameof(adapter));
            }

            // apply each operation in order
            foreach (var op in Operations)
            {
                op.Apply(objectToApplyTo, adapter);
            }
        }

        IList<Operation> IPatchDocument.GetOperations()
        {
            var allOps = new List<Operation>();

            if (Operations != null)
            {
                foreach (var op in Operations)
                {
                    var untypedOp = new Operation
                    {
                        op = op.op,
                        value = op.value,
                        path = op.path,
                        name = op.name,
                        index = op.index,
                        source = op.source,
                        destination = op.destination
                    };

                    allOps.Add(untypedOp);
                }
            }

            return allOps;
        }
    }
}
