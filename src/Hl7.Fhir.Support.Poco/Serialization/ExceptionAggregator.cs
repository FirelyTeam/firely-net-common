/* 
 * Copyright (c) 2021, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-net-sdk/master/LICENSE
 */


#if NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;

#nullable enable

namespace Hl7.Fhir.Serialization
{
    /// <summary>
    /// This class is used by the <see cref="FhirJsonPocoDeserializer"/> to collect errors while performing deserialization.
    /// </summary>
    /// <remarks>Is probably going to be used by the (future)XmlDynamicDeserializer too.</remarks>
    internal class ExceptionAggregator : IEnumerable<FhirJsonException>
    {
        public List<FhirJsonException> _aggregated = new();

        public void Add(FhirJsonException? e)
        {
            if(e is not null)
                _aggregated.Add(e);
        }

        public bool HasExceptions => _aggregated.Count > 0;

        public int Count => _aggregated.Count;

        public IEnumerator<FhirJsonException> GetEnumerator() => ((IEnumerable<FhirJsonException>)_aggregated).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_aggregated).GetEnumerator();
    }


}

#nullable restore
#endif