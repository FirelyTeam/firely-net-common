using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Support.Model
{
    public interface IProfileCollection<T>
    {
        IEnumerable<T> Items { get; }
        T Append();
    }

    public interface IProfileIdentifier { }
    public interface IProfileContactDetail { }
    public interface IProfileElement { }
    public interface IProfileCodeableConcept { }
    public interface IProfileCoding { }
    public interface IProfileMapping { }

    public enum ProfilePublicationStatus
    {
        Draft,
        Active,
        Retired,
        Unknown
    }

    public interface IProfileUsageContext { }

    public interface IProfile
    {
        string Url { get; set; }

        IProfileCollection<IProfileIdentifier> Identifiers { get; }

        string Version { get; set; }
        string Name { get; set; }
        string Title { get; set; }
        ProfilePublicationStatus? Status { get; set; }
        bool? Experimental { get; set; }
        DateTime? Date { get; set; }
        string Publisher { get; set; }

        IProfileCollection<IProfileContactDetail> Contact { get; }

        string Description { get; set; }

        IProfileCollection<IProfileUsageContext> Context { get; }

        IProfileCollection<IProfileCodeableConcept> Jurisdiction { get; }

        string Purpose { get; set; }
        string Copyright { get; set; }

        IProfileCollection<IProfileCoding> Keywords { get; }

        string FhirVersion { get; set; }

        IProfileCollection<IProfileElement> Elements { get; }
    }
}
