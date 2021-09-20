namespace Firely.Fhir.Packages
{

    public enum PublishMode
    {
        New,
        Existing,
        Any
    }

    public static class PackageUrlProviders
    {
        public static IPackageUrlProvider Npm => new NodePackageUrlProvider("https://registry.npmjs.org");
        public static IPackageUrlProvider Simplifier => new FhirPackageUrlProvider("https://packages.simplifier.net");
        public static IPackageUrlProvider SimplifierNpm => new NodePackageUrlProvider("https://packages.simplifier.net");
        public static IPackageUrlProvider Staging => new FhirPackageUrlProvider("https://packages-staging.simplifier.net");
        public static IPackageUrlProvider StagingNpm => new NodePackageUrlProvider("https://packages-staging.simplifier.net");
        public static IPackageUrlProvider Localhost => new FhirPackageUrlProvider("http://packages.simplifier.ro/");

    }



}
