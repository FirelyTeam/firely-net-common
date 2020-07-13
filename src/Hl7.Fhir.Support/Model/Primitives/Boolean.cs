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
    public class Boolean : Any, ICqlEquatable
    {
        public static Boolean True = new Boolean(true);
        public static Boolean False = new Boolean(false);

        public Boolean() : this(default) { }
        public Boolean(bool value) => Value = value;
      
        public bool Value { get; }

        public static Boolean Parse(string value) =>
            TryParse(value, out var result) ? result! : throw new FormatException($"String '{value}' was not recognized as a valid boolean.");

        public static bool TryParse(string representation, out Boolean? value)
        {
            if (representation is null) throw new ArgumentNullException(nameof(representation));

            if (representation == "true")
            {
                value = True;
                return true;
            }
            else if (representation == "false")
            {
                value = False;
                return true;
            }
            else
            {
                value = default;
                return false;               
            }
        }

        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value.ToString();
        public override bool Equals(object obj) => obj is Boolean b && Value == b.Value;

        public static bool operator ==(Boolean a, Boolean b) => Equals(a,b);
        public static bool operator !=(Boolean a, Boolean b) => !Equals(a,b);

        public static implicit operator bool(Boolean b) => b.Value;

        bool? ICqlEquatable.IsEqualTo(Any other) => other is { } ? (bool?)Equals(other) : null;
        bool ICqlEquatable.IsEquivalentTo(Any other) => Equals(other);
    }
}
