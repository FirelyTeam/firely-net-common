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
    public abstract class OperationWithValue : Operation
    {
        public object Value { get; }

        public OperationWithValue (OperationType op, CompiledExpression path, object value)
            : base(op, path)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }
    }
}