/* 
 * Copyright (c) 2015, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */
using Hl7.Fhir.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hl7.Fhir.ElementModel.Functions
{
    internal static class Typecasts
    {
        public delegate object Cast(object source);

        private static object id(object source) => source;

        private static Cast makeNativeCast(Type to) =>
            source => Convert.ChangeType(source, to);

        private static ITypedElement any2ValueProvider(object source) => ElementNode.ForPrimitive(source);

        private static IEnumerable<ITypedElement> any2List(object source) => ElementNode.CreateList(source);

        private static object tryQuantity(object source)
        {
            if (source is ITypedElement element)
            {
                if (element.InstanceType == "Quantity")
                {
                    // Need to downcast from a FHIR Quantity to a System.Quantity
                    return ParseQuantity(element);
                }
                else
                    throw new InvalidCastException($"Cannot convert from '{element.InstanceType}' to Quantity");
            }

            throw new InvalidCastException($"Cannot convert from '{source.GetType().Name}' to Quantity");


        }


        internal static Fhir.Model.Primitives.Quantity? ParseQuantity(ITypedElement qe)
        {
            var value = qe.Children("value").SingleOrDefault()?.Value as decimal?;
            if (value == null) return null;

            var unit = qe.Children("code").SingleOrDefault()?.Value as string;
            return new Fhir.Model.Primitives.Quantity(value.Value, unit);
        }

        private static Cast getImplicitCast(Type from, Type to)
        {
            if (to == typeof(object)) return id;
            if (from.CanBeTreatedAsType(to)) return id;

            if (to == typeof(Fhir.Model.Primitives.Quantity) && from.CanBeTreatedAsType(typeof(ITypedElement))) return tryQuantity;
            if (to == typeof(ITypedElement) && (!from.CanBeTreatedAsType(typeof(IEnumerable<ITypedElement>)))) return any2ValueProvider;
            if (to == typeof(IEnumerable<ITypedElement>)) return any2List;

            if (from == typeof(long) && (to == typeof(decimal) || to == typeof(decimal?))) return makeNativeCast(typeof(decimal));
            if (from == typeof(long?) && to == typeof(decimal?)) return makeNativeCast(typeof(decimal?));
            return null;
        }

        /// <summary>
        /// This will unpack the instance 
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="to">The level to unbox to.</param>
        /// <returns></returns>
        /// <remarks>The level of unboxing is specified using a type. The highest level
        /// being an <see cref="IEnumerable{ITypedElement}"/> followed by 
        /// <see cref="ITypedElement"/> followed by a primitive runtime type.
        /// </remarks>
        internal static object UnboxTo(object instance, Type to)
        {
            if (instance == null) return null;

            if (instance is IEnumerable<ITypedElement> list)
            {
                if (to.CanBeTreatedAsType(typeof(IEnumerable<ITypedElement>))) return instance;

                if (!list.Any()) return null;
                if (list.Count() == 1)
                    instance = list.Single();
            }

            if (instance is ITypedElement element)
            {
                if (to.CanBeTreatedAsType(typeof(ITypedElement))) return instance;

                if (element.Value != null)
                    instance = element.Value;
            }

            return instance;
        }

        public static bool CanCastTo(object source, Type to)
        {
            if (source == null)
                return to.IsNullable();

            var from = UnboxTo(source, to);
            return from == null ? to.IsNullable() : getImplicitCast(from.GetType(), to) != null;
        }

        public static bool CanCastTo(Type from, Type to) => getImplicitCast(from, to) != null;

        public static T CastTo<T>(object source) => (T)CastTo(source, typeof(T));

        public static object CastTo(object source, Type to)
        {
            if (source != null)
            {
                if (source.GetType().CanBeTreatedAsType(to)) return source;  // for efficiency

                source = UnboxTo(source, to);

                if (source != null)
                {
                    Cast cast = getImplicitCast(source.GetType(), to);

                    if (cast == null)
                    {
                        var message = "Cannot cast from '{0}' to '{1}'".FormatWith(Typecasts.ReadableFhirPathName(source),
                          Typecasts.ReadableTypeName(to));
                        throw new InvalidCastException(message);
                    }

                    return cast(source);
                }
            }

            //if source == null, or unboxed source == null....
            if (to == typeof(IEnumerable<ITypedElement>))
                return ElementNode.EmptyList;
            if (to.IsNullable())
                return null;
            else
                throw new InvalidCastException("Cannot cast a null value to non-nullable type '{0}'".FormatWith(to.Name));
        }

        public static bool IsNullable(this Type t)
        {
            if (!t.IsAValueType()) return true; // ref-type
            if (Nullable.GetUnderlyingType(t) != null) return true; // Nullable<T>
            return false; // value-type
        }

        public static string ReadableFhirPathName(object value)
        {
            if (value is IEnumerable<ITypedElement> ete)
            {
                var values = ete.ToList();
                var types = ete.Select(te => ReadableFhirPathName(te)).Distinct();

                return values.Count > 1 ? "collection of " + String.Join("/", types) : types.Single();
            }
            else if (value is ITypedElement te)
                return te.InstanceType;
            else
                return value.GetType().Name;
        }

        public static string ReadableTypeName(Type t)
        {
            if (t.CanBeTreatedAsType(typeof(IEnumerable<ITypedElement>)))
                return "collection";
            else if (t.CanBeTreatedAsType(typeof(ITypedElement)))
                return "any type";
            else
                return t.Name;
        }
    }

}
