/* 
 * Copyright (c) 2019, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Serialization;
using Hl7.Fhir.Utility;
using Hl7.Fhir.Validation.Schema;
using Newtonsoft.Json.Linq;
using System;

namespace Hl7.Fhir.Validation.Impl
{
    public enum MinMax
    {
        [EnumLiteral("MinValue"), Description("Minimum Value")]
        MinValue,
        [EnumLiteral("MaxValue"), Description("Maximum Value")]
        MaxValue
    }

    public class MinMaxValue : SimpleAssertion
    {
        private readonly ITypedElement _minMaxValue;
        private readonly MinMax _minMaxType;
        private readonly string _key;

        public MinMaxValue(string location, ITypedElement minMaxValue, MinMax minMaxType) : base(location)
        {
            _minMaxValue = minMaxValue ?? throw new IncorrectElementDefinitionException($"{nameof(minMaxValue)} cannot be null");
            _minMaxType = minMaxType;

            _key = $"{_minMaxType.GetLiteral().Uncapitalize()}[x]";

            // Min/max are only defined for ordered types
            if (!IsOrderedType(_minMaxValue.InstanceType))
            {
                throw new IncorrectElementDefinitionException($"{_minMaxValue.Name} was given in ElementDefinition, but type '{_minMaxValue.InstanceType}' is not an ordered type");
            }
        }

        public MinMaxValue(string location, int minMaxValue, MinMax minMaxType) : this(location, ElementNode.ForPrimitive(minMaxValue), minMaxType) { }

        public override string Key => _key;

        public override object Value => _minMaxValue;

        public override Assertions Validate(ITypedElement input, ValidationContext vc)
        {
            var comparisonOutcome = _minMaxType == MinMax.MinValue ? -1 : 1;

            // TODO : what to do if Value is not IComparable?
            if (input.Value is IComparable instanceValue)
            {
                if (instanceValue.CompareTo(_minMaxValue.Value) == comparisonOutcome)
                {
                    var label = comparisonOutcome == -1 ? "smaller than" :
                                    comparisonOutcome == 0 ? "equal to" :
                                        "larger than";

                    return Assertions.Failure + new Trace($"Value '{instanceValue}' is {label} {_minMaxValue.Value})");
                }
            }

            return Assertions.Success;
        }

        public override JToken ToJson()
        {
            return new JProperty(Key, _minMaxValue.ToJObject());
        }

        /// <summary>
        /// TODO Validation: this should be altered and moved to a more generic place, and should be more sophisticated
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private bool IsOrderedType(string type)
        {
            switch (type)
            {
                case "date":
                case "dateTime":
                case "instant":
                case "time":
                case "decimal":
                case "integer":
                case "positiveInt":
                case "unsignedInt":
                case "Quantity": return true;
                default:
                    return false;
            }
        }
    }
}
