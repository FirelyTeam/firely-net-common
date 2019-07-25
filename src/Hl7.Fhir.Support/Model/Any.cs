/* 
 * Copyright (c) 2019, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

using Hl7.Fhir.Language;
using System;

namespace Hl7.Fhir.Model.Primitives
{
    public static class Any
    {
        public static bool IsEqualTo(object l, object r)
        {
            if (l == null && r == null) return true;
            if (l == null || r == null) return false;

            if (l.GetType() == typeof(string) && r.GetType() == typeof(string))
                return (string)l == (string)r;
            else if (l.GetType() == typeof(bool) && r.GetType() == typeof(bool))
                return (bool)l == (bool)r;
            else if (l.GetType() == typeof(long) && r.GetType() == typeof(long))
                return (long)l == (long)r;
            else if (l.GetType() == typeof(decimal) && r.GetType() == typeof(decimal))
                return (decimal)l == (decimal)r;
            else if (l.GetType() == typeof(long) && r.GetType() == typeof(decimal))
                return (decimal)(long)l == (decimal)r;
            else if (l.GetType() == typeof(decimal) && r.GetType() == typeof(long))
                return (decimal)l == (decimal)(long)r;
            else if (l.GetType() == typeof(PartialTime) && r.GetType() == typeof(PartialTime))
                return (PartialTime)l == (PartialTime)r;
            else if (l.GetType() == typeof(PartialDateTime) && r.GetType() == typeof(PartialDateTime))
                return (PartialDateTime)l == (PartialDateTime)r;
            else if (l.GetType() == typeof(PartialDate) && r.GetType() == typeof(PartialDate))
                return (PartialDate)l == (PartialDate)r;
            else
                throw new ArgumentException("Can only compare Model.Primitives, bool, long, decimal and string.");
        }


    }
}
