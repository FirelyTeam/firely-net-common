/* 
 * Copyright (c) 2015, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

using Hl7.Fhir.Model.Primitives;
using Hl7.FhirPath.Sprache;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace Hl7.FhirPath.Parser
{
    internal partial class Lexer
    {
        // IDENTIFIER
        //   : ([A-Za-z] | '_')([A-Za-z0-9] | '_')*            // Added _ to support CQL (FHIR could constrain it out)
        //   ;
        public static readonly Parser<string> Id =
            Parse.Identifier(
                Parse.Letter
                    .XOr(Parse.Char('_')),
                Parse.LetterOrDigit
                    .XOr(Parse.Char('_')))
            .Named("Identifier");

        // fragment UNICODE: 'u' HEX HEX HEX HEX;
        // fragment HEX: [0-9a-fA-F];
        public static readonly Parser<string> Unicode =
            from u in Parse.Char('u')
            from hex in Parse.Chars("0123456789ABCDEFabcdef").Repeat(4).Text()
            select hex;

        // fragment ESC
        //   : '\\' (["'`\\/fnrt] | UNICODE)    // allow \`, \", \', \\, \/, \f, etc. and \uXXX
        //   ;
        // EK: We allow \" here as well, to support the older " escaped identifiers
        public static readonly Parser<string> Escape =
            from backslash in Parse.Char('\\')
            from escUnicode in
                Parse.Chars("\"'`\\/fnrt").Once().Unescape().Or(Unicode.Unescape())
            select escUnicode;

        // This represents the fragment <delimiter> (ESC | .)*? <delimiter>
        // as used by DELIMITEDIDENTIFIER and STRING lexers
        internal static Parser<string> DelimitedContents(char delimiter) =>
             from openQ in Parse.Char(delimiter)
             from id in Parse.CharExcept(new[] { delimiter, '\\'}).Many().Text().Or(Escape).Many()
             from closeQ in Parse.Char(delimiter)
             select string.Concat(id);

        //STRING
        //    : '\'' (ESC | .)*? '\''
        //    ;
        public static readonly Parser<string> String = DelimitedContents('\'');

        //DELIMITEDIDENTIFIER
        //   : '"' (ESC | .)+? '"'
        //   | '`' (ESC | .)+? '`'
        //   ;
        public static readonly Parser<string> DelimitedIdentifier =
            DelimitedContents('"')
            .XOr(DelimitedContents('`'));

        // identifier
        //  : IDENTIFIER
        //  | QUOTEDIDENTIFIER
        //  ;
        public static readonly Parser<string> Identifier =
            Id.XOr(DelimitedIdentifier);

        // externalConstant
        //  : '%' identifier
        //  ;
        public static readonly Parser<string> ExternalConstant =
            Parse.Char('%').Then(c => Identifier.XOr(String))
            .Named("external constant");

        // DATE
        //      : '@'  ....
        // Note: I split the lexer rule for DATETIME from the FP spec into separate DATETIME and DATE rules
        public static readonly Regex DateRegEx = new Regex(
                @"@[0-9]{4}     # Year
                (
                    -([0-9][0-9])   # Month
                    (
                        -([0-9][0-9])    # Day
                    )?
                )?", RegexOptions.IgnorePatternWhitespace);

        public static readonly Parser<PartialDate> Date =
            Parse.Regex(DateRegEx).Select(s => PartialDate.Parse(s.Substring(1)));

        // DATETIME
        //      : '@'  ....
        // Note: I split the lexer rule for DATETIME from the FP spec into separate DATETIME and DATE rules
        public static readonly Regex DateTimeRegEx = new Regex(
                @"@[0-9]{4}     # Year
                (
                    ( 
                        -([0-9][0-9])   # Month
                        (
                            (  
                                -([0-9][0-9]) T  #Day
                                (                    
                                  " + TIMEFORMAT + "  #Time  " + @"
                                ) ?
                            )
                            | T
                        )
                    ) 
                    | T
                ) (Z|((\+|-)[0-9][0-9]:[0-9][0-9]))?  #Timezone
                ", RegexOptions.IgnorePatternWhitespace);

        public static readonly Parser<PartialDateTime> DateTime =
            Parse.Regex(DateTimeRegEx).Select(s => PartialDateTime.Parse(s.Substring(1)));

        // fragment TIMEFORMAT
        //      : [0-9] [0-9] (':'[0-9] [0-9] (':'[0-9] [0-9] ('.'[0-9]+)?)?)?
        //      ;
        private const string TIMEFORMAT = @"[0-9][0-9](:[0-9][0-9](:[0-9][0-9](\.[0-9]+)?)?)?";
        
        // TIME
        //      : '@T'  ....
        // NB: No timezone (as specified in FHIR and FhirPath, but CQL *does* allow a timezone
        public static readonly Regex TimeRegEx = new Regex("@T" + TIMEFORMAT, RegexOptions.IgnorePatternWhitespace);

        public static readonly Parser<PartialTime> Time =
            Parse.Regex(TimeRegEx).Select(s => PartialTime.Parse(s.Substring(2)));

        // NUMBER
        //   : [0-9]+('.' [0-9]+)?
        //   ;
        public static readonly Parser<Int64> IntegerNumber =
            Parse.Number.Select(s => Int64.Parse(s)).Named("IntegerNumber");

        public static readonly Parser<decimal> DecimalNumber =
                   from num in Parse.Number
                   from dot in Parse.Char('.')
                   from fraction in Parse.Number
                   select XmlConvert.ToDecimal(num + dot + fraction);

        // BOOL: 'true' | 'false';
        public static readonly Parser<bool> Bool =
            Parse.String("true").XOr(Parse.String("false")).Text().Select(s => Boolean.Parse(s));

        //qualifiedIdentifier
        //   : identifier ('.' identifier)*
        //   ;
        public static readonly Parser<string> QualifiedIdentifier =
            Parse.ChainOperator(Parse.Char('.'), Identifier, (op, a, b) => a + "." + b);

        public static readonly Parser<string> Axis =
            from _ in Parse.Char('$')
            from name in Parse.String("this").XOr(Parse.String("index")).Or(Parse.String("total")).Text()
            select name;
    }


    public static class ParserExtensions
    {
        public static Parser<string> Unescape(this Parser<IEnumerable<char>> c)
        {
            return c.Select(chr => new String(Unescape(chr.Single()), 1));
        }

        public static char Unescape(char c)
        {
            // return the escaped character after a '\'
            switch (c)
            {
                case 'f': return '\f';
                case 'n': return '\n';
                case 'r': return '\r';
                case 't': return '\t';
                default: return c;
            }
        }

        public static Parser<string> Unescape(this Parser<string> c)
        {
            return c.Select(s => new String(Unescape(s), 1));
        }

        public static char Unescape(string unicodeHex)
        {
            return ((char)int.Parse(unicodeHex, NumberStyles.HexNumber));
        }

    }
}
