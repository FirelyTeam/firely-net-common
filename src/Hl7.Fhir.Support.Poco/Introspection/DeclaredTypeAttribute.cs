/* 
 * Copyright (c) 2014, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-net-sdk/master/LICENSE
 */

using System;

namespace Hl7.Fhir.Introspection
{
    [CLSCompliant(false)]
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class DeclaredTypeAttribute : VersionedAttribute
    {
        public DeclaredTypeAttribute()
        {
        }

        public Type Type { get; set; }
    }
}
