/* 
 * Copyright (c) 2020, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/fhir-net-api/blob/master/LICENSE
 */

using System;
using Hl7.FhirPath;

namespace Hl7.Fhir.Patch.Operations
{
    public class OperationBase
    {
        private string _op;
        
        public OperationType OperationType { get; private set; }

        public CompiledExpression path { get; set; }

        public string op
        {
            get
            {
                return _op;
            }
            set
            {
                OperationType result;
                if (!Enum.TryParse(value, ignoreCase: true, result: out result))
                {
                    result = OperationType.Invalid;
                }
                OperationType = result;
                _op = value;
            }
        }

        public string name { get; set; }
        public int? index { get; set; }
        public int? source { get; set; }
        public int? destination { get; set; }

        public OperationBase()
        {

        }

        public OperationBase (string op, CompiledExpression path)
        {
            if ( op == null )
            {
                throw new ArgumentNullException(nameof(op));
            }

            if ( path == null )
            {
                throw new ArgumentNullException(nameof(path));
            }

            this.op = op;
            this.path = path;
        }

        public OperationBase (string op, CompiledExpression path, string name)
            : this(op, path)
        {
            this.name = name;
        }

        public OperationBase (string op, CompiledExpression path, int index)
            : this(op, path)
        {
            this.index = index;
        }

        public OperationBase(string op, CompiledExpression path, int source, int destination)
            : this(op, path)
        {
            this.source = source;
            this.destination = destination;
        }

        public bool ShouldSerializeIndex ()
        {
            return (OperationType == OperationType.Insert);
        }

        public bool ShouldSerializeIndexPair()
        {
            return (OperationType == OperationType.Move);
        }
    }
}