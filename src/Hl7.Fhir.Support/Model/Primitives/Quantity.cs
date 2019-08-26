/* 
 * Copyright (c) 2015, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

using Hl7.Fhir.Utility;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Hl7.Fhir.Model.Primitives
{
    public struct Quantity : IEquatable<Quantity>, IComparable<Quantity>, IComparable
    {
        public const string UCUM = "http://unitsofmeasure.org";
        public const string UCUM_UNIT = "1";

        public decimal Value { get; }
        public string Unit { get; }
        public string System => UCUM;

        public Quantity(long value, string unit = null) : this((decimal)value, unit)
        {
            // call other constructor
        }
        public Quantity(double value, string unit=null) : this((decimal)value, unit)
        {
            // call other constructor
        }

        public Quantity(decimal value, string unit=null)
        {
            Value = value;
            Unit = unit ?? UCUM_UNIT;
        }

        private static readonly string QUANTITY_BASE_REGEX =
           @"(?'value'(\+|-)?\d+(\.\d+)?)\s*(('(?'unit'[^\']+)')|(?'time'[a-zA-Z]+))";

        public static readonly Regex QUANTITYREGEX =
           new Regex(QUANTITY_BASE_REGEX,
#if NETSTANDARD1_1
                        RegexOptions.ExplicitCapture);
#else
                        RegexOptions.ExplicitCapture | RegexOptions.Compiled);
#endif

        internal static readonly Regex QUANTITYREGEX_FOR_PARSE =
            new Regex($"^{QUANTITY_BASE_REGEX}?$",
#if NETSTANDARD1_1
                        RegexOptions.ExplicitCapture);
#else
                        RegexOptions.ExplicitCapture | RegexOptions.Compiled);
#endif

        public static Quantity Parse(string representation) =>
                TryParse(representation, out var result) ? result : throw new FormatException("Quantity is in an invalid format.");

        public static bool TryParse(string representation, out Quantity quantity)
        {
            quantity = default;

            var result = QUANTITYREGEX_FOR_PARSE.Match(representation);
            if (!result.Success) return false;

            if (!Decimal.TryParse(result.Groups["value"].Value, out var value))
                return false;

            if (result.Groups["unit"].Success)
            {
                quantity = new Quantity(value, result.Groups["unit"].Value);
                return true;
            }
            else if (result.Groups["time"].Success)
            {
                if (TryParseTimeUnit(result.Groups["time"].Value, out var tv))
                {
                    quantity = new Quantity(value, tv);
                    return true;
                }
                else
                    return false;
            }
            else
            {
                quantity = new Quantity(value, unit: "1");
                return true;
            }
        }

        public static bool TryParseTimeUnit(string humanUnit, out string ucumUnit)
        {
            ucumUnit = parse();
            return ucumUnit != null;

            string parse()
            {
                switch (humanUnit)
                {
                    case "year":
                    case "years":
                        return "a";
                    case "month":
                    case "months":
                        return "mo";
                    case "week":
                    case "weeks":
                        return "wk";
                    case "day":
                    case "days":
                        return "d";
                    case "hour":
                    case "hours":
                        return "h";
                    case "minute":
                    case "minutes":
                        return "min";
                    case "second":
                    case "seconds":
                        return "s";
                    case "millisecond":
                    case "milliseconds":
                        return "ms";
                    default:
                        return null;
                }
            }
        }

        public static bool operator <(Quantity a, Quantity b)
        {
            enforceSameUnits(a,b);

            return a.Value < b.Value;
        }

        public static bool operator <=(Quantity a, Quantity b)
        {
            enforceSameUnits(a, b);

            return a.Value <= b.Value;
        }


        public static bool operator >(Quantity a, Quantity b)
        {
            enforceSameUnits(a, b);

            return a.Value > b.Value;
        }

        public static bool operator >=(Quantity a, Quantity b)
        {
            enforceSameUnits(a, b);

            return a.Value >= b.Value;
        }


        public static bool operator ==(Quantity a, Quantity b)
        {
            enforceSameUnits(a, b);

            return Object.Equals(a, b);
        }

        public static bool operator !=(Quantity a, Quantity b) => !(a == b);


        private static void enforceSameUnits(Quantity a, Quantity b)
        {
            if (a.Unit != b.Unit)
                throw Error.NotSupported("Comparing quantities with different units is not yet supported");
        }

        public bool Equals(Quantity other) => other.Unit == Unit && other.Value == Value;
        public override bool Equals(object obj) => obj is Quantity other && Equals(other);

        public bool IsEqualTo(Quantity other) => throw new NotImplementedException();

        public bool IsEquivalentTo(Quantity other) => throw new NotImplementedException();

        public override int GetHashCode() =>  Unit.GetHashCode() ^ Value.GetHashCode();

        public override string ToString() => $"{Value.ToString(CultureInfo.InvariantCulture)} '{Unit}'";

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            if (obj is Quantity p)
            {
                if (this < p) return -1;
                if (this > p) return 1;
                return 0;
            }
            else
                throw Error.Argument(nameof(obj), $"Must be a {nameof(Quantity)}");
        }

        public int CompareTo(Quantity other) => CompareTo((object)other);
    }
}
