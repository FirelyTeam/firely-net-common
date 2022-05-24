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

type StepBuildResult<'a> = 
    | Success of Expression:'a * Complexity:int * Assignments:GenericParamAssignments
    | Restart of Suggestion: GenericParamAssignments
    | Fail

    member internal sbr.andThen(f: GenericParamAssignments -> 'a -> StepBuildResult<'b>): StepBuildResult<'b> = 
        match sbr with
        | Fail -> Fail
        | Restart x -> Restart x
        | Success (g,c,gps) -> 
            let m' = f gps g
            match m' with
            | Fail -> Fail
            | Restart x -> Restart x
            | Success(g',c',gps') -> Success(g',c+c',gps')

    member sbr.AndThen(f: Func<GenericParamAssignments,'a,StepBuildResult<'b>>): StepBuildResult<'b> = 
        sbr.andThen(fun gpa a -> f.Invoke(gpa,a))

    member internal sbr.map(f: 'a -> 'b): StepBuildResult<'b> =
        match sbr with
        | Fail -> Fail
        | Restart x -> Restart x
        | Success (g,c,gps) -> Success(f(g), c, gps)

    member internal sbr.IsSuccessful(): bool = 
        match sbr with
        | Success _ -> true
        | _ -> false

    member sbr.Complexity = match sbr with | Success (_,c,_) -> c | _ -> raise (new InvalidOperationException())

    member sbr.Assignments = match sbr with | Success (_,_,a) -> a | _ -> raise (new InvalidOperationException())

