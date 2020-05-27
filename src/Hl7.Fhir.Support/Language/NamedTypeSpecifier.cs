using System;
using System.Collections.Generic;
using Hl7.Fhir.Model.Primitives;
using Hl7.Fhir.Utility;

namespace Hl7.Fhir.Language
{
    public class NamedTypeSpecifier : TypeSpecifier, IEquatable<NamedTypeSpecifier>
    {
        internal NamedTypeSpecifier(string @namespace, string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Namespace = @namespace ?? throw new ArgumentNullException(nameof(@namespace));
        }

        public static NamedTypeSpecifier GetByName(string typeName) => GetByName(TypeSpecifier.SYSTEM_NAMESPACE, typeName);

        public static NamedTypeSpecifier GetByName(string @namespace, string typeName)
        {
            if (@namespace == null) throw new ArgumentNullException(nameof(@namespace));
            if (typeName == null) throw new ArgumentNullException(nameof(typeName));

            NamedTypeSpecifier result = null;

            if (@namespace == TypeSpecifier.SYSTEM_NAMESPACE)
                result = resolveSystemType(typeName);

            return result ?? new NamedTypeSpecifier(@namespace, typeName);

            static NamedTypeSpecifier resolveSystemType(string name)
            {
                return name switch
                {
                    "Any" => Any,
                    "Boolean" => Boolean,
                    "Code" => Code,
                    "Concept" => Concept,
                    "Date" => Date,
                    "DateTime" => DateTime,
                    "Decimal" => Decimal,
                    "Integer" => Integer,
                    "Integer64" => Integer64,
                    "Quantity" => Quantity,
                    "String" => String,
                    "Time" => Time,
                    "Void" => Void,
                    _ => null,
                };
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
                static string esc(string spec)
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
        public static NamedTypeSpecifier ForNativeType(Type dotNetType)
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
            else if (t<Coding>() || dotNetType.CanBeTreatedAsType(typeof(Enum)))
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

            // NOTE: Keep Any.TryConvertToSystemValue, NamedTypeSpecifier.TryGetNativeType and NamedTypeSpecifier.ForNativeType in sync
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

        public override bool Equals(object obj) => Equals(obj as NamedTypeSpecifier);
        public bool Equals(NamedTypeSpecifier other) => 
            other != null && 
            Name == other.Name && 
            Namespace == other.Namespace;

        public override int GetHashCode()
        {
            var hashCode = -179327946;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Namespace);
            return hashCode;
        }

        public static bool operator ==(NamedTypeSpecifier left, NamedTypeSpecifier right) => EqualityComparer<TypeSpecifier>.Default.Equals(left, right);
        public static bool operator !=(NamedTypeSpecifier left, NamedTypeSpecifier right) => !(left == right);
    }
}
