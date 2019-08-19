/* 
 * Copyright (c) 2019, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

using Hl7.Fhir.Language;
using System;
using System.Globalization;
using System.Linq;
using System.Xml;

namespace Hl7.Fhir.Model.Primitives
{
    public static class Any
    {
        public static bool IsEqualTo(object l, object r)
        {
            if (l == null && r == null) return true;
            if (l == null || r == null) return false;

            if (l is string lstr && r is string rstr)
                return lstr == rstr;
            else if (l is bool lbl && r is bool rbl)
                return lbl == rbl;
            else if (l is long llng && r is long rlng)
                return llng == rlng;
            else if (l is decimal ldec && r is decimal rdec)
                return ldec == rdec;
            else if (l is long llng2 && r is decimal rdec2)     // this really should be handled by casts outside this func (and in the engine?)
                return llng2 == rdec2;
            else if (l is decimal ldec3 && r is long rlng3)     // this really should be handled by casts outside this func (and in the engine?)
                return ldec3 == rlng3;
            else if (l is PartialTime lpt && r is PartialTime rpt)
                return lpt == rpt;
            else if (l is PartialDateTime lpdt && r is PartialDateTime rpdt)
                return lpdt == rpdt;
            else if (l is PartialDate lpd && r is PartialDate rpd)
                return lpd == rpd;
            else
                throw new ArgumentException("Can only compare System primitives string, bool, long, decimal and partial date/dateTime/time.");
        }

        // private static readonly string[] FORBIDDEN_DECIMAL_PREFIXES = new[] { "+", ".", "00" };
        // [20190819] EK Consolidated this syntax with CQL and FhirPath, which will allow leading zeroes 
        private static readonly string[] FORBIDDEN_DECIMAL_PREFIXES = new[] { "+", "." };

        public static object Parse(string value, TypeSpecifier systemType)
        {
            if (value == null) return null;

            if (TryParse(value, systemType, out object result))
                return result;
            else
                throw new FormatException($"Input string '{value}' was not in a correct format for type '{systemType}'.");
        }

        public static bool TryParse(string value, TypeSpecifier systemType, out object parsed)
        {
            if(value == null)
            {
                parsed = null;
                return true;
            }

            (bool succ, object output) result = default;

            if (systemType == TypeSpecifier.System.Boolean)
            {
                if (value == "true")
                    result = (true, true);
                else if (value == "false")
                    result = (true, false);
                else
                    result = (false, default);
            }                
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
            {
                if (FORBIDDEN_DECIMAL_PREFIXES.Any(prefix => value.StartsWith(prefix)) || value.EndsWith("."))
                    result = (false, null);
                else
                    result = doXmlConvert(() =>
                        decimal.Parse(value, NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture));
            }
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
