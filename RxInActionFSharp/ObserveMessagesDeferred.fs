module RxInActionFSharp.ObserveMessagesDeferred

open System
open System.Reactive

open System.Reactive.Disposables
open System.Reactive.Observable.Aliases

open FSharp.Control.Reactive
open System.Reactive.Linq

let ObserveMessagesDeferred(): IObservable<string> =
    Observable.Defer(fun () ->
        let connection = ChatConnection() 
        ObservableConnection (connection :> IChatConnection)
        :> IObservable<string>
        )

let test() =
    let messages = ObserveMessagesDeferred();
    let _ = messages.Subscribe(ConsoleObserver "aaa")
    let _ = messages.Subscribe(ConsoleObserver "bbb")
    ()