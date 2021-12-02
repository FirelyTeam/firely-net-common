/* 
 * Copyright (c) 2021, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-net-sdk/master/LICENSE
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#nullable enable

namespace Hl7.Fhir.Serialization
{
    /// <summary>
    /// Tracks the position within an instance as a dotted path. Used in diagnostics for the parser/serializers.
    /// </summary>
    internal class PathStack
    {
        private const string RESOURCEPREFIX = "資";

        private readonly Stack<string> _pathStack = new();

        public void EnterResource(string name)
        {
            _pathStack.Push(RESOURCEPREFIX + name);
        }

        public void ExitResource()
        {
            if (_pathStack.Count == 0)
                throw new InvalidOperationException("No resource or path parts are present on the stack.");

            var top = _pathStack.Pop();
            if (top[0] != RESOURCEPREFIX[0])
            {
                _pathStack.Push(top);
                throw new InvalidOperationException("Cannot exit a resource while inside a property.");
            }
        }

        public void EnterElement(string name)
        {
            _pathStack.Push(name);
        }

        public void ExitElement()
        {
            if (_pathStack.Count == 0)
                throw new InvalidOperationException("No resource or path parts are present on the stack.");

            var top = _pathStack.Pop();
            if (top[0] == RESOURCEPREFIX[0])
            {
                _pathStack.Push(top);
                throw new InvalidOperationException("Cannot exit a property while inside a resource.");
            }
        }

        /// <summary>
        /// Return the path. Note: in contained resources, this is just the path within the contained resource.
        /// </summary>
        public string GetPath()
        {
            if (_pathStack.Count == 0) return "";

            StringBuilder b = new();

            foreach (var part in _pathStack.Reverse())
            {
                if (b.Length != 0) b.Append('.');
                if (part[0] != RESOURCEPREFIX[0])
                    b.Append(part);
                else
                {
                    // Start again at each (contained) resource boundary
                    b.Clear();
                    b.Append(part.Substring(1));
                }
            }

            return b.ToString();
        }
    }
}

#nullable restore