namespace Hl7.Fhir.Validation.Model
{
    public class UniRange : IUniDataType
    {
        public UniQuantity Low { get; set; }
        public UniQuantity High { get; set; }
    }
}
