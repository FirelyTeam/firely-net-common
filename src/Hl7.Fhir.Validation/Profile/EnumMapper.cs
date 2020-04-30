using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Validation.Profile
{
    public class EnumMapper<TExternal, TInternal>
        where TExternal : struct
        where TInternal : struct
    {
        private readonly Dictionary<TInternal, TExternal> _export;
        private readonly Dictionary<TExternal, TInternal> _import;

        public EnumMapper(params (TExternal facade, TInternal intern)[] mappings)
        {
            _export = mappings.ToDictionary(mapping => mapping.intern, mapping => mapping.facade);
            _import = mappings.ToDictionary(mapping => mapping.facade, mapping => mapping.intern);
        }

        public TExternal? Export(TInternal? intern)
        {
            return Translate(_export, intern);
        }

        public TInternal? Import(TExternal? external)
        {
            return Translate(_import, external);
        }

        private TTarget? Translate<TSource, TTarget>(IDictionary<TSource, TTarget> dictionary, TSource? source) 
            where TTarget: struct 
            where TSource: struct
        {
            if (source is null)
            {
                return null;
            }

            if (!dictionary.TryGetValue(source.Value, out var result))
            {
                throw new NotImplementedException($"Can not map '{typeof(TSource)}.{source.Value.ToString()}' to a corresponding '{typeof(TTarget).Name}'");
            }

            return result;
        }
    }
}
