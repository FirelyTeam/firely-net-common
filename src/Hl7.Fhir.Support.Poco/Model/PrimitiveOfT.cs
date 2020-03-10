/* 
 * Copyright (c) 2020, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */


using System;

namespace Hl7.Fhir.Model
{
#if !NETSTANDARD1_1
    [Serializable]
#endif
    public abstract class Primitive<T> : Primitive
    {
        // [WMR 20160615] Cannot provide common generic Value property, as subclasses differ in their implementation
        // e.g. Code<T> exposes T? Value where T : struct
        // T Value { get; set; }
        // => Instead, define and implement a generic interface IValue<T>
    }

}
