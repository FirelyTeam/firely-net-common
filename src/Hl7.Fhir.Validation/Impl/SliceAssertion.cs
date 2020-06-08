
using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Validation.Schema;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hl7.Fhir.Validation.Impl
{
    public class SliceAssertion : IAssertion, IGroupValidatable
    {
        public class Slice : IAssertion
        {
            public readonly string Name;
            public readonly IAssertion Condition;
            public readonly IAssertion Assertion;

            public Slice(string name, IAssertion condition, IAssertion assertion)
            {
                Name = name ?? throw new ArgumentNullException(nameof(name));
                Condition = condition ?? throw new ArgumentNullException(nameof(condition));
                Assertion = assertion ?? throw new ArgumentNullException(nameof(assertion));
            }

            public JToken ToJson() =>
                new JObject(
                    new JProperty("name", Name),
                    new JProperty("condition", Condition.ToJson().MakeNestedProp()),
                    new JProperty("assertion", Assertion.ToJson().MakeNestedProp())
                    );
        }

        public readonly bool Ordered;
        public readonly IAssertion Default;
        public readonly Slice[] Slices;

        public SliceAssertion(bool ordered, IAssertion @default, params Slice[] slices) : this(ordered, @default, slices.AsEnumerable())
        {
        }

        public SliceAssertion(bool ordered, params Slice[] slices) : this(ordered, slices.AsEnumerable())
        {
        }

        public SliceAssertion(bool ordered, IEnumerable<Slice> slices)
            : this(ordered, null, slices)
        {
        }

        public SliceAssertion(bool ordered, IAssertion @default, IEnumerable<Slice> slices)
        {
            Ordered = ordered;
            Default = @default ?? ResultAssertion.CreateFailure(
                            new IssueAssertion(Issue.CONTENT_ELEMENT_FAILS_SLICING_RULE, "TODO: location?", "Element does not match any slice and the group is closed."));
            Slices = slices.ToArray() ?? throw new ArgumentNullException(nameof(slices));
        }

        public async Task<Assertions> Validate(IEnumerable<ITypedElement> input, ValidationContext vc)
        {
            var lastMatchingSlice = -1;
            var defaultInUse = false;
            Assertions result = Assertions.Empty;
            var buckets = new Buckets(Slices);

            // Go over the elements in the instance, in order
            foreach (var candidate in input)
            {
                bool hasSucceeded = false;

                // Try to find the child slice that this element matches
                for (var sliceNumber = 0; sliceNumber < Slices.Length; sliceNumber++)
                {
                    var sliceName = Slices[sliceNumber].Name;
                    var sliceResult = await Slices[sliceNumber].Condition.Validate(candidate, vc).ConfigureAwait(false);

                    if (sliceResult.Result.IsSuccessful)
                    {
                        // The instance matched a slice that we have already passed, if order matters, 
                        // this is not allowed
                        if (sliceNumber < lastMatchingSlice && Ordered)
                            result += new IssueAssertion(Issue.CONTENT_ELEMENT_SLICING_OUT_OF_ORDER, "TODO", $"Element matches slice '{sliceName}', but this is out of order for this group, since a previous element already matched slice '{Slices[lastMatchingSlice].Name}'");
                        else
                            lastMatchingSlice = sliceNumber;

                        if (defaultInUse && Ordered)
                        {
                            // We found a match while we already added a non-match to a "open at end" slicegroup, that's not allowed
                            result += new IssueAssertion(Issue.CONTENT_ELEMENT_FAILS_SLICING_RULE, "TODO", $"Element matched slice '{sliceName}', but it appears after a non-match, which is not allowed for an open-at-end group");
                        }

                        hasSucceeded = true;
                        result += sliceResult;

                        // to add to slice
                        buckets.Add(Slices[sliceNumber], candidate);
                        break;
                    }
                }

                // So we found no slice that can take this candidate, let's pass it to the default slice
                if (!hasSucceeded)
                {
                    defaultInUse = true;

                    var assertions = await Default.Validate(candidate, vc).ConfigureAwait(false);
                    var defaultResult = assertions.Result;

                    if (defaultResult.IsSuccessful)
                        result += new Trace("Element was determined to be in the open slice for group");
                    else
                    {
                        // Sorry, won't fly
                        result += defaultResult;
                    }
                }
            }

            return result += await buckets.Validate(vc).ConfigureAwait(false);
        }

        public JToken ToJson()
        {
            var def = Default.ToJson();
            if (def is JProperty) def = new JObject(def);

            return new JProperty("slice", new JObject(
                new JProperty("ordered", Ordered),
                new JProperty("case", new JArray() { Slices.Select(s => s.ToJson()) }),
                new JProperty("default", def)));
        }

        private class Buckets : Dictionary<Slice, IList<ITypedElement>>
        {
            public Buckets(IEnumerable<Slice> slices)
            {
                // initialize the buckets according to the slice definitions
                foreach (var item in slices)
                {
                    this.Add(item, new List<ITypedElement>());
                }
            }

            public void Add(Slice slice, ITypedElement item)
            {
                if (TryGetValue(slice, out var list)) list.Add(item);
            }

            public async Task<Assertions> Validate(ValidationContext vc)
                => await this.Select(slice => slice.Key.Assertion.Validate(slice.Value, vc)).AggregateAsync();
        }
    }

    /*
     * 
     
   "slice-discrimatorless": {
   "ordered": false,
	"case": [
	  {
		"name": "case-1"
		"condition": { "maxValue": 30 },
		"assertion": {
			"$id": "#slicename",
			"ele-1": "hasValue() or (children().count() > id.count())",
			"ext-1": "extension.exists() != value.exists()",
			"max": 1
		}
	  }
	]
}

"slice-value": {
   "ordered": false,
	"case": [
	  {
		"name": "phone"
		"condition": { 
			"fpath": "system" ,
			"fixed": "phone"
		}
		"assertion": {
			"$id": "#slicename",
            "min": 1
		}
	  },
	  {
		"name": "email"
		"condition": { 
			"fpath": "system" ,
			"fixed": "email"
		}
		"assertion": {
			"$id": "#slicename",
            "min": 1
		}
	  },
	  
	]
}
*/


}

