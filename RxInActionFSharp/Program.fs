module RxInActionFSharp.Program

open System
open System.Reactive

open System.Reactive.Disposables
open System.Reactive.Observable.Aliases

open FSharp.Control.Reactive
open System.Reactive.Linq
open System.Threading.Tasks

let pe() =
    Console.WriteLine("Press any key to continue...")
    Console.ReadKey()|> ignore

let rl() = Console.ReadLine() |> ignore

let [<EntryPoint>] main _ =
    CountdownEvent.EventWithCancel ()
    |> ignore
    pe()
    0
