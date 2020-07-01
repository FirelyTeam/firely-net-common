/* 
 * Copyright (c) 2015, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/ewoutkramer/fhir-net-api/master/LICENSE
 */

using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Model.Primitives;
using System.Xml;

namespace Hl7.FhirPath.Functions
{
    internal static class ConversionOperators
    {
        /// <summary>
        /// FhirPath toBoolean() function
        /// </summary>
        /// <param name="focus"></param>
        /// <returns></returns>
        public static bool? ToBoolean(this ITypedElement focus)
        {
            var val = focus?.Value;
            if (val == null) return null;

            return val switch
            {
                bool b => b,
                string s => convertString(s),
                long l => l == 1 
                        ? true :
                            l == 0 ? false : (bool?)null,
                decimal d => d == 1.0m 
                        ? true :
                            d == 0.0m ? false : (bool?)null,
                _ => null,
            };

            static bool? convertString(string si)
            {
                switch (si.ToLower())
                {
                    case "true":
                    case "t":
                    case "yes":
                    case "y":
                    case "1":
                    case "1.0":
                        return true;
                    case "false":
                    case "f":
                    case "no":
                    case "n":
                    case "0":
                    case "0.0":
                        return false;
                    default:
                        return null;
                }
            }
        }

        /// <summary>
        /// FhirPath convertsToBoolean() function
        /// </summary>
        /// <param name="focus"></param>
        /// <returns></returns>
        public static bool ConvertsToBoolean(this ITypedElement focus) => ToBoolean(focus) != null;


        /// <summary>
        /// FhirPath toInteger() function
        /// </summary>
        /// <param name="focus"></param>
        /// <returns></returns>
        public static long? ToInteger(this ITypedElement focus)
        {
            var val = focus?.Value;
            if (val == null) return null;

            return val switch
            {
                long l => l,
                string s => convertString(s),
                bool b => b ? 1L : 0L,
                _ => null,
            };

            static long? convertString(string si)
            {
                try
                {
                    return XmlConvert.ToInt64(si);
                }
                catch
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// FhirPath convertsToInteger() function.
        /// </summary>
        /// <param name="focus"></param>
        /// <returns></returns>
        public static bool ConvertsToInteger(this ITypedElement focus) => ToInteger(focus) != null;


        /// <summary>
        /// FhirPath toDecimal() function.
        /// </summary>
        /// <param name="focus"></param>
        /// <returns></returns>
        public static decimal? ToDecimal(this ITypedElement focus)
        {
            var val = focus?.Value;
            if (val == null) return null;

            return val switch
            {
                decimal d => d,
                long l => l,
                int i => i,
                string s => convertString(s),
                bool b => b ? 1m : 0m,
                _ => null,
            };

            static decimal? convertString(string si)
            {
                try
                {
                    return XmlConvert.ToDecimal(si);
                }
                catch
                {
                    return null;
                }
            }
        }


        /// <summary>
        /// FhirPath convertsToDecimal() function.
        /// </summary>
        /// <param name="focus"></param>
        /// <returns></returns>
        public static bool ConvertsToDecimal(this ITypedElement focus) => ToDecimal(focus) != null;


        /// <summary>
        /// FhirPath toDateTime() function.
        /// </summary>
        /// <param name="focus"></param>
        /// <returns></returns>
        public static PartialDateTime ToDateTime(this ITypedElement focus)
        {
            var val = focus?.Value;
            if (val == null) return null;

            return val switch
            {
                PartialDateTime pdt => pdt,
                string s => convertString(s),
                _ => null,
            };

            static PartialDateTime convertString(string si) =>
                   PartialDateTime.TryParse(si, out var result) ?
                        result : default;
        }


        /// <summary>
        /// FhirPath convertsToDateTime() function.
        /// </summary>
        /// <param name="focus"></param>
        /// <returns></returns>
        public static bool ConvertsToDateTime(this ITypedElement focus) => ToDateTime(focus) != null;


        /// <summary>
        /// FhirPath toTime() function.
        /// </summary>
        /// <param name="focus"></param>
        /// <returns></returns>
        public static PartialTime ToTime(this ITypedElement focus)
        {
            var val = focus?.Value;
            if (val == null) return null;

            return val switch
            {
                PartialTime pt => pt,
                string s => convertString(s),
                _ => null,
            };

            static PartialTime convertString(string si) => PartialTime.TryParse(si, out var result) ? result : null;
        }


        /// <summary>
        /// FhirPath convertsToTime() function.
        /// </summary>
        /// <param name="focus"></param>
        /// <returns></returns>
        public static bool ConvertsToTime(this ITypedElement focus) => ToTime(focus) != null;


        /// <summary>
        /// FhirPath toDate() function.
        /// </summary>
        /// <param name="focus"></param>
        /// <returns></returns>
        public static PartialDate ToDate(this ITypedElement focus)
        {
            var val = focus?.Value;
            if (val == null) return null;

            return val switch
            {
                PartialDate pt => pt,
                string s => convertString(s),
                _ => null,
            };

            static PartialDate convertString(string si) =>
                PartialDate.TryParse(si, out var result) ?
                     result : null;
        }


        /// <summary>
        /// FhirPath convertsToDate() function.
        /// </summary>
        /// <param name="focus"></param>
        /// <returns></returns>
        public static bool ConvertsToDate(this ITypedElement focus) => ToDate(focus) != null;


        /// <summary>
        /// FhirPath toQuantity() function.
        /// </summary>
        /// <param name="focus"></param>
        /// <returns></returns>
        public static Quantity ToQuantity(this ITypedElement focus)
        {
            var val = focus?.Value;
            if (val == null) return null;

            return val switch
            {
                Quantity q => q,
                long l => new Quantity((decimal)l),
                decimal d => new Quantity(d),
                string s => convertString(s),
                bool b => b == true ? new Quantity(1.0) : new Quantity(0.0),
                _ => null,
            };

            static Quantity convertString(string si) => Quantity.TryParse(si, out var result) ?result : null;
        }

        /// <summary>
        /// FhirPath convertsToQuantity() function.
        /// </summary>
        /// <param name="focus"></param>
        /// <returns></returns>
        public static bool ConvertsToQuantity(this ITypedElement focus) => ToQuantity(focus) != null;


        /// <summary>
        /// FhirPath toString() function.
        /// </summary>
        /// <param name="focus"></param>
        /// <returns></returns>
        public static string ToStringRepresentation(this ITypedElement focus)
        {
            var val = focus?.Value;
            if (val == null) return null;

            return val switch
            {
                string s => s,
                long l => XmlConvert.ToString(l),
                decimal d => XmlConvert.ToString(d),
                PartialDate pd => pd.ToString(),
                PartialDateTime pdt => pdt.ToString(),
                PartialTime pt => pt.ToString(),// again, this inconsistency.
                bool b => b ? "true" : "false",
                Quantity q => q.ToString(),
                _ => null,
            };
        }

        /// <summary>
        /// FhirPath convertsToString() function.
        /// </summary>
        /// <param name="focus"></param>
        /// <returns></returns>
        public static bool ConvertsToString(this ITypedElement focus) => ToStringRepresentation(focus) != null;
    }
}
