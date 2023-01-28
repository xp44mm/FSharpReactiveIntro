module RxInActionFSharp.Synchronize

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
open System.Reactive.Concurrency

type Messenger () =
    let messageReceived:Event<_> = Event<string>()

    [<CLIEvent>]
    member this.MessageReceived = messageReceived.Publish

    member this.Notify(msg:string) = 
        messageReceived.Trigger(msg)

let test () =
    let messenger = new Messenger()
    let messages = messenger.MessageReceived :> IObservable<string>
    messages
        .Synchronize()
        .Subscribe(fun msg ->
            Console.WriteLine("Message {0} arrived", msg)
            Thread.Sleep(1000)
            Console.WriteLine("Message {0} exit", msg)
        ) |> ignore

    for i in [0 .. 2] do
        let msg = $"msg {i}"
        ThreadPool.QueueUserWorkItem(fun _ ->
            messenger.Notify(msg)
        ) |> ignore

let test2 () =
    let messenger = new Messenger()
    let messages = messenger.MessageReceived :> IObservable<string>

    let gate = new obj()

    messages
        .Synchronize(gate)
        .Subscribe(ConsoleObserver "gate")
        |> ignore

    for i in [0 .. 2] do
        let msg = $"msg {i}"
        ThreadPool.QueueUserWorkItem(fun _ ->
            messenger.Notify(msg)
        ) |> ignore

