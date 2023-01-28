namespace ReactiveWPFDrawingApp

type SendingNotificationsToContextXaml = FsXaml.XAML<"SendingNotificationsToContext.xaml">

open System
open System.IO
open System.Text
open System.Collections.Generic
open System.Threading
open System.Threading.Tasks

//open FSharp.Idioms.PointFree
//open FSharp.Literals.Literal
//open FSharp.Control.Reactive

open System.Reactive
open System.Reactive.Disposables
open System.Reactive.Observable.Aliases
open System.Reactive.Threading.Tasks
open System.Reactive.Linq
open System.Reactive.Subjects
open System.Diagnostics
open System.Reactive.Concurrency
open System.Windows.Input

type SendingNotificationsToContext() as this =
    inherit SendingNotificationsToContextXaml()

    let disps =
        Trace.WriteLine($"UI thread is {Environment.CurrentManagedThreadId}")
        Observable.Interval(TimeSpan.FromSeconds(1))
            .ObserveOn(SynchronizationContext.Current)
            .Subscribe(fun x -> Trace.WriteLine(
                $"Interval {x} on thread{Environment.CurrentManagedThreadId}"))

    let dsps2 =
        Trace.WriteLine($"UI thread is {Environment.CurrentManagedThreadId}")
        (this.MouseMove :> IObservable<_>)
            .Map(fun args -> args.GetPosition(this))
            .ObserveOn(System.Reactive.Concurrency.Scheduler.Default)
            .Map(fun position ->
                Thread.Sleep(100)
                let result = position.X + position.Y
                let thread = Environment.CurrentManagedThreadId
                Trace.WriteLine($"Calculated result {result} on thread {thread}")
                result
            )
            .ObserveOn(SynchronizationContext.Current)
            .Subscribe(fun x -> Trace.WriteLine(
                $"Result {x} on thread {Environment.CurrentManagedThreadId}"))

    let subscr =
        (this.MouseMove :> IObservable<MouseEventArgs>)
            .Buffer(TimeSpan.FromSeconds(1))
            .Subscribe(fun g -> Trace.WriteLine(
                $"{DateTime.Now.Second}: Saw {g.Count} items."))

    let Throttling () =
        (this.MouseMove :> IObservable<MouseEventArgs>)
            .Map(fun args -> args.GetPosition(this))
            .Throttle(TimeSpan.FromSeconds(1))
            .Subscribe(fun x -> Trace.WriteLine(
                $"{DateTime.Now.Second}: Saw {x.X + x.Y}"))
    let Sampling () =
        (this.MouseMove :> IObservable<MouseEventArgs>)
            .Map(fun args -> args.GetPosition(this))
            .Sample(TimeSpan.FromSeconds(1))
            .Subscribe(fun x -> Trace.WriteLine(
                $"{DateTime.Now.Second}: Saw {x.X + x.Y}"))
    let Timeouting () =
        (this.MouseMove :> IObservable<MouseEventArgs>)
            .Map(fun args -> args.GetPosition(this))
            .Timeout(TimeSpan.FromSeconds(1))
            .Subscribe(
                (fun x -> Trace.WriteLine($"{DateTime.Now.Second}: Saw {x.X + x.Y}")),
                (fun (ex:exn) -> Trace.WriteLine(ex))
                )

    let clicks =
        (this.MouseDown :> IObservable<MouseButtonEventArgs>)
            .Map(fun args -> args.GetPosition(this))
    let subscr =
        (this.MouseMove :> IObservable<MouseEventArgs>)
            .Map(fun args -> args.GetPosition(this))
            .Timeout(TimeSpan.FromSeconds(1), clicks)
            .Subscribe(
                (fun x -> Trace.WriteLine($"{DateTime.Now.Second}: Saw {x.X + x.Y}")),
                (fun (ex:exn) -> Trace.WriteLine(ex))
                )
