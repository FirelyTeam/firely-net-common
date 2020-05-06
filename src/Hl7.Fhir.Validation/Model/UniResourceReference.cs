namespace Hl7.Fhir.Validation.Model
{
    public class UniResourceReference: IUniElement
    {
        public string Reference { get; set; }
        public string Type { get; set; }
        public UniIdentifier Identifier { get; set; } 
        public string Display { get; set; }
    }
}
