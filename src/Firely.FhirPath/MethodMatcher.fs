namespace Firely.FhirPath

open System.Linq.Expressions
open System.Reflection
open CastStepBuilding
open System

type MethodMatch = MethodMatch of Result:seq<Expression> * Method:MethodInfo

module MethodMatcher = 

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



    let MatchMethods (methods: seq<MethodInfo>) name arguments: StepBuildResult<MethodMatch> =
        let selectedMethod = 
            methods |> 
            Seq.map (fun m -> (m, MatchMethod m name arguments)) |> 
            Seq.where (fun (_,r) -> r.IsSuccessful()) |>
            Seq.sortBy (function |(_,Success(_,c,_)) -> c |_ -> Int32.MaxValue) |>
            Seq.tryHead
    
        match selectedMethod with
        | None -> Fail
        | Some (m,r) -> r.andThen (fun gpa e -> Success(MethodMatch(e,m), 0, gpa))
