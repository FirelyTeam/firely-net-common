using System;
using System.Collections.Generic;
using System.Linq;

namespace Hl7.Fhir.Validation.Model
{
    public static class MappingExtensions
    {
        public static List<T> Map<T>(this IEnumerable<T> source)
#if !NETSTANDARD1_1
            where T: IConvertible // bit of a hack: we only want primitive types here and string for they get copied (not for .NET Standard 1.1, but will check in other targets)
#endif
        {
            if (source is null)
            {
                return null;
            }

            var target = new List<T>(source.Count());
            target.AddRange(source);
            return target;
        }

        public static TTarget Map<TSource, TTarget>(this TSource source, MappingContext context, IAssignMapper<TSource, TTarget> mapper)
        {
            return mapper.Map(context, source);
        }

        public static TTarget Map<TSource, TTarget>(this TSource source, MappingContext context, IAssignMapper<TTarget, TSource> mapper)
        {
            return mapper.Map(context, source);
        }

        public static List<TTarget> Map<TSource, TTarget>(this List<TSource> source, MappingContext context, ITransferMapper<TSource, TTarget> transferer)
            where TSource : class
            where TTarget : class, new()
        {
            if (source is null || !source.Any())
            {
                return null;
            }

            var target = new List<TTarget>(source.Count);
            target.AddRange(source.Select(sourceItem =>
            {
                var targetItem = new TTarget();
                transferer.Transfer(context, sourceItem, targetItem);
                return targetItem;
            }));
            return target;
        }

        public static List<TTarget> Map<TSource, TTarget>(this List<TSource> source, MappingContext context, ITransferMapper<TTarget, TSource> transferer)
            where TSource : class
            where TTarget : class, new()
        {
            if (source is null || !source.Any())
            {
                return null;
            }

            var target = new List<TTarget>(source.Count);
            target.AddRange(source.Select(sourceItem =>
            {
                var targetItem = new TTarget();
                transferer.Transfer(context, sourceItem, targetItem);
                return targetItem;
            }));
            return target;
        }

        public static TTarget Map<TSource, TTarget>(this TSource source, MappingContext context, ITransferMapper<TSource, TTarget> transferer)
            where TSource : class
            where TTarget : class, new()
        {
            if (source is null)
            {
                return null;
            }

            var target = new TTarget();
            transferer.Transfer(context, source, target);
            return target;
        }

        public static TTarget Map<TSource, TTarget>(this TSource source, MappingContext context, ITransferMapper<TTarget, TSource> transferer)
            where TSource : class
            where TTarget : class, new()
        {
            if (source is null)
            {
                return null;
            }

            var target = new TTarget();
            transferer.Transfer(context, source, target);
            return target;
        }

        public static IAssignMapper<T1, T2> ToAssignMapper<T1, T2>(this ITransferMapper<T1, T2> transferMapper)
            where T1 : class, new()
            where T2 : class, new()
        {
            return new AssignMapper<T1, T2>(transferMapper);
        }

        private class AssignMapper<T1, T2> : IAssignMapper<T1, T2>
            where T1: class, new()
            where T2: class, new()
        {
            private readonly ITransferMapper<T1, T2> _transferMapper;

            public AssignMapper(ITransferMapper<T1, T2> transferMapper)
            {
                _transferMapper = transferMapper;
            }

            public T2 Map(MappingContext context, T1 source)
            {
                if (source is null)
                {
                    return null;
                }

                var target = new T2();
                _transferMapper.Transfer(context, source, target);
                return target;
            }

            public T1 Map(MappingContext context, T2 source)
            {
                if (source is null)
                {
                    return null;
                }

                var target = new T1();
                _transferMapper.Transfer(context, source, target);
                return target;
            }
        }
    }
}
