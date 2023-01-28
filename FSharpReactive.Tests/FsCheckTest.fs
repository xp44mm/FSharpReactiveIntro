namespace Tests

open Xunit
open Xunit.Abstractions
open FsCheck
open FSharp.Control.Reactive.Testing
open FSharp.Control.Reactive

type public FsCheckTest(output : ITestOutputHelper) =
    do Arb.register<GenTestNotification> () |> ignore

    [<Fact>]
    member __.``rev Rev Is Orig`` () =
        let revRevIsOrig (xs:list<int>) = List.rev(List.rev xs) = xs
        Check.Quick revRevIsOrig

    [<Fact>]
    member __.``Rev Is Orig`` () =
        let revIsOrig (xs:list<int>) = List.rev xs = xs
        Check.Quick revIsOrig



