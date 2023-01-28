namespace RxInActionFSharp

open System
open System.Reactive
open System.Reactive.Disposables

open FSharp.Control.Reactive

type NumbersObservable(amount) =
    interface IObservable<int> with
        member _.Subscribe(observer:IObserver<int>) =
            [0..amount-1]
            |> Seq.iter(fun i -> observer.OnNext i)

            observer.OnCompleted()
            Disposable.Empty
