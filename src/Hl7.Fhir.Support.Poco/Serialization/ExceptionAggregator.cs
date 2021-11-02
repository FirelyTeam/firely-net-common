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
    internal class ExceptionAggregator : IEnumerable<JsonFhirException>
    {
        public List<JsonFhirException> _aggregated = new();

        public void Add(JsonFhirException? e)
        {
            if(e is not null)
                _aggregated.Add(e);
        }

        public bool HasExceptions => _aggregated.Count > 0;

        public IEnumerator<JsonFhirException> GetEnumerator() => ((IEnumerable<JsonFhirException>)_aggregated).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_aggregated).GetEnumerator();
    }


}

#nullable restore
#endif