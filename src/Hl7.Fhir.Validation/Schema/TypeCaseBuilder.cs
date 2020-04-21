/* 
 * Copyright (c) 2019, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

using System;

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

        public IAssertion BuildProfileRef(string profile)
        {
            var uri = new Uri(profile, UriKind.Absolute);
            return _assertionFactory.CreateReferenceAssertion(() => Resolver.GetSchema(uri), uri);
        }
    }
}