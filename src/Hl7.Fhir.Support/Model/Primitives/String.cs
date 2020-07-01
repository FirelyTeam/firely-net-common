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

namespace Hl7.Fhir.Model.Primitives
{
    public class String: Any, IEquatable<String>, IComparable, IComparable<String>
    {
        public String() : this(string.Empty) { }

        public String(string value) => Value = value;

        public string Value { get; }

        public static String Parse(string value) =>
            TryParse(value, out var result) ? result : throw new FormatException($"String '{value}' was not recognized as a valid string.");

        public static bool TryParse(string representation, out String value)
        {
            if (representation == null) throw new ArgumentNullException(nameof(representation));

            value = new String(representation);   // a bit obvious
            return true;
        }

        /// <summary>
        /// Compares two strings according to CQL equality rules.
        /// </summary>
        /// <returns></returns>
        /// <remarks>The same as <see cref="Equals(String, StringComparison)" />
        /// with a comparison of <see cref="StringComparison.Unicode" />"/>
        /// </remarks>
        public bool Equals(String other) => Equals(other, CQL_EQUALS_COMPARISON);

        /// <summary>
        /// Compares two strings according to CQL equivalence rules.
        /// </summary>
        public bool Equals(String other, StringComparison comparisonType)
        {
            if (other is null) return false;

            if (comparisonType == StringComparison.Unicode)
                return string.CompareOrdinal(Value, other.Value) == 0;

            var l = comparisonType.HasFlag(StringComparison.NormalizeWhitespace) ? normalizeWS(Value) : Value;
            var r = comparisonType.HasFlag(StringComparison.NormalizeWhitespace) ? normalizeWS(other.Value) : other.Value;

#if !NETSTANDARD1_1
            var compareOptions = CompareOptions.None;
            if (comparisonType.HasFlag(StringComparison.IgnoreCase)) compareOptions |= CompareOptions.IgnoreCase;
            if(comparisonType.HasFlag(StringComparison.IgnoreDiacritics)) compareOptions |= CompareOptions.IgnoreNonSpace;

            return string.Compare(l, r, CultureInfo.InvariantCulture, compareOptions) == 0;
#else
            return string.Compare(l, r, comparisonType.HasFlag(StringComparison.IgnoreCase) ? 
                System.StringComparison.OrdinalIgnoreCase : System.StringComparison.Ordinal) == 0;
#endif
        }

        public const StringComparison CQL_EQUALS_COMPARISON = StringComparison.Unicode;
        public const StringComparison CQL_EQUIVALENCE_COMPARISON = StringComparison.IgnoreCase | StringComparison.IgnoreDiacritics | StringComparison.NormalizeWhitespace;

        private static string normalizeWS(string data)
        {
            var dataAsChars = data.ToCharArray();
            for (var ix = 0; ix < dataAsChars.Length; ix++)
            {
                if (char.IsWhiteSpace(dataAsChars[ix]))
                    dataAsChars[ix] = ' ';               
            }

            return new string(dataAsChars);
        }

        public override bool Equals(object obj) => obj is String s && Equals(s);
        public static bool operator ==(String a, String b) => Equals(a, b);
        public static bool operator !=(String a, String b) => !Equals(a, b);

        public int CompareTo(object obj)
        {
            if (obj is null) return 1;      // as defined by the .NET framework guidelines

            if (obj is String s)
                return string.CompareOrdinal(Value, s.Value);
            else
                throw new ArgumentException($"Object is not a {nameof(String)}", nameof(obj));
        }

        public int CompareTo(String obj) => CompareTo((object)obj);

        public static bool operator <(String a, String b) => a.CompareTo(b) == -1;
        public static bool operator <=(String a, String b) => a.CompareTo(b) != 1;
        public static bool operator >(String a, String b) => a.CompareTo(b) == 1;
        public static bool operator >=(String a, String b) => a.CompareTo(b) != -1;

        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value;

        public static implicit operator string(String s) => s.Value;
        public static explicit operator String(string s) => new String(s);
    }

    /// <summary>Specifies the comparison rules for string.</summary>
    /// <remarks>Options are aligned with the equality and equivalence  operations for string
    /// defined in the CQL specification. See https://cql.hl7.org/09-b-cqlreference.html#comparison-operators-4 
    /// for more details.
    /// </remarks>
    [Flags]
    public enum StringComparison
    {
        /// <summary>
        /// Both strings are the same based on the Unicode values for the individual 
        /// characters in the strings.
        /// </summary>
        Unicode = 0,

        /// <summary>
        /// Ignore casing when comparing strings
        /// </summary>
        IgnoreCase = 1,

        /// <summary>
        /// All whitespace characters are treated as equivalent.
        /// </summary>
        NormalizeWhitespace = 2,

        /// <summary>
        /// Ignore all Unicode non-spacing characters when comparing string
        /// </summary>
        IgnoreDiacritics = 4
    }
}


