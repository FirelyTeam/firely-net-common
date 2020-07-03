/* 
 * Copyright (c) 2020, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

#nullable enable

using System;
using System.Text.RegularExpressions;

namespace Hl7.Fhir.Model.Primitives
{
    public class PartialTime : Any, IComparable
    {
        private PartialTime(string original, DateTimeOffset parsedValue, PartialPrecision precision, bool hasOffset)
        {
            _original = original;
            _parsedValue = parsedValue;
            Precision = precision;
            HasOffset = hasOffset;
        }

        public static PartialTime Parse(string representation) =>
            TryParse(representation, out var result) ? result : throw new FormatException($"String '{representation}' was not recognized as a valid partial time.");

        public static bool TryParse(string representation, out PartialTime value) => tryParse(representation, out value);

        public static PartialTime FromDateTimeOffset(DateTimeOffset dto, PartialPrecision prec = PartialPrecision.Fraction,
        bool includeOffset = false)
        {
            string formatString = prec switch
            {
                PartialPrecision.Hour => "HH",
                PartialPrecision.Minute => "HH:mm",
                PartialPrecision.Second => "HH:mm:ss",
                _ => "HH:mm:ss.FFFFFFF",
            };

            if (includeOffset) formatString += "K";

            var representation = dto.ToString(formatString);
            return Parse(representation);
        }

        public static PartialTime Now(bool includeOffset = false) => FromDateTimeOffset(DateTimeOffset.Now, includeOffset: includeOffset);

        public int? Hours => Precision >= PartialPrecision.Hour ? _parsedValue.Hour : (int?)null;
        public int? Minutes => Precision >= PartialPrecision.Minute ? _parsedValue.Minute : (int?)null;
        public int? Seconds => Precision >= PartialPrecision.Second ? _parsedValue.Second : (int?)null;
        public int? Millis => Precision >= PartialPrecision.Fraction ? _parsedValue.Millisecond : (int?)null;

        /// <summary>
        /// The span of time ahead/behind UTC
        /// </summary>
        public TimeSpan? Offset => HasOffset ? _parsedValue.Offset : (TimeSpan?)null;

        private readonly string _original;
        private readonly DateTimeOffset _parsedValue;

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


        private static bool tryParse(string representation, out PartialTime value)
        {
            if (representation is null) throw new ArgumentNullException(nameof(representation));

            var matches = PARTIALTIMEREGEX.Match(representation);
            if (!matches.Success)
            {
                value = new PartialTime(representation, default, default, default);
                return false;
            }

            var hrg = matches.Groups["hours"];
            var ming = matches.Groups["minutes"];
            var secg = matches.Groups["seconds"];
            var fracg = matches.Groups["frac"];
            var offset = matches.Groups["offset"];

            var prec =
                        fracg.Success ? PartialPrecision.Fraction :
                        secg.Success ? PartialPrecision.Second :
                        ming.Success ? PartialPrecision.Minute :
                        PartialPrecision.Hour;

            var parseableDT = $"2016-01-01T" +
                    (hrg.Success ? hrg.Value : "00") +
                    (ming.Success ? ming.Value : ":00") +
                    (secg.Success ? secg.Value : ":00") +
                    (fracg.Success ? fracg.Value : "") +
                    (offset.Success ? offset.Value : "Z");
            
            var success = DateTimeOffset.TryParse(parseableDT, out var parsedValue);
            value = new PartialTime(representation, parsedValue, prec, offset.Success);
            return success;
        }

        /// <summary>
        /// Compare two partial times based on CQL equality rules
        /// </summary>
        /// <param name="other"></param>
        /// <returns>returns true if the values have the same precision, and each time component is exactly the same. Times with timezones are normalized
        /// to zulu before comparison is done. Throws an <see cref="ArgumentException"/> if the arguments differ in precision.</returns>
        /// <remarks>See <see cref="TryCompareTo(PartialTime, out int)"/> for more details.</remarks>
        public override bool Equals(object other) => other is PartialTime pt && TryCompareTo(pt, out var result) && result == 0;

        public bool TryEquals(PartialTime other, out bool result)
        {
            var success = TryCompareTo(other, out var comparison);
            result = comparison == 0;
            return success;
        }

        public static bool operator ==(PartialTime a, PartialTime b) => Equals(a, b);
        public static bool operator !=(PartialTime a, PartialTime b) => !Equals(a, b);

        /// <summary>
        /// Compare two partial times based on CQL equality rules
        /// </summary>
        /// <remarks>See <see cref="TryCompareTo(PartialTime, out int)"/> for more details.</remarks>
        public int CompareTo(object obj)
        {
            if (obj is null) return 1;      // as defined by the .NET framework guidelines

            if (obj is PartialTime p)
            {
                return TryCompareTo(p, out var comparison) ?
                    comparison : throw new ArgumentException($"Value {this} and {p} cannot be compared, since the precision is different.");
            }
            else
                throw new ArgumentException($"Object is not a {nameof(PartialTime)}");
        }

        /// <summary>
        /// Compares two (partial)times according to CQL ordering rules.
        /// </summary> 
        /// <param name="other"></param>
        /// <param name="comparison">the result of the comparison: 0 if this and other are equal, 
        /// -1 if this is smaller than other and +1 if this is bigger than other.</param>
        /// <returns>true is the values can be compared (have the same precision) or false otherwise.</returns>
        /// <remarks>The comparison is performed by considering each precision in order, beginning with hours.
        /// If the values are the same, comparison proceeds to the next precision; 
        /// if the values are different, the comparison stops and the result is false. If one input has a value 
        /// for the precision and the other does not, the comparison stops and the values cannot be compared; if neither
        /// input has a value for the precision, or the last precision has been reached, the comparison stops
        /// and the result is true. For the purposes of comparison, seconds and milliseconds are combined as a 
        /// single precision using a decimal, with decimal equality semantics.</remarks>
        public bool TryCompareTo(PartialTime other, out int comparison)
        {
            if (other is null)
            {
                comparison = 1; // as defined by the .NET framework guidelines
                return true;
            }
            else
                return PartialDateTime.CompareDateTimeParts(_parsedValue, Precision, other._parsedValue, other.Precision, out comparison);
        }

        public static bool operator <(PartialTime a, PartialTime b) => a.CompareTo(b) == -1;
        public static bool operator <=(PartialTime a, PartialTime b) => a.CompareTo(b) != 1;
        public static bool operator >(PartialTime a, PartialTime b) => a.CompareTo(b) == 1;
        public static bool operator >=(PartialTime a, PartialTime b) => a.CompareTo(b) != -1;

        public override int GetHashCode() => _original.GetHashCode();
        public override string ToString() => _original;

        public static explicit operator PartialTime(DateTimeOffset dto) => FromDateTimeOffset(dto);
    }
}



