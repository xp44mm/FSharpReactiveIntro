namespace ReactiveWPFDrawingApp

open System
open System.Windows.Shapes
open System.Windows.Media

open System.Reactive
open System.Reactive.Linq
open System.Reactive.Observable.Aliases

open MahApps.Metro.Controls

type FirstMetroWindowXaml = FsXaml.XAML<"FirstMetroWindow.xaml">

type FirstMetroWindow () =
    inherit FirstMetroWindowXaml ()

