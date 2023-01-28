namespace ReactiveWPFDrawingApp

type ReactiveDrawXaml = FsXaml.XAML<"ReactiveDraw.xaml">

open System

open System.Reactive
open System.Reactive.Linq
open System.Reactive.Observable.Aliases

open System.Windows.Shapes
open System.Windows.Media

type ReactiveDraw() as this =
    inherit ReactiveDrawXaml()

    let mouseDowns = 
        (this.MouseDown :> IObservable<_>)
            .Synchronize()
    let mouseUp =
        (this.MouseUp :> IObservable<_>)
            .Synchronize()            
    let movements = 
        (this.MouseMove :> IObservable<_>)
            .Synchronize()

    let getLine() =
        let ls = this.canvas.Children
        ls.[ls.Count-1] :?> Polyline

    let _subscription = 
        movements
            .SkipUntil(
                mouseDowns.Do(fun _ ->
                    let line = Polyline(Stroke = Brushes.Black, StrokeThickness = 3)
                    this.canvas.Children.Add(line) |> ignore
                ))
            .TakeUntil(mouseUp)
            .Map(fun m -> m.GetPosition(this))
            .Repeat()
            .Subscribe(fun pos -> getLine().Points.Add(pos))
