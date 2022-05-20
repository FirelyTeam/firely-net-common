module rec Firely.FhirPath.CastStepBuilding

open System
open System.Linq
open System.Linq.Expressions
open TypeExtensions

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
    let targetParameterTypes = target.GetDelegateParameters();

    if sourceFunc.Parameters.Count <> targetParameterTypes.Length then
        Fail
    else

    let sourceArgumentExpressions = sourceFunc.Parameters.Cast<Expression>()
    let inputs = Seq.zip sourceArgumentExpressions targetParameterTypes |> List.ofSeq
    let castParams = BuildStepsMany3 inputs gps

    match castParams with
    | Fail -> Fail
    | Restart _ -> raise (new InvalidOperationException("Cannot restart from this point, restart unexpected."))
    | Success (el,c,gpa) ->
        let resultFuncType = 
            let funcParameters = el |> List.map (fun cp -> cp.Type) |> List.toArray
            Expression.GetFuncType(funcParameters)
                
        let callWrapper = CallWrapperExpression(sourceFunc, el |> List.toArray, resultFuncType)
    
        Success(callWrapper :> Expression, c, gpa)

let ConvertGenericParam (source:Expression) (target:Type) (gps:GenericParamAssignments) = 
    if not target.IsGenericParameter then
        Fail
    else
    
    let (hasValue,assigned) = gps.TryGetValue target

    if hasValue then
        let convertToSuggestedGp = BuildSteps source assigned gps
        match convertToSuggestedGp with
        | Success _ -> convertToSuggestedGp
        | Fail -> Fail
        | Restart _ ->   // ignore suggestion from restart? Mmmm...
            let newGpa = new GenericParamAssignments(gps)
            newGpa.Add(target, source.Type)
            Restart newGpa
    else
        let newGpa = new GenericParamAssignments(gps)
        newGpa.Add(target, source.Type)
        Success(source, 0, newGpa)

let ConvertFromCollection (source:Expression) (target:Type) (gps:GenericParamAssignments) = 
    let canApproach = source.Type.IsCollection() && not (target.IsCollection())
    if not canApproach then
        Fail
    else

    let extractSingleElement = Success(CollectionToSingleExpression(source), 1, gps)
    StepBuildResult.andThen extractSingleElement (fun g e -> BuildSteps e target g)

let BuildSteps(source: Expression)(target: Type)(gp: GenericParamAssignments): StepBuildResult<Expression> =
    let start (first: CastStepBuilder) = first source target gp
    let (?=) (result: StepBuildResult<Expression>)(next: CastStepBuilder) = 
        match result with
        | Success _ -> result
        | Restart _ -> result
        | Fail -> next source target gp

    start ConvertIdentity
        ?= ConvertGenericParam
        ?= ConvertFromCollection
        ?= ConvertLambda
        ?= ConvertValue


let BuildStepsMany(inputs: list<Expression*Type>)(gps: GenericParamAssignments): StepBuildResult<Expression list> =    
    if List.isEmpty inputs then 
        Success([],0,gps)
    else
  
    let addToList (e,t) gpa l =
        let buildResult = BuildSteps e t gpa
        match buildResult with
        | Success (e,c,gps) -> Success(l @ [e],c,gps)
        | Fail -> Fail
        | Restart a -> Restart a

    let seed = addToList (List.head inputs) gps []
    let scanner2 (acc:StepBuildResult<Expression list>) p = StepBuildResult.andThen acc (fun gpa (l: Expression list) -> addToList p gpa l)
    
    inputs |> List.fold scanner2 seed


let BuildStepsMany3 (inputs: list<Expression*Type>)(gps: GenericParamAssignments): StepBuildResult<Expression list> =
    let rec tryBuild hypotheses inputs gps = 
        let result = BuildStepsMany inputs gps

        match result with
        | Success _ -> result
        | Fail -> result
        | Restart gpa ->
            if List.contains (gpa.GetHashCode()) hypotheses then
                Fail
            else
                tryBuild (hypotheses @ [gpa.GetHashCode()]) inputs gpa
        
    let firstHypotheses = [gps.GetHashCode()]
    tryBuild firstHypotheses inputs gps
