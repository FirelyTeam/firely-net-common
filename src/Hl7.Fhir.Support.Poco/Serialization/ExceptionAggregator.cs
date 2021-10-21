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
using System.Linq;
using System.Text.Json;

#nullable enable

namespace Hl7.Fhir.Serialization
{
    internal class ExceptionAggregator
    {
        public delegate void ReaderFunction(ref Utf8JsonReader reader);

        public List<Exception> _aggregated = new();

        public static ExceptionAggregator Once(ref Utf8JsonReader reader, ReaderFunction a)
        {
            var aggregator = new ExceptionAggregator();
            aggregator.Try(ref reader, a);

            return aggregator;
        }

        public void Try(ref Utf8JsonReader reader, ReaderFunction a)
        {
            try
            {
                a(ref reader);
            }
            catch (Exception ex) when (ex is JsonFhirException or AggregateException)
            {
                _aggregated.Add(ex);
            }

            return;
        }

        public bool HasExceptions => _aggregated.Count > 0;

        public Exception Aggregate()
        {
            if (_aggregated.Count == 0) throw new InvalidOperationException();

            return _aggregated.Count != 1 ? new AggregateException(_aggregated) : _aggregated.Single();
        }

        public void Throw()
        {
            if (HasExceptions)
                throw Aggregate();
            else
                return;
        }
    }


}

#nullable restore
#endif