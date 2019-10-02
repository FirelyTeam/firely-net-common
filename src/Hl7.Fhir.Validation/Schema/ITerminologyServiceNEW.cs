/* 
 * Copyright (c) 2019, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

using Hl7.Fhir.Model.Primitives;

namespace Hl7.Fhir.Validation.Schema
{
    public interface ITerminologyServiceNEW
    {
        Assertions ValidateCode(string canonical = null, string context = null, string code = null,
                    string system = null, string version = null, string display = null,
                    Coding? coding = null, Concept? codeableConcept = null, PartialDateTime? date = null,
                    bool? @abstract = null, string displayLanguage = null);
    }
}