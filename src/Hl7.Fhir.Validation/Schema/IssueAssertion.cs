using Hl7.Fhir.ElementModel;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace Hl7.Fhir.Validation.Schema
{
    public enum IssueSeverity
    {
        Fatal,
        Error,
        Warning,
        Information
    }

    public class IssueAssertion : IAssertion, IValidatable
    {
        public int IssueNumber { get; }
        public string Location { get; }
        public IssueSeverity? Severity { get; }
        public string Message { get; }

        public IssueAssertion(int issueNumber, string message, IssueSeverity? severity = null) :
            this(issueNumber, null, message, severity)
        {
        }

        public IssueAssertion(int issueNumber, string location, string message, IssueSeverity? severity = null)
        {
            IssueNumber = issueNumber;
            Location = location;
            Severity = severity;
            Message = message;
        }

        public JToken ToJson()
        {
            var props = new JObject(
                      new JProperty("issueNumber", IssueNumber),
                      new JProperty("severity", Severity),
                      new JProperty("message", Message));
            if (Location != null)
                props.Add(new JProperty("location", Location));
            return new JProperty("issue", props);
        }

        public Task<Assertions> Validate(ITypedElement input, ValidationContext vc)
            => Task.FromResult(new Assertions(new IssueAssertion(IssueNumber, input.Location, Message, Severity)));
    }
}
