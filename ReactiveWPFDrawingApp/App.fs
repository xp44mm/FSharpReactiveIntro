module ReactiveWPFDrawingApp.App

open System
open FsXaml

type App = XAML<"App.xaml">

[<STAThread; EntryPoint>]
let main _ =
    System.Threading.SynchronizationContext.SetSynchronizationContext(
        System.Windows.Threading.DispatcherSynchronizationContext())

    let app = App()
    let startup = FirstMetroWindowXaml ()
    app.Run(startup) // Returns application's exit code.