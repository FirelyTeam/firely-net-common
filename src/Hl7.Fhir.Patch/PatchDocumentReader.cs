/* 
 * Copyright (c) 2020, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/fhir-net-api/blob/master/LICENSE
 */


using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Patch;
using Hl7.Fhir.Patch.Operations;
using Hl7.Fhir.Specification;
using Hl7.Fhir.Utility;
using Hl7.FhirPath;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hl7.Fhir.Serialization
{
    public static class PatchDocumentReader
    {
        private static readonly FhirPathCompiler _fhirPathCompiler = new FhirPathCompiler();
        private static readonly Action<Exception> _defaultErrorReporter = (ex) => throw ex;

        enum ParameterName
        {
            Type,
            Path,
            Name,
            Value,
            Index,
            Source,
            Destination
        }

        public static PatchDocument Read (ITypedElement fhirPatch, IStructureDefinitionSummaryProvider provider, Action<Exception> errorReporter = null)
        {
            errorReporter ??= _defaultErrorReporter;
            var patchDocument = new PatchDocument(new List<Operation>(), provider);

            foreach ( var parameter in fhirPatch.Select("Parameters.parameter") )
            {
                if ( parameter.Scalar("name") as string != "operation" )
                {
                    errorReporter(Error.Argument("parameter.Name", "Parameter Name must be 'operation'"));
                    return null;
                }

                var parts = parameter.Select("part");
                if (parts == null || !parts.Any())
                {
                    errorReporter(Error.ArgumentNullOrEmpty("parameter.part"));
                    return null;
                }

                var operationParams = new Dictionary<ParameterName, ITypedElement>();
                foreach ( var part in parts )
                {
                    var partName = part.Scalar("name") as string;
                    var paramName = (partName?.ToUpperInvariant()) switch
                    {
                        "TYPE" => ParameterName.Type,
                        "PATH" => ParameterName.Path,
                        "NAME" => ParameterName.Name,
                        "VALUE" => ParameterName.Value,
                        "INDEX" => ParameterName.Index,
                        "SOURCE" => ParameterName.Source,
                        "DESTINATION" => ParameterName.Destination,
                        _ => (ParameterName?)null
                    };

                    if ( paramName == null )
                    {
                        errorReporter(Error.Argument("parameter.part.Name", $"Operation parameter name '${partName}' is not a valid parameter name"));
                        return null;
                    }

                    operationParams.Add(paramName.Value, part);
                }

                // Verify Operation type is present and valid
                if ( !operationParams.TryGetValue(ParameterName.Type, out var operationTypeComponent) )
                {
                    errorReporter(Error.Argument($"parameter.part['type']", "Operation type must be specified"));
                    return null;
                }

                var operationTypeStr = operationTypeComponent.Scalar("value") as string;
                OperationType? operationType = (operationTypeStr?.ToUpperInvariant()) switch
                {
                    "ADD" => OperationType.Add,
                    "INSERT" => OperationType.Insert,
                    "DELETE" => OperationType.Delete,
                    "REPLACE" => OperationType.Replace,
                    "MOVE" => OperationType.Move,
                    _ => (OperationType?)null
                };

                if ( operationType == null )
                {
                    errorReporter(Error.Argument("parameter.part['type']", $"Operation type '{operationTypeStr}' is not a valid operation type"));
                    return null;
                }

                // Verify path is present and parse it
                if ( !operationParams.TryGetValue(ParameterName.Path, out var pathComponent) )
                {
                    errorReporter(Error.Argument($"parameter.part['path']", "Path must be specified"));
                    return null;
                }

                var path = pathComponent.Scalar("value") as string;
                if ( string.IsNullOrEmpty(path) )
                {
                    errorReporter(Error.ArgumentNullOrEmpty("parameter.part['path']"));
                    return null;
                }

                CompiledExpression fhirPath = null;
                try
                {
                    fhirPath = _fhirPathCompiler.Compile(path);
                }
                catch (Exception ex)
                {
                    errorReporter(Error.InvalidOperation(ex, $"Specified path '{path}' is not valid"));
                    return null;
                }

                // Parse value parameter if it is required
                ITypedElement value = null;
                switch ( operationType )
                {
                    // Value is required
                    case OperationType.Add:
                    case OperationType.Insert:
                    case OperationType.Replace:
                        // Verify value is present and parse it
                        if ( !operationParams.TryGetValue(ParameterName.Value, out var valueComponent) )
                        {
                            errorReporter(Error.Argument($"parameter.part['value']", "Value must be specified"));
                            return null;
                        }

                        value = valueComponent.Select("value").FirstOrDefault();
                        if ( value == null )
                        {
                            errorReporter(Error.ArgumentNull("parameter.part['value']"));
                            return null;
                        }

                        break;
                    // Value is not required
                    case OperationType.Delete:
                    case OperationType.Move:
                    default:
                        break;
                }

                // Parse operation specific parameters and add operation to the patch document
                switch ( operationType )
                {
                    case OperationType.Add:
                        // Verify name is present and parse it
                        if ( !operationParams.TryGetValue(ParameterName.Name, out var nameComponent) )
                        {
                            errorReporter(Error.Argument($"parameter.part['name']", "Name must be specified"));
                            return null;
                        }

                        var name = nameComponent.Scalar("value") as string;
                        if ( string.IsNullOrEmpty(name) )
                        {
                            errorReporter(Error.ArgumentNullOrEmpty("parameter.part['name']"));
                            return null;
                        }

                        patchDocument.Add(new AddOperation(fhirPath, name, value));
                        break;
                    case OperationType.Insert:
                        // Verify index is present and parse it
                        if ( !operationParams.TryGetValue(ParameterName.Index, out var indexComponent) )
                        {
                            errorReporter(Error.Argument($"parameter.part['index']", "Index must be specified"));
                            return null;
                        }

                        var index = indexComponent.Scalar("value") as int?;
                        if ( index == null )
                        {
                            errorReporter(Error.ArgumentNull("parameter.part['index']"));
                            return null;
                        }

                        patchDocument.Add(new InsertOperation(fhirPath, index.Value, value));
                        break;
                    case OperationType.Delete:
                        patchDocument.Add(new DeleteOperation(fhirPath));
                        break;
                    case OperationType.Replace:
                        patchDocument.Add(new ReplaceOperation(fhirPath, value));
                        break;
                    case OperationType.Move:
                        // Verify source index is present and parse it
                        if ( !operationParams.TryGetValue(ParameterName.Source, out var sourceComponent) )
                        {
                            errorReporter(Error.Argument($"parameter.part['source']", "Source index must be specified"));
                            return null;
                        }

                        var source = sourceComponent.Scalar("value") as int?;
                        if ( source == null )
                        {
                            errorReporter(Error.ArgumentNull("parameter.part['source']"));
                            return null;
                        }

                        // Verify destination index is present and parse it
                        if ( !operationParams.TryGetValue(ParameterName.Destination, out var destinationComponent) )
                        {
                            errorReporter(Error.Argument($"parameter.part['destination']", "Destination index must be specified"));
                            return null;
                        }

                        var destination = destinationComponent.Scalar("value") as int?;
                        if ( destination == null )
                        {
                            errorReporter(Error.ArgumentNull("parameter.part['destination']"));
                            return null;
                        }

                        patchDocument.Add(new MoveOperation(fhirPath, source.Value, destination.Value));
                        break;
                    default:
                        errorReporter(Error.InvalidOperation($"Operation type '{operationTypeStr}' is not a valid operation type"));
                        return null;
                }
            }

            return patchDocument;
        }
    }
}
