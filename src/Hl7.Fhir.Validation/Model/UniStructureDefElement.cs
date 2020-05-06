using System.Collections.Generic;

namespace Hl7.Fhir.Validation.Model
{
    public enum UniStructureDefElementSlicingDiscriminatorType
    {
        Value,
        Exists,
        Pattern,
        Type,
        Profile
    }

    public class UniStructureDefElementSlicingDiscriminator
    {
        public UniStructureDefElementSlicingDiscriminatorType? Type { get; set; }
        public string Path { get; set; }
    }

    public class UniStructureDefElementSlicing
    {
        public List<UniStructureDefElementSlicingDiscriminator> SlicingDiscriminators { get; set; }
        public string SlicingDescription { get; set; }
        public bool? SlicingOrdered { get; set; }
        public string SlicingRules { get; set; }
        public int? SlicingMin { get; set; }
        public string SlicingMax { get; set; }
    }

    public class UniStructureDefElement
    {
        public string Name { get; set; }
        
        public UniStructureDefElementSlicing Slicing { get; set; }

        public List<UniStructureDefElementBucket> Buckets { get; set; }
    }
}
