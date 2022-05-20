namespace Firely.FhirPath

open System.Linq.Expressions
open System
open TypeExtensions

type CallWrapperExpression(source: LambdaExpression, parameters: Expression[], targetType: Type) = 
    inherit Expression()

    override e.CanReduce = false

    override e.NodeType = ExpressionType.Extension

    override e.Type = targetType

    member val Wrapped = source with get

    member val Parameters = parameters with get
 

 type CollectionToSingleExpression(sourceList: Expression) = 
    inherit Expression()

    override e.CanReduce = false

    override e.NodeType = ExpressionType.Extension

    override e.Type =
        let collectionElement = sourceList.Type.GetCollectionElement()
        match collectionElement with
        | Some t -> t
        | None -> raise (new InvalidOperationException("sourceList should have been an expression returning a collection."))

    member val SourceList = sourceList with get
