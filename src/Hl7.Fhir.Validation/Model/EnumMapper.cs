using System;
using System.Collections.Generic;
using System.Linq;
using Hl7.Fhir.Utility;

namespace Hl7.Fhir.Validation.Model
{
    public class EnumMapper<T1, T2>: IAssignMapper<T1?, T2?>
        where T1 : struct
        where T2 : struct
    {
        private readonly Dictionary<T2, T1> _export;
        private readonly Dictionary<T1, T2> _import;

        public EnumMapper(params (T1 facade, T2 intern)[] mappings)
        {
            _export = mappings.ToDictionary(mapping => mapping.intern, mapping => mapping.facade);
            _import = mappings.ToDictionary(mapping => mapping.facade, mapping => mapping.intern);
        }

        public T1? Map(MappingContext context, T2? intern)
        {
            return Map(context, _export, intern);
        }

        public T2? Map(MappingContext context, T1? external)
        {
            return Map(context, _import, external);
        }

        private TTarget? Map<TSource, TTarget>(MappingContext context, IDictionary<TSource, TTarget> dictionary, TSource? source) 
            where TTarget: struct 
            where TSource: struct
        {
            if (source is null)
            {
                return null;
            }

            if (!dictionary.TryGetValue(source.Value, out var result))
            {
                context.NotifyOrThrow(this, ExceptionNotification.Error(new NotImplementedException($"Can not map '{typeof(TSource)}.{source.Value.ToString()}' to a corresponding '{typeof(TTarget).Name}'")));
                return null;
            }

            return result;
        }
    }
}
