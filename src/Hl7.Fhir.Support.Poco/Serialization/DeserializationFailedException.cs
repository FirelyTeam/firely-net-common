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
using System.Linq;

#nullable enable

namespace Hl7.Fhir.Serialization
{
    /// <summary>
    /// A <see cref="AggregateException"/> that contains the list of errors detected while deserializing data into
    /// .NET POCOs.
    /// </summary>
    /// <remarks>The deserializers will continue deserialization in the face of errors, and so will collect the full
    /// set of errors detected using this aggregate exception.</remarks>
    public class DeserializationFailedException : AggregateException
    {
        public DeserializationFailedException(Base? partialResult, IEnumerable<Exception> innerExceptions) : base(innerExceptions)
        {
            PartialResult = partialResult;
        }

        /// <summary>
        /// The best-effort result of deserialization. Maybe invalid or incomplete because of the errors encountered.
        /// </summary>
        public Base? PartialResult { get; private set; }
    }
}

#nullable restore
#endif