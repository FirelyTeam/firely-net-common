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
using System.Linq;

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

        public Func<IAssertion, bool> Filter;

    }

    public static class ValidationContextExtensions
    {
        public static bool Foo(this ValidationContext vc, IAssertion assertion)
        {

            return vc?.ValidateAssertions?.Any(a => a == assertion.GetType()) ?? false;

        }
    }
}
