module RxInActionFSharp.Combination

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

let test () =
    let temp1 = Observable.Return(0.0)
    let temp2 = Observable.Return(0.0)

    temp1
        .Zip(temp2, fun t1 t2 -> (t1 + t2)/2.0)
        .Subscribe(ConsoleObserver "Avg Temp.")

let test1 () =
    let heartRate = new Subject<int>()
    let speed = new Subject<int>()

    speed.CombineLatest(heartRate,
                        fun s h -> $"Heart:{h} Speed:{s}")
        .Subscribe(ConsoleObserver "Metrics")

let test2 () =
    let heartRate = new Subject<int>()
    let speed = new Subject<int>()

    speed
        .StartWith(0)
        .CombineLatest(heartRate.StartWith(0),
                       fun s h -> $"Heart:{h} Speed:{s}")
        .Subscribe(ConsoleObserver "Metrics")

let test3 () =
    let facebookMessages: Task<string[]> = 
        Task.Delay(10)
            .ContinueWith(fun _ -> Array.ofList ["Facebook1"; "Facebook2"])
            
    let twitterStatuses: Task<string[]> =
        Task.FromResult(Array.ofList ["Twitter1"; "Twitter2"])

    Observable.Concat(facebookMessages.ToObservable(),
                      twitterStatuses.ToObservable())
        .FlatMap(fun messages->messages.ToObservable())
        .Subscribe(ConsoleObserver "Concat Messages")

let test4 () =
    let facebookMessages: Task<string[]> =
        Task.Delay(50)
            .ContinueWith(fun _ -> Array.ofList ["Facebook1"; "Facebook2"])
            
    let twitterStatuses: Task<string[]> =
        Task.FromResult(Array.ofList ["Twitter1"; "Twitter2"])

    Observable
        .Merge(
            facebookMessages.ToObservable(),
            twitterStatuses.ToObservable())
        .FlatMap(fun messages->messages.ToObservable())
        .Subscribe(ConsoleObserver "Merged Messages")

let test5 () =
    let texts: IObservable<string> = 
        ["Hello"; "World"].ToObservable()

    texts
        .Map(fun txt -> Observable.Return(txt + "-Result"))
        .Merge()
        .Subscribe(ConsoleObserver "Merging from observable")

let test6 () =
    let a: IObservable<string> = 
        Observable.Interval(TimeSpan.FromSeconds(1))
            .Map(sprintf "A%d")
            .Take(2)

    let b: IObservable<string> =
        Observable.Interval(TimeSpan.FromSeconds(1))
            .Map(sprintf "B%d")
            .Take(2)
    let c: IObservable<string> = 
        Observable.Interval(TimeSpan.FromSeconds(1))
            .Map(sprintf "C%d")
            .Take(2)

    [a;b;c]
        .ToObservable()
        .Merge(2)
        .Subscribe(ConsoleObserver "Merge with 2 concurrent subscriptions")
let test7 () =
    let textsSubject = new Subject<string>();
    let texts:IObservable<string> = textsSubject.AsObservable()

    texts
        .Map(fun txt -> 
            Observable.Return(txt + "-Result")
                .Delay(TimeSpan.FromMilliseconds(if txt = "R1" then 10 else 0)))
        .Switch()
        .Subscribe(ConsoleObserver "Merging from observable")
        |> ignore
    textsSubject.OnNext("R1")
    textsSubject.OnNext("R2")
    Thread.Sleep(20)
    textsSubject.OnNext("R3")

let test8 () =
    let server1 =
         Observable.Interval(TimeSpan.FromSeconds(2))
                 .Map(sprintf "Server1-%d")
    let server2 =
         Observable.Interval(TimeSpan.FromSeconds(1))
                 .Map(sprintf "Server2-%d")

    Observable.Amb(server1, server2)
        .Take(3)
        .Subscribe(ConsoleObserver "Amb")

let test9 () =
    let people = new Subject<{|Gender:bool;Age:int|}>()
    let genderAge =
        people
            .GroupBy(fun p -> p.Gender)
            .FlatMap(fun gender ->
                gender
                    .Average(fun p -> p.Age)
                    .Map(fun avg ->
                        {|Gender=gender.Key;AvgAge=avg|}
                    )
            )

    genderAge.Subscribe(ConsoleObserver "Gender Age")
    |> ignore

    people.OnNext({|Gender=true;Age=18|})
    people.OnNext({|Gender=true;Age=16|})
    people.OnNext({|Gender=false;Age=20|})
    people.OnCompleted()



