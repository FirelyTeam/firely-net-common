namespace Firely.FhirPath

open System
open System.Linq
open System.Linq.Expressions
open TypeExtensions
open System.Collections.Generic

type StepBuildResult<'a> = 
    | Success of Expression:'a * Complexity:int * gps:GenericParamAssignments
    | Restart of GenericParamAssignments
    | Fail

module StepBuildResult =
    let andThen(sbr: StepBuildResult<'a>)(f: GenericParamAssignments -> 'a -> StepBuildResult<'b>): StepBuildResult<'b> = 
        match sbr with
        | Fail -> Fail
        | Restart x -> Restart x
        | Success (g,c,gps) -> 
            let m' = f gps g
            match m' with
            | Fail -> Fail
            | Restart x -> Restart x
            | Success(g',c',gps') -> Success(g',c+c',gps')
