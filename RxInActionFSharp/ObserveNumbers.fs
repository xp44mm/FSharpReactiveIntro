module RxInActionFSharp.ObserveNumbers

open System
open System.Reactive

open System.Reactive.Disposables
open System.Reactive.Observable.Aliases

open FSharp.Control.Reactive

open System.Reactive.Linq
let ObserveNumbers(amount:int):IObservable<int> =
    Observable.Create<int>(fun (observer:IObserver<_>) ->
        for i in [0.. amount-1] do
            observer.OnNext(i)
        observer.OnCompleted()
        Disposable.Empty
        )

let test() =
    let numbers = ObserveNumbers(5)
    let subscription =
        numbers.Subscribe(ConsoleObserver<int>("ObserveNumbers") :> IObserver<_>)
    subscription.Dispose()