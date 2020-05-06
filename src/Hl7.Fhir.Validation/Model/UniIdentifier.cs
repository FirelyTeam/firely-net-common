namespace Hl7.Fhir.Validation.Model
{
    public enum UniIdentifierUse
    {
        Usual,
        Official,
        Temporary,
        Secondary,
        Old
    }

    public class UniIdentifier
    {
        public UniIdentifierUse? Use { get; set; }
        public UniCodeableConcept Type { get; set; }
        public string System { get; set; }
        public string Value { get; set; }
        public UniPeriod Period { get; set; } 
        public UniResourceReference Assigner { get; set; }
    }
}
