module RxInActionFSharp.DelaySubscription

open System
open System.IO

open System.Reactive
open System.Collections.Generic

open System.Reactive.Disposables
open System.Reactive.Observable.Aliases

open FSharp.Control.Reactive
open System.Reactive.Linq
open System.Text
open FSharp.Idioms.PointFree
open FSharp.Literals.Literal
open System.Threading
open System.Threading.Tasks
open System.Reactive.Threading.Tasks

let test () =

    Console.WriteLine("Creating subscription at {0}", DateTime.Now)
    Observable
        .Range(1, 5)
        .Timestamp()
        .DelaySubscription(TimeSpan.FromSeconds(5))
        .Subscribe(ConsoleObserver "DelaySubscription")
    |> ignore

let test2 () =
    Console.WriteLine("Creating the observable pipeline at {0}", DateTime.Now)
    let observable =
        Observable
            .Range(1, 5)
            .Timestamp()
            .DelaySubscription(TimeSpan.FromSeconds(5))
    Thread.Sleep(TimeSpan.FromSeconds(2))
    Console.WriteLine("Creating subscription at {0}", DateTime.Now)
    observable
        .Subscribe(ConsoleObserver "DelaySubscription")
    |> ignore

let test3 () =
    Observable
        .Timer(DateTimeOffset.Now,TimeSpan.FromSeconds(1))
        .Map(fun t -> DateTimeOffset.Now)
        .TakeUntil(DateTimeOffset.Now.AddSeconds(5))
        .Subscribe(ConsoleObserver "TakeUntil(time)")
    |> ignore

let test4 () =
    Observable
        .Timer(DateTimeOffset.Now,TimeSpan.FromSeconds(1))
        .Map(fun t -> DateTimeOffset.Now)
        .TakeUntil(
               Observable.Timer(TimeSpan.FromSeconds(5)))
        .Subscribe(ConsoleObserver "TakeUntil(observable)")
    |> ignore


let test5() =
    let messages = 
        Observable
            .Timer(DateTimeOffset.Now,TimeSpan.FromSeconds(1))
            .Map(fun t -> t.ToString())

    let controlChannel =
        Observable
            .Timer(DateTimeOffset.Now,TimeSpan.FromSeconds(1))
            .Map(fun t -> t.ToString())

    messages
        .TakeUntil(controlChannel.Filter((=) "3"))
        .Subscribe(ConsoleObserver "TakeUntil(EXTRIGGERS)")
    |> ignore

let test6 () =
    let messages = 
        Observable
            .Timer(DateTimeOffset.Now,TimeSpan.FromSeconds(1))
            .Map(fun t -> t.ToString())

    let controlChannel =
        Observable
            .Timer(DateTimeOffset.Now,TimeSpan.FromSeconds(1))
            .Map(fun t -> t.ToString())

    messages
        .SkipUntil(controlChannel.Filter((=) "2"))
        .Subscribe(ConsoleObserver "TakeUntil(EXTRIGGERS)")
    |> ignore

let test7 () =
    Observable
        .Range(1, 5)
        .Skip(2)
        .Subscribe(ConsoleObserver "Skip(2)")
    |> ignore

let test8 () =
    Observable
        .Range(1, 10)
        .SkipWhile(flip (<) 2)
        .TakeWhile(flip (<) 7)
        .Subscribe(ConsoleObserver "SkipWhile")
    |> ignore

let test9 () =
    Observable
        .Range(1, 3)
        .Repeat(2)
        .Subscribe(ConsoleObserver "Repeat(2)")
    |> ignore

let test10 () =
    Observable
        .Range(1, 5)
        .Do(fun x -> Console.WriteLine("{0} was emitted",x))
        .Filter(flip(%)2>>(=)0)
        .Do(fun x -> Console.WriteLine("{0} survived the Filter()", x))
        .Map((*)3)
        .Subscribe(ConsoleObserver "final")
    |> ignore

let Log (msg:string) (observable:IObservable<_>) =
    observable.Do(ConsoleObserver msg)

let test11 () =
    Observable
        .Range(1, 5)
        .Do(ConsoleObserver "range")
        .Filter(flip(%)2>>(=)0)
        .Do(ConsoleObserver "filter")
        .Map((*)3)
        .Subscribe(ConsoleObserver "final")
    |> ignore

