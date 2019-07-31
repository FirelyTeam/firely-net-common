/* 
 * Copyright (c) 2019, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

using Hl7.Fhir.Utility;

namespace Hl7.Fhir.Validation.Schema
{
    public class ValidationContext
    {
        public ITerminologyServiceNEW TerminologyService;

        public IExceptionSource ExceptionSink;
    }
}
