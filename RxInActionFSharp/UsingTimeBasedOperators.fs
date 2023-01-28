module RxInActionFSharp.UsingTimeBasedOperators

open System
open System.IO
open System.Text
open System.Collections.Generic
open System.Threading
open System.Threading.Tasks

open FSharp.Control.Reactive
open FSharp.Idioms.PointFree
open FSharp.Literals.Literal

open System.Reactive
open System.Reactive.Disposables
open System.Reactive.Observable.Aliases
open System.Reactive.Threading.Tasks
open System.Reactive.Linq
open System.Reactive.Subjects
open System.Reactive.Concurrency

let test1 () =
    let deviceHeartbeat =
        Observable.Interval(TimeSpan.FromSeconds(1))

    deviceHeartbeat
        .Take(3)
        .Timestamp()
        .Subscribe(ConsoleObserver "Heartbeat")

let test2 () =
    let deviceHeartbeat = 
        Observable
            .Timer(TimeSpan.FromSeconds(1))
            .Concat(Observable.Timer(TimeSpan.FromSeconds(2)))
            .Concat(Observable.Timer(TimeSpan.FromSeconds(4)))

    deviceHeartbeat
        .TimeInterval()
        .Subscribe(ConsoleObserver "time from last heartbeat")

let test3 () =
    let observable = 
        Observable.Timer(TimeSpan.FromSeconds(1))
            .Concat(Observable.Timer(TimeSpan.FromSeconds(1)))
            .Concat(Observable.Timer(TimeSpan.FromSeconds(4)))
            .Concat(Observable.Timer(TimeSpan.FromSeconds(4)))

    observable
        .Timeout(TimeSpan.FromSeconds(3))
        .Subscribe(ConsoleObserver "Timeout")

let test4 () =
    let observable = 
        Observable.Timer(TimeSpan.FromSeconds(1))
            .Concat(Observable.Timer(TimeSpan.FromSeconds(1)))
            .Concat(Observable.Timer(TimeSpan.FromSeconds(4)))
            .Concat(Observable.Timer(TimeSpan.FromSeconds(4)))

    observable
        .Timestamp()
        .Delay(TimeSpan.FromSeconds(2))
        .Timestamp()
        .Take(5)
        .Subscribe(ConsoleObserver "Delay")

let test5 () =
    let observable = [4; 1; 2; 3].ToObservable()

    observable
        .Timestamp()
        .Delay(fun x -> Observable.Timer(TimeSpan.FromSeconds(x.Value)))
        .Timestamp()
        .Subscribe(ConsoleObserver "Delay")
let test6 () =
    let observable = 
        Observable.Return("Update A")
            .Concat(Observable.Timer(TimeSpan.FromSeconds(2)).Map(fun _ -> "Update B"))
            .Concat(Observable.Timer(TimeSpan.FromSeconds(1)).Map(fun _ -> "Update C"))
            .Concat(Observable.Timer(TimeSpan.FromSeconds(1)).Map(fun _ -> "Update D"))
            .Concat(Observable.Timer(TimeSpan.FromSeconds(3)).Map(fun _ -> "Update E"))

    observable.Throttle(TimeSpan.FromSeconds(2))
        .Subscribe(ConsoleObserver "Throttle")

let test7 () =
    let observable = 
        Observable.Return("Msg A")
            .Concat(Observable.Timer(TimeSpan.FromSeconds(2)).Map(always "Msg B"))
            .Concat(Observable.Timer(TimeSpan.FromSeconds(1)).Map(always "Immediate Update"))
            .Concat(Observable.Timer(TimeSpan.FromSeconds(1)).Map(always "Msg D"))
            .Concat(Observable.Timer(TimeSpan.FromSeconds(3)).Map(always "Msg E"))

    observable
        .Throttle(fun x -> 
            if x = "Immediate Update"
            then Observable.Empty<int64>()
            else Observable.Timer(TimeSpan.FromSeconds(2)))
        .Subscribe(ConsoleObserver "Variable Throttling")

let test8 () =
    Observable.Interval(TimeSpan.FromSeconds(1))
        .Sample(TimeSpan.FromSeconds(3.5))
        .Take(3)
        .Subscribe(ConsoleObserver "Sample")

