/* 
 * Copyright (c) 2015, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

using Hl7.Fhir.ElementModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hl7.FhirPath.Functions
{
    internal static class StringOperators
    {
        public static string FpSubstring(this string me, long start, long? length)
        {
            var l = length ?? me.Length;

            if (start < 0 || start >= me.Length) return null;
            l = Math.Min(l, me.Length - start);

            return me.Substring((int)start, (int)l);
        }

        public static ITypedElement FpIndexOf(this string me, string fragment)
        {
            return ElementNode.ForPrimitive(me.IndexOf(fragment));
        }

        public static IEnumerable<ITypedElement> ToChars(this string me) =>
            me.ToCharArray().Select(c => ElementNode.ForPrimitive(c));

        public static string FpReplace(this string me, string find, string replace)
        {
            if (find == String.Empty)
            {
                // weird, but as specified:  "abc".replace("","x") = "xaxbxcx"
                return replace + String.Join(replace, me.ToCharArray()) + replace;
            }
            else
                return me.Replace(find, replace);
        }

        public static IEnumerable<ITypedElement> FpSplit(this string me, string seperator)
        {
            var results = me.Split(new[] { seperator }, StringSplitOptions.RemoveEmptyEntries);
            return results.Select(s => ElementNode.ForPrimitive(s));
        }
    }
}
