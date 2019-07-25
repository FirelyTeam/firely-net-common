/* 
 * Copyright (c) 2015, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

using Hl7.Fhir.Utility;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Hl7.Fhir.Model.Primitives
{
    public struct PartialDate : IComparable, IEquatable<PartialDate>
    {

        public static PartialDate Parse(string value) =>
            TryParse(value, out PartialDate result) ? result : throw new FormatException("Time value is in an invalid format.");

        public static bool TryParse(string representation, out PartialDate value) =>
            tryParse(representation, 12, 0, 0, out value);

        public PartialPrecision Precision { get; private set; }

        public int? Year => Precision >= PartialPrecision.Year ? _parsedValue.Year : (int?)null;
        public int? Month => Precision >= PartialPrecision.Month ? _parsedValue.Month : (int?)null;
        public int? Day => Precision >= PartialPrecision.Day ? _parsedValue.Day : (int?)null;
        public TimeSpan? Offset => HasOffset ? _parsedValue.Offset : (TimeSpan?)null;

        internal const string DATEFORMAT =
            "((?<year>[0-9][0-9][0-9][0-9]) ((?<month>-[0-9][0-9]) ((?<day>-[0-9][0-9]) )?)?)?";
        internal const string OFFSETFORMAT = "(?<offset>Z | (\\+|-) [0-9][0-9]:[0-9][0-9])?";
        internal static readonly string PARTIALDATEFORMAT = $"{DATEFORMAT}{OFFSETFORMAT}";

        public static readonly Regex PARTIALDATEREGEX = new Regex("^" + PARTIALDATEFORMAT + "$", RegexOptions.IgnorePatternWhitespace);

        public bool HasOffset { get; private set; }

        private string _original;
        private DateTimeOffset _parsedValue;

        public const string FMT_FULL = "yyyy-MM-dd.FFFFFFFK";

        public static PartialDate FromDateTimeOffset(DateTimeOffset dto)
        {
            var representation = dto.ToString(FMT_FULL);
            return Parse(representation);
        }

        private static bool tryParse(string representation, int hour, int minutes, int seconds, out PartialDate value)
        {
            value = new PartialDate();

            if (String.IsNullOrEmpty(representation)) return false;

            var matches = PARTIALDATEREGEX.Match(representation);
            if (!matches.Success) return false;

            var y = matches.Groups["year"];
            var m = matches.Groups["month"];
            var d = matches.Groups["day"];
            var offset = matches.Groups["offset"];

            value.Precision =
                d.Success ? PartialPrecision.Day :
                m.Success ? PartialPrecision.Month :
                PartialPrecision.Year;
            value.HasOffset = offset.Success;

            var parseableDT = (y.Success ? y.Value : "0000") +
                (m.Success ? m.Value : "-01") +
                (d.Success ? d.Value : "-01") +
                "T" +
                $"{hour:00}:{minutes:00}:{seconds:00}" +
                (offset.Success ? offset.Value : "");

            value._original = representation;
            return DateTimeOffset.TryParse(parseableDT, out value._parsedValue);
        }

        private DateTimeOffset toComparable() => _parsedValue.ToUniversalTime();

        public static bool operator <(PartialDate a, PartialDate b) => a.toComparable() < b.toComparable();
        public static bool operator <=(PartialDate a, PartialDate b) => a.toComparable() <= b.toComparable();
        public static bool operator >(PartialDate a, PartialDate b) => a.toComparable() > b.toComparable();
        public static bool operator >=(PartialDate a, PartialDate b) => a.toComparable() >= b.toComparable();
        public static bool operator ==(PartialDate a, PartialDate b) => Equals(a, b);
        public static bool operator !=(PartialDate a, PartialDate b) => !(a == b);

        public override bool Equals(object obj) => obj is PartialTime date && Equals(date);
        public bool Equals(PartialDate other) => other.toComparable() == toComparable();

        public override int GetHashCode() => -1939223833 + EqualityComparer<DateTimeOffset>.Default.GetHashCode(toComparable());
        public override string ToString() => _original;
        public static PartialDate Today()
        {
           TryParse(DateTimeOffset.Now.ToString("yyyy-MM-dd"), out PartialDate todayValue);
            return todayValue;
        }

        public bool IsEquivalentTo(PartialDate other) => throw new NotImplementedException();

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
                throw Error.Argument(nameof(obj), "Must be a PartialDate");
        }

        
        
    }
}
