namespace RxInActionFSharp

open System
open System.Reactive

open System.Reactive.Disposables
open System.Reactive.Observable.Aliases

open FSharp.Control.Reactive
open System.Reactive.Linq

type UnitWifiScanner () =
    let connected = DelegateEvent<Action>()
    member _.Connected = connected.Publish
    member _.RaiseConnected() = connected.Trigger[||]

    member this.toObservable() =
        Observable.FromEvent(
            (fun h -> this.Connected.AddHandler h),
            (fun h -> this.Connected.RemoveHandler h))

module UnitWifiScanner =
    let test() =
        let wifiScanner = UnitWifiScanner()
        let networks = wifiScanner.toObservable()

        networks.Subscribe(ConsoleObserver "Connected") 
        |> ignore
        wifiScanner.RaiseConnected()