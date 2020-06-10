using Hl7.Fhir.Utility;

namespace Hl7.Fhir.Validation.Support
{
    public enum AggregationMode
    {
        /// <summary>
        /// MISSING DESCRIPTION
        /// (system: http://hl7.org/fhir/resource-aggregation-mode)
        /// </summary>
        [EnumLiteral("contained", "http://hl7.org/fhir/resource-aggregation-mode"), Description("Contained")]
        Contained,
        /// <summary>
        /// MISSING DESCRIPTION
        /// (system: http://hl7.org/fhir/resource-aggregation-mode)
        /// </summary>
        [EnumLiteral("referenced", "http://hl7.org/fhir/resource-aggregation-mode"), Description("Referenced")]
        Referenced,
        /// <summary>
        /// MISSING DESCRIPTION
        /// (system: http://hl7.org/fhir/resource-aggregation-mode)
        /// </summary>
        [EnumLiteral("bundled", "http://hl7.org/fhir/resource-aggregation-mode"), Description("Bundled")]
        Bundled,
    }

}
