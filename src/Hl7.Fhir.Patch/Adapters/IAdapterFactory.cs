using Hl7.Fhir.Patch.Internal;
using Hl7.Fhir.Specification;

namespace Hl7.Fhir.Patch.Adapters
{
    /// <summary>
    /// Defines the operations used for loading an <see cref="IAdapter"/> based on the current object.
    /// </summary>
    public interface IAdapterFactory
    {
        /// <summary>
        /// Creates an <see cref="IAdapter"/> for the current object
        /// </summary>
        /// <param name="target">The target object</param>
        /// <param name="provider">The structure definition provider</param>
        /// <returns>The needed <see cref="IAdapter"/></returns>
#pragma warning disable PUB0001
        IAdapter Create(object target, IStructureDefinitionSummaryProvider provider);
#pragma warning restore PUB0001
    }
}
