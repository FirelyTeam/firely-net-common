namespace Hl7.Fhir.Validation.Model
{
    public class UniRange : IUniElement
    {
        public UniQuantity Low { get; set; }
        public UniQuantity High { get; set; }
    }
}
