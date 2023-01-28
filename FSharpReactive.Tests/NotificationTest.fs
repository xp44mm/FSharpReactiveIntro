namespace Tests

open Xunit
open Xunit.Abstractions
open FsCheck
open FSharp.Control.Reactive.Testing
open FSharp.Control.Reactive

type public NotificationTest(output : ITestOutputHelper) =
    do Arb.register<GenTestNotification> () |> ignore

    [<Fact>]
    member __.``Functor Law of Observables`` () =
        Check.QuickThrowOnFailure <|
            fun (ms : TestNotifications<int>) (f : int -> int) (g : int -> int) ->
                TestSchedule.usage <| fun sch ->
                    TestSchedule.hotObservable sch ms
                    |> Observable.retry
                    |> Observable.map f
                    |> Observable.map g
                    |> TestSchedule.subscribeTestObserverStart sch
                    |> TestObserver.nexts = TestNotification.mapNexts (f >> g) ms
