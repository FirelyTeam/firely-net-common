using System.Collections.Generic;

namespace Hl7.Fhir.Validation.Model
{
    public enum UniStructureDefStatus
    {
        Active,
        Draft,
        Retired,
        Unknown
    }

    public enum UniStructureDefKind
    {
        Resource,
        Logical,
        PrimitiveType,
        ComplexType
    }

    public enum UniStructureDefDerivation
    {
        Constraint,
        Specialization
    }

    public class UniStructureDef
    {
        public string Url { get; set; }
        public List<UniIdentifier> Identifiers { get; set; }
        public string Version { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public UniStructureDefStatus? Status { get; set; }
        public bool? Experimental { get; set; }
        public string Date { get; set; }
        public string Publisher { get; set; }
        public List<UniContactDetail> Contacts { get; set; } 
        public string Description { get; set; }
        public List<UniUsageContext> UseContexts { get; set; } 
        public List<UniCodeableConcept> Jurisdictions { get; set; }
        public string Purpose { get; set; }
        public string Copyright { get; set; }
        public List<UniCoding> Keywords { get; set; }
        public string FhirVersion { get; set; }
        public List<UniStructureDefMapping> Mappings { get; set; } 
        public UniStructureDefKind? Kind { get; set; }
        public bool? Abstract { get; set; }
        public List<UniStructureDefContext> ExtensionContexts { get; set; } 
        public List<string> ExtensionContextInvariants { get; set; }
        public string Type { get; set; }
        public string BaseDefinition { get; set; }
        public UniStructureDefDerivation? Derivation { get; set; }

        public UniStructureDefElementSchema Differential { get; set; } 
        public UniStructureDefElementSchema Snapshot { get; set; }
    }
}
