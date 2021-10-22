/* 
 * Copyright (c) 2021, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-net-sdk/master/LICENSE
 */


#if NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;

#nullable enable

namespace Hl7.Fhir.Serialization
{
    internal class ExceptionAggregator
    {
        public List<Exception> _aggregated = new();

        public void Add(Exception? e)
        {
            if(e is not null)
                _aggregated.Add(e);
        }

        public bool HasExceptions => _aggregated.Count > 0;

        public Exception? Aggregate()
        {
            if (_aggregated.Count == 0) return null;

            return _aggregated.Count != 1 ? new AggregateException(_aggregated) : _aggregated.Single();
        }
    }


}

#nullable restore
#endif