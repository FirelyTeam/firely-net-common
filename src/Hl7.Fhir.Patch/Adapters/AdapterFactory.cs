using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Patch.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Hl7.Fhir.Patch.Adapters
{
    /// <summary>
    /// The default AdapterFactory to be used for resolving <see cref="IAdapter"/>.
    /// </summary>
    public class AdapterFactory : IAdapterFactory
    {
        /// <inheritdoc />
#pragma warning disable PUB0001
        public virtual IAdapter Create(object target)
#pragma warning restore PUB0001
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if ( target is ElementNode )
            {
                return new ElementNodeAdapter();
            }
            if ( target is IEnumerable<ITypedElement> )
            {
                return new CollectionAdapter();
            } 

            throw new NotSupportedException($"Patch is not supported on objects of type '{target.GetType()}'");
        }
    }
}
