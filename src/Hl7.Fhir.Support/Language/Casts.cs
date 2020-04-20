using System;
using System.Collections.Generic;
using System.Linq;

namespace Hl7.Fhir.Language
{
    public abstract class Cast
    {
        public abstract IEnumerable<Cast> TryCast(Cast parent, TypeSpecifier from, TypeSpecifier to);

        public static readonly IEnumerable<Cast> Empty = Enumerable.Empty<Cast>();

        public IEnumerable<Cast> Me => new[] { this };

        public IEnumerable<Cast> PrependIf(Func<IEnumerable<Cast>> caster)
        {
            var castResult = caster();
            return castResult.Any() ? Me.Union(castResult) : Empty;
        }
    }

    /// <summary>
    /// Cast to a supertype
    /// </summary>
    public class Upcast : Cast
    {
        public override IEnumerable<Cast> TryCast(Cast parent, TypeSpecifier from, TypeSpecifier to) =>
            to is NamedTypeSpecifier nts && nts == TypeSpecifier.Any ? Me : Empty;
    }

    public class CastCollection : Cast
    {
        public readonly static CastCollection AllCasts = new CastCollection(
            new Cast[] {
                new Upcast(),
                new ListConversion(),
                new ListDemotion(), new ListPromotion(),
                new ChoicePromotion(), new OptionPromotion(), new OptionDemotion(),
                new CompatibleCast() });

        public IReadOnlyCollection<Cast> Casts { get; private set; }

        public CastCollection(IEnumerable<Cast> casts)
        {
            Casts = casts.ToReadOnlyCollection() ?? throw new ArgumentNullException(nameof(casts));
        }

        public override IEnumerable<Cast> TryCast(Cast parent, TypeSpecifier from, TypeSpecifier to)
        {
            foreach (var cast in Casts)
            {
                var castResult = cast.TryCast(this, from, to);
                if (castResult.Any()) return castResult;
            }

            return Empty;
        }

        public void test(decimal i) => throw new NotImplementedException();
    }


    /// <summary>
    /// Demote List of T to T
    /// </summary>
    public class ListDemotion : Cast
    {
        public override IEnumerable<Cast> TryCast(Cast parent, TypeSpecifier from, TypeSpecifier to)
        {
            // We can remove an outer List, and then try to cast the inner T to the desired type
            if (from is ListTypeSpecifier lts)
            {
                var innerType = lts.ElementType;
                return PrependIf(() => parent.TryCast(parent, innerType, to));
            }

            return Empty;
        }
    }

    /// <summary>
    /// Cast List of T to List of U
    /// </summary>
    public class ListConversion : Cast
    {
        public override IEnumerable<Cast> TryCast(Cast parent, TypeSpecifier from, TypeSpecifier to)
        {
            if (from is ListTypeSpecifier ltsFrom && to is ListTypeSpecifier ltsTo)
            {
                var innerTypeFrom = ltsFrom.ElementType;
                var innerTypeTo = ltsTo.ElementType;
                return PrependIf(() => parent.TryCast(parent, innerTypeFrom, innerTypeTo));
            }

            return Empty;
        }
    }

    /// <summary>
    /// Promote T to a List of T
    /// </summary>
    public class ListPromotion : Cast
    {
        public override IEnumerable<Cast> TryCast(Cast parent, TypeSpecifier from, TypeSpecifier to)
        {
            // We can remove an outer List, and then try to cast the inner T to the desired type
            if (to is ListTypeSpecifier lts)
            {
                var innerType = lts.ElementType;
                return PrependIf(() => parent.TryCast(parent, from, innerType));
            }

            return Empty;
        }

    }

    /// <summary>
    /// Cast T to a choice containing T
    /// </summary>
    public class ChoicePromotion : Cast
    {
        public override IEnumerable<Cast> TryCast(Cast parent, TypeSpecifier from, TypeSpecifier to)
        {
            if (to is ChoiceTypeSpecifier cts)
            {
                foreach (var choice in cts.Choices)
                {
                    var result = PrependIf(() => parent.TryCast(parent, from, choice));
                    if (result.Any()) return result;
                }
            }

            return Empty;
        }
    }


    /// <summary>
    /// Cast T to a T?
    /// </summary>
    public class OptionPromotion : Cast
    {
        public override IEnumerable<Cast> TryCast(Cast parent, TypeSpecifier from, TypeSpecifier to)
        {
            if (to is OptionTypeSpecifier ots)
            {
                var innerType = ots.Option;
                return PrependIf(() => parent.TryCast(parent, from, innerType));
            }

            return Empty;
        }
    }


    /// <summary>
    /// Cast T? to a T
    /// </summary>
    public class OptionDemotion : Cast
    {
        public override IEnumerable<Cast> TryCast(Cast parent, TypeSpecifier from, TypeSpecifier to)
        {
            if (from is OptionTypeSpecifier ots)
            {
                var innerType = ots.Option;
                return PrependIf(() => parent.TryCast(parent, innerType, to));
            }

            return Empty;
        }
    }


    /// <summary>
    /// Cast T to a compatible type
    /// </summary>
    public class CompatibleCast : Cast
    {
        public override IEnumerable<Cast> TryCast(Cast parent, TypeSpecifier from, TypeSpecifier to)
        {

            if (from is NamedTypeSpecifier fromName && to is NamedTypeSpecifier toName)
            {
                if (fromName == TypeSpecifier.Integer && toName == TypeSpecifier.Decimal ||
                    fromName == TypeSpecifier.Integer && toName == TypeSpecifier.Quantity ||
                    fromName == TypeSpecifier.Decimal && toName == TypeSpecifier.Quantity ||
                    fromName == TypeSpecifier.Date && toName == TypeSpecifier.DateTime ||
                    fromName == TypeSpecifier.Code && toName == TypeSpecifier.Concept)
                    return Me;
            }

            return Empty;
        }
    }
}
