using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Validation.Profile
{
    public interface IProfileSliceFacade
    {
        IElementCollectionFacade Elements { get; }
    }

    public abstract class ProfileSliceFacadeBase<TElementDefinition> : IProfileSliceFacade
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

        public IElementCollectionFacade Elements { get; protected set; }

        protected abstract void PlugIn(TElementDefinition elementDefinition);
    }
}
