/* 
 * Copyright (c) 2019, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

using Hl7.Fhir.Utility;
using Hl7.FhirPath;
using System;

namespace Hl7.Fhir.Validation.Schema
{
    public enum ValidateBestPractices
    {
        Ignore,
        Enabled,
        Disabled
    }
    public class ValidationContext
    {
        public ITerminologyServiceNEW TerminologyService;

        public IExceptionSource ExceptionSink;

        /// <summary>
        /// An instance of the FhirPath compiler to use when evaluating constraints
        /// (provide this if you have custom functions included in the symbol table)
        /// </summary>
        public FhirPathCompiler FhirPathCompiler;

        public ValidateBestPractices ConstraintBestPractices = ValidateBestPractices.Ignore;

        public Type[] ValidateAssertions;

        /// <summary>
        /// A function to include the assertion in the validation or not. If the function is left empty (null) then all the 
        /// assertions are processed in the validation.
        /// </summary>
        public Func<IAssertion, bool> IncludeFilter;

    }
}
