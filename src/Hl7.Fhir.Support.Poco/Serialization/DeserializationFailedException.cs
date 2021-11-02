/* 
 * Copyright (c) 2021, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-net-sdk/master/LICENSE
 */


#if NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER
using Hl7.Fhir.Model;
using System;
using System.Collections.Generic;

#nullable enable

namespace Hl7.Fhir.Serialization
{
    public class DeserializationFailedException : AggregateException
    {
        public DeserializationFailedException(Base? partialResult, IEnumerable<Exception> innerExceptions) : base(innerExceptions)
        {
            PartialResult = partialResult;
        }

        public Base? PartialResult { get; private set; }
    }
}

#nullable restore
#endif