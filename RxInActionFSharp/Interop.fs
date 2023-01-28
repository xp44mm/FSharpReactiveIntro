module RxInActionFSharp.Interop

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

open System.Net
let DownloadStringTaskAsync (client:WebClient) (address:Uri) =
    let tcs = TaskCompletionSource<string>()
    let rec handler s e =
        client.DownloadStringCompleted.RemoveHandler(handler) // rec:注销自己
        if e.Cancelled then
            tcs.TrySetCanceled() |> ignore
        else if e.Error <> null then
            tcs.TrySetException(e.Error) |> ignore
        else
            tcs.TrySetResult(e.Result) |> ignore
    // Register for the event and *then* start the operation
    client.DownloadStringCompleted.AddHandler(handler)
    client.DownloadStringAsync(address)
    tcs.Task

let test() =
    task {
        use client = new WebClient()
        let! s = DownloadStringTaskAsync 
                    client
                    (Uri("https://news.cnblogs.com/"))
        Trace.WriteLine(s)
    }

type IMyAsyncHttpService =
  abstract DownloadString: Uri * (string * exn -> unit) -> unit

let DownloadStringAsync(httpService:IMyAsyncHttpService) (address:Uri) =
    let tcs = TaskCompletionSource<string>()
    httpService.DownloadString(address, fun (result, ex:exn) ->
        if (ex <> null) then
            tcs.TrySetException(ex)
        else
            tcs.TrySetResult(result)
        |> ignore
    )
    tcs.Task

let LastAsync()=
    task {
        let observable = Observable.Range(1,3)
        let! lastElement = observable.LastAsync().ToTask()
        Trace.WriteLine(lastElement)
        let! lastElement = observable.ToTask()
        Trace.WriteLine(lastElement)
    }

let FirstAsync()=
    task {
        let observable = Observable.Range(1,3)
        let! nextElement = observable.FirstAsync().ToTask()
        Trace.WriteLine(nextElement)
        let! allElements = observable.ToList().ToTask()
        Trace.WriteLine(stringify(List.ofSeq allElements))
    }

open System.Net.Http
let GetPage(client:HttpClient) =
    let task = client.GetAsync("https://news.cnblogs.com/")
    task.ToObservable()

let GetPage2(client:HttpClient) =
    Observable.StartAsync(
        fun (token:CancellationToken) -> 
        client.GetAsync("http://www.example.com/", token))
let GetPage3 (client:HttpClient) =
    Observable.FromAsync(
        fun (token:CancellationToken) -> 
        client.GetAsync("http://www.example.com/", token))

let GetPages (client:HttpClient) (urls:IObservable<string>)=
  urls.FlatMap(
      fun url (token:CancellationToken) -> 
      client.GetAsync(url, token)
      )
