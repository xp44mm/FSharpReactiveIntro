namespace RxInActionFSharp

open System
open System.Reactive

open System.Reactive.Disposables
open System.Reactive.Observable.Aliases

open FSharp.Control.Reactive
open System.Reactive.Linq

type ExtendedNetworkFoundEventHandler = delegate of ssid:string * strength:int -> unit

type ExtendedWifiScanner() =
    let extendedNetwork = DelegateEvent<ExtendedNetworkFoundEventHandler>()
    member _.ExtendedNetworkFound = extendedNetwork.Publish
    member _.RaiseFound(ssid:string, strength:int) = extendedNetwork.Trigger[|ssid;strength|]

    member wifiScanner.toObservable() =
        let conversion (rxHandler:Action<string*int>) =
            ExtendedNetworkFoundEventHandler(fun ssid strength -> rxHandler.Invoke(ssid,strength))

        Observable.FromEvent<ExtendedNetworkFoundEventHandler, string*int>(
            Func<_,_> conversion,
            (fun h -> wifiScanner.ExtendedNetworkFound.AddHandler h),
            (fun h -> wifiScanner.ExtendedNetworkFound.RemoveHandler h))

module ExtendedWifiScanner =
    let test() =
        let wifiScanner = ExtendedWifiScanner()
        let networks = wifiScanner.toObservable()
        networks.Subscribe(ConsoleObserver "ExtendedNetworkFound") 
        |> ignore
        wifiScanner.RaiseFound("hello",100)