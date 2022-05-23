module rec Firely.FhirPath.CastStepBuilding

open System
open System.Linq
open System.Linq.Expressions
open TypeExtensions

/// <summary>
/// Codifies the function signature of a function that builds a Linq.Expression to convert from an
/// Expression representing an argument of a type to a parameter of another type.
/// </summary>
type CastStepBuilder = Expression -> Type -> GenericParamAssignments -> StepBuildResult<Expression>


/// <summary>
/// The trivial cast where no conversion is necessary, is tried first to short-cut building of
/// more complex conversions.
/// </summary
let CastNoCast (source:Expression) target gps = 
    if source.Type = target then
        Success(source, 0, gps)
    else
        Fail

let CastValue source target gps = 
    try
        // TODO: This is actually quite a lot more flexible than the casts allowed by FhirPath
        // so we should set some hard limits here
        let conversion = Expression.ConvertChecked(source,target) :> Expression
        Success(conversion,1,gps)
    with 
        | :? InvalidOperationException -> Fail

let CastLambda (source: Expression) (target:Type) gps = 
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
    let castParams = BuildCastMany inputs gps

    match castParams with
    | Fail -> Fail
    | Restart _ -> raise (new InvalidOperationException("Cannot restart from this point, restart unexpected."))
    | Success (el,c,gpa) ->
        let resultFuncType = 
            let funcParameters = el |> List.map (fun cp -> cp.Type) |> List.toArray
            Expression.GetFuncType(funcParameters)
                
        let callWrapper = CallWrapperExpression(sourceFunc, el |> List.toArray, resultFuncType)
    
        Success(callWrapper :> Expression, c, gpa)

let CastGenericParam (source:Expression) (target:Type) (gps:GenericParamAssignments) = 
    if not target.IsGenericParameter then
        Fail
    else
    
    let (hasValue,assigned) = gps.TryGetValue target

    if hasValue then
        let convertToSuggestedGp = BuildCast source assigned gps
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

let CastFromCollection (source:Expression) (target:Type) (gps:GenericParamAssignments) = 
    let canApproach = source.Type.IsCollection() && not (target.IsCollection())
    if not canApproach then
        Fail
    else

    let extractSingleElement = Success(CollectionToSingleExpression(source), 1, gps)
    extractSingleElement.andThen (fun g e -> BuildCast e target g)


let CastToCollection (source:Expression) (target:Type) (gps:GenericParamAssignments) = 
    let canApproach = not (source.Type.IsCollection()) && target.IsCollection()
    if not canApproach then
        Fail
    else

    let targetElementType = target.GetCollectionElement().Value;
    let convertSingleElement = BuildCast source targetElementType gps
    let exprBuilder g e = Success(SingleToCollectionExpression(e) :> Expression,1,g)
    convertSingleElement.andThen exprBuilder


let CastInterCollection (source:Expression) (target:Type) (gps:GenericParamAssignments) = 
    let canApproach = source.Type.IsCollection() && target.IsCollection()
    if not canApproach then
        Fail
    else

    let sourceElementType = source.Type.GetCollectionElement().Value;
    let targetElementType = target.GetCollectionElement().Value;
    
    let elementConversionParam = Expression.Parameter(sourceElementType)
    let elementConversion = BuildCast elementConversionParam targetElementType gps
    let converterBuilder g e =         
        let delegateType = Expression.GetFuncType([| sourceElementType; targetElementType |])
        let lambdaExpression = Expression.Lambda(delegateType,e,elementConversionParam)
        let converter = CollectionToCollectionExpression(source, lambdaExpression) :> Expression
        Success(converter, 2, gps)
    elementConversion.andThen converterBuilder


let BuildCast(source: Expression)(target: Type)(gp: GenericParamAssignments): StepBuildResult<Expression> =
    let start first = first source target gp
    
    // This operator tries a cast, and goes on to try the next if it fails.
    let (?=) result next = 
        match result with
        | Success _ -> result
        | Restart _ -> result
        | Fail -> next source target gp

    start CastNoCast
        ?= CastGenericParam
        ?= CastFromCollection
        ?= CastToCollection
        ?= CastInterCollection
        ?= CastLambda
        ?= CastValue

let private attemptBuildStepsMany(inputs: list<Expression*Type>)(gps: GenericParamAssignments): StepBuildResult<Expression list> =    
    // Trivially, a function with no parameters needs no conversions
    if List.isEmpty inputs then 
        Success([],0,gps)
    else
  
    // Given the list l of parameter conversions so far and a new conversion from e to t for the next parameter, 
    // build the conversion for the new parameter and add the generated casting expression to the list l,
    // creating a new StepBuildResult.
    let addToList (e,t) gpa l =
        let buildResult = BuildCast e t gpa
        buildResult.andThen (fun gpa e -> Success(l @ [e], 0, gpa))

    // The seed is the converted version of the first (expression,type) pair for the first parameter in the input
    let seed = addToList (List.head inputs) gps []

    // Build up the list of expressions for the casta, one for each parameter. The functions forwards the 'gpa', 
    // the list of generic parameter assignments, from each converted parameter to the next, since the gpa is
    // shared by each parameter (to satisfy all generic parameters across the parameters of the function).
    let convertAndAdd (acc:StepBuildResult<Expression list>) p = acc.andThen (fun gpa l -> addToList p gpa l)

    // Now run the converter to find the casts for the rest of the parameters.
    inputs.Tail |> List.fold convertAndAdd seed

/// <summary>
/// Finds a cast given a list of required casts, one for each parameter of a function. The list of required casts is expressed
/// as a list of tuples (Expression,Type), where the Expression is the expression for the argument passed to the function,
/// and the Type the the type of the corresponding argument of the function.
/// </summary>
let BuildCastMany (inputs: list<Expression*Type>)(gps: GenericParamAssignments): StepBuildResult<Expression list> =
    // given a hypotheses (set of generic parameter assignments),
    // try building the casts.
    let rec tryBuild hypotheses inputs gps = 
        let result = attemptBuildStepsMany inputs gps

        match result with
        | Success _ -> result
        | Fail -> result
        | Restart gpa ->
            // If this set of generic parameter assignments does not work out,
            // use the newly suggested set, unless we've seen that before
            // (which means we're in a loop).
            if List.contains (gpa.GetHashCode()) hypotheses then
                Fail
            else
                tryBuild (hypotheses @ [gpa.GetHashCode()]) inputs gpa
    
    // List of hypotheses (fingerprinted by hash code) that we have tried to far.
    // Used to make sure we're not getting into a loop.
    let hypothesesSeen = [gps.GetHashCode()]

    // Start with the initial hypotheses given to us.
    tryBuild hypothesesSeen inputs gps
