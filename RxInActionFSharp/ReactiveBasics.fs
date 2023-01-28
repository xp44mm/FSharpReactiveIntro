module RxInActionFSharp.ReactiveBasics

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

let ConvertingEvents () =
    let progress = new Progress<int>()
    let progressReports =
        progress.ProgressChanged :> IObservable<_>
    progressReports.Subscribe(fun data -> 
        Trace.WriteLine($"OnNext: {data}"))
        |> ignore
    (progress :> IProgress<_>).Report(10)

open System.Timers
let timer() =
    let tmr = new Timer(interval= 1000,Enabled = true)
    let ticks =
        tmr.Elapsed :> IObservable<_>
    ticks.Subscribe(fun data -> 
        Trace.WriteLine($"OnNext: {data.SignalTime}"))
        |> ignore

open System.Net
let web() =
    let client = new WebClient()
    let downloadedStrings =
        client.DownloadStringCompleted :> IObservable<_>

    downloadedStrings.Subscribe(        
        (fun eventArgs ->
            if (eventArgs.Error <> null) then
                Trace.WriteLine($"OnNext: (Error) {eventArgs.Error}")
            else
                Trace.WriteLine($"OnNext: {eventArgs.Result}")
        ),
        (fun (ex:exn) -> Trace.WriteLine($"OnError: {ex.Message}")),
        (fun () -> Trace.WriteLine("OnCompleted")))
        |> ignore
    client.DownloadStringAsync(Uri("https://news.cnblogs.com/"))

let button_click () =
  Trace.WriteLine($"UI thread is {Environment.CurrentManagedThreadId}")
  Observable.Interval(TimeSpan.FromSeconds(1))
      .Subscribe(fun i -> Trace.WriteLine(
          $"Interval {i} on thread {Environment.CurrentManagedThreadId}"))

let buffer()=
    use subscription = 
        Observable.Interval(TimeSpan.FromSeconds(1))
            .Buffer(2)
            .Subscribe(fun arr -> Trace.WriteLine(
                $"{DateTime.Now.Second}: Got {arr.[0]} and {arr.[1]}"))
    ()

let window() =
    Observable.Interval(TimeSpan.FromSeconds(1))
        .Window(2)
        .Subscribe(fun grp ->
            Trace.WriteLine($"{DateTime.Now.Second}: Starting new group")
            grp.Subscribe(
                (fun i -> Trace.WriteLine($"{DateTime.Now.Second}: Saw {i}")),
                (fun () -> Trace.WriteLine($"{DateTime.Now.Second}: Ending group"))
                ) |> ignore
        )

open System.Net.Http
let GetWithTimeout(client:HttpClient) =
    client.GetStringAsync("http://www.example.com/")
        .ToObservable()
        .Timeout(TimeSpan.FromSeconds(1))
        .Subscribe(
            (fun t -> Trace.WriteLine($"{DateTime.Now.Second}: Saw {t.Length}")),
            (fun (ex:exn) -> Trace.WriteLine(ex))
        )
