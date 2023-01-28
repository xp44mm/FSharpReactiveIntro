namespace RxInActionFSharp

open System
open System.Reactive

open System.Reactive.Disposables
open System.Reactive.Observable.Aliases

open FSharp.Control.Reactive
open System.Reactive.Linq

type NetworkFoundEventHandler = delegate of ssid:string -> unit

type WifiScanner() =
    let networkFound = DelegateEvent<NetworkFoundEventHandler>()
    member _.NetworkFound = networkFound.Publish
    member _.RaiseFound(ssid:string) = networkFound.Trigger[|ssid|]

    member wifiScanner.toObservable() =
        Observable.FromEvent<NetworkFoundEventHandler, string>(
            (fun h -> wifiScanner.NetworkFound.AddHandler h),
            (fun h -> wifiScanner.NetworkFound.RemoveHandler h))
module WifiScanner =
    let test() =
        let wifiScanner = WifiScanner()
        let networks = wifiScanner.toObservable()
        networks.Subscribe(ConsoleObserver "ExtendedNetworkFound") 
        |> ignore
        wifiScanner.RaiseFound("hello")