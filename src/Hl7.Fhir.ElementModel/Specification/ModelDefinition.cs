/* 
 * Copyright (c) 2021, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/fhir-net-api/blob/master/LICENSE
 */

using Hl7.Fhir.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Hl7.Fhir.Specification
{
    /// <summary>
    /// Represents the comprehensive set of all definitional artifacts of a model used
    /// by the parsers, validators, mappers and other components that need metadata about
    /// a model to function properly.
    /// </summary>
    /// <remarks>Examples of a model are FHIR STU3, FHIR R5, CDA R1, CDA R2, etcetera.</remarks>
    public class ModelDefinition : IAnnotated, IReadOnlyDictionary<string, TypeDefinition>
    {
        protected static readonly AnnotationList EMPTY_ANNOTATIONS = new AnnotationList();

        public ModelDefinition(string name, string version, IEnumerable<TypeDefinition> types, DeferredModelInitializer<AnnotationList> annotationsInitializer)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Version = version ?? throw new ArgumentNullException(nameof(version));
            _annotationsInitializer = annotationsInitializer ?? throw new ArgumentNullException(nameof(annotationsInitializer));
            if (types is null) throw new ArgumentNullException(nameof(types));

            foreach (var type in types) type.DeclaringModel = this;
            _types = types.ToDictionary(md => md.Name);
        }

        public ModelDefinition(string name, string version, IEnumerable<TypeDefinition> types, AnnotationList annotations = null)
                : this(name, version, types, _ => annotations ?? EMPTY_ANNOTATIONS)
        {
            // no additional initializations
        }

        public override string ToString() => $"{Name}-{Version}";

        // TODO: Can we build a system that guarantees that there are no deadlocks/loops:
        // -> ModelDefinition.Annotations initializer calls TypeDefinition.Annotations, typedef initializer calls modeldef.annotations....
        // separate "annotations" from "skratchpad"?  (= storage/indices for quick access to say list of conformance resources)
        // or just forbid calling properties that are lazy-initialized on an object before that object has been initialized by the model space?
        protected virtual void Initialize()
        {
            _annotations = _annotationsInitializer(this.DeclaringSpace);
        }

        public ModelSpace DeclaringSpace { get; protected internal set; }

        public bool TryGetType(string typeName, out TypeDefinition definition) => this.TryGetValue(typeName, out definition);

        /// <summary>
        /// An name given to this model, e.g. "FHIR". Is also used as a namespace
        /// prefix to make type names unique, e.g. E.g. "CDA" in the fully
        /// qualified type name "CDA.ST".
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The version of the model this class represents, e.g. "3.0.1"
        /// </summary>
        public string Version { get; }

        /// <summary>
        /// All the types defined in this model
        /// </summary>
        public IReadOnlyCollection<TypeDefinition> Types => _types.Values;
        private readonly Dictionary<string, TypeDefinition> _types = null;

        /// <summary>
        /// Collection of additional model information.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public IEnumerable<object> Annotations(Type type)
        {
            LazyInitializer.EnsureInitialized(ref _annotations, () => _annotationsInitializer(this.DeclaringSpace));
            return _annotations.OfType(type);
        }

        private AnnotationList _annotations;
        private readonly DeferredModelInitializer<AnnotationList> _annotationsInitializer;

        #region IReadOnlyDictionary
        public IEnumerable<string> Keys => _types.Keys;
        public IEnumerable<TypeDefinition> Values => _types.Values;
        public TypeDefinition this[string typeName] => _types[typeName];
        public int Count => _types.Count;
        public bool ContainsKey(string key) => _types.ContainsKey(key);
        public bool TryGetValue(string key, out TypeDefinition value) => _types.TryGetValue(key, out value);
        #endregion

        #region IEnumerable
        IEnumerator IEnumerable.GetEnumerator() => _types.GetEnumerator();
        public IEnumerator<KeyValuePair<string, TypeDefinition>> GetEnumerator() => _types.GetEnumerator();
        #endregion

        public Lazy<TypeDefinition> LazyTypeResolver(string typename)
        {
            TypeDefinition factory() => TryGetType(typename, out var typeDef) ? typeDef : default;
            return new Lazy<TypeDefinition>(factory);
        }
    }
}
