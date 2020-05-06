using Hl7.Fhir.Utility;

namespace Hl7.Fhir.Validation.Model
{
    public class MappingContext : IExceptionSource
    {
        public ExceptionNotificationHandler ExceptionHandler { get; set; }
    }
}
