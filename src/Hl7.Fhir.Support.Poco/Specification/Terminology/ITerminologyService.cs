/* 
 * Copyright (c) 2016, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-net-sdk/master/LICENSE
 */

using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using System.Threading.Tasks;

namespace Hl7.Fhir.Specification.Terminology
{
    public interface ITerminologyService
    {
        /// <summary>
        /// Validate that a coded value is in the set of codes allowed by a value set.
        /// </summary>
        /// <param name="parameters">Input parameters for the operation</param>
        /// <param name="id">Id of a specific ValueSet which is used to validate against</param>
        /// <param name="useGet"> Use the GET instead of POST Http method</param>
        /// <returns>Output parameters containing the result of the operation</returns>
        /// <exception cref="FhirOperationException">Thrown when the terminology service encounters an error</exception>
        /// <remarks>See http://hl7.org/valueset-operations.html#validate-code for more information</remarks>
        Task<Parameters> ValueSetValidateCode(Parameters parameters, string id = null, bool useGet = false);

        /// <summary>
        /// Validate that a coded value is in the code system.
        /// </summary>
        /// <param name="parameters">Input parameters for the operation</param>
        /// <param name="id">Id of a specific CodeSystem which is used to validate against</param>
        /// <param name="useGet">Use the GET instead of POST Http method</param>
        /// <returns>Output parameters containing the result of the operation</returns>
        /// <exception cref="FhirOperationException">Thrown when the terminology service encounters an error</exception>
        Task<Parameters> CodeSystemValidateCode(Parameters parameters, string id = null, bool useGet = false);

        /// <summary>
        /// The definition of a value set is used to create a simple collection of codes suitable for use for data entry or validation.
        /// </summary>
        /// <param name="parameters">Input parameters for the operation</param>
        /// <param name="id">Id of a specific ValueSet to expand</param>
        /// <param name="useGet">Use the GET instead of POST Http method</param>
        /// <returns>Output parameters containing the expanded ValueSet</returns>
        /// <exception cref="FhirOperationException">Thrown when the terminology service encounters an error</exception>
        Task<Resource> Expand(Parameters parameters, string id = null, bool useGet = false);


        /// <summary>
        /// Given a code/system, or a Coding, get additional details about the concept, including definition, status, designations, and properties.
        /// </summary>
        /// <param name="parameters">Input parameters for the operation</param>
        /// <param name="useGet">Use the GET instead of POST Http method</param>
        /// <returns>Output parameters containing the result of the operation</returns>
        /// <exception cref="FhirOperationException">Thrown when the terminology service encounters an error</exception>
        Task<Parameters> Lookup(Parameters parameters, bool useGet = false);

        /// <summary>
        /// The transform operation takes input content, applies a structure map transform, and then returns the output.
        /// </summary>
        /// <param name="parameters">Input parameters for the operation</param>
        /// <param name="id">Id of the StructureMap used for the tranformation</param>
        /// <param name="useGet">Use the GET instead of POST Http method</param>
        /// <returns>Output parameter containing the result of the translation</returns>
        /// <exception cref="FhirOperationException">Thrown when the terminology service encounters an error</exception>
        Task<Parameters> Translate(Parameters parameters, string id = null, bool useGet = false);

        /// <summary>
        /// Test the subsumption relationship between code/Coding A and code/Coding B given the semantics of subsumption in the underlying code system
        /// </summary>
        /// <param name="parameters">Input parameters for the operation</param>
        /// <param name="id">Id of the code system in which subsumption testing is to be performed.</param>
        /// <param name="useGet">Use the GET instead of POST Http method</param>
        /// <returns>Output parameters containing the subsumption relationship between code/Coding "A" and code/Coding "B".</returns>
        /// <exception cref="FhirOperationException">Thrown when the terminology service encounters an error</exception>
        Task<Parameters> Subsumes(Parameters parameters, string id = null, bool useGet = false);

        /// <summary>
        /// Provides support for ongoing maintenance of a client-side transitive closure table based on server-side terminological logic. 
        /// </summary>
        /// <param name="parameters">Input parameters for the operation</param>
        /// <param name="useGet">Use the GET instead of POST Http method</param>
        /// <returns>Output parameters containing a ConceptMap with a list of new entries (code / system --> code/system) that the client should add to its closure table.</returns>
        /// <exception cref="FhirOperationException">Thrown when the terminology service encounters an error</exception>
        Task<Resource> Closure(Parameters parameters, bool useGet = false);

    }
}
