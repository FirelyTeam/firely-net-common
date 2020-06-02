/* 
 * Copyright (c) 2019, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Hl7.Fhir.Validation.Schema
{
    public class Assertions : ReadOnlyCollection<IAssertion>
    {
        public static readonly Assertions Success = new Assertions(ResultAssertion.Success);
        public static readonly Assertions Failure = new Assertions(ResultAssertion.Failure);
        public static readonly Assertions Undecided = new Assertions(ResultAssertion.Undecided);
        public static readonly Assertions Empty = new Assertions();

        public Assertions(params IAssertion[] assertions) : this(assertions.AsEnumerable())
        {
        }

        public Assertions(IEnumerable<IAssertion> assertions) : base(merge(assertions ?? Assertions.Empty).ToList())
        {
        }

        public IEnumerable<Assertions> Collection => new[] { this };

        public static Assertions operator +(Assertions left, Assertions right)
            => new Assertions(left.Union(right));

        public static Assertions operator +(Assertions left, IAssertion right)
                => new Assertions(left.Union(new[] { right }));

        private static IEnumerable<IAssertion> merge(IEnumerable<IAssertion> assertions)
        {
            var mergeable = assertions.OfType<IMergeable>();
            var nonMergeable = assertions.Where(a => !(a is IMergeable));

            var merged =
                from sa in mergeable
                group sa by sa.GetType() into grp
                select (IAssertion)grp.Aggregate((sum, other) => sum.Merge(other));

            return nonMergeable.Union(merged);
        }

        public ResultAssertion Result => this.OfType<ResultAssertion>().Single();
    }


}
