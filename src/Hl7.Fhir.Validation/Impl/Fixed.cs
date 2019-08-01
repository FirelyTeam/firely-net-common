using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Validation.Schema;
using System;
using System.Linq;

namespace Hl7.Fhir.Validation.Impl
{
    public class Fixed : SimpleAssertion
    {
        private readonly ITypedElement _fixed;

        public Fixed(ITypedElement fixedValue)
        {
            this._fixed = fixedValue;
        }

        protected override string Key => "fixed[x]";

        protected override object Value => _fixed;

        public override Assertions Validate(ITypedElement input, ValidationContext vc)
        {
            if (!input.IsExactlyEqualTo(_fixed))
            {
                return Assertions.Failure;
                //v.Trace(outcome, $"Value is not exactly equal to fixed value '{toReadable(definition.Fixed)}'",
                //                        Issue.CONTENT_DOES_NOT_MATCH_FIXED_VALUE, instance);
            }

            return Assertions.Success;
        }
    }

    /// <summary>
    /// TODO MV Validation: This should be moved to projetc ElementModel, or Support
    /// </summary>
    internal static class TypeElementExtensions
    {
        public static bool IsExactlyEqualTo(this ITypedElement left, ITypedElement right)
        {
            if (left == null && right == null) return true;
            if (left == null || right == null) return false;

            if (!ValueEquality(left.Value, right.Value)) return false;

            // Compare the children.
            var childrenL = left.Children();
            var childrenR = right.Children();

            if (childrenL.Count() != childrenR.Count()) return false;

            return childrenL.Zip(childrenR,
                            (childL, childR) => childL.Name == childR.Name && childL.IsExactlyEqualTo(childR)).All(t => t);
        }


        private static bool ValueEquality<T1, T2>(T1 val1, T2 val2)
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
