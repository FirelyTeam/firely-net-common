/* 
 * Copyright (c) 2020, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

namespace Hl7.Fhir.Model.Primitives
{
    public class Concept : Any
    {
        public Concept(IEnumerable<Coding> codes, string? display=null)
        {
            Codes = codes.ToArray();
            Display = display;
        }

        public IReadOnlyCollection<Coding> Codes { get; }

        public string? Display { get; }

        public static Concept Parse(string representation) => throw new NotImplementedException();
        public static bool TryParse(string representation, out Concept? value) => throw new NotImplementedException();

        public override bool Equals(object obj) => obj is Concept c && Enumerable.SequenceEqual(Codes, c.Codes) && Display == c.Display;

        public override int GetHashCode() => (Codes, Display).GetHashCode();
        public override string ToString() => string.Join(", ", Codes) + Display != null ? $" \"{Display}\"" : "";
        public static bool operator ==(Concept left, Concept right) => left.Equals(right);
        public static bool operator !=(Concept left, Concept right) => !Equals(left,right);

    }
}
