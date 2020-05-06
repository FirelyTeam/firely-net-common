using Hl7.Fhir.Utility;
using System;
using System.Collections.Generic;

namespace Hl7.Fhir.Validation.Model
{
    public class ElementMapper<TElement> : IAssignMapper<TElement, IUniElement>
        where TElement: class
    {
        private readonly Dictionary<Type, Func<MappingContext, TElement, IUniElement>> _forwards = new Dictionary<Type, Func<MappingContext, TElement, IUniElement>>(8);
        private readonly Dictionary<Type, Func<MappingContext, IUniElement, TElement>> _backwards = new Dictionary<Type, Func<MappingContext, IUniElement, TElement>>(8);

        public ElementMapper<TElement> Add<T1, T2>(IAssignMapper<T1, T2> assignMapper)
            where T1: TElement
            where T2: IUniElement
        {
            _forwards.Add(typeof(T1), (ctx, value) => ((T1)value).Map(ctx, assignMapper));
            _backwards.Add(typeof(T2), (ctx, value) => ((T2)value).Map(ctx, assignMapper));
            return this;
        }

        public ElementMapper<TElement> Add<T1, T2>(ITransferMapper<T1, T2> transferMapper)
            where T1 : class, TElement, new()
            where T2 : class, IUniElement, new()
        {
            _forwards.Add(typeof(T1), (ctx, value) => ((T1)value).Map(ctx, transferMapper));
            _backwards.Add(typeof(T2), (ctx, value) => ((T2)value).Map(ctx, transferMapper));
            return this;
        }

        public IUniElement Map(MappingContext context, TElement source) => Map(context, source, _forwards);

        public TElement Map(MappingContext context, IUniElement source) => Map(context, source, _backwards);

        private TTarget Map<TSource, TTarget>(MappingContext context, TSource source, Dictionary<Type, Func<MappingContext, TSource, TTarget>> maps)
            where TSource: class
            where TTarget: class
        {
            if (source is null)
            {
                return null;
            }

            var type = source.GetType();

            if (!maps.TryGetValue(type, out var map))
            {
                context.NotifyOrThrow(this, ExceptionNotification.Error(new NotImplementedException($"Unable to find a mapper for input type '{type.Name}'")));
                return default;
            }

            return map(context, source);
        }
    }
}
