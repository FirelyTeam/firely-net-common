/* 
 * Copyright (c) 2020, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/fhir-net-api/blob/master/LICENSE
 */

using System;
using Hl7.FhirPath;

namespace Hl7.Fhir.Patch.Operations
{
    public abstract class Operation
    {
        public Operation Parent { get; set; } 

        public OperationType OperationType { get; }

        public CompiledExpression Path { get; }


        public Operation (OperationType op, CompiledExpression path)
        {
            OperationType = op;
            Path = path ?? throw new ArgumentNullException(nameof(path));
            Parent = null;
        }

        public abstract void Apply (object objectToApplyTo, PatchHelper patchHelper);
    }
}