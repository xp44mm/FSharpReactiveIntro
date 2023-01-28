namespace RxInActionFSharp

open System
open System.Reactive
open System.Reactive.Disposables

open FSharp.Control.Reactive
open System.Reactive.Linq
open System.Collections.Generic

type ObservableConnection(chatConnection:IChatConnection) =
    inherit ObservableBase<string>()

    override _.SubscribeCore(observer:IObserver<string>) =
        let received = Action<string>   (observer.OnNext)
        let closed   = Action           (observer.OnCompleted)
        let error    = Action<Exception>(observer.OnError)

        chatConnection.Received.AddHandler(received)
        chatConnection.Closed.AddHandler(closed)
        chatConnection.Error.AddHandler(error)

        Disposable.Create(fun () ->
            chatConnection.Received.RemoveHandler(received)
            chatConnection.Closed.RemoveHandler(closed)
            chatConnection.Error.RemoveHandler(error)
            chatConnection.Disconnect()
        )

module ObservableConnection =
    let test() =
        let connection = ChatConnection()
        let observableConnection = ObservableConnection (connection :> IChatConnection)
        let subscription =
            observableConnection.Subscribe(ConsoleObserver "receiver")

        connection.NotifyRecieved("Hello")
        connection.NotifyClosed()
        subscription.Dispose()

    let testConcat() =
        let connection = ChatConnection()
        let liveMessages:IObservable<string> =
            ObservableConnection (connection :> IChatConnection)
        let loadedMessages:IEnumerable<string> = seq {"loaded1";"loaded2"}

        loadedMessages
            .ToObservable()
            .Concat(liveMessages)
            .Subscribe(ConsoleObserver "merged")
        |> ignore

        connection.NotifyRecieved("live message1")
        connection.NotifyRecieved("live message2")

    let testStartWith() =
        let connection = ChatConnection()
        let liveMessages:IObservable<string> =
            ObservableConnection (connection :> IChatConnection)
        let loadedMessages:IEnumerable<string> = seq {"loaded1";"loaded2"}

        liveMessages
            .StartWith(loadedMessages)
            .Subscribe(ConsoleObserver "loaded first")
        |> ignore

        connection.NotifyRecieved("live message1")
        connection.NotifyRecieved("live message2")
