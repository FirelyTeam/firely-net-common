using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Firely.Fhir.Packages
{

    public class PackageClient : IPackageServer, IDisposable
    {
        public static PackageClient Create(string source, bool npm = false, bool insecure = false)
        {
            var urlprovider = npm ? (IPackageUrlProvider) new NodePackageUrlProvider(source) : new FhirPackageUrlProvider(source);
            var httpClient = insecure ? Testing.GetInsecureClient() : new HttpClient();

            return new PackageClient(urlprovider, httpClient);
            
        }

        public static PackageClient Create()
        {
            var provider = PackageUrlProviders.Simplifier;
            return new PackageClient(provider);
        }

        public PackageClient(IPackageUrlProvider urlProvider, HttpClient? client = null)
        {
            this.urlProvider = urlProvider;
            this.httpClient = client ?? new HttpClient();
        }

        readonly IPackageUrlProvider urlProvider;
        readonly HttpClient httpClient;

        public async ValueTask<string?> DownloadListingRawAsync(string pkgname)
        {
           
            var url = urlProvider.GetPackageListingUrl(pkgname);
            try
            {
                var response = await httpClient.GetAsync(url);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        public async ValueTask<PackageListing?> DownloadListingAsync(string pkgname)
        {
            var body = await DownloadListingRawAsync(pkgname);
            if (body is null) return null;
            return Parser.Deserialize<PackageListing>(body);
        }

        public async ValueTask<List<PackageCatalogEntry>?> CatalogPackagesAsync(
            string? pkgname = null, 
            string? canonical = null, 
            string? fhirversion = null,
            bool preview = false)
        { 
            var parameters = new NameValueCollection();
            parameters.AddWhenValued("name", pkgname);
            parameters.AddWhenValued("canonical", canonical);
            parameters.AddWhenValued("fhirversion", fhirversion);
            parameters.AddWhenValued("prerelease", preview ? "true" : "false");
            string query = parameters.ToQueryString();

            string url = $"{urlProvider.Root}/catalog?{query}";

            try
            {
                var body = await httpClient.GetStringAsync(url);
                var result = Parser.Deserialize<List<PackageCatalogEntry>>(body);
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        internal async ValueTask<byte[]> DownloadPackage(PackageReference reference)
        {
            string url = urlProvider.GetPackageUrl(reference);
            return await httpClient.GetByteArrayAsync(url);
        }

        public async ValueTask<HttpResponseMessage> Publish(PackageReference reference, int fhirVersion, byte[] buffer)
        {
            string url = urlProvider.GetPublishUrl(fhirVersion, reference, PublishMode.Any);
            var content = new ByteArrayContent(buffer);
            var response = await httpClient.PostAsync(url, content);

            return response;
        }

        #region IDisposable

        bool disposed;

        void IDisposable.Dispose() => Dispose(true);

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // [WMR 20181102] HttpClient will dispose internal HttpClientHandler/WebRequestHandler
                    httpClient?.Dispose();
                }

                // release any unmanaged objects
                // set the object references to null

                disposed = true;
            }
        }

        #endregion

        public override string ToString() => urlProvider.ToString();

        public async Task<Versions> GetVersions(string name)
        {
            var listing = await DownloadListingAsync(name);
            if (listing is null) return new Versions();
            
            return listing.ToVersions();
        }

        public async Task<byte[]> GetPackage(PackageReference reference)
        {
            return await DownloadPackage(reference);
        }
    }
}
