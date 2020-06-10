/* 
 * Copyright (c) 2019, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */


using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Utility;
using Hl7.Fhir.Validation.Support;
using Hl7.FhirPath.Sprache;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hl7.Fhir.Validation.Schema
{
    public class ReferenceAssertion : IAssertion, IGroupValidatable
    {
        private const string RESOURCE_URI = "http://hl7.org/fhir/StructureDefinition/Resource";
        private const string REFERENCE_URI = "http://hl7.org/fhir/StructureDefinition/Reference";

        private readonly Func<Uri, Task<IElementSchema>> _getSchema;
        private readonly IEnumerable<AggregationMode?> _aggregations;

        public ReferenceAssertion(IElementSchema schema, Uri referencedUri = null, IEnumerable<AggregationMode?> aggregations = null) :
            this((u) => Task.FromResult(schema), schema.Id, aggregations)
        {
        }

        public ReferenceAssertion(Func<Uri, Task<IElementSchema>> getschema, Uri referencedUri, IEnumerable<AggregationMode?> aggregations = null)
        {
            _getSchema = getschema;
            ReferencedUri = referencedUri;
            _aggregations = aggregations;
        }

        public Uri ReferencedUri { get; private set; }

        private bool HasAggregation => _aggregations?.Any() ?? false;

        public async Task<Assertions> Validate(IEnumerable<ITypedElement> input, ValidationContext vc)
            => (ReferencedUri?.ToString()) switch
            {
                RESOURCE_URI => await input.Select(i => ValidationExtensions.Validate(_getSchema, getCanonical(i), i, vc)).AggregateAsync(),
                REFERENCE_URI => await input.Select(i => ValidateReference(i, vc)).AggregateAsync(),
                _ => await ValidationExtensions.Validate(_getSchema, ReferencedUri, input, vc)
            };

        private async Task<Assertions> ValidateReference(ITypedElement input, ValidationContext vc)
        {
            var result = Assertions.Empty;

            var instance = input as ScopedNode;
            var reference = instance.ParseReference();

            result += resolveReference(instance, reference, out (ITypedElement referencedResource, AggregationMode? encounteredKind) referenceInstance);
            var referencedResource = referenceInstance.referencedResource;

            result += ValidateAggregation(referenceInstance);


            // Bail out if we are asked to follow an *external reference* when this is disabled in the settings
            if (vc.ResolveExternalReferences == false && referenceInstance.encounteredKind == AggregationMode.Referenced)
                return result;

            // If we failed to find a referenced resource within the current instance, try to resolve it using an external method
            //TODO
            if (referencedResource == null && referenceInstance.encounteredKind == AggregationMode.Referenced)
            {
                try
                {
                    referencedResource = vc.ExternalReferenceResolutionNeeded(reference, instance.Location, result);
                }
                catch (Exception e)
                {
                    result += ResultAssertion.CreateFailure(new IssueAssertion(
                        Issue.UNAVAILABLE_REFERENCED_RESOURCE, instance.Location,
                        $"Resolution of external reference {reference} failed. Message: {e.Message}"));
                }
            }

            // If the reference was resolved (either internally or externally), validate it
            result = await ValidateReferencedResource(vc, instance, reference, referenceInstance, referencedResource);

            return result;
        }

        private async Task<Assertions> ValidateReferencedResource(ValidationContext vc, ScopedNode instance, string reference, (ITypedElement referencedResource, AggregationMode? encounteredKind) referenceInstance, ITypedElement referencedResource)
        {
            var result = Assertions.Empty;

            if (referencedResource != null)
            {
                //result += Trace($"Starting validation of referenced resource {reference} ({encounteredKind})");

                // References within the instance are dealt with within the same validator,
                // references to external entities will operate within a new instance of a validator (and hence a new tracking context).
                // In both cases, the outcome is included in the result.
                //OperationOutcome childResult;

                // TODO: BRIAN: Check that this TargetProfile.FirstOrDefault() is actually right, or should
                //              we be permitting more than one target profile here.
                if (referenceInstance.encounteredKind != AggregationMode.Referenced)
                {
                    result += await ValidationExtensions.Validate(_getSchema, getCanonical(referencedResource), referencedResource, vc);
                }
                else
                {
                    // TODO

                    //var newValidator = validator.NewInstance();
                    //childResult = newValidator.ValidateReferences(referencedResource, typeRef.TargetProfile);
                }
            }
            else
            {
                result += ResultAssertion.CreateFailure(new IssueAssertion(
                    Issue.UNAVAILABLE_REFERENCED_RESOURCE, instance.Location,
                    $"Cannot resolve reference {reference}"));
            }

            return result;
        }

        private Assertions ValidateAggregation((ITypedElement referencedResource, AggregationMode? encounteredKind) referenceInstance)
        {
            var result = Assertions.Empty;

            // Validate the kind of aggregation.
            // If no aggregation is given, all kinds of aggregation are allowed, otherwise only allow
            // those aggregation types that are given in the Aggregation element
            if (HasAggregation && !_aggregations.Any(a => a == referenceInstance.encounteredKind))
            {
                result += ResultAssertion.CreateFailure(new IssueAssertion(Issue.CONTENT_REFERENCE_OF_INVALID_KIND, "TODO", $"Encountered a reference ({referenceInstance.referencedResource}) of kind '{referenceInstance.encounteredKind}' which is not allowed"));
            }

            return result;
        }

        private Assertions resolveReference(ScopedNode instance, string reference, out (ITypedElement, AggregationMode?) referenceInstance)
        {
            var result = Assertions.Empty;
            var identity = new ResourceIdentity(reference);

            if (identity.Form == ResourceIdentityForm.Undetermined)
            {
                if (!Uri.IsWellFormedUriString(Uri.EscapeDataString(reference), UriKind.RelativeOrAbsolute))
                {
                    result += ResultAssertion.CreateFailure(new IssueAssertion(Issue.CONTENT_UNPARSEABLE_REFERENCE, "TODO", $"Encountered an unparseable reference ({reference}"));
                    referenceInstance = (null, null);
                    return result;
                }
            }

            var referencedResource = instance.Resolve(reference);
            AggregationMode? aggregationMode = null;

            if (identity.Form == ResourceIdentityForm.Local)
            {
                aggregationMode = AggregationMode.Contained;
                if (referencedResource == null)
                    result += ResultAssertion.CreateFailure(new IssueAssertion(Issue.CONTENT_CONTAINED_REFERENCE_NOT_RESOLVABLE, "TODO", $"Contained reference ({reference}) is not resolvable"));
            }
            else
            {
                aggregationMode = referencedResource != null ? AggregationMode.Bundled : AggregationMode.Referenced;
            }

            referenceInstance = (referencedResource, aggregationMode);
            return result;
        }


        private static Uri getCanonical(ITypedElement input)
            => new Uri($"{ResourceIdentity.CORE_BASE_URL}{input.InstanceType}");

        public JToken ToJson() => new JProperty("$ref", ReferencedUri?.ToString() ??
            throw Error.InvalidOperation("Cannot convert to Json: reference refers to a schema without an identifier"));
    }
}
