module RxInActionFSharp.Schedulers

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
    let scheduler = NewThreadScheduler.Default
    let scheduling =
        scheduler.Schedule(
            Unit.Default,
            TimeSpan.FromSeconds(2),
            fun (scdlr:IScheduler) _ ->
                Console.WriteLine($"Hello World, Now: {scdlr.Now}")
                Disposable.Empty
            )
    ()

let test2 () =
    let scheduler = NewThreadScheduler.Default
    let rec action (scdlr:IScheduler) callNumber =
        $"Hello {callNumber}, Now: {scdlr.Now}, Thread: {Thread.CurrentThread.ManagedThreadId}"
        |> Console.WriteLine
        scdlr.Schedule(callNumber + 1, TimeSpan.FromSeconds(2), action)

    let scheduling =
        scheduler.Schedule(
            0,
            TimeSpan.FromSeconds(2),
            action)

    scheduling.Dispose()

let test3 () =
    $"Before - Thread: {Thread.CurrentThread.ManagedThreadId}"
    |> Console.WriteLine
    Observable.Interval(TimeSpan.FromSeconds(1), CurrentThreadScheduler.Instance)
        .Timestamp()
        .Take(3)
        .Do(fun x -> 
            $"Inside - {x} - Thread: {Thread.CurrentThread.ManagedThreadId}"
            |> Console.WriteLine
            )
        .Subscribe()

let test4 () =
    let subscription =
        Observable.Range(1, 5, NewThreadScheduler.Default)
            //without passing the scheduler, this will run infinitely
            .Repeat()
            .Subscribe(ConsoleObserver "Range on another thread");

    subscription.Dispose()

let TestScheduler(scheduler:IScheduler) = 
    let countdownEvent = new CountdownEvent(2)
    Thread.CurrentThread.ManagedThreadId
    |> sprintf "Calling Thread: %d"
    |> Console.WriteLine

    scheduler.Schedule(
        Unit.Default,
        fun s _ ->
            Thread.CurrentThread.ManagedThreadId
            |> sprintf "Action1 - Thread: %d"
            |> Console.WriteLine
            countdownEvent.Signal() |> ignore
        )
        |> ignore
    scheduler.Schedule(
        Unit.Default,
        fun s _ ->
            Thread.CurrentThread.ManagedThreadId
            |> sprintf "Action2 - Thread: %d"
            |> Console.WriteLine
            countdownEvent.Signal() |> ignore
        )
        |> ignore

    countdownEvent.Wait()

let test5 () =
    TestScheduler(NewThreadScheduler.Default)

let test6 () =
    let immediateScheduler = ImmediateScheduler.Instance
    let countdownEvent = new CountdownEvent(2)
        
    Console.WriteLine("Calling thread: {0} Current time: {1}", Thread.CurrentThread.ManagedThreadId, immediateScheduler.Now);

    immediateScheduler.Schedule(Unit.Default,
        TimeSpan.FromSeconds(2),
        fun (s:IScheduler) _ ->
            Console.WriteLine("Outer Action - Thread:{0}", Thread.CurrentThread.ManagedThreadId)
            s.Schedule(Unit.Default,
                fun (s2:IScheduler) _ ->
                    Console.WriteLine("Inner Action - Thread:{0}", Thread.CurrentThread.ManagedThreadId)
                    countdownEvent.Signal() |> ignore
                    Console.WriteLine("Inner Action - Done:{0}", Thread.CurrentThread.ManagedThreadId)
                    Disposable.Empty
                )
                |> ignore
            countdownEvent.Signal() |> ignore
            Console.WriteLine("Outer Action - Done")
            Disposable.Empty
        )
        |> ignore
    Console.WriteLine("After the Schedule, Time: {0}", immediateScheduler.Now)
    countdownEvent.Wait()

//let GeneratePrimes (amount:int) (scheduler:IScheduler) =
//    Observable.Create<int>(fun (o:IObserver<int>) ->
//        let cancellation = new CancellationDisposable()
//        let scheduledWork = scheduler.Schedule(fun () ->
//            try
//                let magicalPrimeGenerator = new MagicalPrimeGenerator()
//                for prime in magicalPrimeGenerator.Generate(amount) do
//                    cancellation.Token.ThrowIfCancellationRequested()
//                    o.OnNext(prime)
//                o.OnCompleted()
//            with ex -> o.OnError(ex)
//            ()
//        )
//        new CompositeDisposable(scheduledWork, cancellation)
//        :> IDisposable
//    )
