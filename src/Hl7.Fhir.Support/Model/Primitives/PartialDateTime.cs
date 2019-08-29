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
    public struct PartialDateTime : IComparable, IComparable<PartialDateTime>, IEquatable<PartialDateTime>
    {
        public static PartialDateTime Parse(string representation) =>
            TryParse(representation, out var result) ? result : throw new FormatException("DateTime value is in an invalid format.");

        public static bool TryParse(string representation, out PartialDateTime value) =>
            tryParse(representation, out value);

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

        private string _original;
        private DateTimeOffset _parsedValue;

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

        public static PartialDateTime FromDateTimeOffset(DateTimeOffset dto)
        {
            var representation = dto.ToString(FMT_FULL);
            return Parse(representation);
        }

        [Obsolete("FromDateTime() has been renamed to FromDateTimeOffset()")]
        public static PartialDateTime FromDateTime(DateTimeOffset dto) => FromDateTimeOffset(dto);

        public static PartialDateTime Now() => FromDateTimeOffset(DateTimeOffset.Now);

        public static PartialDateTime Today() => PartialDateTime.Parse(DateTimeOffset.Now.ToString("yyyy-MM-ddK"));

        private static bool tryParse(string representation, out PartialDateTime value)
        {
            value = new PartialDateTime();
            
            var matches = DATETIMEREGEX.Match(representation);
            if (!matches.Success) return false;

            var yrg = matches.Groups["year"];
            var mong = matches.Groups["month"];
            var dayg = matches.Groups["day"];
            var hrg = matches.Groups["hours"];
            var ming = matches.Groups["minutes"];
            var secg = matches.Groups["seconds"];
            var fracg = matches.Groups["frac"];
            var offset = matches.Groups["offset"];

            value.Precision =
                        fracg.Success ? PartialPrecision.Fraction :
                        secg.Success ? PartialPrecision.Second :
                        ming.Success ? PartialPrecision.Minute :
                        hrg.Success ? PartialPrecision.Hour :
                        dayg.Success ? PartialPrecision.Day :
                        mong.Success ? PartialPrecision.Month :
                        PartialPrecision.Year;

            value.HasOffset = offset.Success;
            value._original = representation;

            var parseableDT = yrg.Value +
                  (mong.Success ? mong.Value : "-01") +
                  (dayg.Success ? dayg.Value : "-01") +
                  (hrg.Success ? "T"+hrg.Value : "T00") +
                  (ming.Success ? ming.Value : ":00") +
                  (secg.Success ? secg.Value : ":00") +
                  (fracg.Success ? fracg.Value : "") +
                  (offset.Success ? offset.Value : "Z");

            value._original = representation;
            return DateTimeOffset.TryParse(parseableDT, out value._parsedValue);
        }

        public bool IsEqualTo(PartialDateTime other) => this == other;

        public bool IsEquivalentTo(PartialDateTime other) => throw new NotImplementedException();

        // TODO: Note, this enables comparisons between values that did or did not have timezones, need to fix.
        private DateTimeOffset toComparable() => _parsedValue.ToUniversalTime();

        public static bool operator <(PartialDateTime a, PartialDateTime b) => a.toComparable() < b.toComparable();
        public static bool operator <=(PartialDateTime a, PartialDateTime b) => a.toComparable() <= b.toComparable();
        public static bool operator >(PartialDateTime a, PartialDateTime b) => a.toComparable() > b.toComparable();
        public static bool operator >=(PartialDateTime a, PartialDateTime b) => a.toComparable() >= b.toComparable();
        public static bool operator ==(PartialDateTime a, PartialDateTime b) => Equals(a, b);
        public static bool operator !=(PartialDateTime a, PartialDateTime b) => !(a == b);

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            if (obj is PartialDateTime p)
            {
                return (this < p) ? -1 :
                    (this > p) ? 1 : 0;
            }
            else
                throw new ArgumentException($"Object is not a {nameof(PartialDateTime)}");
        }

        public bool Equals(PartialDateTime other) => this.Precision == other.Precision && other.toComparable() == toComparable();
        public override int GetHashCode() => toComparable().GetHashCode();
        public override string ToString() => _original;
        public int CompareTo(PartialDateTime obj) => CompareTo((object)obj);
        public override bool Equals(object obj) => obj is PartialDateTime dt && Equals(dt);
    }
}
