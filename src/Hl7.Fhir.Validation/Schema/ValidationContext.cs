/* 
 * Copyright (c) 2019, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Specification.Source;
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

        public bool ResolveExternalReferences; // = false;

        public event EventHandler<OnResolveResourceReferenceEventArgs> OnExternalResolutionNeeded;

        /// <summary>
        /// An instance of the FhirPath compiler to use when evaluating constraints
        /// (provide this if you have custom functions included in the symbol table)
        /// </summary>
        public FhirPathCompiler FhirPathCompiler;

        public ValidateBestPractices ConstraintBestPractices = ValidateBestPractices.Ignore;

        public Type[] ValidateAssertions;

        public IResourceResolver ResourceResolver;

        public Func<string, IResourceResolver, ITypedElement> ToTypedElement;

        /// <summary>
        /// A function to include the assertion in the validation or not. If the function is left empty (null) then all the 
        /// assertions are processed in the validation.
        /// </summary>
        public Func<IAssertion, bool> IncludeFilter;

        /// <summary>
        /// A function to exclude the assertion in the validation or not. If the function is left empty (null) then all the 
        /// assertions are processed in the validation.
        /// </summary>
        public Func<IAssertion, bool> ExcludeFilter;

        public Func<IAssertion, bool> Filter =>
            a =>
                (IncludeFilter is null || IncludeFilter(a)) &&
                (ExcludeFilter is null || !ExcludeFilter(a));

        internal ITypedElement ExternalReferenceResolutionNeeded(string reference, string path, Assertions assertions)
        {
            if (!ResolveExternalReferences) return null;

            try
            {
                // Default implementation: call event
                if (OnExternalResolutionNeeded != null)
                {
                    var args = new OnResolveResourceReferenceEventArgs(reference);
                    OnExternalResolutionNeeded(this, args);
                    return args.Result;
                }
            }
            catch (Exception e)
            {
                assertions += ResultAssertion.CreateFailure(new IssueAssertion(
                        Issue.UNAVAILABLE_REFERENCED_RESOURCE, path,
                        $"External resolution of '{reference}' caused an error: " + e.Message));
            }


            // Else, try to resolve using the given ResourceResolver 
            // (note: this also happens when the external resolution above threw an exception)
            if (ResourceResolver != null && ToTypedElement != null)
            {
                try
                {
                    return ToTypedElement(reference, ResourceResolver);
                }
                catch (Exception e)
                {
                    assertions += ResultAssertion.CreateFailure(new IssueAssertion(
                        Issue.UNAVAILABLE_REFERENCED_RESOURCE, path,
                        $"Resolution of reference '{reference}' using the Resolver API failed: " + e.Message));
                }
            }

            return null;        // Sorry, nothing worked
        }

        /// <summary>Creates a new <see cref="ValidationContext"/> instance with default property values.</summary>
        public static ValidationContext CreateDefault() => new ValidationContext();
    }

    public class OnResolveResourceReferenceEventArgs : EventArgs
    {
        public OnResolveResourceReferenceEventArgs(string reference)
        {
            Reference = reference;
        }

        public string Reference { get; }

        public ITypedElement Result { get; set; }
    }
}
