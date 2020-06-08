/* 
 * Copyright (c) 2019, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

using Hl7.Fhir.Validation.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hl7.Fhir.Validation.Schema
{
    public class TypeCaseBuilder
    {
        private readonly IElementDefinitionAssertionFactory _assertionFactory;
        public readonly ISchemaResolver Resolver;

        public TypeCaseBuilder(ISchemaResolver resolver, IElementDefinitionAssertionFactory assertionFactory)
        {
            Resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
            _assertionFactory = assertionFactory;
        }

        public IAssertion BuildProfileRef(string type, string profile)
        {
            var uri = new Uri(profile, UriKind.Absolute);
            return type == "Extension" // TODO: some constant.
                ? _assertionFactory.CreateExtensionAssertion(async (u) => await Resolver.GetSchema(u).ConfigureAwait(false), uri)
                : _assertionFactory.CreateReferenceAssertion(async () => await Resolver.GetSchema(uri).ConfigureAwait(false), uri);
        }

        public IAssertion BuildProfileRef(string profile)
        {
            var uri = new Uri(profile, UriKind.Absolute);
            return _assertionFactory.CreateReferenceAssertion(async () => await Resolver.GetSchema(uri).ConfigureAwait(false), uri);
        }

        public IAssertion BuildSliceAssertionForTypeCases(IEnumerable<(string code, IEnumerable<string> profiles)> typeCases)
        {
            // typeCases have a unique key, so there's only one default case, even though
            // we use SelectMany(). It's either empty or has a list of profiles for the only
            // default case (where Code = null in the typeref)
            var defaultCases = typeCases.Where(tc => tc.code == null)
                .SelectMany(tc => tc.profiles);
            var sliceCases = typeCases.Where(tc => tc.code != null)
                .Select(tc => buildSliceForTypeCase(tc.code, tc.profiles));

            var defaultSlice =
                defaultCases.Any() ?
                    BuildSliceForProfiles(defaultCases) as IAssertion : buildSliceFailure();

            return _assertionFactory.CreateSliceAssertion(ordered: false, @default: defaultSlice, sliceCases);

            IAssertion buildSliceFailure()
            {
                var allowedCodes = String.Join(",", typeCases.Select(t => $"'{t.code ?? "(any)"}'"));
                return
                    ResultAssertion.CreateFailure(
                        new IssueAssertion(Issue.CONTENT_ELEMENT_FAILS_SLICING_RULE, "TODO: location?", $"Element is a choice, but the instance does not use one of the allowed choice types ({allowedCodes})"));
            }

            SliceAssertion.Slice buildSliceForTypeCase(string code, IEnumerable<string> profiles)
                => _assertionFactory.CreateSlice(code, new FhirTypeLabel(code), BuildSliceForProfiles(profiles));
        }

        public IAssertion BuildSliceForProfiles(IEnumerable<string> profiles)
        {
            // "special" case, only one possible profile, no need to build a nested
            // discriminatorless slicer to validate possible options
            if (profiles.Count() == 1) return BuildProfileRef(profiles.Single());

            var sliceCases = profiles.Select(p => buildSliceForProfile(p));

            return _assertionFactory.CreateSliceAssertion(ordered: false, @default: buildSliceFailure(), sliceCases);

            IAssertion buildSliceFailure()
            {
                var allowedProfiles = String.Join(",", profiles.Select(p => $"'{p}'"));
                return
                    ResultAssertion.CreateFailure(
                        new IssueAssertion(-1, $"Element does not validate against any of the expected profiles ({allowedProfiles})", IssueSeverity.Error));
            }

            SliceAssertion.Slice buildSliceForProfile(string profile)
                => _assertionFactory.CreateSlice(makeSliceName(profile), BuildProfileRef(profile), ResultAssertion.Success);

            string makeSliceName(string profile)
            {
                var sb = new StringBuilder();
                foreach (var c in profile)
                {
                    if (Char.IsLetterOrDigit(c))
                        sb.Append(c);
                }
                return sb.ToString();

            }
        }
    }
}