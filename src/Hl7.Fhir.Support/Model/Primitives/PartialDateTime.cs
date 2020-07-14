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
using static Hl7.Fhir.Support.Utility.Result;

namespace Hl7.Fhir.Model.Primitives
{
    public class PartialDateTime : Any, IComparable, ICqlEquatable, ICqlOrderable
    {
        internal PartialDateTime(string original, DateTimeOffset parsedValue, PartialPrecision precision, bool hasOffset)
        {
            _original = original;
            _parsedValue = parsedValue;
            Precision = precision;
            HasOffset = hasOffset;
        }

        public static PartialDateTime Parse(string representation) =>
            TryParse(representation, out var result) ? result : throw new FormatException($"String '{representation}' was not recognized as a valid partial datetime.");

        public static bool TryParse(string representation, out PartialDateTime value) => tryParse(representation, out value);

        public static PartialDateTime FromDateTimeOffset(DateTimeOffset dto)
        {
            var representation = dto.ToString(FMT_FULL);
            return Parse(representation);
        }

        [Obsolete("FromDateTime() has been renamed to FromDateTimeOffset()")]
        public static PartialDateTime FromDateTime(DateTimeOffset dto) => FromDateTimeOffset(dto);

        public static PartialDateTime Now() => FromDateTimeOffset(DateTimeOffset.Now);

        public static PartialDateTime Today() => PartialDateTime.Parse(DateTimeOffset.Now.ToString("yyyy-MM-ddK"));

        public int? Years => Precision >= PartialPrecision.Year ? _parsedValue.Year : (int?)null;
        public int? Months => Precision >= PartialPrecision.Month ? _parsedValue.Month : (int?)null;
        public int? Days => Precision >= PartialPrecision.Day ? _parsedValue.Day : (int?)null;
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
        /// The precision of the date and time available. 
        /// </summary>
        public PartialPrecision Precision { get; private set; }

        /// <summary>
        /// Whether the time specifies an offset to UTC
        /// </summary>
        public bool HasOffset { get; private set; }

