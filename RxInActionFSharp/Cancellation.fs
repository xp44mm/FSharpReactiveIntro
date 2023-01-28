module RxInActionFSharp.Cancellation

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

type T()=
    static member CancelableMethodWithOverload (cancellationToken:CancellationToken) = ()
    static member CancelableMethodWithOverload() =
        T.CancelableMethodWithOverload(CancellationToken.None)


let IssueCancelRequest() =
    use cts = new CancellationTokenSource()
    //let task = CancelableMethodAsync(cts.Token)
    // At this point, the operation has been started.
    // Issue the cancellation request.
    cts.Cancel()
//let IssueCancelRequestAsync() =
//    task {
//        use cts = new CancellationTokenSource()
//        let task = CancelableMethodAsync(cts.Token)
//        // At this point, the operation is happily running.
//        // Issue the cancellation request.
//        cts.Cancel()
//        // (Asynchronously) wait for the operation to finish.
//        try
//            do! task;
//            // If we get here, the operation completed successfully
//            //  before the cancellation took effect.
//        with 
//        | (:? OperationCanceledException) ->
//            // If we get here, the operation was canceled before it completed.
//            ()
//        | _ ->
//            // If we get here, the operation completed with an error
//            //  before the cancellation took effect.
//            ()
//    }
let CancelableMethod (token:CancellationToken) =
    for i in [ 0 .. 100] do
        Thread.Sleep(1000)
        token.ThrowIfCancellationRequested()
    42

let CancelableMethod2 (token:CancellationToken) =
    for i in [ 0 .. 100000] do
        Thread.Sleep(1) // Some calculation goes here.
        if i % 1000 = 0 then
            token.ThrowIfCancellationRequested()
    42

let totask () =
    use cts = new CancellationTokenSource()
    task {
        let observable = Observable.Range(1,3)
        let! lastElement = observable.LastAsync().ToTask(cts.Token)
        Trace.WriteLine(lastElement)
        let! lastElement = observable.ToTask(cts.Token)
        Trace.WriteLine(lastElement)
        let! nextElement = observable.FirstAsync().ToTask(cts.Token)
        Trace.WriteLine(nextElement)
        let! allElements = observable.ToList().ToTask(cts.Token)
        Trace.WriteLine(stringify(List.ofSeq allElements))
    }

let cts () =
    use cancellation = new CancellationDisposable()
    let token = cancellation.Token
    // Pass the token to methods that respond to it.
    ()

let IssueTimeoutAsync () =
  use cts = new CancellationTokenSource(TimeSpan.FromSeconds(5))
  let token = cts.Token
  task {
      do! Task.Delay(TimeSpan.FromSeconds(10), token)
  }

let IssueTimeoutAsync2 () =
    use cts = new CancellationTokenSource()
    let token = cts.Token
    cts.CancelAfter(TimeSpan.FromSeconds(5))
    task {
        do! Task.Delay(TimeSpan.FromSeconds(10), token)
    }

open System.Net.Http
let GetWithTimeoutAsync(
    client:HttpClient,
    url:string,
    tok:CancellationToken) =
    use cts:CancellationTokenSource =
        CancellationTokenSource.CreateLinkedTokenSource(tok)
    cts.CancelAfter(TimeSpan.FromSeconds(2))
    let combinedToken = cts.Token
    client.GetAsync(url, combinedToken)

open System.Net.NetworkInformation
let PingAsync(hostNameOrAddress:string, tok:CancellationToken) =
    use ping = new Ping()
    let tsk:Task<PingReply> = ping.SendPingAsync(hostNameOrAddress)
    use _:CancellationTokenRegistration =
        tok.Register(fun () -> ping.SendAsyncCancel())
    task {
        return! tsk
    }
