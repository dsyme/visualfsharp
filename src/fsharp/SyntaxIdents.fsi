// Copyright (c) Microsoft Corporation.  All Rights Reserved.  See License.txt in the project root for license information.

namespace rec FSharp.Compiler.Syntax

open FSharp.Compiler.Text

/// Represents an identifier in F# code
[<Struct; NoEquality; NoComparison>]
type Ident =
     new: text: string * range: range -> Ident
     member idText: string
     member idRange: range
       
/// Represents a long identifier e.g. 'A.B.C'
type LongIdent = Ident list

/// <summary>
///   Represents a long identifier with possible '.' at end.
/// </summary>
///
/// <remarks>
/// Typically dotRanges.Length = lid.Length-1, but they may be same if (incomplete) code ends in a dot, e.g. "Foo.Bar."
/// The dots mostly matter for parsing, and are typically ignored by the typechecker, but
/// if dotRanges.Length = lid.Length, then the parser must have reported an error, so the typechecker is allowed
/// more freedom about typechecking these expressions.
/// LongIdent can be empty list - it is used to denote that name of some AST element is absent (i.e. empty type name in inherit)
/// </remarks>
type LongIdentWithDots =
    | //[<Experimental("This construct is subject to change in future versions of FSharp.Compiler.Service and should only be used if no adequate alternative is available.")>]
      LongIdentWithDots of id: LongIdent * dotRanges: range list

    /// Gets the syntax range of this construct
    member Range: range

    /// Get the long ident for this construct
    member Lid: LongIdent

    /// Indicates if the construct ends in '.' due to error recovery
    member ThereIsAnExtraDotAtTheEnd: bool

    /// Gets the syntax range for part of this construct
    member RangeWithoutAnyExtraDot: range

/// Represents whether a type parameter has a static requirement or not (^T or 'T)
[<RequireQualifiedAccess>]
type TyparStaticReq =
    /// The construct is a normal type inference variable
    | None

    /// The construct is a statically inferred type inference variable '^T'
    | HeadType

/// Indicate if the string had a special format
[<Struct; RequireQualifiedAccess>]
type SynStringKind =
    | Regular
    | Verbatim
    | TripleQuote

/// Indicate if the byte string had a special format
[<Struct; RequireQualifiedAccess>]
type SynByteStringKind =
    | Regular
    | Verbatim

