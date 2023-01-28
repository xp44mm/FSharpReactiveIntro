namespace ReactiveWPFDrawingApp

open FsXaml

type RxSearchXaml = XAML<"RxSearch.xaml">

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

type RxSearch() as this =
    inherit RxSearchXaml()
    let disp =
        (this.TextBox.TextChanged :> IObservable<_>)
            .Map(fun x -> this.TextBox.Text)
            .Throttle(TimeSpan.FromMilliseconds(400))
            .DistinctUntilChanged()
            .ObserveOn(System.Reactive.Concurrency.DispatcherScheduler.Current)
            .Subscribe(fun s -> this.ThrottledResults.Items.Add(s)|>ignore)

