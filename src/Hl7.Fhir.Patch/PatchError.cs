/* 
 * Copyright (c) 2020, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/fhir-net-api/blob/master/LICENSE
 */

using System;
using Hl7.Fhir.Patch.Operations;

namespace Hl7.Fhir.Patch
{
    /// <summary>
    /// Captures error message and the related entity and the operation that caused it.
    /// </summary>
    public class PatchError
    {
        /// <summary>
        /// Initializes a new instance of <see cref="PatchError"/>.
        /// </summary>
        /// <param name="affectedObject">The object that is affected by the error.</param>
        /// <param name="operation">The <see cref="Operation"/> that caused the error.</param>
        /// <param name="errorMessage">The error message.</param>
        public PatchError(
            object affectedObject,
            Operation operation,
            string errorMessage)
        {
            if (errorMessage == null)
            {
                throw new ArgumentNullException(nameof(errorMessage));
            }

            AffectedObject = affectedObject;
            Operation = operation;
            ErrorMessage = errorMessage;
        }

        /// <summary>
        /// Gets the object that is affected by the error.
        /// </summary>
        public object AffectedObject { get; }

        /// <summary>
        /// Gets the <see cref="Operation"/> that caused the error.
        /// </summary>
        public Operation Operation { get; }

        /// <summary>
        /// Gets the error message.
        /// </summary>
        public string ErrorMessage { get; }
    }
}