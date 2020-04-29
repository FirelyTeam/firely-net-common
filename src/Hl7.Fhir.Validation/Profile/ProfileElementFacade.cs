using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Validation.Profile
{
    public interface IProfileElementFacade
    {
        ISliceCollectionFacade Slices { get; }
    }

    public abstract class ProfileElementFacadeBase<TElementDefinition>: IProfileElementFacade
    {
        private TElementDefinition _elementDefinition;
        public TElementDefinition ElementDefinition
        {
            get => _elementDefinition;
            set
            {
                PlugIn(value);
                _elementDefinition = value;
            }
        }

        public ISliceCollectionFacade Slices { get; protected set; }

        protected abstract void PlugIn(TElementDefinition elementDefinition);
    }
}
