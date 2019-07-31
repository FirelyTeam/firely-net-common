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
        public readonly ISchemaResolver Resolver;

        public TypeCaseBuilder(ISchemaResolver resolver)
        {
            Resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
        }

        public ReferenceAssertion BuildProfileRef(string profile)
        {
            var uri = new Uri(profile, UriKind.Absolute);
            return new ReferenceAssertion(() => Resolver.GetSchema(uri), uri);
        }
    }
}