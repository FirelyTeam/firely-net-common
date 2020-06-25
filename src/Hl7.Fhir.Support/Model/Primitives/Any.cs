/* 
 * Copyright (c) 2019, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

using Hl7.Fhir.Language;
using Hl7.Fhir.Utility;
using System;

namespace Hl7.Fhir.Model.Primitives
{
    public static class Any
    {
        public static bool? IsEqualTo(object l, object r)
        {
            if (l == null || r == null) return null;

            // this really should be handled by casts outside this func (and in the engine?)
            l = UpcastOperand(l, r);
            r = UpcastOperand(r, l);

            if (l is string lstr && r is string rstr)
                return String.IsEqualTo(lstr,rstr);
            else if (l is bool lbl && r is bool rbl)
                return Boolean.IsEqualTo(lbl, rbl);
            else if (l is int lint && r is int rint)
                return Integer.IsEqualTo(lint, rint);
            else if (l is long llng && r is long rlng)
                return Integer64.IsEqualTo(llng, rlng);
            else if (l is decimal ldec && r is decimal rdec)
                return Decimal.IsEqualTo(ldec,rdec);
            else if (l is PartialTime lpt && r is PartialTime rpt)
                return lpt.IsEqualTo(rpt);
            else if (l is PartialDateTime lpdt && r is PartialDateTime rpdt)
                return lpdt.IsEqualTo(rpdt);
            else if (l is PartialDate lpd && r is PartialDate rpd)
                return PartialDate.IsEqualTo(lpd,rpd);
            else if (l is Quantity lq && r is Quantity rq)
                return lq.IsEqualTo(rq);
            else
                // the spec says that this should be false - you can compare
                // anything to anything!
                //throw new ArgumentException("Can only compare System primitives string, bool, long, decimal and partial date/dateTime/time.");
                return false;
        }

        internal static object UpcastOperand(object value, object other)
        {
            if (value is int && other is long) return (long)value;
            if (value is long && other is decimal) return (decimal)value;

            // nothing to upcast, return value;
            return value;
        }

        public static bool IsEquivalentTo(object l, object r)
        {
            if (l == null && r == null) return true;
            if (l == null || r == null) return false;

            // this really should be handled by casts outside this func (and in the engine?)
            l = UpcastOperand(l, r);
            r = UpcastOperand(r, l);

            if (l is string lstr && r is string rstr)
                return String.IsEquivalentTo(lstr,rstr);
            else if (l is bool lbl && r is bool rbl)
                return Boolean.IsEquivalentTo(lbl, rbl);
            else if (l is int lint && r is int rint)
                return Integer.IsEquivalentTo(lint,rint);
            else if (l is long llng && r is long rlng)
                return Integer64.IsEquivalentTo(llng, rlng);
            else if (l is decimal ldec && r is decimal rdec)
                return Decimal.IsEquivalentTo(ldec,rdec);
            else if (l is PartialTime lpt && r is PartialTime rpt)
                return lpt.IsEquivalentTo(rpt);
            else if (l is PartialDateTime lpdt && r is PartialDateTime rpdt)
                return lpdt.IsEquivalentTo(rpdt);
            else if (l is PartialDate lpd && r is PartialDate rpd)
                return PartialDate.IsEquivalentTo(lpd,rpd);
            else if (l is Quantity lq && r is Quantity rq)
                return lq.IsEquivalentTo(rq);
            else
                // the spec says that this should be false - you can compare
                // anything to anything!
                // throw new ArgumentException("Can only compare System primitives string, bool, long, decimal and partial date/dateTime/time.");
                return false;
        }

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
            if (value == null)
            {
                parsed = null;
                return true;
            }

            (bool succ, object output) result;

            if (systemType == TypeSpecifier.Boolean)
            {
                var success = Boolean.TryParse(value, out var p);
                result = (success, p);
            }
            else if (systemType == TypeSpecifier.Code)
            {
                var success = Coding.TryParse(value, out var p);
                result = (success, p);
            }
            else if (systemType == TypeSpecifier.Concept)
            {
                var success = Concept.TryParse(value, out var p);
                result = (success, p);
            }
            else if (systemType == TypeSpecifier.Date)
            {
                var success = PartialDate.TryParse(value, out var p);
                result = (success, p);
            }
            else if (systemType == TypeSpecifier.DateTime)
            {
                var success = PartialDateTime.TryParse(value, out var p);
                result = (success, p);
            }
            else if (systemType == TypeSpecifier.Decimal)
            {
                var success = Decimal.TryParse(value, out var p);
                result = (success, p);
            }
            else if (systemType == TypeSpecifier.Integer)
            {
                var success = Integer.TryParse(value, out var p);
                result = (success, p);
            }
            else if (systemType == TypeSpecifier.Integer64)
            {
                var success = Integer64.TryParse(value, out var p);
                result = (success, p);
            }
            else if (systemType == TypeSpecifier.Quantity)
            {
                var success = Quantity.TryParse(value, out var p);
                result = (success, p);
            }
            else if (systemType == TypeSpecifier.String)
            {
                var success = String.TryParse(value, out var p);
                result = (success, p);
            }
            else if (systemType == TypeSpecifier.Time)
            {
                var success = PartialTime.TryParse(value, out var p);
                result = (success, p);
            }
            else
                result = (false, null);

            parsed = result.output;
            return result.succ;


        }

        internal static (bool, T) DoConvert<T>(Func<T> parser)
        {
            try
            {
                return (true, parser());
            }
            catch (Exception)
            {
                return (false, default);
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
            else if (value is int || value is short || value is ushort || value is uint)
                primitiveValue = Convert.ToInt32(value);
            else if (value is long || value is ulong)
                primitiveValue = Convert.ToInt64(value);
            else if (value is DateTimeOffset dto)
                primitiveValue = PartialDateTime.FromDateTimeOffset(dto);
            else if (value is float || value is double || value is decimal)
                primitiveValue = Convert.ToDecimal(value);
            else if (value is char c)
                primitiveValue = new string(c, 1);
            else if (value is Enum en)
                primitiveValue = en.GetLiteral();
#pragma warning disable IDE0045 // Convert to conditional expression
            else if (value is Uri u)
#pragma warning restore IDE0045 // Convert to conditional expression
                primitiveValue = u.OriginalString;
            else
                primitiveValue = null;

            return primitiveValue != null;
        }
    }
}
