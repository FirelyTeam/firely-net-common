/* 
 * Copyright (c) 2018, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/fhir-net-api/blob/master/LICENSE
 */

using Hl7.Fhir.Support.Utility;
using Hl7.Fhir.Utility;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Hl7.Fhir.Specification
{
    public delegate T DeferredModelInitializer<T>(ModelSpace space);

    public class ModelSpace
    {
        public IReadOnlyCollection<ModelDefinition> Models => _models;
        private readonly List<ModelDefinition> _models = new List<ModelDefinition>();

        // todo private list of JITted list definitions

        public bool TryGetModel(string modelName, string modelVersion, out ModelDefinition model)
        {
            model = Models.SingleOrDefault(m => m.Name == modelName && m.Version == modelVersion);
            return model != null;
        }

        public bool TryGetType(string modelName, string modelVersion, string typeName, out TypeDefinition definition)
        {

            if (TryGetModel(modelName, modelVersion, out var model))
                return model.TryGetType(typeName, out definition);

            definition = null;
            return false;
        }

        public bool TryGetType(ModelDefinition model, string typeName, out TypeDefinition definition) =>
            model.TryGetType(typeName, out definition);

        public TypeDefinition GetListTypeDefinition(TypeDefinition elementType)
        {
            throw new NotImplementedException();
        }

        public TypeDefinition GetUnionTypeDefinition(TypeDefinition elementType)
        {
            throw new NotImplementedException();
        }

        public void Add(ModelDefinition definition)
        {
            if (TryGetModel(definition.Name, definition.Version, out _))
                throw new ArgumentException($"ModelSpace already contains a model with name '{definition.Name}' and version '{definition.Version}'.");

            definition.DeclaringSpace = this;
            _models.Add(definition);
        }
    }

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
                : this(name,version,types, _ => annotations ?? EMPTY_ANNOTATIONS)
        {
            // no additional initializations
        }


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
        public IReadOnlyCollection<TypeDefinition> Types => _types.Values.ToReadOnlyCollection();
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
    }


    // TODO: Generate code in poco-modelinfo that references some of these using readonlies (e.g. ModelInfo.Patient, etc.)
    // TODO: Do the above for the system typespace too
    public abstract class TypeDefinition : IAnnotated
    {
        protected static readonly Lazy<AnnotationList> EMPTY_ANNOTATION_LIST = new Lazy<AnnotationList>(() => new AnnotationList());

        public TypeDefinition(string name, Lazy<TypeDefinition> @base) :
            this(name, @base, annotations: null, isAbstract: false, isOrdered: false, binding: null, identifier:null)
        {

        }

        public TypeDefinition(string name, Lazy<TypeDefinition> @base, Lazy<AnnotationList> annotations, 
            bool isAbstract, bool isOrdered, string binding, string identifier)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            _delayedBase = @base;
            _delayedAnnotations = annotations ?? EMPTY_ANNOTATION_LIST;
            IsAbstract = isAbstract;
            IsOrdered = isOrdered;
            Binding = binding;
            Identifier = identifier;
        }

        /// <summary>
        /// The unique name for the type (within this model).
        /// </summary>
        public string Name { get; }

        public ModelDefinition DeclaringModel { get; protected internal set; }

        public TypeDefinition Base => _delayedBase?.Value;
        private readonly Lazy<TypeDefinition> _delayedBase;

        public bool IsAbstract { get; }
        public bool IsOrdered { get; }
        public string Binding { get; }

        /// <summary>
        /// A globally unique identifier, in FHIR this would be the canonical.
        /// </summary>
        public string Identifier { get; }

        /// <summary>
        /// Collection of additional model-specific information for this type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public IEnumerable<object> Annotations(Type type) => _delayedAnnotations.Value.OfType(type);
        private readonly Lazy<AnnotationList> _delayedAnnotations;
    }


    public class PrimitiveTypeDefinition : TypeDefinition
    {
        public PrimitiveTypeDefinition(string name, Lazy<TypeDefinition> @base) : base(name, @base)
        {
        }

        public PrimitiveTypeDefinition(string name, Lazy<TypeDefinition> @base, Lazy<AnnotationList> annotations, 
            bool isAbstract, bool isOrdered, string binding, string identifier) : base(name, @base, annotations, isAbstract, isOrdered, binding, identifier)
        {
        }
    }

    public class ComplexTypeDefinition : TypeDefinition
    {
        public ComplexTypeDefinition(string name, Lazy<TypeDefinition> @base, Lazy<IReadOnlyList<MemberDefinition>> members): base(name, @base)
        {
            _delayedMembers = members ?? throw new ArgumentNullException(nameof(members));
        }

        public ComplexTypeDefinition(string name, Lazy<TypeDefinition> @base, Lazy<IReadOnlyList<MemberDefinition>> members,
            Lazy<AnnotationList> annotations,
            bool isAbstract, bool isOrdered, string binding, string identifier) : base(name, @base, annotations, isAbstract, isOrdered, binding, identifier)
        {
            _delayedMembers = members ?? throw new ArgumentNullException(nameof(members));
        }

        public IReadOnlyList<MemberDefinition> Members => _delayedMembers.Value;
           
        private readonly Lazy<IReadOnlyList<MemberDefinition>> _delayedMembers;
    }

    public class MemberDefinition : IAnnotated
    {
        protected static readonly AnnotationList EMPTY_ANNOTATION_LIST = new AnnotationList();

        public MemberDefinition(string name, TypeDefinition type, AnnotationList annotations)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Type = type ?? throw new ArgumentNullException(nameof(type));
            _annotations = annotations ?? EMPTY_ANNOTATION_LIST;
        }

        public string Name { get; }

        public TypeDefinition Type { get; }

        public IEnumerable<object> Annotations(Type type) => _annotations.OfType(type);
        private readonly AnnotationList _annotations;
    }


    public class UnionTypeDefinition : TypeDefinition
    {
        public UnionTypeDefinition(string name, Lazy<TypeDefinition> @base, Lazy<IReadOnlyCollection<TypeDefinition>> members, Lazy<AnnotationList> annotations,
            bool isAbstract, bool isOrdered, string binding, string identifier) : base(name, @base, annotations, isAbstract, isOrdered, binding, identifier)
        {
            _delayedMemberTypes = members ?? throw new ArgumentNullException(nameof(members));
        }

        public UnionTypeDefinition(string name, Lazy<TypeDefinition> @base, Lazy<IReadOnlyCollection<TypeDefinition>> members)
            : base(name, @base)
        {
            _delayedMemberTypes = members ?? throw new ArgumentNullException(nameof(members));
        }

        public IReadOnlyCollection<TypeDefinition> MemberTypes => _delayedMemberTypes.Value;
        private readonly Lazy<IReadOnlyCollection<TypeDefinition>> _delayedMemberTypes;
    }

    public class ListTypeDefinition : TypeDefinition
    {
        public ListTypeDefinition(string name, Lazy<TypeDefinition> @base, Lazy<TypeDefinition> elementType, Lazy<AnnotationList> annotations,
            bool isAbstract, bool isOrdered, string binding, string identifier) : base(name, @base, annotations, isAbstract, isOrdered, binding, identifier)
        {
            _delayedElementType = elementType ?? throw new ArgumentNullException(nameof(elementType));
        }

        public ListTypeDefinition(string name, Lazy<TypeDefinition> @base, Lazy<TypeDefinition> elementType)
            : base(name, @base)
        {
            _delayedElementType = elementType ?? throw new ArgumentNullException(nameof(elementType));
        }

        public TypeDefinition ElementType => _delayedElementType.Value;
        private readonly Lazy<TypeDefinition> _delayedElementType;
    }
}
