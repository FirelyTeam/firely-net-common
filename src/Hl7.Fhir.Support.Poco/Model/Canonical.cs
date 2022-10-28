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

using Hl7.Fhir.Utility;
using System;
using System.Text;

#nullable enable

namespace Hl7.Fhir.Model
{
    public partial class Canonical
    {
        /// <summary>
        /// Constructs a Canonical based on a given <see cref="Uri"/>.
        /// </summary>
        /// <param name="uri"></param>
        public Canonical(Uri uri) : this(uri?.OriginalString)
        {
            // nothing
        }

        /// <summary>
        /// Create a new Canonical from multiple components
        /// </summary>
        /// <param name="canonicalUrl">A raw canonical URL (with no version or fragment components)</param>
        /// <param name="canonicalVersion">A canonical version</param>
        /// <param name="fragment">A fragment reference within the canonical</param>
        /// <returns>A new Canonical instance preformatted with the parameters provided</returns>
        public Canonical(string canonicalUrl, string? canonicalVersion, string? fragment = null)
        {
            if (canonicalUrl == null) throw Error.ArgumentNull(nameof(canonicalUrl));
            if (canonicalUrl.IndexOfAny(new[] { '|', '#' }) != -1)
                throw Error.Argument(nameof(canonicalUrl), "cannot contain version/fragment data");

            if (canonicalVersion != null && canonicalVersion.IndexOfAny(new[] { '|', '#' }) != -1)
                throw Error.Argument(nameof(canonicalVersion), "cannot contain version/fragment data");

            if (fragment != null && fragment.IndexOfAny(new[] { '|', '#' }) != -1)
                throw Error.Argument(nameof(fragment), "already contains version/fragment data");

            StringBuilder sb = new StringBuilder();
            sb.Append(canonicalUrl);
            if (!string.IsNullOrEmpty(canonicalVersion))
                sb.Append($"|{canonicalVersion}");
            if (!string.IsNullOrEmpty(fragment))
                sb.Append($"#{fragment}");
            this.Value = sb.ToString();
        }

        /// <summary>
        /// Converts a string to a canonical.
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator Canonical(string value) => new(value);

        /// <summary>
        /// Converts a canonical to a string.
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator string?(Canonical value) => value?.Value;

        /// <summary>
        /// Checks whether the given literal is correctly formatted.
        /// </summary>
        public static bool IsValidValue(string value) => FhirUri.IsValidValue(value);

        /// <summary>
        /// The raw canonical value, excluding any version (after |) or fragment (after #) components
        /// </summary>
        /// <remarks>
        /// This would be found in the `Url` property of a referenced canonical resource
        /// </remarks>
        public string CanonicalUrl
        {
            get
            {
                int indexOfVersion = Value.IndexOf("|");
                if (indexOfVersion != -1)
                    return Value.Substring(0, indexOfVersion);
                int indexOfFragment = Value.IndexOf("#");
                if (indexOfFragment != -1)
                    return Value.Substring(0, indexOfFragment);
                return Value;
            }
            set
            {
                Value = new Canonical(value, CanonicalVersion, Fragment).Value;
            }
        }

        /// <summary>
        /// The canonical version (if any) included on the URL
        /// </summary>
        /// <remarks>
        /// This would be found in the `Version` property of a referenced canonical resource
        /// </remarks>
        public string? CanonicalVersion
        {
            get
            {
                int indexOfVersion = Value.IndexOf("|");
                if (indexOfVersion == -1)
                    return null;
                var result = Value.Substring(indexOfVersion + 1);
                int indexOfFragment = result.IndexOf("#");
                if (indexOfFragment != -1)
                    return result.Substring(0, indexOfFragment);
                return result;

            }
            set
            {
                Value = new Canonical(CanonicalUrl, value, Fragment).Value;
            }
        }

        public string? Fragment
        {
            get
            {
                int indexOfFragment = Value.IndexOf("#");
                if (indexOfFragment != -1)
                    return Value.Substring(indexOfFragment + 1);
                return null;
            }
            set
            {
                Value = new Canonical(CanonicalUrl, CanonicalVersion, value).Value;
            }
        }

        /// <summary>
        /// Is a Version component present in the Canonical
        /// </summary>
        /// <returns></returns>
        public bool HasVersion()
        {
            return this.Value?.Contains("|") == true;
        }

        /// <summary>
        /// Is a Fragment component present in the Canonical
        /// </summary>
        /// <returns></returns>
        public bool HasFragment()
        {
            return this.Value?.Contains("#") == true;
        }
    }
}

#nullable restore