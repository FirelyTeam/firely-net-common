namespace Firely.FhirPath

open System.Linq.Expressions
open System.Reflection
open CastStepBuilding
open System

type MethodMatch = MethodMatch of Result:seq<ExpressionGenerator> * Method:MethodInfo

module MethodMatcher = 

    let MatchMethod (method: MethodInfo) name arguments gps: StepBuildResult<seq<ExpressionGenerator>> =
        if method.Name <> name then
            Fail
        else

        let parameters = method.GetParameters() |> Array.map (fun p -> p.ParameterType)
        if (Seq.length arguments) <> (Array.length parameters) then
            Fail
        else

        let pairs = Seq.zip arguments parameters
        BuildCastMany pairs gps


    let CreateMethodMatch (mi:MethodInfo) (gpa:GenericParamAssignments) =
        let orderedParams = 
            gpa 
            |> Seq.sortBy (fun kvp -> kvp.Key.GenericParameterPosition) 
            |> Seq.map (fun kvp -> kvp.Value)
            |> Array.ofSeq
        mi.MakeGenericMethod(orderedParams)

    let MatchMethods (methods: seq<MethodInfo>) name arguments: StepBuildResult<MethodMatch> =
        let methodMatchResultMapper mi =
            let sbResult = MatchMethod mi name arguments (new GenericParamAssignments())
            (mi, sbResult)

        let selectedMethod = 
            methods |> 
            Seq.map methodMatchResultMapper |>
            Seq.where (fun (_,r) -> r.IsSuccessful) |>
            Seq.sortBy (function |(_,Success(_,c,_)) -> c |_ -> Int32.MaxValue) |>
            Seq.tryHead
    
        match selectedMethod with
        | None -> Fail
        | Some (mi,Success(exp,c,a)) ->
            let mm = MethodMatch(exp, mi)
            Success(mm,c,a)
        | Some(_,_) -> raise <| new InvalidOperationException("Encountered unexpected non-successful method match.")