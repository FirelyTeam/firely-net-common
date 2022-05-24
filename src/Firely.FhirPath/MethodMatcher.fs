module Firely.FhirPath.MethodMatcher

open System.Linq.Expressions
open System.Reflection
open CastStepBuilding
open System

let MatchMethod (method: MethodInfo) name arguments: StepBuildResult<seq<Expression>> =
    if method.Name <> name then
        Fail
    else

    let parameters = method.GetParameters() |> Array.map (fun p -> p.ParameterType)
    if (Seq.length arguments) <> (Array.length parameters) then
        Fail
    else

    let pairs = Seq.zip arguments parameters
    BuildCastMany (pairs,(new GenericParamAssignments()))

type MatchResult = 
    | Match of Result:StepBuildResult<seq<Expression>> * Method:MethodInfo
    | NoMatch

let MatchMethods (methods: seq<MethodInfo>) name arguments: MatchResult =
    let selectedMethod = 
        methods |> 
        Seq.map (fun m -> (m, MatchMethod m name arguments)) |> 
        Seq.where (fun (_,r) -> r.IsSuccessful()) |>
        Seq.sortBy (function |(_,Success(_,c,_)) -> c |_ -> Int32.MaxValue) |>
        Seq.tryHead
    
    match selectedMethod with
    | None -> NoMatch
    | Some (m,r) -> Match(r,m)
