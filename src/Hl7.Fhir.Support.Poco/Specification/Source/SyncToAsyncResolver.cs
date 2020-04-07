/* 
 * Copyright (c) 2020, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/fhir-net-api/blob/master/LICENSE
 */

using System;
using Hl7.Fhir.Model;
using System.Threading.Tasks;

namespace Hl7.Fhir.Specification.Source
{
    /// <summary>
    /// Implements <see cref="IAsyncResourceResolver" /> on top of an <see cref="IResourceResolver" />
    /// </summary>
    public class SyncToAsyncResolver : IAsyncResourceResolver
    {
        public IResourceResolver SyncResolver { get; private set; }

        public SyncToAsyncResolver(IResourceResolver sync) => SyncResolver = sync ?? throw new ArgumentNullException(nameof(sync));

        public Task<Resource> ResolveByUriAsync(string uri)
        {
            var result = SyncResolver.ResolveByUri(uri);

#if NET40
            //return TaskEx.FromResult(result);
            return null;
#else
            return System.Threading.Tasks.Task.FromResult(result);
#endif
        }

        public Task<Resource> ResolveByCanonicalUriAsync(string uri)
        {
            var result = SyncResolver.ResolveByCanonicalUri(uri);
#if NET40
            //return TaskEx.FromResult(result);
            return null;
#else
            return System.Threading.Tasks.Task.FromResult(result);
#endif
        }
    }

    public static class SyncToAsyncResolverExtensions
    { 
        /// <summary>
        /// Converts a (possibly non-async) resource resolver to an async one.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        /// <remarks>Note that this async method will block on the possibly synchronous source. This method
        /// is meant for temporary backwards-compatiblity reasons only.</remarks>
        public static IAsyncResourceResolver ToAsync(this IResourceResolver source) =>
            source is IAsyncResourceResolver ar ? ar : new SyncToAsyncResolver(source);
    }

}