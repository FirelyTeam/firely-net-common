using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Validation.Profile
{
    public interface IStructureDefinitionFacadeProvider
    {
        Func<PrimitiveFacade<string>> CreateUrlFacade { get; }
        Func<ICollectionFacade<IdentifierFacade>> CreateIdentifiersFacade { get; }
        Func<IElementSchemaFacade> CreateElementSchemaFacade { get; }
    }

    public class StructureDefinitionFacade
    {
        private readonly IStructureDefinitionFacadeProvider _provider;

        public StructureDefinitionFacade(IStructureDefinitionFacadeProvider provider)
        {
            _provider = provider;
            _urlFacade = new Lazy<PrimitiveFacade<string>>(provider.CreateUrlFacade);
            _identifiersFacade = new Lazy<ICollectionFacade<IdentifierFacade>>(provider.CreateIdentifiersFacade);
            _elementSchemaFacade = new Lazy<IElementSchemaFacade>(provider.CreateElementSchemaFacade);
        }

        private readonly Lazy<PrimitiveFacade<string>> _urlFacade;
        public string Url { get => _urlFacade.Value.Value; set => _urlFacade.Value.Value = value; }

        private readonly Lazy<ICollectionFacade<IdentifierFacade>> _identifiersFacade;
        public ICollectionFacade<IdentifierFacade> Identifiers => _identifiersFacade.Value;

        //string Version { get; set; }
        //string Name { get; set; }
        //string Title { get; set; }
        //ProfilePublicationStatus? Status { get; set; }
        //bool? Experimental { get; set; }
        //DateTime? Date { get; set; }
        //string Publisher { get; set; }

        //IProfileCollection<IProfileContactDetail> Contact { get; }

        //string Description { get; set; }

        //IProfileCollection<IProfileUsageContext> Context { get; }

        //IProfileCollection<IProfileCodeableConcept> Jurisdiction { get; }

        //string Purpose { get; set; }
        //string Copyright { get; set; }

        //IProfileCollection<IProfileCoding> Keywords { get; }

        //string FhirVersion { get; set; }

        private readonly Lazy<IElementSchemaFacade> _elementSchemaFacade;
        IElementCollectionFacade Elements => _elementSchemaFacade.Value.Elements;
    }
}
