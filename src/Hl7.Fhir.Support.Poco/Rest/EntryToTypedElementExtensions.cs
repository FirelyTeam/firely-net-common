using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Serialization;
using Hl7.Fhir.Specification;
using Hl7.Fhir.Utility;
using System;

namespace Hl7.Fhir.Rest
{
    public static class EntryToTypedEntryExtensions
    {
        public static TypedEntryResponse ToTypedEntryResponse(this EntryResponse response, ParserSettings parserSettings, IStructureDefinitionSummaryProvider provider)
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
            
            if (response.Body != null)
            {
                try
                {
                    result.TypedElement = parseResource(response.GetBodyAsText(), response.ContentType, parserSettings, provider, response.IsSuccessful());
                }
                catch (UnsupportedBodyTypeException bte)
                {
                    var errorResult = new TypedEntryResponse
                    {
                        Status = response.Status,
                        BodyException = bte
                    };
                    return errorResult;
                }
            }

            return result;
        }
        
        private static ITypedElement parseResource(string bodyText, string contentType, ParserSettings settings, IStructureDefinitionSummaryProvider provider, bool throwOnFormatException)
        {
            ITypedElement result = null;

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
                if (bodyText == null) throw Error.ArgumentNull(nameof(bodyText));
                if (provider == null) throw Error.ArgumentNull(nameof(provider));

                if (fhirType == ResourceFormat.Json)
                    result = FhirJsonNode.Parse(bodyText).ToTypedElement(provider);
                else
                    result = FhirXmlNode.Parse(bodyText).ToTypedElement(provider);
            }
            catch (FormatException) when (!throwOnFormatException)
            {
                //if (throwOnFormatException) throw fe;

                //         [WMR 20181029]
                //TODO...
                //         ExceptionHandler.NotifyOrThrow(...)_

                return null;
            }
            return result;
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
