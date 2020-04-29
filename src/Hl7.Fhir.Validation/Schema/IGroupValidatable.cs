/* 
* Copyright (c) 2019, Firely (info@fire.ly) and contributors
* See the file CONTRIBUTORS for details.
* 
* This file is licensed under the BSD 3-Clause license
* available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
*/

using Hl7.Fhir.ElementModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hl7.Fhir.Validation.Schema
{
    public interface IGroupValidatable
    {
        Task<Assertions> Validate(IEnumerable<ITypedElement> input, ValidationContext vc);
    }
}
