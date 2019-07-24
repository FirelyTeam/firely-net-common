/* 
 * Copyright (c) 2019, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Hl7.Fhir.Model.Primitives
{
    public struct PartialTime : IComparable, IEquatable<PartialTime>
    {
        public static PartialTime Parse(string value) =>
            TryParse(value, out PartialTime result) ? result : throw new FormatException("Time value is in an invalid format.");

        public static bool TryParse(string representation, out PartialTime value) =>
            tryParse(representation, 2016, 1, 1, out value);

        public int? Hours => HasHours ? _parsedValue.Hour : (int?)null;
        public int? Minutes => HasMinutes ? _parsedValue.Minute : (int?)null;
        public int? Seconds => HasSeconds ? _parsedValue.Second : (int?)null;
        public int? Millis => HasFraction ? _parsedValue.Millisecond : (int?)null;
        public TimeSpan? Offset => HasOffset ? _parsedValue.Offset : (TimeSpan?)null;

        private string _original;
        private DateTimeOffset _parsedValue;

        /// <summary>
        /// Whether the time has precision of hours or more.
        /// </summary>
        /// <returns></returns>
        public bool HasHours { get; private set; }

        /// <summary>
        /// Whether the time has precision of minutes or more.
        /// </summary>
        /// <returns></returns>
        public bool HasMinutes { get; private set; }

        /// <summary>
        /// Whether the time has precision of seconds or more.
        /// </summary>
        public bool HasSeconds { get; private set; }

        /// <summary>
        /// Whether the time includes a seconds fraction.
        /// </summary>
        public bool HasFraction { get; private set; }

        /// <summary>
        /// Whether the time specifies an offset to UTC
        /// </summary>
        public bool HasOffset { get; private set; }

        // Our regex is pretty flexible, it does not bother to capture rules about semantics (12:64 would be legal here), it
        // even accepts just a timezone. Additional semantic checks will be verified using the built-in DateTimeOffset .NET parser.
        // Also, it accept the superset of formats specified by FHIR, CQL, FhirPath and the mapping language. Each of these
        // specific implementations may add additional constraints (e.g. about minimum precision or presence of timezones).
        private const string TIMEFORMAT =
            "^((?<hours>[0-9][0-9]) ((?<minutes>:[0-9][0-9]) ((?<seconds>:[0-9][0-9]) ((?<frac>.[0-9]+))?)?)?)?" +
            "(?<offset>Z | (\\+|-) [0-9][0-9]:[0-9][0-9])?$";

        private static readonly Regex TIMEREGEX = new Regex(TIMEFORMAT, RegexOptions.IgnorePatternWhitespace);

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

        public const string FMT_FULL = "HH:mm:ss.FFFFFFFK";

        public static PartialTime FromDateTimeOffset(DateTimeOffset dto)
        {
            var representation = dto.ToString(FMT_FULL);
            return Parse(representation);
        }

        public static PartialTime Now() => FromDateTimeOffset(DateTimeOffset.Now);

        private static bool tryParse(string representation, int year, int month, int day, out PartialTime value)
        {
            value = new PartialTime();

            if (String.IsNullOrEmpty(representation)) return false;
         
            var matches = TIMEREGEX.Match(representation);
            if (!matches.Success) return false;

            var hrg = matches.Groups["hours"];
            var ming = matches.Groups["minutes"];
            var secg = matches.Groups["seconds"];
            var fracg = matches.Groups["frac"];
            var offset = matches.Groups["offset"];

            value.HasHours = hrg.Success;
            value.HasMinutes = ming.Success;
            value.HasSeconds = secg.Success;
            value.HasFraction = fracg.Success;
            value.HasOffset = offset.Success;

            var parseableDT = $"{year:0000}-{month:00}-{day:00}" + "T" +
                    (hrg.Success ? hrg.Value : "00") +
                    (ming.Success ? ming.Value : ":00") +
                    (secg.Success ? secg.Value : ":00") +
                    (fracg.Success ? fracg.Value : "") +
                    (offset.Success ? offset.Value : "");

            value._original = representation;
            return DateTimeOffset.TryParse(parseableDT, out value._parsedValue);
        }

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
                if (this < p)
                    return -1;
                else if (this > p)
                    return 1;
                else
                    return 0;
            }
            else
                throw new ArgumentException(nameof(obj), "Must be a Time");
        }

        public override bool Equals(object obj) => obj is PartialTime time && Equals(time);
        public bool Equals(PartialTime other) => other.toComparable() == toComparable();
        public override int GetHashCode() => -1939223833 + EqualityComparer<DateTimeOffset>.Default.GetHashCode(toComparable());
        public override string ToString() => _original;
    }
}



