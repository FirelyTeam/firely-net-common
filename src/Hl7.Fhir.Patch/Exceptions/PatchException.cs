/* 
 * Copyright (c) 2020, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/fhir-net-api/blob/master/LICENSE
 */

using System;
using Hl7.Fhir.Patch.Operations;

namespace Hl7.Fhir.Patch.Exceptions
{
    public class PatchException : Exception 
    {
        public Operation FailedOperation { get; private set; }
        public object AffectedObject { get; private set; }
 

        public PatchException()
        {

        }

        public PatchException(PatchError PatchError, Exception innerException)
            : base(PatchError.ErrorMessage, innerException)
        {
            FailedOperation = PatchError.Operation;
            AffectedObject = PatchError.AffectedObject;
        }

        public PatchException(PatchError PatchError)
          : this(PatchError, null)          
        {
        } 

        public PatchException(string message, Exception innerException)
            : base (message, innerException)
        {
           
        }
    }
}