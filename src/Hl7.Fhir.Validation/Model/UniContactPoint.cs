namespace Hl7.Fhir.Validation.Model
{
    public enum UniContactPointSystem
    {
        Phone,
        Fax,
        Email,
        Pager,
        Url,
        Sms,
        Other
    }

    public enum UniContactPointUse
    {
        Home,
        Work,
        Temporary,
        Old,
        Mobile
    }

    public class UniContactPoint
    {
        public UniContactPointSystem? System { get; set; }
        public string Value { get; set; }
        public UniContactPointUse? Use { get; set; }
        public int? Rank { get; set; }
        public UniPeriod Period { get; set; }
    }
}
