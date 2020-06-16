using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Serialization;
using Hl7.Fhir.Specification;
using Hl7.Fhir.Utility;
using System;

namespace Hl7.Fhir.Rest
{
    public static class EntryToTypedEntryExtensions
    {
        public static TypedEntryResponse ToTypedEntryResponse(this EntryResponse response, IStructureDefinitionSummaryProvider provider)
        {
            var result = new TypedEntryResponse
            {
                ContentType = response.ContentType,
                Body = response.Body,
                Etag = response.Etag,
                Headers = response.Headers,
                LastModified = response.LastModified,
                LastRequest = response.LastRequest,
                LastResponse = response.LastResponse,
                Location = response.Location,
                ResponseUri = response.ResponseUri,
                Status = response.Status
            };

            var body = response.GetBodyAsText();
            if (!string.IsNullOrEmpty(body))
            {               
                result.TypedElement = parseResource(body, response.ContentType, provider, response.IsSuccessful());               
            }

            return result;
        }
        
        private static ITypedElement parseResource(string bodyText, string contentType, IStructureDefinitionSummaryProvider provider, bool throwOnFormatException)
        {
            if (bodyText == null) throw Error.ArgumentNull(nameof(bodyText));
            if (provider == null) throw Error.ArgumentNull(nameof(provider));
           
            var fhirType = ContentType.GetResourceFormatFromContentType(contentType);

            if (fhirType == ResourceFormat.Unknown)
                throw new UnsupportedBodyTypeException(
                    "Endpoint returned a body with contentType '{0}', while a valid FHIR xml/json body type was expected. Is this a FHIR endpoint?"
                        .FormatWith(contentType), contentType, bodyText);

            if (!SerializationUtil.ProbeIsJson(bodyText) && !SerializationUtil.ProbeIsXml(bodyText))
                throw new UnsupportedBodyTypeException(
                        "Endpoint said it returned '{0}', but the body is not recognized as either xml or json.".FormatWith(contentType), contentType, bodyText);
            
            try
            {
               return (fhirType == ResourceFormat.Json)
                    ? FhirJsonNode.Parse(bodyText).ToTypedElement(provider)
                    : FhirXmlNode.Parse(bodyText).ToTypedElement(provider);
            }
            catch (FormatException) when (!throwOnFormatException)
            {           
                return null;
            }
        }
    }

    public class UnsupportedBodyTypeException : Exception
    {
        public string BodyType { get; set; }

        public string Body { get; set; }
        public UnsupportedBodyTypeException(string message, string mimeType, string body) : base(message)
        {
            BodyType = mimeType;
            Body = body;
        }
    }
}
