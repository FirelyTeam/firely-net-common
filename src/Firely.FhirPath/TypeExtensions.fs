﻿module internal Firely.FhirPath.TypeExtensions

open System
open System.Collections.Generic

type Type with
  member t.GetContainerInterface(containerType: Type): Type option =
    let interfaces = 
        try
            t.GetInterfaces()
        with
          | _ -> [||]

    seq { yield! interfaces; yield t }
    |> Seq.tryFind (fun x -> x.IsGenericType && x.GetGenericTypeDefinition() = containerType)
    
  member t.IsContainerOf(containerType: Type) = t.GetContainerInterface(containerType).IsSome
  
  member t.IsCollection() = t.IsContainerOf(typedefof<ICollection<_>>)

  member t.IsNullable() = t.GetGenericTypeDefinition() = typedefof<Nullable<_>>

  member t.GetContainerParamOf(containerType: Type): Type option = 
    let getExactlyOneArgument (t: Type) = t.GetGenericArguments() |> Array.tryExactlyOne
    t.GetContainerInterface(containerType) |> Option.bind getExactlyOneArgument

  member t.GetCollectionElement(): Type option = t.GetContainerParamOf(typedefof<ICollection<_>>)

  member t.IsDelegate(): bool = t.IsGenericType && typeof<Delegate>.IsAssignableFrom(t)

  member t.GetDelegateParameters(): Type[] = 
    if t.IsDelegate() then
      t.GetGenericArguments()
    else 
      raise <| new InvalidOperationException($"Type {t} is not a delegate type.")