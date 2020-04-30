using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Validation.Profile
{
    public interface IStructureDefinition: ICommitable
    {
        string Url { get; set; }
        ICollectionFacade<IIdentifier> Identifiers { get; }        
        string Version { get; set; }
        string Name { get; set; }
        string Title { get; set; }

        IStructureDefinitionSchema Differential { get; }
        IStructureDefinitionSchema Snapshot { get; }

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
    }
}
