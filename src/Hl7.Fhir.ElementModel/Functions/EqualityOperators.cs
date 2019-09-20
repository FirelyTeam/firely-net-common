/* 
 * Copyright (c) 2015, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

using Hl7.Fhir.Model.Primitives;
using System.Collections.Generic;
using System.Linq;

namespace Hl7.Fhir.ElementModel.Functions
{

    // TODO MV 20190919: this is coming from FhirPath and should be here I think.
    public static class EqualityOperators
    {
        public static bool IsEqualTo(this IEnumerable<ITypedElement> left, IEnumerable<ITypedElement> right, bool compareNames = false)
        {
            var r = right.GetEnumerator();

            foreach (var l in left)
            {
                if (!r.MoveNext()) return false;        // number of children not the same            
                if (!l.IsEqualTo(r.Current, compareNames)) return false;
            }

            if (r.MoveNext())
                return false;   // number of children not the same
            else
                return true;
        }

        public static bool IsEqualTo(this ITypedElement left, ITypedElement right, bool compareNames = false)
        {
            // TODO: Merge with ElementNodeComparator.IsEqualTo

            if (compareNames && (left.Name != right.Name)) return false;

            var l = left.Value;
            var r = right.Value;

            // TODO: this is actually a cast with knowledge of FHIR->System mappings, we don't want that here anymore
            // Convert quantities
            if (left.InstanceType == "Quantity" && l == null)
                l = Typecasts.ParseQuantity(left);
            if (right.InstanceType == "Quantity" && r == null)
                r = Typecasts.ParseQuantity(right);

            // Compare primitives (or extended primitives)
            if (l != null && r != null)
            {
                return Any.IsEqualTo(l, r);
            }
            else if (l == null && r == null)
            {
                // Compare complex types (extensions on primitives are not compared, but handled (=ignored) above
                var childrenL = left.Children();
                var childrenR = right.Children();

                return childrenL.IsEqualTo(childrenR, compareNames: true);    // NOTE: Assumes null will never be returned when any() children exist
            }
            else
            {
                // Else, we're comparing a complex (without a value) to a primitive which (probably) should return false
                return false;
            }
        }




        public static bool IsEquivalentTo(this IEnumerable<ITypedElement> left, IEnumerable<ITypedElement> right, bool compareNames = false)
        {
            var r = right.ToList();
            int count = 0;

            foreach (var l in left)
            {
                count += 1;
                if (!r.Any(ri => l.IsEquivalentTo(ri, compareNames))) return false;
            }

            if (count != r.Count)
                return false;
            else
                return true;
        }


        public static bool IsEquivalentTo(this ITypedElement left, ITypedElement right, bool compareNames = false)
        {
            if (compareNames && !namesAreEquivalent(left, right)) return false;

            var l = left.Value;
            var r = right.Value;

            // TODO: this is actually a cast with knowledge of FHIR->System mappings, we don't want that here anymore
            // Convert quantities
            if (left.InstanceType == "Quantity" && l == null)
                l = Typecasts.ParseQuantity(left);
            if (right.InstanceType == "Quantity" && r == null)
                r = Typecasts.ParseQuantity(right);

            // Compare primitives (or extended primitives)
            // TODO: Define IsEquivalentTo for ALL datatypes in ITypedElement.value and move to Support assembly + test
            // TODO: Define on object, so this switch can be removed here
            // TODO: Move this IsEquivalentTo to the ElementModel assembly
            // Maybe create an interface?
            if (l != null && r != null)
            {
                return Any.IsEquivalentTo(l, r);
            }
            else if (l == null && r == null)
            {
                // Compare complex types (extensions on primitives are not compared, but handled (=ignored) above
                var childrenL = left.Children();
                var childrenR = right.Children();

                return childrenL.IsEquivalentTo(childrenR, compareNames: true);    // NOTE: Assumes null will never be returned when any() children exist
            }
            else
            {
                // Else, we're comparing a complex (without a value) to a primitive which (probably) should return false
                return false;
            }

            bool namesAreEquivalent(ITypedElement le, ITypedElement ri)
            {
                if (le.Name == "id" && ri.Name == "id") return true;      // don't compare 'id' elements for equivalence
                if (le.Name != ri.Name) return false;

                return true;
            }
        }

        public static bool IsEquivalentTo(this string a, string b)
        {
            if (b == null) return false;

            a = a.Trim().ToLowerInvariant();
            b = b.Trim().ToLowerInvariant();

            return a == b;
            //    return String.Compare(a, b, CultureInfo.InvariantCulture,
            //CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols) == 0;
        }
    }
}
