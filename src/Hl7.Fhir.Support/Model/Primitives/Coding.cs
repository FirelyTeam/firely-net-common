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
    public class Coding : Any
    {
        public Coding(string? system, string code, string? display=null)
        {
            System = system;
            Code = code ?? throw new ArgumentNullException(nameof(code));
            Display = display;
        }

        public string? System { get; }
        public string Code { get; }
        public string? Display { get; }

        public static Coding Parse(string value) => throw new NotImplementedException();
        public static bool TryParse(string representation, out Coding? value) => throw new NotImplementedException();

        public override int GetHashCode() => (System, Code, Display).GetHashCode();
        public override string ToString() => $"{Code}@{System} " + Display ?? "";
        public override bool Equals(object obj) => obj is Coding c && System == c.System && Code == c.Code && Display == c.Display;
        public static bool operator ==(Coding left, Coding right) => left.Equals(right);
        public static bool operator !=(Coding left, Coding right) => !left.Equals(right);

    }
}
