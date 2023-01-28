module RxInActionFSharp.AsyncBasics

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

let DelayResult (result) (delayTime:TimeSpan) =
    task {
      do! Task.Delay(delayTime)
      return result
    }

open System.Net.Http
let DownloadStringWithRetries(client:HttpClient) (uri:string) =
    // Retry after 1 second, then after 2 seconds, then 4.
    let mutable nextDelay = TimeSpan.FromSeconds(1)
    let rec loop i =
        task {
            if i = 1 then
                return! client.GetStringAsync(uri)
            else
                try
                    return! client.GetStringAsync(uri)
                with _ ->
                    nextDelay <- nextDelay * 2.0
                    do! Task.Delay(nextDelay)
                    return! loop (i-1)
        }
    loop 3

let DownloadStringWithTimeout(client:HttpClient) (uri:string) =
    task {
        use cts = new CancellationTokenSource(TimeSpan.FromSeconds(3))
        let downloadTask = client.GetStringAsync(uri)
        let timeoutTask = Task.Delay(Timeout.InfiniteTimeSpan, cts.Token)
        let! completedTask = Task.WhenAny(downloadTask, timeoutTask)
        if completedTask = timeoutTask
        then return ""
        else return! downloadTask
    }

type IMyAsyncInterface =
  abstract GetValueAsync: unit->Task<int>

type MySynchronousImplementation() =
    interface IMyAsyncInterface with
        member this.GetValueAsync() = 
            Task.FromResult(13)
let test1() =
    let x =  MySynchronousImplementation()
    task {
        let! t = (x:>IMyAsyncInterface).GetValueAsync()
        Console.WriteLine(t)
    }


module CompletedTask =
    type IMyAsyncInterface =
      abstract DoSomethingAsync: unit->Task

    type MySynchronousImplementation() =
        interface IMyAsyncInterface with
            member this.DoSomethingAsync() = 
                Task.CompletedTask
    let test() =
        let x =  MySynchronousImplementation()
        task {
            let! t = (x:>IMyAsyncInterface).DoSomethingAsync()
            Console.WriteLine("done!")
        }

let NotImplementedAsync<'T> () =
    Task.FromException<'T>(new NotImplementedException())

let GetValueAsync(cancellationToken:CancellationToken) =
    if cancellationToken.IsCancellationRequested then
        Task.FromCanceled<int>(cancellationToken)
    else
        Task.FromResult(13)

module FromException =
    type IMyAsyncInterface =
      abstract DoSomethingAsync: unit->Task

    type MySynchronousImplementation () =
        interface IMyAsyncInterface with
            member this.DoSomethingAsync()=
                try
                  //DoSomethingSynchronously()
                  Task.CompletedTask
                with (ex:exn) ->
                  Task.FromException(ex)

let zeroTask:Task<int> = Task.FromResult(0)
//let GetValueAsync() = zeroTask

let MyMethodAsync (progress:IProgress<float>) =
    task {
        let mutable dn = false
        let mutable percentComplete = 0.0
        while not dn do
            progress.Report(percentComplete)
            percentComplete<-percentComplete+10.0
            if percentComplete > 100.0 then
                dn <- true
    }
let CallMyMethodAsync()=
    let progress = new Progress<float>()
    progress.ProgressChanged.Add(fun e -> 
        Console.WriteLine($"xxx{e}"))

    MyMethodAsync(progress)

let testWhenAll () =
    let task1 = Task.Delay(TimeSpan.FromSeconds(1))
    let task2 = Task.Delay(TimeSpan.FromSeconds(2))
    let task3 = Task.Delay(TimeSpan.FromSeconds(1))
    Task.WhenAll(task1, task2, task3)

let testWhenAllResult () =
    let task1 = Task.FromResult(3)
    let task2 = Task.FromResult(5)
    let task3 = Task.FromResult(7)
    task {
        let! res = Task.WhenAll(task1, task2, task3)
        Console.WriteLine($"{stringify res}")
        // [|3;5;7|]
    }

let DownloadAllAsync (client:HttpClient) (urls:seq<string>) =
    task {
        let downloadTasks = 
            urls 
            |> Seq.map(fun url -> client.GetStringAsync(url))
            |> Seq.toArray
        let! htmlPages = Task.WhenAll(downloadTasks)
        return htmlPages |> String.concat "\r\n"
    }

let ThrowNotImplementedExceptionAsync() =
  Task.FromException(NotImplementedException())

