namespace RxInActionFSharp

open System
open System.Reactive
open System.Reactive.Disposables

open FSharp.Control.Reactive

type ConsoleObserver<'T> (name:string) =
    interface IObserver<'T> with
        member _.OnNext(value:'T) =
            $"{name} - OnNext({value})"
            |> Console.WriteLine

        member _.OnError(error:Exception) =
            [
                $"{name} - OnError:"
                $"\t {error}"
            ]
            |> String.concat "\n"
            |> Console.WriteLine

        member _.OnCompleted() =
            $"{name} - OnCompleted()"
            |> Console.WriteLine

module ConsoleObserver =
    let test() =
        let numbers = new NumbersObservable(5);
        let subscription =
            (numbers:>IObservable<int>)
                .Subscribe(ConsoleObserver<int>("numbers") :> IObserver<_>);
        subscription.Dispose()