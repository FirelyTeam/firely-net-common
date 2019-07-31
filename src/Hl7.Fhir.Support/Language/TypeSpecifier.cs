using System;
using System.Collections.Generic;
using Hl7.Fhir.Model.Primitives;

namespace Hl7.Fhir.Language
{
    public class TypeSpecifier : IEquatable<TypeSpecifier>
    {
        public const string SYSTEM_NAMESPACE = "System";
        public const string DOTNET_NAMESPACE = "DotNet";

        public static class System
        {
            // From the Types section in Appendix B of the CQL reference
            public static readonly TypeSpecifier Any = new TypeSpecifier(SYSTEM_NAMESPACE, "Any");
            public static readonly TypeSpecifier Boolean = new TypeSpecifier(SYSTEM_NAMESPACE, "Boolean");
            public static readonly TypeSpecifier Code = new TypeSpecifier(SYSTEM_NAMESPACE, "Code");
            public static readonly TypeSpecifier Concept = new TypeSpecifier(SYSTEM_NAMESPACE, "Concept");
            public static readonly TypeSpecifier Date = new TypeSpecifier(SYSTEM_NAMESPACE, "Date");
            public static readonly TypeSpecifier DateTime = new TypeSpecifier(SYSTEM_NAMESPACE, "DateTime");
            public static readonly TypeSpecifier Decimal = new TypeSpecifier(SYSTEM_NAMESPACE, "Decimal");
            public static readonly TypeSpecifier Integer = new TypeSpecifier(SYSTEM_NAMESPACE, "Integer");
            public static readonly TypeSpecifier Quantity = new TypeSpecifier(SYSTEM_NAMESPACE, "Quantity");
            public static readonly TypeSpecifier String = new TypeSpecifier(SYSTEM_NAMESPACE, "String");
            public static readonly TypeSpecifier Time = new TypeSpecifier(SYSTEM_NAMESPACE, "Time");

            // This was added to represent the datatype with a single void element
            public static readonly TypeSpecifier Void = new TypeSpecifier(SYSTEM_NAMESPACE, "Void");

            public static readonly TypeSpecifier[] Types = new[] { Any, Boolean, Code, Concept,
                Date, DateTime, Decimal, Integer, Quantity, String, Time };
        }

        protected TypeSpecifier(string @namespace, string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Namespace = @namespace ?? throw new ArgumentNullException(nameof(@namespace));
        }

        public static TypeSpecifier GetByName(string @namespace, string typeName)
        {
            if (@namespace == null) throw new ArgumentNullException(nameof(@namespace));
            if (typeName == null) throw new ArgumentNullException(nameof(typeName));

            TypeSpecifier result = null;

            if (@namespace == SYSTEM_NAMESPACE)
                result = resolveSystemType(typeName);

            return result ?? new TypeSpecifier(@namespace, typeName);

            TypeSpecifier resolveSystemType(string name)
            {
                switch (name)
                {
                    case "Any": return TypeSpecifier.System.Any;
                    case "Boolean": return TypeSpecifier.System.Boolean;
                    case "Code": return TypeSpecifier.System.Code;
                    case "Concept": return TypeSpecifier.System.Concept;
                    case "Date": return TypeSpecifier.System.Date;
                    case "DateTime": return TypeSpecifier.System.DateTime;
                    case "Decimal": return TypeSpecifier.System.Decimal;
                    case "Integer": return TypeSpecifier.System.Integer;
                    case "Quantity": return TypeSpecifier.System.Quantity;
                    case "String": return TypeSpecifier.System.String;
                    case "Time": return TypeSpecifier.System.Time;
                    case "Void": return TypeSpecifier.System.Void;
                    default:
                        return null;
                }
            }
        }

        public string Name { get; protected set; }
        public string Namespace { get; protected set; }

        public override string ToString() => FullName;

        public string FullName
        {
            get
            {
                return $"{esc(Namespace)}.{esc(Name)}";
                string esc(string spec) => spec.Contains(".") ? $"`{spec}`" : spec;
            }
        }

        /// <summary>
        /// Maps a C# type to a known TypeSpecifier.
        /// </summary>
        /// <param name="dotNetType">Value to determine the type for.</param>
        /// <returns></returns>
        public static TypeSpecifier ForNativeType(Type dotNetType)
        {
            if (dotNetType == null) throw new ArgumentNullException(nameof(dotNetType));

            // NOTE: Keep Any.TryConvertToSystemValue, TypeSpecifier.TryGetNativeType and TypeSpecifier.ForNativeType in sync
            if (t<bool>())
                return System.Boolean;
            else if (t<int>() || t<short>() || t<long>() || t<ushort>() || t<uint>() || t<ulong>())
                return System.Integer;
            else if (t<PartialTime>())
                return System.Time;
            else if (t<PartialDate>())
                return System.Date;
            else if (t<PartialDateTime>() || t<DateTimeOffset>())
                return System.DateTime;
            else if (t<float>() || t<double>() || t<decimal>())
                return System.Decimal;
            else if (t<string>() || t<char>() || t<Uri>())
                return System.String;
            else if (t<Quantity>())
                return System.Quantity;
            else if (t<Coding>())
                return System.Code;
            else if (t<Concept>())
                return System.Concept;
            else if (t<object>())
                return System.Any;
            else
                return GetByName(DOTNET_NAMESPACE, dotNetType.ToString());

            bool t<A>() => dotNetType == typeof(A);
        }
       
        public Type GetNativeType()
        {
            if (!TryGetNativeType(out var nativeType))
                throw new NotSupportedException($"There is no known .NET type for {FullName}.");
            else
                return nativeType;
        }


        /// <summary>
        /// Returns the .NET type used to represent this TypeSpecifier.
        /// </summary>
        /// <returns></returns>
        public bool TryGetNativeType(out Type result)
        {
            result = Namespace == DOTNET_NAMESPACE ? getFromDotNet() : getFromKnownSystemTypes();
            return result != null;
 
            Type getFromDotNet() => Type.GetType(Name, throwOnError: false);

            // NOTE: Keep Any.TryConvertToSystemValue, TypeSpecifier.TryGetNativeType and TypeSpecifier.ForNativeType in sync
            Type getFromKnownSystemTypes()
            {
                if (this == System.Any)
                    return typeof(object);
                else if (this == System.Boolean)
                    return typeof(bool);
                else if (this == System.Code)
                    return typeof(Coding);
                else if (this == System.Concept)
                    return typeof(Concept);
                else if (this == System.Date)
                    return typeof(PartialDate);
                else if (this == System.DateTime)
                    return typeof(PartialDateTime);
                else if (this == System.Decimal)
                    return typeof(decimal);
                else if (this == System.Integer)
                    return typeof(long);
                else if (this == System.Quantity)
                    return typeof(Quantity);
                else if (this == System.String)
                    return typeof(string);
                else if (this == System.Time)
                    return typeof(PartialTime);
                else
                    return null;
            }
        }

        public override bool Equals(object obj) => Equals(obj as TypeSpecifier);
        public bool Equals(TypeSpecifier other) => other != null && Name == other.Name && Namespace == other.Namespace;

        public override int GetHashCode()
        {
            var hashCode = -179327946;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Namespace);
            return hashCode;
        }

        public static bool operator ==(TypeSpecifier left, TypeSpecifier right) => EqualityComparer<TypeSpecifier>.Default.Equals(left, right);
        public static bool operator !=(TypeSpecifier left, TypeSpecifier right) => !(left == right);
    }
}
