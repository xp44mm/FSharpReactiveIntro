﻿module RxInActionFSharp.ReadLine

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

let test () =
    let lines:IObservable<string> =
        let path = Path.Combine(__SOURCE_DIRECTORY__, "ReadLine.fs")
        Observable.Generate(
            File.OpenText(path),
            (fun s -> not s.EndOfStream),
            id,
            (fun s -> s.ReadLine())
            )
    lines.Subscribe(ConsoleObserver "lines")
    |> ignore

