using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Validation.Profile
{
    public struct PrimitiveFacade<TPrimitive>
    {
        private readonly Func<TPrimitive> _getValue;
        private readonly Action<TPrimitive> _setValue;

        public PrimitiveFacade(Func<TPrimitive> getValue)
            : this(getValue, value => throw new NotImplementedException("Editing is not implemented on this primitive value facade")) { }

        public PrimitiveFacade(Func<TPrimitive> getValue, Action<TPrimitive> setValue)
        {
            _getValue = getValue;
            _setValue = setValue;
        }

        public TPrimitive Value { get => _getValue(); set => _setValue(value); }
    }

    //public struct PrimitiveFacade<TEntity, TPrimitive>
    //{
    //    public PrimitiveFacade(Func<TEntity, TPrimitive> getValue)
    //        : this(getValue, (entity, value) => throw new NotImplementedException("Editing is not implemented on this primitive value facade")) { }

    //    public PrimitiveFacade(Func<TEntity, TPrimitive> getValue, Action<TEntity, TPrimitive> setValue)
    //    {
    //        Get = getValue;
    //        Set = setValue;
    //    }

    //    public readonly Func<TEntity, TPrimitive> Get;
    //    public readonly Action<TEntity, TPrimitive> Set;
    //}
}
