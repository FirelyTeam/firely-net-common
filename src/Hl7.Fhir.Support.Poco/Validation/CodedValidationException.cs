/* 
 * Copyright (c) 2021, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-net-sdk/master/LICENSE
 */

using Hl7.Fhir.Utility;
using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using DAVE = Hl7.Fhir.Validation.CodedValidationException;
#nullable enable

namespace Hl7.Fhir.Validation
{
    /// <summary>
    /// An error found during validation of POCO's using the <see cref="ValidationAttribute"/> validators.
    /// </summary>
    public class CodedValidationException : Exception, ICodedException
    {
        public const string CHOICE_TYPE_NOT_ALLOWED_CODE = "PVAL101";
        public const string INCORRECT_CARDINALITY_MIN_CODE = "PVAL102";
        public const string INCORRECT_CARDINALITY_MAX_CODE = "PVAL103";
        public const string REPEATING_ELEMENT_CANNOT_CONTAIN_NULL_CODE = "PVAL104";
        public const string MANDATORY_ELEMENT_CANNOT_BE_NULL_CODE = "PVAL105";
        public const string CODE_LITERAL_INVALID_CODE = "PVAL106";
        public const string DATE_LITERAL_INVALID_CODE = "PVAL107";
        public const string DATETIME_LITERAL_INVALID_CODE = "PVAL108";
        public const string ID_LITERAL_INVALID_CODE = "PVAL109";
        public const string OID_LITERAL_INVALID_CODE = "PVAL110";
        public const string TIME_LITERAL_INVALID_CODE = "PVAL111";
        public const string URI_LITERAL_INVALID_CODE = "PVAL112";
        public const string UUID_LITERAL_INVALID_CODE = "PVAL113";
        public const string NARRATIVE_XML_IS_MALFORMED_CODE = "PVAL114";
        public const string NARRATIVE_XML_IS_INVALID_CODE = "PVAL115";

        internal static readonly DAVE CHOICE_TYPE_NOT_ALLOWED = new(CHOICE_TYPE_NOT_ALLOWED_CODE, "Value is of type '{0}', which is not an allowed choice.");
        internal static readonly DAVE INCORRECT_CARDINALITY_MIN = new(INCORRECT_CARDINALITY_MIN_CODE, "Element has {0} elements, but minium cardinality is {1}.");
        internal static readonly DAVE INCORRECT_CARDINALITY_MAX = new(INCORRECT_CARDINALITY_MAX_CODE, "Element has {0} elements, but maximum cardinality is {1}.");
        internal static readonly DAVE REPEATING_ELEMENT_CANNOT_CONTAIN_NULL = new(REPEATING_ELEMENT_CANNOT_CONTAIN_NULL_CODE, "Repeating elements cannot contain null values.");
        internal static readonly DAVE MANDATORY_ELEMENT_CANNOT_BE_NULL = new(MANDATORY_ELEMENT_CANNOT_BE_NULL_CODE, "Element with minimum cardinality {0} cannot be null.");
        internal static readonly DAVE CODE_LITERAL_INVALID = new(CODE_LITERAL_INVALID_CODE, "'{0}' is not a correct literal for a code.");
        internal static readonly DAVE DATE_LITERAL_INVALID = new(DATE_LITERAL_INVALID_CODE, "'{0}' is not a correct literal for a date.");
        internal static readonly DAVE DATETIME_LITERAL_INVALID = new(DATETIME_LITERAL_INVALID_CODE, "'{0}' is not a correct literal for a dateTime.");
        internal static readonly DAVE ID_LITERAL_INVALID = new(ID_LITERAL_INVALID_CODE, "'{0}' is not a correct literal for an id.");
        internal static readonly DAVE NARRATIVE_XML_IS_MALFORMED = new(NARRATIVE_XML_IS_MALFORMED_CODE, "Value is not well-formatted Xml: {0}");
        internal static readonly DAVE NARRATIVE_XML_IS_INVALID = new(NARRATIVE_XML_IS_INVALID_CODE, "Value is not well-formed Xml adhering to the FHIR schema for Narrative: {0}");
        internal static readonly DAVE OID_LITERAL_INVALID = new(OID_LITERAL_INVALID_CODE, "'{0}' is not a correct literal for an oid.");
        internal static readonly DAVE TIME_LITERAL_INVALID = new(TIME_LITERAL_INVALID_CODE, "'{0}' is not a correct literal for a time.");
        internal static readonly DAVE URI_LITERAL_INVALID = new(URI_LITERAL_INVALID_CODE, "'{0}' is not a correct literal for an uri.");
        internal static readonly DAVE UUID_LITERAL_INVALID = new(UUID_LITERAL_INVALID_CODE, "'{0}' is not a correct literal for a uuid.");

        /// <summary>
        /// The unique and permanent code for this error.
        /// </summary>
        /// <remarks>Developers can assume that these codes will not change in future versions.</remarks>
        public string ErrorCode { get; private set; }

        public Exception Exception => this;

        public CodedValidationException(string code, string message) : base(message) => ErrorCode = code;

        internal DAVE With(params object?[] parameters)
        {
            var formattedMessage = string.Format(CultureInfo.InvariantCulture, Message, parameters);
            return new(ErrorCode, formattedMessage);
        }

        internal CodedValidationResult AsResult(ValidationContext context) =>
            context.MemberName is string mn
                ? new(this, memberNames: new[] { mn })
                : new(this);

        public ICodedException WithMessage(string message) => new DAVE(ErrorCode, message);
    }
}

#nullable restore
