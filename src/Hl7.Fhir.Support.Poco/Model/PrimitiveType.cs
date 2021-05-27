/* 
 * Copyright (c) 2020, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-net-sdk/master/LICENSE
 */


using Hl7.Fhir.Introspection;
using Hl7.Fhir.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Hl7.Fhir.Model
{
    [Serializable]
    [FhirType("PrimitiveType", "http://hl7.org/fhir/StructureDefinition/PrimitiveType")]
    [DataContract]
    public abstract class PrimitiveType : DataType
    {
        public override string TypeName { get { return "PrimitiveType"; } }

        public object ObjectValue { get; set; }

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
            if (other is PrimitiveType dest)
            {
                base.CopyTo(dest);
                if (ObjectValue != null) ((PrimitiveType)other).ObjectValue = ObjectValue;
                return dest;
            }
            else
                throw new ArgumentException("Can only copy to an object of the same type", "other");
        }

        public override IDeepCopyable DeepCopy()
        {
            return CopyTo((IDeepCopyable)Activator.CreateInstance(this.GetType()));
        }

        public override bool Matches(IDeepComparable other) => IsExactly(other);

        public override bool IsExactly(IDeepComparable other)
        {
            if (!(other is PrimitiveType otherT)) return false;

            if (!base.IsExactly(other)) return false;

            var otherValue = otherT.ObjectValue;

            if (ObjectValue is byte[] bytes && otherValue is byte[] bytesOther)
                return Enumerable.SequenceEqual(bytes, bytesOther);
            else
                return Object.Equals(ObjectValue, otherT.ObjectValue);
        }

        [IgnoreDataMember]
        public override IEnumerable<Base> Children
        {
            get
            {
                foreach (var item in base.Children) yield return item;
            }
        }

        [IgnoreDataMember]
        public override IEnumerable<ElementValue> NamedChildren
        {
            get
            {
                foreach (var item in base.NamedChildren) yield return item;

            }
        }

    }
}
