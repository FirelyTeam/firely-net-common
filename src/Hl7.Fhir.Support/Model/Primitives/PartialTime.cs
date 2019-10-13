/* 
 * Copyright (c) 2019, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

using System;
using System.Text.RegularExpressions;

namespace Hl7.Fhir.Model.Primitives
{
    public struct PartialTime : IComparable, IComparable<PartialTime>, IEquatable<PartialTime>
    {
        public static PartialTime Parse(string representation) =>
            TryParse(representation, out var result) ? result : throw new FormatException("Time value is in an invalid format.");

        public static bool TryParse(string representation, out PartialTime value) =>
            tryParse(representation, out value);

        public int? Hours => Precision >= PartialPrecision.Hour ? _parsedValue.Hour : (int?)null;
        public int? Minutes => Precision >= PartialPrecision.Minute ? _parsedValue.Minute : (int?)null;
        public int? Seconds => Precision >= PartialPrecision.Second ? _parsedValue.Second : (int?)null;
        public int? Millis => Precision >= PartialPrecision.Fraction ? _parsedValue.Millisecond : (int?)null;

        /// <summary>
        /// The span of time ahead/behind UTC
        /// </summary>
        public TimeSpan? Offset => HasOffset ? _parsedValue.Offset : (TimeSpan?)null;

        private string _original;
        private DateTimeOffset _parsedValue;

        /// <summary>
        /// The precision of the time available. 
        /// </summary>
        public PartialPrecision Precision { get; private set; }

        /// <summary>
        /// Whether the time specifies an offset to UTC
        /// </summary>
        public bool HasOffset { get; private set; }

        // Our regex is pretty flexible, it does not bother to capture rules about semantics (12:64 would be legal here).
        // Additional semantic checks will be verified using the built-in DateTimeOffset .NET parser.
        // Also, it accept the superset of formats specified by FHIR, CQL, FhirPath and the mapping language. Each of these
        // specific implementations may add additional constraints (e.g. about minimum precision or presence of timezones).

        internal static readonly string PARTIALTIMEFORMAT = $"{TIMEFORMAT}{OFFSETFORMAT}?";
        internal const string TIMEFORMAT =
            "(?<time>(?<hours>[0-9][0-9]) ((?<minutes>:[0-9][0-9]) ((?<seconds>:[0-9][0-9]) ((?<frac>.[0-9]+))?)?)?)";
        internal const string OFFSETFORMAT = "(?<offset>Z | (\\+|-) [0-9][0-9]:[0-9][0-9])";

        private static readonly Regex PARTIALTIMEREGEX =
            new Regex("^" + PARTIALTIMEFORMAT + "$", RegexOptions.IgnorePatternWhitespace);

        /// <summary>
        /// Converts the partial time to a full DateTimeOffset instance.
        /// </summary>
        /// <param name="year">Year used to turn a time into a date</param>
        /// <param name="month">Month used to turn a time into a date</param>
        /// <param name="day">Day used to turn a time into a date</param>
        /// <param name="defaultOffset">Offset used when the partial time does not specify one.</param>
        /// <returns></returns>
        public DateTimeOffset ToDateTimeOffset(int year, int month, int day, TimeSpan defaultOffset) =>
            new DateTimeOffset(year, month, day, _parsedValue.Hour,
                    _parsedValue.Minute, _parsedValue.Second, _parsedValue.Millisecond,
                    HasOffset ? _parsedValue.Offset : defaultOffset);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="prec"></param>
        /// <param name="includeOffset">Whether to include the timezone offset.</param>
        /// <returns></returns>
        public static PartialTime FromDateTimeOffset(DateTimeOffset dto, PartialPrecision prec = PartialPrecision.Fraction,
                bool includeOffset = false)
        {
            string formatString;

            switch (prec)
            {
                case PartialPrecision.Hour:
                    formatString = "HH";
                    break;
                case PartialPrecision.Minute:
                    formatString = "HH:mm";
                    break;
                case PartialPrecision.Second:
                    formatString = "HH:mm:ss";
                    break;
                case PartialPrecision.Fraction:                    
                default:
                    formatString = "HH:mm:ss.FFFFFFF";
                    break;
            }

            if (includeOffset) formatString += "K";

            var representation = dto.ToString(formatString);
            return Parse(representation);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="includeOffset">Whether to include the timezone offset.</param>
        /// <returns></returns>
        public static PartialTime Now(bool includeOffset = false) => FromDateTimeOffset(DateTimeOffset.Now, includeOffset: includeOffset);

        private static bool tryParse(string representation, out PartialTime value)
        {
            value = new PartialTime();

            var matches = PARTIALTIMEREGEX.Match(representation);
            if (!matches.Success) return false;

            var hrg = matches.Groups["hours"];
            var ming = matches.Groups["minutes"];
            var secg = matches.Groups["seconds"];
            var fracg = matches.Groups["frac"];
            var offset = matches.Groups["offset"];

            value.Precision =
                        fracg.Success ? PartialPrecision.Fraction :
                        secg.Success ? PartialPrecision.Second :
                        ming.Success ? PartialPrecision.Minute :
                        PartialPrecision.Hour;

            value.HasOffset = offset.Success;

            var parseableDT = $"2016-01-01T" +
                    (hrg.Success ? hrg.Value : "00") +
                    (ming.Success ? ming.Value : ":00") +
                    (secg.Success ? secg.Value : ":00") +
                    (fracg.Success ? fracg.Value : "") +
                    (offset.Success ? offset.Value : "Z");

            value._original = representation;
            return DateTimeOffset.TryParse(parseableDT, out value._parsedValue);
        }

        public bool IsEqualTo(PartialTime other) => this == other;
        public bool IsEquivalentTo(PartialTime other) => this == other;

        // TODO: Note, this enables comparisons between values that did or did not have timezones, need to fix.
        private DateTimeOffset toComparable() => _parsedValue.ToUniversalTime();

        public static bool operator <(PartialTime a, PartialTime b) => a.toComparable() < b.toComparable();
        public static bool operator <=(PartialTime a, PartialTime b) => a.toComparable() <= b.toComparable();
        public static bool operator >(PartialTime a, PartialTime b) => a.toComparable() > b.toComparable();
        public static bool operator >=(PartialTime a, PartialTime b) => a.toComparable() >= b.toComparable();
        public static bool operator ==(PartialTime a, PartialTime b) => Equals(a, b);
        public static bool operator !=(PartialTime a, PartialTime b) => !(a == b);

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            if (obj is PartialTime p)
            {
                return (this < p) ? -1 :
                     (this > p) ? 1 : 0;
            }
            else
                throw new ArgumentException($"Object is not a {nameof(PartialTime)}");
        }

        public bool Equals(PartialTime other) => this.Precision == other.Precision && other.toComparable() == toComparable();
        public override int GetHashCode() => toComparable().GetHashCode();
        public override string ToString() => _original;

        public int CompareTo(PartialTime obj) => CompareTo((object)obj);
        public override bool Equals(object obj) => obj is PartialTime time && Equals(time);

    }
}



