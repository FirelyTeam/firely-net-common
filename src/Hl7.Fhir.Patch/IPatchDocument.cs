/* 
 * Copyright (c) 2020, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/fhir-net-api/blob/master/LICENSE
 */

using Hl7.Fhir.ElementModel;
using System;

namespace Hl7.Fhir.Patch
{
    public interface IPatchDocument
    {
        /// <summary>
        /// Apply this IPatchDocument
        /// </summary>
        /// <param name="objectToApplyTo">Object to apply the IPatchDocument to</param>
        void ApplyTo (ElementNode objectToApplyTo);
        /// <summary>
        /// Apply this IPatchDocument
        /// </summary>
        /// <param name="objectToApplyTo">Object to apply the IPatchDocument to</param>
        /// <param name="logErrorAction">Action to log errors</param>
        void ApplyTo (ElementNode objectToApplyTo, Action<PatchError> logErrorAction);

        /// <summary>
        /// Apply this IPatchDocument
        /// </summary>
        /// <param name="objectToApplyTo">Object to apply the IPatchDocument to</param>
        /// <param name="adapter">IObjectAdapter instance to use when applying</param>
        /// <param name="logErrorAction">Action to log errors</param>
        void ApplyTo (ElementNode objectToApplyTo, PatchHelper adapter, Action<PatchError> logErrorAction);

        /// <summary>
        /// Apply this IPatchDocument
        /// </summary>
        /// <param name="objectToApplyTo">Object to apply the IPatchDocument to</param>
        /// <param name="adapter">IObjectAdapter instance to use when applying</param>
        void ApplyTo (ElementNode objectToApplyTo, PatchHelper adapter);
    }
}