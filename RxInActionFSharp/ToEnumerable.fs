module RxInActionFSharp.ToEnumerable

open System
open System.Reactive
open System.Collections.Generic

open System.Reactive.Disposables
open System.Reactive.Observable.Aliases

open FSharp.Control.Reactive
open System.Reactive.Linq

let test () =
    let observable =
        Observable.Create<string>(fun (o:IObserver<_>) ->
            o.OnNext("Observable")
            o.OnNext("To")
            o.OnNext("Enumerable")
            o.OnCompleted()
            Disposable.Empty
        )

    let enumerable = observable.ToEnumerable()

    for item in enumerable do
        Console.WriteLine(item)

let testToList() =
    let observable =
        Observable.Create<string>(fun (o:IObserver<_>) ->
            o.OnNext("Observable")
            o.OnNext("To")
            o.OnNext("List")
            o.OnCompleted()
            Disposable.Empty
        );
    let listObservable:IObservable<IList<string>> =
        observable.ToList()
    listObservable
        .Select(fun lst -> String.Join(",", lst))
        .Subscribe(ConsoleObserver"list ready")
    |> ignore