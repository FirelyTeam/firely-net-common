/* 
 * Copyright (c) 2020, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

#nullable enable

using Hl7.Fhir.Utility;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Hl7.Fhir.Model.Primitives
{
    public class Quantity : Any, IEquatable<Quantity>, IComparable<Quantity>, IComparable
    {
        public const string UCUM = "http://unitsofmeasure.org";
        public const string UCUM_UNIT = "1";

        public decimal Value { get; }
        public string? Unit { get; }
        public string System => UCUM;

        public Quantity(double value, string unit =UCUM_UNIT) : this((decimal)value, unit)
        {
        }

        public Quantity(decimal value, string unit=UCUM_UNIT)
        {
            Value = value;
            Unit = unit;
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
                TryParse(representation, out var result) ? result : throw new FormatException($"String '{representation}' was not recognized as a valid quantity.");

        public static bool TryParse(string representation, out Quantity quantity)
        {
            if (representation is null) throw new ArgumentNullException(nameof(representation));

            quantity = new Quantity(default(decimal));

            var result = QUANTITYREGEX_FOR_PARSE.Match(representation);
            if (!result.Success) return false;

            if (!Decimal.TryParse(result.Groups["value"].Value, out var value))
                return false;

            if (result.Groups["unit"].Success)
            {
                quantity = new Quantity(value!, result.Groups["unit"].Value);
                return true;
            }
            else if (result.Groups["time"].Success)
            {
                if (TryParseTimeUnit(result.Groups["time"].Value, out var tv))
                {
                    quantity = new Quantity(value!, tv!);
                    return true;
                }
                else
                    return false;
            }
            else
            {
                quantity = new Quantity(value!, unit: "1");
                return true;
            }
        }

        public static bool TryParseTimeUnit(string humanUnit, out string? ucumUnit)
        {
            if (humanUnit is null) throw new ArgumentNullException(nameof(humanUnit));

            ucumUnit = parse();
            return ucumUnit != null;

            string? parse()
            {
                switch (humanUnit)
                {
                    case "year":
                    case "years":
                        return "year";
                    case "month":
                    case "months":
                        return "month";
                    case "week":
                    case "weeks":
                        return "week";
                    case "day":
                    case "days":
                        return "day";
                    case "hour":
                    case "hours":
                        return "hour";
                    case "minute":
                    case "minutes":
                        return "minute";
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

        public const QuantityComparison CQL_EQUALS_COMPARISON = QuantityComparison.None;
        public const QuantityComparison CQL_EQUIVALENCE_COMPARISON = QuantityComparison.CompareCalendarUnits;

        /// <summary>
        /// Compare two quantities based on CQL equality rules.
        /// </summary>
        /// <param name="other"></param>
        /// <returns>true if the values have comparable units, and the converted values are the same according to decimal equality rules.
        /// Throws an <see cref="ArgumentException"/> if the arguments have incomparable units.</returns>
        /// <remarks>See <see cref="TryCompare(Quantity, QuantityComparison, out int)"/> for more details.</remarks>
        public bool Equals(Quantity other) => CompareTo(other) == 0;

        /// <summary>
        /// Compares two quantities according to CQL equivalence rules.
        /// </summary>
        public bool Equals(Quantity other, QuantityComparison comparisonType) => CompareTo(other, comparisonType) == 0;

        /// <inheritdoc cref="Equals(Quantity)"/>
        public override bool Equals(object obj) => obj is Quantity q && Equals(q);
        public static bool operator ==(Quantity a, Quantity b) => Equals(a, b);
        public static bool operator !=(Quantity a, Quantity b) => !Equals(a, b);

        public int CompareTo(object obj) => CompareTo(obj, CQL_EQUALS_COMPARISON);

        /// <summary>
        /// Compare two partial datetimes based on CQL equivalence rules
        /// </summary>
        /// <remarks>See <see cref="TryCompare(Quantity, QuantityComparison, out int)"/> for more details.</remarks>
        public int CompareTo(object obj, QuantityComparison comparisonType)
        {
            if (obj is null) return 1;      // as defined by the .NET framework guidelines

            if (obj is Quantity q)
            {
                return TryCompare(q, comparisonType, out var comparison) ?
                    comparison : throw new ArgumentException($"Value {this} and {q} cannot be compared, since the units are incompatible.");
            }
            else
                throw new ArgumentException($"Object is not a {nameof(Quantity)}");
        }

        public int CompareTo(Quantity obj) => CompareTo((object)obj);

        public int CompareTo(Quantity obj, QuantityComparison comparisonType) => CompareTo((object)obj, comparisonType);

        public static bool operator <(Quantity a, Quantity b) => a.CompareTo(b) == -1;
        public static bool operator <=(Quantity a, Quantity b) => a.CompareTo(b) != 1;
        public static bool operator >(Quantity a, Quantity b) => a.CompareTo(b) == 1;
        public static bool operator >=(Quantity a, Quantity b) => a.CompareTo(b) != -1;

        /// <summary>
        /// Compares two quantities according to CQL ordering rules.
        /// </summary> 
        /// <param name="other"></param>
        /// <param name="comparisonType"></param>
        /// <param name="comparison">the result of the comparison: 0 if this and other are equal, 
        /// -1 if this is smaller than other and +1 if this is bigger than other.</param>
        /// <returns>true is the values can be compared (have comparable units) or false otherwise.</returns>
        /// <remarks>the dimensions of each quantity must be the same, but not necessarily the unit. For example, units of 'cm' and 'm' can be compared, 
        /// but units of 'cm2' and 'cm' cannot. The comparison will be made using the most granular unit of either input. 
        /// Quantities with invalid units cannot be compared. For time-valued quantities, the comparison of calendar durations and definite 
        /// quantity durations above seconds is determined by the <paramref name="comparisonType"/></remarks>
        public bool TryCompare(Quantity other, QuantityComparison comparisonType, out int comparison)
        {
            if (other is null)
            {
                comparison = 1; // as defined by the .NET framework guidelines
                return true;
            }
            else
            {
                // Need to use our metrics library here, but for now, we'll just refuse to compare
                // if the units are not the same.
                // If we DO implement it, make sure comparison of time-valued quantities (i.e. minutes), 
                // calendar durations (i.e. year) and definite quantity durations ('a', annum) cannot be
                // compared (though seconds and miliseconds can). 
                // See http://hl7.org/fhirpath/#quantity and http://hl7.org/fhirpath/#comparison for more details.
                var unitL = this.Unit;
                var unitR = other.Unit;

                if(comparisonType.HasFlag(QuantityComparison.CompareCalendarUnits))
                {
                    unitL = normalizeCalenderUnit(unitL);
                    unitR = normalizeCalenderUnit(unitR);
                }

                if (unitL != unitR)
                    throw Error.NotSupported("Comparing quantities with different units is not yet supported");

                comparison = decimal.Compare(this.Value,other.Value);   // aligns with Decimal
                return true;
            }
        }
          

        private string? normalizeCalenderUnit(string? unit)
        {
            return unit switch
            {
                "year" => "a",
                "month" => "mo",
                "week" => "wk",
                "day" => "d",
                "hour" => "h",
                "minute" => "min",
                _ => unit
            };
        }

        public static bool operator +(Quantity a, Quantity b) => throw Error.NotSupported("Adding two quantites is not yet supported");
        public static bool operator -(Quantity a, Quantity b) => throw Error.NotSupported("Subtracting two quantites is not yet supported");

        public static bool operator *(Quantity a, Quantity b) => throw Error.NotSupported("Multiplying two quantites is not yet supported");
        public static bool operator /(Quantity a, Quantity b) => throw Error.NotSupported("Dividing two quantites is not yet supported");

        public override int GetHashCode() => (Unit, Value).GetHashCode();

        public override string ToString() => $"{Value.ToString(CultureInfo.InvariantCulture)} '{Unit}'";
    }

    /// <summary>Specifies the comparison rules for quantities.</summary>
    /// <remarks>Options are aligned with the equality and equivalence  operations for quantities
    /// defined in the CQL specification. See https://cql.hl7.org/09-b-cqlreference.html#comparison-operators-4 
    /// for more details.
    /// </remarks>
    [Flags]
    public enum QuantityComparison
    {
        None = 0,

        /// <summary>
        /// For time-valued quantities: calendar durations and definite quantity durations are considered comparable (and equivalent).
        /// </summary>
        CompareCalendarUnits = 1
    }

}
