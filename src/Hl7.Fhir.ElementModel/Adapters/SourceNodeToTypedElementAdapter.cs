/* 
 * Copyright (c) 2018, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

 using System;
using System.Collections.Generic;
using System.Linq;
using Hl7.Fhir.Specification;
using Hl7.Fhir.Utility;

namespace Hl7.Fhir.ElementModel.Adapters
{
    internal class SourceNodeToTypedElementAdapter : ITypedElement, IAnnotated, IExceptionSource
    {
        public readonly ISourceNode Current;
        private readonly ModelDefinition _model;

        public SourceNodeToTypedElementAdapter(ModelDefinition model, ISourceNode node)
        {
            Current = node ?? throw Error.ArgumentNull(nameof(node));
            _model = model ?? throw Error.ArgumentNull(nameof(model));

            if (node is IExceptionSource ies && ies.ExceptionHandler == null)
                ies.ExceptionHandler = (o, a) => ExceptionHandler.NotifyOrThrow(o, a);
        }

        private SourceNodeToTypedElementAdapter(SourceNodeToTypedElementAdapter parent, ISourceNode sourceNode, ModelDefinition model)
        {
            Current = sourceNode;
            ExceptionHandler = parent.ExceptionHandler;
            _model = model;
        }

        public ExceptionNotificationHandler ExceptionHandler { get; set; }

        public string Name => Current.Name;

        public TypeDefinition InstanceTypeD
        {
            get 
            {
                var instanceType = Current.GetResourceTypeIndicator();
                return instanceType == null ? 
                    null : 
                    _model.TryGetType(instanceType, out var definition) ? definition: null;
            }
        }

        public object Value => Current.Text;

        public string Location => Current.Location;

        public IElementDefinitionSummary Definition => throw new NotImplementedException();

        public IEnumerable<ITypedElement> Children(string name) =>
            Current.Children(name).Select(c => new SourceNodeToTypedElementAdapter(this, c, _model));

        IEnumerable<object> IAnnotated.Annotations(Type type) => Current.Annotations(type);
    }
}