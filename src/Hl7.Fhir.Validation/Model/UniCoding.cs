namespace Hl7.Fhir.Validation.Model
{
    public class UniCoding : IUniDataType
    {
        public string System { get; set; }
        public string Version { get; set; }
        public string Code { get; set; }
        public string Display { get; set; }
        public bool? UserSelected { get; set; }
    }
}
