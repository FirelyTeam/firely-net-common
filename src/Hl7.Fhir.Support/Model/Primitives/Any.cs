/* 
 * Copyright (c) 2019, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

#nullable enable

using Hl7.Fhir.Language;
using Hl7.Fhir.Utility;
using System;
using System.Reflection;

namespace Hl7.Fhir.Model.Primitives
{
    public abstract class Any
    {
        // move this code into fhirpath engine
        //public static bool? IsEqualTo(object l, object r)
        //{
        //    if (l == null || r == null) return null;

        //    // this really should be handled by casts outside this func (and in the engine?)
        //    l = UpcastOperand(l, r);
        //    r = UpcastOperand(r, l);

        //    if (l is string lstr && r is string rstr)
        //        return String.IsEqualTo(lstr,rstr);
        //    else if (l is bool lbl && r is bool rbl)
        //        return Boolean.IsEqualTo(lbl, rbl);
        //    else if (l is int lint && r is int rint)
        //        return Integer.IsEqualTo(lint, rint);
        //    else if (l is long llng && r is long rlng)
        //        return Integer64.IsEqualTo(llng, rlng);
        //    else if (l is decimal ldec && r is decimal rdec)
        //        return Decimal.IsEqualTo(ldec,rdec);
        //    else if (l is PartialTime lpt && r is PartialTime rpt)
        //        return PartialTime.IsEqualTo(lpt,rpt);
        //    else if (l is PartialDateTime lpdt && r is PartialDateTime rpdt)
        //        return PartialDateTime.AreEqual(lpdt,rpdt);
        //    else if (l is PartialDate lpd && r is PartialDate rpd)
        //        return PartialDate.IsEqualTo(lpd,rpd);
        //    else if (l is Quantity lq && r is Quantity rq)
        //        return lq.IsEqualTo(rq);
        //    else
        //        // the spec says that this should be false - you can compare
        //        // anything to anything!
        //        //throw new ArgumentException("Can only compare System primitives string, bool, long, decimal and partial date/dateTime/time.");
        //        return false;
        //}

        //internal static object UpcastOperand(object value, object other)
        //{
        //    if (value is int && other is long) return (long)value;
        //    if (value is int && other is decimal) return (decimal)value;
        //    if (value is long && other is decimal) return (decimal)value;

        //    // nothing to upcast, return value;
        //    return value;
        //}

        // public static bool IsEquivalentTo(object l, object r)
        //{
        //    if (l == null && r == null) return true;
        //    if (l == null || r == null) return false;

        //    // this really should be handled by casts outside this func (and in the engine?)
        //    l = UpcastOperand(l, r);
        //    r = UpcastOperand(r, l);

        //    if (l is string lstr && r is string rstr)
        //        return String.IsEquivalentTo(lstr,rstr);
        //    else if (l is bool lbl && r is bool rbl)
        //        return Boolean.IsEquivalentTo(lbl, rbl);
        //    else if (l is int lint && r is int rint)
        //        return Integer.IsEquivalentTo(lint,rint);
        //    else if (l is long llng && r is long rlng)
        //        return Integer64.IsEquivalentTo(llng, rlng);
        //    else if (l is decimal ldec && r is decimal rdec)
        //        return Decimal.IsEquivalentTo(ldec,rdec);
        //    else if (l is PartialTime lpt && r is PartialTime rpt)
        //        return  PartialTime.IsEquivalentTo(lpt,rpt);
        //    else if (l is PartialDateTime lpdt && r is PartialDateTime rpdt)
        //        return PartialDateTime.AreEquivalent(lpdt,rpdt);
        //    else if (l is PartialDate lpd && r is PartialDate rpd)
        //        return PartialDate.IsEquivalentTo(lpd,rpd);
        //    else if (l is Quantity lq && r is Quantity rq)
        //        return lq.IsEquivalentTo(rq);
        //    else
        //        // the spec says that this should be false - you can compare
        //        // anything to anything!
        //        // throw new ArgumentException("Can only compare System primitives string, bool, long, decimal and partial date/dateTime/time.");
        //        return false;
        //}


        public static bool TryGetByName(string name, out Type? result)
        {
            result = get();
            return result != null;

            Type? get() =>
                name switch
                {
                    "Any" => typeof(Any),
                    "Boolean" => typeof(Boolean),
                    "Code" => typeof(Coding),
                    "Concept" => typeof(Concept),
                    "Decimal" => typeof(Decimal),
                    "Integer" => typeof(Integer),
                    "Integer64" => typeof(Integer64),
                    "Date" => typeof(PartialDate),
                    "DateTime" => typeof(PartialDateTime),
                    "Time" => typeof(PartialTime),
                    "Quantity" => typeof(Quantity),
                    "String" => typeof(String),
                    "Void" => typeof(void),
                    _ => null,
                };
        }

        public static object Parse(string value, Type primitiveType)
        {
            if (value is null) throw new ArgumentNullException(nameof(value));

            return TryParse(value, primitiveType, out var result) ? result! : 
                throw new FormatException($"Input string '{value}' was not in a correct format for type '{primitiveType}'.");
        }

        public static bool TryParse(string value, Type primitiveType, out object? parsed)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (!typeof(Any).IsAssignableFrom(primitiveType)) throw new ArgumentException($"Must be a subclass of {nameof(Any)}.", nameof(primitiveType));

            bool success;
            (success, parsed) = parse();
            return success;

            (bool success, object? p) parse()
            {
                if(primitiveType == typeof(Boolean) )
                    return (Boolean.TryParse(value, out var p), p);
                else if (primitiveType == typeof(Coding))
                    return (Coding.TryParse(value, out var p), p);
                else if (primitiveType == typeof(Concept))
                    return (success: Concept.TryParse(value, out var p), p);
                else if (primitiveType == typeof(Decimal))
                    return (success: Decimal.TryParse(value, out var p), p);
                else if (primitiveType == typeof(Integer))
                    return (success: Integer.TryParse(value, out var p), p);
                else if (primitiveType == typeof(Integer64))
                    return (success: Integer64.TryParse(value, out var p), p);
                else if (primitiveType == typeof(PartialDate))
                    return (success: PartialDate.TryParse(value, out var p), p);
                else if (primitiveType == typeof(PartialDateTime))
                    return (success: PartialDateTime.TryParse(value, out var p), p);
                else if (primitiveType == typeof(PartialTime))
                    return (success: PartialTime.TryParse(value, out var p), p);
                else if (primitiveType == typeof(Quantity))
                    return (success: Quantity.TryParse(value, out var p), p);
                else if (primitiveType == typeof(String))
                    return (success: String.TryParse(value, out var p), p);
                else
                    return (false, null);
            }
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
        public static object? ConvertToSystemValue(object value)
        {
            if (value == null) return null;

            if (TryConvertToSystemValue(value, out object? result))
                return result!;
            else
                throw new NotSupportedException($"There is no known System type corresponding to the .NET type {value.GetType().Name} of this instance (with value '{value}').");
        }

        /// <summary>
        /// Try to converts a .NET instance to a System-based instance.
        /// </summary>
        public static bool TryConvertToSystemValue(object value, out object? primitiveValue)
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
