/* 
 * Copyright (c) 2015, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

using System;
using System.Text.RegularExpressions;

namespace Hl7.Fhir.Model.Primitives
{
    public struct PartialDate : IComparable, IComparable<PartialDate>, IEquatable<PartialDate>
    {
        public static PartialDate Parse(string value) =>
            TryParse(value, out PartialDate result) ? result : throw new FormatException("Date value is in an invalid format.");

        public static bool TryParse(string representation, out PartialDate value) =>
            tryParse(representation, out value);

        /// <summary>
        /// The precision of the date available. 
        /// </summary>
        public PartialPrecision Precision { get; private set; }

        public int? Years => Precision >= PartialPrecision.Year ? _parsedValue.Year : (int?)null;
        public int? Months => Precision >= PartialPrecision.Month ? _parsedValue.Month : (int?)null;
        public int? Days => Precision >= PartialPrecision.Day ? _parsedValue.Day : (int?)null;

        /// <summary>
        /// The span of time ahead/behind UTC
        /// </summary>
        public TimeSpan? Offset => HasOffset ? _parsedValue.Offset : (TimeSpan?)null;

        private string _original;
        private DateTimeOffset _parsedValue;

        /// <summary>
        /// Whether the time specifies an offset to UTC
        /// </summary>
        public bool HasOffset { get; private set; }

        private static readonly string DATEFORMAT =
            $"(?<year>[0-9]{{4}}) ((?<month>-[0-9][0-9]) ((?<day>-[0-9][0-9]) )?)? {PartialTime.OFFSETFORMAT}?";
        public static readonly Regex PARTIALDATEREGEX = new Regex("^" + DATEFORMAT +  "$", RegexOptions.IgnorePatternWhitespace);

        /// <summary>
        /// Converts the partial date to a full DateTimeOffset instance.
        /// </summary>
        /// <param name="hours"></param>
        /// <param name="minutes"></param>
        /// <param name="seconds"></param>
        /// <param name="defaultOffset">Offset used when the partial datetime does not specify one.</param>
        /// <returns></returns>
        public DateTimeOffset ToDateTimeOffset(int hours, int minutes, int seconds, TimeSpan defaultOffset) =>
            ToDateTimeOffset(hours, minutes, seconds, 0, defaultOffset);

        /// <summary>
        /// Converts the partial date to a full DateTimeOffset instance.
        /// </summary>
        /// <param name="hours"></param>
        /// <param name="minutes"></param>
        /// <param name="seconds"></param>
        /// <param name="milliseconds"></param>
        /// <param name="defaultOffset">Offset used when the partial datetime does not specify one.</param>
        /// <returns></returns>
        public DateTimeOffset ToDateTimeOffset(int hours, int minutes, int seconds, int milliseconds, TimeSpan defaultOffset) =>
                new DateTimeOffset(_parsedValue.Year, _parsedValue.Month, _parsedValue.Day, hours, minutes, seconds, milliseconds,
                        HasOffset ? _parsedValue.Offset : defaultOffset);

        public static PartialDate FromDateTimeOffset(DateTimeOffset dto, PartialPrecision prec = PartialPrecision.Day,
                bool includeOffset = false)
        {
            string formatString;

            switch(prec)
            {
                case PartialPrecision.Year:
                    formatString = "yyyy";
                    break;
                case PartialPrecision.Month:
                    formatString = "yyyy-MM";
                    break;
                case PartialPrecision.Day:
                default:
                    formatString = "yyyy-MM-dd";
                    break;
            }

            if (includeOffset) formatString += "K";

            var representation = dto.ToString(formatString);
            return Parse(representation);
        }

        public static PartialDate Today(bool includeOffset = false) => FromDateTimeOffset(DateTimeOffset.Now, includeOffset: includeOffset);

        /// <summary>
        /// Converts the partial date to a full DateTimeOffset instance.
        /// </summary>
        /// <returns></returns>
        private static bool tryParse(string representation, out PartialDate value)
        {
            value = new PartialDate();

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

            var parseableDT = y.Value +
                (m.Success ? m.Value : "-01") +
                (d.Success ? d.Value : "-01") +
                "T" + "00:00:00" +
                (offset.Success ? offset.Value : "Z");

            value._original = representation;
            return DateTimeOffset.TryParse(parseableDT, out value._parsedValue);
        }

        // TODO: Note, this enables comparisons between values that did or did not have timezones, need to fix.
        private DateTimeOffset toComparable() => _parsedValue.ToUniversalTime();

        public static bool operator <(PartialDate a, PartialDate b) => a.toComparable() < b.toComparable();
        public static bool operator <=(PartialDate a, PartialDate b) => a.toComparable() <= b.toComparable();
        public static bool operator >(PartialDate a, PartialDate b) => a.toComparable() > b.toComparable();
        public static bool operator >=(PartialDate a, PartialDate b) => a.toComparable() >= b.toComparable();
        public static bool operator ==(PartialDate a, PartialDate b) => a.Equals(b);
        public static bool operator !=(PartialDate a, PartialDate b) => !(a == b);

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
                throw new ArgumentException($"Object is not a {nameof(PartialDate)}");
        }

        public bool Equals(PartialDate other) => this.Precision == other.Precision && other.toComparable() == toComparable();
        public override int GetHashCode() => (Precision, toComparable()).GetHashCode();
        public override string ToString() => _original;

        public int CompareTo(PartialDate obj) => CompareTo((object)obj);
        public override bool Equals(object obj) => obj is PartialDate date && Equals(date);

        // Comparison functions work according to the rules described for CQL, 
        // see https://cql.hl7.org/09-b-cqlreference.html#comparison-operators-4
        // for more details.

        /// <summary>
        /// Compares two (partial)date/times according to CQL equality rules.
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns>true is the precision of the date/times is the same and the individual components for
        /// each precision are the same.</returns>
        /// <remarks>
        /// The comparison is performed by considering each precision in order, beginning with years 
        /// (or hours for time values), and respecting timezone offsets. If the values are the same, comparison
        /// proceeds to the next precision; if the values are different, the comparison stops and the result is false. 
        /// If one input has a value for the precision and the other does not, the comparison stops and the result is null; 
        /// if neither input has a value for the precision, or the last precision has been reached, the comparison stops
        /// and the result is true. For the purposes of comparison, seconds and milliseconds are combined as a 
        /// single precision using a decimal, with decimal equality semantics.
        /// </remarks>
        public static bool? IsEqualTo(PartialDate l, PartialDate r)
        {
            // My interpretation is that if one value has a timezone, and the other does not, 
            // we cannot compare the two.
            if (l.HasOffset ^ r.HasOffset) return null;

            if ( l.Years != r.Years) return false;

            
            return l.toComparable() == r.toComparable();
        }
  
        /// <summary>
        /// Compares two (partial) dates according to CQL equivalence rules.
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns>true is the precision of the dates is the same and the individual components for
        /// each precision are the same.</returns>
        /// <remarks>For Date, DateTime, and Time values, the comparison is performed in the same way as 
        /// it is for equality, except that if one input has a value for a given precision and the other 
        /// does not, the comparison stops and the result is false, rather than null. As with equality, 
        /// the second and millisecond precisions are combined as a single precision using a decimal, 
        /// with decimal equivalence semantics.</remarks>
        public static bool IsEquivalentTo(PartialDate l, PartialDate r)
        {
            if (l.Precision != r.Precision) return false;
            return l.toComparable() == r.toComparable();
        }
    }
}
