namespace Hl7.Fhir.Validation.Model
{
    public interface IAssignMapper<T1, T2>
    {
        T2 Map(MappingContext context, T1 source);
        T1 Map(MappingContext context, T2 source);
    }
}
