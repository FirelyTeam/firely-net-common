/* 
 * Copyright (c) 2020, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

#nullable enable

using System;

namespace Hl7.Fhir.Model.Primitives
{
    public class Code : Any
    {
        public Code(string? system, string code, string? display=null, string? version=null)
        {
            System = system;
            Value = code ?? throw new ArgumentNullException(nameof(code));
            Display = display;
            Version = version;
        }

        public string? System { get; }
        public string Value { get; }
        public string? Display { get; }
        public string? Version { get; }

        public static Code Parse(string value) => throw new NotImplementedException();
        public static bool TryParse(string representation, out Code? value) => throw new NotImplementedException();

        public override int GetHashCode() => (System, Value, Display).GetHashCode();
        public override string ToString() => $"{Value}@{System} " + Display ?? "";
        public override bool Equals(object obj) => obj is Code c && System == c.System && Value == c.Value && Display == c.Display;
        public static bool operator ==(Code left, Code right) => left.Equals(right);
        public static bool operator !=(Code left, Code right) => !left.Equals(right);

        // Does not support equality, equivalence and ordering in the CQL sense, so no explicit implementations of these interfaces
    }
}
