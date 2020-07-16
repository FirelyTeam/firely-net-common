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

namespace Hl7.Fhir.ElementModel.Types
{
    public class Integer : Any, IComparable, ICqlEquatable, ICqlOrderable
    {
        public Integer() : this(default) { }

        public Integer(int value) => Value = value;

        public int Value { get; }

        public static Integer Parse(string value) =>
            TryParse(value, out var result) ? result : throw new FormatException($"String '{value}' was not recognized as a valid integer.");

        public static bool TryParse(string representation, out Integer value)
        {
            if (representation == null) throw new ArgumentNullException(nameof(representation));

            (var succ, var val) = Any.DoConvert(() => XmlConvert.ToInt32(representation));
            value = new Integer(val);
            return succ;
        }

        /// <summary>
        /// Determines if two integers are equal according to CQL equality rules.
        /// </summary>
        /// <remarks>For integers, CQL and .NET equality rules are aligned.
        /// </remarks>
        public override bool Equals(object obj) => obj is Integer i && Value == i.Value;

        public static bool operator ==(Integer a, Integer b) => Equals(a, b);
        public static bool operator !=(Integer a, Integer b) => !Equals(a, b);

        /// <summary>
        /// Compares two integers, according to CQL equality rules
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        /// <remarks>For integers, CQL and .NET comparison rules are aligned.</remarks>
        public int CompareTo(object obj)
        {
            return obj switch
            {
                null => 1,
                Integer i => Value.CompareTo(i.Value),
                _ => throw NotSameTypeComparison(this, obj)
            };
        }

        public static bool operator <(Integer a, Integer b) => a.CompareTo(b) < 0;
        public static bool operator <=(Integer a, Integer b) => a.CompareTo(b) <= 0;
        public static bool operator >(Integer a, Integer b) => a.CompareTo(b) > 0;
        public static bool operator >=(Integer a, Integer b) => a.CompareTo(b) >= 0;


        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value.ToString();

        public static implicit operator int(Integer i) => i.Value;
        public static implicit operator Long(Integer i) => new Long(i.Value);
        public static implicit operator Decimal(Integer i) => new Decimal(i.Value);
        public static implicit operator Quantity(Integer i) => new Quantity((decimal)i.Value, "1");

        public static explicit operator Integer(int i) => new Integer(i);

        bool? ICqlEquatable.IsEqualTo(Any other) => other is { } ? Equals(other) : (bool?)null;
        bool ICqlEquatable.IsEquivalentTo(Any other) => Equals(other);
        int? ICqlOrderable.CompareTo(Any other) => other is { } ? CompareTo(other) : (int?)null;
    }
}
