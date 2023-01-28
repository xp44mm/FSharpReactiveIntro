module RxInActionFSharp.PrimitiveObservables

open System
open System.IO

open System.Reactive
open System.Collections.Generic

open System.Reactive.Disposables
open System.Reactive.Observable.Aliases

open FSharp.Control.Reactive
open System.Reactive.Linq
open System.Text
open FSharp.Idioms.PointFree
open FSharp.Literals.Literal

let testReturn () =
    Observable
        .Return("Hello World")
        .Subscribe(ConsoleObserver "Return")
    |> ignore

let testNever () =
    Observable
        .Never<string>()
        .Subscribe(ConsoleObserver "Never")
    |> ignore

let testThrow () =
    Observable
        .Throw<ApplicationException>(
             new ApplicationException("something bad happened")
             )
        .Subscribe(ConsoleObserver "Throw")
    |> ignore

let testEmpty () =
    Observable
        .Empty<string>()
        .Subscribe(ConsoleObserver "Empty")
    |> ignore


