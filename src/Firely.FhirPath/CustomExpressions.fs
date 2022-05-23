namespace Firely.FhirPath

open System.Linq.Expressions
open System
open TypeExtensions
open System.Linq

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
        match sourceList.Type.GetCollectionElement() with
        | Some t -> t
        | None -> raise (new InvalidOperationException("sourceList should have been an expression returning a collection."))

    member val SourceList = sourceList with get

type SingleToCollectionExpression(sourceElement: Expression) = 
    inherit Expression()
    let targetType = typedefof<System.Collections.Generic.List<_>>.MakeGenericType(sourceElement.Type)

    override e.CanReduce = false

    override e.NodeType = ExpressionType.Extension

    override e.Type = targetType

    member val SourceElement = sourceElement with get

type CollectionToCollectionExpression(sourceList: Expression, converter: LambdaExpression) = 
    inherit Expression()

    // Must look like Func<X,Y>
    let targetType =
        let targetElementType = converter.Type.GetGenericArguments().Last()
        typedefof<System.Collections.Generic.List<_>>.MakeGenericType(targetElementType)
        
    override e.CanReduce = false

    override e.NodeType = ExpressionType.Extension

    override e.Type = targetType

    member val Converter = converter with get

    member val SourceList = sourceList with get
