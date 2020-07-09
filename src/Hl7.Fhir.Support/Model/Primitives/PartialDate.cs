/* 
 * Copyright (c) 2020, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

#nullable enable

using Hl7.Fhir.Support.Utility;
using System;
using System.Text.RegularExpressions;

namespace Hl7.Fhir.Model.Primitives
{
    public class PartialDate : Any, IComparable, ICqlEquatable, ICqlOrderable
    {
        private PartialDate(string original, DateTimeOffset parsedValue, PartialPrecision precision, bool hasOffset)
        {
            _original = original;
            _parsedValue = parsedValue;
            Precision = precision;
            HasOffset = hasOffset;
        }

        public static PartialDate Parse(string representation) =>
            TryParse(representation, out var result) ? result : throw new FormatException($"String '{representation}' was not recognized as a valid partial date.");

        public static bool TryParse(string representation, out PartialDate value) => tryParse(representation, out value);

        public static PartialDate FromDateTimeOffset(DateTimeOffset dto, PartialPrecision prec = PartialPrecision.Day,
        bool includeOffset = false)
        {
            string formatString = prec switch
            {
                PartialPrecision.Year => "yyyy",
                PartialPrecision.Month => "yyyy-MM",
                _ => "yyyy-MM-dd",
            };
            if (includeOffset) formatString += "K";

            var representation = dto.ToString(formatString);
            return Parse(representation);
        }

        public static PartialDate Today(bool includeOffset = false) => FromDateTimeOffset(DateTimeOffset.Now, includeOffset: includeOffset);

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

        private readonly string _original;
        private readonly DateTimeOffset _parsedValue;

        /// <summary>
        /// Whether the time specifies an offset to UTC
        /// </summary>
        public bool HasOffset { get; private set; }

        private static readonly string DATEFORMAT =
            $"(?<year>[0-9]{{4}}) ((?<month>-[0-9][0-9]) ((?<day>-[0-9][0-9]) )?)? {PartialTime.OFFSETFORMAT}?";
        public static readonly Regex PARTIALDATEREGEX = new Regex("^" + DATEFORMAT + "$", RegexOptions.IgnorePatternWhitespace);

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

        /// <summary>
        /// Converts the partial date to a full DateTimeOffset instance.
        /// </summary>
        /// <returns></returns>
        private static bool tryParse(string representation, out PartialDate value)
        {
            if (representation is null) throw new ArgumentNullException(nameof(representation));

            var matches = PARTIALDATEREGEX.Match(representation);
            if (!matches.Success)
            {
                value = new PartialDate(representation, default, default, default);
                return false;
            }

            var y = matches.Groups["year"];
            var m = matches.Groups["month"];
            var d = matches.Groups["day"];
            var offset = matches.Groups["offset"];

            var prec =
                d.Success ? PartialPrecision.Day :
                m.Success ? PartialPrecision.Month :
                PartialPrecision.Year;

            var parseableDT = y.Value +
                (m.Success ? m.Value : "-01") +
                (d.Success ? d.Value : "-01") +
                "T" + "00:00:00" +
                (offset.Success ? offset.Value : "Z");

            var success = DateTimeOffset.TryParse(parseableDT, out var parsedValue);
            value = new PartialDate(representation, parsedValue, prec, offset.Success);
            return success;
        }

        /// <summary>
        /// Determines if two partial dates are equal according to CQL equality rules.
        /// </summary>
        /// <returns>returns true if the values are both PartialDates, have the same precision and each date component is exactly the same. 
        /// Dates with timezones are normalized to zulu before comparison is done.</returns>
        /// <remarks>See <see cref="TryCompareTo(PartialDate)"/> for more details.</remarks>
        public override bool Equals(object obj) => obj is PartialDate pd && TryEquals(pd).Success;

        public Result<bool> TryEquals(PartialDate other) => TryCompareTo(other).Select(i => i == 0);

        public static bool operator ==(PartialDate a, PartialDate b) => Equals(a, b);
        public static bool operator !=(PartialDate a, PartialDate b) => !Equals(a, b);

        /// <summary>
        /// Compare two partial dates according to CQL equality rules
        /// </summary>
        /// <remarks>See <see cref="TryCompareTo(PartialDate)"/> for more details.</remarks>
        public int CompareTo(object obj)
        {
            return obj switch
            {
                null => 1,
                PartialDate p => TryCompareTo(p).ValueOrElse(e => throw e),
                _ => throw NotSameTypeComparison(this, obj)
            };
        }

        /// <summary>
        /// Compares two (partial)dates according to CQL ordering rules.
        /// </summary> 
        /// <param name="other"></param>
        /// <returns>An <see cref="Support.Utility.Ok{T}"/> with an integer value representing the reseult of the comparison: 0 if this and other are equal, 
        /// -1 if this is smaller than other and +1 if this is bigger than other, or the other is null. If the values are incomparable
        /// this function returns a <see cref="Support.Utility.Fail{T}"/> with the reason why the comparison between the two values was impossible.
        /// </returns>
        /// <remarks>The comparison is performed by considering each precision in order, beginning with years. 
        /// If the values are the same, comparison proceeds to the next precision; 
        /// if the values are different, the comparison stops and the result is false. If one input has a value 
        /// for the precision and the other does not, the comparison stops and the values cannot be compared; if neither
        /// input has a value for the precision, or the last precision has been reached, the comparison stops
        /// and the result is true.</remarks>
        public Result<int> TryCompareTo(PartialDate other)
        {
            return other is null ? 1 : PartialDateTime.CompareDateTimeParts(_parsedValue, Precision, other._parsedValue, other.Precision);
        }

        public static bool operator <(PartialDate a, PartialDate b) => a.CompareTo(b) == -1;
        public static bool operator <=(PartialDate a, PartialDate b) => a.CompareTo(b) != 1;
        public static bool operator >(PartialDate a, PartialDate b) => a.CompareTo(b) == 1;
        public static bool operator >=(PartialDate a, PartialDate b) => a.CompareTo(b) != -1;

        public override int GetHashCode() => _original.GetHashCode();
        public override string ToString() => _original;

        public static implicit operator PartialDateTime(PartialDate pd) => throw new NotImplementedException();
        public static explicit operator PartialDate(DateTimeOffset dto) => FromDateTimeOffset(dto);

        bool? ICqlEquatable.IsEqualTo(Any other) => other is PartialDate pd &&
            TryEquals(pd) is Ok<bool> ok ? ok.Value : (bool?)null;

        // Note that, in contrast to equals, this will return false if operators cannot be compared (as described by the spec)
        bool ICqlEquatable.IsEquivalentTo(Any other) => other is PartialDate pd && TryEquals(pd).Success;

        int? ICqlOrderable.CompareTo(Any other)
        {
            if (other is null) return null;
            if (!(other is PartialDate pd)) throw NotSameTypeComparison(this, other);
            
            return TryCompareTo(pd) is Ok<int> ok ? ok.Value : (int?)null;
        }
    }
}
