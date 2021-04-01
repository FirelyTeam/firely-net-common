/* 
 * Copyright (c) 2019, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-net-sdk/master/LICENSE
 */

using Hl7.Fhir.Specification;
using Hl7.Fhir.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace Hl7.Fhir.ElementModel
{
    public class ScopedNode : ITypedElement, IAnnotated, IExceptionSource
    {
        private class Cache
        {
            public readonly object _lock = new();

            public string? Id;
            public IEnumerable<ScopedNode>? ContainedResources;
            public IEnumerable<BundledResource>? BundledResources;
        }

        private readonly Cache _cache = new();

        public readonly ITypedElement Current;

        public ScopedNode(ITypedElement wrapped)
        {
            Current = wrapped;
            if (Current is IExceptionSource ies && ies.ExceptionHandler == null)
                ies.ExceptionHandler = (o, a) => ExceptionHandler.NotifyOrThrow(o, a);
        }

        private ScopedNode(ScopedNode parent, ScopedNode? parentResource, ITypedElement wrapped, string? fullUrl)
        {
            Current = wrapped;
            ExceptionHandler = parent.ExceptionHandler;
            ParentResource = parent.AtResource ? parent : parentResource;

            _fullUrl = fullUrl;

            if (Current.Name == "entry")
                _fullUrl = Current.Children("fullUrl").FirstOrDefault()?.Value as string ?? _fullUrl;

        }

        public ExceptionNotificationHandler? ExceptionHandler { get; set; }

        /// <summary>
        /// Represents the most direct resource parent in which the current node 
        /// is located.
        /// </summary>
        /// <remarks>
        /// When the node is the inital root, there is no parent.
        /// </remarks>
        public readonly ScopedNode? ParentResource;

        public string LocalLocation => ParentResource == null ? Location :
                        $"{ParentResource.InstanceType}.{Location.Substring(ParentResource.Location.Length + 1)}";

        public string Name => Current.Name;

        public string InstanceType => Current.InstanceType;

        public object Value => Current.Value;

        public string Location => Current.Location;

        public bool AtResource => Current.Definition?.IsResource ?? Current is IResourceTypeSupplier rt && rt.ResourceType is not null;

        public string NearestResourceType => ParentResource == null ? Location : ParentResource.InstanceType;

        /// <summary>
        /// The %resource context, as defined by FHIRPath
        /// </summary>
        /// <remarks>
        /// This is the original resource the current context is part of. When evaluating a datatype, 
        /// this would be the resource the element is part of. Do not go past a root resource into a bundle, 
        /// if it is contained in a bundle.
        /// </remarks>
        public ITypedElement ResourceContext
        {
            get
            {
                var scan = this;

                while (scan.ParentResource != null && scan.ParentResource.InstanceType != "Bundle")
                {
                    scan = scan.ParentResource;
                }

                return scan;
            }
        }

        public IElementDefinitionSummary Definition => Current.Definition;

        /// <summary>
        /// Get the list of container parents in a list, nearest parent first.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ScopedNode> ParentResources()
        {
            var scan = this.ParentResource;

            while (scan != null)
            {
                yield return scan;

                scan = scan.ParentResource;
            }
        }

        /// <summary>
        /// Returns the Id of the resource, if the current node is a resource
        /// </summary>
        /// <returns></returns>
        public string? Id()
        {
            if (_cache.Id == null)
            {
                _cache.Id = AtResource ? "#" + Current.Children("id").FirstOrDefault()?.Value : null;
            }

            return _cache.Id;
        }

        public IEnumerable<ScopedNode> ContainedResources()
        {
            if (_cache.ContainedResources == null)
            {
                _cache.ContainedResources = AtResource ? 
                    this.Children("contained").Cast<ScopedNode>():
                    Enumerable.Empty<ScopedNode>();
            }
            return _cache.ContainedResources;
        }

        public class BundledResource
        {
            public string? FullUrl;
            public ScopedNode? Resource;
        }

        public IEnumerable<BundledResource> BundledResources()
        {
            if (_cache.BundledResources == null)
            {
                if (InstanceType == "Bundle")
                    _cache.BundledResources = from e in this.Children("entry")
                                              let fullUrl = e.Children("fullUrl").FirstOrDefault()?.Value as string
                                              let resource = e.Children("resource").FirstOrDefault() as ScopedNode
                                              select new BundledResource { FullUrl = fullUrl, Resource = resource };
                else
                    _cache.BundledResources = Enumerable.Empty<BundledResource>();
            }

            return _cache.BundledResources;
        }


        private readonly string? _fullUrl = null;

        public string? FullUrl() => _fullUrl;

        public IEnumerable<object> Annotations(Type type)
        {
            if (type == typeof(ScopedNode))
                return new[] { this };
            else
                return Current.Annotations(type);
        }

        public IEnumerable<ITypedElement> Children(string? name = null) =>
            Current.Children(name).Select(c => new ScopedNode(this, ParentResource, c, _fullUrl));
    }
}
