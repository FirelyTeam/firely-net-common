using Hl7.Fhir.ElementModel;
using System;
using System.Collections.Generic;

namespace Hl7.FhirPath
{
    public class EvaluationContext
    {
        public static EvaluationContext CreateDefault() => new EvaluationContext();

        public EvaluationContext()
        {
            // no defaults yet
        }

        public EvaluationContext(ITypedElement container) : this(container, null) { }

        public EvaluationContext(ITypedElement container, ITypedElement rootContainer)
        {
            Container = container;
            RootContainer = rootContainer ?? container;
        }

        public ITypedElement RootContainer { get; set; }

        public ITypedElement Container { get; set; }

        public Action<string, IEnumerable<ITypedElement>> Tracer { get; set; }

        #region Obsolete members
        [Obsolete("Please use CreateDefault() instead of this member, which may cause raise conditions.")]
        public static readonly EvaluationContext Default = new EvaluationContext();
        #endregion
    }
}