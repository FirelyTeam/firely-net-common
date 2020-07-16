/* 
 * Copyright (c) 2020, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

#nullable enable

using Hl7.Fhir.Support.Utility;
using Hl7.Fhir.Utility;
using System;
using System.Text.RegularExpressions;

namespace Hl7.Fhir.ElementModel.Types
{
    public class PartialDate : Any, IComparable, ICqlEquatable, ICqlOrderable
    {
        private PartialDate(string original, DateTimeOffset parsedValue, DateTimePrecision precision, bool hasOffset)
        {
            _original = original;
            _parsedValue = parsedValue;
            Precision = precision;
            HasOffset = hasOffset;
        }

        public static PartialDate Parse(string representation) =>
            TryParse(representation, out var result) ? result : throw new FormatException($"String '{representation}' was not recognized as a valid partial date.");

        public static bool TryParse(string representation, out PartialDate value) => tryParse(representation, out value);

        public static PartialDate FromDateTimeOffset(DateTimeOffset dto, DateTimePrecision prec = DateTimePrecision.Day,
        bool includeOffset = false)
        {
            string formatString = prec switch
            {
                DateTimePrecision.Year => "yyyy",
                DateTimePrecision.Month => "yyyy-MM",
                _ => "yyyy-MM-dd",
            };
            if (includeOffset) formatString += "K";

            var representation = dto.ToString(formatString);
            return Parse(representation);
        }

        public PartialDateTime ToPartialDateTime() => new PartialDateTime(_original, _parsedValue, Precision, HasOffset);


        public static PartialDate Today(bool includeOffset = false) => FromDateTimeOffset(DateTimeOffset.Now, includeOffset: includeOffset);

        /// <summary>
        /// The precision of the date available. 
        /// </summary>
        public DateTimePrecision Precision { get; private set; }

        public int? Years => Precision >= DateTimePrecision.Year ? _parsedValue.Year : (int?)null;
        public int? Months => Precision >= DateTimePrecision.Month ? _parsedValue.Month : (int?)null;
        public int? Days => Precision >= DateTimePrecision.Day ? _parsedValue.Day : (int?)null;

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
        public static readonly Regex PARTIALDATEREGEX = new Regex("^" + DATEFORMAT + "$",
#if NETSTANDARD1_1
                RegexOptions.IgnorePatternWhitespace | RegexOptions.ExplicitCapture);
#else
                RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled | RegexOptions.ExplicitCapture);
#endif
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
                d.Success ? DateTimePrecision.Day :
                m.Success ? DateTimePrecision.Month :
                DateTimePrecision.Year;

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
        /// <remarks>See <see cref="TryCompareTo(Any)"/> for more details.</remarks>
        public override bool Equals(object obj) => obj is Any other && TryEquals(other).ValueOrDefault(false);

        public Result<bool> TryEquals(Any other) => other is PartialDate ? TryCompareTo(other).Select(i => i == 0) : false;

        public static bool operator ==(PartialDate a, PartialDate b) => Equals(a, b);
        public static bool operator !=(PartialDate a, PartialDate b) => !Equals(a, b);

        /// <summary>
        /// Compare two partial dates according to CQL equality rules
        /// </summary>
        /// <remarks>See <see cref="TryCompareTo(Any)"/> for more details.</remarks>
        public int CompareTo(object obj) => obj is PartialDate p ?
            TryCompareTo(p).ValueOrThrow() : throw NotSameTypeComparison(this, obj);

        /// <summary>
        /// Compares two (partial)dates according to CQL ordering rules.
        /// </summary> 
        /// <param name="other"></param>
        /// <returns>An <see cref="Ok{T}"/> with an integer value representing the reseult of the comparison: 0 if this and other are equal, 
        /// -1 if this is smaller than other and +1 if this is bigger than other, or the other is null. If the values are incomparable
        /// this function returns a <see cref="Fail{T}"/> with the reason why the comparison between the two values was impossible.
        /// </returns>
        /// <remarks>The comparison is performed by considering each precision in order, beginning with years. 
        /// If the values are the same, comparison proceeds to the next precision; 
        /// if the values are different, the comparison stops and the result is false. If one input has a value 
        /// for the precision and the other does not, the comparison stops and the values cannot be compared; if neither
        /// input has a value for the precision, or the last precision has been reached, the comparison stops
        /// and the result is true.</remarks>
        public Result<int> TryCompareTo(Any other)
        {
            return other switch
            {
                null => 1,
                PartialDate p => PartialDateTime.CompareDateTimeParts(_parsedValue, Precision, HasOffset, p._parsedValue, p.Precision, p.HasOffset),
                _ => throw NotSameTypeComparison(this, other)
            };
        }

        public static bool operator <(PartialDate a, PartialDate b) => a.CompareTo(b) < 0;
        public static bool operator <=(PartialDate a, PartialDate b) => a.CompareTo(b) <= 0;
        public static bool operator >(PartialDate a, PartialDate b) => a.CompareTo(b) > 0;
        public static bool operator >=(PartialDate a, PartialDate b) => a.CompareTo(b) >= 0;


        public override int GetHashCode() => _original.GetHashCode();
        public override string ToString() => _original;

        public static implicit operator PartialDateTime(PartialDate pd) => pd.ToPartialDateTime();
        public static explicit operator PartialDate(DateTimeOffset dto) => FromDateTimeOffset(dto);

        bool? ICqlEquatable.IsEqualTo(Any other) => other is { } && TryEquals(other) is Ok<bool> ok ? ok.Value : (bool?)null;

        // Note that, in contrast to equals, this will return false if operators cannot be compared (as described by the spec)
        bool ICqlEquatable.IsEquivalentTo(Any other) => other is { } pd && TryEquals(pd).ValueOrDefault(false);

        int? ICqlOrderable.CompareTo(Any other) => other is { } && TryCompareTo(other) is Ok<int> ok ? ok.Value : (int?)null;
    }
}