let ThrowInvalidOperationExceptionAsync() =
  Task.FromException(InvalidOperationException())

let ObserveOneExceptionAsync() =
    task {
        let task1 = ThrowNotImplementedExceptionAsync()
        let task2 = ThrowInvalidOperationExceptionAsync()
        try
            return! Task.WhenAll(task1, task2)
        with (ex) ->
            // "ex" is either NotImplementedException or InvalidOperationException.
            raise ex
        }
let ObserveAllExceptionsAsync() =
    task {
        let task1 = ThrowNotImplementedExceptionAsync()
        let task2 = ThrowInvalidOperationExceptionAsync()
        let allTasks = Task.WhenAll(task1, task2)
        try
            do! allTasks
        with (ex) ->
            let allExceptions = allTasks.Exception
            // ...
            do! allTasks
    }

// Returns the length of data at the first URL to respond.
let FirstRespondingUrlAsync (client:HttpClient) (urlA:string) (urlB:string) =
    task {
        let downloadTaskA = client.GetByteArrayAsync(urlA)
        let downloadTaskB = client.GetByteArrayAsync(urlB)
        let! completedTask =
            Task.WhenAny(downloadTaskA, downloadTaskB)
        let! data = completedTask
        return data.Length
    }


let DelayAndReturnAsync(value:int)=
    task {
        do! Task.Delay(TimeSpan.FromSeconds(value))
        return value
    }

//// Currently, this method prints "2", "3", and "1".
//// The desired behavior is for this method to print "1", "2", and "3".
//let ProcessTasksAsync() =
//    task {
//        // Create a sequence of tasks.
//        let taskA = DelayAndReturnAsync(2)
//        let taskB = DelayAndReturnAsync(3)
//        let taskC = DelayAndReturnAsync(1)
//        let tasks = [ taskA; taskB; taskC ]
//        // Await each task in order.
//        for task in tasks do
//            let! result = task
//            Trace.WriteLine(result)
//    }

//async Task<int> DelayAndReturnAsync(int value)
//{
//  await Task.Delay(TimeSpan.FromSeconds(value));
//  return value;
//}
let AwaitAndProcessAsync(tsk:Task<int>)=
    task {
      let! result = tsk
      Trace.WriteLine(result)
    }
//// This method now prints "1", "2", and "3".
//let ProcessTasksAsync()=
//    task {
//        // Create a sequence of tasks.
//        let taskA = DelayAndReturnAsync(2);
//        let taskB = DelayAndReturnAsync(3);
//        let taskC = DelayAndReturnAsync(1);
//        let tasks = [ taskA; taskB; taskC ]
//        let processingTasks =
//            tasks
//            |> Seq.map(fun t -> AwaitAndProcessAsync(t))
//            |> Seq.toArray
//        // Await all processing to complete
//        let! _ = Task.WhenAll(processingTasks)
//        ()
//    }

open System.Linq
// This method now prints "1", "2", and "3".
let ProcessTasksAsync() =
    task {
        let taskA = DelayAndReturnAsync(2);
        let taskB = DelayAndReturnAsync(3);
        let taskC = DelayAndReturnAsync(1);
        let tasks = [ taskA; taskB; taskC ]
        let processingTasks = 
            tasks.Select(fun t ->
                task {
                    let! result = t
                    Trace.WriteLine(result)
                }).ToArray()
        // Await all processing to complete
        let! _ = Task.WhenAll(processingTasks)
        ()
    }

let ResumeOnContextAsync() =
    task {
      do! Task.Delay(TimeSpan.FromSeconds(1))
      // This method resumes within the same context.
    }
let ResumeWithoutContextAsync() =
    task {
      do! Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false)
      // This method discards its context when it resumes.
    }

let ThrowExceptionAsync() =
    task {
      do! Task.Delay(TimeSpan.FromSeconds(1))
      raise(InvalidOperationException("Test"))
    }:>Task
    
let TestAsync()=
    // The exception is thrown by the method and placed on the `tsk`.
    let tsk = ThrowExceptionAsync()
    task {
        try
            // The exception is re-raised here, where the `tsk` is awaited.
            do! tsk
        with :? InvalidOperationException as e ->
            // The exception is correctly caught here.
            ()
    }

//let test() =
//    try
//      AsyncContext.Run(fun () -> MainAsync(args))
//    with (ex:exn) ->
//      Console.Error.WriteLine(ex)
