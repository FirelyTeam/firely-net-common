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
    public class Integer64: Any, IComparable
    {
        public Integer64() : this(default) { }

        public Integer64(long value) => Value = value;

        public long Value { get; }

        public static Integer64 Parse(string value) =>
            TryParse(value, out var result) ? result : throw new FormatException($"String '{value}' was not recognized as a valid long integer.");

        public static bool TryParse(string representation, out Integer64 value)
        {
            if (representation == null) throw new ArgumentNullException(nameof(representation));

            (var succ, var val) = Any.DoConvert(() => XmlConvert.ToInt64(representation));
            value = new Integer64(val);
            return succ;
        }

        /// <summary>
        /// Determines if two 64-bit integers are equal according to CQL equality rules.
        /// </summary>
        /// <remarks>For 64-bits integers, CQL and .NET equality rules are aligned.
        /// </remarks>
        public override bool Equals(object obj) => obj is Integer64 i && Value == i.Value;
        public static bool operator ==(Integer64 a, Integer64 b) => Equals(a, b);
        public static bool operator !=(Integer64 a, Integer64 b) => !Equals(a, b);

        /// <summary>
        /// Compares two 64-bit integers according to CQL equality rules
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        /// <remarks>For 64-bit integers, CQL and .NET comparison rules are aligned.</remarks>
        public int CompareTo(object obj)
        {
            if (obj is null) return 1;      // as defined by the .NET framework guidelines

            if (obj is Integer64 i)
                return Value.CompareTo(i.Value);
            else
                throw new ArgumentException($"Object is not a {nameof(Integer64)}", nameof(obj));
        }

        public static bool operator <(Integer64 a, Integer64 b) => a.CompareTo(b) == -1;
        public static bool operator <=(Integer64 a, Integer64 b) => a.CompareTo(b) != 1;
        public static bool operator >(Integer64 a, Integer64 b) => a.CompareTo(b) == 1;
        public static bool operator >=(Integer64 a, Integer64 b) => a.CompareTo(b) != -1;

        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value.ToString();

        public static implicit operator long(Integer64 i) => i.Value;
        public static explicit operator Integer64(long i) => new Integer64(i);

    }
}