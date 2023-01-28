module RxInActionFSharp.ToLookup

open System
open System.Reactive
open System.Collections.Generic

open System.Reactive.Disposables
open System.Reactive.Observable.Aliases

open FSharp.Control.Reactive
open System.Reactive.Linq
open System.Text

let test () =
    let cities = seq { "London"; "Tel-Aviv"; "Tokyo"; "Rome" }
    let lookupObservable =
        cities
            .ToObservable()
            .ToLookup(fun c -> c.Length)
    lookupObservable
        .Map(fun lookup ->
            let outp = new StringBuilder()
            for grp in lookup do
                outp.AppendFormat("[Key:{0} => {1}]",grp.Key, Seq.length grp)
                |> ignore
            outp.ToString()
        )
        .Subscribe(ConsoleObserver "lookup")
    |> ignore
