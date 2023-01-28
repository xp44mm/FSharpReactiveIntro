module RxInActionFSharp.Concurrency

open System
open System.IO
open System.Text
open System.Collections.Generic
open System.Threading
open System.Threading.Tasks
open System.Diagnostics

open FSharp.Control.Reactive
open FSharp.Idioms.PointFree
open FSharp.Literals.Literal

open System.Reactive
open System.Reactive.Disposables
open System.Reactive.Observable.Aliases
open System.Reactive.Threading.Tasks
open System.Reactive.Linq
open System.Reactive.Subjects
open System.Reactive.Concurrency
open System.Runtime


let test () =
    Observable.Interval(TimeSpan.FromSeconds(1))
        .Timestamp()
        .Filter(fun x -> x.Value % 2L = 0L)
        .Map(fun x -> x.Timestamp)
        .Subscribe(fun x -> Trace.WriteLine(x))

let test2 () =
    let timestamps =
        Observable.Interval(TimeSpan.FromSeconds(1))
            .Timestamp()
            .Filter(fun x -> x.Value % 2L = 0L)
            .Map(fun x -> x.Timestamp)
    timestamps.Subscribe(fun x -> Trace.WriteLine(x))

let test3 () =
    Observable.Interval(TimeSpan.FromSeconds(1))
        .Timestamp()
        .Filter(fun x -> x.Value % 2L = 0L)
        .Map(fun x -> x.Timestamp)
        .Subscribe(
            (fun x -> Trace.WriteLine(x)),
            (fun (ex:exn) -> Trace.WriteLine(ex)))

let DoSomethingAsync() =
    task {
      let mutable value = 13
      do! Task.Delay(TimeSpan.FromSeconds(1))
      value <- value * 2
      do! Task.Delay(TimeSpan.FromSeconds(1))
      Trace.WriteLine(value)
    }

let DoSomethingAsync2() =
    task {
      let mutable value = 13
      do! Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false)
      value <- value * 2
      do! Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false)
      Trace.WriteLine(value)
    }

//let TrySomethingAsync() =
//    task {
//      try
//        do! PossibleExceptionAsync()
//      with (ex:NotSupportedException) ->
//        LogException(ex)
//        raise(ex)
//    }
//let TrySomethingAsync2() =
//    task {
//      let task = PossibleExceptionAsync()
//      try
//        do! task
//      with (ex:NotSupportedException) ->
//        LogException(ex)
//        raise(ex)
//    }

let WaitAsync()=
    task {
      // This await will capture the current context ...
      do! Task.Delay(TimeSpan.FromSeconds(1))
      // ... and will attempt to resume the method here in that context.
    }
let Deadlock()=
    // Start the delay.
    let task = WaitAsync()
    // Synchronously block, waiting for the async method to complete.
    task.Wait()
