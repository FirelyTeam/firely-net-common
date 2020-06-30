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

namespace Hl7.Fhir.Patch
{
    // Implementation details: the purpose of this type of patch document is to allow creation of such
    // documents for cases where there's no class/DTO to work on. Typical use case: backend not built in
    // .NET or architecture doesn't contain a shared DTO layer.
    public class PatchDocument : IPatchDocument
    {
        public List<Operation> Operations { get; }

        public IStructureDefinitionSummaryProvider Provider { get; set; }

        public IAdapterFactory AdapterFactory { get; set; }

        public PatchDocument()
        {
            Operations = new List<Operation>();
            AdapterFactory = new AdapterFactory();
            Provider = null;
        }

        public PatchDocument(List<Operation> operations, IStructureDefinitionSummaryProvider provider, IAdapterFactory adapterFactory = null)
        {
            Operations = operations ?? throw new ArgumentNullException(nameof(operations));
            Provider = provider ?? throw new ArgumentNullException(nameof(provider));
            AdapterFactory = adapterFactory ?? new AdapterFactory();
        }

        /// <summary>
        /// Add operation to this PatchDocument
        /// </summary>
        /// <param name="operationToAdd"><see cref="Operation"/> to add to this <see cref="PatchDocument"/></param>
        /// <returns>The <see cref="PatchDocument"/> for chaining.</returns>
        public PatchDocument Add(Operation operationToAdd)
        {
            if (operationToAdd == null)
            {
                throw new ArgumentNullException(nameof(operationToAdd));
            }

            Operations.Add(operationToAdd);
            return this;
        }

        /// <inheritdoc />
        public void ApplyTo (ElementNode objectToApplyTo)
        {
            if (objectToApplyTo == null)
            {
                throw new ArgumentNullException(nameof(objectToApplyTo));
            }

            ApplyTo(objectToApplyTo, new PatchHelper(Provider, null, AdapterFactory));
        }

        /// <inheritdoc />
        public void ApplyTo(ElementNode objectToApplyTo, Action<PatchError> logErrorAction)
        {
            ApplyTo(objectToApplyTo, new PatchHelper(Provider, logErrorAction, AdapterFactory), logErrorAction);
        }

        /// <inheritdoc />
        public void ApplyTo(ElementNode objectToApplyTo, PatchHelper adapter, Action<PatchError> logErrorAction)
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

        /// <inheritdoc />
        public void ApplyTo(ElementNode objectToApplyTo, PatchHelper adapter)
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
    }
}
