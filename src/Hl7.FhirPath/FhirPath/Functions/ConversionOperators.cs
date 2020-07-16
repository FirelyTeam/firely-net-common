/* 
 * Copyright (c) 2015, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/ewoutkramer/fhir-net-api/master/LICENSE
 */

using Hl7.Fhir.ElementModel;
using P = Hl7.Fhir.ElementModel.Types;
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
                int i => i == 1
                        ? true :
                            i == 0 ? false : (bool?)null,
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
        public static int? ToInteger(this ITypedElement focus)
        {
            var val = focus?.Value;
            if (val == null) return null;

            return val switch
            {
                int i => i,
                string s => convertString(s),
                bool b => b ? 1 : 0,
                _ => null,
            };

            static int? convertString(string si)
            {
                try
                {
                    return XmlConvert.ToInt32(si);
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
        public static P.PartialDateTime ToDateTime(this ITypedElement focus)
        {
            var val = focus?.Value;
            if (val == null) return null;

            return val switch
            {
                P.PartialDateTime pdt => pdt,
                string s => convertString(s),
                _ => null,
            };

            static P.PartialDateTime convertString(string si) =>
                   P.PartialDateTime.TryParse(si, out var result) ?
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
        public static P.PartialTime ToTime(this ITypedElement focus)
        {
            var val = focus?.Value;
            if (val == null) return null;

            return val switch
            {
                P.PartialTime pt => pt,
                string s => convertString(s),
                _ => null,
            };

            static P.PartialTime convertString(string si) => P.PartialTime.TryParse(si, out var result) ? result : null;
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
        public static P.PartialDate ToDate(this ITypedElement focus)
        {
            var val = focus?.Value;
            if (val == null) return null;

            return val switch
            {
                P.PartialDate pt => pt,
                string s => convertString(s),
                _ => null,
            };

            static P.PartialDate convertString(string si) =>
                P.PartialDate.TryParse(si, out var result) ?
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
        public static P.Quantity ToQuantity(this ITypedElement focus)
        {
            var val = focus?.Value;
            if (val == null) return null;

            return val switch
            {
                P.Quantity q => q,
                int i => new P.Quantity((decimal)i),
                long l => new P.Quantity((decimal)l),
                decimal d => new P.Quantity(d),
                string s => convertString(s),
                bool b => b == true ? new P.Quantity(1.0) : new P.Quantity(0.0),
                _ => null,
            };

            static P.Quantity convertString(string si) => P.Quantity.TryParse(si, out var result) ?result : null;
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
                int i => XmlConvert.ToString(i),
                long l => XmlConvert.ToString(l),
                decimal d => XmlConvert.ToString(d),
                P.PartialDate pd => pd.ToString(),
                P.PartialDateTime pdt => pdt.ToString(),
                P.PartialTime pt => pt.ToString(),// again, this inconsistency.
                bool b => b ? "true" : "false",
                P.Quantity q => q.ToString(),
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
