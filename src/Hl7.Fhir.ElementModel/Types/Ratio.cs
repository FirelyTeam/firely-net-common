/* 
 * Copyright (c) 2020, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

#nullable enable

using System;

namespace Hl7.Fhir.ElementModel.Types
{
    public class Ratio : Any
    {
        public Ratio(Quantity numerator, Quantity denominator)
        {
            Numerator = numerator ?? throw new ArgumentNullException(nameof(numerator));
            Denominator = denominator ?? throw new ArgumentNullException(nameof(denominator));
        }

        public Quantity Numerator { get; }
        public Quantity Denominator { get; }

        public static Quantity Parse(string representation) => throw new NotImplementedException();
        public static bool TryParse(string representation, out Quantity? value) => throw new NotImplementedException();

        public override bool Equals(object obj) => obj is Ratio r && Numerator == r.Numerator && Denominator == r.Denominator;

        public override int GetHashCode() => (Numerator, Denominator).GetHashCode();
        public override string ToString() => $"{Numerator} : {Denominator}";
        public static bool operator ==(Ratio left, Ratio right) => left.Equals(right);
        public static bool operator !=(Ratio left, Ratio right) => !Equals(left, right);

        // Does not support equality, equivalence and ordering in the CQL sense, so no explicit implementations of these interfaces
    }
}
