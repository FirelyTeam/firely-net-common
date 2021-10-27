/* 
 * Copyright (c) 2021, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-net-sdk/master/LICENSE
 */


#if NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER
using System;

#nullable enable

namespace Hl7.Fhir.Serialization
{
    /// <summary>
    /// This record hold the result of deserialization, be it an error, a (partial) result or both.
    /// </summary>
    /// <remarks>Returning the partial result is useful for usecases where the caller wants to report an error,
    /// but it still able to deal with partial/incorrect data.</remarks>    
    internal record PartialDeserialization<T>(T? PartialResult, Exception? Exception)
    {
        public PartialDeserialization<U> Cast<U>() => new((U?)(object?)PartialResult, Exception);

        public bool Success => Exception is null;
    }


}

#nullable restore
#endif