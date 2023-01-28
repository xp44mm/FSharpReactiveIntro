module RxInActionFSharp.ObserveOnAndSubscribeOn

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

let test1 () =
    let observable =
        Observable.Create(fun (o:IObserver<int>) ->
            Thread.Sleep(TimeSpan.FromSeconds(5)); //1
            o.OnNext(1)
            o.OnCompleted()
            Disposable.Empty
        )
    observable.Subscribe(ConsoleObserver "LongOperation")

let test2 () =
    let eventLoopScheduler = new EventLoopScheduler();
    let subscription =
        Observable.Interval(TimeSpan.FromSeconds(1))
            .Do(fun _ -> Console.WriteLine("Inside Do")) //1
            .SubscribeOn(eventLoopScheduler) //2
            .Subscribe()

    //3
    eventLoopScheduler.Schedule(1,
        fun s state ->
            Console.WriteLine("Before sleep")
            Thread.Sleep(TimeSpan.FromSeconds(3))
            Console.WriteLine("After sleep")
            Disposable.Empty
        ) |> ignore
    subscription.Dispose() //4
    Console.WriteLine("Subscription disposed")

type IObservable<'T> with
    member observable.LogWithThread<'T> (msg:string) =
        Observable.Defer(fun () ->
            Console.WriteLine("{0} Subscription happened on Thread: {1}", msg,
                            Thread.CurrentThread.ManagedThreadId)
            observable.Do(
                (fun x -> Console.WriteLine("{0} - OnNext({1}) Thread: {2}", msg, x,
                                        Thread.CurrentThread.ManagedThreadId)),
                (fun ex ->
                    Console.WriteLine("{0} – OnError Thread:{1}", msg,
                                        Thread.CurrentThread.ManagedThreadId);
                    Console.WriteLine("\t {0}", ex);
                ),
                (fun () -> Console.WriteLine("{0} - OnCompleted() Thread {1}", msg,
                                        Thread.CurrentThread.ManagedThreadId)))
        )

let test3 () =
    Observable.Range(0,6)
        .Take(3)                                .LogWithThread("A")
        .Where(fun x -> x % 2 = 0)              .LogWithThread("B")
        .SubscribeOn(NewThreadScheduler.Default).LogWithThread("C")
        .Select(fun x -> x * x)                 .LogWithThread("D")
        .ObserveOn(TaskPoolScheduler.Default)   .LogWithThread("E")
        .Subscribe(ConsoleObserver "squares by time")

