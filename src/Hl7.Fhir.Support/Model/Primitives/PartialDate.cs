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
    public class PartialDate : Any, IComparable
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
        /// <remarks>See <see cref="TryCompareTo(PartialDate, out int)"/> for more details.</remarks>
        public override bool Equals(object obj) => obj is PartialDate pd && TryCompareTo(pd, out var result) && result == 0;


        /// <summary>
        /// Determines if two partial dates are equal according to CQL equality rules.
        /// </summary>
        /// <param name="other"></param>
        /// <param name="result">true if each date component is exactly the same. Dates with timezones are normalized to zulu before comparison is done.</param>
        /// <returns>returns true if the partial dates can be compared.</returns>
        /// <remarks>See <see cref="TryCompareTo(PartialDate, out int)"/> for more details.</remarks>
        public bool TryEquals(PartialDate other, out bool result)
        {
            var success = TryCompareTo(other, out var comparison);
            result = comparison == 0;
            return success;
        }
                        
        public static bool operator ==(PartialDate a, PartialDate b) => Equals(a, b);
        public static bool operator !=(PartialDate a, PartialDate b) => !Equals(a, b);

        /// <summary>
        /// Compare two partial dates according to CQL equality rules
        /// </summary>
        /// <remarks>See <see cref="TryCompareTo(PartialDate, out int)"/> for more details.</remarks>
        public int CompareTo(object obj)
        {
            if (obj is null) return 1;      // as defined by the .NET framework guidelines

            if (obj is PartialDate p)
            {
                return TryCompareTo(p, out var comparison) ?
                    comparison : throw new ArgumentException($"Value {this} and {p} cannot be compared, since the precision is different.");
            }
            else
                throw new ArgumentException($"Object is not a {nameof(PartialDate)}");
        }

        /// <summary>
        /// Compares two (partial)dates according to CQL ordering rules.
        /// </summary> 
        /// <param name="other"></param>
        /// <param name="comparison">the result of the comparison: 0 if this and other are equal, 
        /// -1 if this is smaller than other and +1 if this is bigger than other, or the other is null.</param>
        /// <returns>true is the values can be compared (have the same precision) or false otherwise.</returns>
        /// <remarks>The comparison is performed by considering each precision in order, beginning with years. 
        /// If the values are the same, comparison proceeds to the next precision; 
        /// if the values are different, the comparison stops and the result is false. If one input has a value 
        /// for the precision and the other does not, the comparison stops and the values cannot be compared; if neither
        /// input has a value for the precision, or the last precision has been reached, the comparison stops
        /// and the result is true.</remarks>
        public bool TryCompareTo(PartialDate other, out int comparison)
        {
            if (other is null)
            {
                comparison = 1; // as defined by the .NET framework guidelines
                return true;
            }
            else
                return PartialDateTime.CompareDateTimeParts(_parsedValue, Precision, other._parsedValue, other.Precision, out comparison);
        }

        public static bool operator <(PartialDate a, PartialDate b) => a.CompareTo(b) == -1;
        public static bool operator <=(PartialDate a, PartialDate b) => a.CompareTo(b) != 1;
        public static bool operator >(PartialDate a, PartialDate b) => a.CompareTo(b) == 1;
        public static bool operator >=(PartialDate a, PartialDate b) => a.CompareTo(b) != -1;      

        public override int GetHashCode() => _original.GetHashCode();
        public override string ToString() => _original;

        public static explicit operator PartialDate(DateTimeOffset dto) => FromDateTimeOffset(dto);
    }
}
