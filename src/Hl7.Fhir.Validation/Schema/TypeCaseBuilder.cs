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

        public IAssertion BuildProfileRef(string type, string profile)
        {
            var uri = new Uri(profile, UriKind.Absolute);
            return type == "Extension"
                ? _assertionFactory.CreateExtensionAssertion(async (u) => await Resolver.GetSchema(u), uri)
                : _assertionFactory.CreateReferenceAssertion(async () => await Resolver.GetSchema(uri), uri);
        }
    }
}