/* 
 * Copyright (c) 2021, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-net-sdk/master/LICENSE
 */

#nullable enable

#if NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER


using Hl7.Fhir.Introspection;
using System;

namespace Hl7.Fhir.Serialization
{
    /// <summary>
    /// A filter that applies one filter to the top-level elements, and if such a filter allows
    /// an element to pass, another filter to the child elements of that top-level element.
    /// </summary>
    public class TopLevelFilter : SerializationFilter
    {
        /// <summary>
        /// Constructs a filter given a filter for the top-level elements and an optional filter for the
        /// children of top-level elements that matches the top-level filter.
        /// </summary>
        public TopLevelFilter(SerializationFilter toplevelFilter, SerializationFilter? childrenFilter = default)
        {
            ToplevelFilter = toplevelFilter ?? throw new ArgumentNullException(nameof(toplevelFilter));
            ChildrenFilter = childrenFilter;
        }

        private int? _childrenFromDepth = null;
        private int _currentDepth = 0;

        /// <summary>
        /// The filter that is applied to the top-level elements.
        /// </summary>
        public SerializationFilter ToplevelFilter { get; }

        /// <summary>
        /// The filter that is applied to children of top-level elements.
        /// </summary>
        public SerializationFilter? ChildrenFilter { get; }

        private bool inFilteredSubtree => _currentDepth >= _childrenFromDepth;

        /// <inheritdoc/>
        public override void EnterObject(object value, ClassMapping? mapping)
        {
            if (inFilteredSubtree)
                ChildrenFilter?.EnterObject(value, mapping);
            else
                ToplevelFilter.EnterObject(value, mapping);
        }

        /// <inheritdoc/>
        public override void LeaveObject(object value, ClassMapping? mapping)
        {
            if (inFilteredSubtree)
                ChildrenFilter?.LeaveObject(value, mapping);
            else
                ToplevelFilter.LeaveObject(value, mapping);
        }

        /// <inheritdoc/>
        public override void LeaveMember(string name, object value, PropertyMapping? mapping)
        {
            if (inFilteredSubtree)
                ChildrenFilter?.LeaveMember(name, value, mapping);
            else
                ToplevelFilter.LeaveMember(name, value, mapping);

            _currentDepth -= 1;

            if (_currentDepth <= _childrenFromDepth) _childrenFromDepth = null;
        }

        /// <inheritdoc/>
        public override bool TryEnterMember(string name, object value, PropertyMapping? mapping)
        {
            bool success;

            if (inFilteredSubtree)
                success = ChildrenFilter?.TryEnterMember(name, value, mapping) ?? true;
            else
            {
                success = ToplevelFilter.TryEnterMember(name, value, mapping);
                if (success) _childrenFromDepth = _currentDepth;
            }

            if (success)
                _currentDepth += 1;

            return success;
        }
    }

    //public class FilterOnMetadataProperty : SerializationFilter
    //{
    //    /// <summary>
    //    /// Include top-level mandatory elements, including all their children
    //    /// </summary>
    //    public bool IncludeMandatory { get; set; } // = false;

    //    /// <summary>
    //    /// Include all elements marked "in summary" in the definition of the element
    //    /// </summary>
    //    public bool IncludeInSummary { get; set; } // = false;

    //    ///// <summary>
    //    ///// Include all elements marked "is modifier" in the definition of the element
    //    ///// </summary>
    //    //public bool IncludeIsModifier { get; set; } // = false;

    //    /// <summary>
    //    /// When an element is included based, include its children too.
    //    /// </summary>
    //    public bool IncludeChildren { get; set; }


    //    public override void EnterObject(object value, ClassMapping? mapping)
    //    {
    //        _currentDepth += 1;
    //    }

    //    public override void LeaveObject()
    //    {
    //        _currentDepth -= 1;
    //    }

    //    public override void LeaveMember()
    //    {
    //        // nothing
    //    }

    //    public override bool TryEnterMember(string name, object value, PropertyMapping? mapping)
    //    {
    //        // If we're including a subtree, return true immediately
    //        if (_includeFromDepth is not null && _currentDepth >= _includeFromDepth)
    //            return true;
    //        else
    //            _includeFromDepth = null;

    //        if (IncludeInSummary && mapping?.InSummary == true ||
    //            IncludeMandatory && mapping?.IsMandatoryElement == true)
    //        {
    //            if (IncludeChildren)
    //            {
    //                // Include all children too
    //                _includeFromDepth = _currentDepth;
    //            }

    //            return true;
    //        }

    //        return false;
    //    }

    //    private int? _includeFromDepth = null;
    //    private int _currentDepth = -1;
    //}


    //public class PathDumperFilter : SerializationFilter
    //{
    //    readonly Stack<string> pathStack = new();

    //    public override void EnterObject(object value, ClassMapping? mapping)
    //    {
    //        pathStack.Push(mapping?.Name ?? value.GetType().Name);
    //    }

    //    public override bool TryEnterMember(string name, object value, PropertyMapping? mapping)
    //    {
    //        pathStack.Push(mapping?.Name ?? name);

    //        Console.WriteLine(string.Join(".", pathStack.Reverse()));

    //        return true;
    //    }

    //    public override void LeaveMember()
    //    {
    //        pathStack.Pop();
    //    }

    //    public override void LeaveObject() => pathStack.Pop();
    //}
}

#endif
#nullable restore
