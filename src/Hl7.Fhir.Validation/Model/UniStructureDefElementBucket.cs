using System.Collections.Generic;

namespace Hl7.Fhir.Validation.Model
{
    public class UniStructureDefElementMapping
    {

    }

    public class UniStructureDefElementConstraint
    {

    }

    public class UniStructureDefElementBucket
    {
        public string Name { get; set; }
        public string Label { get; set; }
        public List<UniCoding> Codes { get; set; }
        public string Short { get; set; }
        public string Definition { get; set; }
        public string Comment { get; set; }
        public string Requirements { get; set; }
        public List<string> Aliases { get; set; }
        public int? Min { get; set; }
        public int? Max { get; set; }
        public string ContentReference { get; set; }
        public string MeaningWhenMissing { get; set; }
        public string OrderMeaning { get; set; }
        public List<UniStructureDefElementConstraint> Constraints { get; set; }
        public bool? MustSupport { get; set; }
        public bool? IsModifier { get; set; }
        public string IsModifierReason { get; set; }
        public bool? IsSummary { get; set; }
        public List<UniStructureDefElementMapping> Mappings { get; set; }

        public List<UniStructureDefElement> Elements { get; set; } 
    }
}
