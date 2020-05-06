namespace Hl7.Fhir.Validation.Model
{
    public enum UniStructureDefContextType
    {
        Extension,
        Element,
        Fhirpath
    }

    public class UniStructureDefContext
    {
        public UniStructureDefContextType? Type { get; set; }
        public string Expression { get; set; }
    }
}
