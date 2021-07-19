/* 
 * Copyright (c) 2021, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-net-sdk/master/LICENSE
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Hl7.Fhir.Model
{
    internal static class PocoDictionary
    {
        public static string ComposeChoiceElementName(string baseName, Base elementValue) =>
            baseName + char.ToUpperInvariant(elementValue.TypeName[0]) + elementValue.TypeName.Substring(1);

        public static string ComposeChoiceElementName(string baseName, IEnumerable<Base> elementValues)
        {
            var firstValue = elementValues.FirstOrDefault();

            return firstValue is not null
                ? baseName + char.ToUpperInvariant(firstValue.TypeName[0]) + firstValue.TypeName.Substring(1)
                : throw new InvalidOperationException($"Cannot serialize an empty array at element '{baseName}'.");
        }

        public static bool HasCorrectSuffix(string elementName, string expectedSuffix, int elementLength) =>
             string.Compare(elementName.Substring(elementLength), expectedSuffix, StringComparison.OrdinalIgnoreCase) == 0;
    }
}
