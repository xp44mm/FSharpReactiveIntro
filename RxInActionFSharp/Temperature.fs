module RxInActionFSharp.Temperature
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

let getColdObservable() =
        Observable.Create<string>(fun (o:IObserver<_>) ->
            task {
                o.OnNext("Hello")
                do! Task.Delay(TimeSpan.FromSeconds(1))
                o.OnNext("Rx")
            }
            :>Task
        )

let test () =
    let coldObservable = getColdObservable()

    task {
        coldObservable.Subscribe(ConsoleObserver "o1")
        |> ignore
        do! Task.Delay(TimeSpan.FromSeconds(0.5))
        coldObservable.Subscribe(ConsoleObserver "o2")
        |> ignore
    }
let test1()=
    let coldObservable = Observable.Interval(TimeSpan.FromSeconds(1)).Take(5)
    let connectableObservable = coldObservable.Publish()
    connectableObservable.Subscribe(ConsoleObserver "First")
        |> ignore
    connectableObservable.Subscribe(ConsoleObserver "Second")
        |> ignore
    connectableObservable.Connect()
        |> ignore
    Thread.Sleep(2000)
    connectableObservable.Subscribe(ConsoleObserver "Third")
        |> ignore

let test2()=
    let coldObservable = getColdObservable()
    let connectableObservable = coldObservable.Publish()

    connectableObservable.Subscribe(ConsoleObserver "A")
        |> ignore
    connectableObservable.Subscribe(ConsoleObserver "B")
        |> ignore
    connectableObservable.Connect()

let test3()=
    let mutable i = 0
    let numbers = Observable.Range(1, 5).Map(fun _ -> i <- i+1;i)
    let zipped =
        numbers
            .Zip(numbers)
            .Map(fun struct(a,b) -> a + b)
            .Subscribe(ConsoleObserver "zipped")
    ()
let test4()=
    let mutable i = 0
    let numbers = Observable.Range(1, 5).Map(fun _ -> i <- i+1;i)

    let publishedZip =
        numbers
            .Publish(fun published ->
                published
                    .Zip(published)
                    .Map(fun struct(a, b) -> a + b))
    publishedZip.Subscribe(ConsoleObserver "publishedZipped")

let test5()=
    let coldObservable = getColdObservable()
                                   .Map(fun _ -> "Rx")
    let connectableObservable = coldObservable.PublishLast()
    connectableObservable.Subscribe(ConsoleObserver "A")
        |> ignore
    connectableObservable.Subscribe(ConsoleObserver "B")
        |> ignore
    connectableObservable.Connect()
        |> ignore

    Thread.Sleep(6000)
    connectableObservable.Subscribe(ConsoleObserver "C")

let test6()=
    let coldObservable = getColdObservable()

    let connectableObservable =
        Observable.Defer(fun () -> coldObservable)
            .Publish()

    connectableObservable.Subscribe(ConsoleObserver "Messages Screen")
        |> ignore
    connectableObservable.Subscribe(ConsoleObserver "Messages Statistics")
        |> ignore
    let subscription = connectableObservable.Connect()
    //After the application was notified on server outage
    Console.WriteLine("--Disposing the current connection and reconnecting--");
    subscription.Dispose()
    let subscription1 = connectableObservable.Connect()
    ()

let test7()=
    let ObserveMessages = getColdObservable()

    let connectableObservable =
        Observable.Defer(fun () -> ObserveMessages)
            .Publish()

    connectableObservable.Subscribe(ConsoleObserver "Messages Screen")
        |> ignore
    connectableObservable.Subscribe(ConsoleObserver "Messages Statistics")
        |> ignore
    let subscription = connectableObservable.Connect()
    //After the application was notified on server outage
    Console.WriteLine("--Disposing the current connection and reconnecting--")
    subscription.Dispose()
    let _ = connectableObservable.Connect()
    ()

let test8 () =
    let publishedObservable = 
        Observable.Interval(TimeSpan.FromSeconds(1))
            .Do(fun x -> Console.WriteLine($"Generating {x}"))
            .Publish()
            .RefCount()
    let subscription1 = publishedObservable.Subscribe(ConsoleObserver "First")
    let subscription2 = publishedObservable.Subscribe(ConsoleObserver "Second")

    Thread.Sleep(3000)
    subscription1.Dispose()
    Thread.Sleep(3000)
    subscription2.Dispose()

let test9 () =
    let publishedObservable = 
        Observable.Interval(TimeSpan.FromSeconds(1))
            .Take(5)
            .Replay(2) //Creates a connectable observable that replays the last two items
    publishedObservable.Connect()
        |> ignore
    let subscription1 = publishedObservable.Subscribe(ConsoleObserver "First")
    Thread.Sleep(3000)
    let subscription2 = publishedObservable.Subscribe(ConsoleObserver "Second")
    ()
