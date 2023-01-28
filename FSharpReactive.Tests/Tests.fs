module Tests.broadcast

open System
open Xunit

open FsCheck
open FSharp.Control.Reactive.Testing
open FSharp.Control.Reactive

[<Fact>]
let ``Broadcast Subject broadcast to all observers`` () =
    Check.QuickThrowOnFailure <| fun (xs : int list) ->
        TestSchedule.usage <| fun sch ->
            use s = Subject.broadcast
            let observer = TestSchedule.subscribeTestObserver sch s

            Subject.onNexts  xs s
            |> Subject.onCompleted
            |> ignore

            TestObserver.nexts observer = xs
