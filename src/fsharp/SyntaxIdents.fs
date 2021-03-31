// Copyright (c) Microsoft Corporation.  All Rights Reserved.  See License.txt in the project root for license information.

namespace FSharp.Compiler.Syntax

open System.Diagnostics
open Internal.Utilities.Library
open FSharp.Compiler.Text
open FSharp.Compiler.Text.Range

[<Struct; NoEquality; NoComparison; DebuggerDisplay("{idText}")>]
type Ident (text: string, range: range) =
     member _.idText = text
     member _.idRange = range
     override _.ToString() = text

type LongIdent = Ident list

type LongIdentWithDots =
    | LongIdentWithDots of id: LongIdent * dotRanges: range list

    member this.Range =
       match this with
       | LongIdentWithDots([], _) -> failwith "rangeOfLidwd"
       | LongIdentWithDots([id], []) -> id.idRange
       | LongIdentWithDots([id], [m]) -> unionRanges id.idRange m
       | LongIdentWithDots(h :: t, []) -> unionRanges h.idRange (List.last t).idRange
       | LongIdentWithDots(h :: t, dotRanges) -> unionRanges h.idRange (List.last t).idRange |> unionRanges (List.last dotRanges)

    member this.Lid = match this with LongIdentWithDots(lid, _) -> lid

    member this.ThereIsAnExtraDotAtTheEnd = match this with LongIdentWithDots(lid, dots) -> lid.Length = dots.Length

    member this.RangeWithoutAnyExtraDot =
       match this with
       | LongIdentWithDots([], _) -> failwith "rangeOfLidwd"
       | LongIdentWithDots([id], _) -> id.idRange
       | LongIdentWithDots(h :: t, dotRanges) ->
           let nonExtraDots = if dotRanges.Length = t.Length then dotRanges else List.truncate t.Length dotRanges
           unionRanges h.idRange (List.last t).idRange |> unionRanges (List.last nonExtraDots)

[<RequireQualifiedAccess>]
type TyparStaticReq =
    | None

    | HeadType

[<Struct; RequireQualifiedAccess>]
type SynStringKind =
    | Regular
    | Verbatim
    | TripleQuote

[<Struct; RequireQualifiedAccess>]
type SynByteStringKind =
    | Regular
    | Verbatim

