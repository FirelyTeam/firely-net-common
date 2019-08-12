/* 
 * Copyright (c) 2019, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

using Hl7.Fhir.Language;
using System;
using System.Xml;

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

        private delegate bool parseFunc(string value, out object result);


        public static bool TryParse(string value, TypeSpecifier systemType, out object parsed)
        {
            (bool succ, object output) result = default;

            if (systemType == TypeSpecifier.System.Boolean)
                result = doXmlConvert(() => XmlConvert.ToBoolean(value));
            else if (systemType == TypeSpecifier.System.Code)
            {
                var success = Coding.TryParse(value, out var p);
                result = (success, p);
            }
            else if (systemType == TypeSpecifier.System.Concept)
            {
                var success = Concept.TryParse(value, out var p);
                result = (success, p);
            }
            else if (systemType == TypeSpecifier.System.Date)
            {
                var success = PartialDate.TryParse(value, out var p);
                result = (success, p);
            }
            else if (systemType == TypeSpecifier.System.DateTime)
            {
                var success = PartialDateTime.TryParse(value, out var p);
                result = (success, p);
            }
            else if (systemType == TypeSpecifier.System.Decimal)
                result = doXmlConvert(() => XmlConvert.ToDecimal(value));
            else if (systemType == TypeSpecifier.System.Integer)
                result = doXmlConvert(() => XmlConvert.ToInt64(value));
            else if (systemType == TypeSpecifier.System.Quantity)
            {
                var success = Quantity.TryParse(value, out var p);
                result = (success, p);
            }
            else if (systemType == TypeSpecifier.System.String)
                result = (true, value);
            else if (systemType == TypeSpecifier.System.Time)
            {
                var success = PartialTime.TryParse(value, out var p);
                result = (success, p);
            }
            else
                result = (false, null);

            parsed = result.output;
            return result.succ;

            (bool, object) doXmlConvert(Func<object> parser)
            {
                try
                {
                    return (true, parser());
                }
                catch (Exception)
                {
                    return (false, null);
                }
            }

        }

        public static string ToRepresentation() => throw new NotImplementedException();

        /// <summary>
        /// Converts a primitive .NET instance to a System-based instance.
        /// </summary>
        public static object ConvertToSystemValue(object value)
        {
            if (value == null) return null;

            if (TryConvertToSystemValue(value, out object result))
                return result;
            else
                throw new NotSupportedException($"There is no known System type corresponding to the .NET type {value.GetType().Name} of this instance (with value '{value}').");
        }

        /// <summary>
        /// Try to converts a .NET instance to a System-based instance.
        /// </summary>
        public static bool TryConvertToSystemValue(object value, out object primitiveValue)
        {
            if (value == null)
            {
                primitiveValue = null;
                return true;
            }

            // NOTE: Keep Any.TryConvertToSystemValue, TypeSpecifier.TryGetNativeType and TypeSpecifier.ForNativeType in sync
            if (value is bool || value is PartialTime || value is PartialDateTime || value is PartialDate || value is Quantity
                    || value is Coding || value is Concept || value is string)
                primitiveValue = value;
            else if (value is int || value is short || value is ushort || value is uint || value is long || value is ulong)
                primitiveValue = Convert.ToInt64(value);
            else if (value is DateTimeOffset dto)
                primitiveValue = PartialDateTime.FromDateTimeOffset(dto);
            else if (value is float || value is double || value is decimal)
                primitiveValue = Convert.ToDecimal(value);
            else if (value is char)
                primitiveValue = new String((char)value, 1);
            else if (value is Uri u)
                primitiveValue = u.OriginalString;
            else
                primitiveValue = null;

            return primitiveValue != null;
        }
    }
}
