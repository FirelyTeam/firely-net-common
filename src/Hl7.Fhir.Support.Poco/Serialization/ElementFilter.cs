/* 
 * Copyright (c) 2021, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-net-sdk/master/LICENSE
 */

#nullable enable

#if NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER


using Hl7.Fhir.Introspection;
using System;

namespace Hl7.Fhir.Serialization
{
    public delegate ElementFilterOperation ElementFilter(PropertyMapping? pm, int nesting, string localPath, object value);

    public enum ElementFilterOperation
    {
        Include,
        IncludeTree,
        Exclude,
    }
}

#endif
#nullable restore
