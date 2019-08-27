/* 
 * Copyright (c) 2015, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

using System;
using System.Xml;

namespace Hl7.Fhir.Model.Primitives
{
    public abstract class String
    {
        public static string Parse(string value) =>
            TryParse(value, out string result) ? result : throw new FormatException("String value is in an invalid format.");

        public static bool TryParse(string representation, out string value)
        {
            value = representation;   // a bit obvious
            return true;
        }
    }
}
