module RxInActionFSharp.ToDictionary

open System
open System.Reactive
open System.Collections.Generic

open System.Reactive.Disposables
open System.Reactive.Observable.Aliases

open FSharp.Control.Reactive
open System.Reactive.Linq

let test () =
    let cities = seq { "London"; "Tel-Aviv"; "Tokyo"; "Rome" }
    let dictionaryObservable =
        cities
            .ToObservable()
            .ToDictionary(fun c -> c.Length)
    dictionaryObservable
        .Map(fun d -> String.Join(",", d))
        .Subscribe(ConsoleObserver "dictionary")
    |> ignore
