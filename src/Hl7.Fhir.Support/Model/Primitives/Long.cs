/* 
 * Copyright (c) 2020, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

#nullable enable

using System;
using System.Xml;

namespace Hl7.Fhir.Model.Primitives
{
    public class Long: Any, IComparable, ICqlEquatable, ICqlOrderable
    {
        public Long() : this(default) { }

        public Long(long value) => Value = value;

        public long Value { get; }

        public static Long Parse(string value) =>
            TryParse(value, out var result) ? result : throw new FormatException($"String '{value}' was not recognized as a valid long integer.");

        public static bool TryParse(string representation, out Long value)
        {
            if (representation == null) throw new ArgumentNullException(nameof(representation));

            (var succ, var val) = Any.DoConvert(() => XmlConvert.ToInt64(representation));
            value = new Long(val);
            return succ;
        }

        /// <summary>
        /// Determines if two 64-bit integers are equal according to CQL equality rules.
        /// </summary>
        /// <remarks>For 64-bits integers, CQL and .NET equality rules are aligned.
        /// </remarks>
        public override bool Equals(object obj) => obj is Long i && Value == i.Value;
        public static bool operator ==(Long a, Long b) => Equals(a, b);
        public static bool operator !=(Long a, Long b) => !Equals(a, b);

        /// <summary>
        /// Compares two 64-bit integers according to CQL equality rules
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        /// <remarks>For 64-bit integers, CQL and .NET comparison rules are aligned.</remarks>
        public int CompareTo(object obj)
        {
            return obj switch
            {
                null => 1,
                Long i => Value.CompareTo(i.Value),
                _ => throw NotSameTypeComparison(this, obj)
            };
        }

        public static bool operator <(Long a, Long b) => a.CompareTo(b) == -1;
        public static bool operator <=(Long a, Long b) => a.CompareTo(b) != 1;
        public static bool operator >(Long a, Long b) => a.CompareTo(b) == 1;
        public static bool operator >=(Long a, Long b) => a.CompareTo(b) != -1;

        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value.ToString();

        public static implicit operator long(Long i) => i.Value;
        public static implicit operator Decimal(Long i) => new Decimal(i.Value);
        public static implicit operator Quantity(Long i) => new Quantity((decimal)i.Value, "1");

        public static explicit operator Long(long i) => new Long(i);

        bool? ICqlEquatable.IsEqualTo(Any other) => other is { } ? Equals(other) : (bool?)null;
        bool ICqlEquatable.IsEquivalentTo(Any other) => Equals(other);
        int? ICqlOrderable.CompareTo(Any other) => other is { } ? CompareTo(other) : (int?)null;
    }
}