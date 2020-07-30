/* 
 * Copyright (c) 2019, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */


using Hl7.Fhir.ElementModel;
using Hl7.Fhir.ElementModel.Types;
using Hl7.Fhir.Serialization;
using Hl7.Fhir.Utility;
using Hl7.Fhir.Validation.Schema;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

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
        private Any _minMaxAnyValue;
        private readonly int _comparisonOutcome;
        private readonly string _comparisonLabel;
        private readonly Issue _comparisonIssue;

        public MinMaxValue(ITypedElement minMaxValue, MinMax minMaxType)
        {
            _minMaxValue = minMaxValue ?? throw new ArgumentNullException($"{nameof(minMaxValue)} cannot be null");
            _minMaxType = minMaxType;

            _minMaxAnyValue = Any.Convert(_minMaxValue.Value);
            _comparisonOutcome = _minMaxType == MinMax.MinValue ? -1 : 1;
            _comparisonLabel = _comparisonOutcome == -1 ? "smaller than" :
                                    _comparisonOutcome == 0 ? "equal to" :
                                        "larger than";
            _comparisonIssue = _comparisonOutcome == -1 ? Issue.CONTENT_ELEMENT_PRIMITIVE_VALUE_TOO_SMALL :
                                           Issue.CONTENT_ELEMENT_PRIMITIVE_VALUE_TOO_LARGE;

            _key = $"{_minMaxType.GetLiteral().Uncapitalize()}[x]";

            // Min/max are only defined for ordered types
            if (!IsOrderedType(_minMaxValue.Value))
            {
                throw new IncorrectElementDefinitionException($"{_minMaxValue.Name} was given in ElementDefinition, but type '{_minMaxValue.InstanceType}' is not an ordered type");
            }
        }

        public MinMaxValue(int minMaxValue, MinMax minMaxType) : this(ElementNode.ForPrimitive(minMaxValue), minMaxType) { }

        public override string Key => _key;

        public override object Value => _minMaxValue;

        public override Task<Assertions> Validate(ITypedElement input, ValidationContext vc)
        {
            if (!Any.TryConvert(input.Value, out var instanceValue))
            {
                return Task.FromResult(Assertions.Empty + ResultAssertion.CreateFailure(new IssueAssertion(Issue.CONTENT_ELEMENT_PRIMITIVE_VALUE_NOT_COMPARABLE, input.Location, $"Value '{input.Value}' cannot be compared with {_minMaxValue.Value})")));
            }

            try
            {
                if ((instanceValue is ICqlOrderable ce ? ce.CompareTo(_minMaxAnyValue) : -1) == _comparisonOutcome)
                {
                    return Task.FromResult(Assertions.Empty + ResultAssertion.CreateFailure(new IssueAssertion(_comparisonIssue, input.Location, $"Value '{input.Value}' is {_comparisonLabel} {_minMaxValue.Value})")));
                }
            }
            catch (ArgumentException)
            {
                return Task.FromResult(Assertions.Empty + ResultAssertion.CreateFailure(new IssueAssertion(Issue.CONTENT_ELEMENT_PRIMITIVE_VALUE_NOT_COMPARABLE, input.Location, $"Value '{input.Value}' cannot be compared with {_minMaxValue.Value})")));
            }

            return Task.FromResult(Assertions.Success);
        }

        public override JToken ToJson()
        {
            return new JProperty(Key, _minMaxValue.ToJObject());
        }

        /// <summary>
        /// TODO Validation: this should be altered and moved to a more generic place, and should be more sophisticated
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private bool IsOrderedType(object value)
        {
            return Any.TryConvert(value, out _minMaxAnyValue) && _minMaxAnyValue is ICqlOrderable;
        }
    }
}
