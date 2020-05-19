﻿/* 
 * Copyright (c) 2020, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/fhir-net-api/blob/master/LICENSE
 */

using Hl7.Fhir.Specification;

namespace Hl7.Fhir.Patch.Internal
{
    /// <summary>
    /// This API supports infrastructure and is not intended to be used
    /// directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public interface IAdapter
    {
        bool TryAdd(
            object target,
            string name,
            IStructureDefinitionSummaryProvider contractResolver,
            object value,
            out string errorMessage);

        bool TryInsert (
            object target,
            int index,
            IStructureDefinitionSummaryProvider contractResolver,
            object value,
            out string errorMessage);

        bool TryDelete(
            object target,
            IStructureDefinitionSummaryProvider contractResolver,
            out string errorMessage);

        bool TryReplace(
            object target,
            IStructureDefinitionSummaryProvider contractResolver,
            object value,
            out string errorMessage);

        bool TryGet (
            object target,
            IStructureDefinitionSummaryProvider contractResolver,
            out object value,
            out string errorMessage);
    }
}
