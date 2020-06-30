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
    public class Integer: Any, IEquatable<Integer>, IComparable, IComparable<Integer>
    {
        public Integer() : this(default) { }

        public Integer(int value) => Value = value;

        public int Value { get; }

        public static int Parse(string value) =>
            TryParse(value, out int result) ? result : throw new FormatException($"String '{value}' was not recognized as a valid integer.");

        public static bool TryParse(string representation, out int value)
        {
            if (representation == null) throw new ArgumentNullException(nameof(representation));

            (var succ, var val) = Any.DoConvert(() => XmlConvert.ToInt32(representation));
            value = val;
            return succ;
        }

        /// <summary>
        /// Compares two integers according to CQL equality (and equivalence) rules.
        /// </summary>
        /// <returns>Return true if both arguments are exactly the same integer value, false otherwise.
        /// </returns>
        public bool Equals(Integer other) => other is { } && Int32.Equals(Value, other.Value);

        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value.ToString();

        public override bool Equals(object obj) => obj is Integer i && Equals(i);
        public static bool operator ==(Integer a, Integer b) => Equals(a, b);
        public static bool operator !=(Integer a, Integer b) => !Equals(a, b);

    }
}
