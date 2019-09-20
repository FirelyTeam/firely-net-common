/* 
 * Copyright (c) 2019, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Validation.Schema;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hl7.Fhir.Validation.Impl
{
    public class CardinalityAssertion : IAssertion, IGroupValidatable
    {
        private readonly int? _min;
        private readonly string _max;

        public CardinalityAssertion(int? min, string max)
        {
            if (min.HasValue && min.Value < 0)
                throw new IncorrectElementDefinitionException("min cannot be lower than 0");

            if (max != null && ((!int.TryParse(max, out int maximum) && max != "*") || maximum < 0))
                throw new IncorrectElementDefinitionException("max SHALL be a positive number or '*'");

            _min = min;
            _max = max;
        }

        public IList<(Assertions, ITypedElement)> Validate(IEnumerable<ITypedElement> input, ValidationContext vc)
        {
            var result = new List<(Assertions, ITypedElement)>();

            var count = input.Count();
            if (!InRange(count))
            {
                result.Add((Assertions.Failure + new IssueAssertion(1028, "TODO: Unknow location", "message"), null));
            }
            return result;
        }

        private bool InRange(int x)
        {
            if (_min.HasValue && x < _min.Value)
                return false;

            if (_max == "*" || _max == null)
                return true;

            int max = Convert.ToInt16(_max);
            return x <= max;
        }

        public JToken ToJson()
        {
            return new JProperty("Cardinality", $"{_min?.ToString() ?? "<-"}..{_max ?? "->"}");
        }
    }
}
