/* 
 * Copyright (c) 2020, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/fhir-net-api/blob/master/LICENSE
 */

namespace Hl7.Fhir.Patch.Operations
{
    public enum OperationType
    {
        Add,
        Insert,
        Delete,
        Replace,
        Move,
        Invalid
    }
}