module RxInActionFSharp.NumbersAndThrow

open System
open System.Reactive

open System.Reactive.Disposables
open System.Reactive.Observable.Aliases

open FSharp.Control.Reactive
open System.Reactive.Linq

let NumbersAndThrow() =
    seq {
        1
        2
        3
        raise(ApplicationException("Something Bad Happened"))
        4
    }

let test () =
    NumbersAndThrow()
        .ToObservable()
        .Subscribe(ConsoleObserver "names")
    |> ignore

let test2() =
    let names = seq {"Shira"; "Yonatan"; "Gabi"; "Tamir"}
    let observable = names.ToObservable();
    observable.Subscribe(ConsoleObserver "names")
    |> ignore

let test3() =
    let names = seq {"Shira"; "Yonatan"; "Gabi"; "Tamir"}
    names.Subscribe(new ConsoleObserver<string>("subscribe"))
    |> ignore
