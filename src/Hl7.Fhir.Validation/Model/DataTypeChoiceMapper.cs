using Hl7.Fhir.Utility;
using System;
using System.Collections.Generic;

namespace Hl7.Fhir.Validation.Model
{
    public class DataTypeChoiceMapper<TElement> : IAssignMapper<TElement, IUniDataType>
        where TElement: class
    {
        private readonly Dictionary<Type, Func<MappingContext, TElement, IUniDataType>> _forwards = new Dictionary<Type, Func<MappingContext, TElement, IUniDataType>>(8);
        private readonly Dictionary<Type, Func<MappingContext, IUniDataType, TElement>> _backwards = new Dictionary<Type, Func<MappingContext, IUniDataType, TElement>>(8);

        public DataTypeChoiceMapper<TElement> Add<T1, T2>(IAssignMapper<T1, T2> assignMapper)
            where T1: TElement
            where T2: IUniDataType
        {
            _forwards.Add(typeof(T1), (ctx, value) => ((T1)value).Map(ctx, assignMapper));
            _backwards.Add(typeof(T2), (ctx, value) => ((T2)value).Map(ctx, assignMapper));
            return this;
        }

        public DataTypeChoiceMapper<TElement> Add<T1, T2>(ITransferMapper<T1, T2> transferMapper)
            where T1 : class, TElement, new()
            where T2 : class, IUniDataType, new()
        {
            _forwards.Add(typeof(T1), (ctx, value) => ((T1)value).Map(ctx, transferMapper));
            _backwards.Add(typeof(T2), (ctx, value) => ((T2)value).Map(ctx, transferMapper));
            return this;
        }

        public IUniDataType Map(MappingContext context, TElement source) => Map(context, source, _forwards);

        public TElement Map(MappingContext context, IUniDataType source) => Map(context, source, _backwards);

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
