namespace Hl7.Fhir.Validation.Model
{
    public interface ITransferMapper<T1, T2>
    {
        void Transfer(MappingContext context, T1 source, T2 target);
        void Transfer(MappingContext context, T2 source, T1 target);
    }
}
