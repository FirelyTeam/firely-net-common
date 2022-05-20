module rec Firely.FhirPath.CastStepBuilding

open System
open System.Linq.Expressions
open TypeExtensions
open System.Collections.Generic

type GenericParam = GenericParam of string

let private buildGp(gpType: Type) = GenericParam(gpType.Name)

type GenericParamAssignments() =
    inherit Dictionary<GenericParam,Type>()

type StepBuildResult<'a> = 
    | Success of Expression:'a * Complexity:int * gps:GenericParamAssignments
    | Restart of GenericParamAssignments
    | Fail

    member sbr.bind(f: GenericParamAssignments -> 'a -> StepBuildResult<'b>): StepBuildResult<'b> = 
        match sbr with
        | Fail -> Fail
        | Restart x -> Restart x
        | Success (g,c,gps) -> 
            let m' = f gps g
            match m' with
            | Fail -> Fail
            | Restart x -> Restart x
            | Success(g',c',gps') -> Success(g',c+c',gps')

type CastStepBuilder = Expression -> Type -> GenericParamAssignments -> StepBuildResult<Expression>

let ConvertIdentity (source:Expression) target gps = 
    if source.Type = target then
        Success(source, 0, gps)
    else
        Fail

let ConvertValue source target gps = 
    try
        let conversion = Expression.Convert(source,target) :> Expression
        Success(conversion,1,gps)
    with 
        | :? InvalidOperationException -> Fail

let ConvertLambda (source: Expression) (target:Type) gps = 
    if not(source :? LambdaExpression && target.IsDelegate()) then
        Fail
    else
    
    let sourceFunc = source :?> LambdaExpression
    let sourceArgumentExpressions = Seq.map (fun p -> p :> Expression) sourceFunc.Parameters |> List.ofSeq
    let targetParameterTypes = target.GetDelegateParameters() |> List.ofArray;

    if sourceArgumentExpressions.Length <> targetParameterTypes.Length then
        Fail
    else

    let inputs = List.zip sourceArgumentExpressions targetParameterTypes
    let castParams = BuildStepsMany3 inputs gps

    match castParams with
    | Fail -> Fail
    | Restart _ -> raise (new InvalidOperationException("Cannot restart from this point, restart unexpected."))
    | Success (el,c,gpa) ->
        let resultFuncType = 
            let funcParameters = el |> List.map (fun cp -> cp.Type) |> List.toArray
            Expression.GetFuncType(funcParameters)
                
        let callWrapper = CallWrapperExpression(sourceFunc, el |> List.toArray, resultFuncType)
    
        Success(callWrapper, c, gpa)


let BuildSteps(source: Expression)(target: Type)(gp: GenericParamAssignments): StepBuildResult<Expression> =
    let start (first: CastStepBuilder) = first source target gp
    let (?=) (result: StepBuildResult<Expression>)(next: CastStepBuilder) = 
        match result with
        | Success _ -> result
        | Restart _ -> result
        | Fail -> next source target gp

    start ConvertValue
        ?= ConvertIdentity
  //      ?= ConvertLambda

type AssignmentHypotheses() = 
    inherit System.Collections.Generic.List<GenericParamAssignments>()

let isSuccess = function | Success _ -> true | _ -> false

let collectUntil (predicate: 'a->bool)(s: seq<'a>) =
  /// Iterates over the enumerator, yielding elements and
  /// stops after an element for which the predicate does not hold
  let rec loop (en:IEnumerator<_>) = seq {
    if en.MoveNext() then
      // Always yield the current, stop if predicate does not hold
      yield en.Current
      if predicate en.Current then
        yield! loop en }

  // Get enumerator of the sequence and yield all results
  // (making sure that the enumerator gets disposed)
  seq { use en = s.GetEnumerator()
        yield! loop en }

let BuildStepsMany(inputs: list<Expression*Type>)(gps: GenericParamAssignments): StepBuildResult<Expression list> =    
    let (e,t) = List.head inputs
    let seed = BuildSteps e t gps
    let scanner (acc:StepBuildResult<Expression>) (e,t) = acc.bind(fun gpa _ -> BuildSteps e t gpa)        
    let preliminaryResults = inputs.Tail |> List.scan scanner seed |> collectUntil (fun i -> isSuccess i) |> List.ofSeq

    let lastResult = List.last preliminaryResults
    let getSuccess = function | Success (x,_,_) -> x | _ -> raise (new InvalidOperationException("Only expected successess here."))
    match lastResult with
    | Success (_, c, gps) -> Success((List.map (fun (br:StepBuildResult<Expression>) -> (getSuccess br)) preliminaryResults), c, gps)
    | Fail -> Fail
    | Restart x -> Restart x

let BuildStepsMany3 (inputs: list<Expression*Type>)(gps: GenericParamAssignments): StepBuildResult<Expression list> =
    let rec tryBuild (hypotheses: AssignmentHypotheses) (inputs: list<Expression*Type>)(gps: GenericParamAssignments): StepBuildResult<Expression list> = 
        let result = BuildStepsMany inputs gps

        match result with
        | Success _ -> result
        | Fail -> result
        | Restart gpa ->
            if hypotheses.Contains(gpa) then
                Fail
            else
                hypotheses.Add(gpa)
                tryBuild hypotheses inputs gpa
        
    let firstHypotheses = AssignmentHypotheses()
    firstHypotheses.Add(gps)
    tryBuild firstHypotheses inputs gps
