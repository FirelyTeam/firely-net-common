/* 
 * Copyright (c) 2015, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

using System;

namespace Hl7.Fhir.Model.Primitives
{
    public abstract class Boolean
    {
        public static bool Parse(string value) =>
            TryParse(value, out var result) ? result : throw new FormatException("Boolean value is in an invalid format.");

        public static bool TryParse(string representation, out bool value)
        {
            if (representation == "true")
            {
                value = true;
                return true;
            }
            else if (representation == "false")
            {
                value = false;
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }
    }
}
