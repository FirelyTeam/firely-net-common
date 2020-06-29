/* 
 * Copyright (c) 2020, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/fhir-net-api/blob/master/LICENSE
 */

using System;
using System.Linq;
using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Patch.Internal;
using Hl7.Fhir.Patch.Operations;
using Hl7.Fhir.Specification;
using Hl7.FhirPath;

namespace Hl7.Fhir.Patch.Adapters
{
    /// <inheritdoc />
    public class ObjectAdapter : IObjectAdapter
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ObjectAdapter"/>.
        /// </summary>
        /// /// <param name="provider">The <see cref="IStructureDefinitionSummaryProvider"/>.</param>
        /// <param name="logErrorAction">The <see cref="Action"/> for logging <see cref="PatchError"/>.</param>
        public ObjectAdapter (
            IStructureDefinitionSummaryProvider provider,
            Action<PatchError> logErrorAction):
            this(provider, logErrorAction, new AdapterFactory())
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ObjectAdapter"/>.
        /// </summary>
        /// <param name="provider">The <see cref="IStructureDefinitionSummaryProvider"/>.</param>
        /// <param name="logErrorAction">The <see cref="Action"/> for logging <see cref="PatchError"/>.</param>
        /// <param name="adapterFactory">The <see cref="IAdapterFactory"/> to use when creating adaptors.</param>
        public ObjectAdapter (
            IStructureDefinitionSummaryProvider provider,
            Action<PatchError> logErrorAction,
            IAdapterFactory adapterFactory)
         {
            Provider = provider ?? throw new ArgumentNullException(nameof(provider));
            LogErrorAction = logErrorAction;
            AdapterFactory = adapterFactory ?? throw new ArgumentNullException(nameof(adapterFactory));
         }

        /// <summary>
        /// Gets or sets the <see cref="IStructureDefinitionSummaryProvider"/>.
        /// </summary>
        public IStructureDefinitionSummaryProvider Provider { get; }

        /// <summary>
        /// Gets or sets the <see cref="IAdapterFactory"/>
        /// </summary>
        public IAdapterFactory AdapterFactory { get; }

        /// <summary>
        /// Action for logging <see cref="PatchError"/>.
        /// </summary>
        public Action<PatchError> LogErrorAction { get; }

