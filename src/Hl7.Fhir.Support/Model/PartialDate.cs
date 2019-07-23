/* 
 * Copyright (c) 2015, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */


using Hl7.Fhir.Serialization;
using Hl7.Fhir.Utility;
using System;
using System.Xml;

namespace Hl7.Fhir.Model.Primitives
{
    public struct PartialDate : IComparable
    {
        private string _value;

        public static PartialDate Parse(string value)
        {
            if(value.Length > 10)
                throw new FormatException("Partial date is too long, should use ISO8601 YYYY-MM-DD notation");
            if (value.EndsWith("T"))
                throw new FormatException("Partial date may not contain a time part");

            try
            {
                var dummy = PrimitiveTypeConverter.ConvertTo<DateTimeOffset>(value);
            }
            catch
            {
                throw new FormatException("Partial date cannot be parsed, should use ISO8601 YYYY-MM-DD notation");
            }

            return new PartialDate { _value = value };
        }

        public static bool TryParse(string representation, out PartialDate value)
        {
            try
            {
                value = PartialDate.Parse(representation);
                return true;
            }
            catch
            {
                value = default;
                return false;
            }
        }

        public DateTimeOffset ToUniversalTime() => PrimitiveTypeConverter.ConvertTo<DateTimeOffset>(_value).ToUniversalTime();


        // overload operator <
        public static bool operator <(PartialDate a, PartialDate b) => a.ToUniversalTime() < b.ToUniversalTime();

        public static bool operator <=(PartialDate a, PartialDate b) => a.ToUniversalTime() <= b.ToUniversalTime();

        // overload operator >
        public static bool operator >(PartialDate a, PartialDate b) => a.ToUniversalTime() > b.ToUniversalTime();

        public static bool operator >=(PartialDate a, PartialDate b) => a.ToUniversalTime() >= b.ToUniversalTime();

        public static bool operator ==(PartialDate a, PartialDate b) => Equals(a, b);

        public static bool operator !=(PartialDate a, PartialDate b) => !(a == b);

        public bool IsEquivalentTo(PartialDate other)
        {
            if (other == null) return false;

            var len = Math.Min(_value.Length, other._value.Length);
            return String.Compare(_value, 0, other._value, 0, len) == 0;
        }

        public override bool Equals(object obj) => obj is PartialDate other ? _value == other._value : false;

        public override int GetHashCode() => _value.GetHashCode();

        public override string ToString() => _value;

        public static PartialDate Today() => new PartialDate { _value = DateTimeOffset.Now.ToString("yyyy-MM-dd") };

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            if (obj is PartialDate p)
            {
                if (this < p) return -1;
                if (this > p) return 1;
                return 0;
            }
            else
                throw Error.Argument(nameof(obj), "Must be a PartialDateTime");
        }
    }
}
