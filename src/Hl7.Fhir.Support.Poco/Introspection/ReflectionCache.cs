/* 
 * Copyright (c) 2020, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-net-sdk/master/LICENSE
 */

using Hl7.Fhir.Utility;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Hl7.Fhir.Introspection
{
    [Obsolete("These classes have never been finalized, tested or prepared for public use. Please use ClassMapping/PropertyMapping instead.")]
    public class ReflectionCache
    {
        private static readonly ConcurrentDictionary<Type, ReflectedType> _mappings = new ConcurrentDictionary<Type, ReflectedType>();

        public static readonly ReflectionCache Current = new ReflectionCache();

        public ReflectedType Get(Type typeToReflect)
        {
            if (typeToReflect is null)
            {
                throw new ArgumentNullException(nameof(typeToReflect));
            }

            return _mappings.GetOrAdd(typeToReflect, t => new ReflectedType(t));
        }
    }

    [Obsolete("These classes have never been finalized, tested or prepared for public use. Please use ClassMapping/PropertyMapping instead.")]
    public class ReflectedType
    {
        public ReflectedType(Type typeToReflect)
        {
            Reflected = typeToReflect ?? throw new ArgumentNullException(nameof(typeToReflect));

            _attributes = new Lazy<IReadOnlyCollection<Attribute>>(getAttributes);
            _properties = new Lazy<IReadOnlyCollection<ReflectedProperty>>(getProperties);
            _propertiesByName = new Lazy<IReadOnlyDictionary<string, ReflectedProperty>>(() => getPropertiesByName(_properties));

            List<Attribute> getAttributes() => ReflectionHelper.GetAttributes(Reflected).ToList();
            List<ReflectedProperty> getProperties() =>
                ReflectionHelper
                    .FindPublicProperties(Reflected)
                    .Select(pi => new ReflectedProperty(pi))
                    .ToList();
            Dictionary<string, ReflectedProperty> getPropertiesByName(Lazy<IReadOnlyCollection<ReflectedProperty>> props) =>
                    props.Value.ToDictionary(rp => rp.Name);
        }

        public Type Reflected { get; private set; }

        private readonly Lazy<IReadOnlyCollection<Attribute>> _attributes;
        public IReadOnlyCollection<Attribute> Attributes => _attributes.Value;

        private readonly Lazy<IReadOnlyCollection<ReflectedProperty>> _properties;
        private readonly Lazy<IReadOnlyDictionary<string, ReflectedProperty>> _propertiesByName;
        public IReadOnlyCollection<ReflectedProperty> Properties => _properties.Value;

        public bool TryGetProperty(string name, out ReflectedProperty prop) => _propertiesByName.Value.TryGetValue(name, out prop);
    }

    [Obsolete("These classes have never been finalized, tested or prepared for public use. Please use ClassMapping/PropertyMapping instead.")]
    public class ReflectedProperty
    {
        public ReflectedProperty(PropertyInfo propToReflect)
        {
            Reflected = propToReflect ?? throw new ArgumentNullException(nameof(propToReflect));

            _attributes = new Lazy<IReadOnlyCollection<Attribute>>(getAttributes);

            _getter = new Lazy<Func<object, object>>(Reflected.GetValueGetter);
            _setter = new Lazy<Action<object, object>>(Reflected.GetValueSetter);

            List<Attribute> getAttributes() => ReflectionHelper.GetAttributes(Reflected).ToList();
        }

        public PropertyInfo Reflected { get; private set; }

        public string Name => Reflected.Name;

        private readonly Lazy<IReadOnlyCollection<Attribute>> _attributes;

        public IReadOnlyCollection<Attribute> Attributes => _attributes.Value;

        private readonly Lazy<Func<object, object>> _getter;

        public object Get(object instance) => _getter.Value(instance);

        private readonly Lazy<Action<object, object>> _setter;

        public void Set(object instance, object value) => _setter.Value(instance, value);
    }

}
