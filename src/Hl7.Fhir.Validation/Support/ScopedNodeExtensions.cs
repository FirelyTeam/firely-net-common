using Hl7.Fhir.ElementModel;
using System;
using System.Linq;

namespace Hl7.Fhir.Validation.Support
{
    internal static class ScopedNodeExtensions
    {
        /// <summary>
        /// Turn a relative reference into an absolute url, based on the fullUrl of the parent resource
        /// </summary>
        /// <param name="node"></param>
        /// <param name="identity"></param>
        /// <returns></returns>
        /// <remarks>See https://www.hl7.org/fhir/bundle.html#references for more information</remarks>
        public static ResourceIdentity MakeAbsolute(this ScopedNode node, ResourceIdentity identity)
        {
            if (identity.IsRelativeRestUrl)
            {
                // Relocate the relative url on the base given in the fullUrl of the entry (if applicable)
                var fullUrl = node.FullUrl();

                if (fullUrl != null)
                {
                    var parentIdentity = new ResourceIdentity(fullUrl);

                    if (parentIdentity.IsAbsoluteRestUrl)
                        identity = identity.WithBase(parentIdentity.BaseUri);
                    else if (parentIdentity.IsUrn)
                        identity = new ResourceIdentity($"{parentIdentity}/{identity.Id}");
                }

                // Return the identity - will remain relative if we did not find a fullUrl              
            }

            return identity;
        }

        public static T Resolve<T>(this T element, string reference, Func<string, T> externalResolver = null) where T : class, ITypedElement
        {
            // Then, resolve the url within the instance data first - this is only
            // possibly if we have a ScopedNavigator at hand
            if (element is ScopedNode scopedNode)
            {
                var identity = scopedNode.MakeAbsolute(new ResourceIdentity(reference));

                if (identity.IsLocal || identity.IsAbsoluteRestUrl || identity.IsUrn)
                {
                    var result = locateResource(identity);
                    if (result != null) return (T)(object)result;
                }
            }

            // Nothing found internally, now try the external resolver
            if (externalResolver != null)
                return externalResolver(reference);
            else
                return null;

            ScopedNode locateResource(ResourceIdentity identity)
            {
                var url = identity.ToString();

                foreach (var parent in scopedNode.ParentResources())
                {
                    if (parent.InstanceType == "Bundle")
                    {
                        var result = parent.BundledResources().FirstOrDefault(br => br.FullUrl == url)?.Resource;
                        if (result != null) return result;
                    }
                    else
                    {
                        if (parent.Id() == url) return parent;
                        var result = parent.ContainedResources().FirstOrDefault(cr => cr.Id() == url);
                        if (result != null) return result;
                    }
                }

                return null;
            }
        }

        public static string ParseReference(this ScopedNode node)
            => node.Children("reference").GetString();
    }
}
