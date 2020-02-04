// Copyright (c) Microsoft Corporation.  All Rights Reserved.  See License.txt in the project root for license information.

namespace Internal.Utilities.Collections

open System.Collections.Generic
open Microsoft.FSharp.Collections
                                 
// Each entry in the HashMultiMap dictionary has at least one entry. Under normal usage each entry has _only_
// one entry. So use two hash tables: one for the main entries and one for the overflow.
[<Sealed>]
type internal HashMultiMap<'Key,'Value>(n: int, hashEq: IEqualityComparer<'Key>) = 

    let firstEntries = Dictionary<_,_>(n,hashEq)

    let rest = Dictionary<_,_>(3,hashEq)
 
    let getRest key =
        match rest.TryGetValue key with
        | true, res -> res
        | _ -> []

    new (hashEq : IEqualityComparer<'Key>) = HashMultiMap<'Key,'Value>(11, hashEq)

    new (seq : seq<'Key * 'Value>, hashEq : IEqualityComparer<'Key>) as x = 
        new HashMultiMap<'Key,'Value>(11, hashEq)
        then seq |> Seq.iter (fun (k,v) -> x.Add(k,v))

    member __.Add(y,z) = 
        match firstEntries.TryGetValue y with
        | true, res ->
            rest.[y] <- res :: getRest y
        | _ -> ()
        firstEntries.[y] <- z

    member __.Clear() = 
         firstEntries.Clear()
         rest.Clear()

    member __.RemoveAll key = 
         firstEntries.Remove key |> ignore
         rest.Remove key |> ignore

    member t.SetAll(key, values) = 
         match values with 
         | [] -> 
             t.RemoveAll key
         | [h] -> 
             firstEntries.[key] <- h
             rest.Remove key |> ignore
         | h::t -> 
             firstEntries.[key] <- h
             rest.[key] <- t

    member __.FirstEntries = firstEntries

    member __.Rest = rest

    member __.Copy() = 
        let res = HashMultiMap<'Key,'Value>(firstEntries.Count,firstEntries.Comparer)
        for kvp in firstEntries do 
             res.FirstEntries.Add(kvp.Key,kvp.Value)

        for kvp in rest do 
             res.Rest.Add(kvp.Key,kvp.Value)
        res

    member x.Item 
        with get (key : 'Key) = 
            match firstEntries.TryGetValue key with
            | true, res -> res
            | _ -> raise (KeyNotFoundException("The item was not found in collection"))
        and set (key:'Key) (z:'Value) = 
            x.ReplaceLatest(key, z)

    member __.FindAll key = 
        match firstEntries.TryGetValue key with
        | true, res -> res :: getRest key
        | _ -> []

    member __.Fold f acc = 
        let mutable res = acc
        for kvp in firstEntries do
            res <- f kvp.Key kvp.Value res
            match getRest kvp.Key with
            | [] -> ()
            | rest -> 
                for z in rest do
                    res <- f kvp.Key z res
        res

    member __.Iterate(f) =  
        for kvp in firstEntries do
            f kvp.Key kvp.Value
            match getRest kvp.Key with
            | [] -> ()
            | rest -> 
                for z in rest do
                    f kvp.Key z

    member __.ContainsKey key = firstEntries.ContainsKey key

    member __.RemoveLatest key = 
        match firstEntries.TryGetValue key with
        // NOTE: If not ok then nothing to remove - nop
        | true, _res ->
            // We drop the FirstEntry. Here we compute the new FirstEntry and residue MoreEntries
            match rest.TryGetValue key with
            | true, res ->
                match res with 
                | [h] -> 
                    firstEntries.[key] <- h; 
                    rest.Remove(key) |> ignore
                | (h :: t) -> 
                    firstEntries.[key] <- h
                    rest.[key] <- t
                | _ -> 
                    ()
            | _ ->
                firstEntries.Remove(key) |> ignore 
        | _ -> ()

    member __.ReplaceLatest(key, z) = 
        firstEntries.[key] <- z

    member __.TryFind key =
        match firstEntries.TryGetValue key with
        | true, res -> Some res
        | _ -> None

    member __.Count = firstEntries.Count

    interface IEnumerable<KeyValuePair<'Key, 'Value>> with

        member __.GetEnumerator() = 
            let elems = List<_>(firstEntries.Count + rest.Count)
            for kvp in firstEntries do
                elems.Add(kvp)
                for z in getRest kvp.Key do
                   elems.Add(KeyValuePair(kvp.Key, z))
            (elems.GetEnumerator() :> IEnumerator<_>)

    interface System.Collections.IEnumerable with

        member s.GetEnumerator() = ((s :> seq<_>).GetEnumerator() :> System.Collections.IEnumerator)

    interface IDictionary<'Key, 'Value> with 

        member s.Item 
            with get x = s.[x]            
            and  set x v = s.[x] <- v
            
        member s.Keys = ([| for kvp in s -> kvp.Key |] :> ICollection<'Key>)

        member s.Values = ([| for kvp in s -> kvp.Value |] :> ICollection<'Value>)

        member s.Add(k,v) = s.[k] <- v

        member s.ContainsKey(k) = s.ContainsKey(k)

        member s.TryGetValue(k,r) = match s.TryFind k with Some v-> (r <- v; true) | _ -> false

        member s.Remove(key:'Key) =
            let res = s.ContainsKey key
            s.RemoveLatest key
            res

    interface ICollection<KeyValuePair<'Key, 'Value>> with 

        member s.Add(x) = s.[x.Key] <- x.Value

        member s.Clear() = s.Clear()            

        member s.Remove(x) = 
            match s.TryFind x.Key with
            | Some v -> 
                if Unchecked.equals v x.Value then
                    s.RemoveLatest x.Key
                true
            | _ -> false

        member s.Contains(x) =
            match s.TryFind x.Key with
            | Some v when Unchecked.equals v x.Value -> true
            | _ -> false

        member s.CopyTo(arr,arrIndex) = s |> Seq.iteri (fun j x -> arr.[arrIndex+j] <- x)

        member s.IsReadOnly = false

        member s.Count = s.Count

