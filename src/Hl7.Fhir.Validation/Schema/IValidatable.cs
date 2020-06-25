/* 
 * Copyright (c) 2019, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

using Hl7.Fhir.ElementModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hl7.Fhir.Validation.Schema
{
    /// <summary>
    /// Implemented by assertions that work on a single ITypedElement.
    /// </summary>
    public interface IValidatable : IAssertion
    {
        Task<Assertions> Validate(ITypedElement input, ValidationContext vc);
    }

    public static class IValidatableExtensions
    {
        public async static Task<Assertions> ValidateAsync(this IEnumerable<IValidatable> validatables, ITypedElement elt, ValidationContext vc)
        {
            return await validatables.Select(v => v.Validate(elt, vc)).AggregateAsync();
        }

        public async static Task<Assertions> AggregateAsync(this IEnumerable<Task<Assertions>> tasks)
        {
            var result = await Task.WhenAll(tasks);
            return result.Aggregate(Assertions.Empty, (sum, other) => sum += other);
        }
    }
}
