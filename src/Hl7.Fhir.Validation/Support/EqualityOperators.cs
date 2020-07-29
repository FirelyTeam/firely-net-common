/* 
 * Copyright (c) 2015, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

#nullable enable

using Hl7.Fhir.ElementModel;
using System;
using System.Collections.Generic;
using System.Linq;
using P = Hl7.Fhir.ElementModel.Types;

namespace Hl7.Fhir.Validation
{

    // TODO MV 20190919: this is coming from FhirPath and should be here I think.
    public static class EqualityOperators
    {
        public static bool? IsEqualTo(this IEnumerable<ITypedElement> left, IEnumerable<ITypedElement> right, bool compareNames = false)
        {
            // If one or both of the arguments is an empty collection, a comparison operator will return an empty collection.
            // (though we might handle this more generally with the null-propagating functionality of the compiler
            // framework already.
            if (left is null || right is null) return null;

            var r = right.GetEnumerator();

            foreach (var l in left)
            {
                if (!r.MoveNext()) return false;        // number of children not the same
                var comparisonResult = l.IsEqualTo(r.Current, compareNames);
                if (comparisonResult == false) return false;
                if (comparisonResult == null) return null;
            }

            if (r.MoveNext())
                return false;   // number of children not the same
            else
                return true;
        }

        // Note that the Equals as defined by FhirPath/CQL only returns empty when one or both of the arguments
        // are empty. Otherwise, it will return either false or true. Uncomparable values (i.e. datetimes
        // with incompatible precisions) are mapped to false, as are arguments of different types.
        public static bool? IsEqualTo(this ITypedElement left, ITypedElement right, bool compareNames = false)
        {
            // If one or both of the arguments is an empty collection, a comparison operator will return an empty collection.
            // (though we might handle this more generally with the null-propagating functionality of the compiler
            // framework already.
            if (left is null || right is null) return null;

            // TODO: Merge with ElementNodeComparator.IsEqualTo

            if (compareNames && (left.Name != right.Name)) return false;

            var l = left.Value;
            var r = right.Value;

            // TODO: this is actually a cast with knowledge of FHIR->System mappings, we don't want that here anymore
            // Convert quantities
            if (left.InstanceType == "Quantity" && l == null)
                l = left.ParseQuantity();
            if (right.InstanceType == "Quantity" && r == null)
                r = right.ParseQuantity();

            // Compare primitives (or extended primitives)
            if (l != null && r != null && P.Any.TryConvert(l, out var lAny) && P.Any.TryConvert(r, out var rAny))
            {
                return IsEqualTo(lAny, rAny);
            }
            else if (l == null && r == null)
            {
                // Compare complex types (extensions on primitives are not compared, but handled (=ignored) above
                var childrenL = left!.Children();
                var childrenR = right!.Children();

                return childrenL.IsEqualTo(childrenR, compareNames: true);    // NOTE: Assumes null will never be returned when any() children exist
            }
            else
            {
                // Else, we're comparing a complex (without a value) to a primitive which (probably) should return false
                return false;
            }
        }

        public static bool? IsEqualTo(P.Any? left, P.Any? right)
        {
            // If one or both of the arguments is an empty collection, a comparison operator will return an empty collection.
            // (though we might handle this more generally with the null-propagating functionality of the compiler
            // framework already.
            if (left == null || right == null) return null;

            // Try to convert both operands to a common type if they differ.
            // When that fails, the CompareTo function on each type will itself
            // report an error if they cannot handle that.
            // TODO: in the end the engine/compiler will handle this and report an overload resolution fail
            tryCoerce(ref left, ref right);

            return left is P.ICqlEquatable cqle ? cqle.IsEqualTo(right) : null;
        }


        private static bool tryCoerce(ref P.Any left, ref P.Any right)
        {
            left = upcastOne(left, right);
            right = upcastOne(right, left);

            return left.GetType() == right.GetType();

            static P.Any upcastOne(P.Any value, P.Any other) =>
                value switch
                {
                    P.Integer _ when other is P.Long => (P.Long)(P.Integer)value,
                    P.Integer _ when other is P.Decimal => (P.Decimal)(P.Integer)value,
                    P.Integer _ when other is P.Quantity => (P.Quantity)(P.Integer)value,
                    P.Long _ when other is P.Decimal => (P.Decimal)(P.Long)value,
                    P.Long _ when other is P.Quantity => (P.Quantity)(P.Long)value,
                    P.Decimal _ when other is P.Quantity => (P.Quantity)(P.Decimal)value,
                    P.Date _ when other is P.DateTime => (P.DateTime)(P.Date)value,
                    _ => value
                };
        }


        public static bool Matches(this ITypedElement value, ITypedElement pattern)
        {
            if (value == null && pattern == null) return true;
            if (value == null || pattern == null) return false;

            if (!ValueEquality(value.Value, pattern.Value)) return false;

            // Compare the children.
            var valueChildren = value.Children();
            var patternChildren = pattern.Children();

            return patternChildren.All(patternChild => valueChildren.Any(valueChild =>
                  patternChild.Name == valueChild.Name && valueChild.Matches(patternChild)));

        }

        public static bool ValueEquality<T1, T2>(T1 val1, T2 val2)
        {
            // Compare the value
            if (val1 == null && val2 == null) return true;
            if (val1 == null || val2 == null) return false;

            try
            {
                // convert val2 to type of val1.
                T1 boxed2 = (T1)Convert.ChangeType(val2, typeof(T1));

                // compare now that same type.
                return val1.Equals(boxed2);
            }
            catch
            {
                return false;
            }
        }
    }
}
