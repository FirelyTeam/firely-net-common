/* 
 * Copyright (c) 2015, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */


using Hl7.Fhir.ElementModel;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Hl7.FhirPath.Expressions
{
    internal class Closure
    {
        public Closure()
        {
        }

        public EvaluationContext EvaluationContext { get; private set; }

        public static Closure Root(ITypedElement root, EvaluationContext ctx = null)
        {
            var newContext = new Closure() { EvaluationContext = ctx ?? EvaluationContext.CreateDefault() };

            var input = new[] { root };
            newContext.SetThis(input);
            newContext.SetThat(input);
            newContext.SetIndex(ElementNode.CreateList(IndexFromLocation(root)));
            newContext.SetOriginalContext(input);
            if (ctx.Container != null) newContext.SetResource(new[] { ctx.Container });
            if (ctx.RootContainer != null) newContext.SetRootResource(new[] { ctx.RootContainer });

            return newContext;
        }

        private Dictionary<string, IEnumerable<ITypedElement>> _namedValues = new Dictionary<string, IEnumerable<ITypedElement>>();

        public virtual void SetValue(string name, IEnumerable<ITypedElement> value)
        {
            _namedValues.Remove(name);
            _namedValues.Add(name, value);
        }


        public Closure Parent { get; private set; }

        public virtual Closure Nest()
        {
            return new Closure()
            {
                Parent = this,
                EvaluationContext = this.EvaluationContext
            };
        }


        public virtual IEnumerable<ITypedElement> ResolveValue(string name)
        {
            // First, try to directly get "normal" values
            IEnumerable<ITypedElement> result = null;
            _namedValues.TryGetValue(name, out result);

            if (result != null) return result;

            // If that failed, try to see if the parent has it
            if (Parent != null)
            {
                result = Parent.ResolveValue(name);
                if (result != null) return result;
            }

            return null;
        }

#if NETSTANDARD1_1
        private static Regex _indexPattern = new Regex("\\[(?<index>\\d+)]$");
#else
        private static Regex _indexPattern = new Regex("\\[(?<index>\\d+)]$", RegexOptions.Compiled);
#endif
        internal static IEnumerable<ITypedElement> IndexFromLocation(ITypedElement context)
        {
            if (context is null || !context.Definition.IsCollection)
                return new[] { ElementNode.ForPrimitive(0) };

            var index = _indexPattern.Match(context.Location).Groups["index"].Value;
            return new[] { ElementNode.ForPrimitive(int.Parse(index)) };
        }
    }
}
