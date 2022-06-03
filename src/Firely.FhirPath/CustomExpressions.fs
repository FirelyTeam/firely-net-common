namespace Firely.FhirPath

open System.Linq.Expressions
open System
open System.Reflection
open TypeExtensions
open System.Linq

type CallWrapperExpression(source: Expression, parameters: Expression[], targetType: Type) = 
    inherit Expression()

    override e.CanReduce = false

    override e.NodeType = ExpressionType.Extension

    override e.Type = targetType

    member val Wrapped = source with get

    member val Parameters = parameters with get
 

 type CollectionToSingleExpression(sourceList: Expression) = 
    inherit Expression()
    static let singleOrDefault = 
        let methods = typeof<Enumerable>.GetMethods(BindingFlags.Static ||| BindingFlags.Public)
        let methodSelector = fun (m:MethodInfo) -> m.Name = "SingleOrDefault" && m.GetParameters().Length = 1
        Array.find methodSelector methods
    
    static member ElementType(listType: Type): Type = 
        match listType.GetCollectionElement() with
        | Some t -> t
        | None -> raise (new InvalidOperationException("sourceList should have been an expression returning a collection."))

    override e.CanReduce = true

    override e.NodeType = ExpressionType.Extension

    override e.Type = CollectionToSingleExpression.ElementType(sourceList.Type)
       
    member val SourceList = sourceList with get

    override e.Reduce(): Expression = 
        let method = singleOrDefault.MakeGenericMethod(e.Type)
        Expression.Call(method, e.SourceList)        

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