        private static readonly string DATETIMEFORMAT =
            $"(?<year>[0-9]{{4}}) ((?<month>-[0-9][0-9]) ((?<day>-[0-9][0-9]) (T{PartialTime.TIMEFORMAT})?)?)? {PartialTime.OFFSETFORMAT}?";
        private static readonly Regex DATETIMEREGEX =
                new Regex("^" + DATETIMEFORMAT + "$", 
                    RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        /// <summary>
        /// Converts the partial datetime to a full DateTimeOffset instance.
        /// </summary>
        /// <param name="defaultOffset">Offset used when the partial datetime does not specify one.</param>
        /// <returns></returns>
        public DateTimeOffset ToDateTimeOffset(TimeSpan defaultOffset) =>
             new DateTimeOffset(_parsedValue.Year, _parsedValue.Month, _parsedValue.Day,
                 _parsedValue.Hour, _parsedValue.Minute, _parsedValue.Second, _parsedValue.Millisecond,
                    HasOffset ? _parsedValue.Offset : defaultOffset);

        public const string FMT_FULL = "yyyy-MM-dd'T'HH:mm:ss.FFFFFFFK";

        private static bool tryParse(string representation, out PartialDateTime value)
        {
            if (representation is null) throw new ArgumentNullException(nameof(representation));

            var matches = DATETIMEREGEX.Match(representation);
            if (!matches.Success)
            {
                value = new PartialDateTime(representation, default, default, default);
                return false;
            }

            var yrg = matches.Groups["year"];
            var mong = matches.Groups["month"];
            var dayg = matches.Groups["day"];
            var hrg = matches.Groups["hours"];
            var ming = matches.Groups["minutes"];
            var secg = matches.Groups["seconds"];
            var fracg = matches.Groups["frac"];
            var offset = matches.Groups["offset"];

            var prec =
                    fracg.Success ? PartialPrecision.Fraction :
                    secg.Success ? PartialPrecision.Second :
                    ming.Success ? PartialPrecision.Minute :
                    hrg.Success ? PartialPrecision.Hour :
                    dayg.Success ? PartialPrecision.Day :
                    mong.Success ? PartialPrecision.Month :
                    PartialPrecision.Year;

            var parseableDT = yrg.Value +
                  (mong.Success ? mong.Value : "-01") +
                  (dayg.Success ? dayg.Value : "-01") +
                  (hrg.Success ? "T" + hrg.Value : "T00") +
                  (ming.Success ? ming.Value : ":00") +
                  (secg.Success ? secg.Value : ":00") +
                  (fracg.Success ? fracg.Value : "") +
                  (offset.Success ? offset.Value : "Z");

            var success = DateTimeOffset.TryParse(parseableDT, out var parsedValue);
            value = new PartialDateTime(representation, parsedValue, prec, offset.Success);
            return success;
        }

        /// <summary>
        /// Compare two partial datetimess based on CQL equality rules
        /// </summary>
        /// <returns>returns true if the values have the same precision, and each date component is exactly the same. Datetimes with timezones are normalized
        /// to zulu before comparison is done. Throws an <see cref="ArgumentException"/> if the arguments differ in precision.</returns>
        /// <remarks>See <see cref="TryCompareTo(Any)"/> for more details.</remarks>
        public override bool Equals(object obj) => obj is Any other && TryEquals(other).ValueOrDefault(false);

        public Result<bool> TryEquals(Any other) => other is PartialDateTime ? TryCompareTo(other).Select(i => i == 0) : false;

        public static bool operator ==(PartialDateTime a, PartialDateTime b) => Equals(a, b);
        public static bool operator !=(PartialDateTime a, PartialDateTime b) => !Equals(a, b);


        /// <summary>
        /// Compare two partial datetimes based on CQL equality rules
        /// </summary>
        /// <remarks>See <see cref="TryCompareTo(Any)"/> for more details.</remarks>
        public int CompareTo(object obj) => obj is PartialDateTime p ?
            TryCompareTo(p).ValueOrThrow() : throw NotSameTypeComparison(this, obj);

        /// <summary>
        /// Compares two (partial)datetimes according to CQL ordering rules.
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
        public Result<int> TryCompareTo(Any other)
        {
            return other switch
            {
                null => 1,
                PartialDateTime p => PartialDateTime.CompareDateTimeParts(_parsedValue, Precision, HasOffset, p._parsedValue, p.Precision, p.HasOffset),
                _ => throw NotSameTypeComparison(this, other)
            };
        }

        internal static Result<int> CompareDateTimeParts(DateTimeOffset l, PartialPrecision lPrec, bool lHasOffset, DateTimeOffset r, PartialPrecision rPrec, bool rHasOffset)
        {
            l = l.ToUniversalTime();
            r = r.ToUniversalTime();
            var error = new Fail<int>(new InvalidOperationException($"The operands {l} and {r} do not have the same precision and therefore cannot be compared."));

            if (l.Year != r.Year) return Ok(l.Year.CompareTo(r.Year));

            if (lPrec < PartialPrecision.Month ^ rPrec < PartialPrecision.Month) return error;
            if (l.Month != r.Month) return Ok(l.Month.CompareTo(r.Month));

            if (lPrec < PartialPrecision.Day ^ rPrec < PartialPrecision.Day) return error;
            if (l.Day != r.Day) return Ok(l.Day.CompareTo(r.Day));

            if (lPrec < PartialPrecision.Hour ^ rPrec < PartialPrecision.Hour) return error;

            // Before we compare the times, let's first check whether this is possible at all.
            // Actually, this could still influence the dates too, but I don't think people would expect that to
            // be significant.  You'd like now() > Patient.birthday to work, even if one has a timezone,
            // and the other is just a date in the past.
            if ((lHasOffset && !rHasOffset) || (!lHasOffset && rHasOffset))
                return new Fail<int>(new InvalidOperationException($"One of the operands {l} and {r} has a timezone, but not the other."));

            if (l.Hour != r.Hour) return Ok(l.Hour.CompareTo(r.Hour));

            if (lPrec < PartialPrecision.Minute ^ rPrec < PartialPrecision.Minute) return error;
            if (l.Minute != r.Minute) return Ok(l.Minute.CompareTo(r.Minute));

            if (lPrec < PartialPrecision.Second ^ rPrec < PartialPrecision.Second) return error;

            // Note that DateTimeOffset rounds fractional
            // parts to millis (i.e. 12:00:00.12345 would be rounded to 12:00:00.123),
            // so I am not going to bother with the subtle decimal comparison semantics in ordering 
            // as described by the spec ("Note that for the purposes of comparison, seconds and milliseconds
            // are combined as a single precision using a decimal, with *decimal comparison semantics*.")
            // as "decimal comparison semantics" aren't specified anyway. The spec describes
            // equals/equivalence for decimals, but not ordering as far as I can see. I will
            // consider second/millisecond precision to be a single precision, i.e.  12:00:01 == 12:00:01.1
            // is false, rather than null.
            //
            // These simplifications makes my life easier here, otherwise I'd have to create ordering
            // and equivalence as separate functions.
            if (l.Second != r.Second) return Ok(l.Second.CompareTo(r.Second));
            if (l.Millisecond != r.Millisecond) return Ok(l.Millisecond.CompareTo(r.Millisecond));

            return Ok(0);
        }

        public static bool operator <(PartialDateTime a, PartialDateTime b) => a.CompareTo(b) < 0;
        public static bool operator <=(PartialDateTime a, PartialDateTime b) => a.CompareTo(b) <= 0;
        public static bool operator >(PartialDateTime a, PartialDateTime b) => a.CompareTo(b) > 0;
        public static bool operator >=(PartialDateTime a, PartialDateTime b) => a.CompareTo(b) >= 0;


        public override int GetHashCode() => _original.GetHashCode();
        public override string ToString() => _original;

        public static explicit operator PartialDateTime(DateTimeOffset dto) => FromDateTimeOffset(dto);

        bool? ICqlEquatable.IsEqualTo(Any other) => other is { } && TryEquals(other) is Ok<bool> ok ? ok.Value : (bool?)null;

        // Note that, in contrast to equals, this will return false if operators cannot be compared (as described by the spec)
        bool ICqlEquatable.IsEquivalentTo(Any other) => other is { } pd && TryEquals(pd).ValueOrDefault(false);

        int? ICqlOrderable.CompareTo(Any other) => other is { } && TryCompareTo(other) is Ok<int> ok ? ok.Value : (int?)null;

    }
}
