module RxInActionFSharp.Subject

open System
open System.IO
open System.Text
open System.Collections.Generic
open System.Threading
open System.Threading.Tasks


open FSharp.Idioms.PointFree
open FSharp.Literals.Literal
open FSharp.Control.Reactive

open System.Reactive
open System.Reactive.Disposables
open System.Reactive.Observable.Aliases
open System.Reactive.Threading.Tasks
open System.Reactive.Linq
open System.Reactive.Subjects

let test()=
    let sbj = new Subject<int>()
    sbj.Subscribe(ConsoleObserver "First")
    |> ignore
    sbj.Subscribe(ConsoleObserver "Second")
    |> ignore

    sbj.OnNext(1)
    sbj.OnNext(2)
    sbj.OnCompleted()

let test2()=
    let sbj = new Subject<string>()

    Observable.Interval(TimeSpan.FromSeconds(1))
        .Map(fun x -> $"First: {x}")
        .Take(5)
        .Subscribe(sbj)
    |> ignore

    Observable.Interval(TimeSpan.FromSeconds(2))
        .Map(fun x -> $"Second: {x}")
        .Take(5)
        .Subscribe(sbj)
    |> ignore

    sbj.Subscribe(ConsoleObserver "sbj")
    |> ignore

let test3() =
    let sbj = new Subject<string>()
    sbj.Subscribe(ConsoleObserver "sbj")
    |> ignore

    //at some point later...

    let messagesFromDb = seq {"a";"b"}
    let realTimeMessages = 
        Observable.Range(1,5).Map(fun x -> x.ToString())

    messagesFromDb.ToObservable().Subscribe(sbj)
    |> ignore

    realTimeMessages.Subscribe(sbj)
    |> ignore

let test4 () =
    let tcs = new TaskCompletionSource<bool>()
    let task = tcs.Task

    let sbj = new AsyncSubject<bool>()
    task.ContinueWith((fun (t:Task<bool>) ->
        match (t.Status) with
        | TaskStatus.RanToCompletion ->
            sbj.OnNext(t.Result)
            sbj.OnCompleted()
        | TaskStatus.Faulted ->
            sbj.OnError(t.Exception.InnerException)
        | TaskStatus.Canceled ->
            sbj.OnError(new TaskCanceledException(t))
        | _ -> ()
    ) ,TaskContinuationOptions.ExecuteSynchronously)
    |> ignore

    tcs.SetResult(true)
    sbj.Subscribe(ConsoleObserver "sbj")
    |> ignore

let test5 () =
    let src = Observable.Interval(TimeSpan.FromSeconds(1)).Map(int)

    let connection =
        new BehaviorSubject<_>(0)

    src.Subscribe(connection)
    |> ignore

    connection.Subscribe(ConsoleObserver "first")
    |> ignore

    Thread.Sleep(2000)

    //After connection
    connection.Subscribe(ConsoleObserver "second")
    |> ignore
    Console.WriteLine($"Connection is {connection.Value}")

let test6 () =
    let heartRate:IObservable<int> = Observable.Interval(TimeSpan.FromSeconds(1)).Map(int)

    let sbj = new ReplaySubject<int>(bufferSize= 5, window= TimeSpan.FromMinutes(1))

    heartRate.Subscribe(sbj)
    |> ignore

    // After the user selected to show the heart rate on the screen

    sbj.Subscribe(ConsoleObserver "HeartRate Graph")
    |> ignore
