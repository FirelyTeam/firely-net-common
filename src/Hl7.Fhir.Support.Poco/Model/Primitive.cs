/* 
 * Copyright (c) 2020, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */


using System;
using System.Linq;
using Hl7.Fhir.Introspection;
using Hl7.Fhir.Serialization;
using Hl7.Fhir.Utility;

namespace Hl7.Fhir.Model
{
#if !NETSTANDARD1_1
    [Serializable]
#endif
    public abstract class Primitive : Element
    {
        public object ObjectValue { get; set; }

        public override string TypeName
        {
            get { return "Primitive"; }
        }

        public override string ToString()
        {
            // The primitive can exist without a value (when there is an extension present)
            // so we need to be able to handle when there is no extension present
            if (this.ObjectValue == null)
                return null;
            return PrimitiveTypeConverter.ConvertTo<string>(this.ObjectValue);
        }

        public override IDeepCopyable CopyTo(IDeepCopyable other)
        {
            if (other == null) throw Error.ArgumentNull(nameof(other));
            if(this.GetType() != other.GetType())
                throw Error.Argument(nameof(other), "Can only copy to an object of the same type");

            base.CopyTo(other);
            if (ObjectValue != null) ((Primitive)other).ObjectValue = ObjectValue;

            return other;
        }

        public override IDeepCopyable DeepCopy()
        {
            return CopyTo((IDeepCopyable)Activator.CreateInstance(this.GetType()));
        }

        public override bool Matches(IDeepComparable other)
        {
            return IsExactly(other);
        }

        public override bool IsExactly(IDeepComparable other)
        {
            if (other == null) throw Error.ArgumentNull(nameof(other));

            if (this.GetType() != other.GetType()) return false;

            if (!base.IsExactly(other)) return false;

            var otherValue = ((Primitive)other).ObjectValue;

            if (ObjectValue is byte[] bytes && otherValue is byte[] bytesOther)
                return Enumerable.SequenceEqual(bytes, bytesOther);
            else
                return Object.Equals(ObjectValue, ((Primitive)other).ObjectValue);
        }
    }
}
