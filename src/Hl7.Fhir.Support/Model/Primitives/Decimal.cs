/* 
 * Copyright (c) 2020, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

#nullable enable

using System;
using System.Globalization;
using System.Linq;

namespace Hl7.Fhir.Model.Primitives
{
    public class Decimal : Any, IEquatable<Decimal>, IComparable, IComparable<Decimal>
    {
        public Decimal() : this(default) { }

        public Decimal(decimal value) => Value = value;

        public decimal Value { get; }

        // private static readonly string[] FORBIDDEN_DECIMAL_PREFIXES = new[] { "+", ".", "00" };
        // [20190819] EK Consolidated this syntax with CQL and FhirPath, which will allow leading zeroes 
        private static readonly string[] FORBIDDEN_DECIMAL_PREFIXES = new[] { "+", "." };

        public static Decimal Parse(string value) =>
            TryParse(value, out var result) ? result! : throw new FormatException($"String '{value}' was not recognized as a valid decimal.");

        public static bool TryParse(string representation, out Decimal? value)
        {
            if (representation == null) throw new ArgumentNullException(nameof(representation));

            value = default;

            if (FORBIDDEN_DECIMAL_PREFIXES.Any(prefix => representation.StartsWith(prefix)) || representation.EndsWith("."))
                return false;

            (var succ, var val) = Any.DoConvert(() => decimal.Parse(representation, NumberStyles.AllowDecimalPoint |
                   NumberStyles.AllowExponent |
                   NumberStyles.AllowLeadingSign,
                   CultureInfo.InvariantCulture));
            value = new Decimal(val);
            return succ;
        }


        /// <summary>
        /// Compares two decimals according to CQL ordering rules.
        /// </summary>
        /// <remarks>The same as <see cref="Equals(Decimal, DecimalComparison)" />
        /// with a comparison of <see cref="DecimalComparison.IgnoreTrailingZeroes" />"/>
        /// </remarks>
        public bool Equals(Decimal other) => other is { } && decimal.Equals(Value,other.Value);

        /// <summary>
        /// Compares two decimals according to CQL ordering rules.
        /// </summary>
        public bool Equals(Decimal other, DecimalComparison comparisonType) 
        {
            if (other is null) return false;

            return comparisonType switch
            {
                DecimalComparison.Strict =>
                    (Scale(this.Value, ignoreTrailingZeroes: false) == Scale(other.Value, ignoreTrailingZeroes: false)) &&
                        Value == other.Value,
                DecimalComparison.IgnoreTrailingZeroes =>
                    Value == other.Value,      // default .NET decimal behaviour
                DecimalComparison.RoundToSmallestScale => scaleEq(Value, other.Value),
            };

            // The CQL and FhirPath spec talk about 'precision' (number of digits), but might mean 'scale' 
            // (number of decimals). Since the first has no native support on .NET, I'll be sloppy and
            // assume scale is meant.
            static bool scaleEq(decimal a, decimal b)
            {
                var roundPrec = Math.Min(Scale(a, true), Scale(b, true));
                var lr = Math.Round(a, roundPrec);
                var rr = Math.Round(b, roundPrec);
                return lr == rr;
            }
        }

        public static int Scale(decimal d, bool ignoreTrailingZeroes)
        {
            var sr = d.ToString(CultureInfo.InvariantCulture);
            var pointPos = sr.IndexOf('.');
            if (pointPos == -1) return 0;

            if (ignoreTrailingZeroes) sr = sr.TrimEnd('0');

            return pointPos == -1 ? 0 : sr.Length - pointPos - 1;   // -1 for the decimal sign
        }

        public int CompareTo(object obj)
        {
            if (obj is null) return 1;      // as defined by the .NET framework guidelines

            if (obj is Decimal d)
                return decimal.Compare(Value, d.Value);
            else
                throw new ArgumentException($"Object is not a {nameof(PartialDate)}", nameof(obj));
        }

        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value.ToString();

        public override bool Equals(object obj) => obj is Decimal d && Equals(d);
        public static bool operator ==(Decimal a, Decimal b) => Equals(a, b);
        public static bool operator !=(Decimal a, Decimal b) => !Equals(a, b);

        public int CompareTo(Decimal obj) => CompareTo((object)obj);

        public static bool operator <(Decimal a, Decimal b) => a.CompareTo(b) == -1;
        public static bool operator <=(Decimal a, Decimal b) => a.CompareTo(b) != 1;
        public static bool operator >(Decimal a, Decimal b) => a.CompareTo(b) == 1;
        public static bool operator >=(Decimal a, Decimal b) => a.CompareTo(b) != -1;

        public static implicit operator decimal(Decimal d) => d.Value;
        public static explicit operator Decimal(decimal d) => new Decimal(d);        
    }


    /// <summary>Specifies the comparison rules for decimals.</summary>
    /// <remarks>Options are aligned with the equality and equivalence  operations for decimals
    /// defined in the CQL specification. See https://cql.hl7.org/09-b-cqlreference.html#comparison-operators-4 
    /// for more details.
    /// </remarks>
    public enum DecimalComparison
    {
        Strict,

        /// <summary>
        /// Trailing zeroes after the decimal are ignored in determining precision.
        /// </summary>
        IgnoreTrailingZeroes,

        /// <summary>
        /// Comparison is done on values rounded to the scale of the
        /// least precise operand. Implies <see cref="DecimalComparison.IgnoreTrailingZeroes" />.
        /// </summary>
        RoundToSmallestScale
    }
}
