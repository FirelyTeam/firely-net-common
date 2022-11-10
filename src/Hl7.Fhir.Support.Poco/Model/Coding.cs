﻿/*
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

using Hl7.Fhir.Introspection;
using System.Diagnostics;
using System.Text;
using S = Hl7.Fhir.ElementModel.Types;


namespace Hl7.Fhir.Model
{


    [DebuggerDisplay(@"\{{DebuggerDisplay,nq}}")]
    [Bindable(true)]
    public partial class Coding
    {
        public Coding()
        {

        }

        public Coding(string system, string code)
        {
            this.System = system;
            this.Code = code;
        }

        public Coding(string system, string code, string display)
        {
            this.System = system;
            this.Code = code;
            this.Display = display;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public string DebuggerDisplay
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                if (!string.IsNullOrEmpty(this.Code))
                    sb.AppendFormat(" Code=\"{0}\"", Code);
                if (!string.IsNullOrEmpty(this.Display))
                    sb.AppendFormat(" Display=\"{0}\"", Display);
                if (!string.IsNullOrEmpty(this.System))
                    sb.AppendFormat(" System=\"{0}\"", System);

                return sb.ToString();
            }
        }

        public S.Code ToSystemCode() => new S.Code(System, Code, Display, Version);
    }
}
