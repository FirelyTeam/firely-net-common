using Newtonsoft.Json.Linq;

namespace Hl7.Fhir.Validation.Schema
{
    public enum IssueSeverity
    {
        Fatal,
        Error,
        Warning,
        Information
    }

    public class IssueAssertion : IAssertion
    {
        public int IssueNumber { get; }
        public string Location { get; }
        public IssueSeverity? Severity { get; }
        public string Message { get; }

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
                      new JProperty("location", Location),
                      new JProperty("severity", Severity),
                      new JProperty("message", Message));
            return new JProperty("issue", props);
        }
    }
}
