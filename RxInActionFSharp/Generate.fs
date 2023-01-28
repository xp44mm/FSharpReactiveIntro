module RxInActionFSharp.Generate

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
        Observable.Generate(
            0,            // Initial state
            flip (<) 10,  // Condition (false means terminate)
            (+) 1,        // Next iteration step
            ( * ) 2)      // The value in each iteration
    observable
        .Subscribe(ConsoleObserver "Generate")
    |> ignore

