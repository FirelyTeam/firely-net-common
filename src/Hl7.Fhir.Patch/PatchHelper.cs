/* 
 * Copyright (c) 2020, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/fhir-net-api/blob/master/LICENSE
 */

using System;
using Hl7.Fhir.Patch.Adapters;
using Hl7.Fhir.Patch.Operations;
using Hl7.Fhir.Specification;

namespace Hl7.Fhir.Patch
{
    /// <inheritdoc />
    public class PatchHelper
    {
        /// <summary>
        /// Initializes a new instance of <see cref="PatchHelper"/>.
        /// </summary>
        /// /// <param name="provider">The <see cref="IStructureDefinitionSummaryProvider"/>.</param>
        /// <param name="logErrorAction">The <see cref="Action"/> for logging <see cref="PatchError"/>.</param>
        public PatchHelper (
            IStructureDefinitionSummaryProvider provider,
            Action<PatchError> logErrorAction):
            this(provider, logErrorAction, new AdapterFactory())
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="PatchHelper"/>.
        /// </summary>
        /// <param name="provider">The <see cref="IStructureDefinitionSummaryProvider"/>.</param>
        /// <param name="logErrorAction">The <see cref="Action"/> for logging <see cref="PatchError"/>.</param>
        /// <param name="adapterFactory">The <see cref="IAdapterFactory"/> to use when creating adaptors.</param>
        public PatchHelper (
            IStructureDefinitionSummaryProvider provider,
            Action<PatchError> logErrorAction,
            IAdapterFactory adapterFactory)
         {
            Provider = provider ?? throw new ArgumentNullException(nameof(provider));
            ErrorReporter = logErrorAction ?? Internal.ErrorReporter.Default;
            AdapterFactory = adapterFactory ?? throw new ArgumentNullException(nameof(adapterFactory));
         }

        /// <summary>
        /// Gets or sets the <see cref="IStructureDefinitionSummaryProvider"/>.
        /// </summary>
        public IStructureDefinitionSummaryProvider Provider { get; }

        /// <summary>
        /// Gets or sets the <see cref="IAdapterFactory"/>
        /// </summary>
        public IAdapterFactory AdapterFactory { get; }

        /// <summary>
        /// Action for reporting <see cref="PatchError"/>.
        /// </summary>
        public Action<PatchError> ErrorReporter { get; }

        public PatchError CreateOperationFailedError(object target, Operation operation, string errorMessage)
        {
            return new PatchError(
                target,
                operation.Parent ?? operation,
                errorMessage ?? $"The '{(operation.Parent ?? operation).OperationType:G}' operation at path could not be performed.");
        }

        public PatchError CreatePathNotFoundError(object target, Operation operation, string errorMessage)
        {
            return new PatchError(
                target,
                operation.Parent ?? operation,
                errorMessage ?? $"For operation '{(operation.Parent ?? operation).OperationType:G}', the target location specified by path was not found.");
        }
    }
}