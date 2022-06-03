module rec Firely.FhirPath.CastStepBuilding

open System
open System.Linq
open System.Linq.Expressions
open TypeExtensions

type ExpressionGenerator = Expression -> Expression


/// <summary>
/// The trivial cast where no conversion is necessary, is tried first to short-cut building of
/// more complex conversions.
/// </summary
let CastNoCast (source:Type) target = 
    if source = target then
       StepBuilder.Init id
    else
       StepBuilder.BuildFail

//let CastFromNullable (source:Expression) (target:Type) gps =
//    let supported = source.Type.IsNullable() && not(target.IsNullable())


//let CastValue (source:Type) target gps = 
//    let supported = source = typeof<int> && target = typeof<int64>
//    if not supported then 
//        Fail
//    else

//    try
//        // TODO: This is actually quite a lot more flexible than the casts allowed by FhirPath
//        // so we should set some hard limits here
//        // And if we don't allow long->int, we don't need 'Checked' either.
//        let conversion = fun e -> Expression.ConvertChecked(e,target) :> Expression
//        Success(Generator(conversion),2,gps)
//    with 
//        | :? InvalidOperationException -> Fail
//        | :? ArgumentException -> Fail

//let CastLambda (source: Expression) (target:Type) gps = 
//    let supported = source.Type.IsDelegate() && target.IsDelegate()
    
//    if not supported then
//        Fail
//    else
    
//    let sourceParameterTypes = source.Type.GetDelegateParameters();
//    let targetParameterTypes = target.GetDelegateParameters();

//    if sourceParameterTypes.Length <> targetParameterTypes.Length then
//        Fail
//    else

//    let sourceArgumentExpressions = Seq.map (fun t -> Expression.Parameter(t) :> Expression) sourceParameterTypes
//    let inputs = Seq.zip sourceArgumentExpressions targetParameterTypes |> List.ofSeq
//    let castParams = buildCastMany inputs gps

//    match castParams with
//    | Fail -> Fail
//    | Restart _ -> raise (new InvalidOperationException("Cannot restart from this point, restart unexpected."))
//    | Success (el,c,gpa) ->
//        let resultFuncType = 
//            let funcParameters = el |> List.map (fun cp -> cp.Type) |> List.toArray
//            Expression.GetFuncType(funcParameters)
                
//        let callWrapper = CallWrapperExpression(source, el |> List.toArray, resultFuncType)
    
//        Success(callWrapper :> Expression, c+2, gpa)

//let CastGenericParam (source:Expression) (target:Type) (gps:GenericParamAssignments) = 
//    if not target.IsGenericParameter then
//        Fail
//    else
    
//    let (hasValue,assigned) = gps.TryGetValue target

//    if hasValue then
//        let convertToSuggestedGp = BuildCast source assigned gps
//        match convertToSuggestedGp with
//        | Success _ -> convertToSuggestedGp
//        | Fail -> 
//            let newGpa = new GenericParamAssignments(gps)
//            newGpa.Remove(target) |> ignore
//            newGpa.Add(target, source.Type)
//            Restart newGpa
//        | Restart a -> Restart a
           
//    else
//        let newGpa = new GenericParamAssignments(gps)
//        newGpa.Add(target, source.Type)
//        Success(source, 1, newGpa)

//let CastFromCollection (source:Type) (target:Type) (gps:GenericParamAssignments) = 
//    let canApproach = source.IsCollection() && not (target.IsCollection())
//    if not canApproach then
//        Fail
//    else

//    let elementType = CollectionToSingleExpression.ElementType(source)
//    let convertElement = BuildCast source elementType gps
   
//    convertElement.andThen (fun g (Generator(c)) -> 
//        let generator = fun e -> c(CollectionToSingleExpression(e))
//        Success(Generator(generator), 2, g))

//let CastToCollection (source:Expression) (target:Type) (gps:GenericParamAssignments) = 
//    let canApproach = not (source.Type.IsCollection()) && target.IsCollection()
//    if not canApproach then
//        Fail
//    else

//    let targetElementType = target.GetCollectionElement().Value;
//    let convertSingleElement = BuildCast source targetElementType gps
//    let exprBuilder g e = Success(SingleToCollectionExpression(e) :> Expression,2,g)
//    convertSingleElement.andThen exprBuilder


