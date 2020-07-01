/*
  Copyright (c) 2011+, HL7, Inc.
  All rights reserved.
  
  Redistribution and use in source and binary forms, with or without modification, 
  are permitted provided that the following conditions are met:
  
   * Redistributions of source code must retain the above copyright notice, this 
     list of conditions and the following disclaimer.
   * Redistributions in binary form must reproduce the above copyright notice, 
     this list of conditions and the following disclaimer in the documentation 
     and/or other materials provided with the distribution.
   * Neither the name of HL7 nor the names of its contributors may be used to 
     endorse or promote products derived from this software without specific 
     prior written permission.
  
  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
  ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
  WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
  IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
  INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT 
  NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR 
  PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
  WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
  ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE 
  POSSIBILITY OF SUCH DAMAGE.
  

*/

using Hl7.Fhir.Introspection;
using Hl7.Fhir.Validation;
using System.Runtime.Serialization;
using Hl7.Fhir.Specification;
using System;
using System.Text.RegularExpressions;
using Hl7.Fhir.Serialization;

namespace Hl7.Fhir.Model
{
    /// <summary>
    /// Primitive Type date
    /// </summary>
#if !NETSTANDARD1_1
    [Serializable]
#endif
    [System.Diagnostics.DebuggerDisplay(@"\{Value={Value}}")]
    [FhirType("date")]
    [DataContract]
    public partial class Date : PrimitiveType, IStringValue
    {
        public override string TypeName { get { return "date"; } }
        
        // Must conform to the pattern "([0-9]([0-9]([0-9][1-9]|[1-9]0)|[1-9]00)|[1-9]000)(-(0[1-9]|1[0-2])(-(0[1-9]|[1-2][0-9]|3[0-1]))?)?"
        public const string PATTERN = @"([0-9]([0-9]([0-9][1-9]|[1-9]0)|[1-9]00)|[1-9]000)(-(0[1-9]|1[0-2])(-(0[1-9]|[1-2][0-9]|3[0-1]))?)?";

		public Date(string value)
		{
			Value = value;
		}

		public Date(): this(null) {}

        public Date(int year, int month, int day)
            : this(String.Format(Model.FhirDateTime.FMT_YEARMONTHDAY, year, month, day))
        {
        }

        public Date(int year, int month)
            : this(String.Format(Model.FhirDateTime.FMT_YEARMONTH, year, month))
        {
        }

        public Date(int year) : this(String.Format(Model.FhirDateTime.FMT_YEAR, year))
        {
        }

        /// <summary>
        /// Primitive value of the element
        /// </summary>
        [FhirElement("value", IsPrimitiveValue = true, XmlSerialization = XmlRepresentation.XmlAttr, InSummary = true, Order = 30)]
        [DatePattern]
        [DataMember]
        public string Value
        {
            get { return (string)ObjectValue; }
            set { ObjectValue = value; OnPropertyChanged("Value"); }
        }

        public static bool IsValidValue(string value) => Regex.IsMatch(value, "^" + PATTERN + "$", RegexOptions.Singleline);

        /// <summary>
        /// Gets the current date in the local timezone
        /// </summary>
        /// <returns>Gets the current date in the local timezone</returns>
        public static Date Today() => new Date(DateTimeOffset.Now.ToString("yyyy-MM-dd"));

        /// <summary>
        /// Gets the current date in the timezone UTC
        /// </summary>
        /// <returns>Gets the current date in the timezone UTC</returns>
        public static Date UtcToday() => new Date(DateTimeOffset.UtcNow.ToString("yyyy-MM-dd"));

        [Obsolete("Use ToDateTimeOffset instead")]
        public DateTime? ToDateTime() => 
            Value == null ? null : (DateTime?)PrimitiveTypeConverter.ConvertTo<DateTimeOffset>(Value).DateTime;

        public DateTimeOffset? ToDateTimeOffset() =>
            Value == null ? null : (DateTimeOffset?)PrimitiveTypeConverter.ConvertTo<DateTimeOffset>(Value);

        public Primitives.PartialDate ToPartialDate() =>
            Value != null ? Primitives.PartialDate.Parse(Value) : null;

        public Primitives.PartialDateTime ToPartialDateTime() =>
            Value != null ? Primitives.PartialDateTime.Parse(Value) : null;
    }

}
