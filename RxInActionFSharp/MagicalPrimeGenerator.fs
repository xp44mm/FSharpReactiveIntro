module RxInActionFSharp.MagicalPrimeGenerator

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

type MagicalPrimeGenerator() =
    member this.Generate( amount:int): IEnumerable<int> =
        Seq.ofList [0..amount-1]

    member this.GeneratePrimes(amount:int):IObservable<_> =
        Observable.Create(fun (o:IObserver<_>) ->
            for prime in this.Generate(amount) do
                o.OnNext(prime)
            o.OnCompleted()
            Disposable.Empty
        )
    member this.CancellationGeneratePrimes(amount:int):IObservable<_> =
        let cts = new CancellationTokenSource()
        let ct = cts.Token
        Observable.Create(fun (o:IObserver<_>) ->
            Task.Run((fun () ->
                for prime in this.Generate(amount) do
                    ct.ThrowIfCancellationRequested()
                    o.OnNext(prime)
                o.OnCompleted()
            ), ct)
            |> ignore

            new CancellationDisposable(cts)
            :> IDisposable
        )
    member this.TaskGeneratePrimes(amount:int):IObservable<_> =
        Observable.Create(fun (o:IObserver<_>) (ct:CancellationToken) ->
            Task.Run(fun () ->
                for prime in this.Generate(amount) do
                    ct.ThrowIfCancellationRequested()
                    o.OnNext(prime)
                o.OnCompleted()
            )
        )


let test () =
    let generator = new MagicalPrimeGenerator()
    let _ = 
        generator
            .CancellationGeneratePrimes(5)
            .Timestamp()
            .Subscribe(ConsoleObserver "primes observable")
    Console.WriteLine("Generation is done")
    Console.ReadLine()