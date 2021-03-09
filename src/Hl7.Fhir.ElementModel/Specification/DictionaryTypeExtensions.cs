/* 
 * Copyright (c) 2021, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/fhir-net-api/blob/master/LICENSE
 */

using System;
using System.Collections.Generic;

namespace Hl7.Fhir.Specification
{
    /// <summary>
    /// What about just using IDictionary{string,object} as the universal instance?
    /// object might then be an Any subclass, an IEnumerable{Any} or another IDictionary{string,object}.
    /// We might have additional options for adding type info, chose from:
    ///   * _typeName (of type string) + _model (somewhere at root or down, of type ModelDefinition)
    ///   * _type (a reference to a TypeDefinition).
    /// We might add a wrapper with state that tracks the model, the path (old location), contained resources.
    /// Note that this design relies on "by convention" rather than a strict object model.
    /// </summary>
    public static class DictionaryTypeExtensions
    {
        public static NamedTypeDefinition GetTypeDefinition(IDictionary<string, object> instance)
        {
            if (!instance.TryGetValue("_type", out var type)) return null;

            return type switch
            {
                NamedTypeDefinition td => td,
                (string v, ModelDefinition md) when md.TryGetType(v, out var typedef) => typedef,
                _ => null
            };
        }

#if NET45 || NETSTANDARD1_6 || NETSTANDARD2_0
        public static void Deconstruct(this object o, out string v, out ModelDefinition md)
        {
            if (o is ValueTuple<string, ModelDefinition> vt)
            {
                v = vt.Item1;
                md = vt.Item2;
            }
            else if (o is Tuple<string, ModelDefinition> t)
            {
                v = t.Item1;
                md = t.Item2;
            }
            else
                throw new NotImplementedException($"Cannot deconstruct object of type {o.GetType()}");
        }
#endif
    }
}
