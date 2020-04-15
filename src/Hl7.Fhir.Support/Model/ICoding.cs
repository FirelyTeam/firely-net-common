namespace Hl7.Fhir.Model
{
    public interface ICoding
    {
        string System { get; }

        string Code { get; }

        string Display { get; }
    }
}
