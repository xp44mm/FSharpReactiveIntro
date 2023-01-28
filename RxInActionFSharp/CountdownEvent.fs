module RxInActionFSharp.CountdownEvent

open System
open System.IO
open System.Text
open System.Collections.Generic
open System.Threading
open System.Threading.Tasks

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

let test () =
    let source = seq []
    use e = new CountdownEvent(1)
    // fork work:
    for element in source do
        // Dynamically increment signal count.
        e.AddCount()
        ThreadPool.QueueUserWorkItem((fun (state) ->
                try
                    Console.WriteLine(state)
                finally
                    e.Signal() |> ignore
            ),
            element) |> ignore
    e.Signal() |> ignore

    // The first element could be run on this thread.

    // Join with work.
    e.Wait()

type Data = { Num:int }

type DataWithToken = { Token: CancellationToken; Data: Data }

let GetData() =
    [1..5]
    |> Seq.map(fun i -> {Num=i})

let ProcessData (obj:obj) =
    let dataWithToken = unbox<DataWithToken> obj

    if dataWithToken.Token.IsCancellationRequested then
        Console.WriteLine("Canceled before starting {0}", dataWithToken.Data.Num)
    else 
        let rec loop i =
            if i = 0 then
                Console.WriteLine("Processed {0}", dataWithToken.Data.Num)
            else
                if dataWithToken.Token.IsCancellationRequested then
                    Console.WriteLine("Cancelling while executing {0}", dataWithToken.Data.Num)
                else
                    Thread.SpinWait(100000)
                    loop (i-1)
        loop 10000

let EventWithCancel() =
    let source = GetData()
    use cts = new CancellationTokenSource()

    //Enable cancellation request from a simple UI thread.
    Task.Factory.StartNew(fun () ->
                if (Console.ReadKey().KeyChar = 'c') then
                    cts.Cancel()
            ) |> ignore

    // Event must have a count of at least 1
    use e = new CountdownEvent(1)

    // fork work:
    for element in source do
        let item = {Data = element; Token= cts.Token}
        // Dynamically increment signal count.
        e.AddCount();
        ThreadPool.QueueUserWorkItem((fun state ->
                ProcessData(state)
                if not cts.Token.IsCancellationRequested then
                    e.Signal() |> ignore
            ),
            item) |> ignore
    // Decrement the signal count by the one we added
    // in the constructor.
    e.Signal() |> ignore

    // The first element could be run on this thread.

    // Join with work or catch cancellation.
    try
        e.Wait(cts.Token)
    with 
    | :? OperationCanceledException as oce when oce.CancellationToken = cts.Token ->
            Console.WriteLine("User canceled.");
    | ex -> 
        Console.Write("We don't know who canceled us!")
        raise ex

let Main() =
    EventWithCancel()
    Console.WriteLine("Press enter to exit.")
    Console.ReadLine()

