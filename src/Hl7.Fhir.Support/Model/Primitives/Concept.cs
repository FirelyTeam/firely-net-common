/* 
 * Copyright (c) 2015, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Hl7.Fhir.Model.Primitives
{

    public struct Concept : IEquatable<Concept>
    {
        public Concept(Coding[] codes, string display=null)
        {
            Codes = codes;
            Display = display;
        }

        public Coding[] Codes { get; }

        public string Display { get; }

        public override bool Equals(object obj) => obj is Concept concept && Equals(concept);
        public bool Equals(Concept other) =>
            //EqualityComparer<Coding[]>.Default.Equals(Codes, other.Codes) && Display == other.Display;
            Enumerable.SequenceEqual(Codes, other.Codes) && Display == other.Display;

        public override int GetHashCode()
        {
            var hashCode = -1896247902;
            hashCode = hashCode * -1521134295 + EqualityComparer<Coding[]>.Default.GetHashCode(Codes);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Display);
            return hashCode;
        }

        public static bool operator ==(Concept left, Concept right) => left.Equals(right);

        public static bool operator !=(Concept left, Concept right) => !(left == right);

        public static Concept Parse(string value) => throw new NotImplementedException();
        public static bool TryParse(string value, out Concept p) => throw new NotImplementedException();
    }
}
