/* 
 * Copyright (c) 2014, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

using System;
using System.Collections;
using System.Linq;
using Hl7.Fhir.Model;

namespace Hl7.Fhir.Support
{
    public static class NullExtensions
    {
        // Note: argument needs to be strongly typed (List<T>, not IList<T>) in order to prevent resolving conflicts with generic method below

        /// <summary>Determines if the list is <c>null</c> or empty.</summary>
        public static bool IsNullOrEmpty(this IList list) => list == null || list.Count == 0;

        /// <summary>
        /// Determines if the element is <c>null</c> or empty.
        /// For primitive values, verifies that the value equals <c>null</c>.
        /// For primitive string values, verifies that the string value is <c>null</c> or empty.
        /// Recursively verifies that all <see cref="Base.Children"/> instances are <c>null</c> or empty.
        /// </summary>

        public static bool IsNullOrEmpty(this Base element)
        {
            if (element == null) { return true; }

            IStringValue ss;
            PrimitiveType pp;
            var isEmpty = (ss = element as IStringValue) != null ? string.IsNullOrEmpty(ss.Value)
                : (pp = element as PrimitiveType) != null ? pp.ObjectValue == null
                : true;

            // Note: Children collection includes extensions
            return isEmpty && !element.Children.Any(c => !c.IsNullOrEmpty());
        }
    }
}
