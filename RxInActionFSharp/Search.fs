module RxInActionFSharp.Search

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

let Search () =
    Observable.Create(fun (o:IObserver<_>) ->
        task {
            let! resultsA = File.ReadAllTextAsync("")
            for result in resultsA do
                o.OnNext(result)

            let! resultsB = File.ReadAllTextAsync("")
            for result in resultsB do
                o.OnNext(result)

            o.OnCompleted()
        }
        :>Task
    )

open System.Threading.Tasks
open System.Reactive.Threading.Tasks
let testToObservable () =
    let path fl = Path.Combine(__SOURCE_DIRECTORY__, fl+".fs")

    let resultsA = File.ReadAllLinesAsync(path "Search").ToObservable()
    let resultsB = File.ReadAllLinesAsync(path "Program").ToObservable()

    resultsA
        .Concat(resultsB)
        .FlatMap(Observable.ofSeq)
        .Subscribe(ConsoleObserver "TaskToObservable")
    |> ignore


