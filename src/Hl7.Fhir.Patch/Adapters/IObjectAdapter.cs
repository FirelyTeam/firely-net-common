/* 
 * Copyright (c) 2020, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/fhir-net-api/blob/master/LICENSE
 */

using Hl7.Fhir.Patch.Operations;

namespace Hl7.Fhir.Patch.Adapters
{
    /// <summary>
    /// Defines the operations that can be performed on a patch document.
    /// </summary>
    public interface IObjectAdapter
    {
        /// <summary>
        /// Using the "add" operation a new value is inserted into the root of the target
        /// document, into the target array at the end, or to a target object at
        /// the specified location.
        ///
        /// When adding to arrays, index should be omitted.
        ///
        /// When adding to an object, if an object member does not already exist, a new member is added to the object at the
        /// specified location or if an object member does exist, an error is thrown.
        ///
        /// The operation object MUST contain a "value" member whose content
        /// specifies the value to be added.
        ///
        /// For example:
        ///
        /// { "op": "add", "path": "/a/b", "name": "c", "value": [ "foo", "bar" ] }
        ///
        /// </summary>
        /// <param name="operation">The add operation.</param>
        /// <param name="objectToApplyTo">Object to apply the operation to.</param>
        void Add(AddOperation operation, object objectToApplyTo);

        /// <summary>
        /// Using the "insert" operation a new value is inserted into the target array
        /// at the specified valid index. 
        ///
        /// The specified index MUST NOT be greater than the number of elements in the array.
        ///
        /// The operation object MUST contain a "value" member whose content
        /// specifies the value to be inserted.
        /// 
        /// The operation object MUST contain an "index" member whose content
        /// specifies the index at which to insert.
        /// 
        /// For example:
        ///
        /// { "op": "insert", "path": "/a/b/c", "index": 2, "value": { "foo": "bar" } }
        ///
        /// Any elements above or at the specified index are shifted one position to the right.
        /// 
        /// </summary>
        /// <param name="operation">The add operation.</param>
        /// <param name="objectToApplyTo">Object to apply the operation to.</param>
        void Insert (InsertOperation operation, object objectToApplyTo);

        /// <summary>
        /// Using the "delete" operation the value at the target location is deleted.
        /// Only a single element can be deleted.
        /// 
        /// The target location MUST exist for the operation to be successful.
        /// 
        /// For example:
        ///
        /// { "op": "delete", "path": "/a/b/c" }
        ///
        /// If removing an element from an array, any elements above the
        /// specified index are shifted one position to the left.
        ///
        /// </summary>
        /// <param name="operation">The remove operation.</param>
        /// <param name="objectToApplyTo">Object to apply the operation to.</param>
        void Delete (DeleteOperation operation, object objectToApplyTo);

        /// <summary>
        /// Using the "replace" operation the value at the target location is replaced
        /// with a new value.  The operation object MUST contain a "value" member
        /// which specifies the replacement value.
        ///
        /// The target location MUST exist for the operation to be successful.
        ///
        /// For example:
        ///
        /// { "op": "replace", "path": "/a/b/c", "value": 42 }
        ///
        /// </summary>
        /// <param name="operation">The replace operation.</param>
        /// <param name="objectToApplyTo">Object to apply the operation to.</param>
        void Replace(ReplaceOperation operation, object objectToApplyTo);

        /// <summary>
        /// Using the "move" operation the value at a source index in the target array is removed
        /// and inserted at the destination index.
        ///
        /// The operation object MUST contain a "source" and "destination" members, 
        /// which references indices in the target array to move the value from and to.
        ///
        /// The target array MUST exist for the operation to be successful.
        /// The specified source index MUST NOT be greater than the number of elements in the array.
        /// The specified destination index MUST be less than the number of elements in the array.
        /// 
        /// For example:
        ///
        /// { "op": "move", "source": 3, "destination": 5, "path": "/a/b/d" }
        ///
        /// Element is at the source index is deleted first and any elements above the
        /// specified index are shifted one position to the left.
        /// Afterwards it is inserted at the destination index and any elements above
        /// or at the specified index are shifted one position to the right.
        /// 
        /// </summary>
        /// <param name="operation">The move operation.</param>
        /// <param name="objectToApplyTo">Object to apply the operation to.</param>
        void Move (MoveOperation operation, object objectToApplyTo);
    }
}