//let CastInterCollection (source:Expression) (target:Type) (gps:GenericParamAssignments) = 
//    let canApproach = source.Type.IsCollection() && target.IsCollection()
//    if not canApproach then
//        Fail
//    else

//    let sourceElementType = source.Type.GetCollectionElement().Value;
//    let targetElementType = target.GetCollectionElement().Value;
    
//    let elementConversionParam = Expression.Parameter(sourceElementType)
//    let elementConversion = BuildCast elementConversionParam targetElementType gps
//    let converterBuilder g (e:Expression) =
//        let convertedElementType = e.Type
//        let delegateType = Expression.GetFuncType([| sourceElementType; convertedElementType |])
//        let lambdaExpression = Expression.Lambda(delegateType,e,elementConversionParam)
//        let converter = CollectionToCollectionExpression(source, lambdaExpression) :> Expression
//        Success(converter, 2, gps)
//    elementConversion.andThen converterBuilder


let internal buildCast(source: Type)(target: Type): StepBuilder<ExpressionGenerator> =
    let start first = first source target
    
    // This operator tries a cast, and goes on to try the next if it fails.
    let (?=) result next = 
        match result with
        | Success _ -> result
        | Restart _ -> result
        | Fail -> next source target

    start CastNoCast
       // ?= CastGenericParam
       // ?= CastFromCollection
       // ?= CastToCollection
       // ?= CastInterCollection
       // ?= CastLambda
       // ?= CastValue

let BuildCast(source: Type)(target: Type)(gpa: GenericParamAssignments) =
    StepBuilder.Run (buildCast source target) gpa 

let private attemptBuildStepsMany (inputs: list<Type*Type>):
                                            StepBuilder<ExpressionGenerator list> =    
    // Trivially, a function with no parameters needs no conversions
    if List.isEmpty inputs then 
        StepBuilder.Init []
    else
  
    // Given the list l of parameter conversions so far and a new conversion from e to t for the next parameter, 
    // build the conversion for the new parameter and add the generated casting expression to the list l,
    // creating a new StepBuildResult.
    let addToList (e,t) l =
        let buildResult = buildCast e t
        StepBuilder.bind buildResult (fun e -> StepBuilder.BuildSuccess (l @ [e]) 0)

    // The seed is the converted version of the first (expression,type) pair for the first parameter in the input
    let seed = addToList (List.head inputs) []

    // Build up the list of expressions for the casta, one for each parameter. The functions forwards the 'gpa', 
    // the list of generic parameter assignments, from each converted parameter to the next, since the gpa is
    // shared by each parameter (to satisfy all generic parameters across the parameters of the function).
    let convertAndAdd (acc:StepBuilder<ExpressionGenerator list>) p = 
        StepBuilder.bind acc (fun l -> addToList p l)

    // Now run the converter to find the casts for the rest of the parameters.
    inputs.Tail |> List.fold convertAndAdd seed

/// <summary>
/// Finds a cast given a list of required casts, one for each parameter of a function. The list of required casts is expressed
/// as a list of tuples (Type,Type), where the first Type is for the argument passed to the function,
/// and the second Type for the corresponding parameter of the function.
/// </summary>
let internal buildCastMany' (inputs: list<Type*Type>)(gpsi: GenericParamAssignments): 
                        StepBuildResult<ExpressionGenerator list> =
    // given a hypotheses (set of generic parameter assignments),
    // try building the casts.
    let rec tryBuild hypotheses inputs gps =
        let resultBuilder = (attemptBuildStepsMany inputs)
        let result = StepBuilder.Run resultBuilder gps

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
    let hypothesesSeen = [gpsi.GetHashCode()]

    // Start with the initial hypotheses given to us.
    tryBuild hypothesesSeen inputs gpsi

let internal buildCastMany i = buildCastMany' i

let BuildCastMany (inputs: seq<Tuple<Type,Type>>)(gps:GenericParamAssignments) =
    let inputs' = List.ofSeq inputs
    let r = buildCastMany' inputs' gps
    r.map (fun el -> el :> seq<ExpressionGenerator>)

