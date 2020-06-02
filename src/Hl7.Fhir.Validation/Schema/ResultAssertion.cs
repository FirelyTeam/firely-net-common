/* 
 * Copyright (c) 2019, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Utility;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hl7.Fhir.Validation.Schema
{
    public enum ValidationResult
    {
        Success,
        Failure,
        Undecided
    }

    public class ResultAssertion : IAssertion, IMergeable, IValidatable
    {
        public static readonly ResultAssertion Success = new ResultAssertion(ValidationResult.Success);
        public static readonly ResultAssertion Failure = new ResultAssertion(ValidationResult.Failure);
        public static readonly ResultAssertion Undecided = new ResultAssertion(ValidationResult.Undecided);

        public readonly ValidationResult Result;
        public readonly IAssertion[] Evidence;

        public static ResultAssertion CreateFailure(params IAssertion[] evidence) => new ResultAssertion(ValidationResult.Failure, evidence);

        public ResultAssertion(ValidationResult result, params IAssertion[] evidence) : this(result, evidence.AsEnumerable())
        {
        }

        public ResultAssertion(ValidationResult result, IEnumerable<IAssertion> evidence)
        {
            Evidence = evidence?.ToArray() ?? throw new ArgumentNullException(nameof(evidence));
            Result = result;
            Evidence1 = evidence;
        }

        public bool IsSuccessful => Result == ValidationResult.Success;

        public IEnumerable<IAssertion> Evidence1 { get; }

        public IMergeable Merge(IMergeable other)
        {
            if (other is ResultAssertion ra)
            {
                // If we currently are succesful, the new result fully depends on the other
                // Otherwise, we are failing or undecided, which we need to
                // propagate
                // if other is not succesful as well, then combine the evidence as a result
                return new ResultAssertion(IsSuccessful ? ra.Result : Result, Evidence.Concat(ra.Evidence));
            }
            else
                throw Error.InvalidOperation($"Internal logic failed: tried to merge a ResultAssertion with an {other.GetType().Name}");
        }

        public JToken ToJson()
        {
            var evidence = new JArray(Evidence.Select(e => e.ToJson().MakeNestedProp()));
            return new JProperty("raise", new JObject(
                new JProperty("result", Result.ToString()),
                new JProperty("evidence", evidence)));
        }

        public async Task<Assertions> Validate(ITypedElement input, ValidationContext vc)
        {
            var result = new Assertions(this);
            foreach (var item in Evidence)
            {
                result += await item.Validate(input, vc).ConfigureAwait(false);
            }
            return result;
        }


    }
}
