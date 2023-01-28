module RxInActionFSharp.FlatMap

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

let path fl = Path.Combine(__SOURCE_DIRECTORY__, fl+".fs")

let IsPrimeAsync number = 
    File
        .ReadAllLinesAsync(path "FlatMap")
        .ToObservable()
        .Map(fun ln -> ln.Length > number)

let test () =

    Observable
        .Range(36, 9)
        .FlatMap(fun number -> 
            let isPrime = IsPrimeAsync(number)
            isPrime
                .Map(fun x -> number, x)
            )
        .Filter(snd)
        .Map(fst)
        .Subscribe(ConsoleObserver "primes")
    |> ignore

let testConcat () =
    Observable
        .Range(36, 9)
        .Map(fun number -> 
            task {
                let! lns = File.ReadAllLinesAsync(path "FlatMap")
                let isPrime = lns.Length > number
                return number,isPrime
            })
        .Concat()
        .Filter(snd)
        .Map(fst)
        .Subscribe(ConsoleObserver "primes")
    |> ignore

