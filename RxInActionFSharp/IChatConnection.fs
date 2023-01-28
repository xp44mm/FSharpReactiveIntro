namespace RxInActionFSharp

open System
open System.Reactive
open System.Reactive.Disposables

open FSharp.Control.Reactive

type IChatConnection =
    abstract Received:IDelegateEvent<Action<string>>
    abstract Closed:IDelegateEvent<Action>
    abstract Error:IDelegateEvent<Action<Exception>>
    abstract Disconnect:unit -> unit

type ChatConnection() =
    let received = DelegateEvent<Action<string>>()
    let closed = DelegateEvent<Action>()
    let error = DelegateEvent<Action<Exception>>()

    interface IChatConnection with
        override _.Received = received.Publish
        override _.Closed = closed.Publish
        override _.Error = error.Publish

        override _.Disconnect() =
            Console.WriteLine("Disconnect")

    member _.NotifyRecieved(msg:string) = received.Trigger [|msg|]

    member _.NotifyClosed() = closed.Trigger [||]

    member _.NotifyError() = error.Trigger[|OutOfMemoryException()|]