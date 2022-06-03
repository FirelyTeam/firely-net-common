namespace Firely.FhirPath

open System
open System.Collections.Generic

type GenericParamAssignments = 
    inherit Dictionary<Type,Type>
    new() = { inherit Dictionary<Type,Type>() } 
    new(source: IDictionary<Type,Type>) = { inherit Dictionary<Type,Type>(source) }

type StepBuildResult<'a> = 
    | Success of Expression:'a * Complexity:int * Assignments: GenericParamAssignments
    | Restart of Suggestion: GenericParamAssignments
    | Fail

    member internal sbr.map(f: 'a -> 'b): StepBuildResult<'b> =
        match sbr with
        | Fail -> Fail
        | Restart x -> Restart x
        | Success (e,c,g) -> Success(f e,c,g)

    member internal sbr.IsSuccessful =
        match sbr with
        | Success _ -> true
        | _ -> false

    member sbr.Complexity = 
        match sbr with 
        | Success (_,c,_) -> c 
        | _ -> raise (new InvalidOperationException())

type StepBuilder<'a> = GenericParamAssignments -> StepBuildResult<'a>

module StepBuilder =
    let BuildSuccess e c = 
        fun a -> Success(e, c, a) 

    let Init x =
        BuildSuccess x 0

    let BuildFail = fun s -> Fail

    let BuildRestart x = fun s -> Restart x

    let Run(f) gpa = f gpa

    let internal bind(m: StepBuilder<'a>)(f: 'a -> StepBuilder<'b>) =
        let r = fun s->
                let sbr = Run m s
                match sbr with
                | Fail -> Fail
                | Restart h -> Restart h
                | Success (e,c,a) -> 
                    let sb = f e
                    let sbr' = Run sb a
                    match sbr' with
                    | Fail -> Fail
                    | Restart x -> Restart x
                    | Success(e',c',a') -> Success(e',(c+c'),a') 
        r

    let Bind(m: StepBuilder<'a>)(f: Func<'a,StepBuilder<'b>>) =
        bind m (fun a -> f.Invoke(a))
