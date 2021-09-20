namespace Firely.Fhir.Packages
{
    public class NodePackageUrlProvider : IPackageUrlProvider
    {
        public string Root { get; private set; } 

        public NodePackageUrlProvider(string root)
        {
            this.Root = root.TrimEnd('/') ;
        }

        public string GetPackageListingUrl(string scope, string name) => $"{Root}/@{scope}%2F{name}";

        public string GetPackageListingUrl(string name) => $"{Root}/{name}";
            

        public string GetPackageUrl(PackageReference reference)
        {
            return
                (reference.Scope == null)
                ? $"{Root}/{reference.Name}/-/{reference.Name}-{reference.Version}.tgz"
                : $"{Root}/@{reference.Scope}/{reference.Name}/-/{reference.Name}-{reference.Version}.tgz";
        }

        public string GetPublishUrl(int fhirVersion, PackageReference reference, PublishMode mode)
        { 
            // this is not yet made NPM compliant. 
            return $"{Root}/r{fhirVersion}/{reference.Name}";
        }

        public override string ToString() => $"(NPM) {Root}";
        
    }



}
