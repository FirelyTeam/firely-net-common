/* 
 * Copyright (c) 2021, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-net-sdk/master/LICENSE
 */

#nullable enable

using Hl7.Fhir.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hl7.Fhir.Utility
{
    /// <summary>
    /// Utility methods for processing FHIR element names.
    /// </summary>
    public class ElementName
    {
        /// <summary>
        /// Creates the name for a choice element name, by adding the type suffix for the instance in <paramref name="elementValue"/> to the
        /// original element name.
        /// </summary>
        public static string AddSuffixToElementName(string elementName, Base elementValue) =>
            elementName + char.ToUpperInvariant(elementValue.TypeName[0]) + elementValue.TypeName.Substring(1);

        /// <summary>
        /// Creates the name for a choice element name, by adding the type suffix for one of the instances in <paramref name="elementValues"/> to the
        /// original element name.
        /// </summary>
        /// <remarks>FHIR serialization rules require that all repeats of a choice elements be of the same type. For performance reasons,
        /// this function will not enforce this requirement and will pick any one of the instances in <paramref name="elementValues"/>.</remarks>
        public static string AddSuffixToElementName(string baseName, IEnumerable<Base> elementValues) =>
            (elementValues.FirstOrDefault() is { } firstValue)
                ? AddSuffixToElementName(baseName, firstValue)
                : throw new ArgumentException($"Enumerable must not be empty.", nameof(elementValues));

        /// <summary>
        /// Verifies whether a suffixed element name for a choice element is composed of an original element name and the
        /// expected type suffix.
        /// </summary>
        public static bool HasCorrectSuffix(string suffixedName, string elementName, string expectedSuffix) =>
             string.Compare(suffixedName.Substring(elementName.Length), expectedSuffix, StringComparison.OrdinalIgnoreCase) == 0;

        /// <summary>
        /// Verifies whether a suffixed element name for a choice element is composed of an original element name and the
        /// expected suffix for the type of the given instance.
        /// </summary>
        public static bool HasCorrectSuffix(string suffixedName, string elementName, Base elementValue) =>
            HasCorrectSuffix(suffixedName, elementName, elementValue.TypeName);

        /// <summary>
        /// Verifies whether a suffixed element name for a choice element is composed of an original element name and the
        /// expected suffix for the type of the given instance.
        /// </summary>
        /// <remarks>FHIR serialization rules require that all repeats of a choice elements be of the same type. For performance reasons,
        /// this function will not enforce this requirement and will pick any one of the instances in <paramref name="elementValues"/>.</remarks>
        public static bool HasCorrectSuffix(string suffixedName, string elementName, IEnumerable<Base> elementValues) =>
            (elementValues.FirstOrDefault() is { } firstValue)
            ? HasCorrectSuffix(suffixedName, elementName, firstValue.TypeName)
            : throw new ArgumentException($"Enumerable must not be empty.", nameof(elementValues));
    }
}

#nullable restore