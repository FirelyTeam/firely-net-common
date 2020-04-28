using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Validation.Profile
{
    public class IdentifierFacade<TIdentifier>: IdentifierFacade
    {
        protected TIdentifier Identifier { get; private set; }

        public IdentifierFacade(TIdentifier identifier)
        {
            Identifier = identifier;
        }
    }

    public abstract class IdentifierFacade
    {
        protected PrimitiveFacade<string> _systemFacade;
        public string System { get => _systemFacade.Value; set => _systemFacade.Value = value; }
    }
}
