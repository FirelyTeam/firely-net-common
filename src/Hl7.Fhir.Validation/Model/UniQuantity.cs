namespace Hl7.Fhir.Validation.Model
{ 
    public enum UniQuantityComparator
    {
        LessThan,
        LessOrEqual,
        GreaterOrEqual,
        GreaterThen
    }

    public class UniQuantity : IUniDataType
    {
        public decimal? Value { get; set; }
        public UniQuantityComparator? Comparator { get; set; }
        public string Unit { get; set; }
        public string System { get; set; }
        public string Code { get; set; }
    }
}
