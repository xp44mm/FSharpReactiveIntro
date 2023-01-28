namespace ReactiveWPFDrawingApp

open FsXaml

type CancellationTokenSourceWindowXaml = XAML<"CancellationTokenSourceWindow.xaml">

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
open System.Windows
type CancellationTokenSourceWindow() as this =
    inherit CancellationTokenSourceWindowXaml()
    //[<DefaultValue>]
    //val mutable _cts: CancellationTokenSource
    //let startTask () =
    //    task {
    //        try
    //            try
    //                this.StartButton.IsEnabled <- false
    //                this.CancelButton.IsEnabled <- true
    //                this._cts <- new CancellationTokenSource()
    //                let token = this._cts.Token
    //                do! Task.Delay(TimeSpan.FromSeconds(5), token)
    //                Trace.WriteLine("Delay completed successfully.")
    //            with
    //            | :? OperationCanceledException ->
    //                Trace.WriteLine("Delay was canceled.")
    //            | ex ->
    //                Trace.WriteLine("Delay completed with error.")
    //                raise(ex)
    //        finally
    //            this.StartButton.IsEnabled <- true
    //            this.CancelButton.IsEnabled <- false
    //    } :> Task

    //let subscr1 =
    //    (this.StartButton.Click :> IObservable<_>)
    //        .FlatMap(fun _ -> startTask().ToObservable())
    //        .Subscribe()
    //let subscr2 =
    //    (this.CancelButton.Click :> IObservable<_>)
    //        .Subscribe(fun _ -> 
    //            this._cts.Cancel()
    //            this.CancelButton.IsEnabled <- false
    //        )
    [<DefaultValue>]
    val mutable _mouseMovesSubscription:IDisposable

    do
        this.StartButton.Click.Add(fun _ ->
            let mouseMoves =
                (this.MouseMove:>IObservable<_>)
                    .Map(fun args -> args.GetPosition(this))
            this._mouseMovesSubscription <-
                mouseMoves.Subscribe(fun value ->
                this.MousePositionLabel.Content <- $"({value.X}, {value.Y})"
                )
        )
        this.CancelButton.Click.Add(fun _ ->
            if (this._mouseMovesSubscription <> null) then
                this._mouseMovesSubscription.Dispose()
        )
