/*
  Copyright (c) 2011+, HL7, Inc.
  All rights reserved.
  
  Redistribution and use in source and binary forms, with or without modification, 
  are permitted provided that the following conditions are met:
  
   * Redistributions of source code must retain the above copyright notice, this 
     list of conditions and the following disclaimer.
   * Redistributions in binary form must reproduce the above copyright notice, 
     this list of conditions and the following disclaimer in the documentation 
     and/or other materials provided with the distribution.
   * Neither the name of HL7 nor the names of its contributors may be used to 
     endorse or promote products derived from this software without specific 
     prior written permission.
  
  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
  ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
  WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
  IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
  INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT 
  NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR 
  PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
  WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
  ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE 
  POSSIBILITY OF SUCH DAMAGE.
  

*/

using System;
using System.Collections.Generic;
using Hl7.Fhir.Introspection;
using System.Runtime.Serialization;
using Hl7.Fhir.Utility;
using Hl7.Fhir.Validation;
using System.ComponentModel.DataAnnotations;

namespace Hl7.Fhir.Model
{
    [System.Diagnostics.DebuggerDisplay("\\{\"{TypeName,nq}/{Id,nq}\"}")]
    [InvokeIValidatableObject]
    public partial class Resource
    {      
        /// <summary>
        /// Codes indicating the type of validation to perform
        /// </summary>
        //[FhirEnumeration("ResourceValidationMode")]
        //public enum ResourceValidationMode
        //{
        //    /// <summary>
        //    /// The server checks the content, and then checks that the content would be acceptable as a create (e.g. that the content would not violate any uniqueness constraints).
        //    /// </summary>
        //    [EnumLiteral("create", "http://hl7.org/fhir/resource-validation-mode")]
        //    Create,
        //    /// <summary>
        //    /// The server checks the content, and then checks that it would accept it as an update against the nominated specific resource (e.g. that there are no changes to immutable fields the server does not allow to change, and checking version integrity if appropriate).
        //    /// </summary>
        //    [EnumLiteral("update", "http://hl7.org/fhir/resource-validation-mode")]
        //    Update,
        //    /// <summary>
        //    /// The server ignores the content, and checks that the nominated resource is allowed to be deleted (e.g. checking referential integrity rules).
        //    /// </summary>
        //    [EnumLiteral("delete", "http://hl7.org/fhir/resource-validation-mode")]
        //    Delete,
        //}
        

        [Obsolete("Use the TypeMember member instead, or TryDeriveResourceType() to derive the ResourceType from this name.")]
        public object ResourceType => throw new NotSupportedException($"{nameof(ResourceType)} is obsolete and no longer supported.");

        /// <summary>
        /// This is the base URL of the FHIR server that this resource is hosted on
        /// </summary>
        public Uri ResourceBase
        {
            get
            {
                var bd = this.Annotation<ResourceBaseData>();
                return bd?.Base;
            }

            set
            {
                this.RemoveAnnotations<ResourceBaseData>();
                AddAnnotation(new ResourceBaseData { Base = value });
            }
        }

        private class ResourceBaseData
        {
            public Uri Base;
        }

        /// <summary>
        /// This object is internally used for locking the resource in a multithreaded environment.
        /// </summary>
        /// <remarks>
        /// As a consumer of this API, please do not use this object.
        /// </remarks>
        public readonly object SyncLock = new object();

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var result = new List<ValidationResult>();

            if (Meta != null)
            {
                if (Meta.Tag != null && validationContext.ValidateRecursively())
                    DotNetAttributeValidation.TryValidate(Meta.Tag, result, true);
            }

            return result;
        }

        public string VersionId
        {
            get => HasVersionId ? Meta.VersionId : null;
            set
            {
                if (Meta == null) Meta = new Meta();
                Meta.VersionId = value;
            }
        }

        public bool HasVersionId => Meta?.VersionId != null;
    }

}
