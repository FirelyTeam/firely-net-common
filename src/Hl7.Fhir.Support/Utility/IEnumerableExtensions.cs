/* 
 * Copyright (c) 2021, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-net-sdk/master/LICENSE
 */


using System.Collections.Generic;
using System.Linq;

namespace Hl7.Fhir.Utility
{
    public static class IEnumerableExtensions
    {
        public static bool ContainsDuplicates<T>(this IEnumerable<T> enumerable)
        {
            var knownKeys = new HashSet<T>();
            return enumerable.Any(i => !knownKeys.Add(i));
        }
    }
}
