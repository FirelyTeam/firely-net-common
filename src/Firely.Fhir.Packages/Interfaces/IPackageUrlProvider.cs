namespace Firely.Fhir.Packages
{

    public interface IPackageUrlProvider
    {
        string Root { get; }
        string GetPackageListingUrl(string name);
        string GetPackageUrl(PackageReference reference);
        string GetPublishUrl(int fhirVersion, PackageReference reference, PublishMode mode);
    }

}
