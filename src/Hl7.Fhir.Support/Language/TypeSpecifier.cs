using Hl7.Fhir.Model.Primitives;
using System;
using System.Collections.Generic;

namespace Hl7.Fhir.Language
{
    public class TypeSpecifier : IEquatable<TypeSpecifier>
    {
        public const string SYSTEM_NAMESPACE = "System";
        public const string DOTNET_NAMESPACE = "DotNet";

        // From the Types section in Appendix B of the CQL reference
        public static readonly TypeSpecifier Any = new TypeSpecifier(SYSTEM_NAMESPACE, "Any");
        public static readonly TypeSpecifier Boolean = new TypeSpecifier(SYSTEM_NAMESPACE, "Boolean");
        public static readonly TypeSpecifier Code = new TypeSpecifier(SYSTEM_NAMESPACE, "Code");
        public static readonly TypeSpecifier Concept = new TypeSpecifier(SYSTEM_NAMESPACE, "Concept");
        public static readonly TypeSpecifier Date = new TypeSpecifier(SYSTEM_NAMESPACE, "Date");
        public static readonly TypeSpecifier DateTime = new TypeSpecifier(SYSTEM_NAMESPACE, "DateTime");
        public static readonly TypeSpecifier Decimal = new TypeSpecifier(SYSTEM_NAMESPACE, "Decimal");
        public static readonly TypeSpecifier Integer = new TypeSpecifier(SYSTEM_NAMESPACE, "Integer");
        public static readonly TypeSpecifier Integer64 = new TypeSpecifier(SYSTEM_NAMESPACE, "Integer64");
        public static readonly TypeSpecifier Quantity = new TypeSpecifier(SYSTEM_NAMESPACE, "Quantity");
        public static readonly TypeSpecifier String = new TypeSpecifier(SYSTEM_NAMESPACE, "String");
        public static readonly TypeSpecifier Time = new TypeSpecifier(SYSTEM_NAMESPACE, "Time");

        // This was added to represent the datatype with a single void element
        public static readonly TypeSpecifier Void = new TypeSpecifier(SYSTEM_NAMESPACE, "Void");

        public static readonly TypeSpecifier[] AllTypes = new[] { Any, Boolean, Code, Concept,
                Date, DateTime, Decimal, Integer, Integer64, Quantity, String, Time };

        /// <summary>
        /// This is the list of supported types for the primitive values in ITypedElement.Value
        /// </summary>
        public static readonly TypeSpecifier[] PrimitiveTypes =
            new[] { Boolean, Date, DateTime, Decimal, Integer, Integer64, String, Time };


        protected TypeSpecifier(string @namespace, string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Namespace = @namespace ?? throw new ArgumentNullException(nameof(@namespace));
        }

        public static TypeSpecifier GetByName(string typeName) => GetByName(SYSTEM_NAMESPACE, typeName);


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
                    case "Any": return Any;
                    case "Boolean": return Boolean;
                    case "Code": return Code;
                    case "Concept": return Concept;
                    case "Date": return Date;
                    case "DateTime": return DateTime;
                    case "Decimal": return Decimal;
                    case "Integer": return Integer;
                    case "Integer64": return Integer64;
                    case "Quantity": return Quantity;
                    case "String": return String;
                    case "Time": return Time;
                    case "Void": return Void;
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
                string esc(string spec)
                {
                    if (!spec.Contains(".") && !spec.Contains("`")) return spec;

                    spec = spec.Replace("`", "\\`");
                    return $"`{spec}`";
                }
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
                return Boolean;
            else if (t<int>() || t<short>() || t<ushort>() || t<uint>())
                return Integer;
            else if (t<long>() || t<ulong>())
                return Integer64;
            else if (t<PartialTime>())
                return Time;
            else if (t<PartialDate>())
                return Date;
            else if (t<PartialDateTime>() || t<DateTimeOffset>())
                return DateTime;
            else if (t<float>() || t<double>() || t<decimal>())
                return Decimal;
            else if (t<string>() || t<char>() || t<Uri>())
                return String;
            else if (t<Quantity>())
                return Quantity;
            else if (t<Coding>())
                return Code;
            else if (t<Concept>())
                return Concept;
#pragma warning disable IDE0046 // Convert to conditional expression
            else if (t<object>())
#pragma warning restore IDE0046 // Convert to conditional expression
                return Any;
            else
                return GetByName(DOTNET_NAMESPACE, dotNetType.ToString());

            bool t<A>() => dotNetType == typeof(A);

            //TypeSpecifier getDotNetNamespace()
            //{
            //    var tn = dotNetType.ToString();
            //    var pos = tn.LastIndexOf('.');

            //    if (pos == -1) return GetByName(DOTNET_NAMESPACE, dotNetType.ToString());

            //    var ns = tn.Substring(0, pos);
            //    var n = tn.Substring(pos + 1);
            //    return GetByName(DOTNET_NAMESPACE + "." + ns, n);
            //}
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
                if (this == Any)
                    return typeof(object);
                else if (this == Boolean)
                    return typeof(bool);
                else if (this == Code)
                    return typeof(Coding);
                else if (this == Concept)
                    return typeof(Concept);
                else if (this == Date)
                    return typeof(PartialDate);
                else if (this == DateTime)
                    return typeof(PartialDateTime);
                else if (this == Decimal)
                    return typeof(decimal);
                else if (this == Integer)
                    return typeof(int);
                else if (this == Integer64)
                    return typeof(long);
                else if (this == Quantity)
                    return typeof(Quantity);
                else if (this == String)
                    return typeof(string);
#pragma warning disable IDE0046 // Convert to conditional expression
                else if (this == Time)
#pragma warning restore IDE0046 // Convert to conditional expression
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
