namespace Firely.FhirPath

open System
open System.Collections.Generic

type GenericParam = 
    | GenericParam of string
    
    static member buildGp(gpType:Type) = GenericParam(gpType.Name)

type GenericParamAssignments = 
    inherit Dictionary<Type,Type>
    new() = { inherit Dictionary<Type,Type>() } 
    new(source: IDictionary<Type,Type>) = { inherit Dictionary<Type,Type>(source) }