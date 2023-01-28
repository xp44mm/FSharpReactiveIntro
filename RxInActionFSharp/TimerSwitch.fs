module RxInActionFSharp.TimerSwitch

open System
open System.IO

open System.Reactive
open System.Collections.Generic

open System.Reactive.Disposables
open System.Reactive.Observable.Aliases

open FSharp.Control.Reactive
open System.Reactive.Linq
open System.Text
open FSharp.Idioms.PointFree
open FSharp.Literals.Literal
open System.Threading
open System.Threading.Tasks
open System.Reactive.Threading.Tasks

let test () =
    let firstObservable:IObservable<string> =
        Observable
            .Interval(TimeSpan.FromSeconds(1))
            .Select(fun x -> $"value{x}")

    let secondObservable:IObservable<string> =
        Observable
            .Interval(TimeSpan.FromSeconds(2))
            .Select(fun x -> $"second{x}")
            .Take(5)

    let immediateObservable:IObservable<IObservable<string>> =
        Observable.Return(firstObservable)

    //Scheduling the second observable emission
    let scheduledObservable:IObservable<IObservable<string>> =
        Observable
            .Timer(TimeSpan.FromSeconds(5))
            .Select(fun x -> secondObservable)

    immediateObservable
        .Merge(scheduledObservable)
        .Switch()
        .Timestamp()
        .Subscribe(ConsoleObserver "timer switch")

    |> ignore