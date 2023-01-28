module RxInActionFSharp.Buffering

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
    let speedReadings = 
        [ 50.0; 51.0; 51.5; 53.0; 52.0; 52.5; 53.0 ]
            .ToObservable()
    let timeDelta = 1.0/3600.0
    let accelrations =
        speedReadings
            .Buffer(count= 2, skip= 1)
            .Filter(fun buffer -> buffer.Count = 2)
            .Map(fun buffer ->
                let speedDelta = buffer.[1] - buffer.[0]
                speedDelta / timeDelta
            )
    accelrations.Subscribe(ConsoleObserver "Acceleration")

let test2 () =
    let coldMessages = 
        Observable.Interval(TimeSpan.FromMilliseconds(50))
            .Take(4)
            .Map(sprintf "Message %d")

    let messages =
        coldMessages.Concat(
                coldMessages.DelaySubscription(TimeSpan.FromMilliseconds(200)))
            .Publish()
            .RefCount()

    messages.Buffer(messages.Throttle(TimeSpan.FromMilliseconds(100)))
        .FlatMap(fun b i -> b.ToObservable().Map(fun m -> $"Buffer {i} - {m}"))
        .Subscribe(ConsoleObserver "Hi-Rate Messages")

let test3 () =
    let donationsWindow1 = [50M; 55M; 60M].ToObservable()
    let donationsWindow2 = [49M; 48M; 45M].ToObservable()

    let donations =
        donationsWindow1.Concat(donationsWindow2.DelaySubscription(TimeSpan.FromSeconds(1.5)))

    let windows = donations.Window(TimeSpan.FromSeconds(1))

    let donationsSums =
        windows.Do(fun _ -> Console.WriteLine("New Window"))
            .FlatMap(fun window ->
                window.Scan(fun prevSum donation -> prevSum + donation)
            )
            //.Map(fun sum -> sum)

    donationsSums.Subscribe(ConsoleObserver "donations in shift")


