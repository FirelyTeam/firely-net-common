namespace Firely.Fhir.Packages
{
    public class FhirPackageUrlProvider : IPackageUrlProvider
    {
        public string Root { get; private set; }

        public FhirPackageUrlProvider(string root)
        {
            this.Root = root.TrimEnd('/');
        }

        public string GetPackageListingUrl(string name) => $"{Root}/{name}";

        public string GetPackageUrl(PackageReference reference) => $"{Root}/{reference.Name}/{reference.Version}";

        public override string ToString() => $"(FHIR) {Root}";

        public string GetPublishUrl(int fhirVersion, PackageReference reference, PublishMode mode)
        {
            string url = $"{Root}/r{fhirVersion}?publishMode={mode}";
            return url;
        }
    }



}
