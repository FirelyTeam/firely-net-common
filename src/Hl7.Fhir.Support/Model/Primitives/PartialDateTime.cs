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
    public class PartialDateTime : Any, IComparable, IComparable<PartialDateTime>, IEquatable<PartialDateTime>
    {
        private PartialDateTime(string original, DateTimeOffset parsedValue, PartialPrecision precision, bool hasOffset)
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
                new Regex("^" + DATETIMEFORMAT + "$", RegexOptions.IgnorePatternWhitespace);

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
                  (hrg.Success ? "T"+hrg.Value : "T00") +
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
        /// <param name="other"></param>
        /// <returns>returns true if the values have the same precision, and each date component is exactly the same. Datetimes with timezones are normalized
        /// to zulu before comparison is done. Throws an <see cref="ArgumentException"/> if the arguments differ in precision.</returns>
        /// <remarks>See <see cref="TryCompare(PartialDateTime, out int)"/> for more details.</remarks>
        public bool Equals(PartialDateTime other) => CompareTo(other) == 0;

        /// <inheritdoc cref="Equals(PartialDateTime)"/>
        public override bool Equals(object obj) => obj is PartialDateTime dt && Equals(dt);
        public static bool operator ==(PartialDateTime a, PartialDateTime b) => Equals(a, b);
        public static bool operator !=(PartialDateTime a, PartialDateTime b) => !Equals(a, b);


        /// <summary>
        /// Compare two partial datetimes based on CQL equality rules
        /// </summary>
        /// <remarks>See <see cref="TryCompare(PartialDateTime, out int)"/> for more details.</remarks>
        public int CompareTo(object obj)
        {
            if (obj is null) return 1;      // as defined by the .NET framework guidelines

            if (obj is PartialDateTime p)
            {
                return TryCompare(p, out var comparison) ? 
                    comparison : throw new ArgumentException($"Value {this} and {p} cannot be compared, since the precision is different.");                
            }
            else
                throw new ArgumentException($"Object is not a {nameof(PartialDateTime)}");
        }

        /// <inheritdoc cref="CompareTo(object)"/>
        public int CompareTo(PartialDateTime obj) => CompareTo((object)obj);

        public static bool operator <(PartialDateTime a, PartialDateTime b) => a.CompareTo(b) == -1;
        public static bool operator <=(PartialDateTime a, PartialDateTime b) => a.CompareTo(b) != 1;
        public static bool operator >(PartialDateTime a, PartialDateTime b) => a.CompareTo(b) == 1;
        public static bool operator >=(PartialDateTime a, PartialDateTime b) => a.CompareTo(b) != -1;


        /// <summary>
        /// Compares two (partial)date/times according to CQL ordering rules.
        /// </summary> 
        /// <param name="other"></param>
        /// <param name="comparison">the result of the comparison: 0 if this and other are equal, 
        /// -1 if this is smaller than other and +1 if this is bigger than other.</param>
        /// <returns>true is the values can be compared (have the same precision) or false otherwise.</returns>
        /// <remarks>The comparison is performed by considering each precision in order, beginning with years.
        /// If the values are the same, comparison proceeds to the next precision; 
        /// if the values are different, the comparison stops and the result is false. If one input has a value 
        /// for the precision and the other does not, the comparison stops and the values cannot be compared; if neither
        /// input has a value for the precision, or the last precision has been reached, the comparison stops
        /// and the result is true. For the purposes of comparison, seconds and milliseconds are combined as a 
        /// single precision using a decimal, with decimal equality semantics.</remarks>
        public bool TryCompare(PartialDateTime other, out int comparison)
        {
            if (other is null)
            {
                comparison = 1; // as defined by the .NET framework guidelines
                return true;
            }
            else
                return PartialDateTime.CompareDateTimeParts(_parsedValue, Precision, other._parsedValue, other.Precision, out comparison);
        }

        internal static bool CompareDateTimeParts(DateTimeOffset l, PartialPrecision lPrec, DateTimeOffset r, PartialPrecision rPrec, out int comparison)
        {
            l = l.ToUniversalTime();
            r = r.ToUniversalTime();

            bool success;
            (comparison,success) = docomp();
            return success;
            
            (int, bool) docomp()
            {
                if (l.Year != r.Year) return (l.Year.CompareTo(r.Year), true);

                if (lPrec < PartialPrecision.Month ^ rPrec < PartialPrecision.Month) return (0, false);
                if (l.Month != r.Month) return (l.Month.CompareTo(r.Month), true);

                if (lPrec < PartialPrecision.Day ^ rPrec < PartialPrecision.Day) return (0,false);
                if (l.Day != r.Day) return (l.Day.CompareTo(r.Day),true);

                if (lPrec < PartialPrecision.Hour ^ rPrec < PartialPrecision.Hour) return (0,false);
                if (l.Hour != r.Hour) return (l.Hour.CompareTo(r.Hour),true);

                if (lPrec < PartialPrecision.Minute ^ rPrec < PartialPrecision.Minute) return (0,false);
                if (l.Minute != r.Minute) return (l.Minute.CompareTo(r.Minute),true);

                if (lPrec < PartialPrecision.Second ^ rPrec < PartialPrecision.Second) return (0,false);

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
                if (l.Second != r.Second) return (l.Second.CompareTo(r.Second),true);
                if (l.Millisecond != r.Millisecond) return (l.Millisecond.CompareTo(r.Millisecond),true);

                return (0,true);
            }
        }

        public override int GetHashCode() => _original.GetHashCode();
        public override string ToString() => _original;

        public static explicit operator PartialDateTime(DateTimeOffset dto) => FromDateTimeOffset(dto);
    }
}
