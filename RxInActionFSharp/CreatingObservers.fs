module RxInActionFSharp.CreatingObservers

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
    Observable
        .Range(1, 5)
        .Map(fun x -> x/(x - 3))
        .Subscribe({
            new IObserver<_> with
                member this.OnNext(x) = Console.WriteLine($"{x}")
                member this.OnError err = Console.WriteLine(err.Message)
                member this.OnCompleted() = 
                    Console.WriteLine("done!")
        })
    |> ignore

let testOnNext () =
    Observable
        .Range(1, 5)
        .Map(fun x -> Task.Run(fun () -> x / (x - 3)))
        .Concat()
        .Subscribe(fun x -> Console.WriteLine($"{x}"))
    |> ignore

let testCancellationTokenSource() =
    let cts = new CancellationTokenSource();
    cts.Token.Register(fun () -> 
        Console.WriteLine("Subscription canceled"))
    |> ignore

    Observable
        .Interval(TimeSpan.FromSeconds(1))
        .Subscribe(cts.Token)

    cts.CancelAfter(TimeSpan.FromSeconds(5))

let test3() =
    let observer = Observer.Create<string>(fun x -> Console.WriteLine(x))
    
    Observable
        .Interval(TimeSpan.FromSeconds(1))
        .Select(fun x -> $"X{x}")
        .Subscribe(observer)
    |> ignore

    Observable
        .Interval(TimeSpan.FromSeconds(2))
        .Select(fun x -> $"YY{x}")
        .Subscribe(observer)
    |> ignore

