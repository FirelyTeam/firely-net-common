/* 
 * Copyright (c) 2020, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/fhir-net-api/blob/master/LICENSE
 */

using System;
using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Patch.Adapters;

namespace Hl7.Fhir.Patch
{
    public static class ElementNodePatchExtensions
    {
        /// <summary>
        /// Apply <paramref name="patchDocument"/> to this <see cref="ElementNode"/>
        /// </summary>
        /// <param name="original"><see cref="ElementNode"/> to apply patch to</param>
        /// <param name="patchDocument"><see cref="IPatchDocument"/> to apply to this <see cref="ElementNode"/></param>
        internal static void Apply (this ElementNode original, IPatchDocument patchDocument)
        {
            patchDocument.ApplyTo(original);
        }

        /// <summary>
        /// Apply <paramref name="patchDocument"/> to this <see cref="ElementNode"/>
        /// </summary>
        /// <param name="original"><see cref="ElementNode"/> to apply patch to</param>
        /// <param name="patchDocument"><see cref="IPatchDocument"/> to apply to this <see cref="ElementNode"/></param>
        /// <param name="logErrorAction">Action to log errors</param>
        internal static void Apply (this ElementNode original, IPatchDocument patchDocument, Action<PatchError> logErrorAction)
        {
            patchDocument.ApplyTo(original, logErrorAction);
        }

        /// <summary>
        /// Apply <paramref name="patchDocument"/> to this <see cref="ElementNode"/>
        /// </summary>
        /// <param name="original"><see cref="ElementNode"/> to apply patch to</param>
        /// <param name="patchDocument"><see cref="IPatchDocument"/> to apply to this <see cref="ElementNode"/></param>
        /// <param name="adapter">IObjectAdapter instance to use when applying</param>
        /// <param name="logErrorAction">Action to log errors</param>
        internal static void Apply (this ElementNode original, IPatchDocument patchDocument, IObjectAdapter adapter, Action<PatchError> logErrorAction)
        {
            patchDocument.ApplyTo(original, adapter, logErrorAction);
        }

        /// <summary>
        /// Apply <paramref name="patchDocument"/> to this <see cref="ElementNode"/>
        /// </summary>
        /// <param name="original"><see cref="ElementNode"/> to apply patch to</param>
        /// <param name="patchDocument"><see cref="IPatchDocument"/> to apply to this <see cref="ElementNode"/></param>
        /// <param name="adapter">IObjectAdapter instance to use when applying</param>
        internal static void Apply (this ElementNode original, IPatchDocument patchDocument, IObjectAdapter adapter)
        {
            patchDocument.ApplyTo(original, adapter);
        }

        /// <summary>
        /// Apply <paramref name="patchDocument"/> to a copy of this <see cref="ITypedElement"/> and return the patched <see cref="ElementNode"/>
        /// </summary>
        /// <param name="original"><see cref="ElementNode"/> to apply patch to</param>
        /// <param name="patchDocument"><see cref="IPatchDocument"/> to apply to this <see cref="ElementNode"/></param>
        public static ITypedElement Apply (this ITypedElement original, IPatchDocument patchDocument)
        {
            var elementNode = ElementNode.FromElement(original);
            patchDocument.ApplyTo(elementNode);
            return elementNode;
        }

        /// <summary>
        /// Apply <paramref name="patchDocument"/> to a copy of this <see cref="ITypedElement"/> and return the patched <see cref="ElementNode"/>
        /// </summary>
        /// <param name="original"><see cref="ElementNode"/> to apply patch to</param>
        /// <param name="patchDocument"><see cref="IPatchDocument"/> to apply to this <see cref="ElementNode"/></param>
        /// <param name="logErrorAction">Action to log errors</param>
        public static ITypedElement Apply (this ITypedElement original, IPatchDocument patchDocument, Action<PatchError> logErrorAction)
        {
            var elementNode = ElementNode.FromElement(original);
            patchDocument.ApplyTo(elementNode, logErrorAction);
            return elementNode;
        }

        /// <summary>
        /// Apply <paramref name="patchDocument"/> to a copy of this <see cref="ITypedElement"/> and return the patched <see cref="ElementNode"/>
        /// </summary>
        /// <param name="original"><see cref="ElementNode"/> to apply patch to</param>
        /// <param name="patchDocument"><see cref="IPatchDocument"/> to apply to this <see cref="ElementNode"/></param>
        /// <param name="adapter">IObjectAdapter instance to use when applying</param>
        /// <param name="logErrorAction">Action to log errors</param>
        public static ITypedElement Apply (this ITypedElement original, IPatchDocument patchDocument, IObjectAdapter adapter, Action<PatchError> logErrorAction)
        {
            var elementNode = ElementNode.FromElement(original);
            patchDocument.ApplyTo(elementNode, adapter, logErrorAction);
            return elementNode;
        }

        /// <summary>
        /// Apply <paramref name="patchDocument"/> to a copy of this <see cref="ITypedElement"/> and return the patched <see cref="ElementNode"/>
        /// </summary>
        /// <param name="original"><see cref="ElementNode"/> to apply patch to</param>
        /// <param name="patchDocument"><see cref="IPatchDocument"/> to apply to this <see cref="ElementNode"/></param>
        /// <param name="adapter">IObjectAdapter instance to use when applying</param>
        public static ITypedElement Apply (this ITypedElement original, IPatchDocument patchDocument, IObjectAdapter adapter)
        {
            var elementNode = ElementNode.FromElement(original);
            patchDocument.ApplyTo(elementNode, adapter);
            return elementNode;
        }
    }
}
