/* 
 * Copyright (c) 2015, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

using Hl7.Fhir.Utility;
using System;

namespace Hl7.Fhir.Model.Primitives
{
    public struct Quantity : IEquatable<Quantity>, IComparable<Quantity>, IComparable
    {
        public const string UCUM = "http://unitsofmeasure.org";

        public decimal Value { get; }
        public string Unit { get; }
        public string System => UCUM;

        public Quantity(double value, string unit) : this((decimal)value, unit)
        {
            // call other constructor
        }

        public Quantity(decimal value, string unit)
        {
            Value = value;
            Unit = unit;
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

        public override int GetHashCode() =>  Unit.GetHashCode() ^ Value.GetHashCode();

        public override string ToString() => $"{Value} {Unit}";

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

        public static Quantity Parse(string value) => throw new NotImplementedException();
        public static bool TryParse(string value, out Quantity p) => throw new NotImplementedException();
    }
}
