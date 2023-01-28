namespace ReactiveWPFDrawingApp

open FsXaml

type SearchTermWindowXaml = XAML<"SearchTermWindow.xaml">

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

type SearchTermWindow() as this =
    inherit SearchTermWindowXaml()
    let disp =
        (this.SearchTerm.TextChanged :> IObservable<_>)
            .Map(fun x -> this.SearchTerm.Text)
            .Throttle(TimeSpan.FromMilliseconds(400))
            .DistinctUntilChanged()
            .ObserveOn(System.Threading.SynchronizationContext.Current)
            .Subscribe(fun s -> this.Terms.Items.Add(s)|>ignore)

