module RxInActionFSharp.Range

open System
open System.Reactive
open System.Collections.Generic

open System.Reactive.Disposables
open System.Reactive.Observable.Aliases

open FSharp.Control.Reactive
open System.Reactive.Linq
open System.Text
open FSharp.Idioms.PointFree

let test () =
    let observable:IObservable<int> =
        Observable
            .Range(0, 10)
            .Map(( * ) 2)
    observable.Subscribe(ConsoleObserver "Range")
    |> ignore
