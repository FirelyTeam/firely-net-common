/* 
 * Copyright (c) 2014, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-net-sdk/master/LICENSE
 */

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable enable

namespace Hl7.Fhir.Validation
{
    /// <summary>
    /// Interface used by the <see cref="InvokeIValidatableObjectAttribute" /> to start nested validation.
    /// </summary>
    public interface IValidatableObject
    {
        /// <summary>
        /// Invoke validation on an object that implements this interface.
        /// </summary>
        IEnumerable<ValidationResult> Validate(ValidationContext validationContext);
    }
}

#nullable restore