using System;
using System.Collections.Generic;
using System.Linq;
using Hl7.Fhir.Model.Primitives;

namespace Hl7.Fhir.Language
{
    public abstract class TypeSpecifier
    {
        public const string SYSTEM_NAMESPACE = "System";
        public const string DOTNET_NAMESPACE = "DotNet";

        // From the Types section in Appendix B of the CQL reference
        public static readonly NamedTypeSpecifier Any = new NamedTypeSpecifier(SYSTEM_NAMESPACE, "Any");
        public static readonly NamedTypeSpecifier Boolean = new NamedTypeSpecifier(SYSTEM_NAMESPACE, "Boolean");
        public static readonly NamedTypeSpecifier Code = new NamedTypeSpecifier(SYSTEM_NAMESPACE, "Code");
        public static readonly NamedTypeSpecifier Concept = new NamedTypeSpecifier(SYSTEM_NAMESPACE, "Concept");
        public static readonly NamedTypeSpecifier Date = new NamedTypeSpecifier(SYSTEM_NAMESPACE, "Date");
        public static readonly NamedTypeSpecifier DateTime = new NamedTypeSpecifier(SYSTEM_NAMESPACE, "DateTime");
        public static readonly NamedTypeSpecifier Decimal = new NamedTypeSpecifier(SYSTEM_NAMESPACE, "Decimal");
        public static readonly NamedTypeSpecifier Integer = new NamedTypeSpecifier(SYSTEM_NAMESPACE, "Integer");
        public static readonly NamedTypeSpecifier Quantity = new NamedTypeSpecifier(SYSTEM_NAMESPACE, "Quantity");
        public static readonly NamedTypeSpecifier String = new NamedTypeSpecifier(SYSTEM_NAMESPACE, "String");
        public static readonly NamedTypeSpecifier Time = new NamedTypeSpecifier(SYSTEM_NAMESPACE, "Time");

        // This was added to represent the datatype with a single void element
        public static readonly NamedTypeSpecifier Void = new NamedTypeSpecifier(SYSTEM_NAMESPACE, "Void");

        public static readonly TypeSpecifier[] AllTypes = new[] { Any, Boolean, Code, Concept,
                Date, DateTime, Decimal, Integer, Quantity, String, Time };

        /// <summary>
        /// This is the list of supported types for the primitive values in ITypedElement.Value
        /// </summary>
        public static readonly TypeSpecifier[] PrimitiveTypes =
            new[] { Boolean, Date, DateTime, Decimal,    Integer, String, Time };    
    }
}
