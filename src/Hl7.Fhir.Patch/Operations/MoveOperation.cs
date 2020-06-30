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
using Hl7.FhirPath;

namespace Hl7.Fhir.Patch.Operations
{
    public class MoveOperation : Operation
    {
        public int Source { get; }

        public int Destination { get; }

        /// <summary>
        /// Initializes <see cref="OperationType.Move"/> operation
        /// </summary>
        /// <param name="path">target location</param>
        /// <param name="source">index to move element from</param>
        /// <param name="destination">index to move element to</param>
        public MoveOperation (CompiledExpression path, int source, int destination)
            : base(OperationType.Move, path)
        {
            Source = source;
            Destination = destination;
        }

        public override void Apply (object objectToApplyTo, PatchHelper patchHelper)
        {
            if ( patchHelper == null )
            {
                throw new ArgumentNullException(nameof(patchHelper));
            }

            if ( objectToApplyTo == null )
            {
                throw new ArgumentNullException(nameof(objectToApplyTo));
            }

            // Get value at the 'path' location at 'source' index and insert value at the 'destionation' index
            CompiledExpression sourcePath = (ITypedElement el, EvaluationContext ctx) => Path(el, ctx).Where((_, i) => i == Source);
            if ( TryGetValue(patchHelper, sourcePath, objectToApplyTo, out var elementValue) )
            {
                // delete that value
                var deleteOperation = new DeleteOperation(sourcePath) { Parent = this };
                deleteOperation.Apply(objectToApplyTo, patchHelper);

                // add that value to the path location
                var insertOperation = new InsertOperation(Path, Destination, elementValue) { Parent = this };
                insertOperation.Apply(objectToApplyTo, patchHelper);
            }
        }

        private bool TryGetValue (
            PatchHelper patchHelper,
            CompiledExpression path,
            object objectToGetValueFrom,
            out object elementValue)
        {
            if ( path == null )
            {
                throw new ArgumentNullException(nameof(path));
            }

            if ( objectToGetValueFrom == null )
            {
                throw new ArgumentNullException(nameof(objectToGetValueFrom));
            }

            var visitor = new ObjectVisitor(path, patchHelper.AdapterFactory);

            object target = objectToGetValueFrom;
            if ( !visitor.TryVisit(patchHelper.Provider, ref target, out var adapter, out var errorMessage) )
            {
                elementValue = null;
                var error = patchHelper.CreatePathNotFoundError(objectToGetValueFrom, this, errorMessage);
                patchHelper.ErrorReporter(error);
                return false;
            }

            if ( !adapter.TryGet(target, out elementValue, out errorMessage) )
            {
                var error = patchHelper.CreateOperationFailedError(objectToGetValueFrom, this, errorMessage);
                patchHelper.ErrorReporter(error);
                return false;
            }

            return true;
        }
    }
}