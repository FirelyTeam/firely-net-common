using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Validation.Profile
{
    public abstract class ProfileSliceFacade
    {
        public IElementCollectionFacade Elements { get; protected set; }
    }

    public abstract class ProfileSliceFacade<TElementDefinition> : ProfileSliceFacade
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

        protected abstract void PlugIn(TElementDefinition elementDefinition);
    }
}