        public void Add(AddOperation operation, object objectToApplyTo)
        {
            if (operation == null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            if (objectToApplyTo == null)
            {
                throw new ArgumentNullException(nameof(objectToApplyTo));
            }

            Add(operation.Path, operation.Name, operation.Value, objectToApplyTo, operation);
        }

        /// <summary>
        /// Add is used by various operations (eg: add, ...), yet through different operations;
        /// This method allows code reuse yet reporting the correct operation on error
        /// </summary>
        private void Add(
            CompiledExpression path,
            string name,
            object value,
            object objectToApplyTo,
            Operation operation)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            
            if ( name == null )
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (objectToApplyTo == null)
            {
                throw new ArgumentNullException(nameof(objectToApplyTo));
            }

            if (operation == null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            var visitor = new ObjectVisitor(path, AdapterFactory);

            object target = objectToApplyTo;
            if (!visitor.TryVisit(Provider, ref target, out var adapter, out var errorMessage))
            {
                var error = CreatePathNotFoundError(objectToApplyTo, operation, errorMessage);
                ErrorReporter(error);
                return;
            }

            if (!adapter.TryAdd(target, name, value, out errorMessage))
            {
                var error = CreateOperationFailedError(objectToApplyTo, operation, errorMessage);
                ErrorReporter(error);
                return;
            }
        }

        public void Insert (InsertOperation operation, object objectToApplyTo)
        {
            if ( operation == null )
            {
                throw new ArgumentNullException(nameof(operation));
            }

            if ( objectToApplyTo == null )
            {
                throw new ArgumentNullException(nameof(objectToApplyTo));
            }

            Insert(operation.Path, operation.Index, operation.Value, objectToApplyTo, operation);
        }

        /// <summary>
        /// Add is used by various operations (eg: insert, move, ...), yet through different operations;
        /// This method allows code reuse yet reporting the correct operation on error
        /// </summary>
        private void Insert (
            CompiledExpression path,
            int index,
            object value,
            object objectToApplyTo,
            Operation operation)
        {
            if ( path == null )
            {
                throw new ArgumentNullException(nameof(path));
            }

            if ( objectToApplyTo == null )
            {
                throw new ArgumentNullException(nameof(objectToApplyTo));
            }

            if ( operation == null )
            {
                throw new ArgumentNullException(nameof(operation));
            }

            var visitor = new ObjectVisitor(path, AdapterFactory);

            object target = objectToApplyTo;
            if ( !visitor.TryVisit(Provider, ref target, out var adapter, out var errorMessage) )
            {
                var error = CreatePathNotFoundError(objectToApplyTo, operation, errorMessage);
                ErrorReporter(error);
                return;
            }

            if ( !adapter.TryInsert(target, index, value, out errorMessage) )
            {
                var error = CreateOperationFailedError(objectToApplyTo, operation, errorMessage);
                ErrorReporter(error);
                return;
            }
        }

        public void Delete(DeleteOperation operation, object objectToApplyTo)
        {
            if (operation == null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            if (objectToApplyTo == null)
            {
                throw new ArgumentNullException(nameof(objectToApplyTo));
            }

            Delete(operation.Path, objectToApplyTo, operation);
        }

        /// <summary>
        /// Delete is used by various operations (eg: delete, move, ...), yet through different operations;
        /// This method allows code reuse yet reporting the correct operation on error.  The return value
        /// contains the type of the item that has been removed (and a bool possibly signifying an error)
        /// This can be used by other methods, like replace, to ensure that we can pass in the correctly
        /// typed value to whatever method follows.
        /// </summary>
        private void Delete(CompiledExpression path, object objectToApplyTo, Operation operationToReport)
        {
            var visitor = new ObjectVisitor(path, AdapterFactory);

            object target = objectToApplyTo;
            if (!visitor.TryVisit(Provider, ref target, out var adapter, out var errorMessage))
            {
                var error = CreatePathNotFoundError(objectToApplyTo, operationToReport, errorMessage);
                ErrorReporter(error);
                return;
            }

            if (!adapter.TryDelete(target, out errorMessage))
            {
                var error = CreateOperationFailedError(objectToApplyTo, operationToReport, errorMessage);
                ErrorReporter(error);
                return;
            }
        }

        public void Replace(ReplaceOperation operation, object objectToApplyTo)
        {
            if (operation == null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            if (objectToApplyTo == null)
            {
                throw new ArgumentNullException(nameof(objectToApplyTo));
            }

            var visitor = new ObjectVisitor(operation.Path, AdapterFactory);

            object target = objectToApplyTo;
            if (!visitor.TryVisit(Provider, ref target, out var adapter, out var errorMessage))
            {
                var error = CreatePathNotFoundError(objectToApplyTo, operation, errorMessage);
                ErrorReporter(error);
                return;
            }

            if (!adapter.TryReplace(target, operation.Value, out errorMessage))
            {
                var error = CreateOperationFailedError(objectToApplyTo, operation, errorMessage);
                ErrorReporter(error);
                return;
            }
        }

        public void Move (MoveOperation operation, object objectToApplyTo)
        {
            if ( operation == null )
            {
                throw new ArgumentNullException(nameof(operation));
            }

            if ( objectToApplyTo == null )
            {
                throw new ArgumentNullException(nameof(objectToApplyTo));
            }

            // Get value at the 'path' location at 'source' index and insert value at the 'destionation' index
            CompiledExpression sourcePath = (ITypedElement el, EvaluationContext ctx) => operation.Path(el, ctx).Where((_, i) => i == operation.Source);
            if ( TryGetValue(sourcePath, objectToApplyTo, operation, out var propertyValue) )
            {
                // delete that value
                Delete(sourcePath, objectToApplyTo, operation);

                // add that value to the path location
                Insert(operation.Path, operation.Destination, propertyValue, objectToApplyTo, operation);
            }
        }

        private bool TryGetValue(
            CompiledExpression path,
            object objectToGetValueFrom,
            Operation operation,
            out object propertyValue)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (objectToGetValueFrom == null)
            {
                throw new ArgumentNullException(nameof(objectToGetValueFrom));
            }

            if (operation == null)
            {
                throw new ArgumentNullException(nameof(operation));
            }


            var visitor = new ObjectVisitor(path, AdapterFactory);

            object target = objectToGetValueFrom;
            if (!visitor.TryVisit(Provider, ref target, out var adapter, out var errorMessage))
            {
                propertyValue = null;
                var error = CreatePathNotFoundError(objectToGetValueFrom, operation, errorMessage);
                ErrorReporter(error);
                return false;
            }

            if ( !adapter.TryGet(target, out propertyValue, out errorMessage) )
            {
                var error = CreateOperationFailedError(objectToGetValueFrom, operation, errorMessage);
                ErrorReporter(error);
                return false;
            }

            return true;
        }

        private Action<PatchError> ErrorReporter
        {
            get
            {
                return LogErrorAction ?? Internal.ErrorReporter.Default;
            }
        }

        private PatchError CreateOperationFailedError(object target, Operation operation, string errorMessage)
        {
            return new PatchError(
                target,
                operation,
                errorMessage ?? $"The '{operation.OperationType:G}' operation at path could not be performed.");
        }

        private PatchError CreatePathNotFoundError(object target, Operation operation, string errorMessage)
        {
            return new PatchError(
                target,
                operation,
                errorMessage ?? $"For operation '{operation.OperationType:G}', the target location specified by path was not found.");
        }
    }
}